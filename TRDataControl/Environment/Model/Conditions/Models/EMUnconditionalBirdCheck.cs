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
        if (level.Models[TR2Type.BirdMonster]?.Animations[20] is not TRAnimation anim)
        {
            return false;
        }

        return _isRemastered
            ? anim.FrameEnd < anim.FrameStart
            : !anim.Commands.Any(c => c is TRFXCommand { EffectID: (short)TR2FX.EndLevel });
    }

    protected override bool Evaluate(TR3Level level)
    {
        throw new NotSupportedException();
    }
}
