using System.Drawing;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl;
using TRLevelControl.Model;

namespace TextureExport.Types;

public static class FaceMapper
{
    public static void DrawFaces(TR1Level level, string lvl, IEnumerable<int> roomNumbers, bool solidBackground)
    {
        DrawFaces(level, roomNumbers.Select(r => level.Rooms[r].Mesh), solidBackground);
        Directory.CreateDirectory("TR1/Faces");
        new TR1LevelControl().Write(level, "TR1/Faces/" + lvl);
    }

    public static void DrawFaces(TR2Level level, string lvl, IEnumerable<int> roomNumbers, bool solidBackground)
    {
        DrawFaces(level, roomNumbers.Select(r => level.Rooms[r].Mesh), solidBackground);
        Directory.CreateDirectory("TR2/Faces");
        new TR2LevelControl().Write(level, "TR2/Faces/" + lvl);
    }

    public static void DrawFaces(TR3Level level, string lvl, IEnumerable<int> roomNumbers, bool solidBackground)
    {
        DrawFaces(level, roomNumbers.Select(r => level.Rooms[r].Mesh), solidBackground);
        Directory.CreateDirectory("TR3/Faces");
        new TR3LevelControl().Write(level, "TR3/Faces/" + lvl);
    }

    public static void DrawFaces<T, V>(TRLevelBase level, IEnumerable<TRRoomMesh<T, V>> meshes, bool solidBackground)
        where T : Enum
        where V : TRRoomVertex
    {
        var packer = GetPacker(level);

        void RedrawFace(TRFace face, int index)
        {
            var region = GetFaceSegment(face.Texture, packer.Tiles);
            if (region == null)
            {
                return;
            }

            bool isTriangle = face.Type == TRFaceType.Triangle;
            var newRegion = DrawNewFace(region, $"{(isTriangle ? "T" : "Q")}{index}", isTriangle, solidBackground);
            packer.AddRectangle(newRegion);

            face.Texture = (ushort)level.ObjectTextures.Count;
            level.ObjectTextures.Add(newRegion.Segments[0].Texture as TRObjectTexture);
        }

        foreach (var mesh in meshes)
        {
            for (int i = 0; i < mesh.Rectangles.Count; i++)
            {
                RedrawFace(mesh.Rectangles[i], i);
            }
            for (int i = 0; i < mesh.Triangles.Count; i++)
            {
                RedrawFace(mesh.Triangles[i], i);
            }
        }

        packer.Pack(true);
    }

    private static TRTexturePacker GetPacker(TRLevelBase level)
    {
        if (level is TR1Level level1)
        {
            return new TR1TexturePacker(level1, 1024);
        }
        else if (level is TR2Level level2)
        {
            return new TR2TexturePacker(level2, 1024);
        }
        else if (level is TR3Level level3)
        {
            return new TR3TexturePacker(level3, 1024);
        }
        throw new Exception();
    }

    public static void DrawBoxes(TR2Level level, string lvl, IEnumerable<int> roomNumbers)
    {
        TR2TexturePacker packer = new(level, 10000);

        Dictionary<int, Dictionary<int, TRTextileRegion>> rectFaces = new();
        Dictionary<int, Dictionary<int, int>> newRectFaces = new();

        foreach (int roomNumber in roomNumbers)
        {
            rectFaces[roomNumber] = new Dictionary<int, TRTextileRegion>();
            newRectFaces[roomNumber] = new Dictionary<int, int>();

            for (int i = 0; i < level.Rooms[roomNumber].Mesh.Rectangles.Count; i++)
            {
                TRTextileRegion seg = GetBoxFaceSegment(level.Rooms[roomNumber], i, packer.Tiles);
                if (seg != null)
                {
                    rectFaces[roomNumber][i] = seg;
                }
            }

            foreach (int rectIndex in rectFaces[roomNumber].Keys)
            {
                TRTextileRegion region = rectFaces[roomNumber][rectIndex];
                TRTextileRegion newRegion = DrawNewFace(region, GetBoxDescription(level, roomNumber, rectIndex), false);
                packer.AddRectangle(newRegion);

                newRectFaces[roomNumber][rectIndex] = level.ObjectTextures.Count;
                level.ObjectTextures.Add(newRegion.Segments[0].Texture as TRObjectTexture);
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

        Directory.CreateDirectory("TR2/Boxes");
        new TR2LevelControl().Write(level, "TR2/Boxes/" + lvl);
    }

    private static TRTextileRegion DrawNewFace(TRTextileRegion segment, string text, bool isTriangle, bool fillBackground = false)
    {
        TRImage image = segment.Image.Clone();
        if (fillBackground)
        {
            image.Fill(Color.Black);
        }
        image.DrawRectangle(Color.Red, 0, 0, image.Width - 1, image.Height - 1);

        const string fontName = "Arial";
        const int fontSize = 12;

        var size = TRImage.MeasureString(text, fontName, fontSize);
        var width = image.Width - size.Width;
        var height = image.Height - size.Height;
        if (isTriangle)
        {
            // Can't guarantee orientation on face is good, so improve readability chance
            const int padding = 8;
            var corners = new(int x, int y)[]
            {
                (padding, padding),
                (width - padding, padding),
                (width - padding, height - padding),
                (padding, height - padding),
            };

            foreach (var (x, y) in corners)
            {
                image.DrawString(text, fontName, fontSize, Color.Red, x, y);
            }
        }
        else
        {
            image.DrawString(text, fontName, fontSize, Color.Red, width / 2, height / 2);
        }

        return new TRTextileRegion(CreateTexture(segment.Bounds), image);
    }

    private static TRTextileRegion GetFaceSegment(int textureIndex, IReadOnlyList<TRTextile> tiles)
    {
        List<int> indices = new() { textureIndex };
        foreach (TRTextile tile in tiles)
        {
            List<TRTextileRegion> regions = tile.GetObjectRegions(indices);
            if (regions.Count > 0)
            {
                return regions[0];
            }
        }
        return null;
    }

    private static TRTextileSegment CreateTexture(Rectangle rectangle)
    {
        return new()
        {
            Texture = new TRObjectTexture(rectangle)
        };
    }

    private static TRTextileRegion GetBoxFaceSegment(TR2Room room, int rectIndex, IReadOnlyList<TRTextile> tiles)
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
        foreach (TRTextile tile in tiles)
        {
            List<TRTextileRegion> regions = tile.GetObjectRegions(indices);
            if (regions.Count > 0)
            {
                return regions[0];
            }
        }
        return null;
    }

    private static string GetBoxDescription(TR2Level level, int roomNumber, int rectIndex)
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

        TRRoomSector sector = room.GetSector(xmin, zmin);
        if (sector.BoxIndex == TRConsts.NoBox)
        {
            return "NOBOX";
        }

        TRBox box = level.Boxes[sector.BoxIndex];
        string info = "B" + sector.BoxIndex;
        foreach (int overlap in box.Overlaps)
        {
            info += Environment.NewLine + "  " + overlap;
        }
        return info;
    }
}
