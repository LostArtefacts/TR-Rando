namespace TRLevelControl.Model;

public interface ITREntity : ICloneable
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public short Room { get; set; }
    public short Angle { get; set; }
}
