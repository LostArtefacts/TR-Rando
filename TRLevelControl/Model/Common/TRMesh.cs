namespace TRLevelControl.Model;

public class TRMesh : ICloneable
{
    public TRVertex Centre { get; set; } = new();
    public int CollRadius { get; set; }
    public List<TRVertex> Vertices { get; set; } = new();
    public List<TRVertex> Normals { get; set; }
    public List<short> Lights { get; set; }
    public List<TRMeshFace> TexturedRectangles { get; set; } = new();
    public List<TRMeshFace> TexturedTriangles { get; set; } = new();
    public List<TRMeshFace> ColouredRectangles { get; set; } = new();
    public List<TRMeshFace> ColouredTriangles { get; set; } = new();

    public IEnumerable<TRMeshFace> TexturedFaces
        => TexturedRectangles.Concat(TexturedTriangles);

    public IEnumerable<TRMeshFace> ColouredFaces
        => ColouredRectangles.Concat(ColouredTriangles);

    public TRMesh Clone()
    {
        return new()
        {
            Centre = Centre.Clone(),
            CollRadius = CollRadius,
            Vertices = new(Vertices.Select(v => v.Clone())),
            Normals = Normals == null ? null : new(Normals.Select(n => n.Clone())),
            Lights = Lights == null ? null : new(Lights),
            TexturedRectangles = new(TexturedRectangles.Select(t => t.Clone())),
            TexturedTriangles = new(TexturedTriangles.Select(t => t.Clone())),
            ColouredRectangles = new(ColouredRectangles.Select(c => c.Clone())),
            ColouredTriangles = new(ColouredTriangles.Select(c => c.Clone())),
        };
    }

    public void CopyInto(TRMesh otherMesh)
    {
        otherMesh.Centre = Centre;
        otherMesh.CollRadius = CollRadius;
        otherMesh.Vertices = Vertices;
        otherMesh.Normals = Normals;
        otherMesh.Lights = Lights;
        otherMesh.TexturedRectangles = TexturedRectangles;
        otherMesh.TexturedTriangles = TexturedTriangles;
        otherMesh.ColouredRectangles = ColouredRectangles;
        otherMesh.ColouredTriangles = ColouredTriangles;
    }

    public TRBoundingBox GetBounds()
    {
        return new()
        {
            MinX = Vertices.Min(v => v.X),
            MaxX = Vertices.Max(v => v.X),
            MinY = Vertices.Min(v => v.Y),
            MaxY = Vertices.Max(v => v.Y),
            MinZ = Vertices.Min(v => v.Z),
            MaxZ = Vertices.Max(v => v.Z),
        };
    }

    public void SelfCalculateBounds()
    {
        var box = GetBounds();

        Centre.X = (short)((box.MinX + box.MaxX) / 2);
        Centre.Y = (short)((box.MinY + box.MaxY) / 2);
        Centre.Z = (short)((box.MinZ + box.MaxZ) / 2);

        var xs = Math.Abs(box.MaxX - box.MinX);
        var ys = Math.Abs(box.MaxY - box.MinY);
        var zs = Math.Abs(box.MaxZ - box.MinZ);

        var inner = Math.Max(xs, Math.Max(ys, zs)) / 2d;
        var outer = Math.Sqrt(Math.Pow(xs, 2) + Math.Pow(ys, 2) + Math.Pow(zs, 2)) / 2d;
        CollRadius = (int)Math.Ceiling((inner + outer) / 2d);
    }

    public void Scale(float factor)
    {
        var box = GetBounds();
        var midX = (box.MaxX + box.MinX) / 2f;
        var midY = (box.MaxY + box.MinY) / 2f;
        var midZ = (box.MaxZ + box.MinZ) / 2f;

        Vertices.ForEach(v =>
        {
            v.X = (short)((v.X - midX) * factor + midX);
            v.Y = (short)((v.Y - midY) * factor + midY);
            v.Z = (short)((v.Z - midZ) * factor + midZ);
        });

        SelfCalculateBounds();
    }

    object ICloneable.Clone()
        => Clone();
}
