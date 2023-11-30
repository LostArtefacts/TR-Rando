using Newtonsoft.Json;
using System.Diagnostics;
using TREnvironmentEditor.Model.Types;
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
using TRRandomizerCore.Utilities;
using SC = TRRandomizerCore.Randomizers.SecretConsts;

namespace TRRandomizerCore.Randomizers;

public class TR1SecretRandomizer : BaseTR1Randomizer, ISecretRandomizer
{
    private static readonly ushort _maxSecretCount = 5;

    private readonly Dictionary<string, List<Location>> _locations, _unarmedLocations;
    private readonly LocationPicker _routePicker;
    private SecretPicker<TR1Entity> _secretPicker;

    public ItemFactory<TR1Entity> ItemFactory { get; set; }
    public IMirrorControl Mirrorer { get; set; }

    public TR1SecretRandomizer()
    {
        _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\locations.json"));
        _unarmedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\unarmed_locations.json"));
        _routePicker = new(GetResourcePath(@"TR1\Locations\routes.json"));
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

        if (!Settings.UseSecretPack)
        {
            SetSecretCounts();
        }

        SetMessage("Randomizing secrets - loading levels");

        List<SecretProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new SecretProcessor(this));
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

        TR1Script script = ScriptEditor.Script as TR1Script;
        script.FixPyramidSecretTrigger = false;

        if (Settings.UseRecommendedCommunitySettings)
        {
            script.Enable3dPickups = false;
            if (Settings.HardSecrets || Settings.GlitchedSecrets)
            {
                script.EnableBuffering = true;
            }
        }

        if (Settings.GlitchedSecrets)
        {
            script.FixDescendingGlitch = false;
            script.FixQwopGlitch = false;
            script.FixWallJumpGlitch = false;
        }

