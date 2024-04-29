using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Conditions;

public class EMUnconditionalBirdCheck : BaseEMCondition
{
    protected override bool Evaluate(TR1Level level)
    {
        throw new NotSupportedException();
    }

    protected override bool Evaluate(TR2Level level)
    {
        TRModel model = level.Models[TR2Type.BirdMonster];
        return model != null && model.Animations[20].FrameEnd == model.Animations[19].FrameEnd;
    }

    protected override bool Evaluate(TR3Level level)
    {
        throw new NotSupportedException();
    }
}
