using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TREnvironmentEditor.Model.Types;
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
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers
{
    public class TR1SecretRandomizer : BaseTR1Randomizer
    {
        private static readonly string _invalidLocationMsg = "Cannot place a nonvalidated secret where a trigger already exists - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _trapdoorLocationMsg = "Cannot place a secret on the same sector as a bridge/trapdoor - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _midairErrorMsg = "Cannot place a secret in mid-air or on a breakable tile - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _triggerWarningMsg = "Existing trigger object action with parameter {0} will be lost - {1} [X={2}, Y={3}, Z={4}, R={5}]";
        private static readonly string _flipMapWarningMsg = "Secret is being placed in a room that has a flipmap - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _flipMapErrorMsg = "Secret cannot be placed in a flipped room - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _edgeInfoMsg = "Adding extra tile edge trigger for {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly List<int> _devRooms = null;

        private static readonly ushort _maxSecretCount = 5;

        private Dictionary<string, List<Location>> _locations, _unarmedLocations;

        private int _proxEvaluationCount;

        private static readonly float _LARGE_RADIUS = 5000.0f;
        private static readonly float _MED_RADIUS = 2500.0f;
        private static readonly float _SMALL_RADIUS = 750.0f;
        private static readonly float _TINY_RADIUS = 5.0f;

        private static readonly int _LARGE_RETRY_TOLERANCE = 10;
        private static readonly int _MED_RETRY_TOLERANCE = 25;
        private static readonly int _SMALL_RETRY_TOLERANCE = 50;

        private static readonly int _triggerEdgeLimit = 103; // Within ~10% of a tile edge, triggers will be copied into neighbours

        public ItemFactory ItemFactory { get; set; }
        public List<TR1ScriptedLevel> MirrorLevels { get; set; }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\locations.json"));
            _unarmedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\unarmed_locations.json"));

            if (ScriptEditor.Edition.IsCommunityPatch)
            {
                SetSecretCounts();
            }

            SetMessage("Randomizing secrets - loading levels");

            List<SecretProcessor> processors = new List<SecretProcessor>();
            for (int i = 0; i < _maxThreads; i++)
            {
                processors.Add(new SecretProcessor(this));
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

            if (ScriptEditor.Edition.IsCommunityPatch)
            {
                TR1Script script = ScriptEditor.Script as TR1Script;
                script.FixPyramidSecretTrigger = false;

                if (Settings.UseRecommendedCommunitySettings)
                {
                    script.Enable3dPickups = false;
                }

                if (Settings.GlitchedSecrets)
                {
                    script.FixDescendingGlitch = false;
                    script.FixQwopGlitch = false;
                    script.FixWallJumpGlitch = false;
                }

                ScriptEditor.SaveScript();
            }
        }

        private bool Are3DPickupsEnabled()
        {
            return ScriptEditor.Edition.IsCommunityPatch
                && !Settings.UseRecommendedCommunitySettings
                && (ScriptEditor.Script as TR1Script).Enable3dPickups;
        }

        private void SetSecretCounts()
        {
            List<TR1ScriptedLevel> levels = Levels.FindAll(l => !l.Is(TRLevelNames.ASSAULT));

            switch (Settings.SecretCountMode)
            {
                case TRSecretCountMode.Shuffled:
                    List<ushort> defaultCounts = levels.Select(l => l.NumSecrets).ToList();
                    foreach (TR1ScriptedLevel level in levels)
                    {
                        int countIndex = _generator.Next(0, defaultCounts.Count);
                        level.NumSecrets = defaultCounts[countIndex];
                        defaultCounts.RemoveAt(countIndex);
                    }
                    break;

                case TRSecretCountMode.Customized:
                    int min = (int)Math.Max(1, Settings.MinSecretCount);
                    int max = (int)Math.Min(_maxSecretCount, Settings.MaxSecretCount) + 1;
                    foreach (TR1ScriptedLevel level in levels)
                    {
                        level.NumSecrets = (ushort)_generator.Next(min, max);
                    }
                    break;

                default:
                    break;
            }
        }

        private void RemoveDefaultSecrets(TR1CombinedLevel level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            // Scan all rooms and remove any existing secret triggers.
            foreach (TRRoom room in level.Data.Rooms)
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

        private TRSecretRoom<TREntity> MakePlaceholderRewardRoom(TR1CombinedLevel level)
        {
            TRSecretRoom<TREntity> rewardRoom = null;
            string mappingPath = @"TR1\SecretMapping\" + level.Name + "-SecretMapping.json";
            if (ResourceExists(mappingPath))
            {
                // Trigger activation masks have 5 bits so we need a specific number of doors to match.
                // Limited to 1 (5 secrets for the time being).
                //double countedSecrets = Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets;
                int requiredDoors = 1;// (int)Math.Ceiling(countedSecrets / TRSecretPlacement<TREntities>.MaskBits);

                // Make the doors and store the entity indices for the secret triggers
                rewardRoom = new TRSecretRoom<TREntity>
                {
                    DoorIndices = new List<int>()
                };
                List<TREntity> entities = level.Data.Entities.ToList();
                for (int i = 0; i < requiredDoors; i++)
                {
                    TREntity door = ItemFactory.CreateItem(level.Name, entities);
                    rewardRoom.DoorIndices.Add(entities.IndexOf(door));
                    level.Data.NumEntities++;
                }

                level.Data.Entities = entities.ToArray();
            }

            return rewardRoom;
        }

        private void ActualiseRewardRoom(TR1CombinedLevel level, TRSecretRoom<TREntity> placeholder)
        {
            TRSecretMapping<TREntity> secretMapping = TRSecretMapping<TREntity>.Get(GetResourcePath(@"TR1\SecretMapping\" + level.Name + "-SecretMapping.json"));
            if (secretMapping == null || secretMapping.Rooms.Count == 0)
            {
                return;
            }

            // Are any rooms enforced based on level specifics?
            TRSecretRoom<TREntity> rewardRoom = secretMapping.Rooms.Find(r => r.HasUsageCondition);
            if (rewardRoom == null || !rewardRoom.UsageCondition.GetResult(level.Data))
            {
                do
                {
                    rewardRoom = secretMapping.Rooms[_generator.Next(0, secretMapping.Rooms.Count)];
                }
                while (rewardRoom == null || rewardRoom.HasUsageCondition);
            }

            rewardRoom.Room.ApplyToLevel(level.Data);
            short roomIndex = (short)(level.Data.NumRooms - 1);

            // Convert the temporary doors
            rewardRoom.DoorIndices = placeholder.DoorIndices;
            for (int i = 0; i < rewardRoom.DoorIndices.Count; i++)
            {
                int doorIndex = rewardRoom.DoorIndices[i];
                TREntity door = rewardRoom.Doors[i];
                if (door.Room < 0)
                {
                    door.Room = roomIndex;
                }
                level.Data.Entities[doorIndex] = door;

                // If it's a trapdoor, we need to make a dummy trigger for it
                if (TR1EntityUtilities.IsTrapdoor((TREntities)door.TypeID))
                {
                    CreateTrapdoorTrigger(door, (ushort)doorIndex, level.Data);
                }
            }

            // Get the reward entities.
            List<int> rewardEntities = secretMapping.RewardEntities;

            // Spread them out fairly evenly across each defined position in the new room.
            int rewardPositionCount = rewardRoom.RewardPositions.Count;
            for (int i = 0; i < rewardEntities.Count; i++)
            {
                TREntity item = level.Data.Entities[rewardEntities[i]];
                Location position = rewardRoom.RewardPositions[i % rewardPositionCount];

                item.X = position.X;
                item.Y = position.Y;
                item.Z = position.Z;
                item.Room = roomIndex;
            }

            // #238 Make the required number of cameras. Because of the masks, we need
            // a camera per counted secret otherwise it only shows once.
            if (Settings.UseRewardRoomCameras && rewardRoom.Cameras != null)
            {
                double countedSecrets = Settings.DevelopmentMode ? _maxSecretCount : level.Script.NumSecrets;
                rewardRoom.CameraIndices = new List<int>();
                List<TRCamera> cameras = level.Data.Cameras.ToList();
                for (int i = 0; i < countedSecrets; i++)
                {
                    rewardRoom.CameraIndices.Add(cameras.Count);
                    cameras.Add(rewardRoom.Cameras[i % rewardRoom.Cameras.Count]);
                }

                level.Data.Cameras = cameras.ToArray();
                level.Data.NumCameras = (uint)cameras.Count;

                ushort cameraTarget;
                List<TREntity> levelEntities = level.Data.Entities.ToList();
                if (rewardRoom.CameraTarget != null && ItemFactory.CanCreateItem(level.Name, levelEntities))
                {
                    TREntity target = ItemFactory.CreateItem(level.Name, levelEntities, rewardRoom.CameraTarget);
                    target.TypeID = (short)TREntities.CameraTarget_N;
                    cameraTarget = (ushort)levelEntities.IndexOf(target);
                    level.Data.Entities = levelEntities.ToArray();
                    level.Data.NumEntities++;
                }
                else
                {
                    cameraTarget = (ushort)rewardRoom.DoorIndices[0];
                }

                FDControl floorData = new FDControl();
                floorData.ParseFromLevel(level.Data);

                // Get each trigger created for each secret index and add the camera, provided
                // there isn't any existing camera actions.
                for (int i = 0; i < countedSecrets; i++)
                {
                    List<FDTriggerEntry> secretTriggers = FDUtilities.GetSecretTriggers(floorData, i);
                    foreach (FDTriggerEntry trigger in secretTriggers)
                    {
                        if (trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.Camera) == null)
                        {
                            trigger.TrigActionList.Add(new FDActionListItem
                            {
                                TrigAction = FDTrigAction.Camera,
                                CamAction = new FDCameraAction { Value = 4 },
                                Parameter = (ushort)rewardRoom.CameraIndices[i]
                            });
                            trigger.TrigActionList.Add(new FDActionListItem
                            {
                                TrigAction = FDTrigAction.LookAtItem,
                                Parameter = cameraTarget
                            });
                        }
                    }
                }

                // Write back the camera triggers
                floorData.WriteToLevel(level.Data);
            }
        }

        private static void CreateTrapdoorTrigger(TREntity door, ushort doorIndex, TRLevel level)
        {
            new EMTriggerFunction
            {
                Locations = new List<EMLocation>
                {
                    new EMLocation
                    {
                        X = door.X,
                        Y = door.Y,
                        Z = door.Z,
                        Room = door.Room
                    }
                },
                Trigger = new EMTrigger
                {
                    TrigType = FDTrigType.Dummy,
                    Actions = new List<EMTriggerAction>
                    {
                        new EMTriggerAction
                        {
                            Parameter = (short)doorIndex
                        }
                    }
                }
            }.ApplyToLevel(level);
        }

        private void PlaceAllSecrets(TR1CombinedLevel level, List<TREntities> pickupTypes, TRSecretRoom<TREntity> rewardRoom)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            List<TREntity> entities = level.Data.Entities.ToList();
            List<Location> locations = _locations[level.Name];

            TRSecretPlacement<TREntities> secret = new TRSecretPlacement<TREntities>();
            int pickupIndex = 0;
            ushort secretIndex = 0;
            ushort countedSecrets = _maxSecretCount;
            bool damagingLocationUsed = false;
            bool glitchedDamagingLocationUsed = false;
            foreach (Location location in locations)
            {
                if (_devRooms == null || _devRooms.Contains(location.Room))
                {
                    if (MirrorLevels.Contains(level.Script) && location.LevelState == LevelState.NotMirrored)
                        continue;

                    if (!MirrorLevels.Contains(level.Script) && location.LevelState == LevelState.Mirrored)
                        continue;

                    secret.Location = location;
                    secret.EntityIndex = (ushort)ItemFactory.GetNextIndex(level.Name, entities, true);
                    secret.SecretIndex = (ushort)(secretIndex % countedSecrets); // Cycle through each secret number
                    secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count]; // Cycle through the types

                    // #238 Point this secret to a specific camera and look-at target if applicable.
                    if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
                    {
                        secret.CameraIndex = (ushort)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                        secret.CameraTarget = (ushort)rewardRoom.DoorIndices[0];
                    }

                    secret.SetMaskAndDoor(countedSecrets, rewardRoom.DoorIndices);

                    if (PlaceSecret(level, secret, floorData))
                    {
                        // This will either make a new entity or repurpose an old one
                        TREntity entity = ItemFactory.CreateItem(level.Name, entities, secret.Location, true);
                        entity.TypeID = (short)secret.PickupType;

                        secretIndex++;
                        pickupIndex++;

                        if (location.RequiresDamage)
                        {
                            damagingLocationUsed = true;
                            if (location.RequiresGlitch)
                            {
                                glitchedDamagingLocationUsed = true;
                            }
                        }
                    }
                }
            }

            level.Data.Entities = entities.ToArray();
            level.Data.NumEntities = (uint)entities.Count;

            floorData.WriteToLevel(level.Data);

            AddDamageControl(level, damagingLocationUsed, glitchedDamagingLocationUsed);
        }

        private void RandomizeSecrets(TR1CombinedLevel level, List<TREntities> pickupTypes, TRSecretRoom<TREntity> rewardRoom)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            List<TREntity> entities = level.Data.Entities.ToList();
            List<Location> locations = _locations[level.Name];
            locations.Shuffle(_generator);
            List<Location> usedLocations = new List<Location>();

            TRSecretPlacement<TREntities> secret = new TRSecretPlacement<TREntities>();
            int pickupIndex = 0;
            bool damagingLocationUsed = false;
            bool glitchedDamagingLocationUsed = false;
            while (secret.SecretIndex < level.Script.NumSecrets)
            {
                Location location;
                do
                {
                    location = locations[_generator.Next(0, locations.Count)];
                }
                while
                (
                    !EvaluateProximity(location, usedLocations, level, floorData)
                );

                _proxEvaluationCount = 0;

                usedLocations.Add(location);
                secret.Location = location;
                secret.EntityIndex = (ushort)ItemFactory.GetNextIndex(level.Name, entities);
                secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count]; // Cycle through the types

                // #238 Point this secret to a specific camera and look-at target if applicable.
                if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
                {
                    secret.CameraIndex = (ushort)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                    secret.CameraTarget = (ushort)rewardRoom.DoorIndices[0];
                }

                secret.SetMaskAndDoor(level.Script.NumSecrets, rewardRoom.DoorIndices);

                if (PlaceSecret(level, secret, floorData))
                {
                    // This will either make a new entity or repurpose an old one
                    TREntity entity = ItemFactory.CreateItem(level.Name, entities, secret.Location);
                    entity.TypeID = (short)secret.PickupType;

                    secret.SecretIndex++;
                    pickupIndex++;

                    if (location.RequiresDamage)
                    {
                        damagingLocationUsed = true;
                        if (location.RequiresGlitch)
                        {
                            glitchedDamagingLocationUsed = true;
                        }
                    }
                }
            }

            level.Data.Entities = entities.ToArray();
            level.Data.NumEntities = (uint)entities.Count;

            floorData.WriteToLevel(level.Data);

            AddDamageControl(level, damagingLocationUsed, glitchedDamagingLocationUsed);
        }

        private bool EvaluateProximity(Location loc, List<Location> usedLocs, TR1CombinedLevel level, FDControl floorData)
        {
            bool SafeToPlace = true;
            if (loc.Difficulty == Difficulty.Hard && !Settings.HardSecrets)
                return false;

            if (loc.RequiresGlitch && !Settings.GlitchedSecrets)
                return false;

            if (MirrorLevels.Contains(level.Script) && loc.LevelState == LevelState.NotMirrored)
                return false;

            if (!MirrorLevels.Contains(level.Script) && loc.LevelState == LevelState.Mirrored)
                return false;

            if (usedLocs.Count == 0 || usedLocs == null)
                return true;

            _proxEvaluationCount++;

            float proximity;
            //Be more generous with proximity if we are failing to place.
            if (_proxEvaluationCount <= _LARGE_RETRY_TOLERANCE)
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
            // Tilted sectors can still pass the proximity test, so in any case we never want 2 secrets sharing a tile.
            TRRoomSector newSector = FDUtilities.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room, level.Data, floorData);

            foreach (Location used in usedLocs)
            {
                SafeToPlace = !newLoc.IsColliding(new Sphere(new System.Numerics.Vector3(used.X, used.Y, used.Z), proximity))
                    && newSector != FDUtilities.GetRoomSector(used.X, used.Y, used.Z, (short)used.Room, level.Data, floorData);

                if (SafeToPlace == false)
                    break;
            }

            return SafeToPlace;
        }

        private void AddDamageControl(TR1CombinedLevel level, bool damagingLocationUsed, bool glitchedDamagingLocationUsed)
        {
            // If we have used a secret that requires damage, add a large medi to an unarmed level
            // weapon location.
            if (damagingLocationUsed && _unarmedLocations.ContainsKey(level.Name))
            {
                List<TREntity> entities = level.Data.Entities.ToList();
                if (ItemFactory.CanCreateItem(level.Name, entities, Settings.DevelopmentMode))
                {
                    List<Location> pool = _unarmedLocations[level.Name];
                    Location location = pool[_generator.Next(0, pool.Count)];

                    TREntity medi = ItemFactory.CreateItem(level.Name, entities, location, Settings.DevelopmentMode);
                    medi.TypeID = (short)TREntities.LargeMed_S_P;

                    level.Data.Entities = entities.ToArray();
                    level.Data.NumEntities++;
                }
                else if (ScriptEditor.Edition.IsCommunityPatch)
                {
                    level.Script.AddStartInventoryItem(TR1Items.LargeMedi);
                }
            }

            // If we have also used a secret that requires damage and is glitched, add an additional
            // medi as these tend to occur where Lara has to drop far after picking them up.
            if (glitchedDamagingLocationUsed && ScriptEditor.Edition.IsCommunityPatch)
            {
                level.Script.AddStartInventoryItem(TR1Items.SmallMedi);
            }
        }

        private void SetPuzzleTypeName(TR1CombinedLevel level, TREntities itemType, string name)
        {
            if (TR1EntityUtilities.IsKeyType(itemType))
            {
                level.Script.Keys.Add(name);
            }
            else if (TR1EntityUtilities.IsPuzzleType(itemType))
            {
                level.Script.Puzzles.Add(name);
            }
            else if (TR1EntityUtilities.IsQuestType(itemType))
            {
                level.Script.Pickups.Add(name);
            }
        }

        private bool PlaceSecret(TR1CombinedLevel level, TRSecretPlacement<TREntities> secret, FDControl floorData)
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
                        return false;
                    }
                }
            }

            // Get the sector and check if it is shared with a trapdoor, breakable tile or bridge, as these won't work either.
            TRRoomSector sector = FDUtilities.GetRoomSector(secret.Location.X, secret.Location.Y, secret.Location.Z, (short)secret.Location.Room, level.Data, floorData);
            foreach (TREntity otherEntity in level.Data.Entities)
            {
                TREntities type = (TREntities)otherEntity.TypeID;
                if (secret.Location.Room == otherEntity.Room && (TR1EntityUtilities.IsTrapdoor(type) || TR1EntityUtilities.IsBridge(type) || type == TREntities.FallingBlock))
                {
                    TRRoomSector otherSector = FDUtilities.GetRoomSector(otherEntity.X, otherEntity.Y, otherEntity.Z, otherEntity.Room, level.Data, floorData);
                    if (otherSector == sector)
                    {
                        if (Settings.DevelopmentMode)
                        {
                            Debug.WriteLine(string.Format(_trapdoorLocationMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, secret.Location.Room));
                        }
                        return false;
                    }
                }
            }

            // Additional checks for bridge, trapdoor and breakable tile triggers that may be in rooms further below.
            // We look for floating secrets except if underwater or if the flipped room exists and has a floor below it.
            if (!CheckSectorsBelow(level, secret.Location, sector, floorData))
            {
                Debug.WriteLine(string.Format(_midairErrorMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, secret.Location.Room));
                return false;
            }

            // Create the trigger. If this was unsuccessful, bail out.
            if (!CreateSecretTriggers(level, secret, (short)secret.Location.Room, floorData, sector))
            {
                return false;
            }

            // #248 If the room has a flipmap, make sure to add the trigger there too.
            short altRoom = level.Data.Rooms[secret.Location.Room].AlternateRoom;
            if (altRoom != -1)
            {
                sector = FDUtilities.GetRoomSector(secret.Location.X, secret.Location.Y, secret.Location.Z, altRoom, level.Data, floorData);
                if (!CreateSecretTriggers(level, secret, altRoom, floorData, sector))
                {
                    return false;
                }

                if (Settings.DevelopmentMode)
                {
                    Debug.WriteLine(string.Format(_flipMapWarningMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, altRoom));
                }
            }

            // Turn off walk-to-items in T1M if we are placing on a slope above water.
            if (ScriptEditor.Edition.IsCommunityPatch 
                && !level.Data.Rooms[secret.Location.Room].ContainsWater
                && secret.Location.IsSlipperySlope(level.Data, floorData))
            {
                (ScriptEditor as TR1ScriptEditor).WalkToItems = false;
            }

            // Checks have passed, so we can actually create the entity.
            return true;
        }

        private bool CheckSectorsBelow(TR1CombinedLevel level, Location location, TRRoomSector sector, FDControl floorData)
        {
            // Allow this check to be overridden with Validated - covers glitched locations.
            if (!location.Validated && sector.RoomBelow != 255)
            {
                if (level.Data.Rooms[location.Room].ContainsWater)
                {
                    // Floating underwater, this will work
                    return true;
                }

                short altRoom = level.Data.Rooms[location.Room].AlternateRoom;
                if (altRoom != -1)
                {
                    // Flipped room may have a floor here, or be underwater
                    sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, level.Data.Rooms[location.Room].AlternateRoom, level.Data, floorData);
                    return sector.RoomBelow == 255 || level.Data.Rooms[altRoom].ContainsWater;
                }
                
                return false;
            }

            return true;
        }

        private bool CreateSecretTriggers(TR1CombinedLevel level, TRSecretPlacement<TREntities> secret, short room, FDControl floorData, TRRoomSector baseSector)
        {
            if (!CreateSecretTrigger(level, secret, room, floorData, baseSector))
            {
                return false;
            }

            // Check neighbouring sectors if we are very close to tile edges. We scan 8 locations around
            // the secret's position based on the edge tolerance and see if the sector has changed.
            ISet<TRRoomSector> processedSectors = new HashSet<TRRoomSector> { baseSector };
            for (int xNorm = -1; xNorm < 2; xNorm++)
            {
                for (int zNorm = -1; zNorm < 2; zNorm++)
                {
                    if (xNorm == 0 && zNorm == 0) continue; // Primary trigger's sector

                    int x = secret.Location.X + xNorm * _triggerEdgeLimit;
                    int z = secret.Location.Z + zNorm * _triggerEdgeLimit;
                    TRRoomSector neighbour = FDUtilities.GetRoomSector(x, secret.Location.Y, z, room, level.Data, floorData);

                    // Process each unique sector only once and if it's a valid neighbour, add the extra trigger
                    if (processedSectors.Add(neighbour) && !IsInvalidNeighbour(neighbour))
                    {
                        if (neighbour.RoomBelow != baseSector.RoomBelow && neighbour.RoomBelow != 255)
                        {
                            // Try to find the absolute floor
                            do
                            {
                                neighbour = FDUtilities.GetRoomSector(x, (neighbour.Floor + 1) * 256, z, neighbour.RoomBelow, level.Data, floorData);
                            }
                            while (neighbour.RoomBelow != 255);
                        }
                        CreateSecretTrigger(level, secret, room, floorData, neighbour);
                        if (Settings.DevelopmentMode)
                        {
                            Debug.WriteLine(string.Format(_edgeInfoMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                        }
                    }
                }
            }

            return true;
        }

        private bool IsInvalidNeighbour(TRRoomSector neighbour)
        {
            return neighbour.Floor == -127 && neighbour.Ceiling == -127; // Inside a wall
        }

        private bool CreateSecretTrigger(TR1CombinedLevel level, TRSecretPlacement<TREntities> secret, short room, FDControl floorData, TRRoomSector sector)
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
                            if (actionItem.Parameter < level.Data.NumEntities && actionItem.Parameter != secret.DoorIndex)
                            {
                                existingActions.Add(actionItem); // Add it anyway for testing
                                Debug.WriteLine(string.Format(_triggerWarningMsg, actionItem.Parameter, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                            }
                        }
                        else if (secret.TriggerMask == TRSecretPlacement<TREntities>.FullActivation)
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

            floorData.Entries[sector.FDIndex].Add(trigger);

            return true;
        }

        internal class SecretProcessor : AbstractProcessorThread<TR1SecretRandomizer>
        {
            private static readonly Dictionary<TREntities, TREntities> _secretModels = TR1EntityUtilities.GetSecretModels();
            private static readonly Dictionary<TREntities, TREntities> _modelReplacements = TR1EntityUtilities.GetSecretReplacements();

            // Move this to Gamestring Rando once implemented
            private static readonly Dictionary<TREntities, string> _pickupNames = new Dictionary<TREntities, string>
            {
                [TREntities.SecretAnkh_M_H] = "Secret Ankh",
                [TREntities.SecretGoldBar_M_H] = "Secret Gold Bar",
                [TREntities.SecretGoldIdol_M_H] = "Secret Gold Idol",
                [TREntities.SecretLeadBar_M_H] = "Secret Lead Bar",
                [TREntities.SecretScion_M_H] = "Secret Scion"
            };

            private readonly Dictionary<TR1CombinedLevel, TRSecretModelAllocation<TREntities>> _importAllocations;

            internal override int LevelCount => _importAllocations.Count;

            internal SecretProcessor(TR1SecretRandomizer outer)
                : base(outer)
            {
                _importAllocations = new Dictionary<TR1CombinedLevel, TRSecretModelAllocation<TREntities>>();
            }

            internal void AddLevel(TR1CombinedLevel level)
            {
                _importAllocations.Add(level, new TRSecretModelAllocation<TREntities>());
            }

            protected override void StartImpl()
            {
                List<TREntities> availableTypes = _secretModels.Keys.ToList();
                foreach (TR1CombinedLevel level in _importAllocations.Keys)
                {
                    if (level.IsAssault)
                    {
                        continue;
                    }

                    TRSecretModelAllocation<TREntities> allocation = _importAllocations[level];

                    // Work out which models are available to replace as secret pickups.
                    // We exclude current puzzle/key items from the available switching pool.
                    List<TRModel> models = level.Data.Models.ToList();

                    foreach (TREntities puzzleType in _modelReplacements.Keys)
                    {
                        if (models.Find(m => m.ID == (uint)puzzleType) == null)
                        {
                            allocation.AvailablePickupModels.Add(puzzleType);
                        }
                    }

                    TREntities modelType = _outer.Settings.UseRandomSecretModels
                        ? availableTypes[_outer._generator.Next(0, availableTypes.Count)]
                        : TR1EntityUtilities.GetBestLevelSecretModel(level.Name);
                    allocation.ImportModels.Add(modelType);
                }
            }

            protected override void ProcessImpl()
            {
                foreach (TR1CombinedLevel level in _importAllocations.Keys)
                {
                    if (!level.IsAssault)
                    {
                        TRSecretModelAllocation<TREntities> allocation = _importAllocations[level];

                        // Get the artefacts into the level and refresh the model list
                        TR1ModelImporter importer = new TR1ModelImporter
                        {
                            Level = level.Data,
                            LevelName = level.Name,
                            EntitiesToImport = allocation.ImportModels,
                            DataFolder = _outer.GetResourcePath(@"TR1\Models"),
                        };

                        importer.Import();

                        List<TRModel> models = level.Data.Models.ToList();
                        List<TRSpriteSequence> sequences = level.Data.SpriteSequences.ToList();

                        // Redefine the artefacts as puzzle models
                        foreach (TREntities secretModelType in allocation.ImportModels)
                        {
                            TREntities secretPickupType = _secretModels[secretModelType];

                            TREntities puzzleModelType = allocation.AvailablePickupModels.First();
                            TREntities puzzlePickupType = _modelReplacements[puzzleModelType];

                            models.Find(m => m.ID == (uint)secretModelType).ID = (uint)puzzleModelType;
                            sequences.Find(s => s.SpriteID == (int)secretPickupType).SpriteID = (int)puzzlePickupType;

                            if (secretModelType == TREntities.SecretScion_M_H && _outer.Are3DPickupsEnabled())
                            {
                                // T1M embeds scions into the ground when they are puzzle/key types in 3D mode,
                                // so we counteract that here to avoid uncollectable items.
                                TRMesh scionMesh = TRMeshUtilities.GetModelFirstMesh(level.Data, puzzleModelType);
                                foreach (TRVertex vertex in scionMesh.Vertices)
                                {
                                    vertex.Y -= 90;
                                }
                            }

                            // Remove this puzzle type from the available pool
                            allocation.AvailablePickupModels.RemoveAt(0);

                            // Make the pickup type available to assign to items
                            allocation.AssignedPickupModels.Add(puzzlePickupType);

                            // Assign a name for the script
                            _outer.SetPuzzleTypeName(level, puzzlePickupType, _pickupNames[secretModelType]);
                        }

                        level.Data.Models = models.ToArray();
                        level.Data.NumModels = (uint)models.Count;
                    }

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }

            internal void ApplyRandomization()
            {
                foreach (TR1CombinedLevel level in _importAllocations.Keys)
                {
                    if (!level.IsAssault)
                    {
                        // Get rid of existing secret triggers
                        _outer.RemoveDefaultSecrets(level);

                        TRSecretModelAllocation<TREntities> allocation = _importAllocations[level];

                        // Reward rooms can be conditionally chosen based on level state after placing secrets,
                        // but we need to make a placholder for door indices and masks to create those secrets.
                        TRSecretRoom<TREntity> rewardRoom = _outer.MakePlaceholderRewardRoom(level);

                        // Pass the list of artefacts we can use as pickups along with the temporary reward
                        // room to the secret placers.
                        if (_outer.Settings.DevelopmentMode)
                        {
                            _outer.PlaceAllSecrets(level, allocation.AssignedPickupModels, rewardRoom);
                        }
                        else
                        {
                            _outer.RandomizeSecrets(level, allocation.AssignedPickupModels, rewardRoom);
                        }

                        // Convert the placeholder reward room into an actual room now that secrets are positioned.
                        _outer.ActualiseRewardRoom(level, rewardRoom);

                        _outer.SaveLevel(level);
                    }

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }
        }
    }
}