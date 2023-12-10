using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMLockMusicFunction : BaseEMFunction
{
    public static readonly short LockedMusicFlag = 0x800;

    public List<int> RoomNumbers { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        RoomNumbers.ForEach(r => level.Rooms[data.ConvertRoom(r)].Flags |= LockedMusicFlag);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        RoomNumbers.ForEach(r => level.Rooms[data.ConvertRoom(r)].Flags |= LockedMusicFlag);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        RoomNumbers.ForEach(r => level.Rooms[data.ConvertRoom(r)].Flags |= LockedMusicFlag);
    }
}
