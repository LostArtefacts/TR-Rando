using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;

namespace TRLevelReader
{
    public static class TR2LevelReader
    {
        private static readonly uint TR2VersionHeader = 0x0000002D;

        private const uint MAX_PALETTE_SIZE = 256;

        public static TR2Level ReadLevel(string Filename)
        {
            if (!Filename.ToUpper().Contains("TR2"))
            {
                throw new NotImplementedException("File reader only supports TR2 levels");
            }

            TR2Level level = new TR2Level();

            using (BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open)))
            {
                Log.LogF("File opened");

                level.Version = reader.ReadUInt32();

                if (level.Version != TR2VersionHeader)
                {
                    throw new NotImplementedException("File reader only suppors TR2 levels");
                }

                level.Palette = PopulateColourPalette(reader.ReadBytes((int)MAX_PALETTE_SIZE * 3));
                level.Palette16 = PopulateColourPalette16(reader.ReadBytes((int)MAX_PALETTE_SIZE * 4));

                level.NumImages = reader.ReadUInt32();

                level.Images8 = new TRTexImage8[level.NumImages];
                level.Images16 = new TRTexImage16[level.NumImages];

                //Initialize the texture arrays
                for (int i = 0; i < level.NumImages; i++)
                {
                    level.Images8[i] = new TRTexImage8();
                    level.Images16[i] = new TRTexImage16();
                }

                //For each texture8 there are 256 * 256 bytes (65536) we can just do a straight byte read
                for (int i = 0; i < level.NumImages; i++)
                {
                    level.Images8[i].Pixels = reader.ReadBytes(256 * 256);
                }                

                //For each texture16 there are 256 * 256 * 2 bytes (131072)
                for (int i = 0; i < level.NumImages; i++)
                {
                    for (int j = 0; j < (256 * 256); j++)
                    {
                        level.Images16[i].Pixels[j] = reader.ReadUInt16();
                    }
                }

                level.Unused = reader.ReadUInt32();
                level.NumRooms = reader.ReadUInt16();

                #region Rooms
                level.Rooms = new TR2Room[level.NumRooms];

                for (int i = 0; i < level.NumRooms; i++)
                {
                    TR2Room room = new TR2Room();

                    //Grab info
                    room.Info = new TRRoomInfo
                    {
                        X = reader.ReadInt32(),
                        Z = reader.ReadInt32(),
                        YBottom = reader.ReadInt32(),
                        YTop = reader.ReadInt32()
                    };

                    //Grab data
                    room.NumDataWords = reader.ReadUInt32();
                    room.Data = new ushort[room.NumDataWords];
                    for (int j = 0; j < room.NumDataWords; j++)
                    {
                        room.Data[j] = reader.ReadUInt16();
                    }

                    //Store what we just read
                    room.RoomData = ConvertToRoomData(room);

                    //Portals
                    room.NumPortals = reader.ReadUInt16();
                    room.Portals = new TRRoomPortal[room.NumPortals];
                    for (int j = 0; j < room.NumPortals; j++)
                    {
                        TRRoomPortal portal = new TRRoomPortal
                        {
                            AdjoiningRoom = reader.ReadUInt16(),

                            Normal = new TRVertex
                            {
                                X = reader.ReadInt16(),
                                Y = reader.ReadInt16(),
                                Z = reader.ReadInt16()
                            },

                            Vertices = new TRVertex[]
                            {
                                new TRVertex { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                                new TRVertex { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                                new TRVertex { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                                new TRVertex { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                            }
                        };

                        room.Portals[j] = portal;
                    }

                    //Sectors
                    room.NumZSectors = reader.ReadUInt16();
                    room.NumXSectors = reader.ReadUInt16();
                    room.SectorList = new TRRoomSector[room.NumXSectors * room.NumZSectors];
                    for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
                    {
                        TRRoomSector sector = new TRRoomSector
                        {
                            FDIndex = reader.ReadUInt16(),
                            BoxIndex = reader.ReadUInt16(),
                            RoomBelow = reader.ReadByte(),
                            Floor = reader.ReadSByte(),
                            RoomAbove = reader.ReadByte(),
                            Ceiling = reader.ReadSByte()
                        };

                        room.SectorList[j] = sector;
                    }

                    //Lighting
                    room.AmbientIntensity = reader.ReadInt16();
                    room.AmbientIntensity2 = reader.ReadInt16();
                    room.LightMode = reader.ReadInt16();
                    room.NumLights = reader.ReadUInt16();
                    room.Lights = new TR2RoomLight[room.NumLights];
                    for (int j = 0; j < room.NumLights; j++)
                    {
                        TR2RoomLight light = new TR2RoomLight
                        {
                            X = reader.ReadInt32(),
                            Y = reader.ReadInt32(),
                            Z = reader.ReadInt32(),
                            Intensity1 = reader.ReadUInt16(),
                            Intensity2 = reader.ReadUInt16(),
                            Fade1 = reader.ReadUInt32(),
                            Fade2 = reader.ReadUInt32()
                        };

                        room.Lights[j] = light;
                    }

                    //Static meshes
                    room.NumStaticMeshes = reader.ReadUInt16();
                    room.StaticMeshes = new TR2RoomStaticMesh[room.NumStaticMeshes];
                    for (int j = 0; j < room.NumStaticMeshes; j++)
                    {
                        TR2RoomStaticMesh mesh = new TR2RoomStaticMesh
                        {
                            X = reader.ReadUInt32(),
                            Y = reader.ReadUInt32(),
                            Z = reader.ReadUInt32(),
                            Rotation = reader.ReadUInt16(),
                            Intensity1 = reader.ReadUInt16(),
                            Intensity2 = reader.ReadUInt16(),
                            MeshID = reader.ReadUInt16()
                        };

                        room.StaticMeshes[j] = mesh;
                    }

                    room.AlternateRoom = reader.ReadInt16();
                    room.Flags = reader.ReadInt16();

                    level.Rooms[i] = room;
                }
                #endregion

                level.NumFloorData = reader.ReadUInt32();
                level.FloorData = new ushort[level.NumFloorData];

                for (int i = 0; i < level.NumFloorData; i++)
                {
                    level.FloorData[i] = reader.ReadUInt16();
                }

                #region MeshData
                //This tells us how much mesh data (# of words/uint16s) coming up
                //just like the rooms previously.
                level.NumMeshData = reader.ReadUInt32();
                ushort[] TempMeshData = new ushort[level.NumMeshData];

                for (int i = 0; i < level.NumMeshData; i++)
                {
                    TempMeshData[i] = reader.ReadUInt16();
                }

                level.NumMeshPointers = reader.ReadUInt32();
                level.MeshPointers = new uint[level.NumMeshPointers];

                for (int i = 0; i < level.NumMeshPointers; i++)
                {
                    level.MeshPointers[i] = reader.ReadUInt32();
                }

                //level.Meshes = ConstructMeshData(level.NumMeshData, level.NumMeshPointers, TempMeshData);
                #endregion

                #region Animation
                level.NumAnimations = reader.ReadUInt32();
                level.Animations = new TRAnimation[level.NumAnimations];

                for (int i = 0; i < level.NumAnimations; i++)
                {
                    TRAnimation anim = new TRAnimation
                    {
                        FrameOffset = reader.ReadUInt32(),
                        FrameRate = reader.ReadByte(),
                        FrameSize = reader.ReadByte(),
                        StateID = reader.ReadUInt16(),
                        Speed = new FixedFloat<short, ushort>
                        {
                            Whole = reader.ReadInt16(),
                            Fraction = reader.ReadUInt16()
                        },
                        Accel = new FixedFloat<short, ushort>
                        {
                            Whole = reader.ReadInt16(),
                            Fraction = reader.ReadUInt16()
                        },
                        FrameStart = reader.ReadUInt16(),
                        FrameEnd = reader.ReadUInt16(),
                        NextAnimation = reader.ReadUInt16(),
                        NextFrame = reader.ReadUInt16(),
                        NumStateChanges = reader.ReadUInt16(),
                        StateChangeOffset = reader.ReadUInt16(),
                        NumAnimCommands = reader.ReadUInt16(),
                        AnimCommand = reader.ReadUInt16()
                    };

                    level.Animations[i] = anim;
                }

                level.NumStateChanges = reader.ReadUInt32();
                level.StateChanges = new TRStateChange[level.NumStateChanges];

                for (int i = 0; i < level.NumStateChanges; i++)
                {
                    TRStateChange sch = new TRStateChange()
                    {
                        StateID = reader.ReadUInt16(),
                        NumAnimDispatches = reader.ReadUInt16(),
                        AnimDispatch = reader.ReadUInt16()
                    };

                    level.StateChanges[i] = sch;
                }

                level.NumAnimDispatches = reader.ReadUInt32();
                level.AnimDispatches = new TRAnimDispatch[level.NumAnimDispatches];

                for (int i = 0; i < level.NumAnimDispatches; i++)
                {
                    TRAnimDispatch dispatch = new TRAnimDispatch()
                    {
                        Low = reader.ReadInt16(),
                        High = reader.ReadInt16(),
                        NextAnimation = reader.ReadInt16(),
                        NextFrame = reader.ReadInt16()
                    };

                    level.AnimDispatches[i] = dispatch;
                }

                level.NumAnimCommands = reader.ReadUInt32();
                level.AnimCommands = new TRAnimCommand[level.NumAnimCommands];

                for (int i = 0; i < level.NumAnimCommands; i++)
                {
                    TRAnimCommand cmd = new TRAnimCommand()
                    {
                        Value = reader.ReadInt16()
                    };

                    level.AnimCommands[i] = cmd;
                }
                #endregion

                Log.LogF("Bytes Read: " + reader.BaseStream.Position.ToString() + "/" + reader.BaseStream.Length.ToString());
            }

            return level;
        }

        private static TRColour[] PopulateColourPalette(byte[] palette)
        {
            TRColour[] colourPalette = new TRColour[MAX_PALETTE_SIZE];

            int ci = 0;

            for (int i = 0; i < MAX_PALETTE_SIZE; i++)
            {
                TRColour col = new TRColour();

                col.Red = palette[ci];
                ci++;

                col.Green = palette[ci];
                ci++;

                col.Blue = palette[ci];
                ci++;

                colourPalette[i] = col;
            }

            return colourPalette;
        }

        private static TRColour4[] PopulateColourPalette16(byte[] palette)
        {
            TRColour4[] colourPalette = new TRColour4[MAX_PALETTE_SIZE];

            int ci = 0;

            for (int i = 0; i < MAX_PALETTE_SIZE; i++)
            {
                TRColour4 col = new TRColour4();

                col.Red = palette[ci];
                ci++;

                col.Green = palette[ci];
                ci++;

                col.Blue = palette[ci];
                ci++;

                col.Unused = palette[ci];
                ci++;

                colourPalette[i] = col;
            }

            return colourPalette;
        }

        private static TRRoomData ConvertToRoomData(TR2Room room)
        {
            int RoomDataOffset = 0;

            //Grab detailed room data
            TRRoomData RoomData = new TRRoomData();

            //Room vertices
            RoomData.NumVertices = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Vertices = new TR2RoomVertex[RoomData.NumVertices];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumVertices; j++)
            {
                TR2RoomVertex vertex = new TR2RoomVertex();

                vertex.Vertex.X = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                vertex.Vertex.Y = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                vertex.Vertex.Z = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                vertex.Lighting = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                vertex.Attributes = room.Data[RoomDataOffset];
                RoomDataOffset++;
                vertex.Lighting2 = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;

                RoomData.Vertices[j] = vertex;
            }

            //Room rectangles
            RoomData.NumRectangles = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Rectangles = new TRFace4[RoomData.NumRectangles];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumRectangles; j++)
            {
                TRFace4 face = new TRFace4();

                face.Vertices = new ushort[4];
                face.Vertices[0] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[1] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[2] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[3] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Texture = room.Data[RoomDataOffset];
                RoomDataOffset++;

                RoomData.Rectangles[j] = face;
            }

            //Room triangles
            RoomData.NumTriangles = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Triangles = new TRFace3[RoomData.NumTriangles];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumTriangles; j++)
            {
                TRFace3 face = new TRFace3();

                face.Vertices = new ushort[3];
                face.Vertices[0] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[1] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[2] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Texture = room.Data[RoomDataOffset];
                RoomDataOffset++;

                RoomData.Triangles[j] = face;
            }

            //Room sprites
            RoomData.NumSprites = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Sprites = new TRRoomSprite[RoomData.NumSprites];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumSprites; j++)
            {
                TRRoomSprite face = new TRRoomSprite();

                face.Vertex = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                face.Texture = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                
                RoomData.Sprites[j] = face;
            }

