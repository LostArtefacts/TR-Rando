namespace TRLevelControl.Model;

public class TR4Entity : TREntity<TR4Type>, ICloneable
{
    public short Intensity { get; set; }
    public short OCB { get; set; }

    public TR4Entity Clone()
    {
        return (TR4Entity)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public override string ToString()
    {
        return $"{base.ToString()} Intensity: {Intensity} OCB: {OCB}";
    }
}
