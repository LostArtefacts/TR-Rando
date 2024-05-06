using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR2DataImporter : TRDataImporter<TR2Level, TR2Type, TR2SFX, TR2Blob>
{
    private TRPalette16Control _paletteControl;

    public TR2DataImporter(bool isCommunityPatch = false)
    {
        Data = new TR2DataProvider();
        if (isCommunityPatch)
        {
            Data.TextureTileLimit = 32;
            Data.TextureObjectLimit = 8192;
        }
    }

    protected override List<TR2Type> GetExistingTypes()
        => new(Level.Models.Keys.Concat(Level.Sprites.Keys));

    protected override TRTextureRemapper<TR2Level> CreateRemapper()
        => new TR2TextureRemapper();

    protected override bool IsMasterType(TR2Type type)
        => type == TR2Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR2Type.Lara].Meshes[0];

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

    protected override List<TRObjectTexture> ObjectTextures
        => Level.ObjectTextures;

    protected override ushort ImportColour(TR2Blob blob, ushort currentIndex)
    {
        if (!blob.Palette16.ContainsKey(currentIndex))
        {
            return currentIndex;
        }

        _paletteControl ??= new(Level.Palette16, Level.DistinctMeshes);
        return (ushort)_paletteControl.Import(blob.Palette16[currentIndex]);
    }

    protected override void ImportSound(TR2Blob blob)
    {
        if (blob.SoundEffects == null)
            return;

        foreach (TR2SFX sfx in blob.SoundEffects.Keys)
        {
            if (!Level.SoundEffects.ContainsKey(sfx))
            {
                Level.SoundEffects[sfx] = blob.SoundEffects[sfx];
            }
        }
    }

    protected override void BlobImported(TR2Blob blob)
    {
        switch (blob.ID)
        {
            case TR2Type.FlamethrowerGoon:
            case TR2Type.MarcoBartoli:
                AddFlameSprites();
                break;
        }
    }

    private void AddFlameSprites()
    {
        if (!Level.Sprites.ContainsKey(TR2Type.FireBlast_S_H) && Level.Sprites.ContainsKey(TR2Type.Explosion_S_H))
        {
            Level.Sprites[TR2Type.FireBlast_S_H] = Level.Sprites[TR2Type.Explosion_S_H].Clone();
        }
    }
}
