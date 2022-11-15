using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Sound;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers
{
    public class TR1EnemyRandomizer : BaseTR1Randomizer
    {
        private Dictionary<TREntities, List<string>> _gameEnemyTracker;
        private Dictionary<string, List<Location>> _pistolLocations;
        private Dictionary<string, List<Location>> _eggLocations;
        private List<TREntities> _excludedEnemies;
        private ISet<TREntities> _resultantEnemies;

        internal TR1TextureMonitorBroker TextureMonitor { get; set; }
        public ItemFactory ItemFactory { get; set; }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\unarmed_locations.json"));
            _eggLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\egg_locations.json"));

            if (Settings.CrossLevelEnemies)
            {
                RandomizeEnemiesCrossLevel();
            }
            else
            {
                RandomizeExistingEnemies();
            }

            if (ScriptEditor.Edition.IsCommunityPatch)
            {
                (ScriptEditor.Script as TR1Script).DisableTrexCollision = true;
            }
        }

        private void RandomizeExistingEnemies()
        {
            _excludedEnemies = new List<TREntities>();
            _resultantEnemies = new HashSet<TREntities>();

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

            List<EnemyProcessor> processors = new List<EnemyProcessor>();
            for (int i = 0; i < _maxThreads; i++)
            {
                processors.Add(new EnemyProcessor(this));
            }

            List<TR1CombinedLevel> levels = new List<TR1CombinedLevel>(Levels.Count);
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
            _gameEnemyTracker = TR1EnemyUtilities.PrepareEnemyGameTracker(Settings.RandoEnemyDifficulty);

            // #272 Selective enemy pool - convert the shorts in the settings to actual entity types
            _excludedEnemies = Settings.UseEnemyExclusions ?
                Settings.ExcludedEnemies.Select(s => (TREntities)s).ToList() :
                new List<TREntities>();
            _resultantEnemies = new HashSet<TREntities>();

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
            List<TREntities> failedExclusions = _resultantEnemies.ToList().FindAll(_excludedEnemies.Contains);
            if (failedExclusions.Count > 0)
            {
                // A little formatting
                List<string> failureNames = new List<string>();
                foreach (TREntities entity in failedExclusions)
                {
                    failureNames.Add(Settings.ExcludableEnemies[(short)entity]);
                }
                failureNames.Sort();
                SetWarning(string.Format("The following enemies could not be excluded entirely from the randomization pool.{0}{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, failureNames)));
            }
        }

        private EnemyTransportCollection SelectCrossLevelEnemies(TR1CombinedLevel level)
        {
            // For the assault course, nothing will be imported for the time being
            if (level.IsAssault)
            {
                return null;
            }

            // If level-ending Larson is disabled, we make an alternative ending to ToQ.
            // Do this at this stage as it effectively gets rid of ToQ-Larson meaning
            // Sanctuary-Larson can potentially be imported.
            if (level.Is(TRLevelNames.QUALOPEC) && Settings.RemoveLevelEndingLarson)
            {
                AmendToQLarson(level);
            }

            RandoDifficulty difficulty = GetImpliedDifficulty();

            // Get the list of enemy types currently in the level
            List<TREntities> oldEntities = GetCurrentEnemyEntities(level);

            // Get the list of canidadates
            List<TREntities> allEnemies = TR1EntityUtilities.GetCandidateCrossLevelEnemies();

            // Work out how many we can support
            int enemyCount = oldEntities.Count + TR1EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
            if (level.Is(TRLevelNames.QUALOPEC) && Settings.RemoveLevelEndingLarson)
            {
                // Account for Larson having been removed above.
                ++enemyCount;
            }
            List<TREntities> newEntities = new List<TREntities>(enemyCount);

            // Do we need at least one water creature?
            bool waterEnemyRequired = TR1EntityUtilities.GetWaterEnemies().Any(e => oldEntities.Contains(e));

            // Let's try to populate the list. Start by adding a water enemy if needed.
            if (waterEnemyRequired)
            {
                List<TREntities> waterEnemies = TR1EntityUtilities.GetWaterEnemies();
                newEntities.Add(SelectRequiredEnemy(waterEnemies, level, difficulty));
            }

            // Are there any other types we need to retain?
            foreach (TREntities entity in TR1EnemyUtilities.GetRequiredEnemies(level.Name))
            {
                if (level.Is(TRLevelNames.QUALOPEC) && entity == TREntities.Larson && Settings.RemoveLevelEndingLarson)
                {
                    // Only required if normal Larson behaviour is retained in this level.
                    continue;
                }
                if (!newEntities.Contains(entity))
                {
                    newEntities.Add(entity);
                }
            }

            // Remove all exclusions from the pool, and adjust the target capacity
            allEnemies.RemoveAll(e => _excludedEnemies.Contains(e));

            IEnumerable<TREntities> ex = allEnemies.Where(e => !newEntities.Any(TR1EntityUtilities.GetEntityFamily(e).Contains));
            List<TREntities> unalisedEntities = TR1EntityUtilities.RemoveAliases(ex);
            while (unalisedEntities.Count < newEntities.Capacity - newEntities.Count)
            {
                --newEntities.Capacity;
            }

            // Fill the list from the remaining candidates. Keep track of ones tested to avoid
            // looping infinitely if it's not possible to fill to capacity
            ISet<TREntities> testedEntities = new HashSet<TREntities>();
            List<TREntities> eggEntities = TR1EntityUtilities.GetAtlanteanEggEnemies();
            while (newEntities.Count < newEntities.Capacity && testedEntities.Count < allEnemies.Count)
            {
                TREntities entity = allEnemies[_generator.Next(0, allEnemies.Count)];
                testedEntities.Add(entity);

                // Make sure this isn't known to be unsupported in the level
                if (!TR1EnemyUtilities.IsEnemySupported(level.Name, entity, difficulty))
                {
                    continue;
                }

                // Atlanteans and mummies are complex creatures. Grounded ones require the flyer for meshes
                // so we can't have a grounded mummy and meaty flyer, or vice versa as a result.
                if (entity == TREntities.BandagedAtlantean && newEntities.Contains(TREntities.MeatyFlyer) && !newEntities.Contains(TREntities.MeatyAtlantean))
                {
                    entity = TREntities.MeatyAtlantean;
                }
                else if (entity == TREntities.MeatyAtlantean && newEntities.Contains(TREntities.BandagedFlyer) && !newEntities.Contains(TREntities.BandagedAtlantean))
                {
                    entity = TREntities.BandagedAtlantean;
                }
                else if (entity == TREntities.BandagedFlyer && newEntities.Contains(TREntities.MeatyAtlantean))
                {
                    continue;
                }
                else if (entity == TREntities.MeatyFlyer && newEntities.Contains(TREntities.BandagedAtlantean))
                {
                    continue;
                }
                else if (entity == TREntities.AtlanteanEgg && !newEntities.Any(eggEntities.Contains))
                {
                    // Try to pick a type in the inclusion list if possible
                    List<TREntities> preferredEggTypes = eggEntities.FindAll(allEnemies.Contains);
                    if (preferredEggTypes.Count == 0)
                    {
                        preferredEggTypes = eggEntities;
                    }
                    TREntities eggType = preferredEggTypes[_generator.Next(0, preferredEggTypes.Count)];
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
                List<TREntities> family = TR1EntityUtilities.GetEntityFamily(entity);
                if (!newEntities.Any(e1 => family.Any(e2 => e1 == e2)))
                {
                    newEntities.Add(entity);
                }
            }

            if
            (
                newEntities.All(e => TR1EntityUtilities.IsWaterCreature(e) || TR1EnemyUtilities.IsEnemyRestricted(level.Name, e, difficulty)) || 
                (newEntities.Capacity > 1 && newEntities.All(e => TR1EnemyUtilities.IsEnemyRestricted(level.Name, e, difficulty)))
            )
            {
                // Make sure we have an unrestricted enemy available for the individual level conditions. This will
                // guarantee a "safe" enemy for the level; we avoid aliases here to avoid further complication.
                bool RestrictionCheck(TREntities e) =>
                    !TR1EnemyUtilities.IsEnemySupported(level.Name, e, difficulty)
                    || newEntities.Contains(e)
                    || TR1EntityUtilities.IsWaterCreature(e)
                    || TR1EnemyUtilities.IsEnemyRestricted(level.Name, e, difficulty)
                    || TR1EntityUtilities.TranslateEntityAlias(e) != e;

                List<TREntities> unrestrictedPool = allEnemies.FindAll(e => !RestrictionCheck(e));
                if (unrestrictedPool.Count == 0)
                {
                    // We are going to have to pull in the full list of candidates again, so ignoring any exclusions
                    unrestrictedPool = TR1EntityUtilities.GetCandidateCrossLevelEnemies().FindAll(e => !RestrictionCheck(e));
                }

                TREntities entity = unrestrictedPool[_generator.Next(0, unrestrictedPool.Count)];
                newEntities.Add(entity);

                if (entity == TREntities.AtlanteanEgg && !newEntities.Any(eggEntities.Contains))
                {
                    // Try to pick a type in the inclusion list if possible
                    List<TREntities> preferredEggTypes = eggEntities.FindAll(allEnemies.Contains);
                    if (preferredEggTypes.Count == 0)
                    {
                        preferredEggTypes = eggEntities;
                    }
                    TREntities eggType = preferredEggTypes[_generator.Next(0, preferredEggTypes.Count)];
                    newEntities.Add(eggType);
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

        private List<TREntities> GetCurrentEnemyEntities(TR1CombinedLevel level)
        {
            List<TREntities> allGameEnemies = TR1EntityUtilities.GetFullListOfEnemies();
            ISet<TREntities> allLevelEnts = new SortedSet<TREntities>();
            level.Data.Entities.ToList().ForEach(e => allLevelEnts.Add((TREntities)e.TypeID));
            List<TREntities> oldEntities = allLevelEnts.ToList().FindAll(e => allGameEnemies.Contains(e));
            return oldEntities;
        }

        private TREntities SelectRequiredEnemy(List<TREntities> pool, TR1CombinedLevel level, RandoDifficulty difficulty)
        {
            pool.RemoveAll(e => !TR1EnemyUtilities.IsEnemySupported(level.Name, e, difficulty));

            TREntities entity;
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
                List<TREntities> includedEnemies = Settings.ExcludableEnemies.Keys.Except(Settings.ExcludedEnemies).Select(s => (TREntities)s).ToList();
                foreach (TR1ScriptedLevel level in Levels)
                {
                    IEnumerable<TREntities> restrictedRoomEnemies = TR1EnemyUtilities.GetRestrictedEnemyRooms(level.LevelFileBaseName.ToUpper(), RandoDifficulty.Default).Keys;
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

            List<TREntities> availableEnemyTypes = GetCurrentEnemyEntities(level);
            List<TREntities> waterEnemies = TR1EntityUtilities.FilterWaterEnemies(availableEnemyTypes);

            RandomizeEnemies(level, new EnemyRandomizationCollection
            {
                Available = availableEnemyTypes,
                Water = waterEnemies
            });
        }

        private void RandomizeEnemies(TR1CombinedLevel level, EnemyRandomizationCollection enemies)
        {
            AmendAtlanteanModels(level, enemies);

            // Get a list of current enemy entities
            List<TREntities> allEnemies = TR1EntityUtilities.GetFullListOfEnemies();
            List<TREntity> levelEntities = level.Data.Entities.ToList();
            List<TREntity> enemyEntities = levelEntities.FindAll(e => allEnemies.Contains((TREntities)e.TypeID));

            RandoDifficulty difficulty = GetImpliedDifficulty();

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            // First iterate through any enemies that are restricted by room
            Dictionary<TREntities, List<int>> enemyRooms = TR1EnemyUtilities.GetRestrictedEnemyRooms(level.Name, difficulty);
            if (enemyRooms != null)
            {
                foreach (TREntities entity in enemyRooms.Keys)
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
                        TREntity targetEntity = null;
                        do
                        {
                            int room = enemyRooms[entity][_generator.Next(0, enemyRooms[entity].Count)];
                            targetEntity = enemyEntities.Find(e => e.Room == room);
                        }
                        while (targetEntity == null);

                        // If the room has water but this enemy isn't a water enemy, we will assume that environment
                        // modifications will handle assignment of the enemy to entities.
                        if (!TR1EntityUtilities.IsWaterCreature(entity) && level.Data.Rooms[targetEntity.Room].ContainsWater)
                        {
                            continue;
                        }

                        targetEntity.TypeID = (short)TR1EntityUtilities.TranslateEntityAlias(entity);

                        // #146 Ensure OneShot triggers are set for this enemy if needed
                        TR1EnemyUtilities.SetEntityTriggers(level.Data, targetEntity);

                        if (Settings.HideEnemiesUntilTriggered || entity == TREntities.Adam)
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

            foreach (TREntity currentEntity in enemyEntities)
            {
                TREntities currentEntityType = (TREntities)currentEntity.TypeID;
                TREntities newEntityType = currentEntityType;

                // If it's an existing enemy that has to remain in the same spot, skip it
                if (TR1EnemyUtilities.IsEnemyRequired(level.Name, currentEntityType))
                {
                    _resultantEnemies.Add(currentEntityType);
                    continue;
                }

                List<TREntities> enemyPool;
                if (IsEnemyInOrAboveWater(currentEntity, level.Data, floorData))
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
                        List<TREntities> pool = enemyPool.FindAll(e => !TR1EnemyUtilities.IsEnemyRestricted(level.Name, TR1EntityUtilities.TranslateEntityAlias(e)));
                        if (pool.Count > 0)
                        {
                            newEntityType = pool[_generator.Next(0, pool.Count)];
                        }
                    }
                }

                // Rather than individual enemy limits, this accounts for enemy groups such as all Atlanteans
                RandoDifficulty groupDifficulty = difficulty;
                if (level.Is(TRLevelNames.QUALOPEC) && newEntityType == TREntities.Larson && Settings.RemoveLevelEndingLarson)
                {
                    // Non-level ending Larson is not restricted in ToQ, otherwise we adhere to the normal rules.
                    groupDifficulty = RandoDifficulty.NoRestrictions;
                }
                RestrictedEnemyGroup enemyGroup = TR1EnemyUtilities.GetRestrictedEnemyGroup(level.Name, TR1EntityUtilities.TranslateEntityAlias(newEntityType), groupDifficulty);
                if (enemyGroup != null)
                {
                    if (level.Data.Entities.ToList().FindAll(e => enemyGroup.Enemies.Contains((TREntities)e.TypeID)).Count >= enemyGroup.MaximumCount)
                    {
                        List<TREntities> pool = enemyPool.FindAll(e => !TR1EnemyUtilities.IsEnemyRestricted(level.Name, TR1EntityUtilities.TranslateEntityAlias(e), groupDifficulty));
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
                    if (currentRoom.AlternateRoom != -1 && level.Data.Rooms[currentRoom.AlternateRoom].ContainsWater && TR1EntityUtilities.IsWaterLandCreatureEquivalent(currentEntityType) && !TR1EntityUtilities.IsWaterLandCreatureEquivalent(newEntityType))
                    {
                        Dictionary<TREntities, TREntities> hybrids = TR1EntityUtilities.GetWaterEnemyLandCreatures();
                        List<TREntities> pool = enemies.Available.FindAll(e => hybrids.ContainsKey(e) || hybrids.ContainsValue(e));
                        if (pool.Count > 0)
                        {
                            newEntityType = TR1EntityUtilities.GetWaterEnemyLandCreature(pool[_generator.Next(0, pool.Count)]);
                        }
                    }
                }

                if (Settings.HideEnemiesUntilTriggered)
                {
                    // Default to hiding the enemy - checks below for eggs, ex-eggs, Adam and centaur
                    // statues will override as necessary.
                    currentEntity.Invisible = true;
                }

                if (newEntityType == TREntities.AtlanteanEgg)
                {
                    List<TREntities> allEggTypes = TR1EntityUtilities.GetAtlanteanEggEnemies();
                    List<TREntities> spawnTypes = enemies.Available.FindAll(allEggTypes.Contains);
                    TREntities spawnType = TR1EntityUtilities.TranslateEntityAlias(spawnTypes[_generator.Next(0, spawnTypes.Count)]);

                    int entityIndex = levelEntities.IndexOf(currentEntity);
                    Location eggLocation = _eggLocations[level.Name].Find(l => l.EntityIndex == entityIndex);

                    if (eggLocation != null || currentEntityType == newEntityType)
                    {
                        if (Settings.AllowEmptyEggs)
                        {
                            // Add 1/4 chance of an empty egg, provided at least one spawn model is not available
                            List<TREntities> allModels = new List<TREntities>();
                            foreach (TRModel model in level.Data.Models)
                            {
                                allModels.Add((TREntities)model.ID);
                            }

                            // We can add Adam to make it possible for a dud spawn - he's not normally available for eggs because
                            // of his own restrictions.
                            if (!allModels.Contains(TREntities.Adam))
                            {
                                allEggTypes.Add(TREntities.Adam);
                            }

                            if (!allEggTypes.All(e => allModels.Contains(TR1EntityUtilities.TranslateEntityAlias(e))) && _generator.NextDouble() < 0.25)
                            {
                                do
                                {
                                    spawnType = TR1EntityUtilities.TranslateEntityAlias(allEggTypes[_generator.Next(0, allEggTypes.Count)]);
                                }
                                while (allModels.Contains(spawnType));
                            }
                        }

                        switch (spawnType)
                        {
                            case TREntities.ShootingAtlantean_N:
                                currentEntity.CodeBits = 1;
                                break;
                            case TREntities.Centaur:
                                currentEntity.CodeBits = 2;
                                break;
                            case TREntities.Adam:
                                currentEntity.CodeBits = 4;
                                break;
                            case TREntities.NonShootingAtlantean_N:
                                currentEntity.CodeBits = 8;
                                break;
                            default:
                                currentEntity.CodeBits = 0;
                                break;
                        }

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
                else if (currentEntityType == TREntities.AtlanteanEgg)
                {
                    // Hide what used to be eggs and reset the CodeBits otherwise this can interfere with trigger masks.
                    currentEntity.Invisible = true;
                    currentEntity.CodeBits = 0;
                }

                if (newEntityType == TREntities.CentaurStatue)
                {
                    AdjustCentaurStatue(currentEntity, level.Data, floorData);
                }
                else if (newEntityType == TREntities.Adam)
                {
                    // Adam should always be invisible as he is inactive high above the ground
                    // so this can interfere with Lara's route - see Cistern item 36
                    currentEntity.Invisible = true;
                }

                if (newEntityType == TREntities.Pierre)
                {
                    // Pierre will always be OneShot (see SetEntityTriggers) for the time being, because placement
                    // of runaway Pierres is awkward - only one can be active at a time.

                    // He is hard-coded to drop Key1, so add a string if this level doesn't have one.
                    if (ScriptEditor.Edition.IsCommunityPatch && level.Script.Keys.Count == 0)
                    {
                        level.Script.Keys.Add("Pierre's Spare Key");
                    }
                }

                // Make sure to convert back to the actual type
                currentEntity.TypeID = (short)TR1EntityUtilities.TranslateEntityAlias(newEntityType);

                // #146 Ensure OneShot triggers are set for this enemy if needed
                TR1EnemyUtilities.SetEntityTriggers(level.Data, currentEntity);

                // Track every enemy type across the game
                _resultantEnemies.Add(newEntityType);
            }

            if (level.Is(TRLevelNames.TIHOCAN) && level.Data.Entities[82].TypeID != (short)TREntities.Pierre)
            {
                // Add a guaranteed key at the end of the level. Item rando can reposition it.
                List<TREntity> entities = level.Data.Entities.ToList();
                entities.Add(new TREntity
                {
                    TypeID = (short)TREntities.Key1_S_P,
                    X = 30208,
                    Y = 2560,
                    Z = 91648,
                    Room = 110,
                    Intensity = 6144
                });
                level.Data.Entities = entities.ToArray();
                level.Data.NumEntities++;
            }

            // Fix missing OG animation SFX
            FixEnemyAnimations(level);

            // Add extra ammo based on this level's difficulty
            if (Settings.CrossLevelEnemies && ScriptEditor.Edition.IsCommunityPatch && level.Script.RemovesWeapons)
            {
                AddUnarmedLevelAmmo(level);
            }

            if (Settings.SwapEnemyAppearance)
            {
                RandomizeMeshes(level, enemies.Available);
            }
        }

        private int GetEntityCount(TR1CombinedLevel level, TREntities entityType)
        {
            int count = 0;
            TREntities translatedType = TR1EntityUtilities.TranslateEntityAlias(entityType);
            foreach (TREntity entity in level.Data.Entities)
            {
                TREntities type = (TREntities)entity.TypeID;
                if (type == translatedType)
                {
                    count++;
                }
                else if (type == TREntities.AdamEgg || type == TREntities.AtlanteanEgg)
                {
                    TREntities eggType;
                    switch (entity.CodeBits)
                    {
                        case 1:
                            eggType = TREntities.ShootingAtlantean_N;
                            break;
                        case 2:
                            eggType = TREntities.Centaur;
                            break;
                        case 4:
                            eggType = TREntities.Adam;
                            break;
                        case 8:
                            eggType = TREntities.NonShootingAtlantean_N;
                            break;
                        default:
                            eggType = TREntities.FlyingAtlantean;
                            break;
                    }

                    if (eggType == translatedType && Array.Find(level.Data.Models, m => m.ID == (uint)eggType) != null)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private bool IsEnemyInOrAboveWater(TREntity entity, TRLevel level, FDControl floorData)
        {
            if (level.Rooms[entity.Room].ContainsWater)
            {
                return true;
            }

            // Example where we have to search is Midas room 21
            TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y - 256, entity.Z, entity.Room, level, floorData);
            while (sector.RoomBelow != 255)
            {
                if (level.Rooms[sector.RoomBelow].ContainsWater)
                {
                    return true;
                }
                sector = FDUtilities.GetRoomSector(entity.X, (sector.Floor + 1) * 256, entity.Z, sector.RoomBelow, level, floorData);
            }
            return false;
        }

        private void AmendToQLarson(TR1CombinedLevel level)
        {
            TRModel larsonModel = Array.Find(level.Data.Models, m => m.ID == (uint)TREntities.Larson);
            if (larsonModel != null)
            {
                // Convert Larson into a Great Pyramid scion, which is targetable. Environment mods will
                // then add a heavy trigger to end the level. Add an additional enemy beside the scion.
                larsonModel.ID = (uint)TREntities.ScionPiece3_S_P;
                foreach (TREntity entity in Array.FindAll(level.Data.Entities, e => e.TypeID == (short)TREntities.Larson))
                {
                    entity.TypeID = (short)TREntities.ScionPiece3_S_P;
                }

                // Make scion-Larson invisible.
                TRMesh[] larsonMeshes = TRMeshUtilities.GetModelMeshes(level.Data, larsonModel);
                MeshEditor editor = new MeshEditor();
                foreach (TRMesh mesh in larsonMeshes)
                {
                    editor.Mesh = mesh;
                    editor.ClearAllPolygons();
                    editor.WriteToLevel(level.Data);
                }
            }
        }

        private void AmendAtlanteanModels(TR1CombinedLevel level, EnemyRandomizationCollection enemies)
        {
            // If non-shooting grounded Atlanteans are present, we can just duplicate the model to make shooting Atlanteans
            if (enemies.Available.Any(TR1EntityUtilities.GetEntityFamily(TREntities.ShootingAtlantean_N).Contains))
            {
                List<TRModel> models = level.Data.Models.ToList();
                TRModel shooter = models.Find(m => m.ID == (uint)TREntities.ShootingAtlantean_N);
                TRModel nonShooter = models.Find(m => m.ID == (uint)TREntities.NonShootingAtlantean_N);
                if (shooter == null && nonShooter != null)
                {
                    models.Add(new TRModel
                    {
                        ID = (uint)TREntities.ShootingAtlantean_N,
                        Animation = nonShooter.Animation,
                        FrameOffset = nonShooter.FrameOffset,
                        MeshTree = nonShooter.MeshTree,
                        NumMeshes = nonShooter.NumMeshes,
                        StartingMesh = nonShooter.StartingMesh
                    });

                    level.Data.Models = models.ToArray();
                    level.Data.NumModels++;

                    enemies.Available.Add(TREntities.ShootingAtlantean_N);
                }
            }

            // If we're using flying mummies, add a chance that they'll have proper wings
            if (enemies.Available.Contains(TREntities.BandagedFlyer) && _generator.NextDouble() < 0.5)
            {
                TRMesh[] meshes = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.FlyingAtlantean);
                ushort bandageTexture = meshes[1].TexturedRectangles[3].Texture;
                for (int i = 15; i < 21; i++)
                {
                    TRMesh mesh = meshes[i];
                    foreach (TRFace4 f in mesh.TexturedRectangles)
                    {
                        f.Texture = bandageTexture;
                    }
                    foreach (TRFace3 f in mesh.TexturedTriangles)
                    {
                        f.Texture = bandageTexture;
                    }
                }
            }
        }

        private void AdjustCentaurStatue(TREntity entity, TRLevel level, FDControl floorData)
        {
            // If they're floating, they tend not to trigger as Lara's not within range
            TR1LocationGenerator locationGenerator = new TR1LocationGenerator();

            int y = entity.Y;
            short room = entity.Room;
            TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, y, entity.Z, room, level, floorData);
            while (sector.RoomBelow != 255)
            {
                y = (sector.Floor + 1) * 256;
                room = sector.RoomBelow;
                sector = FDUtilities.GetRoomSector(entity.X, y, entity.Z, room, level, floorData);
            }

            entity.Y = sector.Floor * 256;
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
            // Find out which gun we have for this level
            List<TREntity> levelEntities = level.Data.Entities.ToList();
            List<TREntities> weaponTypes = TR1EntityUtilities.GetWeaponPickups();
            List<TREntity> levelWeapons = levelEntities.FindAll(e => weaponTypes.Contains((TREntities)e.TypeID));
            TREntity weaponEntity = null;
            foreach (TREntity weapon in levelWeapons)
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

            List<TREntities> allEnemies = TR1EntityUtilities.GetFullListOfEnemies();
            List<TREntity> levelEnemies = levelEntities.FindAll(e => allEnemies.Contains((TREntities)e.TypeID));
            EnemyDifficulty difficulty = TR1EnemyUtilities.GetEnemyDifficulty(levelEnemies);

            if (difficulty > EnemyDifficulty.Easy)
            {
                while (weaponEntity.TypeID == (short)TREntities.Pistols_S_P)
                {
                    weaponEntity.TypeID = (short)weaponTypes[_generator.Next(0, weaponTypes.Count)];
                }
            }

            TREntities weaponType = (TREntities)weaponEntity.TypeID;
            uint ammoToGive = TR1EnemyUtilities.GetStartingAmmo(weaponType);
            if (ammoToGive > 0)
            {
                ammoToGive *= (uint)difficulty;
                TREntities ammoType = TR1EntityUtilities.GetWeaponAmmo(weaponType);
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

                level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TREntities.SmallMed_S_P), smallMediToGive);
                level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TREntities.LargeMed_S_P), largeMediToGive);
            }

            // Add the pistols as a pickup if the level is hard and there aren't any other pistols around
            if (difficulty > EnemyDifficulty.Medium && levelWeapons.Find(e => e.TypeID == (short)TREntities.Pistols_S_P) == null && ItemFactory.CanCreateItem(level.Name, levelEntities))
            {
                TREntity pistols = ItemFactory.CreateItem(level.Name, levelEntities);
                pistols.TypeID = (short)TREntities.Pistols_S_P;
                pistols.X = weaponEntity.X;
                pistols.Y = weaponEntity.Y;
                pistols.Z = weaponEntity.Z;
                pistols.Room = weaponEntity.Room;

                level.Data.Entities = levelEntities.ToArray();
                level.Data.NumEntities++;
            }
        }

        private void RandomizeMeshes(TR1CombinedLevel level, List<TREntities> availableEnemies)
        {
            if (level.Is(TRLevelNames.ATLANTIS))
            {
                // Atlantis scion swap - Model => Mesh index
                Dictionary<TREntities, int> scionSwaps = new Dictionary<TREntities, int>
                {
                    [TREntities.Lara] = 3,
                    [TREntities.Pistols_M_H] = 1,
                    [TREntities.Shotgun_M_H] = 0,
                    [TREntities.ShotgunAmmo_M_H] = 0,
                    [TREntities.Magnums_M_H] = 1,
                    [TREntities.Uzis_M_H] = 1,
                    [TREntities.Dart_H] = 0,
                    [TREntities.Sunglasses_M_H] = 0,
                    [TREntities.CassettePlayer_M_H] = 1
                };

                TRMesh[] scion = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.ScionPiece4_S_P);
                List<TREntities> replacementKeys = scionSwaps.Keys.ToList();
                TREntities replacement = replacementKeys[_generator.Next(0, replacementKeys.Count)];

                TRMesh[] replacementMeshes = TRMeshUtilities.GetModelMeshes(level.Data, replacement);
                int colRadius = scion[0].CollRadius;
                TRMeshUtilities.DuplicateMesh(level.Data, scion[0], replacementMeshes[scionSwaps[replacement]]);
                scion[0].CollRadius = colRadius; // Retain original as Lara may need to shoot it

                // Cutscene head swaps
                TRMesh[] lara = TRMeshUtilities.GetModelMeshes(level.CutSceneLevel.Data, TREntities.CutsceneActor1);
                TRMesh[] natla = TRMeshUtilities.GetModelMeshes(level.CutSceneLevel.Data, TREntities.CutsceneActor3);
                TRMesh[] pierre = TRMeshUtilities.GetModelMeshes(level.CutSceneLevel.Data, TREntities.Pierre);

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
                        MeshEditor editor = new MeshEditor();
                        TRMesh laraHead = editor.CloneMesh(lara[14]);
                        TRMesh natlaHead = editor.CloneMesh(natla[8]);
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

            if ((level.Is(TRLevelNames.PYRAMID) || availableEnemies.Contains(TREntities.Adam)) && _generator.NextDouble() < 0.4)
            {
                // Replace Adam's head with a much larger version of Natla's, Larson's or normal/angry Lara's.
                MeshEditor editor = new MeshEditor();
                TRMesh[] adam = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.Adam);
                TRMesh replacement;
                if (availableEnemies.Contains(TREntities.Natla) && _generator.NextDouble() < 0.5)
                {
                    replacement = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.Natla)[2];
                }
                else if (availableEnemies.Contains(TREntities.Larson) && _generator.NextDouble() < 0.5)
                {
                    replacement = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.Larson)[8];
                }
                else if (availableEnemies.Contains(TREntities.Pierre) && _generator.NextDouble() < 0.5)
                {
                    replacement = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.Pierre)[8];
                }
                else
                {
                    replacement = TRMeshUtilities.GetModelMeshes(level.Data, _generator.NextDouble() < 0.5 ? TREntities.LaraUziAnimation_H : TREntities.Lara)[14];
                }

                TRMeshUtilities.DuplicateMesh(level.Data, adam[3], editor.CloneMesh(replacement));

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
                    foreach (TRFace3 f in adam[i].TexturedTriangles)
                    {
                        f.Texture = adam[0].TexturedTriangles[0].Texture;
                    }
                    foreach (TRFace4 f in adam[i].TexturedRectangles)
                    {
                        f.Texture = adam[0].TexturedRectangles[0].Texture;
                    }
                }
            }

            if (availableEnemies.Contains(TREntities.Pierre) && _generator.NextDouble() < 0.25)
            {
                // Replace Pierre's head with a slightly bigger version of Lara's (either angry Lara or normal Lara)
                MeshEditor editor = new MeshEditor();
                TRMesh[] pierre = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.Pierre);
                TRMesh[] lara = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.Lara);
                TRMesh[] laraUziAnim = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.LaraUziAnimation_H);

                TRMeshUtilities.DuplicateMesh(level.Data, pierre[8], editor.CloneMesh(_generator.NextDouble() < 0.5 ? laraUziAnim[14] : lara[14]));
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
            List<TREntities> entities = level.Data.Models.Select(m => (TREntities)m.ID).ToList();

            if (entities.Contains(TREntities.Pierre)
                && (level.Is(TRLevelNames.FOLLY) || level.Is(TRLevelNames.COLOSSEUM) || level.Is(TRLevelNames.CISTERN) || level.Is(TRLevelNames.TIHOCAN)))
            {
                TR1ModelExporter.AmendPierreGunshot(level.Data);
                TR1ModelExporter.AmendPierreDeath(level.Data);

                // Non one-shot-Pierre levels won't have the death sound by default, so borrow it from ToT.
                if (level.Data.SoundMap[159] == -1)
                {
                    TRLevel tihocan = new TR1LevelReader().ReadLevel(Path.Combine(BackupPath, TRLevelNames.TIHOCAN));
                    SoundUtilities.ImportLevelSound(level.Data, tihocan, new short[] { 159 });
                }
            }

            if (entities.Contains(TREntities.Larson) && level.Is(TRLevelNames.SANCTUARY))
            {
                TR1ModelExporter.AmendLarsonDeath(level.Data);
            }

            if (entities.Contains(TREntities.SkateboardKid) && level.Is(TRLevelNames.MINES))
            {
                TR1ModelExporter.AmendSkaterBoyDeath(level.Data);
            }

            if (entities.Contains(TREntities.Natla) && level.Is(TRLevelNames.PYRAMID))
            {
                TR1ModelExporter.AmendNatlaDeath(level.Data);
            }
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
                List<TR1CombinedLevel> levels = new List<TR1CombinedLevel>(_enemyMapping.Keys);
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
                        List<TREntities> importModels = new List<TREntities>(enemies.EntitiesToImport);
                        if (level.Is(TRLevelNames.KHAMOON) && (importModels.Contains(TREntities.BandagedAtlantean) || importModels.Contains(TREntities.BandagedFlyer)))
                        {
                            // Mummies may become shooters in Khamoon, but the missiles won't be available by default, so ensure they do get imported.
                            importModels.Add(TREntities.Missile2_H);
                            importModels.Add(TREntities.Missile3_H);
                        }

                        TR1ModelImporter importer = new TR1ModelImporter(_outer.ScriptEditor.Edition.IsCommunityPatch)
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
                        EnemyRandomizationCollection enemies = new EnemyRandomizationCollection
                        {
                            Available = _enemyMapping[level].EntitiesToImport,
                            Water = TR1EntityUtilities.FilterWaterEnemies(_enemyMapping[level].EntitiesToImport)
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
            internal List<TREntities> EntitiesToImport { get; set; }
            internal List<TREntities> EntitiesToRemove { get; set; }
        }

        internal class EnemyRandomizationCollection
        {
            internal List<TREntities> Available { get; set; }
            internal List<TREntities> Water { get; set; }
        }
    }
}