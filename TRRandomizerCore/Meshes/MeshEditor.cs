using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Helpers;

public class MeshEditor
{
    private int _oldLength;
    private TRMesh _mesh;

    public TRMesh Mesh
    {
        get => _mesh;
        set
        {
            _mesh = value;
            _oldLength = _mesh.Serialize().Length;
        }
    }

    public void ClearAllPolygons()
    {
        ClearTexturedRectangles();
        ClearTexturedTriangles();
        ClearColouredRectangles();
        ClearColouredTriangles();
    }

    public void ClearTexturedRectangles()
    {
        Mesh.TexturedRectangles = Array.Empty<TRFace4>();
        Mesh.NumTexturedRectangles = 0;
    }

    public void RemoveTexturedRectangleRange(int index, int count)
    {
        List<TRFace4> rectangles = Mesh.TexturedRectangles.ToList();
        rectangles.RemoveRange(index, count);
        Mesh.TexturedRectangles = rectangles.ToArray();
        Mesh.NumTexturedRectangles = (short)rectangles.Count;
    }

    public void RemoveTexturedRectangles(IEnumerable<int> indices)
    {
        List<TRFace4> rectangles = Mesh.TexturedRectangles.ToList();
        RemovePolygons(rectangles, indices);
        Mesh.TexturedRectangles = rectangles.ToArray();
        Mesh.NumTexturedRectangles = (short)rectangles.Count;
    }

    public void ClearColouredRectangles()
    {
        Mesh.ColouredRectangles = Array.Empty<TRFace4>();
        Mesh.NumColouredRectangles = 0;
    }

    public void RemoveColouredRectangleRange(int index, int count)
    {
        List<TRFace4> rectangles = Mesh.ColouredRectangles.ToList();
        rectangles.RemoveRange(index, count);
        Mesh.ColouredRectangles = rectangles.ToArray();
        Mesh.NumColouredRectangles = (short)rectangles.Count;
    }

    public void RemoveColouredRectangles(IEnumerable<int> indices)
    {
        List<TRFace4> rectangles = Mesh.ColouredRectangles.ToList();
        RemovePolygons(rectangles, indices);
        Mesh.ColouredRectangles = rectangles.ToArray();
        Mesh.NumColouredRectangles = (short)rectangles.Count;
    }

    public void ClearTexturedTriangles()
    {
        Mesh.TexturedTriangles = Array.Empty<TRFace3>();
        Mesh.NumTexturedTriangles = 0;
    }

    public void RemoveTexturedTriangleRange(int index, int count)
    {
        List<TRFace3> triangles = Mesh.TexturedTriangles.ToList();
        triangles.RemoveRange(index, count);
        Mesh.TexturedTriangles = triangles.ToArray();
        Mesh.NumTexturedTriangles = (short)triangles.Count;
    }

    public void RemoveTexturedTriangles(IEnumerable<int> indices)
    {
        List<TRFace3> triangles = Mesh.TexturedTriangles.ToList();
        RemovePolygons(triangles, indices);
        Mesh.TexturedTriangles = triangles.ToArray();
        Mesh.NumTexturedTriangles = (short)triangles.Count;
    }

    public void ClearColouredTriangles()
    {
        Mesh.ColouredTriangles = Array.Empty<TRFace3>();
        Mesh.NumColouredTriangles = 0;
    }

    public void RemoveColouredTriangleRange(int index, int count)
    {
        List<TRFace3> triangles = Mesh.ColouredTriangles.ToList();
        triangles.RemoveRange(index, count);
        Mesh.ColouredTriangles = triangles.ToArray();
        Mesh.NumColouredTriangles = (short)triangles.Count;
    }

    public void RemoveColouredTriangles(IEnumerable<int> indices)
    {
        List<TRFace3> triangles = Mesh.ColouredTriangles.ToList();
        RemovePolygons(triangles, indices);
        Mesh.ColouredTriangles = triangles.ToArray();
        Mesh.NumColouredTriangles = (short)triangles.Count;
    }

