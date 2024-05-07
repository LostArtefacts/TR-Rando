using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR5DataExporter : TRDataExporter<TR5Level, TR5Type, TR5SFX, TR5Blob>
{
    public TR5DataExporter()
    {
        Data = new TR5DataProvider();
    }

    protected override TR5Blob CreateBlob(TR5Level level, TR5Type id, TRBlobType blobType)
    {
        return new()
        {
            Type = blobType,
            ID = Data.TranslateAlias(id),
            Alias = id,
            SpriteOffsets = new(),
            SoundEffects = new()
        };
    }

    protected override TRTextureRemapper<TR5Level> CreateRemapper(TR5Level level)
        => new TR5TextureRemapper(level);

    protected override bool IsMasterType(TR5Type type)
        => type == TR5Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR5Type.Lara].Meshes[0];

    protected override void StoreColour(ushort index, TR5Blob blob) { }

    protected override void StoreSFX(TR5SFX sfx, TR5Blob blob)
    {
        if (Level.SoundEffects.ContainsKey(sfx))
        {
            blob.SoundEffects[sfx] = Level.SoundEffects[sfx];
        }
    }

    protected override TRTexturePacker CreatePacker()
        => new TR5TexturePacker(Level, TRGroupPackingMode.Object, Data.TextureTileLimit);

    protected override TRDictionary<TR5Type, TRModel> Models
        => Level.Models;

    protected override TRDictionary<TR5Type, TRStaticMesh> StaticMeshes
        => Level.StaticMeshes;

    protected override TRDictionary<TR5Type, TRSpriteSequence> SpriteSequences
        => Level.Sprites;

    protected override List<TRCinematicFrame> CinematicFrames
        => null;
}
