using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers
{
    public class TR3EnemyRandomizer : BaseTR3Randomizer
    {
        private Dictionary<TR3Entities, List<string>> _gameEnemyTracker;
        private Dictionary<string, List<Location>> _pistolLocations;
        private List<TR3Entities> _excludedEnemies;
        private ISet<TR3Entities> _resultantEnemies;

        internal TR3TextureMonitorBroker TextureMonitor { get; set; }
        public ItemFactory ItemFactory { get; set; }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
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
            _excludedEnemies = new List<TR3Entities>();
            _resultantEnemies = new HashSet<TR3Entities>();

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

            // #272 Selective enemy pool - convert the shorts in the settings to actual entity types
            _excludedEnemies = Settings.UseEnemyExclusions ?
                Settings.ExcludedEnemies.Select(s => (TR3Entities)s).ToList() :
                new List<TR3Entities>();
            _resultantEnemies = new HashSet<TR3Entities>();

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

            // If any exclusions failed to be avoided, send a message
            if (Settings.ShowExclusionWarnings)
            {
                VerifyExclusionStatus();
            }
        }

        private void VerifyExclusionStatus()
        {
            List<TR3Entities> failedExclusions = _resultantEnemies.ToList().FindAll(_excludedEnemies.Contains);
            if (failedExclusions.Count > 0)
            {
                // A little formatting
                List<string> failureNames = new List<string>();
                foreach (TR3Entities entity in failedExclusions)
                {
                    failureNames.Add(Settings.ExcludableEnemies[(short)entity]);
                }
                failureNames.Sort();
                SetWarning(string.Format("The following enemies could not be excluded entirely from the randomization pool.{0}{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, failureNames)));
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
            List<TR3Entities> allEnemies = TR3EntityUtilities.GetCandidateCrossLevelEnemies().FindAll(e => TR3EnemyUtilities.IsEnemySupported(level.Name, e, Settings.RandoEnemyDifficulty));
            
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
                newEntities.Add(SelectRequiredEnemy(waterEnemies, level, Settings.RandoEnemyDifficulty));
            }

            if (droppableEnemyRequired)
            {
                List<TR3Entities> droppableEnemies = TR3EntityUtilities.FilterDroppableEnemies(allEnemies, Settings.ProtectMonks);
                newEntities.Add(SelectRequiredEnemy(droppableEnemies, level, Settings.RandoEnemyDifficulty));
            }

            // Are there any other types we need to retain?
            foreach (TR3Entities entity in TR3EnemyUtilities.GetRequiredEnemies(level.Name))
            {
                if (!newEntities.Contains(entity))
                {
                    newEntities.Add(entity);
                }
            }

            if (!Settings.DocileWillard || Settings.OneEnemyMode || Settings.IncludedEnemies.Count < newEntities.Capacity)
            {
                // Willie isn't excludable in his own right because supporting a Willie-only game is impossible
                allEnemies.Remove(TR3Entities.Willie);
            }

            // Remove all exclusions from the pool, and adjust the target capacity
            allEnemies.RemoveAll(e => _excludedEnemies.Contains(e));

            IEnumerable<TR3Entities> ex = allEnemies.Where(e => !newEntities.Any(TR3EntityUtilities.GetEntityFamily(e).Contains));
            List<TR3Entities> unalisedEntities = TR3EntityUtilities.RemoveAliases(ex);
            while (unalisedEntities.Count < newEntities.Capacity - newEntities.Count)
            {
                --newEntities.Capacity;
            }

            // Fill the list from the remaining candidates. Keep track of ones tested to avoid
            // looping infinitely if it's not possible to fill to capacity
            ISet<TR3Entities> testedEntities = new HashSet<TR3Entities>();
            while (newEntities.Count < newEntities.Capacity && testedEntities.Count < allEnemies.Count)
            {
                TR3Entities entity = allEnemies[_generator.Next(0, allEnemies.Count)];
                testedEntities.Add(entity);

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
                List<TR3Entities> family = TR3EntityUtilities.GetEntityFamily(entity);
                if (!newEntities.Any(e1 => family.Any(e2 => e1 == e2)))
                {
                    newEntities.Add(entity);
                }
            }

            if (newEntities.Capacity > 1 && newEntities.All(e => TR3EnemyUtilities.IsEnemyRestricted(level.Name, e)))
            {
                // Make sure we have an unrestricted enemy available for the individual level conditions. This will
                // guarantee a "safe" enemy for the level; we avoid aliases here to avoid further complication.
                bool RestrictionCheck(TR3Entities e) =>
                    (droppableEnemyRequired && !TR3EntityUtilities.CanDropPickups(e, Settings.ProtectMonks))
                    || !TR3EnemyUtilities.IsEnemySupported(level.Name, e, Settings.RandoEnemyDifficulty)
                    || newEntities.Contains(e)
                    || TR3EntityUtilities.IsWaterCreature(e)
                    || TR3EnemyUtilities.IsEnemyRestricted(level.Name, e)
                    || TR3EntityUtilities.TranslateEntityAlias(e) != e;

                List<TR3Entities> unrestrictedPool = allEnemies.FindAll(e => !RestrictionCheck(e));
                if (unrestrictedPool.Count == 0)
                {
                    // We are going to have to pull in the full list of candiates again, so ignoring any exclusions
                    unrestrictedPool = TR3EntityUtilities.GetCandidateCrossLevelEnemies().FindAll(e => !RestrictionCheck(e));
                }

                newEntities.Add(unrestrictedPool[_generator.Next(0, unrestrictedPool.Count)]);
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

        private TR3Entities SelectRequiredEnemy(List<TR3Entities> pool, TR3CombinedLevel level, RandoDifficulty difficulty)
        {
            pool.RemoveAll(e => !TR3EnemyUtilities.IsEnemySupported(level.Name, e, difficulty));

            TR3Entities entity;
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

                        // If the room has water but this enemy isn't a water enemy, we will assume that environment
                        // modifications will handle assignment of the enemy to entities.
                        if (!TR3EntityUtilities.IsWaterCreature(entity) && level.Data.Rooms[targetEntity.Room].ContainsWater)
                        {
                            continue;
                        }

                        // Some enemies need pathing like Willard but we have to honour the entity limit
                        List<Location> paths = TR3EnemyUtilities.GetAIPathing(level.Name, entity, targetEntity.Room);
                        if (ItemFactory.CanCreateItems(level.Name, levelEntities, paths.Count))
                        {
                            targetEntity.TypeID = (short)TR3EntityUtilities.TranslateEntityAlias(entity);

                            // #146 Ensure OneShot triggers are set for this enemy if needed
                            TR3EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);

                            // Remove the target entity from the tracker list so it doesn't get replaced
                            enemyEntities.Remove(targetEntity);

                            // Add the pathing if necessary
                            foreach (Location path in paths)
                            {
                                TR2Entity pathItem = ItemFactory.CreateItem(level.Name, levelEntities, path);
                                pathItem.TypeID = (short)TR3Entities.AIPath_N;
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

                // Ensure any added path entities are written back
                level.Data.Entities = levelEntities.ToArray();
                level.Data.NumEntities = (uint)levelEntities.Count;
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
                    if (level.Data.Entities.ToList().FindAll(e => e.TypeID == (short)newEntityType).Count >= maxEntityCount && enemyPool.Count > maxEntityCount)
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
                    // Crash site raptor spawns need special treatment. The 3 entities in this (unreachable) room
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
                else if (level.Is(TR3LevelNames.RXTECH)
                    && level.IsWillardSequence
                    && Settings.RandoEnemyDifficulty == RandoDifficulty.Default
                    && newEntityType == TR3Entities.RXTechFlameLad
                    && (currentEntity.Room == 14 || currentEntity.Room == 45))
                {
                    // #269 We don't want flamethrowers here because they're hostile, so getting off the minecart
                    // safely is too difficult. We can only change them if there is something else unrestricted available.
                    List<TR3Entities> safePool = enemyPool.FindAll(e => e != TR3Entities.RXTechFlameLad && !TR3EnemyUtilities.IsEnemyRestricted(level.Name, e));
                    if (safePool.Count > 0)
                    {
                        newEntityType = safePool[_generator.Next(0, safePool.Count)];
                    }
                }
                else if (level.Is(TR3LevelNames.HSC))
                {
                    if (currentEntity.Room == 87 && newEntityType != TR3Entities.Prisoner)
                    {
                        // #271 The prisoner is needed here to activate the heavy trigger for the trapdoor. If we still have
                        // prisoners in the pool, ensure one is chosen. If this isn't the case, environment rando will provide
                        // a workaround.
                        if (enemies.Available.Contains(TR3Entities.Prisoner))
                        {
                            newEntityType = TR3Entities.Prisoner;
                        }
                    }
                    else if (currentEntity.Room == 78 && newEntityType == TR3Entities.Monkey)
                    {
                        // #286 Monkeys cannot share AI Ambush spots largely, but these are needed here to ensure the enemies
                        // come through the gate before the timer closes them again. Just ensure no monkeys are here.
                        List<TR3Entities> safePool = enemyPool.FindAll(e => e != TR3Entities.Monkey && !TR3EnemyUtilities.IsEnemyRestricted(level.Name, e));
                        if (safePool.Count > 0)
                        {
                            newEntityType = safePool[_generator.Next(0, safePool.Count)];
                        }
                        else
                        {
                            // Full monkey mode means we have to move them inside the gate
                            currentEntity.Z -= 4096;
                        }
                    }
                }
                else if (level.Is(TR3LevelNames.THAMES) && (currentEntity.Room == 61 || currentEntity.Room == 62) && newEntityType == TR3Entities.Monkey)
                {
                    // #286 Move the monkeys away from the AI entities
                    currentEntity.Z -= 1024;
                }
                
                // Make sure to convert back to the actual type
                targetEntity.TypeID = (short)TR3EntityUtilities.TranslateEntityAlias(newEntityType);

                // #146 Ensure OneShot triggers are set for this enemy if needed
                TR3EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);

                // #291 Cobras don't seem to come back into reality when the
                // engine disables them when too many enemies are active, unless
                // invisible is false.
                if (targetEntity.TypeID == (short)TR3Entities.Cobra)
                {
                    targetEntity.Invisible = false;
                }

                // Track every enemy type across the game
                _resultantEnemies.Add(newEntityType);
            }

            // Add extra ammo based on this level's difficulty
            if (Settings.CrossLevelEnemies && level.Script.RemovesWeapons)
            {
                AddUnarmedLevelAmmo(level);
            }
        }

        private void AddUnarmedLevelAmmo(TR3CombinedLevel level)
        {
            // Find out which gun we have for this level
            List<TR2Entity> levelEntities = level.Data.Entities.ToList();
            List<TR3Entities> weaponTypes = TR3EntityUtilities.GetWeaponPickups();
            List<TR2Entity> levelWeapons = levelEntities.FindAll(e => weaponTypes.Contains((TR3Entities)e.TypeID));
            TR2Entity weaponEntity = null;
            foreach (TR2Entity weapon in levelWeapons)
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

            List<TR3Entities> allEnemies = TR3EntityUtilities.GetFullListOfEnemies();
            List<TR2Entity> levelEnemies = levelEntities.FindAll(e => allEnemies.Contains((TR3Entities)e.TypeID));
            EnemyDifficulty difficulty = TR3EnemyUtilities.GetEnemyDifficulty(levelEnemies);

            if (difficulty > EnemyDifficulty.Easy)
            {
                while (weaponEntity.TypeID == (short)TR3Entities.Pistols_P)
                {
                    weaponEntity.TypeID = (short)weaponTypes[_generator.Next(0, weaponTypes.Count)];
                }
            }

            TR3Entities weaponType = (TR3Entities)weaponEntity.TypeID;
            uint ammoToGive = TR3EnemyUtilities.GetStartingAmmo(weaponType);
            if (ammoToGive > 0)
            {
                ammoToGive *= (uint)difficulty;
                TR3Entities ammoType = TR3EntityUtilities.GetWeaponAmmo(weaponType);
                level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(ammoType), ammoToGive);

                uint smallMediToGive = 0;
                uint largeMediToGive = 0;

                if (difficulty == EnemyDifficulty.Medium || difficulty == EnemyDifficulty.Hard)
                {
                    smallMediToGive++;
                }
                if (difficulty > EnemyDifficulty.Medium)
                {
                    largeMediToGive++;
                }
                if (difficulty == EnemyDifficulty.VeryHard)
                {
                    largeMediToGive++;
                }

                level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR3Entities.SmallMed_P), smallMediToGive);
                level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR3Entities.LargeMed_P), largeMediToGive);
            }

            // Add the pistols as a pickup if the level is hard and there aren't any other pistols around
            if (difficulty > EnemyDifficulty.Medium && levelWeapons.Find(e => e.TypeID == (short)TR3Entities.Pistols_P) == null && ItemFactory.CanCreateItem(level.Name, levelEntities))
            {
                TR2Entity pistols = ItemFactory.CreateItem(level.Name, levelEntities);
                pistols.TypeID = (short)TR3Entities.Pistols_P;
                pistols.X = weaponEntity.X;
                pistols.Y = weaponEntity.Y;
                pistols.Z = weaponEntity.Z;
                pistols.Room = weaponEntity.Room;

                level.Data.Entities = levelEntities.ToArray();
                level.Data.NumEntities++;
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
                            TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.EntitiesToImport)
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