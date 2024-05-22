using Newtonsoft.Json;
using System.Diagnostics;
using System.Numerics;
using TRDataControl;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1REnemyRandomizer : BaseTR1RRandomizer
{
    private static readonly int _unkillableEgyptMummy = 163;
    private static readonly Location _egyptMummyLocation = new()
    {
        X = 66048,
        Y = -2304,
        Z = 73216,
        Room = 78
    };

    private static readonly int _unreachableStrongholdRoom = 18;
    private static readonly Location _strongholdCentaurLocation = new()
    {
        X = 57856,
        Y = -26880,
        Z = 43520,
        Room = 14
    };

    private static readonly List<int> _tihocanEndEnemies = new() { 73, 74, 82 };

    private Dictionary<TR1Type, List<string>> _gameEnemyTracker;
    private Dictionary<string, List<Location>> _pistolLocations;
    private Dictionary<string, List<Location>> _eggLocations;
    private Dictionary<string, List<Location>> _pierreLocations;
    private List<TR1Type> _excludedEnemies;
    private HashSet<TR1Type> _resultantEnemies;

    public TR1RDataCache DataCache { get; set; }
    public ItemFactory<TR1Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\unarmed_locations.json"));
        _eggLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\egg_locations.json"));
        _pierreLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\pierre_locations.json"));

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

        List<TR1RCombinedLevel> levels = new(Levels.Count);
        foreach (TRRScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR1RCombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        _gameEnemyTracker = TR1EnemyUtilities.PrepareEnemyGameTracker(Settings.RandoEnemyDifficulty, Levels.Select(l => l.Name));

        _excludedEnemies = Settings.UseEnemyExclusions
            ? Settings.ExcludedEnemies.Select(s => (TR1Type)s).ToList()
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

        // If any exclusions failed to be avoided, send a message
        if (Settings.ShowExclusionWarnings)
        {
            VerifyExclusionStatus();
        }
    }

    private void VerifyExclusionStatus()
    {
        List<TR1Type> failedExclusions = _resultantEnemies.ToList().FindAll(_excludedEnemies.Contains);
        if (failedExclusions.Count > 0)
        {
            List<string> failureNames = new();
            foreach (TR1Type entity in failedExclusions)
            {
                failureNames.Add(Settings.ExcludableEnemies[(short)entity]);
            }
            failureNames.Sort();
            SetWarning(string.Format("The following enemies could not be excluded entirely from the randomization pool.{0}{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, failureNames)));
        }
    }

    private void AdjustUnkillableEnemies(TR1RCombinedLevel level)
    {
        if (level.Is(TR1LevelNames.EGYPT))
        {
            level.Data.Entities[_unkillableEgyptMummy].SetLocation(_egyptMummyLocation);
        }
        else if (level.Is(TR1LevelNames.STRONGHOLD))
        {
            foreach (TR1Entity enemy in level.Data.Entities.Where(e => e.Room == _unreachableStrongholdRoom))
            {
                int index = level.Data.Entities.IndexOf(enemy);
                if (level.Data.FloorData.GetEntityTriggers(index).Count == 0)
                {
                    enemy.TypeID = TR1Type.CameraTarget_N;
                    ItemFactory.FreeItem(level.Name, index);
                }
                else
                {
                    enemy.SetLocation(_strongholdCentaurLocation);
                }
            }
        }
    }

    private EnemyTransportCollection SelectCrossLevelEnemies(TR1RCombinedLevel level)
    {
        // For the assault course, nothing will be imported for the time being
        if (level.IsAssault)
        {
            return null;
        }

        AdjustUnkillableEnemies(level);

        RandoDifficulty difficulty = GetImpliedDifficulty();

        // Get the list of enemy types currently in the level
        List<TR1Type> oldEntities = GetCurrentEnemyEntities(level);

        // Get the list of canidadates
        List<TR1Type> allEnemies = TR1TypeUtilities.GetCandidateCrossLevelEnemies();

        // Work out how many we can support
        int enemyCount = oldEntities.Count + TR1EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
        List<TR1Type> newEntities = new(enemyCount);

        // TR1 doesn't kill land creatures when underwater, so if "no restrictions" is
        // enabled, don't enforce any by default.
        bool waterEnemyRequired = difficulty == RandoDifficulty.Default
            && TR1TypeUtilities.GetWaterEnemies().Any(oldEntities.Contains);

        // Let's try to populate the list. Start by adding a water enemy if needed.
        if (waterEnemyRequired)
        {
            List<TR1Type> waterEnemies = TR1TypeUtilities.GetWaterEnemies();
            newEntities.Add(SelectRequiredEnemy(waterEnemies, level, difficulty));
        }

        // Are there any other types we need to retain?
        foreach (TR1Type entity in TR1EnemyUtilities.GetRequiredEnemies(level.Name))
        {
            if (!newEntities.Contains(entity))
            {
                newEntities.Add(entity);
            }
        }

        // Remove all exclusions from the pool, and adjust the target capacity
        allEnemies.RemoveAll(e => _excludedEnemies.Contains(e));

        IEnumerable<TR1Type> ex = allEnemies.Where(e => !newEntities.Any(TR1TypeUtilities.GetFamily(e).Contains));
        List<TR1Type> unalisedEntities = TR1TypeUtilities.RemoveAliases(ex);
        while (unalisedEntities.Count < newEntities.Capacity - newEntities.Count)
        {
            --newEntities.Capacity;
        }

        // Fill the list from the remaining candidates. Keep track of ones tested to avoid
        // looping infinitely if it's not possible to fill to capacity
        ISet<TR1Type> testedEntities = new HashSet<TR1Type>();
        List<TR1Type> eggEntities = TR1TypeUtilities.GetAtlanteanEggEnemies();
        while (newEntities.Count < newEntities.Capacity && testedEntities.Count < allEnemies.Count)
        {
            TR1Type entity = allEnemies[_generator.Next(0, allEnemies.Count)];
            testedEntities.Add(entity);

            // Make sure this isn't known to be unsupported in the level
            if (!TR1EnemyUtilities.IsEnemySupported(level.Name, entity, difficulty))
            {
                continue;
            }

            // Atlanteans and mummies are complex creatures. Grounded ones require the flyer for meshes
            // so we can't have a grounded mummy and meaty flyer, or vice versa as a result.
            if (entity == TR1Type.BandagedAtlantean && newEntities.Contains(TR1Type.MeatyFlyer) && !newEntities.Contains(TR1Type.MeatyAtlantean))
            {
                entity = TR1Type.MeatyAtlantean;
            }
            else if (entity == TR1Type.MeatyAtlantean && newEntities.Contains(TR1Type.BandagedFlyer) && !newEntities.Contains(TR1Type.BandagedAtlantean))
            {
                entity = TR1Type.BandagedAtlantean;
            }
            else if (entity == TR1Type.BandagedFlyer && newEntities.Contains(TR1Type.MeatyAtlantean))
            {
                continue;
            }
            else if (entity == TR1Type.MeatyFlyer && newEntities.Contains(TR1Type.BandagedAtlantean))
            {
                continue;
            }
            else if (entity == TR1Type.AtlanteanEgg && !newEntities.Any(eggEntities.Contains))
            {
                // Try to pick a type in the inclusion list if possible
                List<TR1Type> preferredEggTypes = eggEntities.FindAll(allEnemies.Contains);
                if (preferredEggTypes.Count == 0)
                {
                    preferredEggTypes = eggEntities;
                }
                TR1Type eggType = preferredEggTypes[_generator.Next(0, preferredEggTypes.Count)];
                newEntities.Add(eggType);
                testedEntities.Add(eggType);
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

            // GetEntityFamily returns all aliases for the likes of the dogs, but if an entity
            // doesn't have any, the returned list just contains the entity itself. This means
            // we can avoid duplicating standard enemies as well as avoiding alias-clashing.
            List<TR1Type> family = TR1TypeUtilities.GetFamily(entity);
            if (!newEntities.Any(e1 => family.Any(e2 => e1 == e2)))
            {
                newEntities.Add(entity);
            }
        }

        if
        (
            newEntities.All(e => TR1TypeUtilities.IsWaterCreature(e) || TR1EnemyUtilities.IsEnemyRestricted(level.Name, e, difficulty)) ||
            (newEntities.Capacity > 1 && newEntities.All(e => TR1EnemyUtilities.IsEnemyRestricted(level.Name, e, difficulty)))
        )
        {
            // Make sure we have an unrestricted enemy available for the individual level conditions. This will
            // guarantee a "safe" enemy for the level; we avoid aliases here to avoid further complication.
            bool RestrictionCheck(TR1Type e) =>
                !TR1EnemyUtilities.IsEnemySupported(level.Name, e, difficulty)
                || newEntities.Contains(e)
                || TR1TypeUtilities.IsWaterCreature(e)
                || TR1EnemyUtilities.IsEnemyRestricted(level.Name, e, difficulty)
                || TR1TypeUtilities.TranslateAlias(e) != e;

            List<TR1Type> unrestrictedPool = allEnemies.FindAll(e => !RestrictionCheck(e));
            if (unrestrictedPool.Count == 0)
            {
                // We are going to have to pull in the full list of candidates again, so ignoring any exclusions
                unrestrictedPool = TR1TypeUtilities.GetCandidateCrossLevelEnemies().FindAll(e => !RestrictionCheck(e));
            }

            TR1Type entity = unrestrictedPool[_generator.Next(0, unrestrictedPool.Count)];
            newEntities.Add(entity);

            if (entity == TR1Type.AtlanteanEgg && !newEntities.Any(eggEntities.Contains))
            {
                // Try to pick a type in the inclusion list if possible
                List<TR1Type> preferredEggTypes = eggEntities.FindAll(allEnemies.Contains);
                if (preferredEggTypes.Count == 0)
                {
                    preferredEggTypes = eggEntities;
                }
                TR1Type eggType = preferredEggTypes[_generator.Next(0, preferredEggTypes.Count)];
                newEntities.Add(eggType);
            }
        }

        if (Settings.DevelopmentMode)
        {
            Debug.WriteLine(level.Name + ": " + string.Join(", ", newEntities));
        }

        return new()
        {
            TypesToImport = newEntities,
            TypesToRemove = oldEntities
        };
    }

    private static List<TR1Type> GetCurrentEnemyEntities(TR1RCombinedLevel level)
    {
        List<TR1Type> allGameEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        SortedSet<TR1Type> allLevelEnts = new(level.Data.Entities.Select(e => e.TypeID));
        return allLevelEnts.Where(allGameEnemies.Contains).ToList();
    }

    private TR1Type SelectRequiredEnemy(List<TR1Type> pool, TR1RCombinedLevel level, RandoDifficulty difficulty)
    {
        pool.RemoveAll(e => !TR1EnemyUtilities.IsEnemySupported(level.Name, e, difficulty));

        TR1Type entity;
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
            List<TR1Type> includedEnemies = Settings.ExcludableEnemies.Keys.Except(Settings.ExcludedEnemies).Select(s => (TR1Type)s).ToList();
            foreach (TRRScriptedLevel level in Levels)
            {
                IEnumerable<TR1Type> restrictedRoomEnemies = TR1EnemyUtilities.GetRestrictedEnemyRooms(level.LevelFileBaseName.ToUpper(), RandoDifficulty.Default).Keys;
                if (includedEnemies.All(e => restrictedRoomEnemies.Contains(e) || _gameEnemyTracker.ContainsKey(e)))
                {
                    return RandoDifficulty.NoRestrictions;
                }
            }
        }
        return Settings.RandoEnemyDifficulty;
    }

    private void RandomizeEnemiesNatively(TR1RCombinedLevel level)
    {
        // For the assault course, nothing will be changed for the time being
        if (level.IsAssault)
        {
            return;
        }

        AdjustUnkillableEnemies(level);
        EnemyRandomizationCollection enemies = new()
        {
            Available = new(),
            Water = new()
        };

        enemies.Available.AddRange(GetCurrentEnemyEntities(level));
        enemies.Water.AddRange(TR1TypeUtilities.FilterWaterEnemies(enemies.Available));

        RandomizeEnemies(level, enemies);
    }

    private void RandomizeEnemies(TR1RCombinedLevel level, EnemyRandomizationCollection enemies)
    {
        AmendAtlanteanModels(level, enemies);

        // Get a list of current enemy entities
        List<TR1Type> allEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        List<TR1Entity> enemyEntities = level.Data.Entities.FindAll(e => allEnemies.Contains(e.TypeID));

        RandoDifficulty difficulty = GetImpliedDifficulty();

        // First iterate through any enemies that are restricted by room
        Dictionary<TR1Type, List<int>> enemyRooms = TR1EnemyUtilities.GetRestrictedEnemyRooms(level.Name, difficulty);
        if (enemyRooms != null)
        {
            foreach (TR1Type entity in enemyRooms.Keys)
            {
                if (!enemies.Available.Contains(entity))
                {
                    continue;
                }

                List<int> rooms = enemyRooms[entity];
                int maxEntityCount = TR1EnemyUtilities.GetRestrictedEnemyLevelCount(entity, difficulty);
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
                    TR1Entity targetEntity = null;
                    do
                    {
                        int room = enemyRooms[entity][_generator.Next(0, enemyRooms[entity].Count)];
                        targetEntity = enemyEntities.Find(e => e.Room == room);
                    }
                    while (targetEntity == null);

                    // If the room has water but this enemy isn't a water enemy, we will assume that environment
                    // modifications will handle assignment of the enemy to entities.
                    if (!TR1TypeUtilities.IsWaterCreature(entity) && level.Data.Rooms[targetEntity.Room].ContainsWater)
                    {
                        continue;
                    }

                    targetEntity.TypeID = TR1TypeUtilities.TranslateAlias(entity);

                    // #146 Ensure OneShot triggers are set for this enemy if needed
                    TR1EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);

                    if (Settings.HideEnemiesUntilTriggered || entity == TR1Type.Adam)
                    {
                        targetEntity.Invisible = true;
                    }

                    // Remove the target entity so it doesn't get replaced
                    enemyEntities.Remove(targetEntity);
                }

                // Remove this entity type from the available rando pool
                enemies.Available.Remove(entity);
            }
        }

        foreach (TR1Entity currentEntity in enemyEntities)
        {
            if (enemies.Available.Count == 0)
            {
                continue;
            }

            int entityIndex = level.Data.Entities.IndexOf(currentEntity);
            TR1Type currentEntityType = currentEntity.TypeID;
            TR1Type newEntityType = currentEntityType;

            // If it's an existing enemy that has to remain in the same spot, skip it
            if (TR1EnemyUtilities.IsEnemyRequired(level.Name, currentEntityType))
            {
                _resultantEnemies.Add(currentEntityType);
                continue;
            }

            List<TR1Type> enemyPool = difficulty == RandoDifficulty.Default && IsEnemyInOrAboveWater(currentEntity, level.Data)
                ? enemyPool = enemies.Water
                : enemies.Available;

            newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];

            // If we are restricting count per level for this enemy and have reached that count, pick
            // something else. This applies when we are restricting by in-level count, but not by room
            // (e.g. Kold, SkateboardKid).
            int maxEntityCount = TR1EnemyUtilities.GetRestrictedEnemyLevelCount(newEntityType, difficulty);
            if (maxEntityCount != -1)
            {
                if (GetEntityCount(level, newEntityType) >= maxEntityCount)
                {
                    List<TR1Type> pool = enemyPool.FindAll(e => !TR1EnemyUtilities.IsEnemyRestricted(level.Name, TR1TypeUtilities.TranslateAlias(e)));
                    if (pool.Count > 0)
                    {
                        newEntityType = pool[_generator.Next(0, pool.Count)];
                    }
                }
            }

            // Rather than individual enemy limits, this accounts for enemy groups such as all Atlanteans
            RestrictedEnemyGroup enemyGroup = TR1EnemyUtilities.GetRestrictedEnemyGroup(level.Name, TR1TypeUtilities.TranslateAlias(newEntityType), difficulty);
            if (enemyGroup != null)
            {
                if (level.Data.Entities.FindAll(e => enemyGroup.Enemies.Contains(e.TypeID)).Count >= enemyGroup.MaximumCount)
                {
                    List<TR1Type> pool = enemyPool.FindAll(e => !TR1EnemyUtilities.IsEnemyRestricted(level.Name, TR1TypeUtilities.TranslateAlias(e), difficulty));
                    if (pool.Count > 0)
                    {
                        newEntityType = pool[_generator.Next(0, pool.Count)];
                    }
                }
            }

            // Tomp1 switches rats/crocs automatically if a room is flooded or drained. But we may have added a normal
            // land enemy to a room that eventually gets flooded. So in default difficulty, ensure the entity is a
            // hybrid, otherwise allow land creatures underwater (which works, but is obviously more difficult).
            if (difficulty == RandoDifficulty.Default)
            {
                TR1Room currentRoom = level.Data.Rooms[currentEntity.Room];
                if (currentRoom.AlternateRoom != -1 && level.Data.Rooms[currentRoom.AlternateRoom].ContainsWater
                    && TR1TypeUtilities.IsWaterLandCreatureEquivalent(currentEntityType)
                    && !TR1TypeUtilities.IsWaterLandCreatureEquivalent(newEntityType))
                {
                    Dictionary<TR1Type, TR1Type> hybrids = TR1TypeUtilities.GetWaterEnemyLandCreatures();
                    List<TR1Type> pool = enemies.Available.FindAll(e => hybrids.ContainsKey(e) || hybrids.ContainsValue(e));
                    if (pool.Count > 0)
                    {
                        newEntityType = TR1TypeUtilities.GetWaterEnemyLandCreature(pool[_generator.Next(0, pool.Count)]);
                    }
                }
            }

            if (Settings.HideEnemiesUntilTriggered)
            {
                currentEntity.Invisible = true;
            }

            if (newEntityType == TR1Type.AtlanteanEgg)
            {
                List<TR1Type> allEggTypes = TR1TypeUtilities.GetAtlanteanEggEnemies();
                List<TR1Type> spawnTypes = enemies.Available.FindAll(allEggTypes.Contains);
                TR1Type spawnType = TR1TypeUtilities.TranslateAlias(spawnTypes[_generator.Next(0, spawnTypes.Count)]);

                Location eggLocation = _eggLocations.ContainsKey(level.Name)
                    ? _eggLocations[level.Name].Find(l => l.EntityIndex == entityIndex)
                    : null;

                if (eggLocation != null || currentEntityType == newEntityType)
                {
                    if (Settings.AllowEmptyEggs)
                    {
                        // Add 1/4 chance of an empty egg, provided at least one spawn model is not available
                        List<TR1Type> allModels = level.Data.Models.Keys.ToList();

                        // We can add Adam to make it possible for a dud spawn - he's not normally available for eggs because
                        // of his own restrictions.
                        if (!allModels.Contains(TR1Type.Adam))
                        {
                            allEggTypes.Add(TR1Type.Adam);
                        }

                        if (!allEggTypes.All(e => allModels.Contains(TR1TypeUtilities.TranslateAlias(e))) && _generator.NextDouble() < 0.25)
                        {
                            do
                            {
                                spawnType = TR1TypeUtilities.TranslateAlias(allEggTypes[_generator.Next(0, allEggTypes.Count)]);
                            }
                            while (allModels.Contains(spawnType));
                        }
                    }

                    currentEntity.CodeBits = TR1EnemyUtilities.AtlanteanToCodeBits(spawnType);
                    if (eggLocation != null)
                    {
                        currentEntity.X = eggLocation.X;
                        currentEntity.Y = eggLocation.Y;
                        currentEntity.Z = eggLocation.Z;
                        currentEntity.Angle = eggLocation.Angle;
                        currentEntity.Room = eggLocation.Room;
                    }

                    // Eggs will always be visible
                    currentEntity.Invisible = false;
                }
                else
                {
                    // We don't want an egg for this particular enemy, so just make it spawn as the actual type
                    newEntityType = spawnType;
                }
            }
            else if (currentEntityType == TR1Type.AtlanteanEgg)
            {
                // Hide what used to be eggs and reset the CodeBits otherwise this can interfere with trigger masks.
                currentEntity.Invisible = true;
                currentEntity.CodeBits = 0;
            }

            if (newEntityType == TR1Type.CentaurStatue)
            {
                AdjustCentaurStatue(currentEntity, level.Data);
            }
            else if (newEntityType == TR1Type.Adam)
            {
                // Adam should always be invisible as he is inactive high above the ground
                // so this can interfere with Lara's route - see Cistern item 36
                currentEntity.Invisible = true;
            }

            // Make sure to convert back to the actual type
            currentEntity.TypeID = TR1TypeUtilities.TranslateAlias(newEntityType);

            // #146 Ensure OneShot triggers are set for this enemy if needed
            TR1EnemyUtilities.SetEntityTriggers(level.Data, currentEntity);

            if (currentEntity.TypeID == TR1Type.Pierre
                && _pierreLocations.ContainsKey(level.Name)
                && _pierreLocations[level.Name].Find(l => l.EntityIndex == entityIndex) is Location location)
            {
                // Pierre is the only enemy who cannot be underwater, so location shifts have been predefined
                // for specific entities.
                currentEntity.SetLocation(location);
            }

            // Track every enemy type across the game
            _resultantEnemies.Add(newEntityType);
        }

        if (level.Is(TR1LevelNames.TIHOCAN)
            && !_tihocanEndEnemies.Any(e => level.Data.Entities[e].TypeID == TR1Type.Pierre))
        {
            // Add Pierre's pickups in a default place. Allows pacifist runs effectively.
            level.Data.Entities.AddRange(TR1ItemRandomizer.TihocanEndItems);
        }

        // Add extra ammo based on this level's difficulty
        if (Settings.CrossLevelEnemies && level.Script.RemovesWeapons)
        {
            AddUnarmedLevelAmmo(level);
        }
    }

    private static int GetEntityCount(TR1RCombinedLevel level, TR1Type entityType)
    {
        int count = 0;
        TR1Type translatedType = TR1TypeUtilities.TranslateAlias(entityType);
        foreach (TR1Entity entity in level.Data.Entities)
        {
            TR1Type type = entity.TypeID;
            if (type == translatedType)
            {
                count++;
            }
            else if (type == TR1Type.AdamEgg || type == TR1Type.AtlanteanEgg)
            {
                TR1Type eggType = TR1EnemyUtilities.CodeBitsToAtlantean(entity.CodeBits);
                if (eggType == translatedType && level.Data.Models.ContainsKey(eggType))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private static bool IsEnemyInOrAboveWater(TR1Entity entity, TR1Level level)
    {
        if (level.Rooms[entity.Room].ContainsWater)
        {
            return true;
        }

        // Example where we have to search is Midas room 21
        TRRoomSector sector = level.GetRoomSector(entity.X, entity.Y - TRConsts.Step1, entity.Z, entity.Room);
        while (sector.RoomBelow != TRConsts.NoRoom)
        {
            if (level.Rooms[sector.RoomBelow].ContainsWater)
            {
                return true;
            }
            sector = level.GetRoomSector(entity.X, (sector.Floor + 1) * TRConsts.Step1, entity.Z, sector.RoomBelow);
        }
        return false;
    }

    private void AmendAtlanteanModels(TR1RCombinedLevel level, EnemyRandomizationCollection enemies)
    {
        // If non-shooting grounded Atlanteans are present, we can just duplicate the model to make shooting Atlanteans
        if (enemies.Available.Any(TR1TypeUtilities.GetFamily(TR1Type.ShootingAtlantean_N).Contains))
        {
            TRModel shooter = level.Data.Models[TR1Type.ShootingAtlantean_N];
            TRModel nonShooter = level.Data.Models[TR1Type.NonShootingAtlantean_N];
            if (shooter == null && nonShooter != null)
            {
                shooter = nonShooter.Clone();
                level.Data.Models[TR1Type.ShootingAtlantean_N] = shooter;
                enemies.Available.Add(TR1Type.ShootingAtlantean_N);
            }
        }

        // If we're using flying mummies, add a chance that they'll have proper wings
        if (enemies.Available.Contains(TR1Type.BandagedFlyer) && _generator.NextDouble() < 0.5)
        {
            List<TRMesh> meshes = level.Data.Models[TR1Type.FlyingAtlantean].Meshes;
            ushort bandageTexture = meshes[1].TexturedRectangles[3].Texture;
            for (int i = 15; i < 21; i++)
            {
                TRMesh mesh = meshes[i];
                foreach (TRMeshFace face in mesh.TexturedFaces)
                {
                    face.Texture = bandageTexture;
                }
            }
        }
    }

    private static void AdjustCentaurStatue(TR1Entity entity, TR1Level level)
    {
        // If they're floating, they tend not to trigger as Lara's not within range
        TR1LocationGenerator locationGenerator = new();

        int y = entity.Y;
        short room = entity.Room;
        TRRoomSector sector = level.GetRoomSector(entity.X, y, entity.Z, room);
        while (sector.RoomBelow != TRConsts.NoRoom)
        {
            y = (sector.Floor + 1) * TRConsts.Step1;
            room = sector.RoomBelow;
            sector = level.GetRoomSector(entity.X, y, entity.Z, room);
        }

        entity.Y = sector.Floor * TRConsts.Step1;
        entity.Room = room;

        // Change this GetHeight
        if (sector.FDIndex != 0)
        {
            FDEntry entry = level.FloorData[sector.FDIndex].Find(e => e is FDSlantEntry s && s.Type == FDSlantType.Floor);
            if (entry is FDSlantEntry slant)
            {
                Vector4? bestMidpoint = locationGenerator.GetBestSlantMidpoint(slant);
                if (bestMidpoint.HasValue)
                {
                    entity.Y += (int)bestMidpoint.Value.Y;
                }
            }
        }

        entity.Invisible = false;
    }

    private void AddUnarmedLevelAmmo(TR1RCombinedLevel level)
    {
        if (!Settings.GiveUnarmedItems)
        {
            return;
        }

        // Find out which gun we have for this level
        List<TR1Type> weaponTypes = TR1TypeUtilities.GetWeaponPickups();
        TR1Entity weaponEntity = level.Data.Entities.Find(e =>
            weaponTypes.Contains(e.TypeID) 
            && _pistolLocations[level.Name].Any(l => l.IsEquivalent(e.GetLocation())));

        if (weaponEntity == null)
        {
            return;
        }

        List<TR1Type> allEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        List<TR1Entity> levelEnemies = level.Data.Entities.FindAll(e => allEnemies.Contains(e.TypeID));

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR1Entity entity = level.Data.Entities[i];
            if ((entity.TypeID == TR1Type.AtlanteanEgg || entity.TypeID == TR1Type.AdamEgg)
                && level.Data.FloorData.GetEntityTriggers(i).Count > 0)
            {
                TR1Entity resultantEnemy = new()
                {
                    TypeID = TR1EnemyUtilities.CodeBitsToAtlantean(entity.CodeBits)
                };

                if (level.Data.Models.ContainsKey(resultantEnemy.TypeID))
                {
                    levelEnemies.Add(resultantEnemy);
                }
            }
        }

        EnemyDifficulty difficulty = TR1EnemyUtilities.GetEnemyDifficulty(levelEnemies);

        // Add the pistols as a pickup if the level is hard and there aren't any other pistols around
        if (difficulty > EnemyDifficulty.Medium
            && !level.Data.Entities.Any(e => e.TypeID == TR1Type.Pistols_S_P)
            && ItemFactory.CanCreateItem(level.Name, level.Data.Entities))
        {
            TR1Entity pistols = ItemFactory.CreateItem(level.Name, level.Data.Entities);
            pistols.TypeID = TR1Type.Pistols_S_P;
            pistols.X = weaponEntity.X;
            pistols.Y = weaponEntity.Y;
            pistols.Z = weaponEntity.Z;
            pistols.Room = weaponEntity.Room;
        }

        if (difficulty > EnemyDifficulty.Easy)
        {
            while (weaponEntity.TypeID == TR1Type.Pistols_S_P)
            {
                weaponEntity.TypeID = weaponTypes[_generator.Next(0, weaponTypes.Count)];
            }
        }

        TR1Type weaponType = weaponEntity.TypeID;
        uint ammoToGive = TR1EnemyUtilities.GetStartingAmmo(weaponType);
        if (ammoToGive > 0)
        {
            ammoToGive *= (uint)difficulty;
            TR1Type ammoType = TR1TypeUtilities.GetWeaponAmmo(weaponType);
            for (int i = 0; i < ammoToGive; i++)
            {
                if (!ItemFactory.CanCreateItem(level.Name, level.Data.Entities))
                {
                    break;
                }

                TR1Entity ammo = ItemFactory.CreateItem(level.Name, level.Data.Entities, weaponEntity.GetLocation());
                ammo.TypeID = ammoType;
            }
        }
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR1REnemyRandomizer>
    {
        private readonly Dictionary<TR1RCombinedLevel, EnemyTransportCollection> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR1REnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR1RCombinedLevel level)
        {
            _enemyMapping.Add(level, null);
        }

        protected override void StartImpl()
        {
            List<TR1RCombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR1RCombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer.SelectCrossLevelEnemies(level);
            }
        }

        // Executed in parallel, so just store the import result to process later synchronously.
        protected override void ProcessImpl()
        {
            foreach (TR1RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection enemies = _enemyMapping[level];
                    List<TR1Type> importModels = new(enemies.TypesToImport);
                    if (level.Is(TR1LevelNames.KHAMOON) && (importModels.Contains(TR1Type.BandagedAtlantean) || importModels.Contains(TR1Type.BandagedFlyer)))
                    {
                        // Mummies may become shooters in Khamoon, but the missiles won't be available by default, so ensure they do get imported.
                        importModels.Add(TR1Type.Missile2_H);
                        importModels.Add(TR1Type.Missile3_H);
                    }

                    TR1DataImporter importer = new(true)
                    {
                        TypesToImport = importModels,
                        TypesToRemove = enemies.TypesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        DataFolder = _outer.GetResourcePath(@"TR1\Objects"),
                    };

                    importer.Data.TextureObjectLimit = RandoConsts.TRRTexLimit;
                    importer.Data.TextureTileLimit = RandoConsts.TRRTileLimit;

                    string remapPath = @"TR1\Textures\Deduplication\" + level.Name + "-TextureRemap.json";
                    if (_outer.ResourceExists(remapPath))
                    {
                        importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                    }

                    importer.Data.AliasPriority = TR1EnemyUtilities.GetAliasPriority(level.Name, enemies.TypesToImport);
                    
                    ImportResult<TR1Type> result = importer.Import();
                    _outer.DataCache.Merge(result, level.PDPData, level.MapData);
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        // This is triggered synchronously after the import work to ensure the RNG remains consistent
        internal void ApplyRandomization()
        {
            foreach (TR1RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyRandomizationCollection enemies = new()
                    {
                        Available = _enemyMapping[level].TypesToImport,
                        Water = TR1TypeUtilities.FilterWaterEnemies(_enemyMapping[level].TypesToImport)
                    };

                    _outer.RandomizeEnemies(level, enemies);
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
        internal List<TR1Type> TypesToImport { get; set; }
        internal List<TR1Type> TypesToRemove { get; set; }
    }

    internal class EnemyRandomizationCollection
    {
        internal List<TR1Type> Available { get; set; }
        internal List<TR1Type> Water { get; set; }
    }
}
