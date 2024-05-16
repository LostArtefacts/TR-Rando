using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMUnconditionalBirdCheck : BaseEMCondition
{
    protected override bool Evaluate(TR1Level level)
    {
        throw new NotSupportedException();
    }

    protected override bool Evaluate(TR2Level level)
    {
        TRAnimation birdDeathAnim = level.Models[TR2Type.BirdMonster]?.Animations[20];
        return birdDeathAnim != null && birdDeathAnim.FrameEnd < birdDeathAnim.FrameStart;
    }

    protected override bool Evaluate(TR3Level level)
    {
        throw new NotSupportedException();
    }
}
