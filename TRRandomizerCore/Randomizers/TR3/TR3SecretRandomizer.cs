using Newtonsoft.Json;
using System.Diagnostics;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRGE.Core.Item.Enums;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;
using SC = TRRandomizerCore.Randomizers.SecretConsts;

namespace TRRandomizerCore.Randomizers;

public class TR3SecretRandomizer : BaseTR3Randomizer, ISecretRandomizer
{
    private static readonly ushort _devModeSecretCount = 6;

    private readonly Dictionary<string, List<Location>> _locations, _unarmedLocations;
    private readonly LocationPicker _routePicker;
    private SecretPicker<TR3Entity> _secretPicker;

    internal TR3TextureMonitorBroker TextureMonitor { get; set; }
    public ItemFactory<TR3Entity> ItemFactory { get; set; }
    public IMirrorControl Mirrorer { get; set; }

    public TR3SecretRandomizer()
    {
        _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\locations.json"));
        _unarmedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\unarmed_locations.json"));
        _routePicker = new(GetResourcePath(@"TR3\Locations\routes.json"));
    }

    public IEnumerable<string> GetPacks()
    {
        return _locations.Values
            .SelectMany(v => v.Select(l => l.PackID))
            .Where(a => a != Location.DefaultPackID)
            .Distinct();
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _secretPicker = new()
        {
            Settings = Settings,
            Generator = _generator,
            ItemFactory = ItemFactory,
            Mirrorer = Mirrorer,
            RouteManager = _routePicker,
        };

        SetMessage("Randomizing secrets - loading levels");

        List<SecretProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new SecretProcessor(this));
        }

        List<TR3CombinedLevel> levels = new(Levels.Count);
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

        _processingException?.Throw();
    }

    private static void RemoveDefaultSecrets(TR3CombinedLevel level)
    {
        FDControl floorData = new();
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

    private TRSecretRoom<TR2Entity> MakePlaceholderRewardRoom(TR3CombinedLevel level)
    {
        TRSecretRoom<TR2Entity> rewardRoom = null;
        string mappingPath = @"TR3\SecretMapping\" + level.Name + "-SecretMapping.json";
        if (ResourceExists(mappingPath))
        {
            // Trigger activation masks have 5 bits so we need a specific number of doors to match.
            // For development mode, test the maximum.
            double countedSecrets = Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets;
            int requiredDoors = (int)Math.Ceiling(countedSecrets / TRConsts.MaskBits);

            // Make the doors and store the entity indices for the secret triggers
            rewardRoom = new TRSecretRoom<TR2Entity>
            {
                DoorIndices = new List<int>()
            };
            for (int i = 0; i < requiredDoors; i++)
            {
                TR3Entity door = ItemFactory.CreateItem(level.Name, level.Data.Entities);
                rewardRoom.DoorIndices.Add(level.Data.Entities.IndexOf(door));
            }
        }

        return rewardRoom;
    }

    private void ActualiseRewardRoom(TR3CombinedLevel level, TRSecretRoom<TR2Entity> placeholder)
    {
        TR3SecretMapping secretMapping = TR3SecretMapping.Get(GetResourcePath($@"TR3\SecretMapping\{level.Name}-SecretMapping.json"), IsJPVersion);
        if (secretMapping == null)
        {
            return;
        }
        
        // Are any rooms enforced based on level specifics?
        TRSecretRoom<TR3Entity> rewardRoom = secretMapping.Rooms.Find(r => r.HasUsageCondition);
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
            TR3Entity door = rewardRoom.Doors[i];
            if (door.Room < 0)
            {
                door.Room = roomIndex;
            }
            level.Data.Entities[doorIndex] = door;

            // If it's a trapdoor, we need to make a dummy trigger for it
            if (TR3TypeUtilities.IsTrapdoor(door.TypeID))
            {
                CreateTrapdoorTrigger(door, (ushort)doorIndex, level.Data);
            }
        }

        // Spread the rewards out fairly evenly across each defined position in the new room.
        int rewardPositionCount = rewardRoom.RewardPositions.Count;
        for (int i = 0; i < secretMapping.RewardEntities.Count; i++)
        {
            TR3Entity item = level.Data.Entities[secretMapping.RewardEntities[i]];
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
            double countedSecrets = Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets;
            rewardRoom.CameraIndices = new List<int>();
            List<TRCamera> cameras = level.Data.Cameras.ToList();
            for (int i = 0; i < countedSecrets; i++)
            {
                rewardRoom.CameraIndices.Add(cameras.Count);
                cameras.Add(rewardRoom.Cameras[i % rewardRoom.Cameras.Count]);
            }

            level.Data.Cameras = cameras.ToArray();
            level.Data.NumCameras = (uint)cameras.Count;

            FDControl floorData = new();
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
                            Parameter = (ushort)rewardRoom.DoorIndices[0]
                        });
                    }
                }
            }

            // Write back the camera triggers
            floorData.WriteToLevel(level.Data);
        }
    }

    private static void CreateTrapdoorTrigger(TR3Entity door, ushort doorIndex, TR3Level level)
    {
        FDControl floorData = new();
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
                new() {
                    TrigAction = FDTrigAction.Object,
                    Parameter = doorIndex
                }
            }
        });

        floorData.WriteToLevel(level);
    }

    private void PlaceAllSecrets(TR3CombinedLevel level, List<TR3Type> pickupTypes, TRSecretRoom<TR2Entity> rewardRoom)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        List<Location> locations = _locations[level.Name];

        TRSecretPlacement<TR3Type> secret = new();
        int pickupIndex = 0;
        ushort secretIndex = 0;
        ushort countedSecrets = _devModeSecretCount; // For dev mode test the max number of secrets in TR3
        foreach (Location location in locations)
        {
            if (Mirrorer.IsMirrored(level.Name) && location.LevelState == LevelState.NotMirrored)
                continue;

            if (!Mirrorer.IsMirrored(level.Name) && location.LevelState == LevelState.Mirrored)
                continue;

            secret.Location = location;
            secret.EntityIndex = (ushort)ItemFactory.GetNextIndex(level.Name, level.Data.Entities, true);
            secret.SecretIndex = (ushort)(secretIndex % countedSecrets); // Cycle through each secret number
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count]; // Cycle through the types

            // #238 Point this secret to a specific camera and look-at target if applicable.
            if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
            {
                secret.CameraIndex = (ushort)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                secret.CameraTarget = (ushort)rewardRoom.DoorIndices[0];
            }

            secret.SetMaskAndDoor(countedSecrets, rewardRoom.DoorIndices);
            PlaceSecret(level, secret, floorData);

            // This will either make a new entity or repurpose an old one
            TR3Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location, true);
            entity.TypeID = secret.PickupType;

            secretIndex++;
            pickupIndex++;
        }

        floorData.WriteToLevel(level.Data);

        AddDamageControl(level, pickupTypes, locations);
    }

    private void RandomizeSecrets(TR3CombinedLevel level, List<TR3Type> pickupTypes, TRSecretRoom<TR2Entity> rewardRoom)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        List<Location> locations = _locations[level.Name];
        locations.Shuffle(_generator);

        _secretPicker.SectorAction = loc
            => FDUtilities.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room, level.Data, floorData);
        _secretPicker.PlacementTestAction = loc
            => TestSecretPlacement(level, loc, floorData);

        _routePicker.RoomInfos = level.Data.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();
        _routePicker.Initialise(level.Name, locations, Settings, _generator);

        List<Location> pickedLocations = _secretPicker.GetLocations(locations, Mirrorer.IsMirrored(level.Name), level.Script.NumSecrets);

        int pickupIndex = 0;
        TRSecretPlacement<TR3Type> secret = new();
        for (int i = 0; i < level.Script.NumSecrets; i++)
        {
            Location location = pickedLocations[i];
            secret.Location = location;
            secret.EntityIndex = (ushort)ItemFactory.GetNextIndex(level.Name, level.Data.Entities);
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count];

            if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
            {
                secret.CameraIndex = (ushort)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                secret.CameraTarget = (ushort)rewardRoom.DoorIndices[0];
            }

            secret.SetMaskAndDoor(level.Script.NumSecrets, rewardRoom.DoorIndices);
            PlaceSecret(level, secret, floorData);

            // This will either make a new entity or repurpose an old one. Ensure it is locked
            // to prevent item rando from potentially treating it as a key item.
            TR3Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location);
            entity.TypeID = secret.PickupType;

            secret.SecretIndex++;
            pickupIndex++;
        }

        floorData.WriteToLevel(level.Data);

        AddDamageControl(level, pickupTypes, pickedLocations);
        _secretPicker.FinaliseSecretPool(pickedLocations, level.Name);
    }

    private void AddDamageControl(TR3CombinedLevel level, List<TR3Type> pickupTypes, List<Location> locations)
    {
        // If we have used a secret that requires damage, add a large medi to an unarmed level
        // weapon location.
        if (locations.Any(l => l.RequiresDamage) && _unarmedLocations.ContainsKey(level.Name))
        {
            if (ItemFactory.CanCreateItem(level.Name, level.Data.Entities, Settings.DevelopmentMode))
            {
                List<Location> pool = _unarmedLocations[level.Name];
                Location location = pool[_generator.Next(0, pool.Count)];

                TR3Entity medi = ItemFactory.CreateItem(level.Name, level.Data.Entities, location, Settings.DevelopmentMode);
                medi.TypeID = TR3Type.LargeMed_P;
            }
            else
            {
                level.Script.AddStartInventoryItem(TR3Items.LargeMedi);
            }
        }

        // If we have also used a secret that requires damage and is glitched, add something to the
        // top ring to allow medi dupes.
        if (locations.Any(l => l.RequiresDamage && l.RequiresGlitch))
        {
            // If we have a spare model slot, duplicate one of the artefacts into this so that
            // we can add a hint with the item name. Otherwise, just re-use a puzzle item.
            List<TRModel> models = level.Data.Models.ToList();
            Dictionary<TR3Type, TR3Type> artefacts = TR3TypeUtilities.GetArtefactReplacements();

            TR3Type availablePickupType = default;
            TR3Type availableMenuType = default;
            foreach (TR3Type pickupType in artefacts.Keys)
            {
                TR3Type menuType = artefacts[pickupType];
                if (models.Find(m => m.ID == (uint)menuType) == null)
                {
                    availablePickupType = pickupType;
                    availableMenuType = menuType;
                    break;
                }
            }

            if (availableMenuType != default)
            {
                // We have a free slot, so duplicate a model
                TR3Type baseArtefact = pickupTypes[_generator.Next(0, pickupTypes.Count)];
                TRModel artefactMenuModel = models.Find(m => m.ID == (uint)artefacts[baseArtefact]);
                models.Add(new TRModel
                {
                    Animation = artefactMenuModel.Animation,
                    FrameOffset = artefactMenuModel.FrameOffset,
                    ID = (uint)availableMenuType,
                    MeshTree = artefactMenuModel.MeshTree,
                    NumMeshes = artefactMenuModel.NumMeshes,
                    StartingMesh = artefactMenuModel.StartingMesh
                });

                level.Data.Models = models.ToArray();
                level.Data.NumModels++;

                // Add a script name - pull from GamestringRando once translations completed
                SetPuzzleTypeName(level, availablePickupType, "Infinite Medi Packs");
            }
            else
            {
                // Otherwise, just use something already available (no change in name)
                availablePickupType = pickupTypes[_generator.Next(0, pickupTypes.Count)];
            }

            level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(availablePickupType));
        }
    }

    private static void SetPuzzleTypeName(TR3CombinedLevel level, TR3Type itemType, string name)
    {
        if (TR3TypeUtilities.IsKeyType(itemType))
        {
            level.Script.Keys[itemType - TR3Type.Key1_P] = name;
        }
        else if (TR3TypeUtilities.IsPuzzleType(itemType))
        {
            level.Script.Puzzles[itemType - TR3Type.Puzzle1_P] = name;
        }
        else if (TR3TypeUtilities.IsQuestType(itemType))
        {
            level.Script.Pickups[itemType - TR3Type.Quest1_P] = name;
        }
    }

    private bool TestSecretPlacement(TR3CombinedLevel level, Location location, FDControl floorData)
    {
        // Check if this secret is being added to a flipped room, as that won't work
        for (int i = 0; i < level.Data.NumRooms; i++)
        {
            if (level.Data.Rooms[i].AlternateRoom == location.Room)
            {
                if (Settings.DevelopmentMode)
                {
                    // Place it anyway in dev mode to allow relocating
                    Debug.WriteLine(string.Format(SC.FlipMapErrorMsg, level.Name, location.X, location.Y, location.Z, location.Room));
                    break;
                }
                else
                {
                    return false;
                }
            }
        }

        // Get the sector and check if it is shared with a trapdoor or bridge, as these won't work either.
        TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level.Data, floorData);
        foreach (TR3Entity otherEntity in level.Data.Entities)
        {
            TR3Type type = otherEntity.TypeID;
            if (location.Room == otherEntity.Room && (TR3TypeUtilities.IsTrapdoor(type) || TR3TypeUtilities.IsBridge(type)))
            {
                TRRoomSector otherSector = FDUtilities.GetRoomSector(otherEntity.X, otherEntity.Y, otherEntity.Z, otherEntity.Room, level.Data, floorData);
                if (otherSector == sector)
                {
                    if (Settings.DevelopmentMode)
                    {
                        Debug.WriteLine(string.Format(SC.TrapdoorLocationMsg, level.Name, location.X, location.Y, location.Z, location.Room));
                    }
                    return false;
                }
            }
        }

        if (!TestTriggerPlacement(level, location, (short)location.Room, sector, floorData))
        {
            return false;
        }

        // If the room has a flipmap, make sure to test the trigger there too.
        short altRoom = level.Data.Rooms[location.Room].AlternateRoom;
        if (altRoom != -1)
        {
            sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, altRoom, level.Data, floorData);
            if (!TestTriggerPlacement(level, location, altRoom, sector, floorData))
            {
                return false;
            }

            if (Settings.DevelopmentMode)
            {
                Debug.WriteLine(string.Format(SC.FlipMapWarningMsg, level.Name, location.X, location.Y, location.Z, altRoom));
            }
        }

        return true;
    }

    private bool TestTriggerPlacement(TR3CombinedLevel level, Location location, short room, TRRoomSector sector, FDControl floorData)
    {
        if (!location.Validated && LocationUtilities.HasAnyTrigger(sector, floorData))
        {
            // There is already a trigger here and the location hasn't been marked as being
            // safe to move the action items to the new pickup trigger.
            if (Settings.DevelopmentMode)
            {
                Debug.WriteLine(string.Format(SC.InvalidLocationMsg, level.Name, location.X, location.Y, location.Z, room));
            }
            return false;
        }
        return true;
    }


    private void PlaceSecret(TR3CombinedLevel level, TRSecretPlacement<TR3Type> secret, FDControl floorData)
    {
        // This assumes TestTriggerPlacement has already been called and passed.
        TRRoomSector sector = FDUtilities.GetRoomSector(secret.Location.X, secret.Location.Y, secret.Location.Z, (short)secret.Location.Room, level.Data, floorData);
        CreateSecretTriggers(level, secret, (short)secret.Location.Room, floorData, sector);

        short altRoom = level.Data.Rooms[secret.Location.Room].AlternateRoom;
        if (altRoom != -1)
        {
            sector = FDUtilities.GetRoomSector(secret.Location.X, secret.Location.Y, secret.Location.Z, altRoom, level.Data, floorData);
            CreateSecretTriggers(level, secret, altRoom, floorData, sector);
        }
    }

    private void CreateSecretTriggers(TR3CombinedLevel level, TRSecretPlacement<TR3Type> secret, short room, FDControl floorData, TRRoomSector baseSector)
    {
        // Try to make the primary trigger
        CreateSecretTrigger(level, secret, room, floorData, baseSector);

        // Check neighbouring sectors if we are very close to tile edges. We scan 8 locations around
        // the secret's position based on the edge tolerance and see if the sector has changed.
        HashSet<TRRoomSector> processedSectors = new() { baseSector };
        for (int xNorm = -1; xNorm < 2; xNorm++)
        {
            for (int zNorm = -1; zNorm < 2; zNorm++)
            {
                if (xNorm == 0 && zNorm == 0) continue; // Primary trigger's sector

                int x = secret.Location.X + xNorm * SC.TriggerEdgeLimit;
                int z = secret.Location.Z + zNorm * SC.TriggerEdgeLimit;
                TRRoomSector neighbour = FDUtilities.GetRoomSector(x, secret.Location.Y, z, room, level.Data, floorData);

                // Process each unique sector only once and if it's a valid neighbour, add the extra trigger.
                // We test neighbouring sector heights as Lara doesn't clip up in TR3 unlike TR1 if she is
                // against the wall, so this avoids unnecessary extra FD.
                if (processedSectors.Add(neighbour)
                    && !IsInvalidNeighbour(baseSector, neighbour)
                    && Math.Abs(secret.Location.Y - LocationUtilities.GetCornerHeight(neighbour, floorData, x, z)) < TRConsts.Step1)
                {
                    CreateSecretTrigger(level, secret, room, floorData, neighbour);
                    if (Settings.DevelopmentMode)
                    {
                        Debug.WriteLine(string.Format(SC.EdgeInfoMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                    }
                }
            }
        }
    }

    private static bool IsInvalidNeighbour(TRRoomSector baseSector, TRRoomSector neighbour)
    {
        return (neighbour.Floor == -127 && neighbour.Ceiling == -127) // Inside a wall
            || (neighbour.RoomBelow != baseSector.RoomBelow)          // Mid-air
            ||
            (
                (neighbour.BoxIndex & 0x7FF0) >> 4 == 2047            // Neighbour is a slope
                && (baseSector.BoxIndex & 0x7FF0) >> 4 != 2047        // But the base sector isn't
            );
    }

    private void CreateSecretTrigger(TR3CombinedLevel level, TRSecretPlacement<TR3Type> secret, short room, FDControl floorData, TRRoomSector sector)
    {
        if (sector.FDIndex == 0)
        {
            floorData.CreateFloorData(sector);
        }

        // Make a new pickup trigger
        FDTriggerEntry trigger = new()
        {
            Setup = new() { Value = 4 },
            TrigSetup = new()
            {
                Value = 15872,
                Mask = secret.TriggerMask
            },
            TrigType = FDTrigType.Pickup,
            TrigActionList = new()
            {
                new()
                {
                    TrigAction = FDTrigAction.Object,
                    Parameter = secret.EntityIndex
                },
                new()
                {
                    TrigAction = FDTrigAction.SecretFound,
                    Parameter = secret.SecretIndex
                }
            }
        };

        if (secret.TriggersDoor)
        {
            trigger.TrigActionList.Add(new()
            {
                TrigAction = FDTrigAction.Object,
                Parameter = secret.DoorIndex
            });
        }

        // Move any existing action list items to the new trigger and remove the old one. We can only
        // move Object actions if the mask on this trigger is full.
        if (floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry existingTrigger)
        {
            List<FDActionListItem> existingActions = new();
            foreach (FDActionListItem actionItem in existingTrigger.TrigActionList)
            {
                if (actionItem.TrigAction == FDTrigAction.Object)
                {
                    if (Settings.DevelopmentMode)
                    {
                        existingActions.Add(actionItem); // Add it anyway for testing
                        Debug.WriteLine(string.Format(SC.TriggerWarningMsg, actionItem.Parameter, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                    }
                    else if (secret.TriggerMask == TRConsts.FullMask)
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
    }

    internal class SecretProcessor : AbstractProcessorThread<TR3SecretRandomizer>
    {
        private static readonly Dictionary<TR3Type, TR3Type> _artefactPickups = TR3TypeUtilities.GetArtefactPickups();
        private static readonly Dictionary<TR3Type, TR3Type> _artefactReplacements = TR3TypeUtilities.GetArtefactReplacements();

        // Move this to Gamestring Rando once implemented
        private static readonly Dictionary<TR3Type, string> _pickupNames = new()
        {
            [TR3Type.Infada_P] = "Secret Infada Stone",
            [TR3Type.OraDagger_P] = "Secret Ora Dagger",
            [TR3Type.Element115_P] = "Secret Element 115",
            [TR3Type.EyeOfIsis_P] = "Secret Eye of Isis",
            [TR3Type.Quest1_P] = "Secret Serpent Stone",
            [TR3Type.Quest2_P] = "Secret Hand of Rathmore"
        };

        private readonly Dictionary<TR3CombinedLevel, TRSecretModelAllocation<TR3Type>> _importAllocations;

        internal override int LevelCount => _importAllocations.Count;

        internal SecretProcessor(TR3SecretRandomizer outer)
            : base(outer)
        {
            _importAllocations = new Dictionary<TR3CombinedLevel, TRSecretModelAllocation<TR3Type>>();
        }

        internal void AddLevel(TR3CombinedLevel level)
        {
            _importAllocations.Add(level, new TRSecretModelAllocation<TR3Type>());
        }

        protected override void StartImpl()
        {
            foreach (TR3CombinedLevel level in _importAllocations.Keys)
            {
                if (!level.HasSecrets && !_outer.Settings.DevelopmentMode)
                {
                    continue;
                }

                TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

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
                    models.Find(m => m.ID == (uint)TR3Type.Quest1_P).ID = (uint)TR3Type.Key3_P;
                    models.Find(m => m.ID == (uint)TR3Type.Quest1_M_H).ID = (uint)TR3Type.Key3_M_H;
                    level.Script.Keys[2] = level.Script.Pickups[0];
                    level.Script.SetStartInventoryItems(new Dictionary<TR3Items, int>
                    {
                        [TR3Items.Key3] = 1
                    });
                }

                foreach (TR3Type puzzleType in _artefactReplacements.Keys)
                {
                    if (models.Find(m => m.ID == (uint)puzzleType) == null)
                    {
                        allocation.AvailablePickupModels.Add(puzzleType);
                    }
                }

                List<TR3Type> artefactTypes = _artefactPickups.Keys.ToList();
                for (int i = artefactTypes.Count - 1; i >= 0; i--)
                {
                    TR3Type artefactType = artefactTypes[i];
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
                    TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

                    // Get the artefacts into the level and refresh the model list
                    TextureMonitor<TR3Type> monitor = _outer.TextureMonitor.CreateMonitor(level.Name, allocation.ImportModels);
                    TR3ModelImporter importer = new()
                    {
                        Level = level.Data,
                        LevelName = level.Name,
                        EntitiesToImport = allocation.ImportModels,
                        DataFolder = _outer.GetResourcePath(@"TR3\Models"),
                        TexturePositionMonitor = monitor
                    };

                    importer.Import();

                    List<TRModel> models = level.Data.Models.ToList();

                    // Redefine the artefacts as puzzle models otherwise the level ends on pickup
                    foreach (TR3Type artefactPickupType in allocation.ImportModels)
                    {
                        TR3Type artefactMenuType = _artefactPickups[artefactPickupType];

                        TR3Type puzzlePickupType = allocation.AvailablePickupModels.First();
                        TR3Type puzzleMenuType = _artefactReplacements[puzzlePickupType];

                        models.Find(m => m.ID == (uint)artefactPickupType).ID = (uint)puzzlePickupType;

                        // #277 Most levels (beyond India) have the artefacts as menu models so we need
                        // to duplicate the models instead of replacing them, otherwise the carried-over
                        // artefacts from previous levels are invisible.
                        TRModel menuModel = models.Find(m => m.ID == (uint)artefactMenuType);
                        models.Add(new TRModel
                        {
                            Animation = menuModel.Animation,
                            FrameOffset = menuModel.FrameOffset,
                            ID = (uint)puzzleMenuType,
                            MeshTree = menuModel.MeshTree,
                            NumMeshes = menuModel.NumMeshes,
                            StartingMesh = menuModel.StartingMesh
                        });

                        // Remove this puzzle type from the available pool
                        allocation.AvailablePickupModels.RemoveAt(0);

                        // Make the pickup type available to assign to items
                        allocation.AssignedPickupModels.Add(puzzlePickupType);

                        // Assign a name for the script
                        SetPuzzleTypeName(level, puzzlePickupType, _pickupNames[artefactPickupType]);

                        // Tell the texture monitor that these artefacts are puzzle items
                        monitor.EntityMap[artefactPickupType] = puzzlePickupType;
                        monitor.EntityMap[artefactMenuType] = puzzleMenuType;
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
            foreach (TR3CombinedLevel level in _importAllocations.Keys)
            {
                // Get rid of existing secret triggers
                RemoveDefaultSecrets(level);

                // Only create new secrets if applicable
                if (level.HasSecrets || _outer.Settings.DevelopmentMode)
                {
                    TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

                    // Reward rooms can be conditionally chosen based on level state after placing secrets,
                    // but we need to make a placholder for door indices and masks to create those secrets.
                    TRSecretRoom<TR2Entity> rewardRoom = _outer.MakePlaceholderRewardRoom(level);

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
