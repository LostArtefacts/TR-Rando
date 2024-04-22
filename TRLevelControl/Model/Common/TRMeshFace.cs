namespace TRLevelControl.Model;

public class TRMeshFace : TRFace, ICloneable
{
    public ushort Effects { get; set; }

    public bool UseAlphaBlending
    {
        get => (Effects & 0x01) > 0;
        set
        {
            if (value)
            {
                Effects |= 0x01;
            }
            else
            {
                Effects = (ushort)(Effects & ~0x01);
            }
        }
    }

    public bool UseEnvironmentMapping
    {
        get => (Effects & 0x02) > 0;
        set
        {
            if (value)
            {
                Effects |= 0x02;
            }
            else
            {
                Effects = (ushort)(Effects & ~0x02);
            }
        }
    }

    public byte EnvironmentMappingStrength
    {
        get => (byte)((Effects & 0xFC) >> 2);
        set
        {
            Effects = (ushort)(Effects & ~0xFC);
            Effects |= (ushort)((value << 2) & 0xFC);
        }
    }

    public TRMeshFace Clone()
    {
        return new()
        {
            Type = Type,
            Vertices = new(Vertices),
            Texture = Texture,
            DoubleSided = DoubleSided,
            UnknownFlag = UnknownFlag,
            Effects = Effects,
        };
    }

    object ICloneable.Clone()
        => Clone();
}
