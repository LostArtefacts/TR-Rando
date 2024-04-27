using TRLevelControl.Build;

namespace TRLevelControl.Model;

public class TRMesh : ICloneable
{
    public TRVertex Centre { get; set; }
    public int CollRadius { get; set; }
    public List<TRVertex> Vertices { get; set; }
    public List<TRVertex> Normals { get; set; }
    public List<short> Lights { get; set; }
    public List<TRMeshFace> TexturedRectangles { get; set; }
    public List<TRMeshFace> TexturedTriangles { get; set; }
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

    object ICloneable.Clone()
        => Clone();
}
