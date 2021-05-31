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
        //FDIndex - Entry
        private Dictionary<int, FDEntry> entries = new Dictionary<int, FDEntry>();

        public void ParseFromLevel(TR2Level lvl)
        {
            foreach (TR2Room room in lvl.Rooms)
            {
                foreach (TRRoomSector sector in room.SectorList)
                {
                    //Index into FData is FDIndex
                    ushort index = sector.FDIndex;

                    //Index 0 is a dummy
                    if (index == 0)
                    {
                        continue;
                    }
                    
                    while(true)
                    {
                        FDSetup data = new FDSetup()
                        {
                            Value = lvl.FloorData[index]
                        };

                        switch ((FDFunctions)data.Function)
                        {
                            case FDFunctions.PortalSector:

                                FDPortalEntry portal = new FDPortalEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] },
                                    Room = lvl.FloorData[++index]
                                };

                                entries.Add(sector.FDIndex, portal);

                                break;
                            case FDFunctions.FloorSlant:
                                //Ignore for now...
                                break;
                            case FDFunctions.CeilingSlant:
                                //Ignore for now...
                                break;
                            case FDFunctions.Trigger:
                                //Subfunction is FDTrigType...
                                //Next uint is FDTrigSetup...
                                //Chain of FDTrigActionListItem s following FDTrigSetup...

                                FDTriggerEntry trig = new FDTriggerEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] },
                                    TrigSetup = new FDTrigSetup() {  Value = lvl.FloorData[++index] }
                                };

                                entries.Add(sector.FDIndex, trig);

                                break;
                            case FDFunctions.KillLara:

                                FDKillLaraEntry kill = new FDKillLaraEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] }
                                };

                                entries.Add(sector.FDIndex, kill);

                                break;
                            case FDFunctions.ClimbableWalls:

                                FDClimbEntry climb = new FDClimbEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] }
                                };

                                entries.Add(sector.FDIndex, climb);

                                break;
                            case FDFunctions.FloorTriangulationNWSE_Solid:
                                //Ignore for now...
                                break;
                            case FDFunctions.FloorTriangulationNESW_Solid:
                                //Ignore for now...
                                break;
                            case FDFunctions.CeilingTriangulationNW_Solid:
                                //Ignore for now...
                                break;
                            case FDFunctions.CeilingTriangulationNE_Solid:
                                //Ignore for now...
                                break;
                            case FDFunctions.FloorTriangulationNWSE_SW:
                                //Ignore for now...
                                break;
                            case FDFunctions.FloorTriangulationNWSE_NE:
                                //Ignore for now...
                                break;
                            case FDFunctions.FloorTriangulationNESW_SW:
                                //Ignore for now...
                                break;
                            case FDFunctions.FloorTriangulationNESW_NW:
                                //Ignore for now...
                                break;
                            case FDFunctions.CeilingTriangulationNW_SW:
                                //Ignore for now...
                                break;
                            case FDFunctions.CeilingTriangulationNW_NE:
                                //Ignore for now...
                                break;
                            case FDFunctions.CeilingTriangulationNE_NW:
                                //Ignore for now...
                                break;
                            case FDFunctions.CeilingTriangulationNE_SE:
                                //Ignore for now...
                                break;
                            case FDFunctions.Monkeyswing:
                                //Ignore for now...
                                break;
                            case FDFunctions.DeferredTriggeringOrMinecartRotateLeft:
                                //Ignore for now...
                                break;
                            case FDFunctions.MechBeetleOrMinecartRotateRight:
                                //Ignore for now...
                                break;
                            default:
                                break;
                        }

                        if (data.EndData)
                        {
                            //End data (from what I understand) means there is no further functions for this sector.
                            //E.G. Sector 52 on Xian has a slant function and portal function. EndData is not set on
                            //slant function, but is on portal function as there are no further functions.
                            break;
                        }
                        else
                        {
                            //There are further functions for this sector - continue parsing.
                            index++;
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
