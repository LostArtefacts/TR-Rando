using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR1DataImporter : TRDataImporter<TR1Level, TR1Type, TR1SFX, TR1Blob>
{
    private TRPalette8Control _paletteControl;

    public TR1DataImporter(bool isCommunityPatch = false)
    {
        Data = new TR1DataProvider();
        if (isCommunityPatch)
        {
            Data.TextureTileLimit = 128;
            Data.TextureObjectLimit = 8192;
        }
    }

    protected override List<TR1Type> GetExistingTypes()
    {
        if (Level.Sprites[TR1Type.Explosion1_S_H]?.Textures.Count == 1)
        {
            // Allow replacing the Explosion sequence in Vilcabamba (it's there but empty, originally dynamite?)
            Level.Sprites.Remove(TR1Type.Explosion1_S_H);
        }
        return new(Level.Models.Keys.Concat(Level.Sprites.Keys));
    }

    protected override TRTextureRemapper<TR1Level> CreateRemapper()
        => new TR1TextureRemapper();

    protected override bool IsMasterType(TR1Type type)
        => type == TR1Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR1Type.Lara].Meshes[0];

    protected override TRTexturePacker CreatePacker()
        => new TR1TexturePacker(Level, Data.TextureTileLimit)
        {
            PaletteControl = _paletteControl = new()
            {
                Level = Level,
                ObsoleteTypes = new(TypesToRemove.Select(t => Data.TranslateAlias(t)))
            }
        };

    protected override TRDictionary<TR1Type, TRModel> Models
        => Level.Models;

    protected override TRDictionary<TR1Type, TRStaticMesh> StaticMeshes
        => Level.StaticMeshes;

    protected override TRDictionary<TR1Type, TRSpriteSequence> SpriteSequences
        => Level.Sprites;

    protected override List<TRCinematicFrame> CinematicFrames
        => Level.CinematicFrames;

    protected override List<TRObjectTexture> ObjectTextures
        => Level.ObjectTextures;

    protected override ushort ImportColour(TR1Blob blob, ushort currentIndex)
    {
        return blob.Palette8.ContainsKey(currentIndex)
            ? (ushort)_paletteControl.GetOrAddPaletteIndex(blob.Palette8[currentIndex])
            : currentIndex;
    }

    protected override void ImportSound(TR1Blob blob)
    {
        if (blob.SoundEffects == null)
            return;

        foreach (TR1SFX sfx in blob.SoundEffects.Keys)
        {
            if (!Level.SoundEffects.ContainsKey(sfx))
            {
                Level.SoundEffects[sfx] = blob.SoundEffects[sfx];
            }
        }
    }
}
