using System.Numerics;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities;

public class TR2LocationGenerator : AbstractLocationGenerator<TR2Type, TR2Level>
{
    public override bool CrawlspacesAllowed => false;
    public override bool WadingAllowed => true;

    protected override TRRoomSector GetSector(Location location, TR2Level level)
    {
        return level.GetRoomSector(location);
    }

    protected override TRRoomSector GetSector(int x, int z, int roomIndex, TR2Level level)
    {
        TR2Room room = level.Rooms[roomIndex];
        return room.GetSector(x, z);
    }

    protected override List<TRRoomSector> GetRoomSectors(TR2Level level, int room)
    {
        return level.Rooms[room].Sectors.ToList();
    }

    protected override TRDictionary<TR2Type, TRStaticMesh> GetStaticMeshes(TR2Level level)
    {
        return level.StaticMeshes;
    }

    protected override int GetRoomCount(TR2Level level)
    {
        return level.Rooms.Count;
    }

    protected override short GetFlipMapRoom(TR2Level level, short room)
    {
        return level.Rooms[room].AlternateRoom;
    }

    protected override bool IsRoomValid(TR2Level level, short room)
    {
        return true;
    }

    protected override Dictionary<TR2Type, List<Location>> GetRoomStaticMeshLocations(TR2Level level, short room)
    {
        Dictionary<TR2Type, List<Location>> locations = new();
        foreach (TR2RoomStaticMesh staticMesh in level.Rooms[room].StaticMeshes)
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

    protected override ushort GetRoomDepth(TR2Level level, short room)
    {
        return level.Rooms[room].NumZSectors;
    }

    protected override int GetRoomYTop(TR2Level level, short room)
    {
        return level.Rooms[room].Info.YTop;
    }

    protected override Vector2 GetRoomPosition(TR2Level level, short room)
    {
        return new Vector2(level.Rooms[room].Info.X, level.Rooms[room].Info.Z);
    }

    protected override int GetHeight(TR2Level level, Location location, bool waterOnly)
    {
        return _floorData.GetHeight(location.X, location.Z, location.Room, level.Rooms, waterOnly);
    }
}
