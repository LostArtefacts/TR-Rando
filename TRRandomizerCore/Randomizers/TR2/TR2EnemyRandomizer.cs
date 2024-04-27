using System.Diagnostics;
using TRFDControl;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2EnemyRandomizer : BaseTR2Randomizer
{
    private Dictionary<TR2Type, List<string>> _gameEnemyTracker;
    private List<TR2Type> _excludedEnemies;
    private ISet<TR2Type> _resultantEnemies;

    internal int MaxPackingAttempts { get; set; }
    internal TR2TextureMonitorBroker TextureMonitor { get; set; }
    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    public TR2EnemyRandomizer()
    {
        MaxPackingAttempts = 5;
    }

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);
        if (Settings.CrossLevelEnemies)
        {
            RandomizeEnemiesCrossLevel();
        }
        else
        {
            RandomizeExistingEnemies();
        }
    }

    private void RandomizeExistingEnemies()
    {
        _excludedEnemies = new List<TR2Type>();
        _resultantEnemies = new HashSet<TR2Type>();

        foreach (TR2ScriptedLevel lvl in Levels)
        {
            //Read the level into a combined data/script level object
            LoadLevelInstance(lvl);

            //Apply the modifications
            RandomizeEnemiesNatively(_levelInstance);

            //Write back the level file
            SaveLevelInstance();

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void RandomizeEnemiesCrossLevel()
    {
        MaxPackingAttempts = Math.Max(1, MaxPackingAttempts);

        SetMessage("Randomizing enemies - loading levels");

        List<EnemyProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new EnemyProcessor(this));
        }

        List<TR2CombinedLevel> levels = new(Levels.Count);
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        // Sort the levels so each thread has a fairly equal weight in terms of import cost/time
        levels.Sort(new TR2LevelTextureWeightComparer());

        int processorIndex = 0;
        foreach (TR2CombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        // Track enemies whose counts across the game are restricted
        _gameEnemyTracker = TR2EnemyUtilities.PrepareEnemyGameTracker(Settings.DocileChickens, Settings.RandoEnemyDifficulty);
        
        // #272 Selective enemy pool - convert the shorts in the settings to actual entity types
        _excludedEnemies = Settings.UseEnemyExclusions ? 
            Settings.ExcludedEnemies.Select(s => (TR2Type)s).ToList() : 
            new List<TR2Type>();
        _resultantEnemies = new HashSet<TR2Type>();

        SetMessage("Randomizing enemies - importing models");
        foreach (EnemyProcessor processor in processors)
        {
            processor.Start();
        }

        foreach (EnemyProcessor processor in processors)
        {
            processor.Join();
        }

        if (!SaveMonitor.IsCancelled && _processingException == null)
        {
            SetMessage("Randomizing enemies - saving levels");
            foreach (EnemyProcessor processor in processors)
            {
                processor.ApplyRandomization();
            }
        }

        _processingException?.Throw();

        // If any exclusions failed to be avoided, send a message
        if (Settings.ShowExclusionWarnings)
        {
            VerifyExclusionStatus();
        }
    }

    private void VerifyExclusionStatus()
    {
        List<TR2Type> failedExclusions = _resultantEnemies.ToList().FindAll(_excludedEnemies.Contains);
        if (failedExclusions.Count > 0)
        {
            // A little formatting
            List<string> failureNames = new();
            foreach (TR2Type entity in failedExclusions)
            {
                failureNames.Add(Settings.ExcludableEnemies[(short)entity]);
            }
            failureNames.Sort();
            SetWarning(string.Format("The following enemies could not be excluded entirely from the randomization pool.{0}{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, failureNames)));
        }
    }

    private EnemyTransportCollection SelectCrossLevelEnemies(TR2CombinedLevel level, int reduceEnemyCountBy = 0)
    {
        // For the assault course, nothing will be imported for the time being
        if (level.IsAssault)
        {
            return null;
        }

        // Get the list of enemy types currently in the level
        List<TR2Type> oldEntities = TR2TypeUtilities.GetEnemyTypeDictionary()[level.Name];

        // Work out how many we can support
        int enemyCount = oldEntities.Count - reduceEnemyCountBy + TR2EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
        List<TR2Type> newEntities = new(enemyCount);

        List<TR2Type> chickenGuisers = TR2EnemyUtilities.GetEnemyGuisers(TR2Type.BirdMonster);
        TR2Type chickenGuiser = TR2Type.BirdMonster;

        RandoDifficulty difficulty = GetImpliedDifficulty();

        // #148 For HSH, we lock the enemies that are required for the kill counter to work outside
        // the gate, which means the game still has the correct target kill count, while allowing
        // us to randomize the ones inside the gate (except the final shotgun goon).
        // If however, we are on the final packing attempt, we will just change the stick goon
        // alias and add docile bird monsters (if selected) as this is known to be supported.
        if (level.Is(TR2LevelNames.HOME) && reduceEnemyCountBy > 0)
        {
            TR2Type newGoon = TR2Type.StickWieldingGoon1BlackJacket;
            List<TR2Type> goonies = TR2TypeUtilities.GetFamily(newGoon);
            do
            {
                newGoon = goonies[_generator.Next(0, goonies.Count)];
            }
            while (newGoon == TR2Type.StickWieldingGoon1BlackJacket);

            newEntities.AddRange(oldEntities);
            newEntities.Remove(TR2Type.StickWieldingGoon1);
            newEntities.Add(newGoon);

            if (Settings.DocileChickens)
            {
                newEntities.Remove(TR2Type.MaskedGoon1);
                newEntities.Add(TR2Type.BirdMonster);
                chickenGuiser = TR2Type.MaskedGoon1;
            }
        }
        else
        {
            // Do we need at least one water creature?
            bool waterEnemyRequired = TR2EnemyUtilities.IsWaterEnemyRequired(level);
            // Do we need at least one enemy that can drop?
            bool droppableEnemyRequired = TR2EnemyUtilities.IsDroppableEnemyRequired(level);

            // Let's try to populate the list. Start by adding one water enemy and one droppable
            // enemy if they are needed. If we want to exclude, try to select based on user priority.
            if (waterEnemyRequired)
            {
                List<TR2Type> waterEnemies = TR2TypeUtilities.KillableWaterCreatures();
                newEntities.Add(SelectRequiredEnemy(waterEnemies, level, difficulty));
            }

            if (droppableEnemyRequired)
            {
                List<TR2Type> droppableEnemies = TR2TypeUtilities.GetCrossLevelDroppableEnemies(!Settings.ProtectMonks, Settings.UnconditionalChickens);
                newEntities.Add(SelectRequiredEnemy(droppableEnemies, level, difficulty));
            }

            // Are there any other types we need to retain?
            foreach (TR2Type entity in TR2EnemyUtilities.GetRequiredEnemies(level.Name))
            {
                if (!newEntities.Contains(entity))
                {
                    newEntities.Add(entity);
                }
            }

            // Some secrets may have locked enemies in place - we must retain those types
            foreach (int itemIndex in ItemFactory.GetLockedItems(level.Name))
            {
                TR2Entity item = level.Data.Entities[itemIndex];
                if (TR2TypeUtilities.IsEnemyType(item.TypeID))
                {
                    List<TR2Type> family = TR2TypeUtilities.GetFamily(TR2TypeUtilities.GetAliasForLevel(level.Name, item.TypeID));
                    if (!newEntities.Any(family.Contains))
                    {
                        newEntities.Add(family[_generator.Next(0, family.Count)]);
                    }
                }
            }

            // Get all other candidate supported enemies
            List<TR2Type> allEnemies = TR2TypeUtilities.GetCandidateCrossLevelEnemies()
                .FindAll(e => TR2EnemyUtilities.IsEnemySupported(level.Name, e, difficulty, Settings.ProtectMonks));
            if (Settings.OneEnemyMode || Settings.IncludedEnemies.Count < newEntities.Capacity || Settings.DragonSpawnType == DragonSpawnType.Minimum)
            {
                // Marco isn't excludable in his own right because supporting a dragon-only game is impossible.
                // If we want a minimum dragon game, he is excluded here as well (for Lair he is required, so already added above).
                allEnemies.Remove(TR2Type.MarcoBartoli);
            }

            // Remove all exclusions from the pool, and adjust the target capacity
            allEnemies.RemoveAll(e => _excludedEnemies.Contains(e));

            IEnumerable<TR2Type> ex = allEnemies.Where(e => !newEntities.Any(TR2TypeUtilities.GetFamily(e).Contains));
            List<TR2Type> unalisedEntities = TR2TypeUtilities.RemoveAliases(ex);
            while (unalisedEntities.Count < newEntities.Capacity - newEntities.Count)
            {
                --newEntities.Capacity;
            }
            
            // Fill the list from the remaining candidates. Keep track of ones tested to avoid
            // looping infinitely if it's not possible to fill to capacity
            ISet<TR2Type> testedEntities = new HashSet<TR2Type>();
            while (newEntities.Count < newEntities.Capacity && testedEntities.Count < allEnemies.Count)
            {
                TR2Type entity;
                // Try to enforce Marco's appearance, but only if this isn't the final packing attempt
                if (Settings.DragonSpawnType == DragonSpawnType.Maximum
                    && !newEntities.Contains(TR2Type.MarcoBartoli)
                    && TR2EnemyUtilities.IsEnemySupported(level.Name, TR2Type.MarcoBartoli, difficulty, Settings.ProtectMonks)
                    && reduceEnemyCountBy == 0)
                {
                    entity = TR2Type.MarcoBartoli;
                }
                else
                {
                    entity = allEnemies[_generator.Next(0, allEnemies.Count)];
                }

                testedEntities.Add(entity);

                int adjustmentCount = TR2EnemyUtilities.GetTargetEnemyAdjustmentCount(level.Name, entity);
                if (!Settings.OneEnemyMode && adjustmentCount != 0)
                {
                    while (newEntities.Count > 0 && newEntities.Count >= newEntities.Capacity + adjustmentCount)
                    {
                        newEntities.RemoveAt(newEntities.Count - 1);
                    }
                    newEntities.Capacity += adjustmentCount;
                }

                // Check if the use of this enemy triggers an overwrite of the pool, for example
                // the dragon in HSH. Null means nothing special has been defined.
                List<List<TR2Type>> restrictedCombinations = TR2EnemyUtilities.GetPermittedCombinations(level.Name, entity, difficulty);
                if (restrictedCombinations != null)
                {
                    do
                    {
                        // Pick a combination, ensuring we honour docile bird monsters if present,
                        // and try to select a group that doesn't contain an excluded enemy.
                        newEntities.Clear();
                        newEntities.AddRange(restrictedCombinations[_generator.Next(0, restrictedCombinations.Count)]);
                    }
                    while (Settings.DocileChickens && newEntities.Contains(TR2Type.BirdMonster) && chickenGuisers.All(g => newEntities.Contains(g))
                       || (newEntities.Any(_excludedEnemies.Contains) && restrictedCombinations.Any(c => !c.Any(_excludedEnemies.Contains))));
                    break;
                }

                // If it's the chicken in HSH with default behaviour, we don't want it ending the level
                if (Settings.DefaultChickens && entity == TR2Type.BirdMonster && level.Is(TR2LevelNames.HOME) && allEnemies.Except(newEntities).Count() > 1)
                {
                    continue;
                }

                // If this is a tracked enemy throughout the game, we only allow it if the number
                // of unique levels is within the limit. Bear in mind we are collecting more than
                // one group of enemies per level.
                if (_gameEnemyTracker.ContainsKey(entity) && !_gameEnemyTracker[entity].Contains(level.Name))
                {
                    if (_gameEnemyTracker[entity].Count < _gameEnemyTracker[entity].Capacity)
                    {
                        // The entity is allowed, so store the fact that this level will have it
                        _gameEnemyTracker[entity].Add(level.Name);
                    }
                    else
                    {
                        // Otherwise, pick something else. If we tried to previously exclude this
                        // enemy and couldn't, it will slip through the net and so the appearances
                        // will increase.
                        if (allEnemies.Except(newEntities).Count() > 1)
                        {
                            continue;
                        }
                    }
                }

                // GetEntityFamily returns all aliases for the likes of the tigers, but if an entity
                // doesn't have any, the returned list just contains the entity itself. This means
                // we can avoid duplicating standard enemies as well as avoiding alias-clashing.
                List<TR2Type> family = TR2TypeUtilities.GetFamily(entity);
                if (!newEntities.Any(e1 => family.Any(e2 => e1 == e2)))
                {
                    // #144 We can include docile chickens provided we aren't including everything
                    // that can be disguised as a chicken.
                    if (Settings.DocileChickens)
                    {
                        bool guisersAvailable = !chickenGuisers.All(g => newEntities.Contains(g));
                        // If the selected entity is the chicken, it can be added provided there are
                        // available guisers.
                        if (!guisersAvailable && entity == TR2Type.BirdMonster)
                        {
                            continue;
                        }

                        // If the selected entity is a potential guiser, it can only be added if it's not
                        // the last available guiser. Otherwise, it will become the guiser.
                        if (chickenGuisers.Contains(entity) && newEntities.Contains(TR2Type.BirdMonster))
                        {
                            if (newEntities.FindAll(e => chickenGuisers.Contains(e)).Count == chickenGuisers.Count - 1)
                            {
                                continue;
                            }
                        }
                    }

                    newEntities.Add(entity);
                }
            }
        }

        // If everything we are including is restriced by room, we need to provide at least one other enemy type
        Dictionary<TR2Type, List<int>> restrictedRoomEnemies = TR2EnemyUtilities.GetRestrictedEnemyRooms(level.Name, difficulty);
        if (restrictedRoomEnemies != null && newEntities.All(e => restrictedRoomEnemies.ContainsKey(e)))
        {
            List<TR2Type> pool = TR2TypeUtilities.GetCrossLevelDroppableEnemies(!Settings.ProtectMonks, Settings.UnconditionalChickens);
            do
            {
                TR2Type fallbackEnemy;
                do
                {
                    fallbackEnemy = pool[_generator.Next(0, pool.Count)];
                }
                while ((_excludedEnemies.Contains(fallbackEnemy) && pool.Any(e => !_excludedEnemies.Contains(e))) 
                || newEntities.Contains(fallbackEnemy) 
                || !TR2EnemyUtilities.IsEnemySupported(level.Name, fallbackEnemy, difficulty, Settings.ProtectMonks));
                newEntities.Add(fallbackEnemy);
            }
            while (newEntities.All(e => restrictedRoomEnemies.ContainsKey(e)));
        }
        else
        {
            // #345 Barkhang/Opera with only Winstons causes freezing issues
            List<TR2Type> friends = TR2EnemyUtilities.GetFriendlyEnemies();
            if ((level.Is(TR2LevelNames.OPERA) || level.Is(TR2LevelNames.MONASTERY)) && newEntities.All(friends.Contains))
            {
                // Add an additional "safe" enemy - so pick from the droppable range, monks and chickens excluded
                List<TR2Type> droppableEnemies = TR2TypeUtilities.GetCrossLevelDroppableEnemies(false, false);
                newEntities.Add(SelectRequiredEnemy(droppableEnemies, level, difficulty));
            }
        }

        // #144 Decide at this point who will be guising unless it has already been decided above (e.g. HSH)          
        if (Settings.DocileChickens && newEntities.Contains(TR2Type.BirdMonster) && chickenGuiser == TR2Type.BirdMonster)
        {
            int guiserIndex = chickenGuisers.FindIndex(g => !newEntities.Contains(g));
            if (guiserIndex != -1)
            {
                chickenGuiser = chickenGuisers[guiserIndex];
            }
        }

        return new EnemyTransportCollection
        {
            EntitiesToImport = newEntities,
            EntitiesToRemove = oldEntities,
            BirdMonsterGuiser = chickenGuiser
        };
    }

    private TR2Type SelectRequiredEnemy(List<TR2Type> pool, TR2CombinedLevel level, RandoDifficulty difficulty)
    {
        pool.RemoveAll(e => !TR2EnemyUtilities.IsEnemySupported(level.Name, e, difficulty, Settings.ProtectMonks));

        TR2Type entity;
        if (pool.All(_excludedEnemies.Contains))
        {
            // Select the last excluded enemy (lowest priority)
            entity = _excludedEnemies.Last(e => pool.Contains(e));
        }
        else
        {
            do
            {
                entity = pool[_generator.Next(0, pool.Count)];
            }
            while (_excludedEnemies.Contains(entity));
        }
        
        return entity;
    }

    private RandoDifficulty GetImpliedDifficulty()
    {
        if (_excludedEnemies.Count > 0 && Settings.RandoEnemyDifficulty == RandoDifficulty.Default)
        {
            // If every enemy in the pool has room restrictions for any level, we have to imply NoRestrictions difficulty mode
            List<TR2Type> includedEnemies = Settings.ExcludableEnemies.Keys.Except(Settings.ExcludedEnemies).Select(s => (TR2Type)s).ToList();
            foreach (TR2ScriptedLevel level in Levels)
            {
                IEnumerable<TR2Type> restrictedRoomEnemies = TR2EnemyUtilities.GetRestrictedEnemyRooms(level.LevelFileBaseName.ToUpper(), RandoDifficulty.Default).Keys;
                if (includedEnemies.All(e => restrictedRoomEnemies.Contains(e) || _gameEnemyTracker.ContainsKey(e)))
                {
                    return RandoDifficulty.NoRestrictions;
                }
            }
        }
        return Settings.RandoEnemyDifficulty;
    }

    private void RandomizeEnemiesNatively(TR2CombinedLevel level)
    {
        // For the assault course, nothing will be changed for the time being
        if (level.IsAssault)
        {
            return;
        }

        List<TR2Type> availableEnemyTypes = TR2TypeUtilities.GetEnemyTypeDictionary()[level.Name];
        List<TR2Type> droppableEnemies = TR2TypeUtilities.DroppableEnemyTypes()[level.Name];
        List<TR2Type> waterEnemies = TR2TypeUtilities.FilterWaterEnemies(availableEnemyTypes);

        if (Settings.DocileChickens && level.Is(TR2LevelNames.CHICKEN))
        {
            DisguiseEntity(level, TR2Type.MaskedGoon1, TR2Type.BirdMonster);
        }

        RandomizeEnemies(level, new EnemyRandomizationCollection
        {
            Available = availableEnemyTypes,
            Droppable = droppableEnemies,
            Water = waterEnemies,
            All = new List<TR2Type>(availableEnemyTypes),
            BirdMonsterGuiser = TR2Type.MaskedGoon1 // If randomizing natively, this will only apply to Ice Palace
        });
    }

    private static void DisguiseEntity(TR2CombinedLevel level, TR2Type guiser, TR2Type targetEntity)
    {
        int existingIndex = level.Data.Models.FindIndex(m => m.ID == (short)guiser);
        if (existingIndex != -1)
        {
            level.Data.Models.RemoveAt(existingIndex);
        }

        TRModel disguiseAsModel = level.Data.Models.Find(m => m.ID == (short)targetEntity);
        if (targetEntity == TR2Type.BirdMonster && level.Is(TR2LevelNames.CHICKEN))
        {
            // We have to keep the original model for the boss, so in
            // this instance we just clone the model for the guiser
            TRModel guiserModel = disguiseAsModel.Clone();
            guiserModel.ID = (uint)guiser;
            level.Data.Models.Add(guiserModel);
        }
        else
        {
            disguiseAsModel.ID = (uint)guiser;
        }
    }

    private void RandomizeEnemies(TR2CombinedLevel level, EnemyRandomizationCollection enemies)
    {
        bool shotgunGoonSeen = level.Is(TR2LevelNames.HOME); // 1 ShotgunGoon in HSH only
        bool dragonSeen = level.Is(TR2LevelNames.LAIR); // 1 Marco in DL only

        // Get a list of current enemy entities
        List<TR2Entity> enemyEntities = level.GetEnemyEntities();

        RandoDifficulty difficulty = GetImpliedDifficulty();

        // #148 If it's HSH and we have been able to import cross-level, we will add 15
        // dogs outside the gate to ensure the kill counter works. Dogs, Goon1 and
        // StickGoons will have been excluded from the cross-level pool for simplicity
        // Their textures will have been removed but they won't spawn anyway as we aren't
        // defining triggers - the game only needs them to be present in the entity list.
        if (level.Is(TR2LevelNames.HOME) && !enemies.Available.Contains(TR2Type.Doberman))
        {
            for (int i = 0; i < 15; i++)
            {
                level.Data.Entities.Add(new()
                {
                    TypeID = TR2Type.Doberman,
                    Room = 85,
                    X = 61919,
                    Y = 2560,
                    Z = 74222,
                    Angle = 16384,
                    Flags = 0,
                    Intensity1 = -1,
                    Intensity2 = -1
                });
            }
        }

        // First iterate through any enemies that are restricted by room
        Dictionary<TR2Type, List<int>> enemyRooms = TR2EnemyUtilities.GetRestrictedEnemyRooms(level.Name, difficulty);
        if (enemyRooms != null)
        {
            foreach (TR2Type entity in enemyRooms.Keys)
            {
                if (!enemies.Available.Contains(entity))
                {
                    continue;
                }

                List<int> rooms = enemyRooms[entity];
                int maxEntityCount = TR2EnemyUtilities.GetRestrictedEnemyLevelCount(entity, difficulty);
                if (maxEntityCount == -1)
                {
                    // We are allowed any number, but this can't be more than the number of unique rooms,
                    // so we will assume 1 per room as these restricted enemies are likely to be tanky.
                    maxEntityCount = rooms.Count;
                }
                else
                {
                    maxEntityCount = Math.Min(maxEntityCount, rooms.Count);
                }

                // Pick an actual count
                int enemyCount = _generator.Next(1, maxEntityCount + 1);
                for (int i = 0; i < enemyCount; i++)
                {
                    // Find an entity in one of the rooms that the new enemy is restricted to
                    TR2Entity targetEntity = null;
                    do
                    {
                        int room = enemyRooms[entity][_generator.Next(0, enemyRooms[entity].Count)];
                        targetEntity = enemyEntities.Find(e => e.Room == room);
                    }
                    while (targetEntity == null);

                    // If the room has water but this enemy isn't a water enemy, we will assume that environment
                    // modifications will handle assignment of the enemy to entities.
                    if (!TR2TypeUtilities.IsWaterCreature(entity) && level.Data.Rooms[targetEntity.Room].ContainsWater)
                    {
                        continue;
                    }

                    targetEntity.TypeID = TR2TypeUtilities.TranslateAlias(entity);

                    // #146 Ensure OneShot triggers are set for this enemy if needed
                    TR2EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);

                    // Remove the target entity so it doesn't get replaced
                    enemyEntities.Remove(targetEntity);
                }

                // Remove this entity type from the available rando pool
                enemies.Available.Remove(entity);
            }
        }

        foreach (TR2Entity currentEntity in enemyEntities)
        {
            TR2Type currentEntityType = currentEntity.TypeID;
            TR2Type newEntityType = currentEntityType;
            int enemyIndex = level.Data.Entities.IndexOf(currentEntity);

            // If it's an existing enemy that has to remain in the same spot, skip it
            if (TR2EnemyUtilities.IsEnemyRequired(level.Name, currentEntityType)
                || ItemFactory.IsItemLocked(level.Name, enemyIndex))
            {
                continue;
            }

            // Generate a new type, ensuring to test for item drops
            newEntityType = enemies.Available[_generator.Next(0, enemies.Available.Count)];
            bool hasPickupItem = level.Data.Entities
                .Any(item => TR2EnemyUtilities.HasDropItem(currentEntity, item));

            if (hasPickupItem
                && !TR2TypeUtilities.CanDropPickups(newEntityType, !Settings.ProtectMonks, Settings.UnconditionalChickens))
            {
                newEntityType = enemies.Droppable[_generator.Next(0, enemies.Droppable.Count)];
            }

            short roomIndex = currentEntity.Room;
            TR2Room room = level.Data.Rooms[roomIndex];

            if (level.Is(TR2LevelNames.DA) && roomIndex == 77)
            {
                // Make sure the end level trigger isn't blocked by an unkillable enemy
                while (TR2TypeUtilities.IsHazardCreature(newEntityType) || (Settings.ProtectMonks && TR2TypeUtilities.IsMonk(newEntityType)))
                {
                    newEntityType = enemies.Available[_generator.Next(0, enemies.Available.Count)];
                }
            }

            if (TR2TypeUtilities.IsWaterCreature(currentEntityType) && !TR2TypeUtilities.IsWaterCreature(newEntityType))
            {
                // Check alternate rooms too - e.g. rooms 74/48 in 40 Fathoms
                short roomDrainIndex = -1;
                if (room.ContainsWater)
                {
                    roomDrainIndex = roomIndex;
                }
                else if (room.AlternateRoom != -1 && level.Data.Rooms[room.AlternateRoom].ContainsWater)
                {
                    roomDrainIndex = room.AlternateRoom;
                }
                
                if (roomDrainIndex != -1 && !level.PerformDraining(roomDrainIndex))
                {
                    // Draining cannot be performed so make the entity a water creature.
                    // The list of provided water creatures will either be those native
                    // to this level, or if randomizing cross-level, a pre-check will
                    // have already been performed on draining so if it's not possible,
                    // at least one water creature will be available.
                    newEntityType = enemies.Water[_generator.Next(0, enemies.Water.Count)];
                }
            }

            // Ensure that if we have to pick a different enemy at this point that we still
            // honour any pickups in the same spot.
            List<TR2Type> enemyPool = hasPickupItem ? enemies.Droppable : enemies.Available;

            if (newEntityType == TR2Type.ShotgunGoon && shotgunGoonSeen) // HSH only
            {
                while (newEntityType == TR2Type.ShotgunGoon)
                {
                    newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                }
            }

            if (newEntityType == TR2Type.MarcoBartoli && dragonSeen) // DL only, other levels use quasi-zoning for the dragon
            {
                while (newEntityType == TR2Type.MarcoBartoli)
                {
                    newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                }
            }

            // #278 Flamethrowers in room 29 after pulling the lever are too difficult, but if difficulty is set to unrestricted
            // and they do end up here, environment mods will change their positions.
            int totalRestrictionCount = TR2EnemyUtilities.GetRestrictedEnemyTotalTypeCount(difficulty);
            if (level.Is(TR2LevelNames.FLOATER) && difficulty == RandoDifficulty.Default && (enemyIndex == 34 || enemyIndex == 35) && enemyPool.Count > totalRestrictionCount)
            {
                while (newEntityType == TR2Type.FlamethrowerGoon)
                {
                    newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                }
            }

            // If we are restricting count per level for this enemy and have reached that count, pick
            // something else. This applies when we are restricting by in-level count, but not by room
            // (e.g. Winston).
            int maxEntityCount = TR2EnemyUtilities.GetRestrictedEnemyLevelCount(newEntityType, difficulty);
            if (maxEntityCount != -1)
            {
                if (level.Data.Entities.FindAll(e => e.TypeID == newEntityType).Count >= maxEntityCount && enemyPool.Count > totalRestrictionCount)
                {
                    TR2Type tmp = newEntityType;
                    while (newEntityType == tmp)
                    {
                        newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                    }
                }
            }

            // #144 Disguise something as the Chicken. Pre-checks will have been done to ensure
            // the guiser is suitable for the level.
            if (Settings.DocileChickens && newEntityType == TR2Type.BirdMonster)
            {
                newEntityType = enemies.BirdMonsterGuiser;
            }

            // Make sure to convert BengalTiger, StickWieldingGoonBandana etc back to their actual types
            currentEntity.TypeID = TR2TypeUtilities.TranslateAlias(newEntityType);

            // #146 Ensure OneShot triggers are set for this enemy if needed. This currently only applies
            // to the dragon, which will be handled above in defined rooms, but the check should be made
            // here in case this needs to be extended later.
            TR2EnemyUtilities.SetEntityTriggers(level.Data, currentEntity);

            // Track every enemy type across the game
            _resultantEnemies.Add(newEntityType);
        }

        // MercSnowMobDriver relies on RedSnowmobile so it will be available in the model list
        if (!level.Is(TR2LevelNames.TIBET))
        {
            TR2Entity mercDriver = level.Data.Entities.Find(e => e.TypeID == TR2Type.MercSnowmobDriver);
            if (mercDriver != null)
            {
                TR2Entity skidoo = new()
                {
                    TypeID = TR2Type.RedSnowmobile,
                    Intensity1 = -1,
                    Intensity2 = -1
                };
                level.Data.Entities.Add(skidoo);

                Location randomLocation = VehicleUtilities.GetRandomLocation(level, TR2Type.RedSnowmobile, _generator);
                if (randomLocation != null)
                {
                    skidoo.Room = (short)randomLocation.Room;
                    skidoo.X = randomLocation.X;
                    skidoo.Y = randomLocation.Y;
                    skidoo.Z = randomLocation.Z;
                    skidoo.Angle = randomLocation.Angle;
                }
                else
                {
                    skidoo.Room = mercDriver.Room;
                    skidoo.X = mercDriver.X;
                    skidoo.Y = mercDriver.Y;
                    skidoo.Z = mercDriver.Z;
                    skidoo.Angle = mercDriver.Angle;
                }
            }
        }
        else
        {
            TR2Entity skidoo = level.Data.Entities.Find(e => e.TypeID == TR2Type.RedSnowmobile);
            if (skidoo != null)
            {
                Location randomLocation = VehicleUtilities.GetRandomLocation(level, TR2Type.RedSnowmobile, _generator);
                if (randomLocation != null)
                {
                    skidoo.Room = (short)randomLocation.Room;
                    skidoo.X = randomLocation.X;
                    skidoo.Y = randomLocation.Y;
                    skidoo.Z = randomLocation.Z;
                    skidoo.Angle = randomLocation.Angle;
                }
                else
                {
                    // A secret depends on this skidoo, so just rotate it for variety.
                    skidoo.Angle = (short)(_generator.Next(0, 8) * (ushort.MaxValue + 1) / 8);
                }
            }
        }

        // Check in case there are too many skidoo drivers
        if (level.Data.Entities.Any(e => e.TypeID == TR2Type.MercSnowmobDriver))
        {
            LimitSkidooEntities(level);
        }

        // Or too many friends - #345
        List<TR2Type> friends = TR2EnemyUtilities.GetFriendlyEnemies();
        if ((level.Is(TR2LevelNames.OPERA) || level.Is(TR2LevelNames.MONASTERY)) && enemies.Available.Any(friends.Contains))
        {
            LimitFriendlyEnemies(level, enemies.Available.Except(friends).ToList(), friends);
        }

        if (Settings.SwapEnemyAppearance)
        {
            RandomizeEnemyMeshes(level, enemies);
        }

        if (Settings.UnconditionalChickens)
        {
            MakeChickensUnconditional(level);
        }

        if (!Settings.AllowEnemyKeyDrops && (!Settings.RandomizeItems || !Settings.IncludeKeyItems))
        {
            // Shift enemies who are on top of key items so they don't pick them up.
            IEnumerable<TR2Entity> keyEnemies = level.Data.Entities.Where(enemy => TR2TypeUtilities.IsEnemyType(enemy.TypeID)
                  && level.Data.Entities.Any(key => TR2TypeUtilities.IsKeyItemType(key.TypeID) 
                  && key.GetLocation().IsEquivalent(enemy.GetLocation()))
            );

            foreach (TR2Entity enemy in keyEnemies)
            {
                enemy.X++;
            }
        }
    }

    private void LimitSkidooEntities(TR2CombinedLevel level)
    {
        // Ensure that the total implied enemy count does not exceed that of the original
        // level. The limit actually varies depending on the number of traps and other objects
        // so for those levels with high entity counts, we further restrict the limit.
        int skidooLimit = TR2EnemyUtilities.GetSkidooDriverLimit(level.Name);

        List<TR2Entity> enemies = level.GetEnemyEntities();
        int normalEnemyCount = enemies.FindAll(e => e.TypeID != TR2Type.MercSnowmobDriver).Count;
        int skidooMenCount = enemies.Count - normalEnemyCount;
        int skidooRemovalCount = skidooMenCount - skidooMenCount / 2;
        if (skidooLimit > 0)
        {
            while (skidooMenCount - skidooRemovalCount > skidooLimit)
            {
                ++skidooRemovalCount;
            }
        }

        if (skidooRemovalCount == 0)
        {
            return;
        }

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);
        List<Location> pickupLocations = level.Data.Entities
            .Where(e => TR2TypeUtilities.IsAnyPickupType(e.TypeID) && !TR2TypeUtilities.IsSecretType(e.TypeID))
            .Select(e => e.GetLocation())
            .ToList();

        List<TR2Type> replacementPool;
        if (!Settings.RandomizeItems || Settings.RandoItemDifficulty == ItemDifficulty.Default)
        {
            // The user is not specifically attempting one-item rando, so we can add anything as replacements
            replacementPool = TR2TypeUtilities.GetAmmoTypes();
        }
        else
        {
            // Camera targets don't take up any savegame space, so in one-item mode use these as replacements
            replacementPool = new() { TR2Type.CameraTarget_N };
        }

        List<TR2Entity> skidMen;
        for (int i = 0; i < skidooRemovalCount; i++)
        {
            skidMen = level.Data.Entities.FindAll(e => e.TypeID == TR2Type.MercSnowmobDriver);
            if (skidMen.Count == 0)
            {
                break;
            }

            // Select a random Skidoo driver and convert him into something else
            TR2Entity skidMan = skidMen[_generator.Next(0, skidMen.Count)];
            TR2Type newType = replacementPool[_generator.Next(0, replacementPool.Count)];
            skidMan.TypeID = newType;
            skidMan.Invisible = false;

            if (TR2TypeUtilities.IsAnyPickupType(newType))
            {
                // Move the pickup to another pickup location
                skidMan.SetLocation(pickupLocations[_generator.Next(0, pickupLocations.Count)]);
            }

            // Get rid of the old enemy's triggers
            FDUtilities.RemoveEntityTriggers(level.Data, level.Data.Entities.IndexOf(skidMan), floorData);
        }

        floorData.WriteToLevel(level.Data);
    }

    private void LimitFriendlyEnemies(TR2CombinedLevel level, List<TR2Type> pool, List<TR2Type> friends)
    {
        // Hard limit of 20 friendly enemies in trap-heavy levels to avoid freezing issues
        const int limit = 20;
        List<TR2Entity> levelFriends = level.Data.Entities.FindAll(e => friends.Contains(e.TypeID));
        while (levelFriends.Count > limit)
        {
            TR2Entity entity = levelFriends[_generator.Next(0, levelFriends.Count)];
            entity.TypeID = TR2TypeUtilities.TranslateAlias(pool[_generator.Next(0, pool.Count)]);
            levelFriends.Remove(entity);
        }
    }

    private void RandomizeEnemyMeshes(TR2CombinedLevel level, EnemyRandomizationCollection enemies)
    {
        // #314 A very primitive start to mixing-up enemy meshes - monks and yetis can take on Lara's meshes
        // without manipulation, so add a random chance of this happening if any of these models are in place.
        if (!Settings.CrossLevelEnemies)
        {
            return;
        }
        
        List<TR2Type> laraClones = new();
        const int chance = 2;
        if (!Settings.DocileChickens)
        {
            AddRandomLaraClone(enemies, TR2Type.MonkWithKnifeStick, laraClones, chance);
            AddRandomLaraClone(enemies, TR2Type.MonkWithLongStick, laraClones, chance);
        }

        AddRandomLaraClone(enemies, TR2Type.Yeti, laraClones, chance);

        if (laraClones.Count > 0)
        {
            TRModel laraModel = level.Data.Models.Find(m => m.ID == (uint)TR2Type.Lara);
            foreach (TR2Type enemyType in laraClones)
            {
                TRModel enemyModel = level.Data.Models.Find(m => m.ID == (uint)enemyType);
                enemyModel.MeshTrees = laraModel.MeshTrees;
                enemyModel.Meshes = laraModel.Meshes;
            }

            // Remove texture randomization for this enemy as it's no longer required
            TextureMonitor.ClearMonitor(level.Name, laraClones);
        }

        if (enemies.All.Contains(TR2Type.MarcoBartoli)
            && enemies.All.Contains(TR2Type.Winston)
            && _generator.Next(0, chance) == 0)
        {
            // Make Marco look and behave like Winston, until Lara gets too close
            TRModel marcoModel = level.Data.Models.Find(m => m.ID == (uint)TR2Type.MarcoBartoli);
            TRModel winnieModel = level.Data.Models.Find(m => m.ID == (uint)TR2Type.Winston);
            marcoModel.Animations = winnieModel.Animations;
            marcoModel.MeshTrees = winnieModel.MeshTrees;
            marcoModel.Meshes = winnieModel.Meshes;
        }
    }

    private void AddRandomLaraClone(EnemyRandomizationCollection enemies, TR2Type enemyType, List<TR2Type> cloneCollection, int chance)
    {
        if (enemies.All.Contains(enemyType) && _generator.Next(0, chance) == 0)
        {
            cloneCollection.Add(enemyType);
        }
    }

    private static void MakeChickensUnconditional(TR2CombinedLevel level)
    {
        // #327 Trick the game into never reaching the final frame of the death animation.
        // This results in a very abrupt death but avoids the level ending. For Ice Palace,
        // environment modifications will be made to enforce an alternative ending.
        TRModel model = level.Data.Models.Find(m => m.ID == (uint)TR2Type.BirdMonster);
        if (model != null)
        {
            model.Animations[20].FrameEnd = model.Animations[19].FrameEnd;
        }
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR2EnemyRandomizer>
    {
        private readonly Dictionary<TR2CombinedLevel, List<EnemyTransportCollection>> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR2EnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new Dictionary<TR2CombinedLevel, List<EnemyTransportCollection>>();
        }

        internal void AddLevel(TR2CombinedLevel level)
        {
            _enemyMapping.Add(level, new List<EnemyTransportCollection>(_outer.MaxPackingAttempts));
        }

        protected override void StartImpl()
        {
            // Load initially outwith the processor thread to ensure the RNG selected for each
            // level/enemy group remains consistent between randomization sessions. We allocate
            // MaxPackingAttempts number of enemy collections to attempt for packing. On the final
            // attempt, the number of entities will be reduced by one.
            List<TR2CombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR2CombinedLevel level in levels)
            {
                int count = _enemyMapping[level].Capacity;
                for (int i = 0; i < count; i++)
                {
                    _enemyMapping[level].Add(_outer.SelectCrossLevelEnemies(level, i == count - 1 ? 1 : 0));
                }
            }
        }

        // Executed in parallel, so just store the import result to process later synchronously.
        protected override void ProcessImpl()
        {
            foreach (TR2CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    int count = _enemyMapping[level].Capacity;
                    for (int i = 0; i < count; i++)
                    {
                        //if (i > 0)
                        //{
                        //    _outer.SetMessage(string.Format("Randomizing enemies [{0} - attempt {1} / {2}]", level.Name, i + 1, _outer.MaxPackingAttempts));
                        //}

                        EnemyTransportCollection enemies = _enemyMapping[level][i];
                        if (Import(level, enemies))
                        {
                            enemies.ImportResult = true;
                            break;
                        }
                    }
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        private bool Import(TR2CombinedLevel level, EnemyTransportCollection enemies)
        {
            try
            {
                // The importer will handle any duplication between the entities to import and 
                // remove so just pass the unfiltered lists to it.
                TR2ModelImporter importer = new()
                {
                    ClearUnusedSprites = true,
                    EntitiesToImport = enemies.EntitiesToImport,
                    EntitiesToRemove = enemies.EntitiesToRemove,
                    Level = level.Data,
                    LevelName = level.Name,
                    DataFolder = _outer.GetResourcePath(@"TR2\Models"),
                    TextureRemapPath = _outer.GetResourcePath(@"TR2\Textures\Deduplication\" + level.JsonID + "-TextureRemap.json"),
                    TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.EntitiesToImport)
                };

                importer.Data.AliasPriority = TR2EnemyUtilities.GetAliasPriority(level.Name, enemies.EntitiesToImport);

                // Try to import the selected models into the level.
                importer.Import();
                return true;
            }
            catch (PackingException/* e*/)
            {
                //System.Diagnostics.Debug.WriteLine(level.Name + ": " + e.Message);
                // We need to reload the level to undo anything that may have changed.
                _outer.ReloadLevelData(level);
                // Tell the monitor to no longer track what we tried to import
                _outer.TextureMonitor.ClearMonitor(level.Name, enemies.EntitiesToImport);
                return false;
            }
        }

        // This is triggered synchronously after the import work to ensure the RNG remains consistent
        internal void ApplyRandomization()
        {
            foreach (TR2CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection importedCollection = null;
                    foreach (EnemyTransportCollection enemies in _enemyMapping[level])
                    {
                        if (enemies.ImportResult)
                        {
                            importedCollection = enemies;
                            break;
                        }
                    }

                    if (importedCollection == null)
                    {
                        // Cross-level was not possible with the enemy combinations. This could be due to either
                        // a lack of space for texture packing, or the max ObjectTexture count (2048) was reached. 
                        _outer.TextureMonitor.RemoveMonitor(level.Name);

                        // And just randomize normally
                        // TODO: maybe trigger a warning to display at the end of randomizing to say that cross-
                        // level was not possible?
                        _outer.RandomizeEnemiesNatively(level);
                        //System.Diagnostics.Debug.WriteLine(level.Name + ": Native enemies");
                    }
                    else
                    {
                        // The import worked, so randomize the entities based on what we now have in place.
                        // All refers to the unmodified list so that checks such as those in RandomizeEnemyMeshes
                        // can refer to the original list, as actual entity randomization may remove models.
                        EnemyRandomizationCollection enemies = new()
                        {
                            Available = importedCollection.EntitiesToImport,
                            Droppable = TR2TypeUtilities.FilterDroppableEnemies(importedCollection.EntitiesToImport, !_outer.Settings.ProtectMonks, _outer.Settings.UnconditionalChickens),
                            Water = TR2TypeUtilities.FilterWaterEnemies(importedCollection.EntitiesToImport),
                            All = new List<TR2Type>(importedCollection.EntitiesToImport)
                        };

                        if (_outer.Settings.DocileChickens && importedCollection.BirdMonsterGuiser != TR2Type.BirdMonster)
                        {
                            DisguiseEntity(level, importedCollection.BirdMonsterGuiser, TR2Type.BirdMonster);
                            enemies.BirdMonsterGuiser = importedCollection.BirdMonsterGuiser;
                        }

                        _outer.RandomizeEnemies(level, enemies);
                        if (_outer.Settings.DevelopmentMode)
                        {
                            Debug.WriteLine(level.Name + ": " + string.Join(", ", enemies.All));
                        }
                    }

                    _outer.SaveLevel(level);
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }
    }

    internal class EnemyTransportCollection
    {
        internal List<TR2Type> EntitiesToImport { get; set; }
        internal List<TR2Type> EntitiesToRemove { get; set; }
        internal TR2Type BirdMonsterGuiser { get; set; }
        internal bool ImportResult { get; set; }

        internal EnemyTransportCollection()
        {
            ImportResult = false;
        }
    }

    internal class EnemyRandomizationCollection
    {
        internal List<TR2Type> Available { get; set; }
        internal List<TR2Type> Droppable { get; set; }
        internal List<TR2Type> Water { get; set; }
        internal List<TR2Type> All { get; set; }
        internal TR2Type BirdMonsterGuiser { get; set; }
    }
}
