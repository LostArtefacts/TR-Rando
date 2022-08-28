using Newtonsoft.Json;
using System;
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
    public class EMImportRoomFunction : BaseEMRoomImportFunction, ITextureModifier
    {
        private static readonly sbyte _solidSector = -127;
        private static readonly byte _noRoom = 255;

        public byte RoomNumber { get; set; }
        public EMLocation NewLocation { get; set; }
        public EMLocation LinkedLocation { get; set; }
        public ushort RectangleTexture { get; set; }
        public ushort TriangleTexture { get; set; }
        public bool PreservePortals { get; set; }
        public bool PreserveBoxes { get; set; }

        public EMImportRoomFunction()
        {
            RectangleTexture = TriangleTexture = ushort.MaxValue;
        }

        public override void ApplyToLevel(TRLevel level)
        {
            // Not yet implemented, plan is to rework this class to read level files instead of JSON
            throw new NotImplementedException();
        }

        public override void ApplyToLevel(TR2Level level)
        {
            Dictionary<byte, EMRoomDefinition<TR2Room>> roomResource = JsonConvert.DeserializeObject<Dictionary<byte, EMRoomDefinition<TR2Room>>>(ReadRoomResource("TR2"), _jsonSettings);
            if (!roomResource.ContainsKey(RoomNumber))
            {
                throw new Exception(string.Format("Missing room {0} in room definition data for {1}.", RoomNumber, LevelID));
            }

            EMRoomDefinition<TR2Room> roomDef = roomResource[RoomNumber];

            int xdiff = NewLocation.X - roomDef.Room.Info.X;
            int ydiff = NewLocation.Y - roomDef.Room.Info.YBottom;
            int zdiff = NewLocation.Z - roomDef.Room.Info.Z;

            TR2Room newRoom = new TR2Room
            {
                AlternateRoom = -1,
                AmbientIntensity = roomDef.Room.AmbientIntensity,
                AmbientIntensity2 = roomDef.Room.AmbientIntensity2,
                Flags = roomDef.Room.Flags,
                Info = new TRRoomInfo
                {
                    X = NewLocation.X,
                    YBottom = NewLocation.Y,
                    YTop = NewLocation.Y + (roomDef.Room.Info.YTop - roomDef.Room.Info.YBottom),
                    Z = NewLocation.Z
                },
                Lights = new TR2RoomLight[roomDef.Room.NumLights],
                LightMode = roomDef.Room.LightMode,
                NumDataWords = roomDef.Room.NumDataWords,
                NumLights = roomDef.Room.NumLights,
                NumPortals = PreservePortals ? roomDef.Room.NumPortals : (ushort)0,
                NumStaticMeshes = roomDef.Room.NumStaticMeshes,
                NumXSectors = roomDef.Room.NumXSectors,
                NumZSectors = roomDef.Room.NumZSectors,
                Portals = new TRRoomPortal[PreservePortals ? roomDef.Room.NumPortals : 0],
                RoomData = new TR2RoomData
                {
                    NumRectangles = roomDef.Room.RoomData.NumRectangles,
                    NumSprites = roomDef.Room.RoomData.NumSprites,
                    NumTriangles = roomDef.Room.RoomData.NumTriangles,
                    NumVertices = roomDef.Room.RoomData.NumVertices,
                    Rectangles = new TRFace4[roomDef.Room.RoomData.NumRectangles],
                    Sprites = new TRRoomSprite[roomDef.Room.RoomData.NumSprites],
                    Triangles = new TRFace3[roomDef.Room.RoomData.NumTriangles],
                    Vertices = new TR2RoomVertex[roomDef.Room.RoomData.NumVertices]
                },
                SectorList = new TRRoomSector[roomDef.Room.SectorList.Length],
                StaticMeshes = new TR2RoomStaticMesh[roomDef.Room.NumStaticMeshes]
            };

            if (PreservePortals)
            {
                for (int i = 0; i < newRoom.Portals.Length; i++)
                {
                    newRoom.Portals[i] = new TRRoomPortal
                    {
                        AdjoiningRoom = roomDef.Room.Portals[i].AdjoiningRoom,
                        Normal = roomDef.Room.Portals[i].Normal,
                        Vertices = roomDef.Room.Portals[i].Vertices
                    };
                }
            }

            // Lights
            for (int i = 0; i < newRoom.Lights.Length; i++)
            {
                newRoom.Lights[i] = new TR2RoomLight
                {
                    Fade1 = roomDef.Room.Lights[i].Fade1,
                    Fade2 = roomDef.Room.Lights[i].Fade2,
                    Intensity1 = roomDef.Room.Lights[i].Intensity1,
                    Intensity2 = roomDef.Room.Lights[i].Intensity2,
                    X = roomDef.Room.Lights[i].X + xdiff,
                    Y = roomDef.Room.Lights[i].Y + ydiff,
                    Z = roomDef.Room.Lights[i].Z + zdiff
                };
            }

            // Faces
            for (int i = 0; i < newRoom.RoomData.NumRectangles; i++)
            {
                newRoom.RoomData.Rectangles[i] = new TRFace4
                {
                    Texture = RectangleTexture == ushort.MaxValue ? roomDef.Room.RoomData.Rectangles[i].Texture : RectangleTexture,
                    Vertices = new ushort[roomDef.Room.RoomData.Rectangles[i].Vertices.Length]
                };
                for (int j = 0; j < newRoom.RoomData.Rectangles[i].Vertices.Length; j++)
                {
                    newRoom.RoomData.Rectangles[i].Vertices[j] = roomDef.Room.RoomData.Rectangles[i].Vertices[j];
                }
            }

            for (int i = 0; i < newRoom.RoomData.NumTriangles; i++)
            {
                newRoom.RoomData.Triangles[i] = new TRFace3
                {
                    Texture = TriangleTexture == ushort.MaxValue ? roomDef.Room.RoomData.Triangles[i].Texture : TriangleTexture,
                    Vertices = new ushort[roomDef.Room.RoomData.Triangles[i].Vertices.Length]
                };
                for (int j = 0; j < newRoom.RoomData.Triangles[i].Vertices.Length; j++)
                {
                    newRoom.RoomData.Triangles[i].Vertices[j] = roomDef.Room.RoomData.Triangles[i].Vertices[j];
                }
            }

            // Vertices
            for (int i = 0; i < newRoom.RoomData.Vertices.Length; i++)
            {
                newRoom.RoomData.Vertices[i] = new TR2RoomVertex
                {
                    Attributes = roomDef.Room.RoomData.Vertices[i].Attributes,
                    Lighting = roomDef.Room.RoomData.Vertices[i].Lighting,
                    Lighting2 = roomDef.Room.RoomData.Vertices[i].Lighting2,
                    Vertex = new TRVertex
                    {
                        X = roomDef.Room.RoomData.Vertices[i].Vertex.X, // Room coords for X and Z
                        Y = (short)(roomDef.Room.RoomData.Vertices[i].Vertex.Y + ydiff),
                        Z = roomDef.Room.RoomData.Vertices[i].Vertex.Z
                    }
                };
            }

            // Sprites
            for (int i = 0; i < newRoom.RoomData.NumSprites; i++)
            {
                newRoom.RoomData.Sprites[i] = new TRRoomSprite
                {
                    Texture = roomDef.Room.RoomData.Sprites[i].Texture,
                    Vertex = roomDef.Room.RoomData.Sprites[i].Vertex
                };
            }

            // Static Meshes
            for (int i = 0; i < newRoom.NumStaticMeshes; i++)
            {
                newRoom.StaticMeshes[i] = new TR2RoomStaticMesh
                {
                    Intensity1 = roomDef.Room.StaticMeshes[i].Intensity1,
                    Intensity2 = roomDef.Room.StaticMeshes[i].Intensity2,
                    MeshID = roomDef.Room.StaticMeshes[i].MeshID,
                    Rotation = roomDef.Room.StaticMeshes[i].Rotation,
                    X = (uint)(roomDef.Room.StaticMeshes[i].X + xdiff),
                    Y = (uint)(roomDef.Room.StaticMeshes[i].Y + ydiff),
                    Z = (uint)(roomDef.Room.StaticMeshes[i].Z + zdiff)
                };
            }

            // Boxes, zones and sectors
            EMLevelData data = GetData(level);
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            ushort newBoxIndex = ushort.MaxValue;
            // Duplicate the zone for the new box and link the current box to the new room
            if (!PreserveBoxes)
            {
                TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
                newBoxIndex = (ushort)level.NumBoxes;
                int linkedBoxIndex = linkedSector.BoxIndex;

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
            }

            for (int i = 0; i < newRoom.SectorList.Length; i++)
            {
                int sectorYDiff = 0;
                ushort sectorBoxIndex = roomDef.Room.SectorList[i].BoxIndex;
                // Only change the sector if it's not impenetrable and we don't want to preserve the existing zoning
                if (roomDef.Room.SectorList[i].Ceiling != _solidSector || roomDef.Room.SectorList[i].Floor != _solidSector)
                {
                    sectorYDiff = ydiff / ClickSize;
                    if (!PreserveBoxes)
                    {
                        sectorBoxIndex = newBoxIndex;
                    }
                }

                newRoom.SectorList[i] = new TRRoomSector
                {
                    BoxIndex = sectorBoxIndex,
                    Ceiling = (sbyte)(roomDef.Room.SectorList[i].Ceiling + sectorYDiff),
                    FDIndex = 0, // Initialise to no FD
                    Floor = (sbyte)(roomDef.Room.SectorList[i].Floor + sectorYDiff),
                    RoomAbove = PreservePortals ? roomDef.Room.SectorList[i].RoomAbove : _noRoom,
                    RoomBelow = PreservePortals ? roomDef.Room.SectorList[i].RoomBelow : _noRoom
                };

                // Duplicate the FD too for everything except triggers. Track any portals
                // so they can be blocked off.
                ushort fdIndex = roomDef.Room.SectorList[i].FDIndex;
                if (roomDef.FloorData.ContainsKey(fdIndex))
                {
                    List<FDEntry> entries = roomDef.FloorData[fdIndex];
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
            Dictionary<byte, EMRoomDefinition<TR3Room>> roomResource = JsonConvert.DeserializeObject<Dictionary<byte, EMRoomDefinition<TR3Room>>>(ReadRoomResource("TR3"), _jsonSettings);
            if (!roomResource.ContainsKey(RoomNumber))
            {
                throw new Exception(string.Format("Missing room {0} in room definition data for {1}.", RoomNumber, LevelID));
            }

            EMRoomDefinition<TR3Room> roomDef = roomResource[RoomNumber];

            int xdiff = NewLocation.X - roomDef.Room.Info.X;
            int ydiff = NewLocation.Y - roomDef.Room.Info.YBottom;
            int zdiff = NewLocation.Z - roomDef.Room.Info.Z;

            TR3Room newRoom = new TR3Room
            {
                AlternateRoom = -1,
                AmbientIntensity = roomDef.Room.AmbientIntensity,
                Filler = roomDef.Room.Filler,
                Flags = roomDef.Room.Flags,
                Info = new TRRoomInfo
                {
                    X = NewLocation.X,
                    YBottom = NewLocation.Y,
                    YTop = NewLocation.Y + (roomDef.Room.Info.YTop - roomDef.Room.Info.YBottom),
                    Z = NewLocation.Z
                },
                Lights = new TR3RoomLight[roomDef.Room.NumLights],
                LightMode = roomDef.Room.LightMode,
                NumDataWords = roomDef.Room.NumDataWords,
                NumLights = roomDef.Room.NumLights,
                NumPortals = 0,
                NumStaticMeshes = roomDef.Room.NumStaticMeshes,
                NumXSectors = roomDef.Room.NumXSectors,
                NumZSectors = roomDef.Room.NumZSectors,
                Portals = new TRRoomPortal[] { },
                ReverbInfo = roomDef.Room.ReverbInfo,
                RoomData = new TR3RoomData
                {
                    NumRectangles = roomDef.Room.RoomData.NumRectangles,
                    NumSprites = roomDef.Room.RoomData.NumSprites,
                    NumTriangles = roomDef.Room.RoomData.NumTriangles,
                    NumVertices = roomDef.Room.RoomData.NumVertices,
                    Rectangles = new TRFace4[roomDef.Room.RoomData.NumRectangles],
                    Sprites = new TRRoomSprite[roomDef.Room.RoomData.NumSprites],
                    Triangles = new TRFace3[roomDef.Room.RoomData.NumTriangles],
                    Vertices = new TR3RoomVertex[roomDef.Room.RoomData.NumVertices]
                },
                Sectors = new TRRoomSector[roomDef.Room.Sectors.Length],
                StaticMeshes = new TR3RoomStaticMesh[roomDef.Room.NumStaticMeshes],
                WaterScheme = roomDef.Room.WaterScheme
            };

            // Lights
            for (int i = 0; i < newRoom.Lights.Length; i++)
            {
                newRoom.Lights[i] = new TR3RoomLight
                {
                    Colour = roomDef.Room.Lights[i].Colour,
                    LightProperties = roomDef.Room.Lights[i].LightProperties,
                    LightType = roomDef.Room.Lights[i].LightType,
                    X = roomDef.Room.Lights[i].X + xdiff,
                    Y = roomDef.Room.Lights[i].Y + ydiff,
                    Z = roomDef.Room.Lights[i].Z + zdiff
                };
            }

            // Faces
            for (int i = 0; i < newRoom.RoomData.NumRectangles; i++)
            {
                newRoom.RoomData.Rectangles[i] = new TRFace4
                {
                    Texture = RectangleTexture == ushort.MaxValue ? roomDef.Room.RoomData.Rectangles[i].Texture : RectangleTexture,
                    Vertices = new ushort[roomDef.Room.RoomData.Rectangles[i].Vertices.Length]
                };
                for (int j = 0; j < newRoom.RoomData.Rectangles[i].Vertices.Length; j++)
                {
                    newRoom.RoomData.Rectangles[i].Vertices[j] = roomDef.Room.RoomData.Rectangles[i].Vertices[j];
                }
            }

            for (int i = 0; i < newRoom.RoomData.NumTriangles; i++)
            {
                newRoom.RoomData.Triangles[i] = new TRFace3
                {
                    Texture = TriangleTexture == ushort.MaxValue ? roomDef.Room.RoomData.Triangles[i].Texture : TriangleTexture,
                    Vertices = new ushort[roomDef.Room.RoomData.Triangles[i].Vertices.Length]
                };
                for (int j = 0; j < newRoom.RoomData.Triangles[i].Vertices.Length; j++)
                {
                    newRoom.RoomData.Triangles[i].Vertices[j] = roomDef.Room.RoomData.Triangles[i].Vertices[j];
                }
            }

            // Vertices
            for (int i = 0; i < newRoom.RoomData.Vertices.Length; i++)
            {
                newRoom.RoomData.Vertices[i] = new TR3RoomVertex
                {
                    Attributes = roomDef.Room.RoomData.Vertices[i].Attributes,
                    Colour = roomDef.Room.RoomData.Vertices[i].Colour,
                    Lighting = roomDef.Room.RoomData.Vertices[i].Lighting,
                    Vertex = new TRVertex
                    {
                        X = roomDef.Room.RoomData.Vertices[i].Vertex.X, // Room coords for X and Z
                        Y = (short)(roomDef.Room.RoomData.Vertices[i].Vertex.Y + ydiff),
                        Z = roomDef.Room.RoomData.Vertices[i].Vertex.Z
                    }
                };
            }

            // Sprites
            for (int i = 0; i < newRoom.RoomData.NumSprites; i++)
            {
                newRoom.RoomData.Sprites[i] = new TRRoomSprite
                {
                    Texture = roomDef.Room.RoomData.Sprites[i].Texture,
                    Vertex = roomDef.Room.RoomData.Sprites[i].Vertex
                };
            }

            // Static Meshes
            for (int i = 0; i < newRoom.NumStaticMeshes; i++)
            {
                newRoom.StaticMeshes[i] = new TR3RoomStaticMesh
                {
                    Colour = roomDef.Room.StaticMeshes[i].Colour,
                    MeshID = roomDef.Room.StaticMeshes[i].MeshID,
                    Rotation = roomDef.Room.StaticMeshes[i].Rotation,
                    Unused = roomDef.Room.StaticMeshes[i].Unused,
                    X = (uint)(roomDef.Room.StaticMeshes[i].X + xdiff),
                    Y = (uint)(roomDef.Room.StaticMeshes[i].Y + ydiff),
                    Z = (uint)(roomDef.Room.StaticMeshes[i].Z + zdiff)
                };
            }

            // Boxes, zones and sectors
            EMLevelData data = GetData(level);
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector linkedSector = FDUtilities.GetRoomSector(LinkedLocation.X, LinkedLocation.Y, LinkedLocation.Z, data.ConvertRoom(LinkedLocation.Room), level, floorData);
            ushort newBoxIndex = ushort.MaxValue;
            int linkedBoxIndex = (linkedSector.BoxIndex & 0x7FF0) >> 4;
            int linkedMaterial = linkedSector.BoxIndex & 0x000F; // TR3-5 store material in bits 0-3 - wood, mud etc

            if (!PreserveBoxes)
            {
                newBoxIndex = (ushort)level.NumBoxes;

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
            }

            // Now update each of the sectors in the new room. The box index in each sector
            // needs to reference the material so pack this into the box index.
            newBoxIndex <<= 4;
            newBoxIndex |= (ushort)linkedMaterial;

            for (int i = 0; i < newRoom.Sectors.Length; i++)
            {
                int sectorYDiff = 0;
                ushort sectorBoxIndex = roomDef.Room.Sectors[i].BoxIndex;
                // Only change the sector if it's not impenetrable
                if (roomDef.Room.Sectors[i].Ceiling != _solidSector || roomDef.Room.Sectors[i].Floor != _solidSector)
                {
                    sectorYDiff = ydiff / ClickSize;
                    if (!PreserveBoxes)
                    {
                        sectorBoxIndex = newBoxIndex;
                    }
                }

                newRoom.Sectors[i] = new TRRoomSector
                {
                    BoxIndex = sectorBoxIndex,
                    Ceiling = (sbyte)(roomDef.Room.Sectors[i].Ceiling + sectorYDiff),
                    FDIndex = 0, // Initialise to no FD
                    Floor = (sbyte)(roomDef.Room.Sectors[i].Floor + sectorYDiff),
                    RoomAbove = PreservePortals ? roomDef.Room.Sectors[i].RoomAbove : _noRoom,
                    RoomBelow = PreservePortals ? roomDef.Room.Sectors[i].RoomBelow : _noRoom
                };

                // Duplicate the FD too for everything except triggers. Track any portals
                // so they can be blocked off.
                ushort fdIndex = roomDef.Room.Sectors[i].FDIndex;
                if (roomDef.FloorData.ContainsKey(fdIndex))
                {
                    List<FDEntry> entries = roomDef.FloorData[fdIndex];
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

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            if (indexMap.ContainsKey(RectangleTexture))
            {
                RectangleTexture = indexMap[RectangleTexture];
            }
            if (indexMap.ContainsKey(TriangleTexture))
            {
                TriangleTexture = indexMap[TriangleTexture];
            }
        }
    }
}