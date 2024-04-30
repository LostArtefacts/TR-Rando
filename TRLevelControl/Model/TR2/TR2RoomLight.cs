namespace TRLevelControl.Model;

public class TR2RoomLight : ICloneable
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public ushort Intensity1 { get; set; }
    public ushort Intensity2 { get; set; }
    public uint Fade1 { get; set; }
    public uint Fade2 { get; set; }

    public TR2RoomLight Clone()
        => (TR2RoomLight)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
