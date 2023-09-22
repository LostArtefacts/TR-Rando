namespace TRLevelControl.Model;

public class TR1Entity : TREntity<TR1Type>, ICloneable
{
    public short Intensity { get; set; }

    public TR1Entity Clone()
    {
        return new()
        {
            TypeID = TypeID,
            Room = Room,
            X = X,
            Y = Y,
            Z = Z,
            Angle = Angle,
            Intensity = Intensity,
            Flags = Flags,
        };
    }

    object ICloneable.Clone()
        => Clone();

    public override string ToString()
    {
        return $"{base.ToString()} Intensity: {Intensity}";
    }
}
