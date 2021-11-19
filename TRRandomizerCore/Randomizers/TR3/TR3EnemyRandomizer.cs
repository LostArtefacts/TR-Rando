using System;
using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;
using System.Diagnostics;

namespace TRRandomizerCore.Randomizers
{
    public class TR3EnemyRandomizer : BaseTR3Randomizer
    {
        private Dictionary<TR3Entities, List<string>> _gameEnemyTracker;

        // Not required until texture rando implemented
        //internal TexturePositionMonitorBroker TextureMonitor { get; set; } 

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
            foreach (TR3ScriptedLevel lvl in Levels)
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

            List<EnemyProcessor> processors = new List<EnemyProcessor>();
            for (int i = 0; i < _maxThreads; i++)
            {
                processors.Add(new EnemyProcessor(this));
            }

            List<TR3CombinedLevel> levels = new List<TR3CombinedLevel>(Levels.Count);
            foreach (TR3ScriptedLevel lvl in Levels)
            {
                levels.Add(LoadCombinedLevel(lvl));
                if (!TriggerProgress())
                {
                    return;
                }
            }

            int processorIndex = 0;
            foreach (TR3CombinedLevel level in levels)
            {
                processors[processorIndex].AddLevel(level);
                processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
            }

            // Track enemies whose counts across the game are restricted
            _gameEnemyTracker = TR3EnemyUtilities.PrepareEnemyGameTracker(Settings.RandoEnemyDifficulty);

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

            if (_processingException != null)
            {
                _processingException.Throw();
            }
        }

        private EnemyTransportCollection SelectCrossLevelEnemies(TR3CombinedLevel level)
        {
            // For the assault course, nothing will be imported for the time being
            if (level.IsAssault)
            {
                return null;
            }

            // Get the list of enemy types currently in the level
            List<TR3Entities> oldEntities = GetCurrentEnemyEntities(level);

            // Get the list of canidadates
            List<TR3Entities> allEnemies = TR3EntityUtilities.GetCandidateCrossLevelEnemies();
            if (!Settings.DocileBirdMonsters)
            {
                allEnemies.Remove(TR3Entities.Willie);
            }

            // Work out how many we can support
            int enemyCount = oldEntities.Count + TR3EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
            List<TR3Entities> newEntities = new List<TR3Entities>(enemyCount);

            // Do we need at least one water creature?
            bool waterEnemyRequired = TR3EntityUtilities.GetWaterEnemies().Any(e => oldEntities.Contains(e));
            // Do we need at least one enemy that can drop?
            bool droppableEnemyRequired = TR3EnemyUtilities.IsDroppableEnemyRequired(level);

            // Let's try to populate the list. Start by adding one water enemy
            // and one droppable enemy if they are needed.
            if (waterEnemyRequired)
            {
                List<TR3Entities> waterEnemies = TR3EntityUtilities.GetKillableWaterEnemies();
                TR3Entities entity;
                do
                {
                    entity = waterEnemies[_generator.Next(0, waterEnemies.Count)];
                }
                while (!TR3EnemyUtilities.IsEnemySupported(level.Name, entity, Settings.RandoEnemyDifficulty));
                newEntities.Add(entity);
            }

            if (droppableEnemyRequired)
            {
                List<TR3Entities> droppableEnemies = TR3EntityUtilities.FilterDroppableEnemies(allEnemies, Settings.ProtectMonks);
                TR3Entities entity;
                do
                {
                    entity = droppableEnemies[_generator.Next(0, droppableEnemies.Count)];
                }
                while (!TR3EnemyUtilities.IsEnemySupported(level.Name, entity, Settings.RandoEnemyDifficulty));
                newEntities.Add(entity);
            }

            // Are there any other types we need to retain?
            foreach (TR3Entities entity in TR3EnemyUtilities.GetRequiredEnemies(level.Name))
            {
                if (!newEntities.Contains(entity))
                {
                    newEntities.Add(entity);
                }
            }

            // Fill the list from the remaining candidates
            while (newEntities.Count < newEntities.Capacity)
            {
                TR3Entities entity = allEnemies[_generator.Next(0, allEnemies.Count)];

                // Make sure this isn't known to be unsupported in the level
                if (!TR3EnemyUtilities.IsEnemySupported(level.Name, entity, Settings.RandoEnemyDifficulty))
                {
                    continue;
                }

                // If it's Willie but Cavern is off-sequence, he can't be used
                if (entity == TR3Entities.Willie && level.Is(TR3LevelNames.WILLIE) && !level.IsWillardSequence)
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
                        // Otherwise, pick something else
                        continue;
                    }
                }

                // GetEntityFamily returns all aliases for the likes of the dogs, but if an entity
                // doesn't have any, the returned list just contains the entity itself. This means
                // we can avoid duplicating standard enemies as well as avoiding alias-clashing.
                List<TR3Entities> family = TR3EntityUtilities.GetEntityFamily(entity);
                if (!newEntities.Any(e1 => family.Any(e2 => e1 == e2)))
                {
                    newEntities.Add(entity);
                }
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

