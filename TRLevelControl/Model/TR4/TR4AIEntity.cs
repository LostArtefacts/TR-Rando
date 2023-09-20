namespace TRLevelControl.Model;

public class TR4AIEntity : TREntity<TR4Type>, ICloneable
{
    public short OCB { get; set; }
    public short Box { get; set; }

    public TR4AIEntity Clone()
    {
        return (TR4AIEntity)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public override string ToString()
    {
        return $"{base.ToString()} OCB: {OCB} Box: {Box}";
    }
}
