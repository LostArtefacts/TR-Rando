using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TextureExport.Types;

public static class FaceMapper
{
    public static void DrawFaces(TR1Level level, string lvl, int[] roomNumbers)
    {
        using TR1TexturePacker packer = new(level);
        packer.MaximumTiles = 255;

        Dictionary<int, Dictionary<int, TexturedTileSegment>> rectFaces = new();
        Dictionary<int, Dictionary<int, TexturedTileSegment>> triFaces = new();

        Dictionary<int, Dictionary<int, int>> newRectFaces = new();
        Dictionary<int, Dictionary<int, int>> newTriFaces = new();

        foreach (int roomNumber in roomNumbers)
        {
            rectFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
            triFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
            newRectFaces[roomNumber] = new Dictionary<int, int>();
            newTriFaces[roomNumber] = new Dictionary<int, int>();

            for (int i = 0; i < level.Rooms[roomNumber].Mesh.Rectangles.Count; i++)
            {
                rectFaces[roomNumber][i] = GetFaceSegment(level.Rooms[roomNumber].Mesh.Rectangles[i].Texture, packer.Tiles);
            }
            for (int i = 0; i < level.Rooms[roomNumber].Mesh.Triangles.Count; i++)
            {
                triFaces[roomNumber][i] = GetFaceSegment(level.Rooms[roomNumber].Mesh.Triangles[i].Texture, packer.Tiles);
            }

            foreach (int rectIndex in rectFaces[roomNumber].Keys)
            {
                TexturedTileSegment segment = rectFaces[roomNumber][rectIndex];
                if (segment != null)
                {
                    TexturedTileSegment newSegment = DrawNewFace(segment, "Q" + rectIndex, true);
                    packer.AddRectangle(newSegment);

                    newRectFaces[roomNumber][rectIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                }
            }

            foreach (int triIndex in triFaces[roomNumber].Keys)
            {
                TexturedTileSegment segment = triFaces[roomNumber][triIndex];
                if (segment != null)
                {
                    TexturedTileSegment newSegment = DrawNewFace(segment, "T" + triIndex, true);
                    packer.AddRectangle(newSegment);

                    newTriFaces[roomNumber][triIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                }
            }
        }

        packer.Pack(true);

        foreach (int roomNumber in roomNumbers)
        {
            foreach (int rectIndex in newRectFaces[roomNumber].Keys)
            {
                level.Rooms[roomNumber].Mesh.Rectangles[rectIndex].Texture = (ushort)newRectFaces[roomNumber][rectIndex];
            }
            foreach (int triIndex in newTriFaces[roomNumber].Keys)
            {
                level.Rooms[roomNumber].Mesh.Triangles[triIndex].Texture = (ushort)newTriFaces[roomNumber][triIndex];
            }
        }

        Directory.CreateDirectory(@"TR1\Faces");
        new TR1LevelControl().Write(level, @"TR1\Faces\" + lvl);
    }

    public static void DrawFaces(TR2Level level, string lvl, int[] roomNumbers)
    {
        using TR2TexturePacker packer = new(level);
        packer.MaximumTiles = 255;

        Dictionary<int, Dictionary<int, TexturedTileSegment>> rectFaces = new();
        Dictionary<int, Dictionary<int, TexturedTileSegment>> triFaces = new();

        Dictionary<int, Dictionary<int, int>> newRectFaces = new();
        Dictionary<int, Dictionary<int, int>> newTriFaces = new();

        foreach (int roomNumber in roomNumbers)
        {
            rectFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
            triFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
            newRectFaces[roomNumber] = new Dictionary<int, int>();
            newTriFaces[roomNumber] = new Dictionary<int, int>();

            for (int i = 0; i < level.Rooms[roomNumber].Mesh.Rectangles.Count; i++)
            {
                rectFaces[roomNumber][i] = GetFaceSegment(level.Rooms[roomNumber].Mesh.Rectangles[i].Texture, packer.Tiles);
            }
            for (int i = 0; i < level.Rooms[roomNumber].Mesh.Triangles.Count; i++)
            {
                triFaces[roomNumber][i] = GetFaceSegment(level.Rooms[roomNumber].Mesh.Triangles[i].Texture, packer.Tiles);
            }

            foreach (int rectIndex in rectFaces[roomNumber].Keys)
            {
                TexturedTileSegment segment = rectFaces[roomNumber][rectIndex];
                if (segment != null)
                {
                    TexturedTileSegment newSegment = DrawNewFace(segment, "Q" + rectIndex);
                    packer.AddRectangle(newSegment);

                    newRectFaces[roomNumber][rectIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                }
            }

            foreach (int triIndex in triFaces[roomNumber].Keys)
            {
                TexturedTileSegment segment = triFaces[roomNumber][triIndex];
                if (segment != null)
                {
                    TexturedTileSegment newSegment = DrawNewFace(segment, "T" + triIndex);
                    packer.AddRectangle(newSegment);

                    newTriFaces[roomNumber][triIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                }
            }
        }

        packer.Pack(true);

        foreach (int roomNumber in roomNumbers)
        {
            foreach (int rectIndex in newRectFaces[roomNumber].Keys)
            {
                level.Rooms[roomNumber].Mesh.Rectangles[rectIndex].Texture = (ushort)newRectFaces[roomNumber][rectIndex];
            }
            foreach (int triIndex in newTriFaces[roomNumber].Keys)
            {
                level.Rooms[roomNumber].Mesh.Triangles[triIndex].Texture = (ushort)newTriFaces[roomNumber][triIndex];
            }
        }

        Directory.CreateDirectory(@"TR2\Faces");
        new TR2LevelControl().Write(level, @"TR2\Faces\" + lvl);
    }

    public static void DrawFaces(TR3Level level, string lvl, int[] roomNumbers)
    {
        using TR3TexturePacker packer = new(level);
        packer.MaximumTiles = 255;

        Dictionary<int, Dictionary<int, TexturedTileSegment>> rectFaces = new();
        Dictionary<int, Dictionary<int, TexturedTileSegment>> triFaces = new();

        Dictionary<int, Dictionary<int, int>> newRectFaces = new();
        Dictionary<int, Dictionary<int, int>> newTriFaces = new();

        foreach (int roomNumber in roomNumbers)
        {
            rectFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
            triFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
            newRectFaces[roomNumber] = new Dictionary<int, int>();
            newTriFaces[roomNumber] = new Dictionary<int, int>();

            for (int i = 0; i < level.Rooms[roomNumber].Mesh.Rectangles.Count; i++)
            {
                rectFaces[roomNumber][i] = GetFaceSegment(level.Rooms[roomNumber].Mesh.Rectangles[i].Texture, packer.Tiles);
            }
            for (int i = 0; i < level.Rooms[roomNumber].Mesh.Triangles.Count; i++)
            {
                triFaces[roomNumber][i] = GetFaceSegment(level.Rooms[roomNumber].Mesh.Triangles[i].Texture, packer.Tiles);
            }

            foreach (int rectIndex in rectFaces[roomNumber].Keys)
            {
                TexturedTileSegment segment = rectFaces[roomNumber][rectIndex];
                if (segment != null)
                {
                    TexturedTileSegment newSegment = DrawNewFace(segment, "Q" + rectIndex);
                    packer.AddRectangle(newSegment);

                    newRectFaces[roomNumber][rectIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                }
            }

            foreach (int triIndex in triFaces[roomNumber].Keys)
            {
                TexturedTileSegment segment = triFaces[roomNumber][triIndex];
                if (segment != null)
                {
                    TexturedTileSegment newSegment = DrawNewFace(segment, "T" + triIndex);
                    packer.AddRectangle(newSegment);

                    newTriFaces[roomNumber][triIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
                }
            }
        }

        packer.Pack(true);

        foreach (int roomNumber in roomNumbers)
        {
            foreach (int rectIndex in newRectFaces[roomNumber].Keys)
            {
                level.Rooms[roomNumber].Mesh.Rectangles[rectIndex].Texture = (ushort)newRectFaces[roomNumber][rectIndex];
            }
            foreach (int triIndex in newTriFaces[roomNumber].Keys)
            {
                level.Rooms[roomNumber].Mesh.Triangles[triIndex].Texture = (ushort)newTriFaces[roomNumber][triIndex];
            }
        }

        Directory.CreateDirectory(@"TR3\Faces");
        new TR3LevelControl().Write(level, @"TR3\Faces\" + lvl);
    }

    public static void DrawBoxes(TR2Level level, string lvl, int[] roomNumbers)
    {
        using TR2TexturePacker packer = new(level);
        packer.MaximumTiles = 10000;

        Dictionary<int, Dictionary<int, TexturedTileSegment>> rectFaces = new();
        Dictionary<int, Dictionary<int, int>> newRectFaces = new();

        FDControl control = new();
        control.ParseFromLevel(level);

        foreach (int roomNumber in roomNumbers)
        {
            rectFaces[roomNumber] = new Dictionary<int, TexturedTileSegment>();
            newRectFaces[roomNumber] = new Dictionary<int, int>();

            for (int i = 0; i < level.Rooms[roomNumber].Mesh.Rectangles.Count; i++)
            {
                TexturedTileSegment seg = GetBoxFaceSegment(level.Rooms[roomNumber], i, packer.Tiles);
                if (seg != null)
                {
                    rectFaces[roomNumber][i] = seg;
                }
            }

            foreach (int rectIndex in rectFaces[roomNumber].Keys)
            {
                TexturedTileSegment segment = rectFaces[roomNumber][rectIndex];
                TexturedTileSegment newSegment = DrawNewFace(segment, GetBoxDescription(level, control, roomNumber, rectIndex));
                packer.AddRectangle(newSegment);

                newRectFaces[roomNumber][rectIndex] = level.ObjectTextures.Count;
                level.ObjectTextures.Add((newSegment.FirstTexture as IndexedTRObjectTexture).Texture);
            }
        }

        packer.Pack(true);

        foreach (int roomNumber in roomNumbers)
        {
            foreach (int rectIndex in newRectFaces[roomNumber].Keys)
            {
                level.Rooms[roomNumber].Mesh.Rectangles[rectIndex].Texture = (ushort)newRectFaces[roomNumber][rectIndex];
            }
        }

        Directory.CreateDirectory(@"TR2\Boxes");
        new TR2LevelControl().Write(level, @"TR2\Boxes\" + lvl);
    }

    private static TexturedTileSegment DrawNewFace(TexturedTileSegment segment, string text, bool fillBackground = false)
    {
        Bitmap bitmap = segment.Bitmap.Clone(new Rectangle(0, 0, segment.Width, segment.Height), PixelFormat.Format32bppArgb);
        
        Graphics g = Graphics.FromImage(bitmap);

        if (fillBackground)
        {
            g.FillRectangle(Brushes.Black, 0, 0, segment.Width, segment.Height);
        }

        g.SmoothingMode = SmoothingMode.Default;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawString(text, new Font("Tahoma", 8), Brushes.Red, new RectangleF(0, 0, bitmap.Width, bitmap.Height));

        g.Flush();

        return new TexturedTileSegment(CreateTexture(segment.Bounds), bitmap);
    }

    private static TexturedTileSegment GetFaceSegment(int textureIndex, IReadOnlyList<TexturedTile> tiles)
    {
        List<int> indices = new() { textureIndex & 0x0fff };
        foreach (TexturedTile tile in tiles)
        {
            List<TexturedTileSegment> segments = tile.GetObjectTextureIndexSegments(indices);
            if (segments.Count > 0)
            {
                return segments[0];
            }
        }
        return null;
    }

    private static IndexedTRObjectTexture CreateTexture(Rectangle rectangle)
    {
        // Make a dummy texture object with the given bounds
        TRObjectTexture texture = new()
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

    private static TRObjectTextureVert CreatePoint(int x, int y)
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

    private static TexturedTileSegment GetBoxFaceSegment(TR2Room room, int rectIndex, IReadOnlyList<TexturedTile> tiles)
    {
        TRFace face = room.Mesh.Rectangles[rectIndex];

        List<TRVertex> verts = new()
        {
            room.Mesh.Vertices[face.Vertices[0]].Vertex,
            room.Mesh.Vertices[face.Vertices[1]].Vertex,
            room.Mesh.Vertices[face.Vertices[2]].Vertex,
            room.Mesh.Vertices[face.Vertices[3]].Vertex
        };

        // Ignore walls
        if (verts.All(v => v.X == verts[0].X) || verts.All(v => v.Z == verts[0].Z))
        {
            return null;
        }

        List<int> indices = new() { face.Texture };
        foreach (TexturedTile tile in tiles)
        {
            List<TexturedTileSegment> segments = tile.GetObjectTextureIndexSegments(indices);
            if (segments.Count > 0)
            {
                return segments[0];
            }
        }
        return null;
    }

    private static string GetBoxDescription(TR2Level level, FDControl control, int roomNumber, int rectIndex)
    {
        TR2Room room = level.Rooms[roomNumber];
        TRFace face = room.Mesh.Rectangles[rectIndex];
        List<TRVertex> verts = new()
        {
            room.Mesh.Vertices[face.Vertices[0]].Vertex,
            room.Mesh.Vertices[face.Vertices[1]].Vertex,
            room.Mesh.Vertices[face.Vertices[2]].Vertex,
            room.Mesh.Vertices[face.Vertices[3]].Vertex
        };

        int xmin = verts.Min(v => v.X) + room.Info.X;
        int zmin = verts.Min(v => v.Z) + room.Info.Z;

        TRRoomSector sector = FDUtilities.GetRoomSector(xmin, verts[0].Y, zmin, (short)roomNumber, level, control);
        if (sector.BoxIndex == ushort.MaxValue)
        {
            return "NOBOX";
        }

        TR2Box box = level.Boxes[sector.BoxIndex];
        List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, box);

        string info = "B" + sector.BoxIndex;
        foreach (int overlap in overlaps)
        {
            info += Environment.NewLine + "  " + overlap;
        }
        return info;
    }
}
