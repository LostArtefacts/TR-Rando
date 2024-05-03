namespace TRLevelControl.Model;

public class TRBox
{
    public uint ZMin { get; set; }
    public uint ZMax { get; set; }
    public uint XMin { get; set; }
    public uint XMax { get; set; }
    public short TrueFloor { get; set; }
    public bool Blockable { get; set; }
    public bool Blocked { get; set; }
    public List<ushort> Overlaps { get; set; } = new();
    public TRZoneGroup Zone { get; set; } = new();
}
