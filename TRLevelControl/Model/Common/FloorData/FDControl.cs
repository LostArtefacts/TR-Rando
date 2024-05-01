using TRFDControl.FDEntryTypes;
using TRLevelControl.Model;

namespace TRFDControl;

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

    public void ParseFromLevel (TR1Level level)
    {
        ParseLevel(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    public void ParseFromLevel(TR2Level level)
    {
        ParseLevel(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    public void ParseFromLevel(TR3Level level)
    {
        ParseLevel(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    public void ParseFromLevel(TR4Level level)
    {
        ParseLevel(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    public void ParseFromLevel(TR5Level level)
    {
        ParseLevel(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    private void ParseLevel(IEnumerable<TRRoomSector> roomSectors, List<ushort> floorData)
    {
        _entries = new();
        foreach (TRRoomSector sector in roomSectors)
        {
            ParseFromSector(sector, floorData);
        }
    }

    private void ParseFromSector(TRRoomSector sector, List<ushort> floorData)
    {
        ushort index = sector.FDIndex;
        // Index 0 is always dummy, so NOOP.
        if (index == 0)
        {
            return;
        }

        // List of floordata functions for this sector.
        List<FDEntry> functions = new();
        _entries[sector.FDIndex] = functions;

        while (true)
        {
            FDSetup data = new()
            {
                Value = floorData[index]
            };

            switch ((FDFunctions)data.Function)
            {
                case FDFunctions.PortalSector:
                    functions.Add(new FDPortalEntry
                    {
                        Setup = new() { Value = floorData[index] },
                        Room = floorData[++index]
                    });
                    break;

                case FDFunctions.FloorSlant:
                    functions.Add(new FDSlantEntry
                    {
                        Setup = new() { Value = floorData[index] },
                        SlantValue = floorData[++index],
                        Type = FDSlantEntryType.FloorSlant
                    });
                    break;

                case FDFunctions.CeilingSlant:
                    functions.Add(new FDSlantEntry
                    {
                        Setup = new() { Value = floorData[index] },
                        SlantValue = floorData[++index],
                        Type = FDSlantEntryType.CeilingSlant
                    });
                    break;

                case FDFunctions.Trigger:

                    FDTriggerEntry trig = new()
                    {
                        Setup = new FDSetup() { Value = floorData[index] },
                        TrigSetup = new FDTrigSetup() {  Value = floorData[++index] }
                    };
                    functions.Add(trig);

                    if (trig.TrigType == FDTrigType.Switch || trig.TrigType == FDTrigType.Key)
                    {
                        // First entry in action list is reference to switch/key entity for switch/key types.
                        trig.SwitchOrKeyRef = floorData[++index];
                    }

                    // We don't know if there are any more yet.
                    bool continueFDParse;

                    // Do not enter do...while if key/switch ref uint16 does not set continue
                    if (trig.SwitchKeyContinue)
                    {
                        // Parse trigactions
                        do
                        {
                            // New trigger action
                            FDActionListItem action = new() { Value = floorData[++index] };
                            trig.TrigActionList.Add(action);

                            continueFDParse = action.Continue;

                            if (action.TrigAction == FDTrigAction.Camera || action.TrigAction == FDTrigAction.Flyby)
                            {
                                // Camera trig actions have a special extra uint16...
                                FDCameraAction camAction = new() { Value = floorData[++index] };

                                // store associated camera action
                                action.CamAction = camAction;

                                // Is there more?
                                continueFDParse = camAction.Continue;
                            }
                        }
                        while (index < floorData.Count && continueFDParse);
                    }
                    break;

                case FDFunctions.KillLara:
                    functions.Add(new FDKillLaraEntry
                    {
                        Setup = new() { Value = floorData[index] }
                    });
                    break;

                case FDFunctions.ClimbableWalls:
                    functions.Add(new FDClimbEntry
                    {
                        Setup = new() { Value = floorData[index] }
                    });
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
                    functions.Add(new TR3TriangulationEntry
                    {
                        Setup = new() { Value = floorData[index] },
                        TriData = new() { Value = floorData[++index] }
                    });
                    break;

                case FDFunctions.Monkeyswing:
                    functions.Add(new TR3MonkeySwingEntry
                    {
                        Setup = new() { Value = floorData[index] },
                    });
                    break;

                case FDFunctions.DeferredTriggeringOrMinecartRotateLeft:
                    functions.Add(new TR3MinecartRotateLeftEntry
                    {
                        Setup = new() { Value = floorData[index] },
                    });
                    break;

                case FDFunctions.MechBeetleOrMinecartRotateRight:
                    functions.Add(new TR3MinecartRotateRightEntry
                    {
                        Setup = new() { Value = floorData[index] },
                    });
                    break;

                default:
                    break;
            }

            if (data.EndData)
            {
                // End data (from what I understand) means there is no further functions for this sector.
                // E.G. Sector 52 on Xian has a slant function and portal function. EndData is not set on
                // slant function, but is on portal function as there are no further functions.
                break;
            }
            else
            {
                // There are further functions for this sector - continue parsing.
                index++;
            }
        }
    }

    public void WriteToLevel(TR1Level level)
    {
        Flatten(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    public void WriteToLevel(TR2Level level)
    {
        Flatten(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    public void WriteToLevel(TR3Level level)
    {
        Flatten(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    public void WriteToLevel(TR4Level level)
    {
        Flatten(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    public void WriteToLevel(TR5Level level)
    {
        Flatten(level.Rooms.SelectMany(r => r.Sectors), level.FloorData);
    }

    private void Flatten(IEnumerable<TRRoomSector> sectors, List<ushort> data)
    {
        ushort dummyEntry = data.Count > 0 ? data[0] : (ushort)0;
        data.Clear();
        data.Add(dummyEntry);

        // Flatten each entry list and map old indices to new.
        Dictionary<int, int> newIndices = new();
        foreach (int currentIndex in _entries.Keys)
        {
            List<ushort> sectorData = Flatten(_entries[currentIndex]);
            if (sectorData.Count > 0)
            {
                newIndices.Add(currentIndex, data.Count);
                data.AddRange(sectorData);
            }
        }

        // Update each TRRoomSector by repointing its FDIndex value to the newly calculated value.
        SortedDictionary<int, List<FDEntry>> updatedEntries = new();
        foreach (TRRoomSector sector in sectors)
        {
            ushort index = sector.FDIndex;
            if (newIndices.ContainsKey(index))
            {
                sector.FDIndex = (ushort)newIndices[index];

                // Map the list of entries against the new index
                updatedEntries[sector.FDIndex] = _entries[index];
            }
            else if (_entries.ContainsKey(index))
            {
                // FD has been removed - we only reset it if it was a valid entry before
                // because some levels, e.g. most of TRUB, have stale data.
                sector.FDIndex = 0;
            }
        }

        // Update the stored values in case of further changes
        _entries = updatedEntries;
    }

    public static List<ushort> Flatten(List<FDEntry> entries)
    {
        List<ushort> data = new();
        for (int i = 0; i < entries.Count; i++)
        {
            FDEntry function = entries[i];

            // Ensure EndData is set on the last function in the list only
            function.Setup.EndData = i == entries.Count - 1;

            //Convert function to ushort array
            ushort[] fdata = function.Flatten();
            data.AddRange(fdata);
        }

        return data;
    }
}
