using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRFDBuilder
{
    private readonly TRGameVersion _version;
    private readonly ITRLevelObserver _observer;

    public TRFDBuilder(TRGameVersion version, ITRLevelObserver observer = null)
    {
        _version = version;
        _observer = observer;
    }

    public FDControl ReadFloorData(TRLevelReader reader, IEnumerable<TRRoomSector> sectors)
    {
        uint numFloorData = reader.ReadUInt32();
        ushort[] data = reader.ReadUInt16s(numFloorData);

        FDControl floorData = new(_version, _observer, data.Length == 0 ? (ushort)0 : data[0]);
        if (_observer?.UseOriginalFloorData ?? false)
        {
            int index = 0;
            while (++index < data.Length)
            {
                floorData[index] = ReadFromIndex(ref index, data);
            }
        }
        else
        {
            Dictionary<TRRoomSector, List<FDEntry>> expandedFunctions = new();
            foreach (TRRoomSector sector in sectors)
            {
                int index = sector.FDIndex;
                if (index == 0)
                {
                    continue;
                }

                List<FDEntry> functions = ReadFromIndex(ref index, data);
                if (floorData.ContainsKey(sector.FDIndex))
                {
                    expandedFunctions[sector] = functions;
                }
                else
                {
                    floorData[sector.FDIndex] = functions;
                }
            }

            foreach (var (sector, functions) in expandedFunctions)
            {
                floorData.CreateFloorData(sector);
                floorData[sector.FDIndex] = functions;
            }
        }

        return floorData;
    }

    private List<FDEntry> ReadFromIndex(ref int index, ushort[] data)
    {
        List<FDEntry> functions = new();

        while (true)
        {
            ushort value = data[index];
            FDFunction function = (FDFunction)(value & 0x001F);
            switch (function)
            {
                case FDFunction.PortalSector:
                    functions.Add(new FDPortalEntry()
                    {
                        Room = (short)data[++index]
                    });
                    break;

                case FDFunction.FloorSlant:
                case FDFunction.CeilingSlant:
                    ushort slantData = data[++index];
                    functions.Add(new FDSlantEntry
                    {
                        Type = (FDSlantType)function,
                        XSlant = (sbyte)(slantData & 0x00FF),
                        ZSlant = (sbyte)((slantData & 0xFF00) >> 8)
                    });
                    break;

                case FDFunction.Trigger:
                    ushort trigSetup = data[++index];
                    FDTriggerEntry trig = new()
                    {
                        TrigType = (FDTrigType)((value & 0x7F00) >> 8),
                        Timer = (byte)(trigSetup & 0x00FF),
                        OneShot = (trigSetup & 0x0100) > 0,
                        Mask = (byte)((trigSetup & 0x3E00) >> 9)
                    };
                    functions.Add(trig);

                    bool done = false;
                    if (trig.TrigType == FDTrigType.Switch || trig.TrigType == FDTrigType.Key)
                    {
                        ushort switchRef = data[++index];
                        trig.SwitchOrKeyRef = (short)(switchRef & 0x7FFF);
                        done = (switchRef & 0x8000) > 0;
                    }

                    while (!done && index < data.Length)
                    {
                        ushort actionData = data[++index];
                        FDActionItem action = new()
                        {
                            Action = (FDTrigAction)((actionData & 0x7C00) >> 10),
                            Parameter = (short)(actionData & 0x03FF)
                        };
                        trig.Actions.Add(action);

                        done = (actionData & 0x8000) > 0;

                        if (action.Action == FDTrigAction.Camera || action.Action == FDTrigAction.Flyby)
                        {
                            ushort camData = data[++index];
                            action.CamAction = new()
                            {
                                Timer = (byte)(camData & 0x00FF),
                                Once = (camData & 0x0100) > 0,
                                MoveTimer = (byte)((camData & 0x3E00) >> 9)
                            };

                            done = (camData & 0x8000) > 0;
                        }
                    }

                    break;
                case FDFunction.KillLara:
                    functions.Add(new FDKillLaraEntry());
                    break;

                case FDFunction.ClimbableWalls:
                    functions.Add(new FDClimbEntry
                    {
                        Direction = (FDClimbDirection)((value & 0x7F00) >> 8)
                    });
                    break;

                case FDFunction.FloorTriangulationNWSE_Solid:
                case FDFunction.FloorTriangulationNESW_Solid:
                case FDFunction.CeilingTriangulationNW_Solid:
                case FDFunction.CeilingTriangulationNE_Solid:
                case FDFunction.FloorTriangulationNWSE_SW:
                case FDFunction.FloorTriangulationNWSE_NE:
                case FDFunction.FloorTriangulationNESW_SE:
                case FDFunction.FloorTriangulationNESW_NW:
                case FDFunction.CeilingTriangulationNW_SW:
                case FDFunction.CeilingTriangulationNW_NE:
                case FDFunction.CeilingTriangulationNE_NW:
                case FDFunction.CeilingTriangulationNE_SE:
                    ushort triData = data[++index];
                    functions.Add(new FDTriangulationEntry
                    {
                        Type = (FDTriangulationType)function,
                        C10 = (byte)(triData & 0x000F),
                        C00 = (byte)((triData & 0x00F0) >> 4),
                        C01 = (byte)((triData & 0x0F00) >> 8),
                        C11 = (byte)((triData & 0xF000) >> 12),
                        H1 = (sbyte)((value & 0x03E0) >> 5),
                        H2 = (sbyte)((value & 0x7C00) >> 10)
                    });
                    break;

                case FDFunction.Monkeyswing:
                    functions.Add(new FDMonkeySwingEntry());
                    break;

                case FDFunction.DeferredTrigOrMinecartRotateLeft:
                    if (_version < TRGameVersion.TR4)
                    {
                        functions.Add(new FDMinecartEntry
                        {
                            Type = (FDMinecartType)function
                        });
                    }
                    else
                    {
                        functions.Add(new FDDeferredTriggerEntry());
                    }
                    break;

                case FDFunction.MechBeetleOrMinecartRotateRight:
                    if (_version < TRGameVersion.TR4)
                    {
                        functions.Add(new FDMinecartEntry
                        {
                            Type = (FDMinecartType)function
                        });
                    }
                    else
                    {
                        functions.Add(new FDBeetleEntry());
                    }
                    break;

                default:
                    break;
            }

            if ((value & 0x8000) > 0)
            {
                break;
            }
            else
            {
                index++;
            }
        }

        return functions;
    }

    public List<ushort> Flatten(List<FDEntry> entries)
    {
        List<ushort> data = new();

        for (int i = 0; i < entries.Count; i++)
        {
            FDEntry entry = entries[i];
            if (_observer == null && !IsValidEntry(entry))
            {
                continue;
            }

            int setupValue = (byte)entry.GetFunction() & 0x001F;

            // Check for defined SubFunction - 0x7F00
            if (entry is FDClimbEntry climbEntry)
            {
                setupValue |= ((byte)climbEntry.Direction & 0x001F) << 8;
            }
            else if (entry is FDTriggerEntry trigger)
            {
                setupValue |= ((byte)trigger.TrigType & 0x001F) << 8;
            }
            // Or triangulation height adjustments
            else if (entry is FDTriangulationEntry triangulation)
            {
                setupValue |= (triangulation.H1 & 0x001F) << 5;
                setupValue |= (triangulation.H2 & 0x001F) << 10;
            }

            // Final entry for a sector?
            if (i == entries.Count - 1)
            {
                setupValue |= 0x8000;
            }

            // Setup done
            data.Add((ushort)setupValue);

            // Add entry specifics
            if (entry is FDPortalEntry portal)
            {
                data.Add((ushort)portal.Room);
            }
            else if (entry is FDSlantEntry slant)
            {
                int value = (slant.XSlant & 0xFF) + (slant.ZSlant << 8);
                if (value < 0)
                {
                    value = ushort.MaxValue + 1 + value;
                }
                data.Add((ushort)value);
            }
            else if (entry is FDTriggerEntry trigger)
            {
                data.AddRange(Flatten(trigger));
            }
            else if (entry is FDTriangulationEntry triangulation)
            {
                int corners = triangulation.C10 & 0x000F;
                corners |= (triangulation.C00 & 0x000F) << 4;
                corners |= (triangulation.C01 & 0x000F) << 8;
                corners |= (triangulation.C11 & 0x000F) << 12;
                data.Add((ushort)corners);
            }
        }

        return data;
    }

    private bool IsValidEntry(FDEntry entry)
    {
        return entry switch
        {
            FDTriggerEntry trigger => trigger.Actions.Count > 0,
            FDClimbEntry => _version >= TRGameVersion.TR2,
            FDMinecartEntry => _version == TRGameVersion.TR3,
            FDTriangulationEntry or FDMonkeySwingEntry => _version >= TRGameVersion.TR3,
            FDBeetleEntry or FDDeferredTriggerEntry => _version >= TRGameVersion.TR4,
            _ => true,
        };
    }

    private static List<ushort> Flatten(FDTriggerEntry trigger)
    {
        List<ushort> data = new();

        int trigSetup = trigger.Timer;
        if (trigger.OneShot)
        {
            trigSetup |= 0x0100;
        }
        trigSetup |= (trigger.Mask & 0x001F) << 9;
        data.Add((ushort)trigSetup);

        if (trigger.TrigType == FDTrigType.Switch || trigger.TrigType == FDTrigType.Key)
        {
            ushort switchRef = (ushort)trigger.SwitchOrKeyRef;
            if (trigger.Actions.Count == 0)
            {
                switchRef |= 0x8000;
            }
            data.Add(switchRef);
        }

        foreach (FDActionItem action in trigger.Actions)
        {
            int actionValue = action.Parameter & 0x03FF;
            actionValue |= ((byte)action.Action & 0x001F) << 10;
            data.Add((ushort)actionValue);

            if (action.CamAction != null
                && (action.Action == FDTrigAction.Camera || action.Action == FDTrigAction.Flyby))
            {
                int cameraValue = action.CamAction.Timer;
                if (action.CamAction.Once)
                {
                    cameraValue |= 0x0100;
                }
                cameraValue |= (action.CamAction.MoveTimer & 0x001F) << 9;
                data.Add((ushort)cameraValue);
            }
        }

        data[^1] |= 0x8000;
        return data;
    }
}
