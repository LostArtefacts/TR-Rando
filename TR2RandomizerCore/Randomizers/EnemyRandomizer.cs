using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Processors;
using TR2RandomizerCore.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;

namespace TR2RandomizerCore.Randomizers
{
    public class EnemyRandomizer : RandomizerBase
    {
        private Dictionary<TR2Entities, List<string>> _gameEnemyTracker;

        internal bool CrossLevelEnemies { get; set; }
        internal bool ProtectMonks { get; set; }
        internal int MaxPackingAttempts { get; set; }
        internal TexturePositionMonitorBroker TextureMonitor { get; set; }

        public EnemyRandomizer()
        {
            MaxPackingAttempts = 5;
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            if (CrossLevelEnemies)
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
            foreach (TR23ScriptedLevel lvl in Levels)
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
            foreach (TR23ScriptedLevel lvl in Levels)
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
            _gameEnemyTracker = EnemyUtilities.PrepareEnemyGameTracker();
            
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
                throw _processingException;
            }
        }

        private EnemyTransportCollection SelectCrossLevelEnemies(TR2CombinedLevel level, int reduceEnemyCountBy = 0)
        {
            // Get the list of enemy types currently in the level
            List<TR2Entities> oldEntities = TR2EntityUtilities.GetEnemyTypeDictionary()[level.Name];

            // Work out how many we can support
            int enemyCount = oldEntities.Count - reduceEnemyCountBy + EnemyUtilities.GetEnemyAdjustmentCount(level.Name);
            List<TR2Entities> newEntities = new List<TR2Entities>(enemyCount);

            if (level.Is(LevelNames.HOME))
            {
                // In HSH, changing the enemies means the level can potentially end after the first
                // kill. So let's just change the type of StickWieldingGoon1 for now.
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
            }
            else
            {
                // Do we need at least one water creature?
                bool waterEnemyRequired = EnemyUtilities.IsWaterEnemyRequired(level);
                // Do we need at least one enemy that can drop?
                bool droppableEnemyRequired = EnemyUtilities.IsDroppableEnemyRequired(level);

                // Let's try to populate the list. Start by adding one water enemy
                // and one droppable enemy if they are needed.
                if (waterEnemyRequired)
                {
                    List<TR2Entities> waterEnemies = TR2EntityUtilities.KillableWaterCreatures();
                    newEntities.Add(waterEnemies[_generator.Next(0, waterEnemies.Count)]);
                }

                if (droppableEnemyRequired)
                {
                    List<TR2Entities> droppableEnemies = TR2EntityUtilities.GetCrossLevelDroppableEnemies(!ProtectMonks);
                    newEntities.Add(droppableEnemies[_generator.Next(0, droppableEnemies.Count)]);
                }

                // Are there any other types we need to retain?
                foreach (TR2Entities entity in EnemyUtilities.GetRequiredEnemies(level.Name))
                {
                    if (!newEntities.Contains(entity))
                    {
                        newEntities.Add(entity);
                    }
                }

                // Get all other candidate enemies and fill the list
                List<TR2Entities> allEnemies = TR2EntityUtilities.GetCandidateCrossLevelEnemies();
                List<TR2Entities> unsupportedEnemies = EnemyUtilities.GetUnsupportedEnemies(level.Name);

                while (newEntities.Count < newEntities.Capacity)
                {
                    TR2Entities entity = allEnemies[_generator.Next(0, allEnemies.Count)];

                    // Make sure this isn't known to be unsupported in the level
                    if (unsupportedEnemies.Contains(entity))
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
                        newEntities.Add(entity);
                    }
                }
            }

