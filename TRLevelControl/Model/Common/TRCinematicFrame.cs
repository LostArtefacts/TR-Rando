namespace TRLevelControl.Model;

public class TRCinematicFrame
{
    public TRVertex Position { get; set; }
    public TRVertex Target { get; set; }
    public short FOV { get; set; }
    public short Roll { get; set; }
}
