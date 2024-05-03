namespace TRLevelControl.Model;

public class TRBox
{
    public byte ZMin { get; set; }
    public byte ZMax { get; set; }
    public byte XMin { get; set; }
    public byte XMax { get; set; }
    public short TrueFloor { get; set; }
    public bool Blockable { get; set; }
    public bool Blocked { get; set; }
    public List<ushort> Overlaps { get; set; } = new();
    public TRZoneGroup Zone { get; set; } = new();
}
