using System.Drawing;

namespace TRLevelControl.Model;

public class TRObjectTexture : TRTexture
{
    public TRObjectTextureMode Target { get; set; }
    public TRBlendingMode BlendingMode { get; set; }
    public List<TRObjectTextureVert> Vertices { get; set; }
    public bool IsTriangle { get; set; }
    public byte BumpLevel { get; set; }
    public byte MappingCorrection { get; set; }
    public bool UnknownFlag { get; set; }

    public bool HasTriangleVertex =>
        Vertices[^1].IsEmpty || IsTriangle;

    public TRUVMode UVMode
    {
        get => GetUVMode();
        set => SetUVMode(value);
    }

    public TRObjectTexture() { }

    public TRObjectTexture(int x, int y, int w, int h)
        : this(new(x, y, w, h)) { }

    public TRObjectTexture(Rectangle bounds)
    {
        Vertices = new()
        {
            new((ushort)bounds.Left, (ushort)bounds.Top),
            new((ushort)(bounds.Right - 1), (ushort)bounds.Top),
            new((ushort)(bounds.Right - 1), (ushort)(bounds.Bottom - 1)),
            new((ushort)bounds.Left, (ushort)(bounds.Bottom - 1)),
        };
    }

    protected TRUVMode GetUVMode()
    {
        List<TRObjectTextureVert> verts = GetExtendedPoints();
        Rectangle bounds = GetBounds();

        TRObjectTextureVert root = verts[0];
        TRObjectTextureVert last = verts[3];

        if (root.X == last.X)
        {
            if (root.X == bounds.X)
            {
                return root.Y < last.Y ? TRUVMode.NW_Clockwise : TRUVMode.SW_AntiClockwise;
            }
            return root.Y < last.Y ? TRUVMode.NE_AntiClockwise : TRUVMode.SE_Clockwise;
        }

        if (root.Y == bounds.Y)
        {
            return root.X > last.X ? TRUVMode.NE_Clockwise : TRUVMode.NW_AntiClockwise;
        }
        return root.X > last.X ? TRUVMode.SE_AntiClockwise : TRUVMode.SW_Clockwise;
    }

    public List<TRObjectTextureVert> GetExtendedPoints()
    {
        if (!HasTriangleVertex)
            return new(Vertices);

        List<TRObjectTextureVert> vertices = Vertices.GetRange(0, 3);
        Point missingPoint = GetPoints().Find(p =>
            vertices.FindIndex(v => v.X == p.X && v.Y == p.Y) == -1);

        TRObjectTextureVert lastVertex = vertices[0];
        int insertIndex = -1;
        for (int i = 1; i < 3; i++)
        {
            TRObjectTextureVert vert = vertices[i];
            if (vert.X - lastVertex.X != 0 && vert.Y - lastVertex.Y != 0)
            {
                insertIndex = i;
                break;
            }
            lastVertex = vert;
        }

        TRObjectTextureVert newVert = new((ushort)missingPoint.X, (ushort)missingPoint.Y);
        if (insertIndex == -1)
        {
            vertices.Add(newVert);
        }
        else
        {
            vertices.Insert(insertIndex, newVert);
        }

        return vertices;
    }

    public void FlipHorizontal()
    {
        switch (UVMode)
        {
            case TRUVMode.NW_Clockwise:
                UVMode = TRUVMode.NE_AntiClockwise;
                break;
            case TRUVMode.NE_AntiClockwise:
                UVMode = TRUVMode.NW_Clockwise;
                break;
            case TRUVMode.SW_AntiClockwise:
                UVMode = TRUVMode.SE_Clockwise;
                break;
            case TRUVMode.SE_Clockwise:
                UVMode = TRUVMode.SW_AntiClockwise;
                break;
            case TRUVMode.NE_Clockwise:
                UVMode = TRUVMode.NW_AntiClockwise;
                break;
            case TRUVMode.NW_AntiClockwise:
                UVMode = TRUVMode.NE_Clockwise;
                break;
            case TRUVMode.SW_Clockwise:
                UVMode = TRUVMode.SE_AntiClockwise;
                break;
            case TRUVMode.SE_AntiClockwise:
                UVMode = TRUVMode.SW_Clockwise;
                break;
        }
    }

