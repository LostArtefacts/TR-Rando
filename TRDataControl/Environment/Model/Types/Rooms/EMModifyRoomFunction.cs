using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMModifyRoomFunction : BaseEMFunction
{
    public int[] Rooms { get; set; }
    public bool? IsSkyboxVisible { get; set; }
    public bool? IsWindy { get; set; }
    public bool? IsSwamp { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        var data = GetData(level);
        ModifyRooms(data, i => level.Rooms[i]);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        var data = GetData(level);
        ModifyRooms(data, i => level.Rooms[i]);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        var data = GetData(level);
        ModifyRooms(data, i => level.Rooms[i]);
    }

    private void ModifyRooms(EMLevelData data, Func<int, TRRoom> roomGetter)
    {
        foreach (int roomNumber in Rooms)
        {
            var room = roomGetter(data.ConvertRoom(roomNumber));
            if (IsSkyboxVisible.HasValue)
            {
                room.IsSkyboxVisible = IsSkyboxVisible.Value;
            }
            if (IsWindy.HasValue)
            {
                room.IsWindy = IsWindy.Value;
            }
            if (IsSwamp.HasValue)
            {
                room.IsSwamp = IsSwamp.Value;
            }
        }
    }
}
