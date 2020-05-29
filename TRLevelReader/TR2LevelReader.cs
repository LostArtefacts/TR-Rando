using System;
using System.Collections.Generic;
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

                    //Grab detailed room data
                    TRRoomData RoomData = new TRRoomData();

                    //Room vertices
                    RoomData.NumVertices = reader.ReadInt16();
                    RoomData.Vertices = new TR2RoomVertex[RoomData.NumVertices];
                    for (int j = 0; j < RoomData.NumVertices; j++)
                    {
                        TR2RoomVertex vertex = new TR2RoomVertex
                        {
                            Vertex = new TRVertex
                            {
                                X = reader.ReadInt16(),
                                Y = reader.ReadInt16(),
                                Z = reader.ReadInt16()
                            },

                            Lighting = reader.ReadInt16(),

                            Attributes = reader.ReadUInt16(),

                            Lighting2 = reader.ReadInt16()
                        };

                        RoomData.Vertices[j] = vertex;
                    }

                    //Room rectangles
                    RoomData.NumRectangles = reader.ReadInt16();
                    RoomData.Rectangles = new TRFace4[RoomData.NumRectangles];
                    for (int j = 0; j < RoomData.NumRectangles; j++)
                    {
                        TRFace4 face = new TRFace4
                        {
                            Vertices = new ushort[]
                            {
                                reader.ReadUInt16(),
                                reader.ReadUInt16(),
                                reader.ReadUInt16(),
                                reader.ReadUInt16()
                            },

                            Texture = reader.ReadUInt16()
                        };

                        RoomData.Rectangles[j] = face;
                    }

                    //Room triangles
                    RoomData.NumTriangles = reader.ReadInt16();
                    RoomData.Triangles = new TRFace3[RoomData.NumTriangles];
                    for (int j = 0; j < RoomData.NumTriangles; j++)
                    {
                        TRFace3 face = new TRFace3
                        {
                            Vertices = new ushort[]
                            {
                                reader.ReadUInt16(),
                                reader.ReadUInt16(),
                                reader.ReadUInt16()
                            },

                            Texture = reader.ReadUInt16()
                        };

                        RoomData.Triangles[j] = face;
                    }

                    //Room sprites
                    RoomData.NumSprites = reader.ReadInt16();
                    RoomData.Sprites = new TRRoomSprite[RoomData.NumSprites];
                    for (int j = 0; j < RoomData.NumSprites; j++)
                    {
                        TRRoomSprite face = new TRRoomSprite
                        {
                            Vertex = reader.ReadInt16(),

                            Texture = reader.ReadInt16()
                        };

                        RoomData.Sprites[j] = face;
                    }
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
    }
}
