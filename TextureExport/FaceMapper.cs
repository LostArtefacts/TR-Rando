using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader;
using TRLevelReader.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TextureExport
{
    public class FaceMapper
    {
        private readonly TR2Level _level;
        private TexturePacker _packer;

        public FaceMapper(TR2Level level)
        {
            _level = level;
        }

        public void GenerateFaces(string writePath, int[] roomNumbers)
        {
            using (_packer = new TexturePacker(_level))
            {
                _packer.MaximumTiles = 255;

                Dictionary<int, Dictionary<int, TexturedTileSegment>> rectFaces = new Dictionary<int, Dictionary<int, TexturedTileSegment>>();
                Dictionary<int, Dictionary<int, TexturedTileSegment>> triFaces = new Dictionary<int, Dictionary<int, TexturedTileSegment>>();

                Dictionary<int, Dictionary<int, int>> newRectFaces = new Dictionary<int, Dictionary<int, int>>();
                Dictionary<int, Dictionary<int, int>> newTriFaces = new Dictionary<int, Dictionary<int, int>>();

                List<TRObjectTexture> objectTextures = _level.ObjectTextures.ToList();

                foreach (int roomNumber in roomNumbers)
                {
                    rectFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
                    triFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
                    newRectFaces[roomNumber] = new Dictionary<int, int>();
                    newTriFaces[roomNumber] = new Dictionary<int, int>();

                    for (int i = 0; i < _level.Rooms[roomNumber].RoomData.NumRectangles; i++)
                    {
                        rectFaces[roomNumber][i] = GetFaceSegment(_level.Rooms[roomNumber].RoomData.Rectangles[i].Texture);
                    }
                    for (int i = 0; i < _level.Rooms[roomNumber].RoomData.NumTriangles; i++)
                    {
                        triFaces[roomNumber][i] = GetFaceSegment(_level.Rooms[roomNumber].RoomData.Triangles[i].Texture);
                    }

                    foreach (int rectIndex in rectFaces[roomNumber].Keys)
                    {
                        TexturedTileSegment segment = rectFaces[roomNumber][rectIndex];
                        TexturedTileSegment newSegment = DrawNewFace(segment, "Q" + rectIndex);

                        newRectFaces[roomNumber][rectIndex] = objectTextures.Count;
                        objectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                    }

                    foreach (int triIndex in triFaces[roomNumber].Keys)
                    {
                        TexturedTileSegment segment = triFaces[roomNumber][triIndex];
                        TexturedTileSegment newSegment = DrawNewFace(segment, "T" + triIndex);

                        newTriFaces[roomNumber][triIndex] = objectTextures.Count;
                        objectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                    }
                }

                _packer.Pack(true);

                foreach (int roomNumber in roomNumbers)
                {
                    foreach (int rectIndex in newRectFaces[roomNumber].Keys)
                    {
                        _level.Rooms[roomNumber].RoomData.Rectangles[rectIndex].Texture = (ushort)newRectFaces[roomNumber][rectIndex];
                    }
                    foreach (int triIndex in newTriFaces[roomNumber].Keys)
                    {
                        _level.Rooms[roomNumber].RoomData.Triangles[triIndex].Texture = (ushort)newTriFaces[roomNumber][triIndex];
                    }
                }

                _level.ObjectTextures = objectTextures.ToArray();
                _level.NumObjectTextures = (uint)objectTextures.Count;

                new TR2LevelWriter().WriteLevelToFile(_level, writePath);
            }
        }

        private TexturedTileSegment DrawNewFace(TexturedTileSegment segment, string text)
        {
            Bitmap bitmap = segment.Bitmap.Clone(new Rectangle(0, 0, segment.Width, segment.Height), PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(bitmap);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawString(text, new Font("Tahoma", 8), Brushes.Red, new RectangleF(0, 0, bitmap.Width, bitmap.Height));

            g.Flush();

            TexturedTileSegment newSegment = new TexturedTileSegment(CreateTexture(segment.Bounds), bitmap);
            _packer.AddRectangle(newSegment);
            return newSegment;
        }

        private TexturedTileSegment GetFaceSegment(int textureIndex)
        {
            List<int> indices = new List<int> { textureIndex };
            foreach (TexturedTile tile in _packer.Tiles)
            {
                List<TexturedTileSegment> segments = tile.GetObjectTextureIndexSegments(indices);
                if (segments.Count > 0)
                {
                    return segments[0];
                }
            }
            return null;
        }

        private IndexedTRObjectTexture CreateTexture(Rectangle rectangle)
        {
            // Make a dummy texture object with the given bounds
            TRObjectTexture texture = new TRObjectTexture
            {
                AtlasAndFlag = 0,
                Attribute = 0,
                Vertices = new TRObjectTextureVert[]
                {
                    CreatePoint(0, 0),
                    CreatePoint(rectangle.Width, 0),
                    CreatePoint(rectangle.Width, rectangle.Height),
                    CreatePoint(0, rectangle.Height)
                }
            };

            return new IndexedTRObjectTexture
            {
                Index = 0,
                Texture = texture
            };
        }

        private TRObjectTextureVert CreatePoint(int x, int y)
        {
            return new TRObjectTextureVert
            {
                XCoordinate = new FixedFloat16
                {
                    Whole = (byte)(x == 0 ? 1 : 255),
                    Fraction = (byte)(x == 0 ? 0 : x - 1)
                },
                YCoordinate = new FixedFloat16
                {
                    Whole = (byte)(y == 0 ? 1 : 255),
                    Fraction = (byte)(y == 0 ? 0 : y - 1)
                }
            };
        }

        public void GenerateBoxes(string writePath, int[] roomNumbers)
        {
            using (_packer = new TexturePacker(_level))
            {
                _packer.MaximumTiles = 10000;

                Dictionary<int, Dictionary<int, TexturedTileSegment>> rectFaces = new Dictionary<int, Dictionary<int, TexturedTileSegment>>();
                Dictionary<int, Dictionary<int, int>> newRectFaces = new Dictionary<int, Dictionary<int, int>>();

                List<TRObjectTexture> objectTextures = _level.ObjectTextures.ToList();
                FDControl control = new FDControl();
                control.ParseFromLevel(_level);

                foreach (int roomNumber in roomNumbers)
                {
                    rectFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
                    newRectFaces[roomNumber] = new Dictionary<int, int>();

                    for (int i = 0; i < _level.Rooms[roomNumber].RoomData.NumRectangles; i++)
                    {
                        TexturedTileSegment seg = GetBoxFaceSegment(_level.Rooms[roomNumber], i);
                        if (seg != null)
                        {
                            rectFaces[roomNumber][i] = seg;
                        }
                    }

                    foreach (int rectIndex in rectFaces[roomNumber].Keys)
                    {
                        TexturedTileSegment segment = rectFaces[roomNumber][rectIndex];
                        TexturedTileSegment newSegment = DrawNewFace(segment, GetBoxDescription(control, roomNumber, rectIndex));

                        newRectFaces[roomNumber][rectIndex] = objectTextures.Count;
                        objectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                    }
                }

                _packer.Pack(true);

                foreach (int roomNumber in roomNumbers)
                {
                    foreach (int rectIndex in newRectFaces[roomNumber].Keys)
                    {
                        _level.Rooms[roomNumber].RoomData.Rectangles[rectIndex].Texture = (ushort)newRectFaces[roomNumber][rectIndex];
                    }
                }

                _level.ObjectTextures = objectTextures.ToArray();
                _level.NumObjectTextures = (uint)objectTextures.Count;

                new TR2LevelWriter().WriteLevelToFile(_level, writePath);
            }
        }

        private TexturedTileSegment GetBoxFaceSegment(TR2Room room, int rectIndex)
        {
            TRFace4 face = room.RoomData.Rectangles[rectIndex];

            List<TRVertex> verts = new List<TRVertex>
            {
                room.RoomData.Vertices[face.Vertices[0]].Vertex,
                room.RoomData.Vertices[face.Vertices[1]].Vertex,
                room.RoomData.Vertices[face.Vertices[2]].Vertex,
                room.RoomData.Vertices[face.Vertices[3]].Vertex
            };

            // Ignore walls
            if (verts.All(v => v.X == verts[0].X) || verts.All(v => v.Z == verts[0].Z))
            {
                return null;
            }

            List<int> indices = new List<int> { face.Texture};
            foreach (TexturedTile tile in _packer.Tiles)
            {
                List<TexturedTileSegment> segments = tile.GetObjectTextureIndexSegments(indices);
                if (segments.Count > 0)
                {
                    return segments[0];
                }
            }
            return null;
        }

        private string GetBoxDescription(FDControl control, int roomNumber, int rectIndex)
        {
            TR2Room room = _level.Rooms[roomNumber];
            TRFace4 face = room.RoomData.Rectangles[rectIndex];
            List<TRVertex> verts = new List<TRVertex>
            {
                room.RoomData.Vertices[face.Vertices[0]].Vertex,
                room.RoomData.Vertices[face.Vertices[1]].Vertex,
                room.RoomData.Vertices[face.Vertices[2]].Vertex,
                room.RoomData.Vertices[face.Vertices[3]].Vertex
            };

            int xmin = verts.Min(v => v.X) + room.Info.X;
            int zmin = verts.Min(v => v.Z) + room.Info.Z;

            TRRoomSector sector = FDUtilities.GetRoomSector(xmin, verts[0].Y, zmin, (short)roomNumber, _level, control);
            if (sector.BoxIndex == ushort.MaxValue)
            {
                return "NOBOX";
            }

            TR2Box box = _level.Boxes[sector.BoxIndex];
            int index = box.OverlapIndex & 0x3fff;
            int boxNumber;
            bool done = false;
            List<int> overlaps = new List<int>();

            do
            {
                boxNumber = _level.Overlaps[index++];
                if ((boxNumber & 0x8000) > 0)
                {
                    done = true;
                    boxNumber &= 0x7fff;
                }
                overlaps.Add(boxNumber);
            }
            while (!done);

            string info = "B" + sector.BoxIndex;
            foreach (int overlap in overlaps)
            {
                info += Environment.NewLine + "  " + overlap;
            }
            return info;
        }
    }
}