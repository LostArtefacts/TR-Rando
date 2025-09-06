using System.Diagnostics;
using System.Drawing;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR2DataExporter : TRDataExporter<TR2Level, TR2Type, TR2SFX, TR2Blob>
{
    public TR2DataExporter()
    {
        Data = new TR2DataProvider();
    }

    protected override TR2Blob CreateBlob(TR2Level level, TR2Type id, TRBlobType blobType)
    {
        return new()
        {
            Type = blobType,
            ID = Data.TranslateAlias(id),
            Alias = id,
            Palette16 = new(),
            SpriteOffsets = new(),
            SoundEffects = new()
        };
    }

    protected override TRTextureRemapper<TR2Level> CreateRemapper(TR2Level level)
        => new TR2TextureRemapper(level);

    protected override bool IsMasterType(TR2Type type)
        => type == TR2Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR2Type.Lara].Meshes[0];

    protected override string GetTypeName(TR2Type type)
        => TR2TypeUtilities.GetName(type);

    protected override void StoreColour(ushort index, TR2Blob blob)
    {
        blob.Palette16[index] = Level.Palette16[index >> 8];
    }

    protected override void StoreSFX(TR2SFX sfx, TR2Blob blob)
    {
        if (Level.SoundEffects.ContainsKey(sfx))
        {
            blob.SoundEffects[sfx] = Level.SoundEffects[sfx];
        }
    }

    protected override TRTexturePacker CreatePacker()
        => new TR2TexturePacker(Level, Data.TextureTileLimit);

    protected override TRDictionary<TR2Type, TRModel> Models
        => Level.Models;

    protected override TRDictionary<TR2Type, TRStaticMesh> StaticMeshes
        => Level.StaticMeshes;

    protected override TRDictionary<TR2Type, TRSpriteSequence> SpriteSequences
        => Level.Sprites;

    protected override List<TRCinematicFrame> CinematicFrames
        => Level.CinematicFrames;

    protected override void PostCreation(TR2Blob blob)
    {
        switch (blob.Alias)
        {
            case TR2Type.DragonExplosion1_H:
            case TR2Type.DragonExplosion2_H:
                ScaleSphereOfDoom(blob);
                break;

        }
    }

    private static void ScaleSphereOfDoom(TR2Blob blob)
    {
        // Clip the Sphere of Doom texture for a better chance of
        // importing into levels later.
        Debug.Assert(blob.Textures.Count == 1);
        Debug.Assert(blob.Textures[0].Width == 128);
        Debug.Assert(blob.Textures[0].Height == 128);

        TRTextileRegion region = blob.Textures[0];
        Rectangle clip = new(32, 32, 64, 64);

        region.Image = region.Image.Export(clip);
        region.Bounds = new(0, 0, clip.Width, clip.Height);
        region.GenerateID();
        region.Segments.ForEach(s => s.Texture.Size = clip.Size);
    }
}
