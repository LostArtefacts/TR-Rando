namespace TRLevelControl.Model;

public class TRObjectTextureVert : ICloneable
{
    public ushort U { get; set; }
    public ushort V { get; set; }

    public bool IsEmpty
        => U == 0 && V == 0;

    public TRObjectTextureVert()
        : this(0, 0) { }

    public TRObjectTextureVert(ushort x, ushort y)
    {
        U = V = 1;
        X = x;
        Y = y;
    }

    public int X
    {
        get => (U & 0xFF00) >> 8;
        set
        {
            U = (ushort)((((byte)value) << 8) | (U & 0xFF));
        }
    }

    public int Y
    {
        get => (V & 0xFF00) >> 8;
        set
        {
            V = (ushort)((((byte)value) << 8) | (V & 0xFF));
        }
    }

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}";
    }

    public TRObjectTextureVert Clone()
    {
        return new()
        {
            U = U,
            V = V
        };
    }

    object ICloneable.Clone()
        => Clone();
}
