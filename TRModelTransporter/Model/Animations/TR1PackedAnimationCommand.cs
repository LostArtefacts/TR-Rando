using TRLevelControl.Model;

namespace TRModelTransporter.Model.Animations;

public class TR1PackedAnimationCommand
{
    public TRAnimCommandType Command { get; set; }
    public short[] Params { get; set; }
}
