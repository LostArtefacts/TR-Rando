using System.Security.Cryptography;
using System.Text;
using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRDataControl;

public static class TRModelExtensions
{
    public static void ResetUnusedTextures(this TRLevelBase level)
    {
        switch (level.Version.Game)
        {
            case TRGameVersion.TR1:
                new TR1TextureRemapper().ResetUnusedTextures(level as TR1Level);
                break;
            case TRGameVersion.TR2:
                new TR2TextureRemapper().ResetUnusedTextures(level as TR2Level);
                break;
            case TRGameVersion.TR3:
                new TR3TextureRemapper().ResetUnusedTextures(level as TR3Level);
                break;
            case TRGameVersion.TR4:
                new TR4TextureRemapper().ResetUnusedTextures(level as TR4Level);
                break;
            case TRGameVersion.TR5:
                new TR5TextureRemapper().ResetUnusedTextures(level as TR5Level);
                break;
        }
    }

    public static string ComputeSkeletonHash(this IEnumerable<TRMesh> meshes)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        using MD5 md5 = MD5.Create();

        // We only care about the structure, so reset all texture references
        // as there is no guarantee these will match across levels.
        TRObjectMeshBuilder<TR1Type> builder = new(TRGameVersion.TR1);
        foreach (TRMesh mesh in meshes)
        {
            TRMesh clone = mesh.Clone();
            clone.TexturedRectangles.ForEach(t => t.Texture = 0);
            clone.TexturedTriangles.ForEach(t => t.Texture = 0);
            clone.ColouredRectangles?.ForEach(t => t.Texture = 0);
            clone.ColouredTriangles?.ForEach(t => t.Texture = 0);
            writer.Write(builder.Serialize(clone));
        }

        byte[] hash = md5.ComputeHash(ms.ToArray());
        StringBuilder sb = new();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }

        return sb.ToString();
    }
}
