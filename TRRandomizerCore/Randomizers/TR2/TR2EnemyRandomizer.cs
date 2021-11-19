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
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;

namespace TRRandomizerCore.Randomizers
{
    public class TR2EnemyRandomizer : BaseTR2Randomizer
    {
        private Dictionary<TR2Entities, List<string>> _gameEnemyTracker;

        internal int MaxPackingAttempts { get; set; }
        internal TexturePositionMonitorBroker TextureMonitor { get; set; }

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

            List<EnemyProcessor> processors = new List<EnemyProcessor>();
            for (int i = 0; i < _maxThreads; i++)
            {
                processors.Add(new EnemyProcessor(this));
            }

            List<TR2CombinedLevel> levels = new List<TR2CombinedLevel>(Levels.Count);
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
            _gameEnemyTracker = TR2EnemyUtilities.PrepareEnemyGameTracker(Settings.DocileBirdMonsters, Settings.RandoEnemyDifficulty);
            
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

        private EnemyTransportCollection SelectCrossLevelEnemies(TR2CombinedLevel level, int reduceEnemyCountBy = 0)
        {
            // For the assault course, nothing will be imported for the time being
            if (level.IsAssault)
            {
                return null;
            }

            // Get the list of enemy types currently in the level
            List<TR2Entities> oldEntities = TR2EntityUtilities.GetEnemyTypeDictionary()[level.Name];

            // Work out how many we can support
            int enemyCount = oldEntities.Count - reduceEnemyCountBy + TR2EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
            List<TR2Entities> newEntities = new List<TR2Entities>(enemyCount);

            List<TR2Entities> chickenGuisers = TR2EnemyUtilities.GetEnemyGuisers(TR2Entities.BirdMonster);
            TR2Entities chickenGuiser = TR2Entities.BirdMonster;

            // #148 For HSH, we lock the enemies that are required for the kill counter to work outside
            // the gate, which means the game still has the correct target kill count, while allowing
            // us to randomize the ones inside the gate (except the final shotgun goon).
            // If however, we are on the final packing attempt, we will just change the stick goon
            // alias and add docile bird monsters (if selected) as this is known to be supported.
            if (level.Is(TR2LevelNames.HOME) && reduceEnemyCountBy > 0)
            {
                TR2Entities newGoon = TR2Entities.StickWieldingGoon1BlackJacket;
                List<TR2Entities> goonies = TR2EntityUtilities.GetEntityFamily(newGoon);
                do
                {
                    newGoon = goonies[_generator.Next(0, goonies.Count)];
                }
                while (newGoon == TR2Entities.StickWieldingGoon1BlackJacket);

                newEntities.AddRange(oldEntities);
                newEntities.Remove(TR2Entities.StickWieldingGoon1);
                newEntities.Add(newGoon);

                if (Settings.DocileBirdMonsters)
                {
                    newEntities.Remove(TR2Entities.MaskedGoon1);
                    newEntities.Add(TR2Entities.BirdMonster);
                    chickenGuiser = TR2Entities.MaskedGoon1;
                }
            }
            else
            {
                // Do we need at least one water creature?
                bool waterEnemyRequired = TR2EnemyUtilities.IsWaterEnemyRequired(level);
                // Do we need at least one enemy that can drop?
                bool droppableEnemyRequired = TR2EnemyUtilities.IsDroppableEnemyRequired(level);

                // Let's try to populate the list. Start by adding one water enemy
                // and one droppable enemy if they are needed.
                if (waterEnemyRequired)
                {
                    List<TR2Entities> waterEnemies = TR2EntityUtilities.KillableWaterCreatures();
                    TR2Entities entity;
                    do
                    {
                        entity = waterEnemies[_generator.Next(0, waterEnemies.Count)];
                    }
                    while (!TR2EnemyUtilities.IsEnemySupported(level.Name, entity, Settings.RandoEnemyDifficulty));
                    newEntities.Add(entity);
                }

                if (droppableEnemyRequired)
                {
                    List<TR2Entities> droppableEnemies = TR2EntityUtilities.GetCrossLevelDroppableEnemies(!Settings.ProtectMonks);
                    TR2Entities entity;
                    do
                    {
                        entity = droppableEnemies[_generator.Next(0, droppableEnemies.Count)];
                    }
                    while (!TR2EnemyUtilities.IsEnemySupported(level.Name, entity, Settings.RandoEnemyDifficulty));
                    newEntities.Add(entity);
                }

                // Are there any other types we need to retain?
                foreach (TR2Entities entity in TR2EnemyUtilities.GetRequiredEnemies(level.Name))
                {
                    if (!newEntities.Contains(entity))
                    {
                        newEntities.Add(entity);
                    }
                }

                // Get all other candidate enemies and fill the list
                List<TR2Entities> allEnemies = TR2EntityUtilities.GetCandidateCrossLevelEnemies();

                while (newEntities.Count < newEntities.Capacity)
                {
                    TR2Entities entity = allEnemies[_generator.Next(0, allEnemies.Count)];

                    // Make sure this isn't known to be unsupported in the level
                    if (!TR2EnemyUtilities.IsEnemySupported(level.Name, entity, Settings.RandoEnemyDifficulty))
                    {
                        continue;
                    }

                    // If it's the chicken in HSH but we're not using docile, we don't want it ending the level
                    if (!Settings.DocileBirdMonsters && entity == TR2Entities.BirdMonster && level.Is(TR2LevelNames.HOME))
                    {
                        continue;
                    }

                    // If it's a docile chicken in Barkhang, it won't work because we can't disguise monks in this level.
                    if (Settings.DocileBirdMonsters && entity == TR2Entities.BirdMonster && level.Is(TR2LevelNames.MONASTERY))
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

                    // GetEntityFamily returns all aliases for the likes of the tigers, but if an entity
                    // doesn't have any, the returned list just contains the entity itself. This means
                    // we can avoid duplicating standard enemies as well as avoiding alias-clashing.
                    List<TR2Entities> family = TR2EntityUtilities.GetEntityFamily(entity);
                    if (!newEntities.Any(e1 => family.Any(e2 => e1 == e2)))
                    {
                        // #144 We can include docile chickens provided we aren't including everything
                        // that can be disguised as a chicken.
                        if (Settings.DocileBirdMonsters)
                        {
                            bool guisersAvailable = !chickenGuisers.All(g => newEntities.Contains(g));
                            // If the selected entity is the chicken, it can be added provided there are
                            // available guisers.
                            if (!guisersAvailable && entity == TR2Entities.BirdMonster)
                            {
                                continue;
                            }

                            // If the selected entity is a potential guiser, it can only be added if it's not
                            // the last available guiser. Otherwise, it will become the guiser.
                            if (chickenGuisers.Contains(entity) && newEntities.Contains(TR2Entities.BirdMonster))
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

            // #144 Decide at this point who will be guising unless it has already been decided above (e.g. HSH)          
            if (Settings.DocileBirdMonsters && newEntities.Contains(TR2Entities.BirdMonster) && chickenGuiser == TR2Entities.BirdMonster)
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

        private void RandomizeEnemiesNatively(TR2CombinedLevel level)
        {
            // For the assault course, nothing will be changed for the time being
            if (level.IsAssault)
            {
                return;
            }

            List<TR2Entities> availableEnemyTypes = TR2EntityUtilities.GetEnemyTypeDictionary()[level.Name];
            List<TR2Entities> droppableEnemies = TR2EntityUtilities.DroppableEnemyTypes()[level.Name];
            List<TR2Entities> waterEnemies = TR2EntityUtilities.FilterWaterEnemies(availableEnemyTypes);

            if (Settings.DocileBirdMonsters && level.Is(TR2LevelNames.CHICKEN))
            {
                DisguiseEntity(level, TR2Entities.MaskedGoon1, TR2Entities.BirdMonster);
            }

            RandomizeEnemies(level, new EnemyRandomizationCollection
            {
                Available = availableEnemyTypes,
                Droppable = droppableEnemies,
                Water = waterEnemies,
                BirdMonsterGuiser = TR2Entities.MaskedGoon1 // If randomizing natively, this will only apply to Ice Palace
            });
        }

        private void DisguiseEntity(TR2CombinedLevel level, TR2Entities guiser, TR2Entities targetEntity)
        {
            List<TRModel> models = level.Data.Models.ToList();
            int existingIndex = models.FindIndex(m => m.ID == (short)guiser);
            if (existingIndex != -1)
            {
                models.RemoveAt(existingIndex);
            }

            TRModel disguiseAsModel = models[models.FindIndex(m => m.ID == (short)targetEntity)];
            if (targetEntity == TR2Entities.BirdMonster && level.Is(TR2LevelNames.CHICKEN))
            {
                // We have to keep the original model for the boss, so in
                // this instance we just clone the model for the guiser
                models.Add(new TRModel
                {
                    Animation = disguiseAsModel.Animation,
                    FrameOffset = disguiseAsModel.FrameOffset,
                    ID = (uint)guiser,
                    MeshTree = disguiseAsModel.MeshTree,
                    NumMeshes = disguiseAsModel.NumMeshes,
                    StartingMesh = disguiseAsModel.StartingMesh
                });
            }
            else
            {
                disguiseAsModel.ID = (uint)guiser;
            }

            level.Data.Models = models.ToArray();
            level.Data.NumModels = (uint)models.Count;
        }

        private void RandomizeEnemies(TR2CombinedLevel level, EnemyRandomizationCollection enemies)
        {
            bool shotgunGoonSeen = level.Is(TR2LevelNames.HOME); // 1 ShotgunGoon in HSH only
            bool dragonSeen = level.Is(TR2LevelNames.LAIR); // 1 Marco in DL only

            // Get a list of current enemy entities
            List<TR2Entity> enemyEntities = level.GetEnemyEntities();

            // Keep track of any new entities added (e.g. Skidoo)
            List<TR2Entity> newEntities = new List<TR2Entity>();

            // #148 If it's HSH and we have been able to import cross-level, we will add 15
            // dogs outside the gate to ensure the kill counter works. Dogs, Goon1 and
            // StickGoons will have been excluded from the cross-level pool for simplicity
            // Their textures will have been removed but they won't spawn anyway as we aren't
            // defining triggers - the game only needs them to be present in the entity list.
            if (level.Is(TR2LevelNames.HOME) && !enemies.Available.Contains(TR2Entities.Doberman))
            {
                for (int i = 0; i < 15; i++)
                {
                    newEntities.Add(new TR2Entity
                    {
                        TypeID = (short)TR2Entities.Doberman,
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
            Dictionary<TR2Entities, List<int>> enemyRooms = TR2EnemyUtilities.GetRestrictedEnemyRooms(level.Name, Settings.RandoEnemyDifficulty);
            if (enemyRooms != null)
            {
                foreach (TR2Entities entity in enemyRooms.Keys)
                {
                    if (!enemies.Available.Contains(entity))
                    {
                        continue;
                    }

                    List<int> rooms = enemyRooms[entity];
                    int maxEntityCount = TR2EnemyUtilities.GetRestrictedEnemyLevelCount(entity, Settings.RandoEnemyDifficulty);
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

                        targetEntity.TypeID = (short)TR2EntityUtilities.TranslateEntityAlias(entity);

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
                TR2Entities currentEntityType = (TR2Entities)currentEntity.TypeID;
                TR2Entities newEntityType = currentEntityType;

                // If it's an existing enemy that has to remain in the same spot, skip it
                if (TR2EnemyUtilities.IsEnemyRequired(level.Name, currentEntityType))
                {
                    continue;
                }

                //#45 - Check to see if any items are at the same location as the enemy.
                //If there are we need to ensure that the new random enemy type is one that can drop items.
                List<TR2Entity> sharedItems = new List<TR2Entity>(Array.FindAll
                (
                    level.Data.Entities,
                    e =>
                    (
                        e.X == currentEntity.X &&
                        e.Y == currentEntity.Y &&
                        e.Z == currentEntity.Z
                    )
                ));

                //Do multiple entities share one location?
                bool isPickupItem = false;
                if (sharedItems.Count > 1 && enemies.Droppable.Count != 0)
                {
                    //Are any entities sharing a location a droppable pickup?
                    
                    foreach (TR2Entity ent in sharedItems)
                    {
                        TR2Entities entType = (TR2Entities)ent.TypeID;

                        isPickupItem = TR2EntityUtilities.IsUtilityType(entType) ||
                                       TR2EntityUtilities.IsGunType(entType) ||
                                       TR2EntityUtilities.IsKeyItemType(entType);

                        if (isPickupItem)
                            break;
                    }

                    //Generate a new type
                    newEntityType = enemies.Available[_generator.Next(0, enemies.Available.Count)];

                    //Do we need to ensure the enemy can drop the item on the same tile?
                    if (!TR2EntityUtilities.CanDropPickups(newEntityType, !Settings.ProtectMonks) && isPickupItem)
                    {
                        //Ensure the new random entity can drop pickups
                        newEntityType = enemies.Droppable[_generator.Next(0, enemies.Droppable.Count)];
                    }
                }
                else
                {
                    //Generate a new type
                    newEntityType = enemies.Available[_generator.Next(0, enemies.Available.Count)];
                }

                short roomIndex = currentEntity.Room;
                TR2Room room = level.Data.Rooms[roomIndex];

                if (level.Is(TR2LevelNames.DA) && roomIndex == 77)
                {
                    // Make sure the end level trigger isn't blocked by an unkillable enemy
                    while (TR2EntityUtilities.IsHazardCreature(newEntityType) || (Settings.ProtectMonks && TR2EntityUtilities.IsMonk(newEntityType)))
                    {
                        newEntityType = enemies.Available[_generator.Next(0, enemies.Available.Count)];
                    }
                }

                if (TR2EntityUtilities.IsWaterCreature(currentEntityType) && !TR2EntityUtilities.IsWaterCreature(newEntityType))
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
                List<TR2Entities> enemyPool = isPickupItem ? enemies.Droppable : enemies.Available;

                if (newEntityType == TR2Entities.ShotgunGoon && shotgunGoonSeen) // HSH only
                {
                    while (newEntityType == TR2Entities.ShotgunGoon)
                    {
                        newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                    }
                }

                if (newEntityType == TR2Entities.MarcoBartoli && dragonSeen) // DL only, other levels use quasi-zoning for the dragon
                {
                    while (newEntityType == TR2Entities.MarcoBartoli)
                    {
                        newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                    }
                }

                // If we are restricting count per level for this enemy and have reached that count, pick
                // something else. This applies when we are restricting by in-level count, but not by room
                // (e.g. Winston).
                int maxEntityCount = TR2EnemyUtilities.GetRestrictedEnemyLevelCount(newEntityType, Settings.RandoEnemyDifficulty);
                if (maxEntityCount != -1)
                {
                    if (level.Data.Entities.ToList().FindAll(e => e.TypeID == (short)newEntityType).Count >= maxEntityCount)
                    {
                        TR2Entities tmp = newEntityType;
                        while (newEntityType == tmp)
                        {
                            newEntityType = enemyPool[_generator.Next(0, enemyPool.Count)];
                        }
                    }
                }

                // #144 Disguise something as the Chicken. Pre-checks will have been done to ensure
                // the guiser is suitable for the level.
                if (Settings.DocileBirdMonsters && newEntityType == TR2Entities.BirdMonster)
                {
                    newEntityType = enemies.BirdMonsterGuiser;
                }

                // Make sure to convert BengalTiger, StickWieldingGoonBandana etc back to their actual types
                currentEntity.TypeID = (short)TR2EntityUtilities.TranslateEntityAlias(newEntityType);

                // #146 Ensure OneShot triggers are set for this enemy if needed. This currently only applies
                // to the dragon, which will be handled above in defined rooms, but the check should be made
                // here in case this needs to be extended later.
                TR2EnemyUtilities.SetEntityTriggers(level.Data, currentEntity);
            }

            // MercSnowMobDriver relies on RedSnowmobile so it will be available in the model list
            if (!level.Is(TR2LevelNames.TIBET))
            {
                TR2Entity mercDriver = level.Data.Entities.ToList().Find(e => e.TypeID == (short)TR2Entities.MercSnowmobDriver);
                if (mercDriver != null)
                {
                    short room, angle;
                    int x, y, z;

                    // we will only spawn one skidoo, so only need one random location
                    Location randomLocation = VehicleUtilities.GetRandomLocation(level.Name, TR2Entities.RedSnowmobile, _generator);
                    if (randomLocation != null)
                    {
                        room = (short)randomLocation.Room;
                        x = randomLocation.X;
                        y = randomLocation.Y;
                        z = randomLocation.Z;
                        angle = randomLocation.Angle;
                    }
                    else
                    {
                        // if the level does not have skidoo locations for some reason, just spawn it on the MercSnowMobDriver
                        room = mercDriver.Room;
                        x = mercDriver.X;
                        y = mercDriver.Y;
                        z = mercDriver.Z;
                        angle = mercDriver.Angle;
                    }

                    newEntities.Add(new TR2Entity
                    {
                        TypeID = (short)TR2Entities.RedSnowmobile,
                        Room = room,
                        X = x,
                        Y = y,
                        Z = z,
                        Angle = angle,
                        Flags = 0,
                        Intensity1 = -1,
                        Intensity2 = -1
                    });
                }
            }

            // Did we add any new entities?
            if (newEntities.Count > 0)
            {
                List<TR2Entity> levelEntities = level.Data.Entities.ToList();
                levelEntities.AddRange(newEntities);
                level.Data.Entities = levelEntities.ToArray();
                level.Data.NumEntities = (uint)levelEntities.Count;
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
                List<TR2CombinedLevel> levels = new List<TR2CombinedLevel>(_enemyMapping.Keys);
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
                    TR2ModelImporter importer = new TR2ModelImporter
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
                            //System.Diagnostics.Debug.WriteLine(level.Name + ": " + string.Join(", ", importedCollection.EntitiesToImport));
                            EnemyRandomizationCollection enemies = new EnemyRandomizationCollection
                            {
                                Available = importedCollection.EntitiesToImport,
                                Droppable = TR2EntityUtilities.FilterDroppableEnemies(importedCollection.EntitiesToImport, !_outer.Settings.ProtectMonks),
                                Water = TR2EntityUtilities.FilterWaterEnemies(importedCollection.EntitiesToImport)
                            };

                            if (_outer.Settings.DocileBirdMonsters && importedCollection.BirdMonsterGuiser != TR2Entities.BirdMonster)
                            {
                                _outer.DisguiseEntity(level, importedCollection.BirdMonsterGuiser, TR2Entities.BirdMonster);
                                enemies.BirdMonsterGuiser = importedCollection.BirdMonsterGuiser;
                            }

                            _outer.RandomizeEnemies(level, enemies);
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
            internal List<TR2Entities> EntitiesToImport { get; set; }
            internal List<TR2Entities> EntitiesToRemove { get; set; }
            internal TR2Entities BirdMonsterGuiser { get; set; }
            internal bool ImportResult { get; set; }

            internal EnemyTransportCollection()
            {
                ImportResult = false;
            }
        }

        internal class EnemyRandomizationCollection
        {
            internal List<TR2Entities> Available { get; set; }
            internal List<TR2Entities> Droppable { get; set; }
            internal List<TR2Entities> Water { get; set; }
            internal TR2Entities BirdMonsterGuiser { get; set; }
        }
    }
}