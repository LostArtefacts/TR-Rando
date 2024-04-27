using Newtonsoft.Json;
using System.Diagnostics;
using System.Numerics;
using TREnvironmentEditor.Model.Types;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1EnemyRandomizer : BaseTR1Randomizer
{
    public static readonly uint MaxClones = 8;
    private static readonly EnemyTransportCollection _emptyEnemies = new()
    {
        EntitiesToImport = new(),
        EntitiesToRemove = new()
    };

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

    private Dictionary<TR1Type, List<string>> _gameEnemyTracker;
    private Dictionary<string, List<Location>> _pistolLocations;
    private Dictionary<string, List<Location>> _eggLocations;
    private Dictionary<string, List<Location>> _pierreLocations;
    private List<TR1Type> _excludedEnemies;
    private ISet<TR1Type> _resultantEnemies;

    internal TR1TextureMonitorBroker TextureMonitor { get; set; }
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

        TR1Script script = ScriptEditor.Script as TR1Script;
        script.DisableTrexCollision = true;
        if (Settings.UseRecommendedCommunitySettings)
        {
            script.ConvertDroppedGuns = true;
        }
    }

    private void RandomizeExistingEnemies()
    {
        _excludedEnemies = new List<TR1Type>();
        _resultantEnemies = new HashSet<TR1Type>();

        foreach (TR1ScriptedLevel lvl in Levels)
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
        SetMessage("Randomizing enemies - loading levels");

        List<EnemyProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new EnemyProcessor(this));
        }

        List<TR1CombinedLevel> levels = new(Levels.Count);
        foreach (TR1ScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR1CombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        // Track enemies whose counts across the game are restricted
        _gameEnemyTracker = TR1EnemyUtilities.PrepareEnemyGameTracker(Settings.RandoEnemyDifficulty, Levels.Select(l => l.Name));

        // #272 Selective enemy pool - convert the shorts in the settings to actual entity types
        _excludedEnemies = Settings.UseEnemyExclusions ?
            Settings.ExcludedEnemies.Select(s => (TR1Type)s).ToList() :
            new List<TR1Type>();
        _resultantEnemies = new HashSet<TR1Type>();

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
            // A little formatting
            List<string> failureNames = new();
            foreach (TR1Type entity in failedExclusions)
            {
                failureNames.Add(Settings.ExcludableEnemies[(short)entity]);
            }
            failureNames.Sort();
            SetWarning(string.Format("The following enemies could not be excluded entirely from the randomization pool.{0}{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, failureNames)));
        }
    }

    private void AdjustUnkillableEnemies(TR1CombinedLevel level)
    {
        if (level.Is(TR1LevelNames.EGYPT))
        {
            // The OG mummy normally falls out of sight when triggered, so move it.
            level.Data.Entities[_unkillableEgyptMummy].SetLocation(_egyptMummyLocation);
        }
        else if (level.Is(TR1LevelNames.STRONGHOLD))
        {
            // There is a triggered centaur in room 18, plus several untriggered eggs for show.
            // Move the centaur, and free the eggs to be repurposed elsewhere.
            FDControl floorData = new();
            floorData.ParseFromLevel(level.Data);
            foreach (TR1Entity enemy in level.Data.Entities.Where(e => e.Room == _unreachableStrongholdRoom))
            {
                int index = level.Data.Entities.IndexOf(enemy);
                if (FDUtilities.GetEntityTriggers(floorData, index).Count == 0)
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

        level.Script.UnobtainableKills = null;
    }

    private EnemyTransportCollection SelectCrossLevelEnemies(TR1CombinedLevel level)
    {
        // For the assault course, nothing will be imported for the time being
        if (level.IsAssault)
        {
            return null;
        }

        AdjustUnkillableEnemies(level);

        if (Settings.UseEnemyClones && Settings.CloneOriginalEnemies)
        {
            // Skip import altogether for OG clone mode
            return _emptyEnemies;
        }

        // If level-ending Larson is disabled, we make an alternative ending to ToQ.
        // Do this at this stage as it effectively gets rid of ToQ-Larson meaning
        // Sanctuary-Larson can potentially be imported.
        if (level.Is(TR1LevelNames.QUALOPEC) && Settings.ReplaceRequiredEnemies)
        {
            AmendToQLarson(level);
        }

        if (level.IsExpansion)
        {
            // Ensure big eggs are randomized by converting to normal ones because
            // big eggs are never part of the enemy pool.
            level.Data.Entities.FindAll(e => e.TypeID == TR1Type.AdamEgg)
                .ForEach(e => e.TypeID = TR1Type.AtlanteanEgg);
        }

        RandoDifficulty difficulty = GetImpliedDifficulty();

        // Get the list of enemy types currently in the level
        List<TR1Type> oldEntities = GetCurrentEnemyEntities(level);

        // Get the list of canidadates
        List<TR1Type> allEnemies = TR1TypeUtilities.GetCandidateCrossLevelEnemies();

        // Work out how many we can support
        int enemyCount = oldEntities.Count + TR1EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
        if (level.Is(TR1LevelNames.QUALOPEC) && Settings.ReplaceRequiredEnemies)
        {
            // Account for Larson having been removed above.
            ++enemyCount;
        }
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
        if (!Settings.ReplaceRequiredEnemies)
        {
            foreach (TR1Type entity in TR1EnemyUtilities.GetRequiredEnemies(level.Name))
            {
                if (!newEntities.Contains(entity))
                {
                    newEntities.Add(entity);
                }
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

        if (level.Is(TR1LevelNames.PYRAMID) && Settings.ReplaceRequiredEnemies && !newEntities.Contains(TR1Type.Adam))
        {
            AmendPyramidTorso(level);
        }

        if (Settings.DevelopmentMode)
        {
            Debug.WriteLine(level.Name + ": " + string.Join(", ", newEntities));
        }

        return new EnemyTransportCollection
        {
            EntitiesToImport = newEntities,
            EntitiesToRemove = oldEntities
        };
    }

    private static List<TR1Type> GetCurrentEnemyEntities(TR1CombinedLevel level)
    {
        List<TR1Type> allGameEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        ISet<TR1Type> allLevelEnts = new SortedSet<TR1Type>();
        level.Data.Entities.ForEach(e => allLevelEnts.Add(e.TypeID));
        List<TR1Type> oldEntities = allLevelEnts.ToList().FindAll(e => allGameEnemies.Contains(e));
        return oldEntities;
    }

    private TR1Type SelectRequiredEnemy(List<TR1Type> pool, TR1CombinedLevel level, RandoDifficulty difficulty)
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
            foreach (TR1ScriptedLevel level in Levels)
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

    private void RandomizeEnemiesNatively(TR1CombinedLevel level)
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

        if (!Settings.UseEnemyClones || !Settings.CloneOriginalEnemies)
        {
            enemies.Available.AddRange(GetCurrentEnemyEntities(level));
            enemies.Water.AddRange(TR1TypeUtilities.FilterWaterEnemies(enemies.Available));
        }

        RandomizeEnemies(level, enemies);
    }

    private void RandomizeEnemies(TR1CombinedLevel level, EnemyRandomizationCollection enemies)
    {
        AmendAtlanteanModels(level, enemies);

        // Clear all default enemy item drops
        level.Script.ItemDrops.Clear();

        // Get a list of current enemy entities
        List<TR1Type> allEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        List<TR1Entity> enemyEntities = level.Data.Entities.FindAll(e => allEnemies.Contains(e.TypeID));

        RandoDifficulty difficulty = GetImpliedDifficulty();

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

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
                    TR1EnemyUtilities.SetEntityTriggers(level.Data, targetEntity, floorData);

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
            if (!Settings.ReplaceRequiredEnemies && TR1EnemyUtilities.IsEnemyRequired(level.Name, currentEntityType))
            {
                _resultantEnemies.Add(currentEntityType);
                continue;
            }

            List<TR1Type> enemyPool;
            if (difficulty == RandoDifficulty.Default && IsEnemyInOrAboveWater(currentEntity, level.Data, floorData))
            {
                // Make sure we replace with another water enemy
                enemyPool = enemies.Water;
            }
            else
            {
                // Otherwise we can pick any other available enemy
                enemyPool = enemies.Available;
            }

            // Pick a new type
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
            RandoDifficulty groupDifficulty = difficulty;
            if (level.Is(TR1LevelNames.QUALOPEC) && newEntityType == TR1Type.Larson && Settings.ReplaceRequiredEnemies)
            {
                // Non-level ending Larson is not restricted in ToQ, otherwise we adhere to the normal rules.
                groupDifficulty = RandoDifficulty.NoRestrictions;
            }
            RestrictedEnemyGroup enemyGroup = TR1EnemyUtilities.GetRestrictedEnemyGroup(level.Name, TR1TypeUtilities.TranslateAlias(newEntityType), groupDifficulty);
            if (enemyGroup != null)
            {
                if (level.Data.Entities.FindAll(e => enemyGroup.Enemies.Contains(e.TypeID)).Count >= enemyGroup.MaximumCount)
                {
                    List<TR1Type> pool = enemyPool.FindAll(e => !TR1EnemyUtilities.IsEnemyRestricted(level.Name, TR1TypeUtilities.TranslateAlias(e), groupDifficulty));
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
                TRRoom currentRoom = level.Data.Rooms[currentEntity.Room];
                if (currentRoom.AlternateRoom != -1 && level.Data.Rooms[currentRoom.AlternateRoom].ContainsWater && TR1TypeUtilities.IsWaterLandCreatureEquivalent(currentEntityType) && !TR1TypeUtilities.IsWaterLandCreatureEquivalent(newEntityType))
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
                // Default to hiding the enemy - checks below for eggs, ex-eggs, Adam and centaur
                // statues will override as necessary.
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
                        IEnumerable<TR1Type> allModels = level.Data.Models.Select(m => (TR1Type)m.ID);

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
                        currentEntity.Room = (short)eggLocation.Room;
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
                AdjustCentaurStatue(currentEntity, level.Data, floorData);
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
            TR1EnemyUtilities.SetEntityTriggers(level.Data, currentEntity, floorData);

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

        if (level.Is(TR1LevelNames.COLOSSEUM) && Settings.FixOGBugs)
        {
            FixColosseumBats(level);
        }

        if (level.Is(TR1LevelNames.TIHOCAN) && (!Settings.RandomizeItems || !Settings.IncludeKeyItems))
        {
            TR1Entity pierreReplacement = level.Data.Entities[TR1ItemRandomizer.TihocanPierreIndex];
            if (Settings.AllowEnemyKeyDrops
                && TR1EnemyUtilities.CanDropItems(pierreReplacement, level, floorData))
            {
                // Whichever enemy has taken Pierre's place will drop the items. Move the pickups to the enemy for trview lookup.
                level.Script.AddItemDrops(TR1ItemRandomizer.TihocanPierreIndex, TR1ItemRandomizer.TihocanEndItems
                    .Select(e => ItemUtilities.ConvertToScriptItem(e.TypeID)));
                foreach (TR1Entity drop in TR1ItemRandomizer.TihocanEndItems)
                {
                    level.Data.Entities.Add(new()
                    {
                        TypeID = drop.TypeID,
                        X = pierreReplacement.X,
                        Y = pierreReplacement.Y,
                        Z = pierreReplacement.Z,
                        Room = pierreReplacement.Room,
                    });
                    ItemUtilities.HideEntity(level.Data.Entities[^1]);
                }
            }
            else
            {
                // Add Pierre's pickups in a default place. Allows pacifist runs effectively.
                level.Data.Entities.AddRange(TR1ItemRandomizer.TihocanEndItems);
            }
        }

        floorData.WriteToLevel(level.Data);

        // Fix missing OG animation SFX
        FixEnemyAnimations(level);

        if (Settings.UseEnemyClones)
        {
            CloneEnemies(level);
        }

        // Add extra ammo based on this level's difficulty
        if (Settings.CrossLevelEnemies && level.Script.RemovesWeapons)
        {
            AddUnarmedLevelAmmo(level);
        }

        if (Settings.SwapEnemyAppearance)
        {
            RandomizeMeshes(level, enemies.Available);
        }
    }

    private static int GetEntityCount(TR1CombinedLevel level, TR1Type entityType)
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
                if (eggType == translatedType && level.Data.Models.Find(m => m.ID == (uint)eggType) != null)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private static bool IsEnemyInOrAboveWater(TR1Entity entity, TR1Level level, FDControl floorData)
    {
        if (level.Rooms[entity.Room].ContainsWater)
        {
            return true;
        }

        // Example where we have to search is Midas room 21
        TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y - TRConsts.Step1, entity.Z, entity.Room, level, floorData);
        while (sector.RoomBelow != TRConsts.NoRoom)
        {
            if (level.Rooms[sector.RoomBelow].ContainsWater)
            {
                return true;
            }
            sector = FDUtilities.GetRoomSector(entity.X, (sector.Floor + 1) * TRConsts.Step1, entity.Z, sector.RoomBelow, level, floorData);
        }
        return false;
    }

    private static void AmendToQLarson(TR1CombinedLevel level)
    {
        TRModel larsonModel = level.Data.Models.Find(m => m.ID == (uint)TR1Type.Larson);
        if (larsonModel != null)
        {
            // Convert the Larson model into the Great Pyramid scion to allow ending the level. Larson will
            // become a raptor to allow for normal randomization. Environment mods will handle the specifics here. 
            larsonModel.ID = (uint)TR1Type.ScionPiece3_S_P;
            level.Data.Entities
                .Where(e => e.TypeID == TR1Type.Larson)
                .ToList()
                .ForEach(e => e.TypeID = TR1Type.Raptor);

            // Make the scion invisible.
            MeshEditor editor = new();
            foreach (TRMesh mesh in larsonModel.Meshes)
            {
                editor.Mesh = mesh;
                editor.ClearAllPolygons();
            }
        }
    }

    private void AmendPyramidTorso(TR1CombinedLevel level)
    {
        // We want to keep Adam's egg, but simulate something else hatching.
        // In hard mode, two enemies take his place.
        level.RemoveModel(TR1Type.Adam);
        
        TR1Entity egg = level.Data.Entities.Find(e => e.TypeID == TR1Type.AdamEgg);
        TR1Entity lara = level.Data.Entities.Find(e => e.TypeID == TR1Type.Lara);

        EMAppendTriggerActionFunction trigFunc = new()
        {
            Location = new()
            {
                X = lara.X,
                Y = lara.Y,
                Z = lara.Z,
                Room = lara.Room
            },
            Actions = new()
        };

        int count = Settings.RandoEnemyDifficulty == RandoDifficulty.Default ? 1 : 2;
        for (int i = 0; i < count; i++)
        {
            trigFunc.Actions.Add(new()
            {
                Parameter = (short)level.Data.Entities.Count
            });

            level.Data.Entities.Add(new()
            {
                TypeID = TR1Type.Adam,
                X = egg.X,
                Y = egg.Y - i * TRConsts.Step4,
                Z = egg.Z - TRConsts.Step4,
                Room = egg.Room,
                Angle = egg.Angle,
                Intensity = egg.Intensity,
                Invisible = true
            });
        }

        trigFunc.ApplyToLevel(level.Data);
    }

    private void AmendAtlanteanModels(TR1CombinedLevel level, EnemyRandomizationCollection enemies)
    {
        // If non-shooting grounded Atlanteans are present, we can just duplicate the model to make shooting Atlanteans
        if (enemies.Available.Any(TR1TypeUtilities.GetFamily(TR1Type.ShootingAtlantean_N).Contains))
        {
            TRModel shooter = level.Data.Models.Find(m => m.ID == (uint)TR1Type.ShootingAtlantean_N);
            TRModel nonShooter = level.Data.Models.Find(m => m.ID == (uint)TR1Type.NonShootingAtlantean_N);
            if (shooter == null && nonShooter != null)
            {
                shooter = nonShooter.Clone();
                shooter.ID = (uint)TR1Type.ShootingAtlantean_N;
                level.Data.Models.Add(shooter);
                enemies.Available.Add(TR1Type.ShootingAtlantean_N);
            }
        }

        // If we're using flying mummies, add a chance that they'll have proper wings
        if (enemies.Available.Contains(TR1Type.BandagedFlyer) && _generator.NextDouble() < 0.5)
        {
            List<TRMesh> meshes = level.Data.Models.Find(m => m.ID == (uint)TR1Type.FlyingAtlantean).Meshes;
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

    private static void AdjustCentaurStatue(TR1Entity entity, TR1Level level, FDControl floorData)
    {
        // If they're floating, they tend not to trigger as Lara's not within range
        TR1LocationGenerator locationGenerator = new();

        int y = entity.Y;
        short room = entity.Room;
        TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, y, entity.Z, room, level, floorData);
        while (sector.RoomBelow != TRConsts.NoRoom)
        {
            y = (sector.Floor + 1) * TRConsts.Step1;
            room = sector.RoomBelow;
            sector = FDUtilities.GetRoomSector(entity.X, y, entity.Z, room, level, floorData);
        }

        entity.Y = sector.Floor * TRConsts.Step1;
        entity.Room = room;

        if (sector.FDIndex != 0)
        {
            FDEntry entry = floorData.Entries[sector.FDIndex].Find(e => e is FDSlantEntry s && s.Type == FDSlantEntryType.FloorSlant);
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

    private void AddUnarmedLevelAmmo(TR1CombinedLevel level)
    {
        if (!Settings.GiveUnarmedItems)
        {
            return;
        }

        // Find out which gun we have for this level
        List<TR1Type> weaponTypes = TR1TypeUtilities.GetWeaponPickups();
        List<TR1Entity> levelWeapons = level.Data.Entities.FindAll(e => weaponTypes.Contains(e.TypeID));
        TR1Entity weaponEntity = null;
        foreach (TR1Entity weapon in levelWeapons)
        {
            int match = _pistolLocations[level.Name].FindIndex
            (
                location =>
                    location.X == weapon.X &&
                    location.Y == weapon.Y &&
                    location.Z == weapon.Z &&
                    location.Room == weapon.Room
            );
            if (match != -1)
            {
                weaponEntity = weapon;
                break;
            }
        }

        if (weaponEntity == null)
        {
            return;
        }

        List<TR1Type> allEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        List<TR1Entity> levelEnemies = level.Data.Entities.FindAll(e => allEnemies.Contains(e.TypeID));
        // #409 Eggs are excluded as they are not part of the cross-level enemy pool, so create copies of any
        // of these using their actual types so to ensure they are part of the difficulty calculation.
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);
        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR1Entity entity = level.Data.Entities[i];
            if ((entity.TypeID == TR1Type.AtlanteanEgg || entity.TypeID == TR1Type.AdamEgg)
                && FDUtilities.GetEntityTriggers(floorData, i).Count > 0)
            {
                TR1Entity resultantEnemy = new()
                {
                    TypeID = TR1EnemyUtilities.CodeBitsToAtlantean(entity.CodeBits)
                };

                // Only include it if the model is present i.e. it's not an empty egg.
                if (level.Data.Models.Find(m => (TR1Type)m.ID == resultantEnemy.TypeID) != null)
                {
                    levelEnemies.Add(resultantEnemy);
                }
            }
        }

        EnemyDifficulty difficulty = TR1EnemyUtilities.GetEnemyDifficulty(levelEnemies);

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
            level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(ammoType), ammoToGive);

            uint smallMediToGive = 0;
            uint largeMediToGive = 0;

            if (difficulty == EnemyDifficulty.Medium || difficulty == EnemyDifficulty.Hard)
            {
                smallMediToGive++;
                largeMediToGive++;
            }
            if (difficulty > EnemyDifficulty.Medium)
            {
                largeMediToGive++;
            }
            if (difficulty == EnemyDifficulty.VeryHard)
            {
                largeMediToGive++;
            }

            level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR1Type.SmallMed_S_P), smallMediToGive);
            level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR1Type.LargeMed_S_P), largeMediToGive);
        }

        // Add the pistols as a pickup if the level is hard and there aren't any other pistols around
        if (difficulty > EnemyDifficulty.Medium
            && levelWeapons.Find(e => e.TypeID == TR1Type.Pistols_S_P) == null
            && ItemFactory.CanCreateItem(level.Name, level.Data.Entities))
        {
            TR1Entity pistols = ItemFactory.CreateItem(level.Name, level.Data.Entities);
            pistols.TypeID = TR1Type.Pistols_S_P;
            pistols.X = weaponEntity.X;
            pistols.Y = weaponEntity.Y;
            pistols.Z = weaponEntity.Z;
            pistols.Room = weaponEntity.Room;
        }
    }

    private void RandomizeMeshes(TR1CombinedLevel level, List<TR1Type> availableEnemies)
    {
        if (level.Is(TR1LevelNames.ATLANTIS))
        {
            // Atlantis scion swap - Model => Mesh index
            Dictionary<TR1Type, int> scionSwaps = new()
            {
                [TR1Type.Lara] = 3,
                [TR1Type.Pistols_M_H] = 1,
                [TR1Type.Shotgun_M_H] = 0,
                [TR1Type.ShotgunAmmo_M_H] = 0,
                [TR1Type.Magnums_M_H] = 1,
                [TR1Type.Uzis_M_H] = 1,
                [TR1Type.Dart_H] = 0,
                [TR1Type.Sunglasses_M_H] = 0,
                [TR1Type.CassettePlayer_M_H] = 1
            };

            List<TRMesh> scion = level.Data.Models.Find(m => m.ID == (uint)TR1Type.ScionPiece4_S_P).Meshes;
            List<TR1Type> replacementKeys = scionSwaps.Keys.ToList();
            TR1Type replacement = replacementKeys[_generator.Next(0, replacementKeys.Count)];

            List<TRMesh> replacementMeshes = level.Data.Models.Find(m => m.ID == (uint)replacement).Meshes;
            int colRadius = scion[0].CollRadius;
            TRMeshUtilities.DuplicateMesh(level.Data, scion[0], replacementMeshes[scionSwaps[replacement]]);
            scion[0].CollRadius = colRadius; // Retain original as Lara may need to shoot it

            // Cutscene head swaps
            List<TRMesh> lara = level.Data.Models.Find(m => m.ID == (uint)TR1Type.CutsceneActor1).Meshes;
            List<TRMesh> natla = level.Data.Models.Find(m => m.ID == (uint)TR1Type.CutsceneActor3).Meshes;
            List<TRMesh> pierre = level.Data.Models.Find(m => m.ID == (uint)TR1Type.Pierre).Meshes;

            switch (_generator.Next(0, 6))
            {
                case 0:
                    // Natla becomes Lara
                    TRMeshUtilities.DuplicateMesh(level.CutSceneLevel.Data, natla[8], lara[14]);
                    break;
                case 1:
                    // Lara becomes Natla
                    TRMeshUtilities.DuplicateMesh(level.CutSceneLevel.Data, lara[14], natla[8]);
                    break;
                case 2:
                    // Switch Lara and Natla
                    TRMesh laraHead = MeshEditor.CloneMesh(lara[14]);
                    TRMesh natlaHead = MeshEditor.CloneMesh(natla[8]);
                    TRMeshUtilities.DuplicateMesh(level.CutSceneLevel.Data, lara[14], natlaHead);
                    TRMeshUtilities.DuplicateMesh(level.CutSceneLevel.Data, natla[8], laraHead);
                    break;
                case 3:
                    // Natla becomes Pierre
                    TRMeshUtilities.DuplicateMesh(level.CutSceneLevel.Data, natla[8], pierre[8]);
                    break;
                case 4:
                    // Lara becomes Pierre
                    TRMeshUtilities.DuplicateMesh(level.CutSceneLevel.Data, lara[14], pierre[8]);
                    break;
                case 5:
                    // Two Pierres
                    TRMeshUtilities.DuplicateMesh(level.CutSceneLevel.Data, natla[8], pierre[8]);
                    TRMeshUtilities.DuplicateMesh(level.CutSceneLevel.Data, lara[14], pierre[8]);
                    break;
            }
        }

        if (availableEnemies.Contains(TR1Type.Adam) && _generator.NextDouble() < 0.4)
        {
            // Replace Adam's head with a much larger version of Natla's, Larson's or normal/angry Lara's.
            List<TRMesh> adam = level.Data.Models.Find(m => m.ID == (uint)TR1Type.Adam).Meshes;
            TRMesh replacement;
            if (availableEnemies.Contains(TR1Type.Natla) && _generator.NextDouble() < 0.5)
            {
                replacement = level.Data.Models.Find(m => m.ID == (uint)TR1Type.Natla).Meshes[2];
            }
            else if (availableEnemies.Contains(TR1Type.Larson) && _generator.NextDouble() < 0.5)
            {
                replacement = level.Data.Models.Find(m => m.ID == (uint)TR1Type.Larson).Meshes[8];
            }
            else if (availableEnemies.Contains(TR1Type.Pierre) && _generator.NextDouble() < 0.5)
            {
                replacement = level.Data.Models.Find(m => m.ID == (uint)TR1Type.Pierre).Meshes[8];
            }
            else
            {
                TR1Type laraSwapType = _generator.NextDouble() < 0.5 ? TR1Type.LaraUziAnimation_H : TR1Type.Lara;
                replacement = level.Data.Models.Find(m => m.ID == (uint)laraSwapType).Meshes[14];                
            }

            TRMeshUtilities.DuplicateMesh(level.Data, adam[3], MeshEditor.CloneMesh(replacement));

            // Enlarge and rotate about Y
            foreach (TRVertex vertex in adam[3].Vertices)
            {
                vertex.X = (short)(vertex.X * -6);
                vertex.Y = (short)(vertex.Y * 6);
                vertex.Z = (short)(vertex.Z * -6);
            }

            adam[3].CollRadius *= 6;

            // Replace the neck texture to suit the head
            for (int i = 1; i < 3; i++)
            {
                foreach (TRMeshFace face in adam[i].TexturedTriangles)
                {
                    face.Texture = adam[0].TexturedTriangles[0].Texture;
                }
                foreach (TRMeshFace face in adam[i].TexturedRectangles)
                {
                    face.Texture = adam[0].TexturedRectangles[0].Texture;
                }
            }
        }

        if (availableEnemies.Contains(TR1Type.Pierre) && _generator.NextDouble() < 0.25)
        {
            // Replace Pierre's head with a slightly bigger version of Lara's (either angry Lara or normal Lara)
            List<TRMesh> pierre = level.Data.Models.Find(m => m.ID == (uint)TR1Type.Pierre).Meshes;
            List<TRMesh> lara = level.Data.Models.Find(m => m.ID == (uint)TR1Type.Lara).Meshes;
            List<TRMesh> laraUziAnim = level.Data.Models.Find(m => m.ID == (uint)TR1Type.LaraUziAnimation_H).Meshes;

            TRMeshUtilities.DuplicateMesh(level.Data, pierre[8], MeshEditor.CloneMesh(_generator.NextDouble() < 0.5 ? laraUziAnim[14] : lara[14]));
            foreach (TRVertex vertex in pierre[8].Vertices)
            {
                vertex.X = (short)(vertex.X * 1.5 + 6);
                vertex.Y = (short)(vertex.Y * 1.5);
                vertex.Z = (short)(vertex.Z * 1.5);
            }

            pierre[8].CollRadius = (short)(lara[14].CollRadius * 1.5);
        }
    }

    private void FixEnemyAnimations(TR1CombinedLevel level)
    {
        // Model transport will handle these missing SFX by default, but we need to fix them in
        // the levels where these enemies already exist.
        List<TR1Type> entities = level.Data.Models.Select(m => (TR1Type)m.ID).ToList();

        if (entities.Contains(TR1Type.Pierre)
            && (level.Is(TR1LevelNames.FOLLY) || level.Is(TR1LevelNames.COLOSSEUM) || level.Is(TR1LevelNames.CISTERN) || level.Is(TR1LevelNames.TIHOCAN)))
        {
            TR1ModelExporter.AmendPierreGunshot(level.Data);
            TR1ModelExporter.AmendPierreDeath(level.Data);

            // Non one-shot-Pierre levels won't have the death sound by default, so borrow it from ToT.
            if (!level.Data.SoundEffects.ContainsKey(TR1SFX.PierreDeath))
            {
                TR1Level tihocan = new TR1LevelControl().Read(Path.Combine(BackupPath, TR1LevelNames.TIHOCAN));
                level.Data.SoundEffects[TR1SFX.PierreDeath] = tihocan.SoundEffects[TR1SFX.PierreDeath];
            }
        }

        if (entities.Contains(TR1Type.Larson) && level.Is(TR1LevelNames.SANCTUARY))
        {
            TR1ModelExporter.AmendLarsonDeath(level.Data);
        }

        if (entities.Contains(TR1Type.SkateboardKid) && level.Is(TR1LevelNames.MINES))
        {
            TR1ModelExporter.AmendSkaterBoyDeath(level.Data);
        }

        if (entities.Contains(TR1Type.Natla) && level.Is(TR1LevelNames.PYRAMID))
        {
            TR1ModelExporter.AmendNatlaDeath(level.Data);
        }
    }

    private static void FixColosseumBats(TR1CombinedLevel level)
    {
        // Fix the bat trigger in Colosseum. Done outside of environment mods to allow for cloning.
        // Item 74 is duplicated in each trigger.
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        foreach (FDTriggerEntry trigger in FDUtilities.GetEntityTriggers(floorData, 74))
        {
            List<FDActionListItem> actions = trigger.TrigActionList
                .FindAll(a => a.TrigAction == FDTrigAction.Object && a.Parameter == 74);
            if (actions.Count == 2)
            {
                actions[0].Parameter = 73;
            }
        }

        floorData.WriteToLevel(level.Data);
    }

    private void CloneEnemies(TR1CombinedLevel level)
    {
        List<TR1Type> enemyTypes = TR1TypeUtilities.GetFullListOfEnemies();
        List<TR1Entity> enemies = level.Data.Entities.FindAll(e => enemyTypes.Contains(e.TypeID));

        // If Adam is still in his egg, clone the egg as well. Otherwise there will be separate
        // entities inside the egg that will have already been accounted for.
        TR1Entity adamEgg = level.Data.Entities.Find(e => e.TypeID == TR1Type.AdamEgg);
        if (adamEgg != null
            && TR1EnemyUtilities.CodeBitsToAtlantean(adamEgg.CodeBits) == TR1Type.Adam
            && level.Data.Models.Find(m => m.ID == (uint)TR1Type.Adam) != null)
        {
            enemies.Add(adamEgg);
        }

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);
        
        uint cloneCount = Math.Max(2, Math.Min(MaxClones, Settings.EnemyMultiplier)) - 1;
        short angleDiff = (short)Math.Ceiling(ushort.MaxValue / (cloneCount + 1d));

        foreach (TR1Entity enemy in enemies)
        {
            List<FDTriggerEntry> triggers = FDUtilities.GetEntityTriggers(floorData, level.Data.Entities.IndexOf(enemy));
            if (Settings.UseKillableClonePierres && enemy.TypeID == TR1Type.Pierre)
            {
                // Ensure OneShot, otherwise only ever one runaway Pierre
                triggers.ForEach(t => t.TrigSetup.OneShot = true);
            }

            for (int i = 0; i < cloneCount; i++)
            {
                foreach (FDTriggerEntry trigger in triggers)
                {
                    trigger.TrigActionList.Add(new()
                    {
                        TrigAction = FDTrigAction.Object,
                        Parameter = (ushort)level.Data.Entities.Count
                    });
                }

                TR1Entity clone = (TR1Entity)enemy.Clone();
                level.Data.Entities.Add(clone);

                if (enemy.TypeID != TR1Type.AtlanteanEgg
                    && enemy.TypeID != TR1Type.AdamEgg)
                {
                    clone.Angle -= (short)((i + 1) * angleDiff);
                }
            }
        }

        floorData.WriteToLevel(level.Data);
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR1EnemyRandomizer>
    {
        private readonly Dictionary<TR1CombinedLevel, EnemyTransportCollection> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR1EnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new Dictionary<TR1CombinedLevel, EnemyTransportCollection>();
        }

        internal void AddLevel(TR1CombinedLevel level)
        {
            _enemyMapping.Add(level, null);
        }

        protected override void StartImpl()
        {
            // Load initially outwith the processor thread to ensure the RNG selected for each
            // level/enemy group remains consistent between randomization sessions.
            List<TR1CombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR1CombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer.SelectCrossLevelEnemies(level);
            }
        }

        // Executed in parallel, so just store the import result to process later synchronously.
        protected override void ProcessImpl()
        {
            foreach (TR1CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection enemies = _enemyMapping[level];
                    List<TR1Type> importModels = new(enemies.EntitiesToImport);
                    if (level.Is(TR1LevelNames.KHAMOON) && (importModels.Contains(TR1Type.BandagedAtlantean) || importModels.Contains(TR1Type.BandagedFlyer)))
                    {
                        // Mummies may become shooters in Khamoon, but the missiles won't be available by default, so ensure they do get imported.
                        importModels.Add(TR1Type.Missile2_H);
                        importModels.Add(TR1Type.Missile3_H);
                    }

                    TR1ModelImporter importer = new(true)
                    {
                        EntitiesToImport = importModels,
                        EntitiesToRemove = enemies.EntitiesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        DataFolder = _outer.GetResourcePath(@"TR1\Models"),
                        TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.EntitiesToImport)
                    };

                    string remapPath = @"TR1\Textures\Deduplication\" + level.Name + "-TextureRemap.json";
                    if (_outer.ResourceExists(remapPath))
                    {
                        importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                    }

                    importer.Data.AliasPriority = TR1EnemyUtilities.GetAliasPriority(level.Name, enemies.EntitiesToImport);
                    importer.Import();
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
            foreach (TR1CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyRandomizationCollection enemies = new()
                    {
                        Available = _enemyMapping[level].EntitiesToImport,
                        Water = TR1TypeUtilities.FilterWaterEnemies(_enemyMapping[level].EntitiesToImport)
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
        internal List<TR1Type> EntitiesToImport { get; set; }
        internal List<TR1Type> EntitiesToRemove { get; set; }
    }

    internal class EnemyRandomizationCollection
    {
        internal List<TR1Type> Available { get; set; }
        internal List<TR1Type> Water { get; set; }
    }
}
