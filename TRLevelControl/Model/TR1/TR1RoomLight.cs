namespace TRLevelControl.Model;

public class TR1RoomLight : ICloneable
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public ushort Intensity { get; set; }
    public uint Fade { get; set; }

    public TR1RoomLight Clone()
        => (TR1RoomLight)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
