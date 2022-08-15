using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMCopyRoomFunction : BaseEMFunction
    {
        // Floor and ceiling of -127 on sectors means impenetrable walls around it
        private static readonly sbyte _solidSector = -127;
        private static readonly byte _noRoom = 255;

        public short RoomIndex { get; set; }
        public EMLocation NewLocation { get; set; }
        public EMLocation LinkedLocation { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            TRRoom baseRoom = level.Rooms[RoomIndex];

            int xdiff = NewLocation.X - baseRoom.Info.X;
            int ydiff = NewLocation.Y - baseRoom.Info.YBottom;
            int zdiff = NewLocation.Z - baseRoom.Info.Z;

            TRRoom newRoom = new TRRoom
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
                Portals = new TRRoomPortal[] { },
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

            // Boxes, zones and sectors
            EMLevelData data = GetData(level);
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            ushort newBoxIndex = (ushort)level.NumBoxes;
            int linkedBoxIndex = linkedSector.BoxIndex;

            // Duplicate the zone for the new box and link the current box to the new room
            TR1BoxUtilities.DuplicateZone(level, linkedBoxIndex);
            TRBox linkedBox = level.Boxes[linkedBoxIndex];
            List<ushort> overlaps = TR1BoxUtilities.GetOverlaps(level, linkedBox);
            overlaps.Add(newBoxIndex);
            TR1BoxUtilities.UpdateOverlaps(level, linkedBox, overlaps);

            // Make a new box for the new room. Tomp1 boxes are in world coordinates and they
            // do not span into walls.
            uint xmin = (uint)(newRoom.Info.X + SectorSize);
            uint zmin = (uint)(newRoom.Info.Z + SectorSize);
            uint xmax = (uint)(xmin + (newRoom.NumXSectors - 2) * SectorSize);
            uint zmax = (uint)(zmin + (newRoom.NumZSectors - 2) * SectorSize);
            TRBox box = new TRBox
            {
                XMin = xmin,
                ZMin = zmin,
                XMax = xmax,
                ZMax = zmax,
                TrueFloor = (short)newRoom.Info.YBottom
            };
            List<TRBox> boxes = level.Boxes.ToList();
            boxes.Add(box);
            level.Boxes = boxes.ToArray();
            level.NumBoxes++;

            // Link the box to the room we're joining to
            TR1BoxUtilities.UpdateOverlaps(level, box, new List<ushort> { (ushort)linkedBoxIndex });

            for (int i = 0; i < newRoom.Sectors.Length; i++)
            {
                int sectorYDiff = 0;
                ushort sectorBoxIndex = baseRoom.Sectors[i].BoxIndex;
                // Only change the sector if it's not impenetrable
                if (baseRoom.Sectors[i].Ceiling != _solidSector || baseRoom.Sectors[i].Floor != _solidSector)
                {
                    sectorYDiff = ydiff / ClickSize;
                    sectorBoxIndex = newBoxIndex;
                }

                newRoom.Sectors[i] = new TRRoomSector
                {
                    BoxIndex = sectorBoxIndex,
                    Ceiling = (sbyte)(baseRoom.Sectors[i].Ceiling + sectorYDiff),
                    FDIndex = 0, // Initialise to no FD
                    Floor = (sbyte)(baseRoom.Sectors[i].Floor + sectorYDiff),
                    RoomAbove = _noRoom,
                    RoomBelow = _noRoom
                };

                // Duplicate the FD too for everything except triggers. Track any portals
                // so they can be blocked off.
                if (baseRoom.Sectors[i].FDIndex != 0)
                {
                    List<FDEntry> entries = floorData.Entries[baseRoom.Sectors[i].FDIndex];
                    List<FDEntry> newEntries = new List<FDEntry>();
                    foreach (FDEntry entry in entries)
                    {
                        switch ((FDFunctions)entry.Setup.Function)
                        {
                            case FDFunctions.PortalSector:
                                // This portal will no longer be valid in the new room's position,
                                // so block off the wall
                                newRoom.Sectors[i].Floor = newRoom.Sectors[i].Ceiling = _solidSector;
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
                        }
                    }

                    if (newEntries.Count > 0)
                    {
                        floorData.CreateFloorData(newRoom.Sectors[i]);
                        floorData.Entries[newRoom.Sectors[i].FDIndex].AddRange(newEntries);
                    }
                }
            }

            floorData.WriteToLevel(level);

            List<TRRoom> rooms = level.Rooms.ToList();
            rooms.Add(newRoom);
            level.Rooms = rooms.ToArray();
            level.NumRooms++;
        }

        public override void ApplyToLevel(TR2Level level)
        {
            TR2Room baseRoom = level.Rooms[RoomIndex];

            int xdiff = NewLocation.X - baseRoom.Info.X;
            int ydiff = NewLocation.Y - baseRoom.Info.YBottom;
            int zdiff = NewLocation.Z - baseRoom.Info.Z;

            TR2Room newRoom = new TR2Room
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
                Portals = new TRRoomPortal[] { },
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

            // Boxes, zones and sectors
            EMLevelData data = GetData(level);
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            ushort newBoxIndex = (ushort)level.NumBoxes;
            int linkedBoxIndex = linkedSector.BoxIndex;

            // Duplicate the zone for the new box and link the current box to the new room
            TR2BoxUtilities.DuplicateZone(level, linkedBoxIndex);
            TR2Box linkedBox = level.Boxes[linkedBoxIndex];
            List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, linkedBox);
            overlaps.Add(newBoxIndex);
            TR2BoxUtilities.UpdateOverlaps(level, linkedBox, overlaps);

            // Make a new box for the new room
            byte xmin = (byte)(newRoom.Info.X / SectorSize);
            byte zmin = (byte)(newRoom.Info.Z / SectorSize);
            byte xmax = (byte)(xmin + newRoom.NumXSectors);
            byte zmax = (byte)(zmin + newRoom.NumZSectors);
            TR2Box box = new TR2Box
            {
                XMin = xmin,
                ZMin = zmin,
                XMax = xmax,
                ZMax = zmax,
                TrueFloor = (short)newRoom.Info.YBottom
            };
            List<TR2Box> boxes = level.Boxes.ToList();
            boxes.Add(box);
            level.Boxes = boxes.ToArray();
            level.NumBoxes++;

            // Link the box to the room we're joining to
            TR2BoxUtilities.UpdateOverlaps(level, box, new List<ushort> { (ushort)linkedBoxIndex });

            for (int i = 0; i < newRoom.SectorList.Length; i++)
            {
                int sectorYDiff = 0;
                ushort sectorBoxIndex = baseRoom.SectorList[i].BoxIndex;
                // Only change the sector if it's not impenetrable
                if (baseRoom.SectorList[i].Ceiling != _solidSector || baseRoom.SectorList[i].Floor != _solidSector)
                {
                    sectorYDiff = ydiff / ClickSize;
                    sectorBoxIndex = newBoxIndex;
                }

                newRoom.SectorList[i] = new TRRoomSector
                {
                    BoxIndex = sectorBoxIndex,
                    Ceiling = (sbyte)(baseRoom.SectorList[i].Ceiling + sectorYDiff),
                    FDIndex = 0, // Initialise to no FD
                    Floor = (sbyte)(baseRoom.SectorList[i].Floor + sectorYDiff),
                    RoomAbove = _noRoom,
                    RoomBelow = _noRoom
                };

                // Duplicate the FD too for everything except triggers. Track any portals
                // so they can be blocked off.
                if (baseRoom.SectorList[i].FDIndex != 0)
                {
                    List<FDEntry> entries = floorData.Entries[baseRoom.SectorList[i].FDIndex];
                    List<FDEntry> newEntries = new List<FDEntry>();
                    foreach (FDEntry entry in entries)
                    {
                        switch ((FDFunctions)entry.Setup.Function)
                        {
                            case FDFunctions.PortalSector:
                                // This portal will no longer be valid in the new room's position,
                                // so block off the wall
                                newRoom.SectorList[i].Floor = newRoom.SectorList[i].Ceiling = _solidSector;
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
                        }
                    }

                    if (newEntries.Count > 0)
                    {
                        floorData.CreateFloorData(newRoom.SectorList[i]);
                        floorData.Entries[newRoom.SectorList[i].FDIndex].AddRange(newEntries);
                    }
                }
            }

            floorData.WriteToLevel(level);

            List<TR2Room> rooms = level.Rooms.ToList();
            rooms.Add(newRoom);
            level.Rooms = rooms.ToArray();
            level.NumRooms++;
        }

        public override void ApplyToLevel(TR3Level level)
        {
            TR3Room baseRoom = level.Rooms[RoomIndex];

            int xdiff = NewLocation.X - baseRoom.Info.X;
            int ydiff = NewLocation.Y - baseRoom.Info.YBottom;
            int zdiff = NewLocation.Z - baseRoom.Info.Z;

            TR3Room newRoom = new TR3Room
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
                Portals = new TRRoomPortal[] { },
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

            // Boxes, zones and sectors
            EMLevelData data = GetData(level);
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            ushort newBoxIndex = (ushort)level.NumBoxes;
            int linkedBoxIndex = (linkedSector.BoxIndex & 0x7FF0) >> 4;
            int linkedMaterial = linkedSector.BoxIndex & 0x000F; // TR3-5 store material in bits 0-3 - wood, mud etc

            // Duplicate the zone for the new box and link the current box to the new room
            TR2BoxUtilities.DuplicateZone(level, linkedBoxIndex);
            TR2Box linkedBox = level.Boxes[linkedBoxIndex];
            List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, linkedBox);
            overlaps.Add(newBoxIndex);
            TR2BoxUtilities.UpdateOverlaps(level, linkedBox, overlaps);

            // Make a new box for the new room
            byte xmin = (byte)(newRoom.Info.X / SectorSize);
            byte zmin = (byte)(newRoom.Info.Z / SectorSize);
            byte xmax = (byte)(xmin + newRoom.NumXSectors);
            byte zmax = (byte)(zmin + newRoom.NumZSectors);
            TR2Box box = new TR2Box
            {
                XMin = xmin,
                ZMin = zmin,
                XMax = xmax,
                ZMax = zmax,
                TrueFloor = (short)newRoom.Info.YBottom
            };
            List<TR2Box> boxes = level.Boxes.ToList();
            boxes.Add(box);
            level.Boxes = boxes.ToArray();
            level.NumBoxes++;

            // Link the box to the room we're joining to
            TR2BoxUtilities.UpdateOverlaps(level, box, new List<ushort> { (ushort)linkedBoxIndex });

            // Now update each of the sectors in the new room. The box index in each sector
            // needs to reference the material so pack this into the box index.
            newBoxIndex <<= 4;
            newBoxIndex |= (ushort)linkedMaterial;

            for (int i = 0; i < newRoom.Sectors.Length; i++)
            {
                int sectorYDiff = 0;
                ushort sectorBoxIndex = baseRoom.Sectors[i].BoxIndex;
                // Only change the sector if it's not impenetrable
                if (baseRoom.Sectors[i].Ceiling != _solidSector || baseRoom.Sectors[i].Floor != _solidSector)
                {
                    sectorYDiff = ydiff / ClickSize;
                    sectorBoxIndex = newBoxIndex;
                }

                newRoom.Sectors[i] = new TRRoomSector
                {
                    BoxIndex = sectorBoxIndex,
                    Ceiling = (sbyte)(baseRoom.Sectors[i].Ceiling + sectorYDiff),
                    FDIndex = 0, // Initialise to no FD
                    Floor = (sbyte)(baseRoom.Sectors[i].Floor + sectorYDiff),
                    RoomAbove = _noRoom,
                    RoomBelow = _noRoom
                };

                // Duplicate the FD too for everything except triggers. Track any portals
                // so they can be blocked off.
                if (baseRoom.Sectors[i].FDIndex != 0)
                {
                    List<FDEntry> entries = floorData.Entries[baseRoom.Sectors[i].FDIndex];
                    List<FDEntry> newEntries = new List<FDEntry>();
                    foreach (FDEntry entry in entries)
                    {
                        switch ((FDFunctions)entry.Setup.Function)
                        {
                            case FDFunctions.PortalSector:
                                // This portal will no longer be valid in the new room's position,
                                // so block off the wall
                                newRoom.Sectors[i].Floor = newRoom.Sectors[i].Ceiling = _solidSector;
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
                        floorData.CreateFloorData(newRoom.Sectors[i]);
                        floorData.Entries[newRoom.Sectors[i].FDIndex].AddRange(newEntries);
                    }
                }
            }

            floorData.WriteToLevel(level);

            List<TR3Room> rooms = level.Rooms.ToList();
            rooms.Add(newRoom);
            level.Rooms = rooms.ToArray();
            level.NumRooms++;
        }
    }
}