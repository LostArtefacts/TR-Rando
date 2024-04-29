using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Conditions;

public class EMModelExistsCondition : BaseEMCondition
{
    public uint ModelID { get; set; }

    protected override bool Evaluate(TR1Level level)
    {
        return level.Models.ContainsKey((TR1Type)ModelID);
    }

    protected override bool Evaluate(TR2Level level)
    {
        return level.Models.ContainsKey((TR2Type)ModelID);
    }

    protected override bool Evaluate(TR3Level level)
    {
        return level.Models.ContainsKey((TR3Type)ModelID);
    }
}
