using System.Text;

namespace TRLevelControl.Model;

public class TR2Entity : ICloneable
{
    public short TypeID { get; set; }

    public short Room { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public short Angle { get; set; }

    public short Intensity1 { get; set; }

    public short Intensity2 { get; set; }

    public ushort Flags { get; set; }

    public bool ClearBody
    {
        get => (Flags & 0x8000) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x8000;
            }
            else
            {
                Flags = (ushort)(Flags & ~0x8000);
            }
        }
    }

    public bool Invisible
    {
        get => (Flags & 0x100) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x100;
            }
            else
            {
                Flags = (ushort)(Flags & ~0x100);
            }
        }
    }

    public ushort CodeBits
    {
        get => (ushort)((Flags & 0x3E00) >> 9);
        set
        {
            Flags = (ushort)(Flags & ~(Flags & 0x3E00));
            Flags |= (ushort)(value << 9);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" TypeID: " + TypeID);
        sb.Append(" Room: " + Room);
        sb.Append(" X: " + X);
        sb.Append(" Y: " + Y);
        sb.Append(" Z: " + Z);
        sb.Append(" Angle: " + Angle);
        sb.Append(" Int1: " + Intensity1);
        sb.Append(" Int2: " + Intensity2);
        sb.Append(" Flags " + Flags.ToString("X4"));

        return sb.ToString();
    }

    public TR2Entity Clone()
    {
        return (TR2Entity)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
}
