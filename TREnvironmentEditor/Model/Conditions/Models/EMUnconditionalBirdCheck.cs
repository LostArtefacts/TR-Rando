using System;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;

namespace TREnvironmentEditor.Model.Conditions;

public class EMUnconditionalBirdCheck : BaseEMCondition
{
    protected override bool Evaluate(TR1Level level)
    {
        throw new NotSupportedException();
    }

    protected override bool Evaluate(TR2Level level)
    {
        TRModel model = Array.Find(level.Models, m => m.ID == (uint)TR2Entities.BirdMonster);
        if (model != null)
        {
            return level.Animations[model.Animation + 20].FrameEnd == level.Animations[model.Animation + 19].FrameEnd;
        }
        return false;
    }

    protected override bool Evaluate(TR3Level level)
    {
        throw new NotSupportedException();
    }
}
