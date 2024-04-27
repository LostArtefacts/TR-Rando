using TRLevelControl.Model;

namespace TRRandomizerCore.Helpers;

public class MeshEditor
{
    public TRMesh Mesh { get; set; }

    public void ClearAllPolygons()
    {
        ClearTexturedRectangles();
        ClearTexturedTriangles();
        ClearColouredRectangles();
        ClearColouredTriangles();
    }

    public void ClearTexturedRectangles()
    {
        Mesh.TexturedRectangles.Clear();
    }

    public void RemoveTexturedRectangleRange(int index, int count)
    {
        Mesh.TexturedRectangles.RemoveRange(index, count);
    }

    public void RemoveTexturedRectangles(IEnumerable<int> indices)
    {
        RemovePolygons(Mesh.TexturedRectangles, indices);
    }

    public void ClearColouredRectangles()
    {
        Mesh.ColouredRectangles.Clear();
    }

    public void RemoveColouredRectangleRange(int index, int count)
    {
        Mesh.ColouredRectangles.RemoveRange(index, count);
    }

    public void RemoveColouredRectangles(IEnumerable<int> indices)
    {
        RemovePolygons(Mesh.ColouredRectangles, indices);
    }

    public void ClearTexturedTriangles()
    {
        Mesh.TexturedTriangles.Clear();
    }

    public void RemoveTexturedTriangleRange(int index, int count)
    {
        Mesh.TexturedTriangles.RemoveRange(index, count);
    }

    public void RemoveTexturedTriangles(IEnumerable<int> indices)
    {
        RemovePolygons(Mesh.TexturedTriangles, indices);
    }

    public void ClearColouredTriangles()
    {
        Mesh.ColouredTriangles.Clear();
    }

    public void RemoveColouredTriangleRange(int index, int count)
    {
        Mesh.ColouredTriangles.RemoveRange(index, count);
    }

    public void RemoveColouredTriangles(IEnumerable<int> indices)
    {
        RemovePolygons(Mesh.ColouredTriangles, indices);
    }

    public void AddTexturedRectangle(TRMeshFace face)
    {
        Mesh.TexturedRectangles.Add(face);
    }

    public void AddTexturedTriangle(TRMeshFace face)
    {
        Mesh.TexturedTriangles.Add(face);
    }

    public void AddColouredRectangle(TRMeshFace face)
    {
        Mesh.ColouredRectangles.Add(face);
    }

    public void AddColouredTriangle(TRMeshFace face)
    {
        Mesh.ColouredTriangles.Add(face);
    }

    public static TRMesh CloneMesh(TRMesh mesh)
    {
        return mesh.Clone();
    }

    public static TRMesh CloneMeshAsColoured(TRMesh mesh, ushort paletteIndex)
    {
        TRMesh clone = CloneMesh(mesh);

        clone.ColouredRectangles.Clear();
        clone.ColouredTriangles.Clear();

        clone.ColouredRectangles.AddRange(clone.TexturedRectangles);
        clone.ColouredTriangles.AddRange(clone.TexturedTriangles);

        clone.TexturedRectangles.Clear();
        clone.TexturedTriangles.Clear();

        clone.ColouredRectangles.ForEach(f => f.Texture = paletteIndex);
        clone.ColouredTriangles.ForEach(f => f.Texture = paletteIndex);

        return clone;
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
