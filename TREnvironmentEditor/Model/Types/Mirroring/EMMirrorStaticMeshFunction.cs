using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMirrorStaticMeshFunction : BaseEMFunction
{
    public uint[] MeshIDs { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        IEnumerable<TRMesh> meshes = level.StaticMeshes
            .Where(kvp => MeshIDs.Contains(kvp.Key - TR1Type.SceneryBase))
            .Select(kvp => kvp.Value.Mesh);

        MirrorMeshes(meshes);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        IEnumerable<TRMesh> meshes = level.StaticMeshes
            .Where(kvp => MeshIDs.Contains(kvp.Key - TR2Type.SceneryBase))
            .Select(kvp => kvp.Value.Mesh);

        MirrorMeshes(meshes);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        IEnumerable<TRMesh> meshes = level.StaticMeshes
            .Where(kvp => MeshIDs.Contains(kvp.Key - TR3Type.SceneryBase))
            .Select(kvp => kvp.Value.Mesh);

        MirrorMeshes(meshes);
    }

    private static void MirrorMeshes(IEnumerable<TRMesh> meshes)
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
