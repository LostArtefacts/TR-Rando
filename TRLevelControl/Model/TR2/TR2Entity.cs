namespace TRLevelControl.Model;

public class TR2Entity : TREntity<TR2Type>, ICloneable
{
    public short Intensity1 { get; set; }
    public short Intensity2 { get; set; }

    public TR2Entity Clone()
    {
        return (TR2Entity)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public override string ToString()
    {
        return $"{base.ToString()} Intensity1: {Intensity1} Intensity2: {Intensity2}";
    }
}
