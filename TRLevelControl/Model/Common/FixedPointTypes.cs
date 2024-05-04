namespace TRLevelControl.Model;

public class FixedFloat<T, U>
{
    public T Whole { get; set; }
    public U Fraction { get; set; }
}

public sealed class FixedFloat32 : FixedFloat<short, ushort> { }
public sealed class FixedFloat16 : FixedFloat<byte, byte> { }
