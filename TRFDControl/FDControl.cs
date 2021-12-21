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
        private SortedDictionary<int, List<FDEntry>> _entries; //Key is Sector.FDIndex

        public IReadOnlyDictionary<int, List<FDEntry>> Entries => _entries;

        /// <summary>
        /// Create a slot in the FD for a room sector that currently points to dummy FD.
        /// </summary>
        public void CreateFloorData(TRRoomSector sector)
        {
            int index;
            if (_entries.Count == 0)
            {
                // Highly unlikely that we would have cleared all FD, but in any case the
                // first index should always be 1
                index = 1;
            }
            else
            {
                // Get the highest index, which is the last key as we use a SortedDictionary.
                // Add the total length of the entries at that key to get the next index for
                // allocating to this sector.
                index = _entries.Keys.ToList().Last();
                List<FDEntry> entries = _entries[index];
                foreach (FDEntry entry in entries)
                {
                    index += entry.Flatten().Length;
                }
            }

            _entries.Add(index, new List<FDEntry>());
            sector.FDIndex = (ushort)index;
        }

        /// <summary>
        /// Remove a given room sector's FD and reset its FDIndex to 0.
        /// </summary>
        public void RemoveFloorData(TRRoomSector sector)
        {
            if (_entries.Remove(sector.FDIndex))
            {
                sector.FDIndex = 0;
            }
        }

        public void ParseFromLevel (TRLevel lvl)
        {
            _entries = new SortedDictionary<int, List<FDEntry>>();

            foreach (TRRoom room in lvl.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    ParseFromSector(sector, lvl.FloorData, lvl.NumFloorData);
                }
            }
        }

        public void ParseFromLevel(TR2Level lvl)
        {
            _entries = new SortedDictionary<int, List<FDEntry>>();

            foreach (TR2Room room in lvl.Rooms)
            {
                foreach (TRRoomSector sector in room.SectorList)
                {
                    ParseFromSector(sector, lvl.FloorData, lvl.NumFloorData);
                }
            }
        }

        public void ParseFromLevel(TR3Level lvl)
        {
            _entries = new SortedDictionary<int, List<FDEntry>>();

            foreach (TR3Room room in lvl.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    ParseFromSector(sector, lvl.FloorData, lvl.NumFloorData);
                }
            }
        }

        public void ParseFromLevel(TR4Level lvl)
        {
            _entries = new SortedDictionary<int, List<FDEntry>>();

            foreach (TR4Room room in lvl.LevelDataChunk.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    ParseFromSector(sector, lvl.LevelDataChunk.Floordata, lvl.LevelDataChunk.NumFloorData);
                }
            }
        }

        public void ParseFromLevel(TR5Level lvl)
        {
            _entries = new SortedDictionary<int, List<FDEntry>>();

            foreach (TR5Room room in lvl.LevelDataChunk.Rooms)
            {
                foreach (TRRoomSector sector in room.RoomData.SectorList)
                {
                    ParseFromSector(sector, lvl.LevelDataChunk.Floordata, lvl.LevelDataChunk.NumFloorData);
                }
            }
        }

        private void ParseFromSector(TRRoomSector sector, ushort[] FloorData, uint NumFloorData)
        {
            //Index into FData is FDIndex
            ushort index = sector.FDIndex;

            //Index 0 is a dummy
            if (index == 0)
            {
                return;
            }

            //List of floordata functions for the sector
            List<FDEntry> floordataFunctions = new List<FDEntry>();

            while (true)
            {
                FDSetup data = new FDSetup()
                {
                    Value = FloorData[index]
                };

                switch ((FDFunctions)data.Function)
                {
                    case FDFunctions.PortalSector:

                        FDPortalEntry portal = new FDPortalEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] },
                            Room = FloorData[++index]
                        };

                        floordataFunctions.Add(portal);

                        break;
                    case FDFunctions.FloorSlant:

                        FDSlantEntry floorSlant = new FDSlantEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] },
                            SlantValue = FloorData[++index],
                            Type = FDSlantEntryType.FloorSlant
                        };

                        floordataFunctions.Add(floorSlant);

                        break;
                    case FDFunctions.CeilingSlant:

                        FDSlantEntry ceilingSlant = new FDSlantEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] },
                            SlantValue = FloorData[++index],
                            Type = FDSlantEntryType.CeilingSlant
                        };

                        floordataFunctions.Add(ceilingSlant);

                        break;
                    case FDFunctions.Trigger:

                        FDTriggerEntry trig = new FDTriggerEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] },
                            TrigSetup = new FDTrigSetup() {  Value = FloorData[++index] }
                        };

                        if (trig.TrigType == FDTrigType.Switch || trig.TrigType == FDTrigType.Key)
                        {
                            //First entry in action list is reference to switch/key entity for switch/key types.
                            trig.SwitchOrKeyRef = FloorData[++index];
                        }

                        //We don't know if there are any more yet.
                        bool continueFDParse;

                        //Do not enter do...while if key/switch ref uint16 does not set continue
                        if (trig.SwitchKeyContinue)
                        {
                            //Parse trigactions
                            do
                            {
                                //New trigger action
                                FDActionListItem action = new FDActionListItem() { Value = FloorData[++index] };

                                continueFDParse = action.Continue;

                                if (action.TrigAction == FDTrigAction.Camera || action.TrigAction == FDTrigAction.Flyby)
                                {
                                    //Camera trig actions have a special extra uint16...
                                    FDCameraAction camAction = new FDCameraAction() { Value = FloorData[++index] };

                                    //store associated camera action
                                    action.CamAction = camAction;

                                    //Is there more?
                                    continueFDParse = camAction.Continue;
                                }

                                //add action
                                trig.TrigActionList.Add(action);

                            } while (index < NumFloorData && continueFDParse);
                        }

                        floordataFunctions.Add(trig);

                        break;
                    case FDFunctions.KillLara:

                        FDKillLaraEntry kill = new FDKillLaraEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] }
                        };

                        floordataFunctions.Add(kill);

                        break;
                    case FDFunctions.ClimbableWalls:

                        FDClimbEntry climb = new FDClimbEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] }
                        };

                        floordataFunctions.Add(climb);

                        break;
                    case FDFunctions.FloorTriangulationNWSE_Solid:
                    case FDFunctions.FloorTriangulationNESW_Solid:
                    case FDFunctions.CeilingTriangulationNW_Solid:
                    case FDFunctions.CeilingTriangulationNE_Solid:
                    case FDFunctions.FloorTriangulationNWSE_SW:
                    case FDFunctions.FloorTriangulationNWSE_NE:
                    case FDFunctions.FloorTriangulationNESW_SE:
                    case FDFunctions.FloorTriangulationNESW_NW:
                    case FDFunctions.CeilingTriangulationNW_SW:
                    case FDFunctions.CeilingTriangulationNW_NE:
                    case FDFunctions.CeilingTriangulationNE_NW:
                    case FDFunctions.CeilingTriangulationNE_SE:

                        TR3TriangulationEntry tri = new TR3TriangulationEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] },
                            TriData = new FDTriangulationData() { Value = FloorData[++index] }
                        };

                        floordataFunctions.Add(tri);

                        break;
                    case FDFunctions.Monkeyswing:

                        TR3MonkeySwingEntry swing = new TR3MonkeySwingEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] },
                        };

                        floordataFunctions.Add(swing);

                        break;
                    case FDFunctions.DeferredTriggeringOrMinecartRotateLeft:

                        TR3MinecartRotateLeftEntry mineleft = new TR3MinecartRotateLeftEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] },
                        };

                        floordataFunctions.Add(mineleft);

                        break;
                    case FDFunctions.MechBeetleOrMinecartRotateRight:

                        TR3MinecartRotateRightEntry mineright = new TR3MinecartRotateRightEntry()
                        {
                            Setup = new FDSetup() { Value = FloorData[index] },
                        };

                        floordataFunctions.Add(mineright);

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
            _entries.Add(sector.FDIndex, floordataFunctions);
        }

        public void WriteToLevel(TRLevel lvl)
        {
            List<ushort> data = new List<ushort>
            {
                lvl.FloorData[0] // Index 0 is always dummy
            };

            Dictionary<int, int> entryLengths = new Dictionary<int, int>();

            foreach (KeyValuePair<int, List<FDEntry>> entry in Entries)
            {
                //Get the list of functions per sector
                List<FDEntry> functions = entry.Value;

                //Get the initial sector index into fdata
                int index = entry.Key;
                //Track the total length of the entry
                int entryLength = 0;

                for (int i = 0; i < functions.Count; i++)
                {
                    FDEntry function = functions[i];

                    // Ensure EndData is set on the last function in the list only
                    function.Setup.EndData = i == functions.Count - 1;

                    //Convert function to ushort array
                    ushort[] fdata = function.Flatten();
                    data.AddRange(fdata);

                    //Store how many shorts there are
                    entryLength += fdata.Length;
                }

                // Map the current index to its FD length
                entryLengths.Add(index, entryLength);
            }

            // Recalculate the indices based on the length of the floor data for each.
            // This allows for new entries to be added (or their entries added to) or removed.
            Dictionary<int, int> newIndices = new Dictionary<int, int>();
            int newIndex = 1; // Always start at 1 as 0 is dummy data
            foreach (int index in entryLengths.Keys)
            {
                newIndices.Add(index, newIndex);
                // The next index will be this index plus the number of ushorts against it in the FD
                newIndex += entryLengths[index];
            }

            // Update each TRRoomSector by repointing its FDIndex value to the newly calculated value.
            SortedDictionary<int, List<FDEntry>> _updatedEntries = new SortedDictionary<int, List<FDEntry>>();
            foreach (TRRoom room in lvl.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    ushort index = sector.FDIndex;
                    if (newIndices.ContainsKey(index))
                    {
                        sector.FDIndex = (ushort)newIndices[index];

                        // Map the list of entries against the new index
                        _updatedEntries.Add(sector.FDIndex, _entries[index]);
                    }
                }
            }

            // Update the raw floor data in the level
            lvl.FloorData = data.ToArray();
            lvl.NumFloorData = (uint)data.Count;

            // Update the stored values in case of further changes
            _entries = _updatedEntries;
        }

        public void WriteToLevel(TR2Level lvl)
        {
            List<ushort> data = new List<ushort>
            {
                lvl.FloorData[0] // Index 0 is always dummy
            };

            Dictionary<int, int> entryLengths = new Dictionary<int, int>();

            foreach (KeyValuePair<int, List<FDEntry>> entry in Entries)
            {
                //Get the list of functions per sector
                List<FDEntry> functions = entry.Value;

                //Get the initial sector index into fdata
                int index = entry.Key;
                //Track the total length of the entry
                int entryLength = 0;

                for (int i = 0; i < functions.Count; i++)
                {
                    FDEntry function = functions[i];

                    // Ensure EndData is set on the last function in the list only
                    function.Setup.EndData = i == functions.Count - 1;

                    //Convert function to ushort array
                    ushort[] fdata = function.Flatten();
                    data.AddRange(fdata);

                    //Store how many shorts there are
                    entryLength += fdata.Length;
                }

                // Map the current index to its FD length
                entryLengths.Add(index, entryLength);
            }

            // Recalculate the indices based on the length of the floor data for each.
            // This allows for new entries to be added (or their entries added to) or removed.
            Dictionary<int, int> newIndices = new Dictionary<int, int>();
            int newIndex = 1; // Always start at 1 as 0 is dummy data
            foreach (int index in entryLengths.Keys)
            {
                newIndices.Add(index, newIndex);
                // The next index will be this index plus the number of ushorts against it in the FD
                newIndex += entryLengths[index];
            }

            // Update each TRRoomSector by repointing its FDIndex value to the newly calculated value.
            SortedDictionary<int, List<FDEntry>> _updatedEntries = new SortedDictionary<int, List<FDEntry>>();
            foreach (TR2Room room in lvl.Rooms)
            {
                foreach (TRRoomSector sector in room.SectorList)
                {
                    ushort index = sector.FDIndex;
                    if (newIndices.ContainsKey(index))
                    {
                        sector.FDIndex = (ushort)newIndices[index];

                        // Map the list of entries against the new index
                        _updatedEntries.Add(sector.FDIndex, _entries[index]);
                    }
                }
            }

            // Update the raw floor data in the level
            lvl.FloorData = data.ToArray();
            lvl.NumFloorData = (uint)data.Count;

            // Update the stored values in case of further changes
            _entries = _updatedEntries;
        }

        public void WriteToLevel(TR3Level lvl)
        {
            List<ushort> data = new List<ushort>
            {
                lvl.FloorData[0] // Index 0 is always dummy
            };

            Dictionary<int, int> entryLengths = new Dictionary<int, int>();

            foreach (KeyValuePair<int, List<FDEntry>> entry in Entries)
            {
                //Get the list of functions per sector
                List<FDEntry> functions = entry.Value;

                //Get the initial sector index into fdata
                int index = entry.Key;
                //Track the total length of the entry
                int entryLength = 0;

                for (int i = 0; i < functions.Count; i++)
                {
                    FDEntry function = functions[i];

                    // Ensure EndData is set on the last function in the list only
                    function.Setup.EndData = i == functions.Count - 1;

                    //Convert function to ushort array
                    ushort[] fdata = function.Flatten();
                    data.AddRange(fdata);

                    //Store how many shorts there are
                    entryLength += fdata.Length;
                }

                // Map the current index to its FD length
                entryLengths.Add(index, entryLength);
            }

            // Recalculate the indices based on the length of the floor data for each.
            // This allows for new entries to be added (or their entries added to) or removed.
            Dictionary<int, int> newIndices = new Dictionary<int, int>();
            int newIndex = 1; // Always start at 1 as 0 is dummy data
            foreach (int index in entryLengths.Keys)
            {
                newIndices.Add(index, newIndex);
                // The next index will be this index plus the number of ushorts against it in the FD
                newIndex += entryLengths[index];
            }

            // Update each TRRoomSector by repointing its FDIndex value to the newly calculated value.
            SortedDictionary<int, List<FDEntry>> _updatedEntries = new SortedDictionary<int, List<FDEntry>>();
            foreach (TR3Room room in lvl.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    ushort index = sector.FDIndex;
                    if (newIndices.ContainsKey(index))
                    {
                        sector.FDIndex = (ushort)newIndices[index];

                        // Map the list of entries against the new index
                        _updatedEntries.Add(sector.FDIndex, _entries[index]);
                    }
                }
            }

            // Update the raw floor data in the level
            lvl.FloorData = data.ToArray();
            lvl.NumFloorData = (uint)data.Count;

            // Update the stored values in case of further changes
            _entries = _updatedEntries;
        }

        public void WriteToLevel(TR4Level lvl)
        {
            List<ushort> data = new List<ushort>
            {
                lvl.LevelDataChunk.Floordata[0] // Index 0 is always dummy
            };

            Dictionary<int, int> entryLengths = new Dictionary<int, int>();

            foreach (KeyValuePair<int, List<FDEntry>> entry in Entries)
            {
                //Get the list of functions per sector
                List<FDEntry> functions = entry.Value;

                //Get the initial sector index into fdata
                int index = entry.Key;
                //Track the total length of the entry
                int entryLength = 0;

                for (int i = 0; i < functions.Count; i++)
                {
                    FDEntry function = functions[i];

                    // Ensure EndData is set on the last function in the list only
                    function.Setup.EndData = i == functions.Count - 1;

                    //Convert function to ushort array
                    ushort[] fdata = function.Flatten();
                    data.AddRange(fdata);

                    //Store how many shorts there are
                    entryLength += fdata.Length;
                }

                // Map the current index to its FD length
                entryLengths.Add(index, entryLength);
            }

            // Recalculate the indices based on the length of the floor data for each.
            // This allows for new entries to be added (or their entries added to) or removed.
            Dictionary<int, int> newIndices = new Dictionary<int, int>();
            int newIndex = 1; // Always start at 1 as 0 is dummy data
            foreach (int index in entryLengths.Keys)
            {
                newIndices.Add(index, newIndex);
                // The next index will be this index plus the number of ushorts against it in the FD
                newIndex += entryLengths[index];
            }

            // Update each TRRoomSector by repointing its FDIndex value to the newly calculated value.
            SortedDictionary<int, List<FDEntry>> _updatedEntries = new SortedDictionary<int, List<FDEntry>>();
            foreach (TR4Room room in lvl.LevelDataChunk.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    ushort index = sector.FDIndex;
                    if (newIndices.ContainsKey(index))
                    {
                        sector.FDIndex = (ushort)newIndices[index];

                        // Map the list of entries against the new index
                        _updatedEntries.Add(sector.FDIndex, _entries[index]);
                    }
                }
            }

            // Update the raw floor data in the level
            lvl.LevelDataChunk.Floordata = data.ToArray();
            lvl.LevelDataChunk.NumFloorData = (uint)data.Count;

            // Update the stored values in case of further changes
            _entries = _updatedEntries;
        }

        public void WriteToLevel(TR5Level lvl)
        {
            List<ushort> data = new List<ushort>
            {
                lvl.LevelDataChunk.Floordata[0] // Index 0 is always dummy
            };

            Dictionary<int, int> entryLengths = new Dictionary<int, int>();

            foreach (KeyValuePair<int, List<FDEntry>> entry in Entries)
            {
                //Get the list of functions per sector
                List<FDEntry> functions = entry.Value;

                //Get the initial sector index into fdata
                int index = entry.Key;
                //Track the total length of the entry
                int entryLength = 0;

                for (int i = 0; i < functions.Count; i++)
                {
                    FDEntry function = functions[i];

                    // Ensure EndData is set on the last function in the list only
                    function.Setup.EndData = i == functions.Count - 1;

                    //Convert function to ushort array
                    ushort[] fdata = function.Flatten();
                    data.AddRange(fdata);

                    //Store how many shorts there are
                    entryLength += fdata.Length;
                }

                // Map the current index to its FD length
                entryLengths.Add(index, entryLength);
            }

            // Recalculate the indices based on the length of the floor data for each.
            // This allows for new entries to be added (or their entries added to) or removed.
            Dictionary<int, int> newIndices = new Dictionary<int, int>();
            int newIndex = 1; // Always start at 1 as 0 is dummy data
            foreach (int index in entryLengths.Keys)
            {
                newIndices.Add(index, newIndex);
                // The next index will be this index plus the number of ushorts against it in the FD
                newIndex += entryLengths[index];
            }

            // Update each TRRoomSector by repointing its FDIndex value to the newly calculated value.
            SortedDictionary<int, List<FDEntry>> _updatedEntries = new SortedDictionary<int, List<FDEntry>>();
            foreach (TR5Room room in lvl.LevelDataChunk.Rooms)
            {
                foreach (TRRoomSector sector in room.RoomData.SectorList)
                {
                    ushort index = sector.FDIndex;
                    if (newIndices.ContainsKey(index))
                    {
                        sector.FDIndex = (ushort)newIndices[index];

                        // Map the list of entries against the new index
                        _updatedEntries.Add(sector.FDIndex, _entries[index]);
                    }
                }
            }

            // Update the raw floor data in the level
            lvl.LevelDataChunk.Floordata = data.ToArray();
            lvl.LevelDataChunk.NumFloorData = (uint)data.Count;

            // Update the stored values in case of further changes
            _entries = _updatedEntries;
        }
    }
}
