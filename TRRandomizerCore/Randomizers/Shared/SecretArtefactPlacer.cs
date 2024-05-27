using System.Diagnostics;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class SecretArtefactPlacer<T, E>
    where T : Enum
    where E : TREntity<T>, new()
{
    private static readonly string _invalidLocationMsg = "Cannot place a nonvalidated secret where a trigger already exists - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    private static readonly string _trapdoorLocationMsg = "Cannot place a secret on the same sector as a bridge/trapdoor - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    private static readonly string _midairErrorMsg = "Cannot place a secret in mid-air or on a breakable tile - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    private static readonly string _triggerWarningMsg = "Existing trigger object action with parameter {0} will be lost - {1} [X={2}, Y={3}, Z={4}, R={5}]";
    private static readonly string _flipMapWarningMsg = "Secret is being placed in a room that has a flipmap - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    private static readonly string _flipMapErrorMsg = "Secret cannot be placed in a flipped room - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    private static readonly string _edgeInfoMsg = "Adding extra tile edge trigger for {0} [X={1}, Y={2}, Z={3}, R={4}]";

    private static readonly int _triggerEdgeLimit = (int)Math.Ceiling(TRConsts.Step4 / 10d); // Within ~10% of a tile edge, triggers will be copied into neighbours

    public RandomizerSettings Settings { get; set; }
    public ItemFactory<E> ItemFactory { get; set; }

    private TRGameVersion _version;
    private string _levelName;
    private List<TRRoom> _rooms;
    private FDControl _floorData;
    private Func<Location, TRRoomSector> _sectorGetter;
    private Func<TRRoomSector, Location, bool> _sectorEntityTester;

    public void InitialiseLevel(TR1CombinedLevel level)
        => InitialiseLevel(level.Name, level.Data);

    public void InitialiseLevel(TR1RCombinedLevel level)
        => InitialiseLevel(level.Name, level.Data);

    public void InitialiseLevel(TR3CombinedLevel level)
        => InitialiseLevel(level.Name, level.Data);

    public void InitialiseLevel(TR3RCombinedLevel level)
        => InitialiseLevel(level.Name, level.Data);

    public void InitialiseLevel(string levelName, TR1Level level)
    {
        InitialiseLevel(TRGameVersion.TR1, levelName, new(level.Rooms), level.FloorData, loc => level.GetRoomSector(loc), (sector, loc) =>
        {
            return !level.Entities
                .Where(e => e.Room == loc.Room && (TR1TypeUtilities.IsTrapdoor(e.TypeID) || TR1TypeUtilities.IsBridge(e.TypeID) || e.TypeID == TR1Type.FallingBlock))
                .Any(e => level.GetRoomSector(e) == sector);
        });
    }

    public void InitialiseLevel(string levelName, TR3Level level)
    {
        InitialiseLevel(TRGameVersion.TR3, levelName, new(level.Rooms), level.FloorData, loc => level.GetRoomSector(loc), (sector, loc) =>
        {
            return !level.Entities
                .Where(e => e.Room == loc.Room && (TR3TypeUtilities.IsTrapdoor(e.TypeID) || TR3TypeUtilities.IsBridge(e.TypeID)))
                .Any(e => level.GetRoomSector(e) == sector);
        });
    }

    public void InitialiseLevel(TRGameVersion version,
        string levelName, 
        List<TRRoom> rooms,
        FDControl floorData,
        Func<Location, TRRoomSector> sectorGetter,
        Func<TRRoomSector, Location, bool> sectorEntityTester)
    {
        _version = version;
        _levelName = levelName;
        _rooms = rooms;
        _floorData = floorData;
        _sectorGetter = sectorGetter;
        _sectorEntityTester = sectorEntityTester;

        foreach (TRRoomSector sector in _rooms.SelectMany(r => r.Sectors))
        {
            if (sector.FDIndex == 0)
            {
                continue;
            }

            List<FDEntry> entries = floorData[sector.FDIndex];
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i] is FDTriggerEntry trig)
                {
                    // #230 Remove the secret action but retain anything else that may be triggered here
                    trig.Actions.RemoveAll(a => a.Action == FDTrigAction.SecretFound);
                    if (trig.Actions.Count == 0)
                    {
                        entries.RemoveAt(i);
                    }
                }
            }
        }
    }

    public bool TestSecretPlacement(Location location)
    {
        // Check if this secret is being added to a flipped room, as that won't work
        if (_rooms.Any(r => r.AlternateRoom == location.Room))
        {
            if (Settings.DevelopmentMode)
            {
                // Place it anyway in dev mode to allow relocating
                Debug.WriteLine(string.Format(_flipMapErrorMsg, _levelName, location.X, location.Y, location.Z, location.Room));
            }
            else
            {
                return false;
            }
        }

        // Get the sector and check if it is shared with the likes of a trapdoor, breakable tile or bridge, as these won't work either.
        TRRoomSector sector = _sectorGetter(location);
        if (!_sectorEntityTester(sector, location))
        {
            if (Settings.DevelopmentMode)
            {
                Debug.WriteLine(string.Format(_trapdoorLocationMsg, _levelName, location.X, location.Y, location.Z, location.Room));
            }
            return false;
        }

        // Additional checks for bridge, trapdoor and breakable tile triggers that may be in rooms further below.
        // We look for floating secrets except if underwater or if the flipped room exists and has a floor below it.
        if (!CheckSectorsBelow(location, sector))
        {
            Debug.WriteLine(string.Format(_midairErrorMsg, _levelName, location.X, location.Y, location.Z, location.Room));
            return false;
        }

        if (!TestTriggerPlacement(location, location.Room, sector))
        {
            return false;
        }

        // If the room has a flipmap, make sure to test the trigger there too.
        short altRoom = _rooms[location.Room].AlternateRoom;
        if (altRoom != -1)
        {
            Location flip = location.Clone();
            flip.Room = altRoom;
            sector = _sectorGetter(flip);
            if (!TestTriggerPlacement(location, altRoom, sector))
            {
                return false;
            }

            if (Settings.DevelopmentMode)
            {
                Debug.WriteLine(string.Format(_flipMapWarningMsg, _levelName, location.X, location.Y, location.Z, altRoom));
            }
        }

        return true;
    }

    private bool TestTriggerPlacement(Location location, short room, TRRoomSector sector)
    {
        if (!location.Validated && LocationUtilities.HasAnyTrigger(sector, _floorData))
        {
            // There is already a trigger here and the location hasn't been marked as being
            // safe to move the action items to the new pickup trigger.
            if (Settings.DevelopmentMode)
            {
                Debug.WriteLine(string.Format(_invalidLocationMsg, _levelName, location.X, location.Y, location.Z, room));
            }
            return false;
        }
        return true;
    }

    public void PlaceSecret(TRSecretPlacement<T> secret)
    {
        // This assumes TestTriggerPlacement has already been called and passed.
        TRRoomSector sector = _sectorGetter(secret.Location);
        CreateSecretTriggers(secret, secret.Location.Room, sector);

        short altRoom = _rooms[secret.Location.Room].AlternateRoom;
        if (altRoom != -1)
        {
            Location location = secret.Location.Clone();
            location.Room = altRoom;
            sector = _sectorGetter(location);
            CreateSecretTriggers(secret, altRoom, sector);
        }
    }

    private bool CheckSectorsBelow(Location location, TRRoomSector sector)
    {
        // Allow this check to be overridden with Validated - covers glitched locations.
        if (!location.Validated && sector.RoomBelow != TRConsts.NoRoom)
        {
            if (_rooms[location.Room].ContainsWater)
            {
                // Floating underwater, this will work
                return true;
            }

            short altRoom = _rooms[location.Room].AlternateRoom;
            if (altRoom != -1)
            {
                // Flipped room may have a floor here, or be underwater
                Location flip = location.Clone();
                flip.Room = _rooms[location.Room].AlternateRoom;
                sector = _sectorGetter(flip);
                return sector.RoomBelow == TRConsts.NoRoom || _rooms[altRoom].ContainsWater;
            }

            return false;
        }

        return true;
    }

    private void CreateSecretTriggers(TRSecretPlacement<T> secret, short room, TRRoomSector baseSector)
    {
        TRRoomSector mainSector = baseSector;
        Location location = secret.Location.Clone();
        while (mainSector.RoomBelow != TRConsts.NoRoom)
        {
            // Ensure we go as far down as possible - for example, Atlantis room 47 sector 10,9 - but stay in the same room
            location.Y = (mainSector.Floor + 1) * TRConsts.Step1;
            location.Room = mainSector.RoomBelow;
            mainSector = _sectorGetter(location);
        }

        CreateSecretTrigger(secret, room, mainSector);

        // Check neighbouring sectors if we are very close to tile edges. We scan 8 locations around
        // the secret's position based on the edge tolerance and see if the sector has changed.
        HashSet<TRRoomSector> processedSectors = new() { baseSector };
        for (int xNorm = -1; xNorm < 2; xNorm++)
        {
            for (int zNorm = -1; zNorm < 2; zNorm++)
            {
                if (xNorm == 0 && zNorm == 0) continue; // Primary trigger's sector

                location.X = secret.Location.X + xNorm * _triggerEdgeLimit;
                location.Z = secret.Location.Z + zNorm * _triggerEdgeLimit;
                location.Y = secret.Location.Y;
                location.Room = room;

                TRRoomSector neighbour = _sectorGetter(location);

                // Process each unique sector only once and if it's a valid neighbour, add the extra trigger
                if (processedSectors.Add(neighbour) && !IsInvalidNeighbour(neighbour))
                {
                    // Lara doesn't clip up in TR3+ unlike TR1 if she is against the wall, so this avoids unnecessary extra FD.
                    if (_version >= TRGameVersion.TR3
                        && Math.Abs(secret.Location.Y - LocationUtilities.GetCornerHeight(neighbour, _floorData, location.X, location.Z)) >= TRConsts.Step1)
                    {
                        continue;
                    }

                    if (neighbour.RoomBelow != baseSector.RoomBelow && neighbour.RoomBelow != TRConsts.NoRoom)
                    {
                        // Try to find the absolute floor
                        do
                        {
                            location.Y = (neighbour.Floor + 1) * TRConsts.Step1;
                            location.Room = neighbour.RoomBelow;
                            neighbour = _sectorGetter(location);
                        }
                        while (neighbour.RoomBelow != TRConsts.NoRoom);
                    }

                    CreateSecretTrigger(secret, room, neighbour);
                    if (Settings.DevelopmentMode)
                    {
                        Debug.WriteLine(string.Format(_edgeInfoMsg, _levelName, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                    }
                }
            }
        }
    }

    private static bool IsInvalidNeighbour(TRRoomSector neighbour)
    {
        return neighbour.Floor == TRConsts.WallClicks && neighbour.Ceiling == TRConsts.WallClicks; // Inside a wall
    }

    private void CreateSecretTrigger(TRSecretPlacement<T> secret, short room, TRRoomSector sector)
    {
        if (sector.FDIndex == 0)
        {
            _floorData.CreateFloorData(sector);
        }

        // Make a new pickup trigger
        FDTriggerEntry trigger = new()
        {
            Mask = secret.TriggerMask,
            TrigType = FDTrigType.Pickup,
            Actions = new()
            {
                new()
                {
                    Parameter = secret.EntityIndex
                },
                new()
                {
                    Action = FDTrigAction.SecretFound,
                    Parameter = secret.SecretIndex
                }
            }
        };

        if (secret.TriggersDoor)
        {
            trigger.Actions.Add(new()
            {
                Parameter = secret.DoorIndex
            });
        }

        // Move any existing action list items to the new trigger and remove the old one. We can only
        // move Object actions if the mask on this trigger is full.
        if (_floorData[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry existingTrigger)
        {
            List<FDActionItem> existingActions = new();
            foreach (FDActionItem actionItem in existingTrigger.Actions)
            {
                if (actionItem.Action == FDTrigAction.Object)
                {
                    if (Settings.DevelopmentMode)
                    {
                        if (actionItem.Parameter != secret.DoorIndex)
                        {
                            existingActions.Add(actionItem); // Add it anyway for testing
                            Debug.WriteLine(string.Format(_triggerWarningMsg, actionItem.Parameter, _levelName, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                        }
                    }
                    else if (secret.TriggerMask == TRConsts.FullMask)
                    {
                        existingActions.Add(actionItem);
                    }
                }
                else if (!Settings.UseRewardRoomCameras
                    || (actionItem.Action != FDTrigAction.Camera && actionItem.Action != FDTrigAction.LookAtItem))
                {
                    existingActions.Add(actionItem);
                }
            }

            trigger.Actions.AddRange(existingActions);
            _floorData[sector.FDIndex].Remove(existingTrigger);
        }

        _floorData[sector.FDIndex].Add(trigger);
    }

    public TRSecretRoom<E> MakePlaceholderRewardRoom(TRGameVersion gameVersion, string levelName, int secretCount, List<E> allItems)
    {
        TRSecretRoom<E> rewardRoom = null;
        string mappingPath = $@"Resources\{gameVersion}\SecretMapping\{levelName}-SecretMapping.json";
        if (File.Exists(mappingPath))
        {
            int requiredDoors = (int)Math.Ceiling((double)secretCount / TRConsts.MaskBits);
            rewardRoom = new()
            {
                DoorIndices = new()
            };

            for (int i = 0; i < requiredDoors; i++)
            {
                E door = ItemFactory.CreateItem(levelName, allItems);
                rewardRoom.DoorIndices.Add(allItems.IndexOf(door));
            }
        }

        return rewardRoom;
    }

    public void CreateRewardRoom(string levelName,
        TRSecretRoom<E> placeholder,
        TRSecretRoom<E> finalRoom,
        List<E> allItems,
        List<TRCamera> allCameras,
        T cameraTargetType,
        List<int> rewardIndices,
        FDControl floorData,
        short roomIndex,
        int secretCount,
        Func<T, bool> isTrapdoor)
    {
        // Convert the temporary doors
        finalRoom.DoorIndices = placeholder.DoorIndices;
        for (int i = 0; i < finalRoom.DoorIndices.Count; i++)
        {
            int doorIndex = finalRoom.DoorIndices[i];
            E door = finalRoom.Doors[i];
            if (door.Room < 0)
            {
                door.Room = roomIndex;
            }
            allItems[doorIndex] = door;

            if (isTrapdoor(door.TypeID))
            {
                TRRoomSector sector = _sectorGetter(door.GetFloorLocation(_sectorGetter));
                if (sector.FDIndex == 0)
                {
                    floorData.CreateFloorData(sector);
                }

                floorData[sector.FDIndex].Add(new FDTriggerEntry
                {
                    TrigType = FDTrigType.Dummy,
                    Actions = new()
                    {
                        new()
                        {
                            Parameter = (short)doorIndex
                        }
                    }
                });
            }
        }

        // Spread the rewards out fairly evenly across each defined position in the new room.
        int rewardPositionCount = finalRoom.RewardPositions.Count;
        for (int i = 0; i < rewardIndices.Count; i++)
        {
            E item = allItems[rewardIndices[i]];
            item.SetLocation(finalRoom.RewardPositions[i % rewardPositionCount]);
            item.Room = roomIndex;
        }

        // #238 Make the required number of cameras. Because of the masks, we need
        // a camera per counted secret otherwise it only shows once.
        if (Settings.UseRewardRoomCameras && finalRoom.Cameras != null)
        {
            finalRoom.CameraIndices = new();
            for (int i = 0; i < secretCount; i++)
            {
                finalRoom.CameraIndices.Add(allCameras.Count);
                allCameras.Add(finalRoom.Cameras[i % finalRoom.Cameras.Count]);
            }

            short cameraTarget;
            if (finalRoom.CameraTarget != null && ItemFactory.CanCreateItem(levelName, allItems))
            {
                E target = ItemFactory.CreateItem(levelName, allItems, finalRoom.CameraTarget);
                target.TypeID = cameraTargetType;
                cameraTarget = (short)allItems.IndexOf(target);
            }
            else
            {
                cameraTarget = (short)finalRoom.DoorIndices[0];
            }

            // Get each trigger created for each secret index and add the camera, provided
            // there isn't any existing camera actions.
            for (int i = 0; i < secretCount; i++)
            {
                List<FDTriggerEntry> secretTriggers = floorData.GetSecretTriggers(i);
                foreach (FDTriggerEntry trigger in secretTriggers)
                {
                    if (trigger.Actions.Find(a => a.Action == FDTrigAction.Camera) == null)
                    {
                        trigger.Actions.Add(new()
                        {
                            Action = FDTrigAction.Camera,
                            CamAction = new()
                            {
                                Timer = 4
                            },
                            Parameter = (short)finalRoom.CameraIndices[i]
                        });
                        trigger.Actions.Add(new()
                        {
                            Action = FDTrigAction.LookAtItem,
                            Parameter = cameraTarget
                        });
                    }
                }
            }
        }
    }
}
