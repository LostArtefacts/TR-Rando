namespace TRLevelControl.Model;

public enum TRFileVersion : uint
{
    Unknown = 0,
    TR1     = 0x00000020,
    TR2     = 0x0000002D,
    TR3a    = 0xFF080038,
    TR3b    = 0xFF180038,
    TR45    = 0x00345254,
}