        private List<TR3Entities> GetCurrentEnemyEntities(TR3CombinedLevel level)
        {
            List<TR3Entities> allGameEnemies = TR3EntityUtilities.GetFullListOfEnemies();
            ISet<TR3Entities> allLevelEnts = new SortedSet<TR3Entities>();
            level.Data.Entities.ToList().ForEach(e => allLevelEnts.Add((TR3Entities)e.TypeID));
            List<TR3Entities> oldEntities = allLevelEnts.ToList().FindAll(e => allGameEnemies.Contains(e));
            return oldEntities;
        }

        private void RandomizeEnemiesNatively(TR3CombinedLevel level)
        {
            // For the assault course, nothing will be changed for the time being
            if (level.IsAssault)
            {
                return;
            }

            List<TR3Entities> availableEnemyTypes = GetCurrentEnemyEntities(level);
            List<TR3Entities> droppableEnemies = TR3EntityUtilities.FilterDroppableEnemies(availableEnemyTypes, Settings.ProtectMonks);
            List<TR3Entities> waterEnemies = TR3EntityUtilities.FilterWaterEnemies(availableEnemyTypes);

            RandomizeEnemies(level, new EnemyRandomizationCollection
            {
                Available = availableEnemyTypes,
                Droppable = droppableEnemies,
                Water = waterEnemies
            });
        }

