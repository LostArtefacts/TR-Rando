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
    // Floor and ceiling of -127 on sectors means impenetrable walls around it
    private static readonly sbyte _solidSector = -127;

    public short RoomIndex { get; set; }
    public EMLocation NewLocation { get; set; }
    public EMLocation LinkedLocation { get; set; }
    public Dictionary<sbyte, List<int>> FloorHeights { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        TRRoom baseRoom = level.Rooms[data.ConvertRoom(RoomIndex)];

        int xdiff = NewLocation.X - baseRoom.Info.X;
        int ydiff = NewLocation.Y - baseRoom.Info.YBottom;
        int zdiff = NewLocation.Z - baseRoom.Info.Z;

        TRRoom newRoom = new()
        {
            AlternateRoom = -1,
            AmbientIntensity = baseRoom.AmbientIntensity,
            Flags = baseRoom.Flags,
            Info = new TRRoomInfo
            {
                X = NewLocation.X,
                YBottom = NewLocation.Y,
                YTop = NewLocation.Y + (baseRoom.Info.YTop - baseRoom.Info.YBottom),
                Z = NewLocation.Z
            },
            Lights = new TRRoomLight[baseRoom.NumLights],
            
            NumDataWords = baseRoom.NumDataWords,
            NumLights = baseRoom.NumLights,
            NumPortals = 0,
            NumStaticMeshes = baseRoom.NumStaticMeshes,
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = Array.Empty<TRRoomPortal>(),
            RoomData = new TRRoomData
            {
                NumRectangles = baseRoom.RoomData.NumRectangles,
                NumSprites = baseRoom.RoomData.NumSprites,
                NumTriangles = baseRoom.RoomData.NumTriangles,
                NumVertices = baseRoom.RoomData.NumVertices,
                Rectangles = new TRFace4[baseRoom.RoomData.NumRectangles],
                Sprites = new TRRoomSprite[baseRoom.RoomData.NumSprites],
                Triangles = new TRFace3[baseRoom.RoomData.NumTriangles],
                Vertices = new TRRoomVertex[baseRoom.RoomData.NumVertices]
            },
            Sectors = new TRRoomSector[baseRoom.Sectors.Length],
            StaticMeshes = new TRRoomStaticMesh[baseRoom.NumStaticMeshes]
        };

        // Lights
        for (int i = 0; i < newRoom.Lights.Length; i++)
        {
            newRoom.Lights[i] = new TRRoomLight
            {
                Fade = baseRoom.Lights[i].Fade,
                Intensity = baseRoom.Lights[i].Intensity,
                X = baseRoom.Lights[i].X + xdiff,
                Y = baseRoom.Lights[i].Y + ydiff,
                Z = baseRoom.Lights[i].Z + zdiff
            };
        }

        // Faces
        for (int i = 0; i < newRoom.RoomData.NumRectangles; i++)
        {
            newRoom.RoomData.Rectangles[i] = new TRFace4
            {
                Texture = baseRoom.RoomData.Rectangles[i].Texture,
                Vertices = new ushort[baseRoom.RoomData.Rectangles[i].Vertices.Length]
            };
            for (int j = 0; j < newRoom.RoomData.Rectangles[i].Vertices.Length; j++)
            {
                newRoom.RoomData.Rectangles[i].Vertices[j] = baseRoom.RoomData.Rectangles[i].Vertices[j];
            }
        }

        for (int i = 0; i < newRoom.RoomData.NumTriangles; i++)
        {
            newRoom.RoomData.Triangles[i] = new TRFace3
            {
                Texture = baseRoom.RoomData.Triangles[i].Texture,
                Vertices = new ushort[baseRoom.RoomData.Triangles[i].Vertices.Length]
            };
            for (int j = 0; j < newRoom.RoomData.Triangles[i].Vertices.Length; j++)
            {
                newRoom.RoomData.Triangles[i].Vertices[j] = baseRoom.RoomData.Triangles[i].Vertices[j];
            }
        }

        // Vertices
        for (int i = 0; i < newRoom.RoomData.Vertices.Length; i++)
        {
            newRoom.RoomData.Vertices[i] = new TRRoomVertex
            {
                Lighting = baseRoom.RoomData.Vertices[i].Lighting,
                Vertex = new TRVertex
                {
                    X = baseRoom.RoomData.Vertices[i].Vertex.X, // Room coords for X and Z
                    Y = (short)(baseRoom.RoomData.Vertices[i].Vertex.Y + ydiff),
                    Z = baseRoom.RoomData.Vertices[i].Vertex.Z
                }
            };
        }

        // Sprites
        for (int i = 0; i < newRoom.RoomData.NumSprites; i++)
        {
            newRoom.RoomData.Sprites[i] = new TRRoomSprite
            {
                Texture = baseRoom.RoomData.Sprites[i].Texture,
                Vertex = baseRoom.RoomData.Sprites[i].Vertex
            };
        }

        // Static Meshes
        for (int i = 0; i < newRoom.NumStaticMeshes; i++)
        {
            newRoom.StaticMeshes[i] = new TRRoomStaticMesh
            {
                Intensity = baseRoom.StaticMeshes[i].Intensity,
                MeshID = baseRoom.StaticMeshes[i].MeshID,
                Rotation = baseRoom.StaticMeshes[i].Rotation,
                X = (uint)(baseRoom.StaticMeshes[i].X + xdiff),
                Y = (uint)(baseRoom.StaticMeshes[i].Y + ydiff),
                Z = (uint)(baseRoom.StaticMeshes[i].Z + zdiff)
            };
        }

        // Rebuild the sectors
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        for (int i = 0; i < newRoom.Sectors.Length; i++)
        {
            newRoom.Sectors[i] = RebuildSector(baseRoom.Sectors[i], i, floorData, ydiff, baseRoom.Info);
        }

        floorData.WriteToLevel(level);

        // Generate new boxes, unless this room is meant to be isolated
        if (LinkedLocation != null)
        {
            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            BoxGenerator.Generate(newRoom, level, linkedSector);
        }

        List<TRRoom> rooms = level.Rooms.ToList();
        rooms.Add(newRoom);
        level.Rooms = rooms.ToArray();
        level.NumRooms++;
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
            Info = new TRRoomInfo
            {
                X = NewLocation.X,
                YBottom = NewLocation.Y,
                YTop = NewLocation.Y + (baseRoom.Info.YTop - baseRoom.Info.YBottom),
                Z = NewLocation.Z
            },
            Lights = new TR2RoomLight[baseRoom.NumLights],
            LightMode = baseRoom.LightMode,
            NumDataWords = baseRoom.NumDataWords,
            NumLights = baseRoom.NumLights,
            NumPortals = 0,
            NumStaticMeshes = baseRoom.NumStaticMeshes,
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = Array.Empty<TRRoomPortal>(),
            RoomData = new TR2RoomData
            {
                NumRectangles = baseRoom.RoomData.NumRectangles,
                NumSprites = baseRoom.RoomData.NumSprites,
                NumTriangles = baseRoom.RoomData.NumTriangles,
                NumVertices = baseRoom.RoomData.NumVertices,
                Rectangles = new TRFace4[baseRoom.RoomData.NumRectangles],
                Sprites = new TRRoomSprite[baseRoom.RoomData.NumSprites],
                Triangles = new TRFace3[baseRoom.RoomData.NumTriangles],
                Vertices = new TR2RoomVertex[baseRoom.RoomData.NumVertices]
            },
            SectorList = new TRRoomSector[baseRoom.SectorList.Length],
            StaticMeshes = new TR2RoomStaticMesh[baseRoom.NumStaticMeshes]
        };

        // Lights
        for (int i = 0; i < newRoom.Lights.Length; i++)
        {
            newRoom.Lights[i] = new TR2RoomLight
            {
                Fade1 = baseRoom.Lights[i].Fade1,
                Fade2 = baseRoom.Lights[i].Fade2,
                Intensity1 = baseRoom.Lights[i].Intensity1,
                Intensity2 = baseRoom.Lights[i].Intensity2,
                X = baseRoom.Lights[i].X + xdiff,
                Y = baseRoom.Lights[i].Y + ydiff,
                Z = baseRoom.Lights[i].Z + zdiff
            };
        }

        // Faces
        for (int i = 0; i < newRoom.RoomData.NumRectangles; i++)
        {
            newRoom.RoomData.Rectangles[i] = new TRFace4
            {
                Texture = baseRoom.RoomData.Rectangles[i].Texture,
                Vertices = new ushort[baseRoom.RoomData.Rectangles[i].Vertices.Length]
            };
            for (int j = 0; j < newRoom.RoomData.Rectangles[i].Vertices.Length; j++)
            {
                newRoom.RoomData.Rectangles[i].Vertices[j] = baseRoom.RoomData.Rectangles[i].Vertices[j];
            }
        }

        for (int i = 0; i < newRoom.RoomData.NumTriangles; i++)
        {
            newRoom.RoomData.Triangles[i] = new TRFace3
            {
                Texture = baseRoom.RoomData.Triangles[i].Texture,
                Vertices = new ushort[baseRoom.RoomData.Triangles[i].Vertices.Length]
            };
            for (int j = 0; j < newRoom.RoomData.Triangles[i].Vertices.Length; j++)
            {
                newRoom.RoomData.Triangles[i].Vertices[j] = baseRoom.RoomData.Triangles[i].Vertices[j];
            }
        }

        // Vertices
        for (int i = 0; i < newRoom.RoomData.Vertices.Length; i++)
        {
            newRoom.RoomData.Vertices[i] = new TR2RoomVertex
            {
                Attributes = baseRoom.RoomData.Vertices[i].Attributes,
                Lighting = baseRoom.RoomData.Vertices[i].Lighting,
                Lighting2 = baseRoom.RoomData.Vertices[i].Lighting2,
                Vertex = new TRVertex
                {
                    X = baseRoom.RoomData.Vertices[i].Vertex.X, // Room coords for X and Z
                    Y = (short)(baseRoom.RoomData.Vertices[i].Vertex.Y + ydiff),
                    Z = baseRoom.RoomData.Vertices[i].Vertex.Z
                }
            };
        }

        // Sprites
        for (int i = 0; i < newRoom.RoomData.NumSprites; i++)
        {
            newRoom.RoomData.Sprites[i] = new TRRoomSprite
            {
                Texture = baseRoom.RoomData.Sprites[i].Texture,
                Vertex = baseRoom.RoomData.Sprites[i].Vertex
            };
        }

        // Static Meshes
        for (int i = 0; i < newRoom.NumStaticMeshes; i++)
        {
            newRoom.StaticMeshes[i] = new TR2RoomStaticMesh
            {
                Intensity1 = baseRoom.StaticMeshes[i].Intensity1,
                Intensity2 = baseRoom.StaticMeshes[i].Intensity2,
                MeshID = baseRoom.StaticMeshes[i].MeshID,
                Rotation = baseRoom.StaticMeshes[i].Rotation,
                X = (uint)(baseRoom.StaticMeshes[i].X + xdiff),
                Y = (uint)(baseRoom.StaticMeshes[i].Y + ydiff),
                Z = (uint)(baseRoom.StaticMeshes[i].Z + zdiff)
            };
        }

        // Rebuild the sectors
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        for (int i = 0; i < newRoom.SectorList.Length; i++)
        {
            newRoom.SectorList[i] = RebuildSector(baseRoom.SectorList[i], i, floorData, ydiff, baseRoom.Info);
        }

        floorData.WriteToLevel(level);

        // Generate new boxes, unless this room is meant to be isolated
        if (LinkedLocation != null)
        {
            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            BoxGenerator.Generate(newRoom, level, linkedSector);
        }

        List<TR2Room> rooms = level.Rooms.ToList();
        rooms.Add(newRoom);
        level.Rooms = rooms.ToArray();
        level.NumRooms++;
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
            Info = new TRRoomInfo
            {
                X = NewLocation.X,
                YBottom = NewLocation.Y,
                YTop = NewLocation.Y + (baseRoom.Info.YTop - baseRoom.Info.YBottom),
                Z = NewLocation.Z
            },
            Lights = new TR3RoomLight[baseRoom.NumLights],
            LightMode = baseRoom.LightMode,
            NumDataWords = baseRoom.NumDataWords,
            NumLights = baseRoom.NumLights,
            NumPortals = 0,
            NumStaticMeshes = baseRoom.NumStaticMeshes,
            NumXSectors = baseRoom.NumXSectors,
            NumZSectors = baseRoom.NumZSectors,
            Portals = Array.Empty<TRRoomPortal>(),
            ReverbInfo = baseRoom.ReverbInfo,
            RoomData = new TR3RoomData
            {
                NumRectangles = baseRoom.RoomData.NumRectangles,
                NumSprites = baseRoom.RoomData.NumSprites,
                NumTriangles = baseRoom.RoomData.NumTriangles,
                NumVertices = baseRoom.RoomData.NumVertices,
                Rectangles = new TRFace4[baseRoom.RoomData.NumRectangles],
                Sprites = new TRRoomSprite[baseRoom.RoomData.NumSprites],
                Triangles = new TRFace3[baseRoom.RoomData.NumTriangles],
                Vertices = new TR3RoomVertex[baseRoom.RoomData.NumVertices]
            },
            Sectors = new TRRoomSector[baseRoom.Sectors.Length],
            StaticMeshes = new TR3RoomStaticMesh[baseRoom.NumStaticMeshes],
            WaterScheme = baseRoom.WaterScheme
        };

        // Lights
        for (int i = 0; i < newRoom.Lights.Length; i++)
        {
            newRoom.Lights[i] = new TR3RoomLight
            {
                Colour = baseRoom.Lights[i].Colour,
                LightProperties = baseRoom.Lights[i].LightProperties,
                LightType = baseRoom.Lights[i].LightType,
                X = baseRoom.Lights[i].X + xdiff,
                Y = baseRoom.Lights[i].Y + ydiff,
                Z = baseRoom.Lights[i].Z + zdiff
            };
        }

        // Faces
        for (int i = 0; i < newRoom.RoomData.NumRectangles; i++)
        {
            newRoom.RoomData.Rectangles[i] = new TRFace4
            {
                Texture = baseRoom.RoomData.Rectangles[i].Texture,
                Vertices = new ushort[baseRoom.RoomData.Rectangles[i].Vertices.Length]
            };
            for (int j = 0; j < newRoom.RoomData.Rectangles[i].Vertices.Length; j++)
            {
                newRoom.RoomData.Rectangles[i].Vertices[j] = baseRoom.RoomData.Rectangles[i].Vertices[j];
            }
        }

        for (int i = 0; i < newRoom.RoomData.NumTriangles; i++)
        {
            newRoom.RoomData.Triangles[i] = new TRFace3
            {
                Texture = baseRoom.RoomData.Triangles[i].Texture,
                Vertices = new ushort[baseRoom.RoomData.Triangles[i].Vertices.Length]
            };
            for (int j = 0; j < newRoom.RoomData.Triangles[i].Vertices.Length; j++)
            {
                newRoom.RoomData.Triangles[i].Vertices[j] = baseRoom.RoomData.Triangles[i].Vertices[j];
            }
        }

        // Vertices
        for (int i = 0; i < newRoom.RoomData.Vertices.Length; i++)
        {
            newRoom.RoomData.Vertices[i] = new TR3RoomVertex
            {
                Attributes = baseRoom.RoomData.Vertices[i].Attributes,
                Colour = baseRoom.RoomData.Vertices[i].Colour,
                Lighting = baseRoom.RoomData.Vertices[i].Lighting,
                Vertex = new TRVertex
                {
                    X = baseRoom.RoomData.Vertices[i].Vertex.X, // Room coords for X and Z
                    Y = (short)(baseRoom.RoomData.Vertices[i].Vertex.Y + ydiff),
                    Z = baseRoom.RoomData.Vertices[i].Vertex.Z
                }
            };
        }

        // Sprites
        for (int i = 0; i < newRoom.RoomData.NumSprites; i++)
        {
            newRoom.RoomData.Sprites[i] = new TRRoomSprite
            {
                Texture = baseRoom.RoomData.Sprites[i].Texture,
                Vertex = baseRoom.RoomData.Sprites[i].Vertex
            };
        }

        // Static Meshes
        for (int i = 0; i < newRoom.NumStaticMeshes; i++)
        {
            newRoom.StaticMeshes[i] = new TR3RoomStaticMesh
            {
                Colour = baseRoom.StaticMeshes[i].Colour,
                MeshID = baseRoom.StaticMeshes[i].MeshID,
                Rotation = baseRoom.StaticMeshes[i].Rotation,
                Unused = baseRoom.StaticMeshes[i].Unused,
                X = (uint)(baseRoom.StaticMeshes[i].X + xdiff),
                Y = (uint)(baseRoom.StaticMeshes[i].Y + ydiff),
                Z = (uint)(baseRoom.StaticMeshes[i].Z + zdiff)
            };
        }

        // Rebuild the sectors
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        for (int i = 0; i < newRoom.Sectors.Length; i++)
        {
            newRoom.Sectors[i] = RebuildSector(baseRoom.Sectors[i], i, floorData, ydiff, baseRoom.Info);
        }

        floorData.WriteToLevel(level);

        // Generate new boxes, unless this room is meant to be isolated
        if (LinkedLocation != null)
        {
            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            BoxGenerator.Generate(newRoom, level, linkedSector);
        }

        List<TR3Room> rooms = level.Rooms.ToList();
        rooms.Add(newRoom);
        level.Rooms = rooms.ToArray();
        level.NumRooms++;
    }

    private TRRoomSector RebuildSector(TRRoomSector originalSector, int sectorIndex, FDControl floorData, int ydiff, TRRoomInfo oldRoomInfo)
    {
        int sectorYDiff = 0;
        // Only change the sector if it's not impenetrable
        if (originalSector.Ceiling != _solidSector || originalSector.Floor != _solidSector)
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

            if (originalSector.IsImpenetrable)
            {
                // This is effectively a promise that this sector is no longer
                // going to be a wall, so reset it to a standard sector.
                ceiling = (sbyte)(oldRoomInfo.YTop / TRConsts.Step1);
                sectorYDiff = ydiff / TRConsts.Step1;
            }

            wallOpened = originalSector.IsImpenetrable || originalSector.BoxIndex == ushort.MaxValue;
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
                            newSector.Floor = newSector.Ceiling = _solidSector;
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
