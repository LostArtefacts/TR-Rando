namespace TRLevelControl.Model;

// Rename eventually to something like TRXYZ<T>; merge TR5Vertex as well
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

public class TRVertex32 : ICloneable
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public TRVertex32 Clone()
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

public class TRVertex8 : ICloneable
{
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte Z { get; set; }

    public TRVertex8 Clone()
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