    public void FlipVertical()
    {
        switch (UVMode)
        {
            case TRUVMode.NW_Clockwise:
                UVMode = TRUVMode.SW_AntiClockwise;
                break;
            case TRUVMode.SW_AntiClockwise:
                UVMode = TRUVMode.NW_Clockwise;
                break;
            case TRUVMode.NE_Clockwise:
                UVMode = TRUVMode.SE_AntiClockwise;
                break;
            case TRUVMode.SE_AntiClockwise:
                UVMode = TRUVMode.NE_Clockwise;
                break;
            case TRUVMode.SE_Clockwise:
                UVMode = TRUVMode.NE_AntiClockwise;
                break;
            case TRUVMode.NE_AntiClockwise:
                UVMode = TRUVMode.SE_Clockwise;
                break;
            case TRUVMode.SW_Clockwise:
                UVMode = TRUVMode.NW_AntiClockwise;
                break;
            case TRUVMode.NW_AntiClockwise:
                UVMode = TRUVMode.SW_Clockwise;
                break;
        }
    }

    protected void SetUVMode(TRUVMode mode)
    {
        if (mode == UVMode)
            return;

        List<TRObjectTextureVert> verts = GetExtendedPoints();
        List<Point> points = GetPoints();

        // Rebase to NW_Clockwise
        verts.Sort((v1, v2) =>
        {
            return points.FindIndex(p => p.X == v1.X && p.Y == v1.Y)
                .CompareTo(points.FindIndex(p => p.X == v2.X && p.Y == v2.Y));
        });

        int rotations = (int)mode % (int)TRUVMode.NW_AntiClockwise;
        for (int i = 0; i < rotations; i++)
        {
            TRObjectTextureVert first = verts[0];
            for (int j = 0; j < verts.Count - 1; j++)
            {
                verts[j] = verts[j + 1];
            }
            verts[^1] = first;
        }

        if (mode >= TRUVMode.NW_AntiClockwise)
        {
            (verts[3], verts[1]) = (verts[1], verts[3]);
        }

        if (HasTriangleVertex)
        {
            verts.RemoveAt(3);
            verts.Add(Vertices[^1]);
        }

        Vertices.Clear();
        Vertices.AddRange(verts);
    }

    protected override Rectangle GetBounds()
    {
        int minX = TRConsts.TPageWidth - 1, minY = TRConsts.TPageHeight - 1;
        int maxX = 0, maxY = 0;

        for (int i = 0; i < Vertices.Count; i++)
        {
            TRObjectTextureVert vert = Vertices[i];
            if (IsTriangleVertex(vert))
            {
                continue;
            }

            minX = Math.Min(minX, vert.X);
            maxX = Math.Max(maxX, vert.X);
            minY = Math.Min(minY, vert.Y);
            maxY = Math.Max(maxY, vert.Y);
        }

        return new(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    protected override void SetPosition(Point position)
    {
        Rectangle oldBounds = GetBounds();

        int xDiff = position.X - oldBounds.X;
        int yDiff = position.Y - oldBounds.Y;

        foreach (TRObjectTextureVert vert in Vertices)
        {
            if (IsTriangleVertex(vert))
            {
                continue;
            }

            vert.X += xDiff;
            vert.Y += yDiff;
        }
    }

    protected override void SetSize(Size size)
    {
        Rectangle oldBounds = GetBounds();
        int wDiff = size.Width - oldBounds.Width;
        int hDiff = size.Height - oldBounds.Height;

        foreach (TRObjectTextureVert vert in Vertices)
        {
            if (IsTriangleVertex(vert))
            {
                continue;
            }

            if (vert.X == oldBounds.Right - 1)
            {
                vert.X += wDiff;
            }
            if (vert.Y == oldBounds.Bottom - 1)
            {
                vert.Y += hDiff;
            }
        }
    }

    protected bool IsTriangleVertex(TRObjectTextureVert vert)
    {
        return Vertices.IndexOf(vert) == Vertices.Count - 1
            && (vert.IsEmpty || IsTriangle);
    }

    public override TRObjectTexture Clone()
    {
        return new()
        {
            Atlas = Atlas,
            BlendingMode = BlendingMode,
            BumpLevel = BumpLevel,
            IsTriangle = IsTriangle,
            MappingCorrection = MappingCorrection,
            Target = Target,
            UnknownFlag = UnknownFlag,
            Vertices = new(Vertices.Select(v => v.Clone()))
        };
    }
}
