using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Conditions;

public class EMSectorContainsSecretCondition : BaseEMCondition
{
    public EMLocation Location { get; set; }

    protected override bool Evaluate(TR1Level level)
    {
        return Location.GetContainedSecretEntity(level) != -1;
    }

    protected override bool Evaluate(TR2Level level)
    {
        return Location.GetContainedSecretEntity(level) != -1;
    }

    protected override bool Evaluate(TR3Level level)
    {
        return Location.GetContainedSecretEntity(level) != -1;
    }
}
