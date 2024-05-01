using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl;
using TRLevelControl.Helpers.Pathing;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMCopyRoomFunction : BaseEMFunction
{
    public short RoomIndex { get; set; }
    public EMLocation NewLocation { get; set; }
    public EMLocation LinkedLocation { get; set; }
    public Dictionary<sbyte, List<int>> FloorHeights { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        TR1Room baseRoom = level.Rooms[data.ConvertRoom(RoomIndex)];

        int xdiff = NewLocation.X - baseRoom.Info.X;
        int ydiff = NewLocation.Y - baseRoom.Info.YBottom;
        int zdiff = NewLocation.Z - baseRoom.Info.Z;

        TR1Room newRoom = new()
        {
            AlternateRoom = -1,
            AmbientIntensity = baseRoom.AmbientIntensity,
            Flags = baseRoom.Flags,
            Info = new()
            {
                X = NewLocation.X,
                YBottom = NewLocation.Y,
                YTop = NewLocation.Y + (baseRoom.Info.YTop - baseRoom.Info.YBottom),
                Z = NewLocation.Z
            },
            Lights = new(baseRoom.Lights.Select(l => l.Clone())),
            
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = new(),
            Mesh = baseRoom.Mesh.Clone(),
            Sectors = new(),
            StaticMeshes = new(baseRoom.StaticMeshes.Select(s => s.Clone()))
        };

        foreach (TR1RoomLight light in newRoom.Lights)
        {
            light.X += xdiff;
            light.Y += ydiff;
            light.Z += zdiff;
        }

        foreach (TRVertex vertex in newRoom.Mesh.Vertices.Select(v => v.Vertex))
        {
            vertex.Y += (short)ydiff;
        }

        foreach (TR1RoomStaticMesh staticMesh in newRoom.StaticMeshes)
        {
            staticMesh.X += xdiff;
            staticMesh.Y += ydiff;
            staticMesh.Z += zdiff;
        }

        // Rebuild the sectors
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        for (int i = 0; i < baseRoom.Sectors.Count; i++)
        {
            newRoom.Sectors.Add(RebuildSector(baseRoom.Sectors[i], i, floorData, ydiff, baseRoom.Info));
        }

        floorData.WriteToLevel(level);

        // Generate new boxes, unless this room is meant to be isolated
        if (LinkedLocation != null)
        {
            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            BoxGenerator.Generate(newRoom, level, linkedSector);
        }

        level.Rooms.Add(newRoom);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        TR2Room baseRoom = level.Rooms[data.ConvertRoom(RoomIndex)];

        int xdiff = NewLocation.X - baseRoom.Info.X;
        int ydiff = NewLocation.Y - baseRoom.Info.YBottom;
        int zdiff = NewLocation.Z - baseRoom.Info.Z;

        TR2Room newRoom = new()
        {
            AlternateRoom = -1,
            AmbientIntensity = baseRoom.AmbientIntensity,
            AmbientIntensity2 = baseRoom.AmbientIntensity2,
            Flags = baseRoom.Flags,
            Info = new()
            {
                X = NewLocation.X,
                YBottom = NewLocation.Y,
                YTop = NewLocation.Y + (baseRoom.Info.YTop - baseRoom.Info.YBottom),
                Z = NewLocation.Z
            },
            Lights = new(baseRoom.Lights.Select(l => l.Clone())),
            LightMode = baseRoom.LightMode,
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = new(),
            Mesh = baseRoom.Mesh.Clone(),
            Sectors = new(),
            StaticMeshes = new(baseRoom.StaticMeshes.Select(s => s.Clone()))
        };

        foreach (TR2RoomLight light in newRoom.Lights)
        {
            light.X += xdiff;
            light.Y += ydiff;
            light.Z += zdiff;
        }

        foreach (TRVertex vertex in newRoom.Mesh.Vertices.Select(v => v.Vertex))
        {
            vertex.Y += (short)ydiff;
        }

        foreach (TR2RoomStaticMesh staticMesh in newRoom.StaticMeshes)
        {
            staticMesh.X += xdiff;
            staticMesh.Y += ydiff;
            staticMesh.Z += zdiff;
        }

        // Rebuild the sectors
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        for (int i = 0; i < baseRoom.Sectors.Count; i++)
        {
            newRoom.Sectors.Add(RebuildSector(baseRoom.Sectors[i], i, floorData, ydiff, baseRoom.Info));
        }

        floorData.WriteToLevel(level);

        // Generate new boxes, unless this room is meant to be isolated
        if (LinkedLocation != null)
        {
            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            BoxGenerator.Generate(newRoom, level, linkedSector);
        }

        level.Rooms.Add(newRoom);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        TR3Room baseRoom = level.Rooms[data.ConvertRoom(RoomIndex)];

        int xdiff = NewLocation.X - baseRoom.Info.X;
        int ydiff = NewLocation.Y - baseRoom.Info.YBottom;
        int zdiff = NewLocation.Z - baseRoom.Info.Z;

        TR3Room newRoom = new()
        {
            AlternateRoom = -1,
            AmbientIntensity = baseRoom.AmbientIntensity,
            Flags = baseRoom.Flags,
            Info = new()
            {
                X = NewLocation.X,
                YBottom = NewLocation.Y,
                YTop = NewLocation.Y + (baseRoom.Info.YTop - baseRoom.Info.YBottom),
                Z = NewLocation.Z
            },
            Lights = new(baseRoom.Lights.Select(l => l.Clone())),
            LightMode = baseRoom.LightMode,
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = new(),
            ReverbMode = baseRoom.ReverbMode,
            Mesh = baseRoom.Mesh.Clone(),
            Sectors = new(),
            StaticMeshes = new(baseRoom.StaticMeshes.Select(s => s.Clone())),
            WaterScheme = baseRoom.WaterScheme
        };

        foreach (TR3RoomLight light in newRoom.Lights)
        {
            light.X += xdiff;
            light.Y += ydiff;
            light.Z += zdiff;
        }

        foreach (TRVertex vertex in newRoom.Mesh.Vertices.Select(v => v.Vertex))
        {
            vertex.Y += (short)ydiff;
        }

        foreach (TR3RoomStaticMesh staticMesh in newRoom.StaticMeshes)
        {
            staticMesh.X += xdiff;
            staticMesh.Y += ydiff;
            staticMesh.Z += zdiff;
        }

        // Rebuild the sectors
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        for (int i = 0; i < baseRoom.Sectors.Count; i++)
        {
            newRoom.Sectors.Add(RebuildSector(baseRoom.Sectors[i], i, floorData, ydiff, baseRoom.Info));
        }

        floorData.WriteToLevel(level);

        // Generate new boxes, unless this room is meant to be isolated
        if (LinkedLocation != null)
        {
            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            BoxGenerator.Generate(newRoom, level, linkedSector);
        }

        level.Rooms.Add(newRoom);
    }

    private TRRoomSector RebuildSector(TRRoomSector originalSector, int sectorIndex, FDControl floorData, int ydiff, TRRoomInfo oldRoomInfo)
    {
        int sectorYDiff = 0;
        // Only change the sector if it's not impenetrable
        if (originalSector.Ceiling != TRConsts.WallClicks || originalSector.Floor != TRConsts.WallClicks)
        {
            sectorYDiff = ydiff / TRConsts.Step1;
        }

        sbyte ceiling = originalSector.Ceiling;
        sbyte floor = originalSector.Floor;

        sbyte? customHeight = GetSectorHeight(sectorIndex);
        bool wallOpened = false;
        if (customHeight.HasValue)
        {
            floor = (sbyte)(oldRoomInfo.YBottom / TRConsts.Step1);
            floor += customHeight.Value;

            if (originalSector.IsWall)
            {
                // This is effectively a promise that this sector is no longer
                // going to be a wall, so reset it to a standard sector.
                ceiling = (sbyte)(oldRoomInfo.YTop / TRConsts.Step1);
                sectorYDiff = ydiff / TRConsts.Step1;
            }

            wallOpened = originalSector.IsWall || originalSector.BoxIndex == ushort.MaxValue;
        }

        TRRoomSector newSector = new()
        {
            BoxIndex = ushort.MaxValue,
            Ceiling = (sbyte)(ceiling + sectorYDiff),
            FDIndex = 0, // Initialise to no FD
            Floor = (sbyte)(floor + sectorYDiff),
            RoomAbove = TRConsts.NoRoom,
            RoomBelow = TRConsts.NoRoom
        };

        // Duplicate the FD too for everything except triggers. Track any portals
        // so they can be blocked off.
        if (originalSector.FDIndex != 0)
        {
            List<FDEntry> entries = floorData.Entries[originalSector.FDIndex];
            List<FDEntry> newEntries = new();
            foreach (FDEntry entry in entries)
            {
                switch ((FDFunction)entry.Setup.Function)
                {
                    case FDFunction.PortalSector:
                        // This portal will no longer be valid in the new room's position,
                        // so block off the wall provided we haven't opened the wall above.
                        if (!wallOpened)
                        {
                            newSector.Floor = newSector.Ceiling = TRConsts.WallClicks;
                        }
                        break;
                    case FDFunction.FloorSlant:
                        FDSlantEntry slantEntry = entry as FDSlantEntry;
                        newEntries.Add(new FDSlantEntry()
                        {
                            Setup = new FDSetup() { Value = slantEntry.Setup.Value },
                            SlantValue = slantEntry.SlantValue,
                            Type = FDSlantType.FloorSlant
                        });
                        break;
                    case FDFunction.CeilingSlant:
                        FDSlantEntry ceilingSlant = entry as FDSlantEntry;
                        newEntries.Add(new FDSlantEntry()
                        {
                            Setup = new FDSetup() { Value = ceilingSlant.Setup.Value },
                            SlantValue = ceilingSlant.SlantValue,
                            Type = FDSlantType.CeilingSlant
                        });
                        break;
                    case FDFunction.KillLara:
                        newEntries.Add(new FDKillLaraEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
                        });
                        break;
                    case FDFunction.ClimbableWalls:
                        newEntries.Add(new FDClimbEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
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
                        FDTriangulationEntry triEntry = entry as FDTriangulationEntry;
                        newEntries.Add(new FDTriangulationEntry
                        {
                            Setup = new FDSetup { Value = triEntry.Setup.Value },
                            TriData = new FDTriangulationData { Value = triEntry.TriData.Value }
                        });
                        break;
                    case FDFunction.Monkeyswing:
                        newEntries.Add(new FDMonkeySwingEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
                        });
                        break;
                    case FDFunction.DeferredTriggeringOrMinecartRotateLeft:
                        newEntries.Add(new FDMinecartEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
                        });
                        break;
                    case FDFunction.MechBeetleOrMinecartRotateRight:
                        newEntries.Add(new TR3MinecartRotateRightEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
                        });
                        break;
                }
            }

            if (newEntries.Count > 0)
            {
                floorData.CreateFloorData(newSector);
                floorData.Entries[newSector.FDIndex].AddRange(newEntries);
            }
        }

        return newSector;
    }

    private sbyte? GetSectorHeight(int sectorIndex)
    {
        if (FloorHeights != null)
        {
            foreach (sbyte height in FloorHeights.Keys)
            {
                if (FloorHeights[height].Contains(sectorIndex))
                {
                    return height;
                }
            }
        }

        return null;
    }
}
