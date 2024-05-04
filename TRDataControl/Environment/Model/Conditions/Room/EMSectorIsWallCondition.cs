using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMSectorIsWallCondition : BaseEMCondition
{
    public EMLocation Location { get; set; }

    protected override bool Evaluate(TR1Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);
        TR1Room room = level.Rooms[data.ConvertRoom(Location.Room)];
        return room.Sectors[GetSectorIndex(room.Info, Location, room.NumZSectors)].IsWall;
    }
    protected override bool Evaluate(TR2Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);
        TR2Room room = level.Rooms[data.ConvertRoom(Location.Room)];
        return room.Sectors[GetSectorIndex(room.Info, Location, room.NumZSectors)].IsWall;
    }

    protected override bool Evaluate(TR3Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);
        TR3Room room = level.Rooms[data.ConvertRoom(Location.Room)];
        return room.Sectors[GetSectorIndex(room.Info, Location, room.NumZSectors)].IsWall;
    }

    private static int GetSectorIndex(TRRoomInfo roomInfo, EMLocation location, int roomDepth)
    {
        int x = (location.X - roomInfo.X) / TRConsts.Step4;
        int z = (location.Z - roomInfo.Z) / TRConsts.Step4;
        return x * roomDepth + z;
    }
}
