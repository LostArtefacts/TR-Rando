using System.Numerics;
using TRFDControl.Utilities;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities;

public class TR2LocationGenerator : AbstractLocationGenerator<TR2Type, TR2Level>
{
    public override bool CrawlspacesAllowed => false;
    public override bool WadingAllowed => true;

    protected override void ReadFloorData(TR2Level level)
    {
        _floorData.ParseFromLevel(level);
    }

    protected override TRRoomSector GetSector(Location location, TR2Level level)
    {
        return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level, _floorData);
    }

    protected override TRRoomSector GetSector(int x, int z, int roomIndex, TR2Level level)
    {
        TR2Room room = level.Rooms[roomIndex];
        return FDUtilities.GetRoomSector(x, z, room.Sectors.ToArray(), room.Info, room.NumZSectors);
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
            TR2Type id = staticMesh.MeshID + TR2Type.SceneryBase;
            locations[id] ??= new();
            locations[id].Add(new()
            {
                X = (int)staticMesh.X,
                Y = (int)staticMesh.Y,
                Z = (int)staticMesh.Z,
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
        return FDUtilities.GetHeight(location.X, location.Z, (short)location.Room, level, _floorData, waterOnly);
    }
}
