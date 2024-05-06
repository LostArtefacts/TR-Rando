using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR3DataExporter : TRDataExporter<TR3Level, TR3Type, TR3SFX, TR3Blob>
{
    public TR3DataExporter()
    {
        Data = new TR3DataProvider();
    }

    protected override TR3Blob CreateBlob(TR3Level level, TR3Type id, TRBlobType blobType)
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

    protected override TRTextureRemapper<TR3Level> CreateRemapper()
        => new TR3TextureRemapper();

    protected override bool IsMasterType(TR3Type type)
        => type == TR3Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR3Type.Lara].Meshes[0];

    protected override void StoreColour(ushort index, TR3Blob blob)
    {
        blob.Palette16[index] = Level.Palette16[index >> 8];
    }

    protected override void StoreSFX(TR3SFX sfx, TR3Blob blob)
    {
        if (Level.SoundEffects.ContainsKey(sfx))
        {
            blob.SoundEffects[sfx] = Level.SoundEffects[sfx];
        }
    }

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
}
