namespace TRLevelControl.Model;

public class TRFixedFloat<T, U>
{
    public T Whole { get; set; }
    public U Fraction { get; set; }
}

public sealed class TRFixedFloat32 : TRFixedFloat<short, ushort> { }