            Debug.Assert(RoomDataOffset == room.NumDataWords);

            return RoomData;
        }

        private static TRMesh[] ConstructMeshData(uint DataCount, uint NumPointers, ushort[] MeshData)
        {
            //Track where we are in mesh data
            int MeshDataOffset = 0;

            //Temp storage for forming an int from two ushorts 
            ushort LowBytes;
            ushort HighBytes;

            //We know the amount of meshes via NumPointers and we know amount of mesh data via DataCount
            //The mesh data as uint16s/words are stored in MeshData

            //We need to pack that data into the objects
            TRMesh[] meshes = new TRMesh[NumPointers];

            for (int i = 0; i < NumPointers; i++)
            {
                TRMesh mesh = new TRMesh();

                //Centre
                mesh.Centre.X = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.Centre.Y = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.Centre.Z = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;

                //Coll Radius
                LowBytes = MeshData[MeshDataOffset];
                MeshDataOffset++;
                HighBytes = MeshData[MeshDataOffset];
                MeshDataOffset++;

                mesh.CollRadius = LowBytes | HighBytes;

                //Vertices
                mesh.NumVetices = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.Vertices = new TRVertex[mesh.NumVetices];

                for (int j = 0; j < mesh.NumVetices; j++)
                {
                    TRVertex v = new TRVertex();

                    v.X = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                    MeshDataOffset++;
                    v.Y = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                    MeshDataOffset++;
                    v.Z = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                    MeshDataOffset++;

                    mesh.Vertices[j] = v;
                }

                //Lights or Normals
                mesh.NumNormals = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;

                if (mesh.NumNormals > 0)
                {
                    mesh.Normals = new TRVertex[mesh.NumNormals];

                    for (int j = 0; j < mesh.NumNormals; j++)
                    {
                        TRVertex v = new TRVertex();

                        v.X = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                        MeshDataOffset++;
                        v.Y = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                        MeshDataOffset++;
                        v.Z = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                        MeshDataOffset++;

                        mesh.Normals[j] = v;
                    }
                }
                else
                {
                    mesh.Lights = new short[Math.Abs(mesh.NumNormals)];

                    for (int j = 0; j < mesh.Lights.Count(); j++)
                    {
                        mesh.Lights[j] = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                        MeshDataOffset++;
                    }
                }

                //Textured Rectangles
                mesh.NumTexturedRectangles = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.TexturedRectangles = new TRFace4[mesh.NumTexturedRectangles];

                for (int j = 0; j < mesh.NumTexturedRectangles; j++)
                {
                    TRFace4 face = new TRFace4();

                    face.Vertices = new ushort[4];

                    face.Vertices[0] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[1] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[2] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[3] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Texture = MeshData[MeshDataOffset];
                    MeshDataOffset++;

                    mesh.TexturedRectangles[j] = face;
                }

                //Textured Triangles
                mesh.NumTexturedTriangles = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.TexturedTriangles = new TRFace3[mesh.NumTexturedTriangles];

                for (int j = 0; j < mesh.NumTexturedTriangles; j++)
                {
                    TRFace3 face = new TRFace3();

                    face.Vertices = new ushort[3];

                    face.Vertices[0] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[1] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[2] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Texture = MeshData[MeshDataOffset];
                    MeshDataOffset++;

                    mesh.TexturedTriangles[j] = face;
                }

                //Coloured Rectangles
                mesh.NumColouredRectangles = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.ColouredRectangles = new TRFace4[mesh.NumColouredRectangles];

                for (int j = 0; j < mesh.NumColouredRectangles; j++)
                {
                    TRFace4 face = new TRFace4();

                    face.Vertices = new ushort[4];

                    face.Vertices[0] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[1] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[2] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[3] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Texture = MeshData[MeshDataOffset];
                    MeshDataOffset++;

                    mesh.ColouredRectangles[j] = face;
                }

                //Coloured Triangles
                mesh.NumColouredTriangles = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.ColouredTriangles = new TRFace3[mesh.NumColouredTriangles];

                for (int j = 0; j < mesh.NumColouredTriangles; j++)
                {
                    TRFace3 face = new TRFace3();

                    face.Vertices = new ushort[3];

                    face.Vertices[0] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[1] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[2] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Texture = MeshData[MeshDataOffset];
                    MeshDataOffset++;

                    mesh.ColouredTriangles[j] = face;
                }

                meshes[i] = mesh;
            }

            //The offset should match the total amount of data.
            Debug.Assert(MeshDataOffset == DataCount);

            return meshes;
        }
    }
}
