using System.Numerics;
using TRFDControl.Utilities;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities;

public class TR3LocationGenerator : AbstractLocationGenerator<TR3Level>
{
    public override bool CrawlspacesAllowed => true;
    public override bool WadingAllowed => true;

    protected override void ReadFloorData(TR3Level level)
    {
        _floorData.ParseFromLevel(level);
    }

    protected override TRRoomSector GetSector(Location location, TR3Level level)
    {
        return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level, _floorData);
    }

    protected override TRRoomSector GetSector(int x, int z, int roomIndex, TR3Level level)
    {
        TR3Room room = level.Rooms[roomIndex];
        return FDUtilities.GetRoomSector(x, z, room.Sectors, room.Info, room.NumZSectors);
    }

    protected override List<TRRoomSector> GetRoomSectors(TR3Level level, int room)
    {
        return level.Rooms[room].Sectors.ToList();
    }

    protected override List<TRStaticMesh> GetStaticMeshes(TR3Level level)
    {
        return level.StaticMeshes.ToList();
    }

    protected override int GetRoomCount(TR3Level level)
    {
        return level.Rooms.Count;
    }

    protected override short GetFlipMapRoom(TR3Level level, short room)
    {
        return level.Rooms[room].AlternateRoom;
    }

    protected override bool IsRoomValid(TR3Level level, short room)
    {
        return !level.Rooms[room].IsSwamp;
    }

    protected override Dictionary<ushort, List<Location>> GetRoomStaticMeshLocations(TR3Level level, short room)
    {
        Dictionary<ushort, List<Location>> locations = new();
        foreach (TR3RoomStaticMesh staticMesh in level.Rooms[room].StaticMeshes)
        {
            if (!locations.ContainsKey(staticMesh.MeshID))
            {
                locations[staticMesh.MeshID] = new List<Location>();
            }

            locations[staticMesh.MeshID].Add(new Location
            {
                X = (int)staticMesh.X,
                Y = (int)staticMesh.Y,
                Z = (int)staticMesh.Z,
                Room = room
            });
        }

        return locations;
    }

    protected override ushort GetRoomDepth(TR3Level level, short room)
    {
        return level.Rooms[room].NumZSectors;
    }

    protected override int GetRoomYTop(TR3Level level, short room)
    {
        return level.Rooms[room].Info.YTop;
    }

    protected override Vector2 GetRoomPosition(TR3Level level, short room)
    {
        return new Vector2(level.Rooms[room].Info.X, level.Rooms[room].Info.Z);
    }

    protected override int GetHeight(TR3Level level, Location location, bool waterOnly)
    {
        return FDUtilities.GetHeight(location.X, location.Z, (short)location.Room, level, _floorData, waterOnly);
    }
}
