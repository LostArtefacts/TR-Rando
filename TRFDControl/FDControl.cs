using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRFDControl.FDEntryTypes;
using TRLevelReader.Model;

namespace TRFDControl
{
    public class FDControl
    {
        public Dictionary<int, List<FDEntry>> Entries = new Dictionary<int, List<FDEntry>>(); //Key is Sector.FDIndex

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

                    //List of floordata functions for the sector
                    List<FDEntry> floordataFunctions = new List<FDEntry>();

                    while (true)
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

                                floordataFunctions.Add(portal);

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

                                    continueFDParse = action.Continue;

                                    if (action.TrigAction == FDTrigAction.Camera)
                                    {
                                        //Camera trig actions have a special extra uint16...
                                        FDCameraAction camAction = new FDCameraAction() { Value = lvl.FloorData[++index] };

                                        //store associated camera action
                                        action.CamAction = camAction;

                                        //Is there more?
                                        continueFDParse = camAction.Continue;
                                    }

                                    //add action
                                    trig.TrigActionList.Add(action);
                                    
                                } while (index < lvl.NumFloorData && continueFDParse);

                                floordataFunctions.Add(trig);

                                break;
                            case FDFunctions.KillLara:

                                FDKillLaraEntry kill = new FDKillLaraEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] }
                                };

                                floordataFunctions.Add(kill);

                                break;
                            case FDFunctions.ClimbableWalls:

                                FDClimbEntry climb = new FDClimbEntry()
                                {
                                    Setup = new FDSetup() { Value = lvl.FloorData[index] }
                                };

                                floordataFunctions.Add(climb);

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

                    //Store the sector index and all of its associated functions
                    Entries.Add(sector.FDIndex, floordataFunctions);
                }
            }
        }

        public void WriteToLevel(TR2Level lvl)
        {
            foreach (KeyValuePair<int, List<FDEntry>> entry in Entries)
            {
                //Get the list of functions per sector
                List<FDEntry> functions = entry.Value;

                //Get the initial sector index into fdata
                int index = entry.Key;

                foreach (FDEntry function in functions)
                {
                    //Convert function to ushort array
                    ushort[] fdata = function.Flatten();

                    //store how many shorts there are
                    int fdataCount = fdata.Count();

                    //Copy the array into the level floordata array at sector index
                    Array.Copy(fdata, 0, lvl.FloorData, index, fdataCount);

                    //Increment index so the next function data is following previous
                    index += fdataCount;
                }
            }
        }
    }
}
