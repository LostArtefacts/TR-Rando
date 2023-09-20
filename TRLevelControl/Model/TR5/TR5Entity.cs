namespace TRLevelControl.Model;

public class TR5Entity : TREntity<TR5Type>, ICloneable
{
    public short Intensity { get; set; }
    public short OCB { get; set; }

    public TR5Entity Clone()
    {
        return (TR5Entity)MemberwiseClone();
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
