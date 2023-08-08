using System.Drawing;
using TRLevelControl.Model;

namespace TRModelTransporter.Model.Textures;

public class IndexedTRObjectTexture : AbstractIndexedTRTexture
{
    private TRObjectTexture _texture;
    private Dictionary<TRObjectTextureVert, Point> _vertexPoints;

    public override int Atlas
    {
        get => _texture.AtlasAndFlag;
        set => _texture.AtlasAndFlag = (ushort)value;
    }

    public TRObjectTexture Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            GetBoundsFromTexture();
        }
    }

    protected override void GetBoundsFromTexture()
    {
        _vertexPoints = new Dictionary<TRObjectTextureVert, Point>();
        TRObjectTextureVert[] vertices = Texture.Vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            TRObjectTextureVert vertex = vertices[i];
            if (i == vertices.Length - 1 && IsTriangleVertex(vertex))
            {
                continue;
            }

            _vertexPoints[vertex] = VertexToPoint(vertex);
        }

        IEnumerable<Point> points = _vertexPoints.Values;
        int minX = points.Min(p => p.X);
        int minY = points.Min(p => p.Y);
        int maxX = points.Max(p => p.X);
        int maxY = points.Max(p => p.Y);

        _bounds = new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
    }

    protected override void ApplyBoundDiffToTexture(int xDiff, int yDiff)
    {
        // By storing the translated points against the vertices, we can maintain
        // the correct order when the bounds have moved e.g. some textures load
        // the graphic as [top-right,bottom-right,bottom-left,top-left], others
        // as [top-left,top-right.... etc

        List<TRObjectTextureVert> vertices = new List<TRObjectTextureVert>(_vertexPoints.Keys);
        foreach (TRObjectTextureVert vertex in vertices)
        {
            Point p = _vertexPoints[vertex];
            p.X += xDiff;
            p.Y += yDiff;
            ApplyPointToVertex(p, vertex);
            _vertexPoints[vertex] = p;
        }
    }

    private static Point VertexToPoint(TRObjectTextureVert vertex)
    {
        int x = vertex.XCoordinate.Fraction;
        if (vertex.XCoordinate.Whole == 255)
        {
            x++;
        }

        int y = vertex.YCoordinate.Fraction;
        if (vertex.YCoordinate.Whole == 255)
        {
            y++;
        }
        return new Point(x, y);
    }

    private static void ApplyPointToVertex(Point point, TRObjectTextureVert vertex)
    {
        int x = point.X;
        int y = point.Y;
        if (vertex.XCoordinate.Whole == 255)
        {
            x--;
        }

        if (vertex.YCoordinate.Whole == 255)
        {
            y--;
        }

        vertex.XCoordinate.Fraction = (byte)x;
        vertex.YCoordinate.Fraction = (byte)y;
    }

    public bool IsTriangle
    {
        get
        {
            return _vertexPoints.Count == 3;
        }
    }

    private static bool IsTriangleVertex(TRObjectTextureVert vertex)
    {
        return vertex.XCoordinate.Fraction == 0 && vertex.XCoordinate.Whole == 0 && vertex.YCoordinate.Fraction == 0 && vertex.YCoordinate.Whole == 0;
    }

    public override bool Equals(object obj)
    {
        if (obj is IndexedTRObjectTexture otherTexture)
        {
            if (_vertexPoints.Count == otherTexture._vertexPoints.Count)
            {
                // Are all the points in the same order?
                List<Point> points1 = new List<Point>(_vertexPoints.Values);
                List<Point> points2 = new List<Point>(otherTexture._vertexPoints.Values);
                return points1.SequenceEqual(points2);
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        return 613946457 + EqualityComparer<TRObjectTexture>.Default.GetHashCode(_texture);
    }

    public override AbstractIndexedTRTexture Clone()
    {
        TRObjectTexture copiedTexture = new TRObjectTexture
        {
            AtlasAndFlag = Texture.AtlasAndFlag,
            Attribute = Texture.Attribute,
            Vertices = new TRObjectTextureVert[Texture.Vertices.Length]
        };

        for (int i = 0; i < Texture.Vertices.Length; i++)
        {
            copiedTexture.Vertices[i] = new TRObjectTextureVert
            {
                XCoordinate = new FixedFloat16
                {
                    Fraction = Texture.Vertices[i].XCoordinate.Fraction,
                    Whole = Texture.Vertices[i].XCoordinate.Whole
                },
                YCoordinate = new FixedFloat16
                {
                    Fraction = Texture.Vertices[i].YCoordinate.Fraction,
                    Whole = Texture.Vertices[i].YCoordinate.Whole
                }
            };
        }

        return new IndexedTRObjectTexture
        {
            Index = Index,
            Classification = Classification,
            Texture = copiedTexture
        };
    }
}
