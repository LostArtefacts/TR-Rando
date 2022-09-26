using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMirrorStaticMeshFunction : BaseEMFunction
    {
        public uint[] MeshIDs { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            IEnumerable<TRMesh> meshes = level.StaticMeshes.ToList()
                .FindAll(s => MeshIDs.Contains(s.ID))
                .Select(s => TRMeshUtilities.GetMesh(level, s.Mesh));

            MirrorMeshes(meshes);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            IEnumerable<TRMesh> meshes = level.StaticMeshes.ToList()
                .FindAll(s => MeshIDs.Contains(s.ID))
                .Select(s => TRMeshUtilities.GetMesh(level, s.Mesh));

            MirrorMeshes(meshes);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            IEnumerable<TRMesh> meshes = level.StaticMeshes.ToList()
                .FindAll(s => MeshIDs.Contains(s.ID))
                .Select(s => TRMeshUtilities.GetMesh(level, s.Mesh));

            MirrorMeshes(meshes);
        }

        private void MirrorMeshes(IEnumerable<TRMesh> meshes)
        {
            foreach (TRMesh mesh in meshes)
            {
                foreach (TRVertex vert in mesh.Vertices)
                {
                    vert.X *= -1;
                }

                if (mesh.Normals != null)
                {
                    foreach (TRVertex norm in mesh.Normals)
                    {
                        norm.X *= -1;
                    }
                }

                foreach (TRFace4 f in mesh.TexturedRectangles)
                {
                    Swap(f.Vertices, 0, 3);
                    Swap(f.Vertices, 1, 2);
                }

                foreach (TRFace4 f in mesh.ColouredRectangles)
                {
                    Swap(f.Vertices, 0, 3);
                    Swap(f.Vertices, 1, 2);
                }

                foreach (TRFace3 f in mesh.TexturedTriangles)
                {
                    Swap(f.Vertices, 0, 2);
                }

                foreach (TRFace3 f in mesh.ColouredTriangles)
                {
                    Swap(f.Vertices, 0, 2);
                }
            }
        }

        private static void Swap<T>(T[] arr, int pos1, int pos2)
        {
            T temp = arr[pos1];
            arr[pos1] = arr[pos2];
            arr[pos2] = temp;
        }
    }
}