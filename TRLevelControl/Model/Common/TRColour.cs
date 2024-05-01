namespace TRLevelControl.Model;

public class TRColour : ICloneable
{
    public byte Red { get; set; }
    public byte Green { get; set; }    
    public byte Blue { get; set; }

    public TRColour() { }

    public TRColour(ushort rgb555)
    {
        Red = (byte)(((rgb555 & 0x7C00) >> 10) << 3);
        Green = (byte)(((rgb555 & 0x03E0) >> 5) << 3);
        Blue = (byte)((rgb555 & 0x001F) << 3);
    }

    public virtual ushort ToRGB555()
    {
        return (ushort)(((Red >> 3) << 10) | ((Green >> 3) << 5) | (Blue >> 3));
    }

    public TRColour Clone()
        => (TRColour)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
