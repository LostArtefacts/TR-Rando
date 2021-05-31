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
        public Dictionary<int, FDEntry> Entries = new Dictionary<int, FDEntry>(); //Key is Sector.FDIndex

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

                                Entries.Add(sector.FDIndex, portal);

                                break;
                            case FDFunctions.FloorSlant:
                                //Ignore for now...
                                break;
                            case FDFunctions.CeilingSlant:
                                //Ignore for now...
                                break;
                            case FDFunctions.Trigger:

                                FDTriggerEntry trig = new FDTriggerEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] },
                                    TrigSetup = new FDTrigSetup() {  Value = lvl.FloorData[++index] }
                                };

                                if (trig.TrigType == FDTrigType.Switch || trig.TrigType == FDTrigType.Key)
                                {
                                    //First entry in action list is reference to switch/key entity for switch/key types.
                                    trig.SwitchOrKeyRef = lvl.FloorData[++index];
                                }

                                //We don't know if there are any more yet.
                                bool continueFDParse = false;

                                //Parse trigactions
                                do
                                {
                                    //New trigger action
                                    FDActionListItem action = new FDActionListItem() { Value = lvl.FloorData[++index] };

                                    //Add action
                                    trig.TrigActionList.Add(action);

                                    //Is there more?
                                    continueFDParse = action.Continue;

                                    if (action.TrigAction == FDTrigAction.Camera)
                                    {
                                        //Camera trig actions have a special extra uint16...
                                        FDCameraAction camAction = new FDCameraAction() { Value = lvl.FloorData[++index] };

                                        //Is there more?
                                        continueFDParse = camAction.Continue;
                                    }
                                } while (index < lvl.NumFloorData && continueFDParse);

                                Entries.Add(sector.FDIndex, trig);

                                break;
                            case FDFunctions.KillLara:

                                FDKillLaraEntry kill = new FDKillLaraEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] }
                                };

                                Entries.Add(sector.FDIndex, kill);

                                break;
                            case FDFunctions.ClimbableWalls:

                                FDClimbEntry climb = new FDClimbEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] }
                                };

                                Entries.Add(sector.FDIndex, climb);

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
