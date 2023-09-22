namespace TRLevelControl.Model;

public class TR3Entity : TREntity<TR3Type>, ICloneable
{
    public short Intensity1 { get; set; }
    public short Intensity2 { get; set; }

    public TR3Entity Clone()
    {
        return (TR3Entity)MemberwiseClone();
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
