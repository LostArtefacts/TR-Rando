using System.Numerics;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities;

public class TR1LocationGenerator : AbstractLocationGenerator<TR1Type, TR1Level>
{
    public override bool CrawlspacesAllowed => false;
    public override bool WadingAllowed => false;

    protected override void ReadFloorData(TR1Level level)
    {
        _floorData.ParseFromLevel(level);
    }

    protected override TRRoomSector GetSector(Location location, TR1Level level)
    {
        return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level, _floorData);
    }

    protected override TRRoomSector GetSector(int x, int z, int roomIndex, TR1Level level)
    {
        TR1Room room = level.Rooms[roomIndex];
        return FDUtilities.GetRoomSector(x, z, room.Sectors, room.Info, room.NumZSectors);
    }

    protected override List<TRRoomSector> GetRoomSectors(TR1Level level, int room)
    {
        return level.Rooms[room].Sectors.ToList();
    }

    protected override TRDictionary<TR1Type, TRStaticMesh> GetStaticMeshes(TR1Level level)
    {
        return level.StaticMeshes;
    }

    protected override int GetRoomCount(TR1Level level)
    {
        return level.Rooms.Count;
    }

    protected override short GetFlipMapRoom(TR1Level level, short room)
    {
        return level.Rooms[room].AlternateRoom;
    }

    protected override bool IsRoomValid(TR1Level level, short room)
    {
        return true;
    }

    protected override bool TriggerSupportsItems(TR1Level level, FDTriggerEntry trigger)
    {
        // Assume a Thor hammer trigger is directly below the hammer head.
        return !trigger.TrigActionList.Any(a => a.TrigAction == FDTrigAction.Object
            && level.Entities[a.Parameter].TypeID == TR1Type.ThorHammerHandle);
    }

    protected override Dictionary<TR1Type, List<Location>> GetRoomStaticMeshLocations(TR1Level level, short room)
    {
        Dictionary<TR1Type, List<Location>> locations = new();
        foreach (TR1RoomStaticMesh staticMesh in level.Rooms[room].StaticMeshes)
        {
            if (!locations.ContainsKey(staticMesh.ID))
            {
                locations[staticMesh.ID] = new();
            }
            locations[staticMesh.ID].Add(new()
            {
                X = staticMesh.X,
                Y = staticMesh.Y,
                Z = staticMesh.Z,
                Room = room
            });
        }

        return locations;
    }

    protected override ushort GetRoomDepth(TR1Level level, short room)
    {
        return level.Rooms[room].NumZSectors;
    }

    protected override int GetRoomYTop(TR1Level level, short room)
    {
        return level.Rooms[room].Info.YTop;
    }

    protected override Vector2 GetRoomPosition(TR1Level level, short room)
    {
        return new Vector2(level.Rooms[room].Info.X, level.Rooms[room].Info.Z);
    }

    protected override int GetHeight(TR1Level level, Location location, bool waterOnly)
    {
        return FDUtilities.GetHeight(location.X, location.Z, (short)location.Room, level, _floorData, waterOnly);
    }
}
