namespace TRLevelControl.Model;

public class TRColour4 : TRColour
{
    public byte Alpha { get; set; }

    public TRColour4() { }

    public TRColour4(ushort rgb555)
        : base(rgb555)
    {
        Alpha = (byte)((rgb555 & 0x8000) >> 15);
    }

    public TRColour4(uint argb)
    {
        Alpha = (byte)((argb & 0xFF000000) >> 24);
        Red = (byte)((argb & 0xFF0000) >> 16);
        Green = (byte)((argb & 0xFF00) >> 8);
        Blue = (byte)(argb & 0xFF);
    }

    public override ushort ToRGB555()
    {
        ushort value = base.ToRGB555();
        if (Alpha > 0)
        {
            value |= 0x8000;
        }
        return value;
    }

    public uint ToARGB()
    {
        return (uint)((Alpha << 24) | (Red << 16) | (Green << 8) | Blue);
    }

    public new TRColour4 Clone()
        => (TRColour4)MemberwiseClone();
}
