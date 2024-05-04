using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMRemoveCollisionalPortalFunction : BaseEMFunction
{
    public EMLocation Location1 { get; set; }
    public EMLocation Location2 { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        Location1.Room = data.ConvertRoom(Location1.Room);
        Location2.Room = data.ConvertRoom(Location2.Room);

        TR1Room room1 = level.Rooms[Location1.Room];
        TR1Room room2 = level.Rooms[Location2.Room];

        // Change all this to room1.GetSector(Location1.X, Location1.Z);
        TRRoomSector sector1 = room1.Sectors[GetSectorIndex(room1.Info, Location1, room1.NumZSectors)];
        TRRoomSector sector2 = room2.Sectors[GetSectorIndex(room2.Info, Location2, room2.NumZSectors)];

        RemovePortals(sector1, sector2, level.FloorData);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        Location1.Room = data.ConvertRoom(Location1.Room);
        Location2.Room = data.ConvertRoom(Location2.Room);

        TR2Room room1 = level.Rooms[Location1.Room];
        TR2Room room2 = level.Rooms[Location2.Room];

        TRRoomSector sector1 = room1.Sectors[GetSectorIndex(room1.Info, Location1, room1.NumZSectors)];
        TRRoomSector sector2 = room2.Sectors[GetSectorIndex(room2.Info, Location2, room2.NumZSectors)];

        RemovePortals(sector1, sector2, level.FloorData);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        Location1.Room = data.ConvertRoom(Location1.Room);
        Location2.Room = data.ConvertRoom(Location2.Room);

        TR3Room room1 = level.Rooms[Location1.Room];
        TR3Room room2 = level.Rooms[Location2.Room];

        TRRoomSector sector1 = room1.Sectors[GetSectorIndex(room1.Info, Location1, room1.NumZSectors)];
        TRRoomSector sector2 = room2.Sectors[GetSectorIndex(room2.Info, Location2, room2.NumZSectors)];

        RemovePortals(sector1, sector2, level.FloorData);
    }

    private void RemovePortals(TRRoomSector sector1, TRRoomSector sector2, FDControl floorData)
    {
        if (sector1 == sector2)
        {
            return;
        }

        RemoveVerticalPortals(sector1);
        RemoveVerticalPortals(sector2);
        RemoveHorizontalPortals(sector1, floorData);
        RemoveHorizontalPortals(sector2, floorData);
    }

    private void RemoveVerticalPortals(TRRoomSector sector)
    {
        if (sector.RoomBelow == Location1.Room || sector.RoomBelow == Location2.Room)
        {
            sector.RoomBelow = TRConsts.NoRoom;
        }
        if (sector.RoomAbove == Location1.Room || sector.RoomAbove == Location2.Room)
        {
            sector.RoomAbove = TRConsts.NoRoom;
        }
    }

    private void RemoveHorizontalPortals(TRRoomSector sector, FDControl floorData)
    {
        if (sector.FDIndex == 0)
        {
            return;
        }

        List<FDEntry> entries = floorData[sector.FDIndex];
        if (entries.RemoveAll(e => e is FDPortalEntry portal && (portal.Room == Location1.Room || portal.Room == Location2.Room)) > 0)
        {
            // Ensure it's a wall and remove all FD - don't leave nospace in our trails
            sector.Floor = sector.Ceiling = TRConsts.WallClicks;
            entries.Clear();
        }
    }

    private static int GetSectorIndex(TRRoomInfo roomInfo, EMLocation location, int roomDepth)
    {
        int x = (location.X - roomInfo.X) / TRConsts.Step4;
        int z = (location.Z - roomInfo.Z) / TRConsts.Step4;
        return x * roomDepth + z;
    }
}
