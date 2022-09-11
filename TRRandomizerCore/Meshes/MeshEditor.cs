using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TRRandomizerCore.Helpers
{
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
            Mesh.TexturedRectangles = new TRFace4[] { };
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
            Mesh.ColouredRectangles = new TRFace4[] { };
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
            Mesh.TexturedTriangles = new TRFace3[] { };
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
            Mesh.ColouredTriangles = new TRFace3[] { };
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

        public TRMesh CloneMesh(TRMesh mesh)
        {
            TRMesh clone = new TRMesh
            {
                Centre = mesh.Centre,
                CollRadius = mesh.CollRadius,
                ColouredRectangles = new TRFace4[mesh.NumColouredRectangles],
                ColouredTriangles = new TRFace3[mesh.NumColouredTriangles],
                Lights = mesh.Lights,
                Normals = mesh.Normals,
                NumColouredRectangles = mesh.NumColouredRectangles,
                NumColouredTriangles = mesh.NumColouredTriangles,
                NumNormals = mesh.NumNormals,
                NumTexturedRectangles = mesh.NumTexturedRectangles,
                NumTexturedTriangles = mesh.NumTexturedTriangles,
                NumVertices = mesh.NumVertices,
                Pointer = mesh.Pointer,
                TexturedRectangles = new TRFace4[mesh.NumTexturedRectangles],
                TexturedTriangles = new TRFace3[mesh.NumTexturedTriangles],
                Vertices = new TRVertex[mesh.NumVertices]
            };

            for (int i = 0; i < mesh.NumColouredRectangles; i++)
            {
                clone.ColouredRectangles[i] = new TRFace4
                {
                    Texture = mesh.ColouredRectangles[i].Texture,
                    Vertices = mesh.ColouredRectangles[i].Vertices
                };
            }

            for (int i = 0; i < mesh.NumColouredTriangles; i++)
            {
                clone.ColouredTriangles[i] = new TRFace3
                {
                    Texture = mesh.ColouredTriangles[i].Texture,
                    Vertices = mesh.ColouredTriangles[i].Vertices
                };
            }

            for (int i = 0; i < mesh.NumTexturedRectangles; i++)
            {
                clone.TexturedRectangles[i] = new TRFace4
                {
                    Texture = mesh.TexturedRectangles[i].Texture,
                    Vertices = mesh.TexturedRectangles[i].Vertices
                };
            }

            for (int i = 0; i < mesh.NumTexturedTriangles; i++)
            {
                clone.TexturedTriangles[i] = new TRFace3
                {
                    Texture = mesh.TexturedTriangles[i].Texture,
                    Vertices = mesh.TexturedTriangles[i].Vertices
                };
            }

            for (int i = 0; i < mesh.NumVertices; i++)
            {
                clone.Vertices[i] = new TRVertex
                {
                    X = mesh.Vertices[i].X,
                    Y = mesh.Vertices[i].Y,
                    Z = mesh.Vertices[i].Z
                };
            }

            return clone;
        }

        public void WriteToLevel(TRLevel level)
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
}