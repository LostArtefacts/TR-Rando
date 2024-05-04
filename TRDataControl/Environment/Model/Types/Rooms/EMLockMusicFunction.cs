using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMLockMusicFunction : BaseEMFunction
{
    public List<int> RoomNumbers { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        RoomNumbers.ForEach(r => level.Rooms[data.ConvertRoom(r)].SetFlag(TRRoomFlag.Unused2, true));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        RoomNumbers.ForEach(r => level.Rooms[data.ConvertRoom(r)].SetFlag(TRRoomFlag.Unused2, true));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        RoomNumbers.ForEach(r => level.Rooms[data.ConvertRoom(r)].SetFlag(TRRoomFlag.Unused2, true));
    }
}
