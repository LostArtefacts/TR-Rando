using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TREnvironmentEditor;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRGE.Core.Item.Enums;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Randomizers
{
    public class TR3SecretRandomizer : BaseTR3Randomizer
    {
        private static readonly string _invalidDoorsMsg = "{0} secret doors required for {1}, but only {2} found.";
        private static readonly string _invalidLocationMsg = "Cannot place a nonvalidated secret where a trigger already exists - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _trapdoorLocationMsg = "Cannot place a secret on the same sector as a bridge/trapdoor - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _triggerWarningMsg = "Existing trigger object action with parameter {0} will be lost - {1} [X={2}, Y={3}, Z={4}, R={5}]";
        private static readonly string _flipMapWarningMsg = "Secret is being placed in a room that has a flipmap - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _flipMapErrorMsg = "Secret cannot be placed in a flipped room - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly List<int> _devRooms = null;
        private static readonly ushort _devModeSecretCount = 6;

        private Dictionary<string, List<Location>> _locations;

        private int _proxEvaluationCount;

        private static readonly float _LARGE_RADIUS = 5000.0f;
        private static readonly float _MED_RADIUS = 2500.0f;
        private static readonly float _SMALL_RADIUS = 750.0f;
        private static readonly float _TINY_RADIUS = 5.0f;

        private static readonly int _LARGE_RETRY_TOLERANCE = 10;
        private static readonly int _MED_RETRY_TOLERANCE = 25;
        private static readonly int _SMALL_RETRY_TOLERANCE = 50;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\locations.json"));

            SetMessage("Randomizing secrets - loading levels");

            List<SecretProcessor> processors = new List<SecretProcessor>();
            for (int i = 0; i < _maxThreads; i++)
            {
                processors.Add(new SecretProcessor(this));
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

            SetMessage("Randomizing secrets - importing models");
            foreach (SecretProcessor processor in processors)
            {
                processor.Start();
            }

            foreach (SecretProcessor processor in processors)
            {
                processor.Join();
            }

            if (!SaveMonitor.IsCancelled && _processingException == null)
            {
                SetMessage("Randomizing secrets - placing items");
                foreach (SecretProcessor processor in processors)
                {
                    processor.ApplyRandomization();
                }
            }

            if (_processingException != null)
            {
                _processingException.Throw();
            }
        }

        private void RemoveDefaultSecrets(TR3CombinedLevel level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            // Scan all rooms and remove any existing secret triggers.
            foreach (TR3Room room in level.Data.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    if (sector.FDIndex == 0)
                    {
                        continue;
                    }

                    List<FDEntry> entries = floorData.Entries[sector.FDIndex];
                    for (int i = entries.Count - 1; i >= 0; i--)
                    {
                        if (entries[i] is FDTriggerEntry trig)
                        {
                            // #230 Remove the secret action but retain anything else that may be triggered here
                            trig.TrigActionList.RemoveAll(a => a.TrigAction == FDTrigAction.SecretFound);
                            if (trig.TrigActionList.Count == 0)
                            {
                                entries.RemoveAt(i);
                            }
                        }
                    }

                    if (entries.Count == 0)
                    {
                        // If there isn't anything left, reset the sector to point to the dummy FD
                        floorData.RemoveFloorData(sector);
                    }
                }
            }

            floorData.WriteToLevel(level.Data);
        }

        private TRSecretRoom<TR2Entity> MakeRewardRoom(TR3CombinedLevel level)
        {
            TRSecretRoom<TR2Entity> rewardRoom = null;

            string mappingPath = @"TR3\SecretMapping\" + level.Name + "-SecretMapping.json";
            if (ResourceExists(mappingPath))
            {
                TRSecretMapping<TR2Entity> secretMapping = JsonConvert.DeserializeObject<TRSecretMapping<TR2Entity>>(ReadResource(mappingPath), EMEditorMapping.Converter);

                // Select a reward room and create it.
                rewardRoom = secretMapping.Rooms[_generator.Next(0, secretMapping.Rooms.Count)];
                rewardRoom.Room.ApplyToLevel(level.Data);

                // Trigger activation masks have 5 bits so we need a specific number of doors to match.
                // For development mode, test the maximum.
                double countedSecrets = Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets;
                int requiredDoors = (int)Math.Ceiling(countedSecrets / TRSecretPlacement<TR3Entities>.MaskBits);
                if (rewardRoom.Doors.Count < requiredDoors)
                {
                    throw new Exception(string.Format(_invalidDoorsMsg, requiredDoors, level.Name, rewardRoom.Doors.Count));
                }

                // Make the doors and store the entity indices for the secret triggers
                rewardRoom.DoorIndices = new List<int>();
                List<TR2Entity> entities = level.Data.Entities.ToList();
                for (int i = 0; i < requiredDoors; i++)
                {
                    TR2Entity door = rewardRoom.Doors[i];
                    if (door.Room == short.MaxValue)
                    {
                        door.Room = (short)(level.Data.NumRooms - 1);
                    }
                    entities.Add(door);
                    rewardRoom.DoorIndices.Add((int)level.Data.NumEntities);

                    // If it's a trapdoor, we need to make a dummy trigger for it
                    if (TR3EntityUtilities.IsTrapdoor((TR3Entities)door.TypeID))
                    {
                        CreateTrapdoorTrigger(door, (ushort)level.Data.NumEntities, level.Data);
                    }

                    level.Data.NumEntities++;
                }

                level.Data.Entities = entities.ToArray();

                // Spread the rewards out fairly evenly across each defined position in the new room.
                int rewardPositionCount = rewardRoom.RewardPositions.Count;
                for (int i = 0; i < secretMapping.RewardEntities.Count; i++)
                {
                    TR2Entity item = level.Data.Entities[secretMapping.RewardEntities[i]];
                    Location position = rewardRoom.RewardPositions[i % rewardPositionCount];

                    item.X = position.X;
                    item.Y = position.Y;
                    item.Z = position.Z;
                    item.Room = (short)(level.Data.NumRooms - 1);
                }

                // #238 Make the required number of cameras. Because of the masks, we need
                // a camera per counted secret otherwise it only shows once.
                if (Settings.UseRewardRoomCameras && rewardRoom.Cameras != null)
                {
                    rewardRoom.CameraIndices = new List<int>();
                    List<TRCamera> cameras = level.Data.Cameras.ToList();
                    for (int i = 0; i < countedSecrets; i++)
                    {
                        rewardRoom.CameraIndices.Add(cameras.Count);
                        cameras.Add(rewardRoom.Cameras[i % rewardRoom.Cameras.Count]);
                    }

                    level.Data.Cameras = cameras.ToArray();
                    level.Data.NumCameras = (uint)cameras.Count;
                }
            }

            return rewardRoom;
        }

        private static void CreateTrapdoorTrigger(TR2Entity door, ushort doorIndex, TR3Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(door.X, door.Y, door.Z, door.Room, level, floorData);
            if (sector.FDIndex == 0)
            {
                floorData.CreateFloorData(sector);
            }

            floorData.Entries[sector.FDIndex].Add(new FDTriggerEntry
            {
                Setup = new FDSetup { Value = 4 },
                TrigSetup = new FDTrigSetup
                {
                    Value = 15872
                },
                TrigType = FDTrigType.Dummy,
                TrigActionList = new List<FDActionListItem>
                {
                    new FDActionListItem
                    {
                        TrigAction = FDTrigAction.Object,
                        Parameter = doorIndex
                    }
                }
            });

            floorData.WriteToLevel(level);
        }

        private void PlaceAllSecrets(TR3CombinedLevel level, List<TR3Entities> pickupTypes, TRSecretRoom<TR2Entity> rewardRoom)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            List<TR2Entity> entities = level.Data.Entities.ToList();
            List<Location> locations = _locations[level.Name];

            TRSecretPlacement<TR3Entities> secret = new TRSecretPlacement<TR3Entities>();
            int pickupIndex = 0;
            ushort secretIndex = 0;
            ushort countedSecrets = _devModeSecretCount; // For dev mode test the max number of secrets in TR3
            foreach (Location location in locations)
            {
                if (_devRooms == null || _devRooms.Contains(location.Room))
                {
                    secret.Location = location;
                    secret.EntityIndex = (ushort)entities.Count;
                    secret.SecretIndex = (ushort)(secretIndex % countedSecrets); // Cycle through each secret number
                    secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count]; // Cycle through the types

                    // #238 Point this secret to a specific camera and look-at target if applicable.
                    if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
                    {
                        secret.CameraIndex = (ushort)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                        secret.CameraTarget = (ushort)rewardRoom.DoorIndices[0];
                    }

                    secret.SetMaskAndDoor(countedSecrets, rewardRoom.DoorIndices);

                    TR2Entity secretEntity = PlaceSecret(level, secret, floorData);
                    if (secretEntity != null)
                    {
                        entities.Add(secretEntity);
                        secretIndex++;
                        pickupIndex++;
                    }
                }
            }

            level.Data.Entities = entities.ToArray();
            level.Data.NumEntities = (uint)entities.Count;

            floorData.WriteToLevel(level.Data);
        }

        private void RandomizeSecrets(TR3CombinedLevel level, List<TR3Entities> pickupTypes, TRSecretRoom<TR2Entity> rewardRoom)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            List<TR2Entity> entities = level.Data.Entities.ToList();
            List<Location> locations = _locations[level.Name];
            List<Location> usedLocations = new List<Location>();

            TRSecretPlacement<TR3Entities> secret = new TRSecretPlacement<TR3Entities>();
            int pickupIndex = 0;
            while (secret.SecretIndex < level.Script.NumSecrets)
            {
                Location location;
                do
                {
                    location = locations[_generator.Next(0, locations.Count)];
                }
                while
                (
                    !EvaluateProximity(location, usedLocations)     
                );

                _proxEvaluationCount = 0;

                usedLocations.Add(location);
                secret.Location = location;
                secret.EntityIndex = (ushort)entities.Count;
                secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count]; // Cycle through the types

                // #238 Point this secret to a specific camera and look-at target if applicable.
                if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
                {
                    secret.CameraIndex = (ushort)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                    secret.CameraTarget = (ushort)rewardRoom.DoorIndices[0];
                }

                secret.SetMaskAndDoor(level.Script.NumSecrets, rewardRoom.DoorIndices);

                TR2Entity secretEntity = PlaceSecret(level, secret, floorData);
                if (secretEntity != null)
                {
                    entities.Add(secretEntity);
                    secret.SecretIndex++;
                    pickupIndex++;
                }
            }

            level.Data.Entities = entities.ToArray();
            level.Data.NumEntities = (uint)entities.Count;

            floorData.WriteToLevel(level.Data);
        }

        private bool EvaluateProximity(Location loc, List<Location> usedLocs)
        {
            bool SafeToPlace = true;
            float proximity = 10000.0f;

            if (loc.Difficulty == Difficulty.Hard && !Settings.HardSecrets)
                return false;

            if (loc.RequiresGlitch && !Settings.GlitchedSecrets)
                return false;

            if (usedLocs.Count == 0 || usedLocs == null)
                return true;

            _proxEvaluationCount++;

            //Be more generous with proximity if we are failing to place.
            if ( _proxEvaluationCount <= _LARGE_RETRY_TOLERANCE)
            {
                proximity = _LARGE_RADIUS;
            }
            else if (_proxEvaluationCount > _LARGE_RETRY_TOLERANCE && _proxEvaluationCount <= _MED_RETRY_TOLERANCE)
            {
                proximity = _MED_RADIUS;
            }
            else if (_proxEvaluationCount > _MED_RETRY_TOLERANCE && _proxEvaluationCount <= _SMALL_RETRY_TOLERANCE)
            {
                proximity = _SMALL_RADIUS;
            }
            else
            {
                proximity = _TINY_RADIUS;
            }

            Sphere newLoc = new Sphere(new System.Numerics.Vector3(loc.X, loc.Y, loc.Z), proximity);

            foreach (Location used in usedLocs)
            {
                SafeToPlace = !newLoc.IsColliding(new Sphere(new System.Numerics.Vector3(used.X, used.Y, used.Z), proximity));

                if (SafeToPlace == false)
                    break;
            }

            return SafeToPlace;
        }

        private TR2Entity PlaceSecret(TR3CombinedLevel level, TRSecretPlacement<TR3Entities> secret, FDControl floorData)
        {
            // Check if this secret is being added to a flipped room, as that won't work
            for (int i = 0; i < level.Data.NumRooms; i++)
            {
                if (level.Data.Rooms[i].AlternateRoom == secret.Location.Room)
                {
                    if (Settings.DevelopmentMode)
                    {
                        // Place it anyway in dev mode to allow relocating
                        Debug.WriteLine(string.Format(_flipMapErrorMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, secret.Location.Room));
                        break;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            TR2Entity entity = new TR2Entity
            {
                TypeID = (short)secret.PickupType,
                X = secret.Location.X,
                Y = secret.Location.Y,
                Z = secret.Location.Z,
                Room = (short)secret.Location.Room,
                Angle = 0,
                Intensity1 = -1,
                Intensity2 = -1
            };

            // Get the sector and check if it is shared with a trapdoor or bridge, as these won't work either.
            TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level.Data, floorData);
            foreach (TR2Entity otherEntity in level.Data.Entities)
            {
                TR3Entities type = (TR3Entities)otherEntity.TypeID;
                if (entity.Room == otherEntity.Room && (TR3EntityUtilities.IsTrapdoor(type) || TR3EntityUtilities.IsBridge(type)))
                {
                    TRRoomSector otherSector = FDUtilities.GetRoomSector(otherEntity.X, otherEntity.Y, otherEntity.Z, otherEntity.Room, level.Data, floorData);
                    if (otherSector == sector)
                    {
                        if (Settings.DevelopmentMode)
                        {
                            Debug.WriteLine(string.Format(_trapdoorLocationMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, secret.Location.Room));
                        }
                        return null;
                    }
                }
            }

            // Create the trigger. If this was unsuccessful, bail out.
            if (!CreateSecretTrigger(level, secret, entity.Room, floorData, sector))
            {
                return null;
            }

            // #248 If the room has a flipmap, make sure to add the trigger there too.
            short altRoom = level.Data.Rooms[secret.Location.Room].AlternateRoom;
            if (altRoom != -1)
            {
                sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, altRoom, level.Data, floorData);
                if (!CreateSecretTrigger(level, secret, altRoom, floorData, sector))
                {
                    return null;
                }

                if (Settings.DevelopmentMode)
                {
                    Debug.WriteLine(string.Format(_flipMapWarningMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, altRoom));
                }
            }

            return entity;
        }

        private bool CreateSecretTrigger(TR3CombinedLevel level, TRSecretPlacement<TR3Entities> secret, short room, FDControl floorData, TRRoomSector sector)
        {
            if (sector.FDIndex == 0)
            {
                floorData.CreateFloorData(sector);
            }

            FDTriggerEntry existingTrigger = floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            if (existingTrigger != null && !secret.Location.Validated)
            {
                // There is already a trigger here and the location hasn't been marked as being
                // safe to move the action items to the new pickup trigger.
                if (Settings.DevelopmentMode)
                {
                    Debug.WriteLine(string.Format(_invalidLocationMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                }
                return false;
            }

            // Make a new pickup trigger
            FDTriggerEntry trigger = new FDTriggerEntry
            {
                Setup = new FDSetup { Value = 4 },
                TrigSetup = new FDTrigSetup
                {
                    Value = 15872,
                    Mask = secret.TriggerMask
                },
                TrigType = FDTrigType.Pickup,
                TrigActionList = new List<FDActionListItem>
                {
                    new FDActionListItem
                    {
                        TrigAction = FDTrigAction.Object,
                        Parameter = secret.EntityIndex
                    },
                    new FDActionListItem
                    {
                        TrigAction = FDTrigAction.SecretFound,
                        Parameter = secret.SecretIndex
                    }
                }
            };

            if (secret.TriggersDoor)
            {
                trigger.TrigActionList.Add(new FDActionListItem
                {
                    TrigAction = FDTrigAction.Object,
                    Parameter = secret.DoorIndex
                });
            }

            // Move any existing action list items to the new trigger and remove the old one. We can only
            // move Object actions if the mask on this trigger is full.
            if (existingTrigger != null)
            {
                List<FDActionListItem> existingActions = new List<FDActionListItem>();
                foreach (FDActionListItem actionItem in existingTrigger.TrigActionList)
                {
                    if (actionItem.TrigAction == FDTrigAction.Object)
                    {
                        if (Settings.DevelopmentMode)
                        {
                            existingActions.Add(actionItem); // Add it anyway for testing
                            Debug.WriteLine(string.Format(_triggerWarningMsg, actionItem.Parameter, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                        }
                        else if (secret.TriggerMask == TRSecretPlacement<TR3Entities>.FullActivation)
                        {
                            existingActions.Add(actionItem);
                        }
                    }
                    else
                    {
                        existingActions.Add(actionItem);
                    }
                }

                trigger.TrigActionList.AddRange(existingActions);
                floorData.Entries[sector.FDIndex].Remove(existingTrigger);
            }

            // #238 Add a camera action provided there isn't one already
            if (secret.TriggersCamera && trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.Camera) == null)
            {
                trigger.TrigActionList.Add(new FDActionListItem
                {
                    TrigAction = FDTrigAction.Camera,
                    CamAction = new FDCameraAction { Value = 4 },
                    Parameter = secret.CameraIndex
                });
                trigger.TrigActionList.Add(new FDActionListItem
                {
                    TrigAction = FDTrigAction.LookAtItem,
                    Parameter = secret.CameraTarget
                });
            }

            floorData.Entries[sector.FDIndex].Add(trigger);

            return true;
        }

        internal class SecretProcessor : AbstractProcessorThread<TR3SecretRandomizer>
        {
            private static readonly Dictionary<TR3Entities, TR3Entities> _artefactPickups = TR3EntityUtilities.GetArtefactPickups();
            private static readonly Dictionary<TR3Entities, TR3Entities> _artefactReplacements = TR3EntityUtilities.GetArtefactReplacements();

            // Move this to Gamestring Rando once implemented
            private static readonly Dictionary<TR3Entities, string> _pickupNames = new Dictionary<TR3Entities, string>
            {
                [TR3Entities.Infada_P] = "Secret Infada Stone",
                [TR3Entities.OraDagger_P] = "Secret Ora Dagger",
                [TR3Entities.Element115_P] = "Secret Element 115",
                [TR3Entities.EyeOfIsis_P] = "Secret Eye of Isis",
                [TR3Entities.Quest1_P] = "Secret Serpent Stone",
                [TR3Entities.Quest2_P] = "Secret Hand of Rathmore"
            };

            private readonly Dictionary<TR3CombinedLevel, TRSecretModelAllocation<TR3Entities>> _importAllocations;

            internal override int LevelCount => _importAllocations.Count;

            internal SecretProcessor(TR3SecretRandomizer outer)
                : base(outer)
            {
                _importAllocations = new Dictionary<TR3CombinedLevel, TRSecretModelAllocation<TR3Entities>>();
            }

            internal void AddLevel(TR3CombinedLevel level)
            {
                _importAllocations.Add(level, new TRSecretModelAllocation<TR3Entities>());
            }

            protected override void StartImpl()
            {
                foreach (TR3CombinedLevel level in _importAllocations.Keys)
                {
                    if (!level.HasSecrets && !_outer.Settings.DevelopmentMode)
                    {
                        continue;
                    }

                    TRSecretModelAllocation<TR3Entities> allocation = _importAllocations[level];

                    // Work out which models are available to replace as secret pickups.
                    // We exclude artefacts from import if this level already has an 
                    // actual artefact model, and we exclude current puzzle/key items
                    // from the available switching pool.
                    List<TRModel> models = level.Data.Models.ToList();

                    if (level.Is(TR3LevelNames.CRASH))
                    {
                        // Special case for Crash Site, which is the only level that uses Quest1 (the swamp map).
                        // We want to reallocate this as a key to allow us to reuse Quest1 on import. Amend the
                        // models to become Key3 and update the script to match.
                        models.Find(m => m.ID == (uint)TR3Entities.Quest1_P).ID = (uint)TR3Entities.Key3_P;
                        models.Find(m => m.ID == (uint)TR3Entities.Quest1_M_H).ID = (uint)TR3Entities.Key3_M_H;
                        level.Script.Keys[2] = level.Script.Pickups[0];
                        level.Script.SetStartInventoryItems(new Dictionary<TR3Items, int>
                        {
                            [TR3Items.Key3] = 1
                        });
                    }

                    foreach (TR3Entities puzzleType in _artefactReplacements.Keys)
                    {
                        if (models.Find(m => m.ID == (uint)puzzleType) == null)
                        {
                            allocation.AvailablePickupModels.Add(puzzleType);
                        }
                    }

                    List<TR3Entities> artefactTypes = _artefactPickups.Keys.ToList();
                    for (int i = artefactTypes.Count - 1; i >= 0; i--)
                    {
                        TR3Entities artefactType = artefactTypes[i];
                        if (models.Find(m => m.ID == (uint)artefactType) != null)
                        {
                            artefactTypes.RemoveAt(i);
                        }
                    }

                    // How many models do we actually need?
                    int modelImportCount = Math.Min(_outer.Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets, allocation.AvailablePickupModels.Count);
                    modelImportCount = Math.Min(modelImportCount, artefactTypes.Count);

                    for (int i = 0; i < modelImportCount; i++)
                    {
                        allocation.ImportModels.Add(artefactTypes[i]);
                    }
                }
            }

            protected override void ProcessImpl()
            {
                foreach (TR3CombinedLevel level in _importAllocations.Keys)
                {
                    if (level.HasSecrets || _outer.Settings.DevelopmentMode)
                    {
                        TRSecretModelAllocation<TR3Entities> allocation = _importAllocations[level];

                        // Get the artefacts into the level and refresh the model list
                        TR3ModelImporter importer = new TR3ModelImporter
                        {
                            Level = level.Data,
                            LevelName = level.Name,
                            EntitiesToImport = allocation.ImportModels,
                            DataFolder = _outer.GetResourcePath(@"TR3\Models")
                        };

                        importer.Import();

                        List<TRModel> models = level.Data.Models.ToList();

                        // Redefine the artefacts as puzzle models otherwise the level ends on pickup
                        foreach (TR3Entities artefactPickupType in allocation.ImportModels)
                        {
                            TR3Entities artefactMenuType = _artefactPickups[artefactPickupType];

                            TR3Entities puzzlePickupType = allocation.AvailablePickupModels.First();
                            TR3Entities puzzleMenuType = _artefactReplacements[puzzlePickupType];

                            models.Find(m => m.ID == (uint)artefactPickupType).ID = (uint)puzzlePickupType;
                            models.Find(m => m.ID == (uint)artefactMenuType).ID = (uint)puzzleMenuType;

                            // Remove this puzzle type from the available pool
                            allocation.AvailablePickupModels.RemoveAt(0);

                            // Make the pickup type available to assign to items
                            allocation.AssignedPickupModels.Add(puzzlePickupType);

                            // Assign a name for the script
                            string name = _pickupNames[artefactPickupType];
                            if (TR3EntityUtilities.IsKeyType(puzzlePickupType))
                            {
                                level.Script.Keys[puzzlePickupType - TR3Entities.Key1_P] = name;
                            }
                            else if (TR3EntityUtilities.IsPuzzleType(puzzlePickupType))
                            {
                                level.Script.Puzzles[puzzlePickupType - TR3Entities.Puzzle1_P] = name;
                            }
                            else if (TR3EntityUtilities.IsQuestType(puzzlePickupType))
                            {
                                level.Script.Pickups[puzzlePickupType - TR3Entities.Quest1_P] = name;
                            }
                        }
                    }

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }

            internal void ApplyRandomization()
            {
                foreach (TR3CombinedLevel level in _importAllocations.Keys)
                {
                    // Get rid of existing secret triggers
                    _outer.RemoveDefaultSecrets(level);

                    // Only create new secrets if applicable
                    if (level.HasSecrets || _outer.Settings.DevelopmentMode)
                    {
                        TRSecretModelAllocation<TR3Entities> allocation = _importAllocations[level];

                        // Create the reward room. The returned object will contain the door and
                        // any camera indices.
                        TRSecretRoom<TR2Entity> rewardRoom = _outer.MakeRewardRoom(level);

                        // Pass the list of artefacts we can use as pickups along with the reward
                        // room to the secret placers.
                        if (_outer.Settings.DevelopmentMode)
                        {
                            _outer.PlaceAllSecrets(level, allocation.AssignedPickupModels, rewardRoom);
                        }
                        else
                        {
                            _outer.RandomizeSecrets(level, allocation.AssignedPickupModels, rewardRoom);
                        }
                    }

                    _outer.SaveLevel(level);

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }
        }
    }
}