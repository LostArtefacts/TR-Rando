namespace TRLevelControl.Model;

public class TRBox
{
    public byte ZMin { get; set; }
    public byte ZMax { get; set; }
    public byte XMin { get; set; }
    public byte XMax { get; set; }
    public short TrueFloor { get; set; }
    public ushort OverlapIndex { get; set; }
}
