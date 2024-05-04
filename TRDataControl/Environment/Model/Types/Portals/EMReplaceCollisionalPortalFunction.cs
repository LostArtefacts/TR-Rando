using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMReplaceCollisionalPortalFunction : BaseEMFunction
{
    public short Room { get; set; }
    public short X { get; set; }
    public short Z { get; set; }
    public short AdjoiningRoom { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        TR1Room room = level.Rooms[data.ConvertRoom(Room)];
        TRRoomSector sector = room.Sectors[X * room.NumZSectors + Z];
        ReplacePortal(sector, data.ConvertRoom(AdjoiningRoom), level.FloorData);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        TR2Room room = level.Rooms[data.ConvertRoom(Room)];
        TRRoomSector sector = room.Sectors[X * room.NumZSectors + Z];
        ReplacePortal(sector, data.ConvertRoom(AdjoiningRoom), level.FloorData);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        TR3Room room = level.Rooms[data.ConvertRoom(Room)];
        TRRoomSector sector = room.Sectors[X * room.NumZSectors + Z];
        ReplacePortal(sector, data.ConvertRoom(AdjoiningRoom), level.FloorData);
    }

    private static void ReplacePortal(TRRoomSector sector, short adjoiningRoom, FDControl floorData)
    {
        if (sector.FDIndex == 0)
        {
            return;
        }

        foreach (FDEntry entry in floorData[sector.FDIndex].FindAll(e => e is FDPortalEntry))
        {
            (entry as FDPortalEntry).Room = adjoiningRoom;
        }
    }
}
