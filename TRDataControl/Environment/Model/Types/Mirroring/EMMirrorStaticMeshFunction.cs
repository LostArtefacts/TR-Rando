using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMirrorStaticMeshFunction : BaseEMFunction
{
    public uint[] MeshIDs { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        MirrorMeshes(level.StaticMeshes);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        MirrorMeshes(level.StaticMeshes);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        MirrorMeshes(level.StaticMeshes);
    }

    private void MirrorMeshes<T>(TRDictionary<T, TRStaticMesh> staticMeshes)
        where T : Enum
    {
        IEnumerable<TRMesh> meshes = staticMeshes
            .Where(kvp => MeshIDs.Contains((uint)(object)kvp.Key))
            .Select(kvp => kvp.Value.Mesh);

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

            foreach (TRMeshFace face in mesh.TexturedRectangles)
            {
                face.SwapVertices(0, 3);
                face.SwapVertices(1, 2);
            }

            foreach (TRMeshFace face in mesh.ColouredRectangles)
            {
                face.SwapVertices(0, 3);
                face.SwapVertices(1, 2);
            }

            foreach (TRMeshFace face in mesh.TexturedTriangles)
            {
                face.SwapVertices(0, 2);
            }

            foreach (TRMeshFace face in mesh.ColouredTriangles)
            {
                face.SwapVertices(0, 2);
            }
        }
    }
}
