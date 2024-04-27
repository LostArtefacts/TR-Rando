using TRLevelControl.Model;
using TRModelTransporter.Model.Textures;

namespace TREnvironmentEditor.Model.Types;

public class EMMirrorModelFunction : BaseEMFunction
{
    public uint[] ModelIDs { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        IEnumerable<TRModel> meshes = level.Models
            .Where(m => ModelIDs.Contains(m.ID) && m.Meshes.Count == 1);
        MirrorObjectTextures(MirrorMeshes(meshes), level.ObjectTextures);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        IEnumerable<TRModel> meshes = level.Models
            .Where(m => ModelIDs.Contains(m.ID) && m.Meshes.Count == 1);
        MirrorObjectTextures(MirrorMeshes(meshes), level.ObjectTextures);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        IEnumerable<TRModel> meshes = level.Models
            .Where(m => ModelIDs.Contains(m.ID) && m.Meshes.Count == 1);
        MirrorObjectTextures(MirrorMeshes(meshes), level.ObjectTextures);
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
                textureReferences.Add((ushort)(face.Texture & 0x0fff));
            }

            foreach (TRMeshFace face in mesh.ColouredRectangles)
            {
                face.SwapVertices(0, 3);
                face.SwapVertices(1, 2);
            }

            foreach (TRMeshFace face in mesh.TexturedTriangles)
            {
                face.SwapVertices(0, 2);
                textureReferences.Add((ushort)(face.Texture & 0x0fff));
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
            IndexedTRObjectTexture texture = new()
            {
                Texture = objectTextures[textureRef]
            };

            if (texture.IsTriangle)
            {
                Swap(texture.Texture.Vertices, 0, 2);
            }
            else
            {
                Swap(texture.Texture.Vertices, 0, 3);
                Swap(texture.Texture.Vertices, 1, 2);
            }
        }
    }

    private static void Swap<T>(T[] arr, int pos1, int pos2)
    {
        (arr[pos2], arr[pos1]) = (arr[pos1], arr[pos2]);
    }
}
