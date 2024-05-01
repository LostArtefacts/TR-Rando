namespace TRLevelControl.Model;

public class TR3RoomLight : ICloneable
{
    public TR3RoomLightType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public TRColour Colour { get; set; }
    public short[] LightProperties { get; set; }

    public TR3RoomLight Clone()
    {
        return new()
        {
            Type = Type,
            X = X,
            Y = Y,
            Z = Z,
            Colour = Colour.Clone(),
            LightProperties = LightProperties.ToList().ToArray()
        };
    }

    object ICloneable.Clone()
        => Clone();
}
