namespace TRLevelControl.Model;

public class TRAnimation
{
    public uint FrameOffset { get; set; }

    public byte FrameRate { get; set; }

    public byte FrameSize { get; set; }

    public ushort StateID { get; set; }
    public FixedFloat32 Speed { get; set; }
    public FixedFloat32 Accel { get; set; }
    public FixedFloat32 SpeedLateral { get; set; }
    public FixedFloat32 AccelLateral { get; set; }

    public ushort FrameStart { get; set; }

    public ushort FrameEnd { get; set; }

    public ushort NextAnimation { get; set; }

    public ushort NextFrame { get; set; }

    public ushort NumStateChanges { get; set; }

    public ushort StateChangeOffset { get; set; }

    public ushort NumAnimCommands { get; set; }

    public ushort AnimCommand { get; set; }
    public List<TRStateChange> Changes { get; set; } = new();
    public List<TRAnimCommand> Commands { get; set; } = new();
}
