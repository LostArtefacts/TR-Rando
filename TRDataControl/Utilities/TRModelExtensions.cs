using System.Security.Cryptography;
using System.Text;
using TRImageControl;
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
                new TR1TextureRemapper(level as TR1Level).ResetUnusedTextures();
                break;
            case TRGameVersion.TR2:
                new TR2TextureRemapper(level as TR2Level).ResetUnusedTextures();
                break;
            case TRGameVersion.TR3:
                new TR3TextureRemapper(level as TR3Level).ResetUnusedTextures();
                break;
            case TRGameVersion.TR4:
                new TR4TextureRemapper(level as TR4Level).ResetUnusedTextures();
                break;
            case TRGameVersion.TR5:
                new TR5TextureRemapper(level as TR5Level).ResetUnusedTextures();
                break;
        }
    }

    public static List<int> GetFreeTextureSlots(this TRLevelBase level)
    {
        List<int> slots = new();
        for (int i = 0; i < level.ObjectTextures.Count; i++)
        {
            if (!level.ObjectTextures[i].IsValid())
            {
                slots.Add(i);
            }
        }
        return slots;
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