            return new EnemyTransportCollection
            {
                EntitiesToImport = newEntities,
                EntitiesToRemove = oldEntities
            };
        }

        private void RandomizeEnemiesNatively(TR2CombinedLevel level)
        {
            List<TR2Entities> availableEnemyTypes = TR2EntityUtilities.GetEnemyTypeDictionary()[level.Name];
            List<TR2Entities> droppableEnemies = TR2EntityUtilities.DroppableEnemyTypes()[level.Name];
            List<TR2Entities> waterEnemies = TR2EntityUtilities.FilterWaterEnemies(availableEnemyTypes);

            RandomizeEnemies(level, new EnemyRandomizationCollection
            {
                Available = availableEnemyTypes,
                Droppable = droppableEnemies,
                Water = waterEnemies
            });
        }

        private void RandomizeEnemies(TR2CombinedLevel level, EnemyRandomizationCollection enemies)
        {
            bool shotgunGoonSeen = level.Is(LevelNames.HOME); // 1 ShotgunGoon in HSH only
            bool dragonSeen = level.Is(LevelNames.LAIR); // 1 Marco in DL only

            // Get a list of current enemy entities
            List<TR2Entity> enemyEntities = level.GetEnemyEntities();

            // Keep track of any new entities added (e.g. Skidoo)
            List<TR2Entity> newEntities = new List<TR2Entity>();

            // First iterate through any enemies that are restricted by room
            Dictionary<TR2Entities, List<int>> enemyRooms = EnemyUtilities.GetRestrictedEnemyRooms(level.Name);
            if (enemyRooms != null)
            {
                foreach (TR2Entities entity in enemyRooms.Keys)
                {
                    if (!enemies.Available.Contains(entity))
                    {
                        continue;
                    }

                    List<int> rooms = enemyRooms[entity];
                    int maxEntityCount = EnemyUtilities.GetRestrictedEnemyLevelCount(entity);
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
                if (EnemyUtilities.IsEnemyRequired(level.Name, currentEntityType))
                {
                    continue;
                }

                // #136 If it's the monastery and we still have monks, entity ID 167 has to
                // remain either type of monk, otherwise saving at the sack/spindle area
                // and reloading causes entities to freeze. If there are no monks, this
                // doesn't seem to be an issue.
                if (level.Is(LevelNames.MONASTERY) && currentEntity.Room == 99)
                {
                    int monkIndex = enemies.Available.FindIndex(e => TR2EntityUtilities.IsMonk(e));
                    if (monkIndex != -1)
                    {
                        currentEntity.TypeID = (short)enemies.Available[monkIndex];
                        continue;
                    }
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
                if (sharedItems.Count > 1 && enemies.Droppable.Count != 0)
                {
                    //Are any entities sharing a location a droppable pickup?
                    bool isPickupItem = false;

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
                    if (!TR2EntityUtilities.CanDropPickups(newEntityType, !ProtectMonks) && isPickupItem)
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

                if (level.Is(LevelNames.DA) && roomIndex == 77)
                {
                    // Make sure the end level trigger isn't blocked by an unkillable enemy
                    while (TR2EntityUtilities.IsHazardCreature(newEntityType) || (ProtectMonks && TR2EntityUtilities.IsMonk(newEntityType)))
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

                if (newEntityType == TR2Entities.ShotgunGoon && shotgunGoonSeen) // HSH only
                {
                    while (newEntityType == TR2Entities.ShotgunGoon)
                    {
                        newEntityType = enemies.Available[_generator.Next(0, enemies.Available.Count)];
                    }
                }

                if (newEntityType == TR2Entities.MarcoBartoli && dragonSeen) // DL only, other levels use quasi-zoning for the dragon
                {
                    while (newEntityType == TR2Entities.MarcoBartoli)
                    {
                        newEntityType = enemies.Available[_generator.Next(0, enemies.Available.Count)];
                    }
                }

                // TODO: Maybe restrict the chicken's appearance across the game?

                // Make sure to convert BengalTiger, StickWieldingGoonBandana etc back to their actual types
                currentEntity.TypeID = (short)TR2EntityUtilities.TranslateEntityAlias(newEntityType);
            }

            // MercSnowMobDriver relies on RedSnowmobile so it will be available in the model list
            if (!level.Is(LevelNames.TIBET))
            {
                TR2Entity mercDriver = level.Data.Entities.ToList().Find(e => e.TypeID == (short)TR2Entities.MercSnowmobDriver);
                if (mercDriver != null)
                {
                    Dictionary<string, List<Location>> allSkidooLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\skidoo_locations.json"));

                    short room;
                    int x, y, z;
                    if (allSkidooLocations.ContainsKey(level.Name))
                    {
                        // we will only spawn one skidoo, so only need one random location
                        List<Location> levelSkidooLocations = allSkidooLocations[level.Name];
                        Location randomLocation = levelSkidooLocations[_generator.Next(0, levelSkidooLocations.Count)];
                        room = (short)randomLocation.Room;
                        x = randomLocation.X;
                        y = randomLocation.Y;
                        z = randomLocation.Z;
                    }
                    else
                    {
                        // if the level does not have skidoo locations for some reason, just spawn it on the MercSnowMobDriver
                        room = mercDriver.Room;
                        x = mercDriver.X;
                        y = mercDriver.Y;
                        z = mercDriver.Z;
                    }

                    newEntities.Add(new TR2Entity
                    {
                        TypeID = (short)TR2Entities.RedSnowmobile,
                        Room = room,
                        X = x,
                        Y = y,
                        Z = z,
                        Angle = 16384,
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

        internal class EnemyProcessor : AbstractProcessorThread<EnemyRandomizer>
        {
            private readonly Dictionary<TR2CombinedLevel, List<EnemyTransportCollection>> _enemyMapping;

            internal override int LevelCount => _enemyMapping.Count;

            internal EnemyProcessor(EnemyRandomizer outer)
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
                    TRModelImporter importer = new TRModelImporter
                    {
                        ClearUnusedSprites = true,
                        EntitiesToImport = enemies.EntitiesToImport,
                        EntitiesToRemove = enemies.EntitiesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        TextureRemapPath = @"Resources\Textures\Deduplication\" + level.JsonID + "-TextureRemap.json",
                        TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.EntitiesToImport)
                    };

                    // Try to import the selected models into the level.
                    importer.Import();
                    return true;
                }
                catch (PackingException/* e*/)
                {
                    //System.Diagnostics.Debug.WriteLine(level.Name + ": " + e.Message);
                    // We need to reload the level to undo anything that may have changed.
                    _outer.ReloadLevelData(level);
                    return false;
                }
            }

            // This is triggered synchronously after the import work to ensure the RNG remains consistent
            internal void ApplyRandomization() 
            {
                foreach (TR2CombinedLevel level in _enemyMapping.Keys)
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
                        _outer.RandomizeEnemies(level, new EnemyRandomizationCollection
                        {
                            Available = importedCollection.EntitiesToImport,
                            Droppable = TR2EntityUtilities.FilterDroppableEnemies(importedCollection.EntitiesToImport, !_outer.ProtectMonks),
                            Water = TR2EntityUtilities.FilterWaterEnemies(importedCollection.EntitiesToImport)
                        });
                    }

                    _outer.SaveLevel(level);
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
        }
    }
}