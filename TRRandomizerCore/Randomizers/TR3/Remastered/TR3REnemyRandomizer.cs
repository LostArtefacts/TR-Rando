using Newtonsoft.Json;
using System.Diagnostics;
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

public class TR3REnemyRandomizer : BaseTR3RRandomizer
{
    private Dictionary<TR3Type, List<string>> _gameEnemyTracker;
    private Dictionary<string, List<Location>> _pistolLocations;
    private List<TR3Type> _excludedEnemies;
    private HashSet<TR3Type> _resultantEnemies;

    public TR3RDataCache DataCache { get; set; }
    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\unarmed_locations.json"));

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

        List<TR3RCombinedLevel> levels = new(Levels.Count);
        foreach (TRRScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR3RCombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        _gameEnemyTracker = TR3EnemyUtilities.PrepareEnemyGameTracker(Settings.RandoEnemyDifficulty);
        _excludedEnemies = Settings.UseEnemyExclusions
            ? Settings.ExcludedEnemies.Select(s => (TR3Type)s).ToList()
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
        List<TR3Type> failedExclusions = _resultantEnemies.ToList().FindAll(_excludedEnemies.Contains);
        if (failedExclusions.Count > 0)
        {
            List<string> failureNames = new();
            foreach (TR3Type entity in failedExclusions)
            {
                failureNames.Add(Settings.ExcludableEnemies[(short)entity]);
            }
            failureNames.Sort();
            SetWarning(string.Format("The following enemies could not be excluded entirely from the randomization pool.{0}{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, failureNames)));
        }
    }

    private EnemyTransportCollection SelectCrossLevelEnemies(TR3RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return null;
        }

        List<TR3Type> oldEntities = GetCurrentEnemyEntities(level);
        List<TR3Type> allEnemies = TR3TypeUtilities.GetCandidateCrossLevelEnemies()
            .FindAll(e => TR3EnemyUtilities.IsEnemySupported(level.Name, e, Settings.RandoEnemyDifficulty));

        int enemyCount = oldEntities.Count + TR3EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
        List<TR3Type> newEntities = new(enemyCount);

        if (TR3TypeUtilities.GetWaterEnemies().Any(e => oldEntities.Contains(e)))
        {
            List<TR3Type> waterEnemies = TR3TypeUtilities.GetKillableWaterEnemies();
            newEntities.Add(SelectRequiredEnemy(waterEnemies, level, Settings.RandoEnemyDifficulty));
        }

        bool droppableEnemyRequired = TR3EnemyUtilities.IsDroppableEnemyRequired(level.Data);
        if (droppableEnemyRequired)
        {
            List<TR3Type> droppableEnemies = TR3TypeUtilities.FilterDroppableEnemies(allEnemies, Settings.ProtectMonks);
            newEntities.Add(SelectRequiredEnemy(droppableEnemies, level, Settings.RandoEnemyDifficulty));
        }

        foreach (TR3Type entity in TR3EnemyUtilities.GetRequiredEnemies(level.Name))
        {
            if (!newEntities.Contains(entity))
            {
                newEntities.Add(entity);
            }
        }

        foreach (int itemIndex in ItemFactory.GetLockedItems(level.Name))
        {
            TR3Entity item = level.Data.Entities[itemIndex];
            if (TR3TypeUtilities.IsEnemyType(item.TypeID))
            {
                List<TR3Type> family = TR3TypeUtilities.GetFamily(TR3TypeUtilities.GetAliasForLevel(level.Name, item.TypeID));
                if (!newEntities.Any(family.Contains))
                {
                    newEntities.Add(family[_generator.Next(0, family.Count)]);
                }
            }
        }

        if (!Settings.DocileWillard || Settings.OneEnemyMode || Settings.IncludedEnemies.Count < newEntities.Capacity)
        {
            // Willie isn't excludable in his own right because supporting a Willie-only game is impossible
            allEnemies.Remove(TR3Type.Willie);
        }

        allEnemies.RemoveAll(e => _excludedEnemies.Contains(e));

        IEnumerable<TR3Type> ex = allEnemies.Where(e => !newEntities.Any(TR3TypeUtilities.GetFamily(e).Contains));
        List<TR3Type> unalisedEntities = TR3TypeUtilities.RemoveAliases(ex);
        while (unalisedEntities.Count < newEntities.Capacity - newEntities.Count)
        {
            --newEntities.Capacity;
        }

        ISet<TR3Type> testedEntities = new HashSet<TR3Type>();
        while (newEntities.Count < newEntities.Capacity && testedEntities.Count < allEnemies.Count)
        {
            TR3Type entity = allEnemies[_generator.Next(0, allEnemies.Count)];
            testedEntities.Add(entity);

            if (!TR3EnemyUtilities.IsEnemySupported(level.Name, entity, Settings.RandoEnemyDifficulty))
            {
                continue;
            }

            // Monkeys are friendly when the tiger model is present, and when they are friendly,
            // mounting a vehicle will crash the game.
            if (level.Data.Entities.Any(e => TR3TypeUtilities.IsVehicleType(e.TypeID))
                && ((entity == TR3Type.Monkey && newEntities.Contains(TR3Type.Tiger))
                || (entity == TR3Type.Tiger && newEntities.Contains(TR3Type.Monkey))))
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

            List<TR3Type> family = TR3TypeUtilities.GetFamily(entity);
            if (!newEntities.Any(e1 => family.Any(e2 => e1 == e2)))
            {
                newEntities.Add(entity);
            }
        }

        if (newEntities.Count == 0
            || (newEntities.Capacity > 1 && newEntities.All(e => TR3EnemyUtilities.IsEnemyRestricted(level.Name, e))))
        {
            bool RestrictionCheck(TR3Type e) =>
                (droppableEnemyRequired && !TR3TypeUtilities.CanDropPickups(e, Settings.ProtectMonks))
                || !TR3EnemyUtilities.IsEnemySupported(level.Name, e, Settings.RandoEnemyDifficulty)
                || newEntities.Contains(e)
                || TR3TypeUtilities.IsWaterCreature(e)
                || TR3EnemyUtilities.IsEnemyRestricted(level.Name, e)
                || TR3TypeUtilities.TranslateAlias(e) != e;

            List<TR3Type> unrestrictedPool = allEnemies.FindAll(e => !RestrictionCheck(e));
            if (unrestrictedPool.Count == 0)
            {
                unrestrictedPool = TR3TypeUtilities.GetCandidateCrossLevelEnemies().FindAll(e => !RestrictionCheck(e));
            }

            newEntities.Add(unrestrictedPool[_generator.Next(0, unrestrictedPool.Count)]);
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

    private static List<TR3Type> GetCurrentEnemyEntities(TR3RCombinedLevel level)
    {
        List<TR3Type> allGameEnemies = TR3TypeUtilities.GetFullListOfEnemies();
        SortedSet<TR3Type> allLevelEnts = new(level.Data.Entities.Select(e => e.TypeID));
        return allLevelEnts.Where(allGameEnemies.Contains).ToList();
    }

    private TR3Type SelectRequiredEnemy(List<TR3Type> pool, TR3RCombinedLevel level, RandoDifficulty difficulty)
    {
        pool.RemoveAll(e => !TR3EnemyUtilities.IsEnemySupported(level.Name, e, difficulty));

        TR3Type entity;
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

    private void RandomizeEnemiesNatively(TR3RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        List<TR3Type> availableEnemyTypes = GetCurrentEnemyEntities(level);
        if (level.Data.Entities.Any(e => TR3TypeUtilities.IsVehicleType(e.TypeID))
            && availableEnemyTypes.Contains(TR3Type.Tiger)
            && availableEnemyTypes.Contains(TR3Type.Monkey))
        {
            TR3Type banishedType = _generator.NextDouble() < 0.5 ? TR3Type.Tiger : TR3Type.Monkey;
            availableEnemyTypes.Remove(banishedType);
            level.Data.Models.Remove(banishedType);
        }

        List<TR3Type> droppableEnemies = TR3TypeUtilities.FilterDroppableEnemies(availableEnemyTypes, Settings.ProtectMonks);
        List<TR3Type> waterEnemies = TR3TypeUtilities.FilterWaterEnemies(availableEnemyTypes);

        RandomizeEnemies(level, new()
        {
            Available = availableEnemyTypes,
            Droppable = droppableEnemies,
            Water = waterEnemies
        });
    }

    private void RandomizeEnemies(TR3RCombinedLevel level, EnemyRandomizationCollection enemies)
    {
        // Get a list of current enemy entities
        List<TR3Type> allEnemies = TR3TypeUtilities.GetFullListOfEnemies();
        List<TR3Entity> enemyEntities = level.Data.Entities.FindAll(e => allEnemies.Contains(e.TypeID));

        // First iterate through any enemies that are restricted by room
        Dictionary<TR3Type, List<int>> enemyRooms = TR3EnemyUtilities.GetRestrictedEnemyRooms(level.Name, Settings.RandoEnemyDifficulty);
        if (enemyRooms != null)
        {
            foreach (TR3Type entity in enemyRooms.Keys)
            {
                if (!enemies.Available.Contains(entity))
                {
                    continue;
                }

                List<int> rooms = enemyRooms[entity];
                int maxEntityCount = TR3EnemyUtilities.GetRestrictedEnemyLevelCount(entity, Settings.RandoEnemyDifficulty);
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
                    TR3Entity targetEntity = null;
                    do
                    {
                        int room = enemyRooms[entity][_generator.Next(0, enemyRooms[entity].Count)];
                        targetEntity = enemyEntities.Find(e => e.Room == room);
                    }
                    while (targetEntity == null);

                    // If the room has water but this enemy isn't a water enemy, we will assume that environment
                    // modifications will handle assignment of the enemy to entities.
                    if (!TR3TypeUtilities.IsWaterCreature(entity) && level.Data.Rooms[targetEntity.Room].ContainsWater)
                    {
                        continue;
                    }

                    // Some enemies need pathing like Willard but we have to honour the entity limit
                    List<Location> paths = TR3EnemyUtilities.GetAIPathing(level.Name, entity, targetEntity.Room);
                    if (ItemFactory.CanCreateItems(level.Name, level.Data.Entities, paths.Count))
                    {
                        targetEntity.TypeID = TR3TypeUtilities.TranslateAlias(entity);
                        TR3EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);
                        enemyEntities.Remove(targetEntity);

                        foreach (Location path in paths)
                        {
                            TR3Entity pathItem = ItemFactory.CreateItem(level.Name, level.Data.Entities, path);
                            pathItem.TypeID = TR3Type.AIPath_N;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                // Remove this entity type from the available rando pool
                enemies.Available.Remove(entity);
            }
        }

        foreach (TR3Entity currentEntity in enemyEntities)
        {
            TR3Type currentEntityType = currentEntity.TypeID;
            TR3Type newEntityType = currentEntityType;
            int enemyIndex = level.Data.Entities.IndexOf(currentEntity);

            // If it's an existing enemy that has to remain in the same spot, skip it
            if (TR3EnemyUtilities.IsEnemyRequired(level.Name, currentEntityType)
                || ItemFactory.IsItemLocked(level.Name, enemyIndex))
            {
                continue;
            }

            List<TR3Type> enemyPool = enemies.Available;

            // Check if the enemy drops an item
            bool hasPickupItem = level.Data.Entities
                .Any(item => TR3EnemyUtilities.HasDropItem(currentEntity, item));

            if (hasPickupItem)
            {
                enemyPool = enemies.Droppable;
            }
            else if (TR3TypeUtilities.IsWaterCreature(currentEntityType))
            {
                enemyPool = enemies.Water;
            }

            newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];

            // If we are restricting count per level for this enemy and have reached that count, pick
            // something else. This applies when we are restricting by in-level count, but not by room
            // (e.g. Winston).
            int maxEntityCount = TR3EnemyUtilities.GetRestrictedEnemyLevelCount(newEntityType, Settings.RandoEnemyDifficulty);
            if (maxEntityCount != -1)
            {
                if (level.Data.Entities.FindAll(e => e.TypeID == newEntityType).Count >= maxEntityCount && enemyPool.Count > maxEntityCount)
                {
                    TR3Type tmp = newEntityType;
                    while (newEntityType == tmp || TR3EnemyUtilities.IsEnemyRestricted(level.Name, newEntityType))
                    {
                        newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                    }
                }
            }

            TR3Entity targetEntity = currentEntity;

            if (level.Is(TR3LevelNames.CRASH) && currentEntity.Room == 15)
            {
                // Crash site raptor spawns need special treatment. The 3 entities in this (unreachable) room
                // are normally raptors, and the game positions them to the spawn points. If we no longer have
                // raptors, then replace the spawn points with the actual enemies. Otherwise, ensure they remain
                // as raptors.
                if (!enemies.Available.Contains(TR3Type.Raptor))
                {
                    TR3Entity raptorSpawn = level.Data.Entities.Find(e => e.TypeID == TR3Type.RaptorRespawnPoint_N && e.Room != 15);
                    if (raptorSpawn != null)
                    {
                        (targetEntity = raptorSpawn).TypeID = TR3TypeUtilities.TranslateAlias(newEntityType);
                        currentEntity.TypeID = TR3Type.RaptorRespawnPoint_N;
                    }
                }
            }
            else if (level.Is(TR3LevelNames.HSC))
            {
                if (currentEntity.Room == 87 && newEntityType != TR3Type.Prisoner)
                {
                    // #271 The prisoner is needed here to activate the heavy trigger for the trapdoor. If we still have
                    // prisoners in the pool, ensure one is chosen. If this isn't the case, environment rando will provide
                    // a workaround.
                    if (enemies.Available.Contains(TR3Type.Prisoner))
                    {
                        newEntityType = TR3Type.Prisoner;
                    }
                }
                else if (currentEntity.Room == 78 && newEntityType == TR3Type.Monkey)
                {
                    // #286 Monkeys cannot share AI Ambush spots largely, but these are needed here to ensure the enemies
                    // come through the gate before the timer closes them again. Just ensure no monkeys are here.
                    List<TR3Type> safePool = enemyPool.FindAll(e => e != TR3Type.Monkey && !TR3EnemyUtilities.IsEnemyRestricted(level.Name, e));
                    if (safePool.Count > 0)
                    {
                        newEntityType = safePool[_generator.Next(0, safePool.Count)];
                    }
                    else
                    {
                        // Full monkey mode means we have to move them inside the gate
                        currentEntity.Z -= 4 * TRConsts.Step4;
                    }
                }
            }
            else if (level.Is(TR3LevelNames.THAMES) && (currentEntity.Room == 61 || currentEntity.Room == 62) && newEntityType == TR3Type.Monkey)
            {
                // #286 Move the monkeys away from the AI entities
                currentEntity.Z -= TRConsts.Step4;
            }

            targetEntity.TypeID = TR3TypeUtilities.TranslateAlias(newEntityType);
            TR3EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);

            if (targetEntity.TypeID == TR3Type.Cobra)
            {
                targetEntity.Invisible = false;
            }

            _resultantEnemies.Add(newEntityType);
        }

        // Add extra ammo based on this level's difficulty
        if (Settings.CrossLevelEnemies && level.Script.RemovesWeapons)
        {
            AddUnarmedLevelAmmo(level);
        }

        if (!Settings.AllowEnemyKeyDrops && (!Settings.RandomizeItems || !Settings.IncludeKeyItems))
        {
            IEnumerable<TR3Entity> keyEnemies = level.Data.Entities.Where(enemy => TR3TypeUtilities.IsEnemyType(enemy.TypeID)
                  && level.Data.Entities.Any(key => TR3TypeUtilities.IsKeyItemType(key.TypeID)
                  && key.GetLocation().IsEquivalent(enemy.GetLocation()))
            );

            foreach (TR3Entity enemy in keyEnemies)
            {
                enemy.X++;
            }
        }
    }

    private void AddUnarmedLevelAmmo(TR3RCombinedLevel level)
    {
        if (!Settings.GiveUnarmedItems)
        {
            return;
        }

        List<TR3Type> weaponTypes = TR3TypeUtilities.GetWeaponPickups();
        TR3Entity weaponEntity = level.Data.Entities.Find(e =>
            weaponTypes.Contains(e.TypeID)
            && _pistolLocations[level.Name].Any(l => l.IsEquivalent(e.GetLocation())));

        // We can't give more ammo because HSC is so close to the limit. Instead just guarantee
        // pistols in the starting area
        Location location;
        do
        {
            location = _pistolLocations[level.Name][_generator.Next(0, _pistolLocations[level.Name].Count)];
        }
        while (location.Room != 7);

        TR3Entity pistols = ItemFactory.CreateItem(level.Name, level.Data.Entities, location);
        pistols.TypeID = TR3Type.Pistols_P;

        if (weaponEntity != null)
        {
            while (weaponEntity.TypeID == TR3Type.Pistols_P)
            {
                weaponEntity.TypeID = weaponTypes[_generator.Next(0, weaponTypes.Count - 1)];
            }
        }
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR3REnemyRandomizer>
    {
        private readonly Dictionary<TR3RCombinedLevel, EnemyTransportCollection> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR3REnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR3RCombinedLevel level)
        {
            _enemyMapping.Add(level, null);
        }

        protected override void StartImpl()
        {
            List<TR3RCombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR3RCombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer.SelectCrossLevelEnemies(level);
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR3RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection enemies = _enemyMapping[level];
                    TR3DataImporter importer = new()
                    {
                        TypesToImport = enemies.TypesToImport,
                        TypesToRemove = enemies.TypesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        DataFolder = _outer.GetResourcePath(@"TR3\Objects"),
                    };

                    importer.Data.TextureObjectLimit = RandoConsts.TRRTexLimit;
                    importer.Data.TextureTileLimit = RandoConsts.TRRTileLimit;

                    string remapPath = $@"TR3\Textures\Deduplication\{level.Name}-TextureRemap.json";
                    if (_outer.ResourceExists(remapPath))
                    {
                        importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                    }

                    ImportResult<TR3Type> result = importer.Import();
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
            foreach (TR3RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyRandomizationCollection enemies = new()
                    {
                        Available = _enemyMapping[level].TypesToImport,
                        Droppable = TR3TypeUtilities.FilterDroppableEnemies(_enemyMapping[level].TypesToImport, _outer.Settings.ProtectMonks),
                        Water = TR3TypeUtilities.FilterWaterEnemies(_enemyMapping[level].TypesToImport)
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
        internal List<TR3Type> TypesToImport { get; set; }
        internal List<TR3Type> TypesToRemove { get; set; }
    }

    internal class EnemyRandomizationCollection
    {
        internal List<TR3Type> Available { get; set; }
        internal List<TR3Type> Droppable { get; set; }
        internal List<TR3Type> Water { get; set; }
    }
}
