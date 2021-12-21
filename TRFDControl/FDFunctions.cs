using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl
{
    public enum FDFunctions
    {
        PortalSector = 0x01,
        FloorSlant = 0x02,
        CeilingSlant = 0x03,
        Trigger = 0x04,
        KillLara = 0x05,
        ClimbableWalls = 0x06,
        FloorTriangulationNWSE_Solid = 0x07,
        FloorTriangulationNESW_Solid = 0x08,
        CeilingTriangulationNW_Solid = 0x09,
        CeilingTriangulationNE_Solid = 0x0A,
        FloorTriangulationNWSE_SW = 0x0B,
        FloorTriangulationNWSE_NE = 0x0C,
        FloorTriangulationNESW_SE = 0x0D, // TRosetta names this _SW but should be SE
        FloorTriangulationNESW_NW = 0x0E,
        CeilingTriangulationNW_SW = 0x0F,
        CeilingTriangulationNW_NE = 0x10,
        CeilingTriangulationNE_NW = 0x11,
        CeilingTriangulationNE_SE = 0x12,
        Monkeyswing = 0x13,
        DeferredTriggeringOrMinecartRotateLeft = 0x14,
        MechBeetleOrMinecartRotateRight = 0x15
    }
}
