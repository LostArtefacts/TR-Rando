namespace TRLevelControl.Model;

public class TRSFXCommand : TRAnimCommand
{
    public override TRAnimCommandType Type => TRAnimCommandType.PlaySound;
    public TRSFXEnvironment Environment { get; set; }
    public short FrameNumber { get; set; }
    public short SoundID { get; set; }

    public override TRAnimCommand Clone()
        => (TRSFXCommand)MemberwiseClone();
}
