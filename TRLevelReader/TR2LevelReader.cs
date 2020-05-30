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
                int bytesRead = 0;

                Log.LogF("File opened");

                level.Version = reader.ReadUInt32();
                bytesRead += sizeof(uint);

                if (level.Version != TR2VersionHeader)
                {
                    throw new NotImplementedException("File reader only suppors TR2 levels");
                }

                level.Palette = PopulateColourPalette(reader.ReadBytes((int)MAX_PALETTE_SIZE * 3));
                bytesRead += (int)(sizeof(byte) * (MAX_PALETTE_SIZE * 3));
                level.Palette16 = PopulateColourPalette16(reader.ReadBytes((int)MAX_PALETTE_SIZE * 4));
                bytesRead += (int)(sizeof(byte) * (MAX_PALETTE_SIZE * 4));

                level.NumImages = reader.ReadUInt32();
                bytesRead += sizeof(uint);

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
                    bytesRead += (sizeof(byte) * (256 * 256));
                }                

                //For each texture16 there are 256 * 256 * 2 bytes (131072)
                for (int i = 0; i < level.NumImages; i++)
                {
                    for (int j = 0; j < (256 * 256); j++)
                    {
                        level.Images16[i].Pixels[j] = reader.ReadUInt16();
                        bytesRead += sizeof(ushort);
                    }
                }

                level.Unused = reader.ReadUInt32();
                level.NumRooms = reader.ReadUInt16();
                bytesRead += (sizeof(ushort));

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

                Log.LogF("Bytes Read: " + bytesRead.ToString());
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
    }
}
