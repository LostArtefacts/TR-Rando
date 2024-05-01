namespace TRLevelControl.Model;

[Flags]
public enum TRRoomFlag
{
    None               = 0x0000,
    Water              = 0x0001, // TR1
    Skybox             = 0x0008, // TR2+
    DynamicLit         = 0x0010,
    Wind               = 0x0020,
    Inside             = 0x0040,
    SwampOrNoLensflare = 0x0080, // TR3+
    Mist               = 0x0100, // TR5
    Reflect            = 0x0200,
    Unknown1           = 0x0400,
    Unknown2           = 0x0800,
    ReflectCeiling     = 0x1000,
    Unused1            = 0x2000,
    Unused2            = 0x4000,
}
