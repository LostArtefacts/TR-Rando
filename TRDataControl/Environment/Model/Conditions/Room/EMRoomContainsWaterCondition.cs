using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Conditions;

public class EMRoomContainsWaterCondition : BaseEMCondition
{
    public short RoomIndex { get; set; }

    protected override bool Evaluate(TR1Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);
        return level.Rooms[data.ConvertRoom(RoomIndex)].ContainsWater;
    }

    protected override bool Evaluate(TR2Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);
        return level.Rooms[data.ConvertRoom(RoomIndex)].ContainsWater;
    }

    protected override bool Evaluate(TR3Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);
        return level.Rooms[data.ConvertRoom(RoomIndex)].ContainsWater;
    }
}
