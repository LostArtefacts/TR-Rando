using System.Numerics;

namespace TRLevelControl.Model;

public class TR4RoomLight : ICloneable
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public TRColour Colour { get; set; }
    public TR4RoomLightType Type { get; set; }
    public ushort Intensity { get; set; }
    public float Inner { get; set; }
    public float Outer { get; set; }
    public float Length { get; set; }
    public float CutOff { get; set; }
    public Vector3 Direction { get; set; }

    public TR4RoomLight Clone()
        => (TR4RoomLight)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
