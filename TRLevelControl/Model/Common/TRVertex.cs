namespace TRLevelControl.Model;

public class TRVertex : ICloneable
{
    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }

    public TRVertex Clone()
    {
        return new()
        {
            X = X,
            Y = Y,
            Z = Z
        };
    }

    object ICloneable.Clone()
        => Clone();
}
