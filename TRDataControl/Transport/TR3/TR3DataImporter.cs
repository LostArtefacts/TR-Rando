using TRDataControl.Remapping;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR3DataImporter : TRDataImporter<TR3Level, TR3Type, TR3SFX, TR3Blob>
{
    private TRPalette16Control _paletteControl;

    public TR3DataImporter(bool isCommunityPatch = false)
    {
        Data = new TR3DataProvider();
        if (isCommunityPatch)
        {
            Data.TextureTileLimit = 128;
            Data.TextureObjectLimit = 16384;
        }
    }

    protected override List<TR3Type> GetExistingTypes()
        => new(Level.Models.Keys.Concat(Level.Sprites.Keys));

    protected override TRTextureRemapper<TR3Level> CreateRemapper()
        => new TR3TextureRemapper();

    protected override bool IsMasterType(TR3Type type)
        => type == TR3Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR3Type.Lara].Meshes[0];

    protected override TRTexturePacker CreatePacker()
        => new TR3TexturePacker(Level, Data.TextureTileLimit);

    protected override TRDictionary<TR3Type, TRModel> Models
        => Level.Models;

    protected override TRDictionary<TR3Type, TRStaticMesh> StaticMeshes
        => Level.StaticMeshes;

    protected override TRDictionary<TR3Type, TRSpriteSequence> SpriteSequences
        => Level.Sprites;

    protected override List<TRCinematicFrame> CinematicFrames
        => Level.CinematicFrames;

    protected override List<TRObjectTexture> ObjectTextures
        => Level.ObjectTextures;

    protected override ushort ImportColour(TR3Blob blob, ushort currentIndex)
    {
        if (!blob.Palette16.ContainsKey(currentIndex))
        {
            return currentIndex;
        }

        _paletteControl ??= new(Level.Palette16, Level.DistinctMeshes);
        return (ushort)_paletteControl.Import(blob.Palette16[currentIndex]);
    }

    protected override void ImportSound(TR3Blob blob)
    {
        if (blob.SoundEffects == null)
            return;

        foreach (TR3SFX sfx in blob.SoundEffects.Keys)
        {
            if (!Level.SoundEffects.ContainsKey(sfx))
            {
                Level.SoundEffects[sfx] = blob.SoundEffects[sfx];
            }
        }
    }
}
