using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMirrorModelFunction : BaseEMFunction
{
    public uint[] ModelIDs { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        IEnumerable<TRModel> models = level.Models
            .Where(kvp => ModelIDs.Contains((uint)kvp.Key) && kvp.Value.Meshes.Count == 1)
            .Select(kvp => kvp.Value);
        MirrorObjectTextures(MirrorMeshes(models), level.ObjectTextures);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        IEnumerable<TRModel> models = level.Models
            .Where(kvp => ModelIDs.Contains((uint)kvp.Key) && kvp.Value.Meshes.Count == 1)
            .Select(kvp => kvp.Value);
        MirrorObjectTextures(MirrorMeshes(models), level.ObjectTextures);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        IEnumerable<TRModel> models = level.Models
            .Where(kvp => ModelIDs.Contains((uint)kvp.Key) && kvp.Value.Meshes.Count == 1)
            .Select(kvp => kvp.Value);
        MirrorObjectTextures(MirrorMeshes(models), level.ObjectTextures);
    }

    private static ISet<ushort> MirrorMeshes(IEnumerable<TRModel> models)
    {
        ISet<ushort> textureReferences = new HashSet<ushort>();

        foreach (TRMesh mesh in models.SelectMany(m => m.Meshes))
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
                textureReferences.Add(face.Texture);
            }

            foreach (TRMeshFace face in mesh.ColouredRectangles)
            {
                face.SwapVertices(0, 3);
                face.SwapVertices(1, 2);
            }

            foreach (TRMeshFace face in mesh.TexturedTriangles)
            {
                face.SwapVertices(0, 2);
                textureReferences.Add(face.Texture);
            }

            foreach (TRMeshFace face in mesh.ColouredTriangles)
            {
                face.SwapVertices(0, 2);
            }
        }

        return textureReferences;
    }

    private static void MirrorObjectTextures(ISet<ushort> textureReferences, List<TRObjectTexture> objectTextures)
    {
        foreach (ushort textureRef in textureReferences)
        {
            objectTextures[textureRef].FlipVertical();
        }
    }
}
