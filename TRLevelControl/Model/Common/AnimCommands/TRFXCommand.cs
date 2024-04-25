namespace TRLevelControl.Model;

public class TRFXCommand : TRAnimCommand
{
    public override TRAnimCommandType Type => TRAnimCommandType.FlipEffect;
    public short FrameNumber { get; set; }
    public short EffectID { get; set; }

    public override TRAnimCommand Clone()
        => (TRFXCommand)MemberwiseClone();
}
