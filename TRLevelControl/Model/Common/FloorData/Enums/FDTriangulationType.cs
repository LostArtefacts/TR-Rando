namespace TRLevelControl.Model;

public enum FDTriangulationType
{
    FloorNWSE_Solid   = 0x07,
    FloorNESW_Solid   = 0x08,
    CeilingNWSE_Solid = 0x09,
    CeilingNESW_Solid = 0x0A,
    FloorNWSE_SW      = 0x0B,
    FloorNWSE_NE      = 0x0C,
    FloorNESW_SE      = 0x0D, // TRosetta incorrectly names this _SW
    FloorNESW_NW      = 0x0E,    
    CeilingNWSE_SW    = 0x0F,
    CeilingNWSE_NE    = 0x10,
    CeilingNESW_NW    = 0x11,
    CeilingNESW_SE    = 0x12,
}
