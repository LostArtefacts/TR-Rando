namespace TRLevelControl.Model;

public class TRFootprintCommand : TRAnimCommand
{
    public override TRAnimCommandType Type => TRAnimCommandType.FlipEffect;
    public short FrameNumber { get; set; }
    public TRFootprint Foot { get; set; }

    public override TRAnimCommand Clone()
        => (TRFootprintCommand)MemberwiseClone();
}
