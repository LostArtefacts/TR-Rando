using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;

namespace TRFDControl
{
    public class FDControl
    {
        private List<FDEntry> entries;

        public void ParseFromLevel(TR2Level lvl)
        {
            foreach (TR2Room room in lvl.Rooms)
            {
                foreach (TRRoomSector sector in room.SectorList)
                {
                    //Index 0 is a dummy
                    if (sector.FDIndex == 0)
                    {
                        continue;
                    }

                    FDSetup data = new FDSetup();

                    while(data.EndData == false)
                    {
                        data.Value = lvl.FloorData[sector.FDIndex];

                        switch ((FDFunctions)data.Function)
                        {
                            case FDFunctions.PortalSector:
                                break;
                            case FDFunctions.FloorSlant:
                                break;
                            case FDFunctions.CeilingSlant:
                                break;
                            case FDFunctions.Trigger:
                                break;
                            case FDFunctions.KillLara:
                                break;
                            case FDFunctions.ClimbableWalls:
                                break;
                            case FDFunctions.FloorTriangulationNWSE_Solid:
                                break;
                            case FDFunctions.FloorTriangulationNESW_Solid:
                                break;
                            case FDFunctions.CeilingTriangulationNW_Solid:
                                break;
                            case FDFunctions.CeilingTriangulationNE_Solid:
                                break;
                            case FDFunctions.FloorTriangulationNWSE_SW:
                                break;
                            case FDFunctions.FloorTriangulationNWSE_NE:
                                break;
                            case FDFunctions.FloorTriangulationNESW_SW:
                                break;
                            case FDFunctions.FloorTriangulationNESW_NW:
                                break;
                            case FDFunctions.CeilingTriangulationNW_SW:
                                break;
                            case FDFunctions.CeilingTriangulationNW_NE:
                                break;
                            case FDFunctions.CeilingTriangulationNE_NW:
                                break;
                            case FDFunctions.CeilingTriangulationNE_SE:
                                break;
                            case FDFunctions.Monkeyswing:
                                break;
                            case FDFunctions.DeferredTriggeringOrMinecartRotateLeft:
                                break;
                            case FDFunctions.MechBeetleOrMinecartRotateRight:
                                break;
                            default:
                                break;
                        }
                    } 
                }
            }
        }

        public void WriteToLevel(TR2Level lvl)
        {

        }
    }
}
