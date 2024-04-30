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
            Lights = new(),
            
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = new(),
            Mesh = new()
            {
                Rectangles = new(),
                Sprites = new(),
                Triangles = new(),
                Vertices = new(),
            },
            Sectors = new(),
            StaticMeshes = new()
        };

        // Lights
        for (int i = 0; i < baseRoom.Lights.Count; i++)
        {
            newRoom.Lights.Add(new()
            {
                Fade = baseRoom.Lights[i].Fade,
                Intensity = baseRoom.Lights[i].Intensity,
                X = baseRoom.Lights[i].X + xdiff,
                Y = baseRoom.Lights[i].Y + ydiff,
                Z = baseRoom.Lights[i].Z + zdiff
            });
        }

        // Faces
        for (int i = 0; i < baseRoom.Mesh.Rectangles.Count; i++)
        {
            newRoom.Mesh.Rectangles.Add(new()
            {
                Texture = baseRoom.Mesh.Rectangles[i].Texture,
                Vertices = new ushort[baseRoom.Mesh.Rectangles[i].Vertices.Length]
            });
            for (int j = 0; j < newRoom.Mesh.Rectangles[i].Vertices.Length; j++)
            {
                newRoom.Mesh.Rectangles[i].Vertices[j] = baseRoom.Mesh.Rectangles[i].Vertices[j];
            }
        }

        for (int i = 0; i < baseRoom.Mesh.Triangles.Count; i++)
        {
            newRoom.Mesh.Triangles.Add(new()
            {
                Texture = baseRoom.Mesh.Triangles[i].Texture,
                Vertices = new ushort[baseRoom.Mesh.Triangles[i].Vertices.Length]
            });
            for (int j = 0; j < newRoom.Mesh.Triangles[i].Vertices.Length; j++)
            {
                newRoom.Mesh.Triangles[i].Vertices[j] = baseRoom.Mesh.Triangles[i].Vertices[j];
            }
        }

        // Vertices
        for (int i = 0; i < baseRoom.Mesh.Vertices.Count; i++)
        {
            newRoom.Mesh.Vertices.Add(new()
            {
                Lighting = baseRoom.Mesh.Vertices[i].Lighting,
                Vertex = new()
                {
                    X = baseRoom.Mesh.Vertices[i].Vertex.X, // Room coords for X and Z
                    Y = (short)(baseRoom.Mesh.Vertices[i].Vertex.Y + ydiff),
                    Z = baseRoom.Mesh.Vertices[i].Vertex.Z
                }
            });
        }

        // Sprites
        for (int i = 0; i < baseRoom.Mesh.Sprites.Count; i++)
        {
            newRoom.Mesh.Sprites.Add(new()
            {
                Texture = baseRoom.Mesh.Sprites[i].Texture,
                Vertex = baseRoom.Mesh.Sprites[i].Vertex
            });
        }

        // Static Meshes
        for (int i = 0; i < baseRoom.StaticMeshes.Count; i++)
        {
            newRoom.StaticMeshes.Add(new()
            {
                Intensity = baseRoom.StaticMeshes[i].Intensity,
                ID = baseRoom.StaticMeshes[i].ID,
                Angle = baseRoom.StaticMeshes[i].Angle,
                X = baseRoom.StaticMeshes[i].X + xdiff,
                Y = baseRoom.StaticMeshes[i].Y + ydiff,
                Z = baseRoom.StaticMeshes[i].Z + zdiff
            });
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
            Lights = new(),
            LightMode = baseRoom.LightMode,
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = new(),
            Mesh = new()
            {
                Rectangles = new(),
                Sprites = new(),
                Triangles = new(),
                Vertices = new(),
            },
            Sectors = new(),
            StaticMeshes = new()
        };

        // Lights
        for (int i = 0; i < baseRoom.Lights.Count; i++)
        {
            newRoom.Lights.Add(new()
            {
                Fade1 = baseRoom.Lights[i].Fade1,
                Fade2 = baseRoom.Lights[i].Fade2,
                Intensity1 = baseRoom.Lights[i].Intensity1,
                Intensity2 = baseRoom.Lights[i].Intensity2,
                X = baseRoom.Lights[i].X + xdiff,
                Y = baseRoom.Lights[i].Y + ydiff,
                Z = baseRoom.Lights[i].Z + zdiff
            });
        }

        // Faces
        for (int i = 0; i < baseRoom.Mesh.Rectangles.Count; i++)
        {
            newRoom.Mesh.Rectangles.Add(new()
            {
                Texture = baseRoom.Mesh.Rectangles[i].Texture,
                Vertices = new ushort[baseRoom.Mesh.Rectangles[i].Vertices.Length]
            });
            for (int j = 0; j < newRoom.Mesh.Rectangles[i].Vertices.Length; j++)
            {
                newRoom.Mesh.Rectangles[i].Vertices[j] = baseRoom.Mesh.Rectangles[i].Vertices[j];
            }
        }

        for (int i = 0; i < baseRoom.Mesh.Triangles.Count; i++)
        {
            newRoom.Mesh.Triangles.Add(new()
            {
                Texture = baseRoom.Mesh.Triangles[i].Texture,
                Vertices = new ushort[baseRoom.Mesh.Triangles[i].Vertices.Length]
            });
            for (int j = 0; j < newRoom.Mesh.Triangles[i].Vertices.Length; j++)
            {
                newRoom.Mesh.Triangles[i].Vertices[j] = baseRoom.Mesh.Triangles[i].Vertices[j];
            }
        }

        // Vertices
        for (int i = 0; i < baseRoom.Mesh.Vertices.Count; i++)
        {
            newRoom.Mesh.Vertices.Add(new()
            {
                Attributes = baseRoom.Mesh.Vertices[i].Attributes,
                Lighting = baseRoom.Mesh.Vertices[i].Lighting,
                Lighting2 = baseRoom.Mesh.Vertices[i].Lighting2,
                Vertex = new TRVertex
                {
                    X = baseRoom.Mesh.Vertices[i].Vertex.X, // Room coords for X and Z
                    Y = (short)(baseRoom.Mesh.Vertices[i].Vertex.Y + ydiff),
                    Z = baseRoom.Mesh.Vertices[i].Vertex.Z
                }
            });
        }

        // Sprites
        for (int i = 0; i < baseRoom.Mesh.Sprites.Count; i++)
        {
            newRoom.Mesh.Sprites.Add(new()
            {
                Texture = baseRoom.Mesh.Sprites[i].Texture,
                Vertex = baseRoom.Mesh.Sprites[i].Vertex
            });
        }

        // Static Meshes
        for (int i = 0; i < baseRoom.StaticMeshes.Count; i++)
        {
            newRoom.StaticMeshes.Add(new()
            {
                Intensity1 = baseRoom.StaticMeshes[i].Intensity1,
                Intensity2 = baseRoom.StaticMeshes[i].Intensity2,
                ID = baseRoom.StaticMeshes[i].ID,
                Angle = baseRoom.StaticMeshes[i].Angle,
                X = baseRoom.StaticMeshes[i].X + xdiff,
                Y = baseRoom.StaticMeshes[i].Y + ydiff,
                Z = baseRoom.StaticMeshes[i].Z + zdiff
            });
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
            Filler = baseRoom.Filler,
            Flags = baseRoom.Flags,
            Info = new()
            {
                X = NewLocation.X,
                YBottom = NewLocation.Y,
                YTop = NewLocation.Y + (baseRoom.Info.YTop - baseRoom.Info.YBottom),
                Z = NewLocation.Z
            },
            Lights = new(),
            LightMode = baseRoom.LightMode,
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = new(),
            ReverbInfo = baseRoom.ReverbInfo,
            Mesh = new()
            {
                Rectangles = new(),
                Sprites = new(),
                Triangles = new(),
                Vertices = new(),
            },
            Sectors = new(),
            StaticMeshes = new(),
            WaterScheme = baseRoom.WaterScheme
        };

        // Lights
        for (int i = 0; i < baseRoom.Lights.Count; i++)
        {
            newRoom.Lights.Add(new()
            {
                Colour = baseRoom.Lights[i].Colour,
                LightProperties = baseRoom.Lights[i].LightProperties,
                LightType = baseRoom.Lights[i].LightType,
                X = baseRoom.Lights[i].X + xdiff,
                Y = baseRoom.Lights[i].Y + ydiff,
                Z = baseRoom.Lights[i].Z + zdiff
            });
        }

        // Faces
        for (int i = 0; i < baseRoom.Mesh.Rectangles.Count; i++)
        {
            newRoom.Mesh.Rectangles.Add(new()
            {
                Texture = baseRoom.Mesh.Rectangles[i].Texture,
                Vertices = new ushort[baseRoom.Mesh.Rectangles[i].Vertices.Length]
            });
            for (int j = 0; j < newRoom.Mesh.Rectangles[i].Vertices.Length; j++)
            {
                newRoom.Mesh.Rectangles[i].Vertices[j] = baseRoom.Mesh.Rectangles[i].Vertices[j];
            }
        }

        for (int i = 0; i < baseRoom.Mesh.Triangles.Count; i++)
        {
            newRoom.Mesh.Triangles.Add(new()
            {
                Texture = baseRoom.Mesh.Triangles[i].Texture,
                Vertices = new ushort[baseRoom.Mesh.Triangles[i].Vertices.Length]
            });
            for (int j = 0; j < newRoom.Mesh.Triangles[i].Vertices.Length; j++)
            {
                newRoom.Mesh.Triangles[i].Vertices[j] = baseRoom.Mesh.Triangles[i].Vertices[j];
            }
        }

        // Vertices
        for (int i = 0; i < baseRoom.Mesh.Vertices.Count; i++)
        {
            newRoom.Mesh.Vertices.Add(new()
            {
                Attributes = baseRoom.Mesh.Vertices[i].Attributes,
                Colour = baseRoom.Mesh.Vertices[i].Colour,
                Lighting = baseRoom.Mesh.Vertices[i].Lighting,
                Vertex = new TRVertex
                {
                    X = baseRoom.Mesh.Vertices[i].Vertex.X, // Room coords for X and Z
                    Y = (short)(baseRoom.Mesh.Vertices[i].Vertex.Y + ydiff),
                    Z = baseRoom.Mesh.Vertices[i].Vertex.Z
                }
            });
        }

        // Sprites
        for (int i = 0; i < baseRoom.Mesh.Sprites.Count; i++)
        {
            newRoom.Mesh.Sprites.Add(new()
            {
                Texture = baseRoom.Mesh.Sprites[i].Texture,
                Vertex = baseRoom.Mesh.Sprites[i].Vertex
            });
        }

        // Static Meshes
        for (int i = 0; i < baseRoom.StaticMeshes.Count; i++)
        {
            newRoom.StaticMeshes.Add(new()
            {
                Colour = baseRoom.StaticMeshes[i].Colour,
                ID = baseRoom.StaticMeshes[i].ID,
                Unused = baseRoom.StaticMeshes[i].Unused,
                Angle = baseRoom.StaticMeshes[i].Angle,
                X = baseRoom.StaticMeshes[i].X + xdiff,
                Y = baseRoom.StaticMeshes[i].Y + ydiff,
                Z = baseRoom.StaticMeshes[i].Z + zdiff
            });
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
                switch ((FDFunctions)entry.Setup.Function)
                {
                    case FDFunctions.PortalSector:
                        // This portal will no longer be valid in the new room's position,
                        // so block off the wall provided we haven't opened the wall above.
                        if (!wallOpened)
                        {
                            newSector.Floor = newSector.Ceiling = TRConsts.WallClicks;
                        }
                        break;
                    case FDFunctions.FloorSlant:
                        FDSlantEntry slantEntry = entry as FDSlantEntry;
                        newEntries.Add(new FDSlantEntry()
                        {
                            Setup = new FDSetup() { Value = slantEntry.Setup.Value },
                            SlantValue = slantEntry.SlantValue,
                            Type = FDSlantEntryType.FloorSlant
                        });
                        break;
                    case FDFunctions.CeilingSlant:
                        FDSlantEntry ceilingSlant = entry as FDSlantEntry;
                        newEntries.Add(new FDSlantEntry()
                        {
                            Setup = new FDSetup() { Value = ceilingSlant.Setup.Value },
                            SlantValue = ceilingSlant.SlantValue,
                            Type = FDSlantEntryType.CeilingSlant
                        });
                        break;
                    case FDFunctions.KillLara:
                        newEntries.Add(new FDKillLaraEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
                        });
                        break;
                    case FDFunctions.ClimbableWalls:
                        newEntries.Add(new FDClimbEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
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
                        TR3TriangulationEntry triEntry = entry as TR3TriangulationEntry;
                        newEntries.Add(new TR3TriangulationEntry
                        {
                            Setup = new FDSetup { Value = triEntry.Setup.Value },
                            TriData = new FDTriangulationData { Value = triEntry.TriData.Value }
                        });
                        break;
                    case FDFunctions.Monkeyswing:
                        newEntries.Add(new TR3MonkeySwingEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
                        });
                        break;
                    case FDFunctions.DeferredTriggeringOrMinecartRotateLeft:
                        newEntries.Add(new TR3MinecartRotateLeftEntry()
                        {
                            Setup = new FDSetup() { Value = entry.Setup.Value }
                        });
                        break;
                    case FDFunctions.MechBeetleOrMinecartRotateRight:
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
