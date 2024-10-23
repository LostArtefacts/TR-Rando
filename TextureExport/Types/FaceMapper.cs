using System.Drawing;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl;
using TRLevelControl.Model;

namespace TextureExport.Types;

public static class FaceMapper
{
    public static void DrawFaces(TR1Level level, string lvl, int[] roomNumbers)
    {
        TR1TexturePacker packer = new(level, 255);

        Dictionary<int, Dictionary<int, TRTextileRegion>> rectFaces = new();
        Dictionary<int, Dictionary<int, TRTextileRegion>> triFaces = new();

        Dictionary<int, Dictionary<int, int>> newRectFaces = new();
        Dictionary<int, Dictionary<int, int>> newTriFaces = new();

        foreach (int roomNumber in roomNumbers)
        {
            rectFaces[roomNumber] = new Dictionary<int, TRTextileRegion>();
            triFaces[roomNumber] = new Dictionary<int, TRTextileRegion>();
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
                TRTextileRegion region = rectFaces[roomNumber][rectIndex];
                if (region != null)
                {
                    TRTextileRegion newRegion = DrawNewFace(region, "Q" + rectIndex, true);
                    packer.AddRectangle(newRegion);

                    newRectFaces[roomNumber][rectIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add(newRegion.Segments[0].Texture as TRObjectTexture);
                }
            }

            foreach (int triIndex in triFaces[roomNumber].Keys)
            {
                TRTextileRegion region = triFaces[roomNumber][triIndex];
                if (region != null)
                {
                    TRTextileRegion newRegion = DrawNewFace(region, "T" + triIndex, true);
                    packer.AddRectangle(newRegion);

                    newTriFaces[roomNumber][triIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add(newRegion.Segments[0].Texture as TRObjectTexture);
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

        Directory.CreateDirectory("TR1/Faces");
        new TR1LevelControl().Write(level, "TR1/Faces/" + lvl);
    }

    public static void DrawFaces(TR2Level level, string lvl, int[] roomNumbers)
    {
        TR2TexturePacker packer = new(level, 255);

        Dictionary<int, Dictionary<int, TRTextileRegion>> rectFaces = new();
        Dictionary<int, Dictionary<int, TRTextileRegion>> triFaces = new();

        Dictionary<int, Dictionary<int, int>> newRectFaces = new();
        Dictionary<int, Dictionary<int, int>> newTriFaces = new();

        foreach (int roomNumber in roomNumbers)
        {
            rectFaces[roomNumber] = new Dictionary<int, TRTextileRegion>();
            triFaces[roomNumber] = new Dictionary<int, TRTextileRegion>();
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
                TRTextileRegion region = rectFaces[roomNumber][rectIndex];
                if (region != null)
                {
                    TRTextileRegion newRegion = DrawNewFace(region, "Q" + rectIndex);
                    packer.AddRectangle(newRegion);

                    newRectFaces[roomNumber][rectIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add(newRegion.Segments[0].Texture as TRObjectTexture);
                }
            }

            foreach (int triIndex in triFaces[roomNumber].Keys)
            {
                TRTextileRegion region = triFaces[roomNumber][triIndex];
                if (region != null)
                {
                    TRTextileRegion newRegion = DrawNewFace(region, "T" + triIndex);
                    packer.AddRectangle(newRegion);

                    newTriFaces[roomNumber][triIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add(newRegion.Segments[0].Texture as TRObjectTexture);
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

        Directory.CreateDirectory("TR2/Faces");
        new TR2LevelControl().Write(level, "TR2/Faces/" + lvl);
    }

    public static void DrawFaces(TR3Level level, string lvl, int[] roomNumbers)
    {
        TR3TexturePacker packer = new(level, 255);

        Dictionary<int, Dictionary<int, TRTextileRegion>> rectFaces = new();
        Dictionary<int, Dictionary<int, TRTextileRegion>> triFaces = new();

        Dictionary<int, Dictionary<int, int>> newRectFaces = new();
        Dictionary<int, Dictionary<int, int>> newTriFaces = new();

        foreach (int roomNumber in roomNumbers)
        {
            rectFaces[roomNumber] = new Dictionary<int, TRTextileRegion>();
            triFaces[roomNumber] = new Dictionary<int, TRTextileRegion>();
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
                TRTextileRegion region = rectFaces[roomNumber][rectIndex];
                if (region != null)
                {
                    TRTextileRegion newRegion = DrawNewFace(region, "Q" + rectIndex);
                    packer.AddRectangle(newRegion);

                    newRectFaces[roomNumber][rectIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add(newRegion.Segments[0].Texture as TRObjectTexture);
                }
            }

            foreach (int triIndex in triFaces[roomNumber].Keys)
            {
                TRTextileRegion region = triFaces[roomNumber][triIndex];
                if (region != null)
                {
                    TRTextileRegion newRegion = DrawNewFace(region, "T" + triIndex);
                    packer.AddRectangle(newRegion);

                    newTriFaces[roomNumber][triIndex] = level.ObjectTextures.Count;
                    level.ObjectTextures.Add(newRegion.Segments[0].Texture as TRObjectTexture);
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

        Directory.CreateDirectory("TR3/Faces");
        new TR3LevelControl().Write(level, "TR3/Faces/" + lvl);
    }

    public static void DrawBoxes(TR2Level level, string lvl, int[] roomNumbers)
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
                TRTextileRegion newRegion = DrawNewFace(region, GetBoxDescription(level, roomNumber, rectIndex));
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

    private static TRTextileRegion DrawNewFace(TRTextileRegion segment, string text, bool fillBackground = false)
    {
        TRImage image = segment.Image.Clone();
        if (fillBackground)
        {
            image.Fill(Color.Black);
        }

        image.DrawString(text, "Arial", 10, Color.Red, 4, 4);

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
