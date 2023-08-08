using TRLevelControl.Model.Enums;

namespace TRModelTransporter.Model.Animations;

public class TR1PackedAnimationCommand
{
    public TRAnimCommandTypes Command { get; set; }
    public short[] Params { get; set; }
}
