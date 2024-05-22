using System.Diagnostics;
using TRDataControl;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2REnemyRandomizer : BaseTR2RRandomizer
{
    private Dictionary<TR2Type, List<string>> _gameEnemyTracker;
    private List<TR2Type> _excludedEnemies;
    private HashSet<TR2Type> _resultantEnemies;

    public TR2RDataCache DataCache { get; set; }
    public ItemFactory<TR2Entity> ItemFactory { get; set; }

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
        _excludedEnemies = new();
        _resultantEnemies = new();

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            RandomizeEnemiesNatively(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void RandomizeEnemiesCrossLevel()
    {
        SetMessage("Randomizing enemies - loading levels");

        List<EnemyProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new(this));
        }

        List<TR2RCombinedLevel> levels = new(Levels.Count);
        foreach (TRRScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR2RCombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        _gameEnemyTracker = TR2EnemyUtilities.PrepareEnemyGameTracker(false, Settings.RandoEnemyDifficulty);

        _excludedEnemies = Settings.UseEnemyExclusions
            ? Settings.ExcludedEnemies.Select(s => (TR2Type)s).ToList()
            : new();
        _resultantEnemies = new();

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

    private EnemyTransportCollection SelectCrossLevelEnemies(TR2RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return null;
        }

        // Get the list of enemy types currently in the level
        List<TR2Type> oldEntities = TR2TypeUtilities.GetEnemyTypeDictionary()[level.Name];

        int enemyCount = oldEntities.Count + TR2EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
        List<TR2Type> newEntities = new(enemyCount);

        RandoDifficulty difficulty = GetImpliedDifficulty();

        // Do we need at least one water creature?
        bool waterEnemyRequired = TR2EnemyUtilities.IsWaterEnemyRequired(level.Data);
        // Do we need at least one enemy that can drop?
        bool droppableEnemyRequired = TR2EnemyUtilities.IsDroppableEnemyRequired(level.Data);

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
        if (Settings.OneEnemyMode || Settings.IncludedEnemies.Count < newEntities.Capacity)
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
            TR2Type entity = allEnemies[_generator.Next(0, allEnemies.Count)];
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
                    newEntities.Clear();
                    newEntities.AddRange(restrictedCombinations[_generator.Next(0, restrictedCombinations.Count)]);
                }
                while (newEntities.Any(_excludedEnemies.Contains) && restrictedCombinations.Any(c => !c.Any(_excludedEnemies.Contains)));
                break;
            }

            // If it's the chicken in HSH with default behaviour, we don't want it ending the level
            if (Settings.DefaultChickens && entity == TR2Type.BirdMonster && level.Is(TR2LevelNames.HOME) && allEnemies.Except(newEntities).Count() > 1)
            {
                continue;
            }

            if (_gameEnemyTracker.ContainsKey(entity) && !_gameEnemyTracker[entity].Contains(level.Name))
            {
                if (_gameEnemyTracker[entity].Count < _gameEnemyTracker[entity].Capacity)
                {
                    _gameEnemyTracker[entity].Add(level.Name);
                }
                else if (allEnemies.Except(newEntities).Count() > 1)
                {
                    continue;
                }
            }

            List<TR2Type> family = TR2TypeUtilities.GetFamily(entity);
            if (!newEntities.Any(e1 => family.Any(e2 => e1 == e2)))
            {
                newEntities.Add(entity);
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
            List<TR2Type> friends = TR2EnemyUtilities.GetFriendlyEnemies();
            if ((level.Is(TR2LevelNames.OPERA) || level.Is(TR2LevelNames.MONASTERY)) && newEntities.All(friends.Contains))
            {
                // Add an additional "safe" enemy - so pick from the droppable range, monks and chickens excluded
                List<TR2Type> droppableEnemies = TR2TypeUtilities.GetCrossLevelDroppableEnemies(false, false);
                newEntities.Add(SelectRequiredEnemy(droppableEnemies, level, difficulty));
            }
        }

        return new()
        {
            TypesToImport = newEntities,
            TypesToRemove = oldEntities,
        };
    }

    private TR2Type SelectRequiredEnemy(List<TR2Type> pool, TR2RCombinedLevel level, RandoDifficulty difficulty)
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
            foreach (TRRScriptedLevel level in Levels)
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

    private void RandomizeEnemiesNatively(TR2RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        List<TR2Type> availableEnemyTypes = TR2TypeUtilities.GetEnemyTypeDictionary()[level.Name];
        List<TR2Type> droppableEnemies = TR2TypeUtilities.DroppableEnemyTypes()[level.Name];
        List<TR2Type> waterEnemies = TR2TypeUtilities.FilterWaterEnemies(availableEnemyTypes);

        RandomizeEnemies(level, new()
        {
            Available = availableEnemyTypes,
            Droppable = droppableEnemies,
            Water = waterEnemies,
            All = new(availableEnemyTypes),
        });
    }

    private void RandomizeEnemies(TR2RCombinedLevel level, EnemyRandomizationCollection enemies)
    {
        bool shotgunGoonSeen = level.Is(TR2LevelNames.HOME); // 1 ShotgunGoon in HSH only
        bool dragonSeen = level.Is(TR2LevelNames.LAIR); // 1 Marco in DL only

        // Get a list of current enemy entities
        List<TR2Entity> enemyEntities = level.Data.Entities.FindAll(e => TR2TypeUtilities.GetFullListOfEnemies().Contains(e.TypeID));

        RandoDifficulty difficulty = GetImpliedDifficulty();

        if (level.Is(TR2LevelNames.HOME) && !enemies.Available.Contains(TR2Type.Doberman))
        {
            // The game requires 15 items of type dog, stick goon or masked goon. The models will have been
            // eliminated at this stage, so just create a placeholder to trigger the correct HSH behaviour.
            level.Data.Models[TR2Type.Doberman] = new()
            {
                Meshes = new() { level.Data.Models[TR2Type.Lara].Meshes.First() }
            };
            level.PDPData[TR2Type.Doberman] = new();

            for (int i = 0; i < 15; i++)
            {
                level.Data.Entities.Add(new()
                {
                    TypeID = TR2Type.Doberman,
                    Room = 85,
                    X = 61952,
                    Y = 2560,
                    Z = 74240,
                    Invisible = true,
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

                if (roomDrainIndex != -1)
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

            if (newEntityType == TR2Type.ShotgunGoon && shotgunGoonSeen)
            {
                while (newEntityType == TR2Type.ShotgunGoon)
                {
                    newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                }
            }

            if (newEntityType == TR2Type.MarcoBartoli && dragonSeen)
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

            currentEntity.TypeID = TR2TypeUtilities.TranslateAlias(newEntityType);
            TR2EnemyUtilities.SetEntityTriggers(level.Data, currentEntity);

            _resultantEnemies.Add(newEntityType);
        }

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

                Location randomLocation = VehicleUtilities.GetRandomLocation(level.Name, level.Data, TR2Type.RedSnowmobile, _generator)
                    ?? mercDriver.GetLocation();
                skidoo.Room = randomLocation.Room;
                skidoo.X = randomLocation.X;
                skidoo.Y = randomLocation.Y;
                skidoo.Z = randomLocation.Z;
                skidoo.Angle = randomLocation.Angle;
            }
        }
        else
        {
            TR2Entity skidoo = level.Data.Entities.Find(e => e.TypeID == TR2Type.RedSnowmobile);
            if (skidoo != null)
            {
                Location randomLocation = VehicleUtilities.GetRandomLocation(level.Name, level.Data, TR2Type.RedSnowmobile, _generator);
                if (randomLocation != null)
                {
                    skidoo.Room = randomLocation.Room;
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

    private void LimitSkidooEntities(TR2RCombinedLevel level)
    {
        // Ensure that the total implied enemy count does not exceed that of the original
        // level. The limit actually varies depending on the number of traps and other objects
        // so for those levels with high entity counts, we further restrict the limit.
        int skidooLimit = TR2EnemyUtilities.GetSkidooDriverLimit(level.Name);

        List<TR2Entity> enemies = level.Data.Entities.FindAll(e => TR2TypeUtilities.GetFullListOfEnemies().Contains(e.TypeID));
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
            level.Data.FloorData.RemoveEntityTriggers(level.Data.Entities.IndexOf(skidMan));
        }
    }

    private void LimitFriendlyEnemies(TR2RCombinedLevel level, List<TR2Type> pool, List<TR2Type> friends)
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

    private static void MakeChickensUnconditional(TR2RCombinedLevel level)
    {
        if (level.Is(TR2LevelNames.CHICKEN))
        {
            return;
        }

        TRAnimation birdDeathAnim = level.Data.Models[TR2Type.BirdMonster]?.Animations[20];
        if (birdDeathAnim != null)
        {
            birdDeathAnim.FrameEnd = -1;
        }

        birdDeathAnim = level.PDPData[TR2Type.BirdMonster]?.Animations[20];
        if (birdDeathAnim != null)
        {
            birdDeathAnim.FrameEnd = -1;
        }
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR2REnemyRandomizer>
    {
        private readonly Dictionary<TR2RCombinedLevel, EnemyTransportCollection> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR2REnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR2RCombinedLevel level)
        {
            _enemyMapping.Add(level, new());
        }

        protected override void StartImpl()
        {
            List<TR2RCombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR2RCombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer.SelectCrossLevelEnemies(level);
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR2RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection enemies = _enemyMapping[level];
                    TR2DataImporter importer = new(true)
                    {
                        TypesToImport = enemies.TypesToImport,
                        TypesToRemove = enemies.TypesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        DataFolder = _outer.GetResourcePath(@"TR2\Objects"),
                    };

                    importer.Data.TextureObjectLimit = RandoConsts.TRRTexLimit;
                    importer.Data.TextureTileLimit = RandoConsts.TRRTileLimit;

                    string remapPath = $@"TR2\Textures\Deduplication\{level.Name}-TextureRemap.json";
                    if (_outer.ResourceExists(remapPath))
                    {
                        importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                    }

                    importer.Data.AliasPriority = TR2EnemyUtilities.GetAliasPriority(level.Name, enemies.TypesToImport);
                    
                    ImportResult<TR2Type> result = importer.Import();
                    _outer.DataCache.Merge(result, level.PDPData, level.MapData);
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        internal void ApplyRandomization()
        {
            foreach (TR2RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection importedCollection = _enemyMapping[level];
                    EnemyRandomizationCollection enemies = new()
                    {
                        Available = importedCollection.TypesToImport,
                        Droppable = TR2TypeUtilities.FilterDroppableEnemies(importedCollection.TypesToImport, !_outer.Settings.ProtectMonks, _outer.Settings.UnconditionalChickens),
                        Water = TR2TypeUtilities.FilterWaterEnemies(importedCollection.TypesToImport),
                        All = new(importedCollection.TypesToImport)
                    };

                    _outer.RandomizeEnemies(level, enemies);
                    if (_outer.Settings.DevelopmentMode)
                    {
                        Debug.WriteLine(level.Name + ": " + string.Join(", ", enemies.All));
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
        internal List<TR2Type> TypesToImport { get; set; }
        internal List<TR2Type> TypesToRemove { get; set; }
    }

    internal class EnemyRandomizationCollection
    {
        internal List<TR2Type> Available { get; set; }
        internal List<TR2Type> Droppable { get; set; }
        internal List<TR2Type> Water { get; set; }
        internal List<TR2Type> All { get; set; }
    }
}