        private void RandomizeEnemies(TR3CombinedLevel level, EnemyRandomizationCollection enemies)
        {
            // Get a list of current enemy entities
            List<TR3Entities> allEnemies = TR3EntityUtilities.GetFullListOfEnemies();
            List<TR2Entity> levelEntities = level.Data.Entities.ToList();
            List<TR2Entity> enemyEntities = levelEntities.FindAll(e => allEnemies.Contains((TR3Entities)e.TypeID));

            // Keep track of any new entities added (e.g. Lizard for Puna)
            List<TR2Entity> newEntities = new List<TR2Entity>();

            // First iterate through any enemies that are restricted by room
            Dictionary<TR3Entities, List<int>> enemyRooms = TR3EnemyUtilities.GetRestrictedEnemyRooms(level.Name, Settings.RandoEnemyDifficulty);
            if (enemyRooms != null)
            {
                foreach (TR3Entities entity in enemyRooms.Keys)
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
                        TR2Entity targetEntity = null;
                        do
                        {
                            int room = enemyRooms[entity][_generator.Next(0, enemyRooms[entity].Count)];
                            targetEntity = enemyEntities.Find(e => e.Room == room);
                        }
                        while (targetEntity == null);

                        // Some enemies need pathing like Willard but we have to honour the entity limit
                        List<Location> paths = TR3EnemyUtilities.GetAIPathing(level.Name, entity, targetEntity.Room);
                        if (paths.Count + levelEntities.Count <= 256)
                        {
                            targetEntity.TypeID = (short)TR3EntityUtilities.TranslateEntityAlias(entity);

                            // #146 Ensure OneShot triggers are set for this enemy if needed
                            TR3EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);

                            // Remove the target entity from the tracker list so it doesn't get replaced
                            enemyEntities.Remove(targetEntity);

                            // Add the pathing if necessary
                            foreach (Location path in paths)
                            {
                                newEntities.Add(new TR2Entity
                                {
                                    TypeID = (short)TR3Entities.AIPath_N,
                                    X = path.X,
                                    Y = path.Y,
                                    Z = path.Z,
                                    Room = (short)path.Room,
                                    Angle = path.Angle
                                });
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

            foreach (TR2Entity currentEntity in enemyEntities)
            {
                TR3Entities currentEntityType = (TR3Entities)currentEntity.TypeID;
                TR3Entities newEntityType = currentEntityType;

                // If it's an existing enemy that has to remain in the same spot, skip it
                if (TR3EnemyUtilities.IsEnemyRequired(level.Name, currentEntityType))
                {
                    continue;
                }

                List<TR3Entities> enemyPool;

                // Check if the enemy drops an item
                TR2Entity pickupEntity = levelEntities.Find
                (
                    e =>
                        e != currentEntity &&
                        e.X == currentEntity.X &&
                        e.Y == currentEntity.Y &&
                        e.Z == currentEntity.Z &&
                        TR3EntityUtilities.IsAnyPickupType((TR3Entities)e.TypeID)
                );

                if (pickupEntity != null)
                {
                    // Make sure this enemy can also drop
                    enemyPool = enemies.Droppable;
                }
                else if (TR3EntityUtilities.IsWaterCreature(currentEntityType))
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
                // (e.g. Winston).
                int maxEntityCount = TR3EnemyUtilities.GetRestrictedEnemyLevelCount(newEntityType, Settings.RandoEnemyDifficulty);
                if (maxEntityCount != -1)
                {
                    if (level.Data.Entities.ToList().FindAll(e => e.TypeID == (short)newEntityType).Count >= maxEntityCount)
                    {
                        TR3Entities tmp = newEntityType;
                        while (newEntityType == tmp || TR3EnemyUtilities.IsEnemyRestricted(level.Name, newEntityType))
                        {
                            newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                        }
                    }
                }

                TR2Entity targetEntity = currentEntity;

                if (level.Is(TR3LevelNames.CRASH) && currentEntity.Room == 15)
                {
                    // Crash site raptor spawns needs special treatment. The 3 entities in this (unreachable) room
                    // are normally raptors, and the game positions them to the spawn points. If we no longer have
                    // raptors, then replace the spawn points with the actual enemies. Otherwise, ensure they remain
                    // as raptors.
                    if (!enemies.Available.Contains(TR3Entities.Raptor))
                    {
                        TR2Entity raptorSpawn = level.Data.Entities.ToList().Find(e => e.TypeID == (short)TR3Entities.RaptorRespawnPoint_N && e.Room != 15);
                        if (raptorSpawn != null)
                        {
                            (targetEntity = raptorSpawn).TypeID = (short)TR3EntityUtilities.TranslateEntityAlias(newEntityType);
                            currentEntity.TypeID = (short)TR3Entities.RaptorRespawnPoint_N;
                        }
                    }
                }
                else
                {
                    // Make sure to convert back to the actual type
                    targetEntity.TypeID = (short)TR3EntityUtilities.TranslateEntityAlias(newEntityType);
                }

                // #146 Ensure OneShot triggers are set for this enemy if needed
                TR3EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);
            }

            // Did we add any new entities?
            if (newEntities.Count > 0)
            {
                levelEntities.AddRange(newEntities);
                level.Data.Entities = levelEntities.ToArray();
                level.Data.NumEntities = (uint)levelEntities.Count;
            }
        }

        internal class EnemyProcessor : AbstractProcessorThread<TR3EnemyRandomizer>
        {
            private readonly Dictionary<TR3CombinedLevel, EnemyTransportCollection> _enemyMapping;

            internal override int LevelCount => _enemyMapping.Count;

            internal EnemyProcessor(TR3EnemyRandomizer outer)
                : base(outer)
            {
                _enemyMapping = new Dictionary<TR3CombinedLevel, EnemyTransportCollection>();
            }

            internal void AddLevel(TR3CombinedLevel level)
            {
                _enemyMapping.Add(level, null);
            }

            protected override void StartImpl()
            {
                // Load initially outwith the processor thread to ensure the RNG selected for each
                // level/enemy group remains consistent between randomization sessions.
                List<TR3CombinedLevel> levels = new List<TR3CombinedLevel>(_enemyMapping.Keys);
                foreach (TR3CombinedLevel level in levels)
                {
                    _enemyMapping[level] = _outer.SelectCrossLevelEnemies(level);
                }
            }

            // Executed in parallel, so just store the import result to process later synchronously.
            protected override void ProcessImpl()
            {
                foreach (TR3CombinedLevel level in _enemyMapping.Keys)
                {
                    if (!level.IsAssault)
                    {
                        EnemyTransportCollection enemies = _enemyMapping[level];
                        TR3ModelImporter importer = new TR3ModelImporter
                        {
                            EntitiesToImport = enemies.EntitiesToImport,
                            EntitiesToRemove = enemies.EntitiesToRemove,
                            Level = level.Data,
                            LevelName = level.Name,
                            DataFolder = _outer.GetResourcePath(@"TR3\Models"),
                            //TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.EntitiesToImport)
                        };

                        string remapPath = @"TR3\Textures\Deduplication\" + level.Name + "-TextureRemap.json";
                        if (_outer.ResourceExists(remapPath))
                        {
                            importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                        }

                        importer.Data.AliasPriority = TR3EnemyUtilities.GetAliasPriority(level.Name, enemies.EntitiesToImport);
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
                foreach (TR3CombinedLevel level in _enemyMapping.Keys)
                {
                    if (!level.IsAssault)
                    {
                        EnemyRandomizationCollection enemies = new EnemyRandomizationCollection
                        {
                            Available = _enemyMapping[level].EntitiesToImport,
                            Droppable = TR3EntityUtilities.FilterDroppableEnemies(_enemyMapping[level].EntitiesToImport, _outer.Settings.ProtectMonks),
                            Water = TR3EntityUtilities.FilterWaterEnemies(_enemyMapping[level].EntitiesToImport)
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
            internal List<TR3Entities> EntitiesToImport { get; set; }
            internal List<TR3Entities> EntitiesToRemove { get; set; }
        }

        internal class EnemyRandomizationCollection
        {
            internal List<TR3Entities> Available { get; set; }
            internal List<TR3Entities> Droppable { get; set; }
            internal List<TR3Entities> Water { get; set; }
        }
    }
}