        ScriptEditor.SaveScript();
    }

    private bool Are3DPickupsEnabled()
    {
        return !Settings.UseRecommendedCommunitySettings
            && (ScriptEditor.Script as TR1Script).Enable3dPickups;
    }

    private void SetSecretCounts()
    {
        List<TR1ScriptedLevel> levels = Levels.FindAll(l => !l.Is(TR1LevelNames.ASSAULT));

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

    private static void RemoveDefaultSecrets(TR1CombinedLevel level)
    {
        FDControl floorData = new();
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

    private TRSecretRoom<TR1Entity> MakePlaceholderRewardRoom(TR1CombinedLevel level)
    {
        TRSecretRoom<TR1Entity> rewardRoom = null;
        string mappingPath = @"TR1\SecretMapping\" + level.Name + "-SecretMapping.json";
        if (ResourceExists(mappingPath))
        {
            // Trigger activation masks have 5 bits so we need a specific number of doors to match.
            // Limited to 1 (5 secrets for the time being).
            //double countedSecrets = Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets;
            int requiredDoors = 1;// (int)Math.Ceiling(countedSecrets / TRSecretPlacement<TREntities>.MaskBits);

            // Make the doors and store the entity indices for the secret triggers
            rewardRoom = new()
            {
                DoorIndices = new()
            };
            
            for (int i = 0; i < requiredDoors; i++)
            {
                TR1Entity door = ItemFactory.CreateItem(level.Name, level.Data.Entities);
                rewardRoom.DoorIndices.Add(level.Data.Entities.IndexOf(door));
            }
        }

        return rewardRoom;
    }

    private void ActualiseRewardRoom(TR1CombinedLevel level, TRSecretRoom<TR1Entity> placeholder)
    {
        TRSecretMapping<TR1Entity> secretMapping = TRSecretMapping<TR1Entity>.Get(GetResourcePath(@"TR1\SecretMapping\" + level.Name + "-SecretMapping.json"));
        if (secretMapping == null || secretMapping.Rooms.Count == 0)
        {
            return;
        }

        // Are any rooms enforced based on level specifics?
        TRSecretRoom<TR1Entity> rewardRoom = secretMapping.Rooms.Find(r => r.HasUsageCondition);
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
            TR1Entity door = rewardRoom.Doors[i];
            if (door.Room < 0)
            {
                door.Room = roomIndex;
            }
            level.Data.Entities[doorIndex] = door;

            // If it's a trapdoor, we need to make a dummy trigger for it
            if (TR1TypeUtilities.IsTrapdoor(door.TypeID))
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
            TR1Entity item = level.Data.Entities[rewardEntities[i]];
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
            if (rewardRoom.CameraTarget != null && ItemFactory.CanCreateItem(level.Name, level.Data.Entities))
            {
                TR1Entity target = ItemFactory.CreateItem(level.Name, level.Data.Entities, rewardRoom.CameraTarget);
                target.TypeID = TR1Type.CameraTarget_N;
                cameraTarget = (ushort)level.Data.Entities.IndexOf(target);
            }
            else
            {
                cameraTarget = (ushort)rewardRoom.DoorIndices[0];
            }

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
                            Parameter = cameraTarget
                        });
                    }
                }
            }

            // Write back the camera triggers
            floorData.WriteToLevel(level.Data);
        }
    }

    private static void CreateTrapdoorTrigger(TR1Entity door, ushort doorIndex, TR1Level level)
    {
        new EMTriggerFunction
        {
            Locations = new()
            {
                new()
                {
                    X = door.X,
                    Y = door.Y,
                    Z = door.Z,
                    Room = door.Room
                }
            },
            Trigger = new()
            {
                TrigType = FDTrigType.Dummy,
                Actions = new()
                {
                    new()
                    {
                        Parameter = (short)doorIndex
                    }
                }
            }
        }.ApplyToLevel(level);
    }

    private void PlaceAllSecrets(TR1CombinedLevel level, List<TR1Type> pickupTypes, TRSecretRoom<TR1Entity> rewardRoom)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        List<Location> locations = _locations[level.Name];

        TRSecretPlacement<TR1Type> secret = new();
        int pickupIndex = 0;
        ushort secretIndex = 0;
        ushort countedSecrets = _maxSecretCount;
        foreach (Location location in locations)
        {
            if (Mirrorer.IsMirrored(level.Name) && location.LevelState == LevelState.NotMirrored)
                continue;

            if (!Mirrorer.IsMirrored(level.Name) && location.LevelState == LevelState.Mirrored)
                continue;

            secret.Location = location;
            secret.EntityIndex = (ushort)ItemFactory.GetNextIndex(level.Name, level.Data.Entities, true);
            secret.SecretIndex = (ushort)(secretIndex % countedSecrets);
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count];

            if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
            {
                secret.CameraIndex = (ushort)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                secret.CameraTarget = (ushort)rewardRoom.DoorIndices[0];
            }

            secret.SetMaskAndDoor(countedSecrets, rewardRoom.DoorIndices);
            PlaceSecret(level, secret, floorData);

            // This will either make a new entity or repurpose an old one
            TR1Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location, true);
            entity.TypeID = secret.PickupType;

            secretIndex++;
            pickupIndex++;
        }

        floorData.WriteToLevel(level.Data);

        AddDamageControl(level, locations);
    }

    private void RandomizeSecrets(TR1CombinedLevel level, List<TR1Type> pickupTypes, TRSecretRoom<TR1Entity> rewardRoom)
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
        TRSecretPlacement<TR1Type> secret = new();
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
            TR1Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location);
            entity.TypeID = secret.PickupType;

            secret.SecretIndex++;
            pickupIndex++;
        }

        floorData.WriteToLevel(level.Data);

        AddDamageControl(level, pickedLocations);
        _secretPicker.FinaliseSecretPool(pickedLocations, level.Name);
    }

    private void AddDamageControl(TR1CombinedLevel level, List<Location> locations)
    {
        // If we have used a secret that requires damage, add a large medi to an unarmed level
        // weapon location.
        if (locations.Any(l => l.RequiresDamage) && _unarmedLocations.ContainsKey(level.Name))
        {
            if (ItemFactory.CanCreateItem(level.Name, level.Data.Entities, Settings.DevelopmentMode))
            {
                List<Location> pool = _unarmedLocations[level.Name];
                Location location = pool[_generator.Next(0, pool.Count)];

                TR1Entity medi = ItemFactory.CreateItem(level.Name, level.Data.Entities, location, Settings.DevelopmentMode);
                medi.TypeID = TR1Type.LargeMed_S_P;
            }
            else
            {
                level.Script.AddStartInventoryItem(TR1Items.LargeMed_S_P);
            }
        }

        // If we have also used a secret that requires damage and is glitched, add an additional
        // medi as these tend to occur where Lara has to drop far after picking them up.
        if (locations.Any(l => l.RequiresDamage && l.RequiresGlitch))
        {
            level.Script.AddStartInventoryItem(TR1Items.SmallMed_S_P);
        }
    }

    private static void SetPuzzleTypeName(TR1CombinedLevel level, TR1Type itemType, string name)
    {
        if (TR1TypeUtilities.IsKeyType(itemType))
        {
            PopulateScriptStrings(itemType - TR1Type.Key1_S_P, level.Script.Keys, "K");
            level.Script.Keys.Add(name);
        }
        else if (TR1TypeUtilities.IsPuzzleType(itemType))
        {
            PopulateScriptStrings(itemType - TR1Type.Puzzle1_S_P, level.Script.Puzzles, "P");
            level.Script.Puzzles.Add(name);
        }
        else if (TR1TypeUtilities.IsQuestType(itemType))
        {
            PopulateScriptStrings(itemType - TR1Type.Quest1_S_P, level.Script.Pickups, "Q");
            level.Script.Pickups.Add(name);
        }
    }

    private static void PopulateScriptStrings(int index, List<string> gameStrings, string id)
    {
        while (index > gameStrings.Count)
        {
            gameStrings.Add(id + (gameStrings.Count + 1));
        }
    }

    private bool TestSecretPlacement(TR1CombinedLevel level, Location location, FDControl floorData)
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

        // Get the sector and check if it is shared with a trapdoor, breakable tile or bridge, as these won't work either.
        TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level.Data, floorData);
        foreach (TR1Entity otherEntity in level.Data.Entities)
        {
            TR1Type type = otherEntity.TypeID;
            if (location.Room == otherEntity.Room && (TR1TypeUtilities.IsTrapdoor(type) || TR1TypeUtilities.IsBridge(type) || type == TR1Type.FallingBlock))
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

        // Additional checks for bridge, trapdoor and breakable tile triggers that may be in rooms further below.
        // We look for floating secrets except if underwater or if the flipped room exists and has a floor below it.
        if (!CheckSectorsBelow(level, location, sector, floorData))
        {
            Debug.WriteLine(string.Format(SC.MidairErrorMsg, level.Name, location.X, location.Y, location.Z, location.Room));
            return false;
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

    private bool TestTriggerPlacement(TR1CombinedLevel level, Location location, short room, TRRoomSector sector, FDControl floorData)
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

    private void PlaceSecret(TR1CombinedLevel level, TRSecretPlacement<TR1Type> secret, FDControl floorData)
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

        // Turn off walk-to-items in TR1X if we are placing on a slope above water.
        if (!level.Data.Rooms[secret.Location.Room].ContainsWater
            && secret.Location.IsSlipperySlope(level.Data, floorData))
        {
            (ScriptEditor as TR1ScriptEditor).WalkToItems = false;
        }
    }

    private static bool CheckSectorsBelow(TR1CombinedLevel level, Location location, TRRoomSector sector, FDControl floorData)
    {
        // Allow this check to be overridden with Validated - covers glitched locations.
        if (!location.Validated && sector.RoomBelow != TRConsts.NoRoom)
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
                return sector.RoomBelow == TRConsts.NoRoom || level.Data.Rooms[altRoom].ContainsWater;
            }
            
            return false;
        }

        return true;
    }

    private void CreateSecretTriggers(TR1CombinedLevel level, TRSecretPlacement<TR1Type> secret, short room, FDControl floorData, TRRoomSector baseSector)
    {
        TRRoomSector mainSector = baseSector;
        while (mainSector.RoomBelow != TRConsts.NoRoom)
        {
            // Ensure we go as far down as possible - for example, Atlantis room 47 sector 10,9 - but stay in the same room
            mainSector = FDUtilities.GetRoomSector(secret.Location.X, (mainSector.Floor + 1) * TRConsts.Step1, secret.Location.Z, mainSector.RoomBelow, level.Data, floorData);
        }

        CreateSecretTrigger(level, secret, room, floorData, mainSector);

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

                // Process each unique sector only once and if it's a valid neighbour, add the extra trigger
                if (processedSectors.Add(neighbour) && !IsInvalidNeighbour(neighbour))
                {
                    if (neighbour.RoomBelow != baseSector.RoomBelow && neighbour.RoomBelow != TRConsts.NoRoom)
                    {
                        // Try to find the absolute floor
                        do
                        {
                            neighbour = FDUtilities.GetRoomSector(x, (neighbour.Floor + 1) * TRConsts.Step1, z, neighbour.RoomBelow, level.Data, floorData);
                        }
                        while (neighbour.RoomBelow != TRConsts.NoRoom);
                    }
                    CreateSecretTrigger(level, secret, room, floorData, neighbour);
                    if (Settings.DevelopmentMode)
                    {
                        Debug.WriteLine(string.Format(SC.EdgeInfoMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                    }
                }
            }
        }
    }

    private static bool IsInvalidNeighbour(TRRoomSector neighbour)
    {
        return neighbour.Floor == TRConsts.WallClicks && neighbour.Ceiling == TRConsts.WallClicks; // Inside a wall
    }

    private void CreateSecretTrigger(TR1CombinedLevel level, TRSecretPlacement<TR1Type> secret, short room, FDControl floorData, TRRoomSector sector)
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
                        if (actionItem.Parameter < level.Data.Entities.Count && actionItem.Parameter != secret.DoorIndex)
                        {
                            existingActions.Add(actionItem); // Add it anyway for testing
                            Debug.WriteLine(string.Format(SC.TriggerWarningMsg, actionItem.Parameter, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                        }
                    }
                    else if (secret.TriggerMask == TRConsts.FullMask)
                    {
                        existingActions.Add(actionItem);
                    }
                }
                else if (!Settings.UseRewardRoomCameras
                    || (actionItem.TrigAction != FDTrigAction.Camera && actionItem.TrigAction != FDTrigAction.LookAtItem))
                {
                    existingActions.Add(actionItem);
                }
            }

            trigger.TrigActionList.AddRange(existingActions);
            floorData.Entries[sector.FDIndex].Remove(existingTrigger);
        }

        floorData.Entries[sector.FDIndex].Add(trigger);
    }

    internal class SecretProcessor : AbstractProcessorThread<TR1SecretRandomizer>
    {
        private static readonly Dictionary<TR1Type, TR1Type> _secretModels = TR1TypeUtilities.GetSecretModels();
        private static readonly Dictionary<TR1Type, TR1Type> _modelReplacements = TR1TypeUtilities.GetSecretReplacements();

        // Move this to Gamestring Rando once implemented
        private static readonly Dictionary<TR1Type, string> _pickupNames = new()
        {
            [TR1Type.SecretAnkh_M_H] = "Secret Ankh",
            [TR1Type.SecretGoldBar_M_H] = "Secret Gold Bar",
            [TR1Type.SecretGoldIdol_M_H] = "Secret Gold Idol",
            [TR1Type.SecretLeadBar_M_H] = "Secret Lead Bar",
            [TR1Type.SecretScion_M_H] = "Secret Scion"
        };

        private readonly Dictionary<TR1CombinedLevel, TRSecretModelAllocation<TR1Type>> _importAllocations;

        internal override int LevelCount => _importAllocations.Count;

        internal SecretProcessor(TR1SecretRandomizer outer)
            : base(outer)
        {
            _importAllocations = new Dictionary<TR1CombinedLevel, TRSecretModelAllocation<TR1Type>>();
        }

        internal void AddLevel(TR1CombinedLevel level)
        {
            _importAllocations.Add(level, new TRSecretModelAllocation<TR1Type>());
        }

        protected override void StartImpl()
        {
            List<TR1Type> availableTypes = _secretModels.Keys.ToList();
            foreach (TR1CombinedLevel level in _importAllocations.Keys)
            {
                if (level.IsAssault)
                {
                    continue;
                }

                TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];

                // Work out which models are available to replace as secret pickups.
                // We exclude current puzzle/key items from the available switching pool.
                List<TRModel> models = level.Data.Models.ToList();

                foreach (TR1Type puzzleType in _modelReplacements.Keys)
                {
                    if (models.Find(m => m.ID == (uint)puzzleType) == null)
                    {
                        allocation.AvailablePickupModels.Add(puzzleType);
                    }
                }

                TR1Type modelType = _outer.Settings.UseRandomSecretModels
                    ? availableTypes[_outer._generator.Next(0, availableTypes.Count)]
                    : TR1TypeUtilities.GetBestLevelSecretModel(level.Name);
                allocation.ImportModels.Add(modelType);
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR1CombinedLevel level in _importAllocations.Keys)
            {
                if (!level.IsAssault)
                {
                    TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];

                    // Get the artefacts into the level and refresh the model list
                    TR1ModelImporter importer = new(true)
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
                    foreach (TR1Type secretModelType in allocation.ImportModels)
                    {
                        TR1Type secretPickupType = _secretModels[secretModelType];

                        TR1Type puzzleModelType = allocation.AvailablePickupModels.First();
                        TR1Type puzzlePickupType = _modelReplacements[puzzleModelType];

                        models.Find(m => m.ID == (uint)secretModelType).ID = (uint)puzzleModelType;
                        sequences.Find(s => s.SpriteID == (int)secretPickupType).SpriteID = (int)puzzlePickupType;

                        if (secretModelType == TR1Type.SecretScion_M_H && _outer.Are3DPickupsEnabled())
                        {
                            // TR1X embeds scions into the ground when they are puzzle/key types in 3D mode,
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
                        SetPuzzleTypeName(level, puzzlePickupType, _pickupNames[secretModelType]);
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
                    RemoveDefaultSecrets(level);

                    TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];

                    // Reward rooms can be conditionally chosen based on level state after placing secrets,
                    // but we need to make a placholder for door indices and masks to create those secrets.
                    TRSecretRoom<TR1Entity> rewardRoom = _outer.MakePlaceholderRewardRoom(level);

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
