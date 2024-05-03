namespace TRLevelControl.Model;

public class TRFlybyCamera
{
    public TRVertex32 Position { get; set; }
    public TRVertex32 Target { get; set; }
    public ushort FOV { get; set; }
    public short Roll { get; set; }
    public ushort Timer { get; set; }
    public ushort Speed { get; set; }
    public TRFlybyFlags Flags { get; set; }
    public uint Room { get; set; }
}
