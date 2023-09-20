namespace TRLevelControl.Model;

public class TR4AIEntity
{
    public short TypeID { get; set; }
    public short Room { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public short Angle { get; set; }
    public ushort Flags { get; set; }
    public short OCB { get; set; }
    public short Box { get; set; }
}