    public void AddTexturedRectangle(TRFace4 face)
    {
        List<TRFace4> rectangles = Mesh.TexturedRectangles.ToList();
        rectangles.Add(face);
        Mesh.TexturedRectangles = rectangles.ToArray();
        Mesh.NumTexturedRectangles = (short)rectangles.Count;
    }

    public void AddTexturedTriangle(TRFace3 face)
    {
        List<TRFace3> triangles = Mesh.TexturedTriangles.ToList();
        triangles.Add(face);
        Mesh.TexturedTriangles = triangles.ToArray();
        Mesh.NumTexturedTriangles = (short)triangles.Count;
    }

    public void AddColouredRectangle(TRFace4 face)
    {
        List<TRFace4> rectangles = Mesh.ColouredRectangles.ToList();
        rectangles.Add(face);
        Mesh.ColouredRectangles = rectangles.ToArray();
        Mesh.NumColouredRectangles = (short)rectangles.Count;
    }

    public void AddColouredTriangle(TRFace3 face)
    {
        List<TRFace3> triangles = Mesh.ColouredTriangles.ToList();
        triangles.Add(face);
        Mesh.ColouredTriangles = triangles.ToArray();
        Mesh.NumColouredTriangles = (short)triangles.Count;
    }

    public static TRMesh CloneMesh(TRMesh mesh)
    {
        return mesh.Clone();
    }

    public static TRMesh CloneMeshAsColoured(TRMesh mesh, ushort paletteIndex)
    {
        TRMesh clone = CloneMesh(mesh);

        clone.ColouredRectangles = new TRFace4[mesh.NumTexturedRectangles];
        clone.ColouredTriangles = new TRFace3[mesh.NumTexturedTriangles];
        clone.NumColouredRectangles = mesh.NumTexturedRectangles;
        clone.NumColouredTriangles = mesh.NumTexturedTriangles;
        clone.NumTexturedRectangles = 0;
        clone.NumTexturedTriangles = 0;
        clone.TexturedRectangles = Array.Empty<TRFace4>();
        clone.TexturedTriangles = Array.Empty<TRFace3>();

        for (int i = 0; i < mesh.NumTexturedRectangles; i++)
        {
            clone.ColouredRectangles[i] = new TRFace4
            {
                Texture = paletteIndex,
                Vertices = new List<ushort>(mesh.TexturedRectangles[i].Vertices).ToArray()
            };
        }

        for (int i = 0; i < mesh.NumTexturedTriangles; i++)
        {
            clone.ColouredTriangles[i] = new TRFace3
            {
                Texture = paletteIndex,
                Vertices = new List<ushort>(mesh.TexturedTriangles[i].Vertices).ToArray()
            };
        }

        return clone;
    }

    public void WriteToLevel(TR1Level level)
    {
        TRMeshUtilities.UpdateMeshPointers(level, Mesh, _oldLength);
        _oldLength = _mesh.Serialize().Length; // in case of any further changes without changing the mesh var
    }

    public void WriteToLevel(TR2Level level)
    {
        TRMeshUtilities.UpdateMeshPointers(level, Mesh, _oldLength);
        _oldLength = _mesh.Serialize().Length; // in case of any further changes without changing the mesh var
    }

    public void WriteToLevel(TR3Level level)
    {
        TRMeshUtilities.UpdateMeshPointers(level, Mesh, _oldLength);
        _oldLength = _mesh.Serialize().Length; // in case of any further changes without changing the mesh var
    }

    private static void RemovePolygons<T>(List<T> polygons, IEnumerable<int> indices)
    {
        // Remove any duplicates and put them in order
        List<int> sortedIndices = new SortedSet<int>(indices).ToList();
        for (int i = sortedIndices.Count - 1; i >= 0; i--)
        {
            polygons.RemoveAt(sortedIndices[i]);
        }
    }
}
