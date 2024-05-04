using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMConvertEntityFunction : BaseEMFunction
{
    public int EntityIndex { get; set; }
    public short NewEntityType { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        level.Entities[data.ConvertEntity(EntityIndex)].TypeID = (TR1Type)NewEntityType;
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        level.Entities[data.ConvertEntity(EntityIndex)].TypeID = (TR2Type)NewEntityType;
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        level.Entities[data.ConvertEntity(EntityIndex)].TypeID = (TR3Type)NewEntityType;
    }
}
