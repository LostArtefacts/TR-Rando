namespace TRLevelControl.Model;

public class TR5AIEntity : TREntity<TR5Type>, ICloneable
{
    public short OCB { get; set; }
    public short Box { get; set; }

    public TR5AIEntity Clone()
    {
        return (TR5AIEntity)MemberwiseClone();
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
