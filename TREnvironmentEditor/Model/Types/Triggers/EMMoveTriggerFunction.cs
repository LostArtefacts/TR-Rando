using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMoveTriggerFunction : BaseEMFunction
{
    public EMLocation BaseLocation { get; set; }
    public EMLocation NewLocation { get; set; }
    public int? EntityLocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        TRRoomSector baseSector = level.FloorData.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level);

        if (NewLocation != null)
        {
            NewLocation.Room = data.ConvertRoom(NewLocation.Room);
        }
        else if (EntityLocation.HasValue)
        {
            TR1Entity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
            NewLocation = new()
            {
                X = entity.X,
                Y = entity.Y,
                Z = entity.Z,
                Room = data.ConvertRoom(entity.Room)
            };
        }
        else
        {
            throw new InvalidOperationException("No means to determine new sector for moving trigger.");
        }

        TRRoomSector newSector = level.FloorData.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, NewLocation.Room, level);
        if (MoveTriggers(baseSector, newSector, level.FloorData))
        {
            // Make sure to copy the trigger into the flipped room if applicable
            short altRoom = level.Rooms[NewLocation.Room].AlternateRoom;
            if (altRoom != -1)
            {
                CreateFlipmapTriggerFunction(altRoom).ApplyToLevel(level);
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        TRRoomSector baseSector = level.FloorData.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level);

        if (NewLocation != null)
        {
            NewLocation.Room = data.ConvertRoom(NewLocation.Room);
        }
        else if (EntityLocation.HasValue)
        {
            TR2Entity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
            NewLocation = new()
            {
                X = entity.X,
                Y = entity.Y,
                Z = entity.Z,
                Room = data.ConvertRoom(entity.Room)
            };
        }
        else
        {
            throw new InvalidOperationException("No means to determine new sector for moving trigger.");
        }

        TRRoomSector newSector = level.FloorData.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, NewLocation.Room, level);
        if (MoveTriggers(baseSector, newSector, level.FloorData))
        {
            // Make sure to copy the trigger into the flipped room if applicable
            short altRoom = level.Rooms[NewLocation.Room].AlternateRoom;
            if (altRoom != -1)
            {
                CreateFlipmapTriggerFunction(altRoom).ApplyToLevel(level);
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        TRRoomSector baseSector = level.FloorData.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level);

        if (NewLocation != null)
        {
            NewLocation.Room = data.ConvertRoom(NewLocation.Room);
        }
        else if (EntityLocation.HasValue)
        {
            TR3Entity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
            NewLocation = new()
            {
                X = entity.X,
                Y = entity.Y,
                Z = entity.Z,
                Room = data.ConvertRoom(entity.Room)
            };
        }
        else
        {
            throw new InvalidOperationException("No means to determine new sector for moving trigger.");
        }

        TRRoomSector newSector = level.FloorData.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, NewLocation.Room, level);
        if (MoveTriggers(baseSector, newSector, level.FloorData))
        {
            // Make sure to copy the trigger into the flipped room if applicable
            short altRoom = level.Rooms[NewLocation.Room].AlternateRoom;
            if (altRoom != -1)
            {
                CreateFlipmapTriggerFunction(altRoom).ApplyToLevel(level);
            }
        }
    }

    private static bool MoveTriggers(TRRoomSector baseSector, TRRoomSector newSector, FDControl control)
    {
        if (baseSector != newSector && baseSector.FDIndex != 0)
        {
            List<FDEntry> triggers = control[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
            if (triggers.Count > 0)
            {
                if (newSector.FDIndex == 0)
                {
                    control.CreateFloorData(newSector);
                }

                control[newSector.FDIndex].AddRange(triggers);
                control[baseSector.FDIndex].RemoveAll(e => triggers.Contains(e));

                return true;
            }
        }

        return false;
    }

    private EMDuplicateTriggerFunction CreateFlipmapTriggerFunction(short altRoom)
    {
        // We want the trigger that's in the new target location added to the same
        // position in the flipped room.
        return new()
        {
            BaseLocation = NewLocation,
            Locations = new()
            {
                new()
                {
                    X = NewLocation.X,
                    Y = NewLocation.Y,
                    Z = NewLocation.Z,
                    Room = altRoom
                }
            }
        };
    }
}
