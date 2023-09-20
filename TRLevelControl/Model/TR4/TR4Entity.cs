namespace TRLevelControl.Model;

public class TR4Entity
{
    public short TypeID { get; set; }

    public short Room { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public short Angle { get; set; }

    public short Intensity { get; set; }

    public short OCB { get; set; }

    public ushort Flags { get; set; }
}
