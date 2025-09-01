namespace TRLevelControl.Model;

public class TRCinematicFrame : ICloneable
{
    public TRVertex Position { get; set; }
    public TRVertex Target { get; set; }
    public short FOV { get; set; }
    public short Roll { get; set; }

    public TRCinematicFrame Clone()
    {
        return new()
        {
            Position = Position.Clone(),
            Target = Target.Clone(),
            FOV = FOV,
            Roll = Roll,
        };
    }

    object ICloneable.Clone()
        => Clone();
}
