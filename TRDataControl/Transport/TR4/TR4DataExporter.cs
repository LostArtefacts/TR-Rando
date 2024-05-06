using TRDataControl.Remapping;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR4DataExporter : TRDataExporter<TR4Level, TR4Type, TR4SFX, TR4Blob>
{
    public TR4DataExporter()
    {
        Data = new TR4DataProvider();
    }

    protected override TR4Blob CreateBlob(TR4Level level, TR4Type id, TRBlobType blobType)
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

    protected override TRTextureRemapper<TR4Level> CreateRemapper()
        => new TR4TextureRemapper();

    protected override bool IsMasterType(TR4Type type)
        => type == TR4Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR4Type.Lara].Meshes[0];

    protected override void StoreColour(ushort index, TR4Blob blob) { }

    protected override void StoreSFX(TR4SFX sfx, TR4Blob blob)
    {
        if (Level.SoundEffects.ContainsKey(sfx))
        {
            blob.SoundEffects[sfx] = Level.SoundEffects[sfx];
        }
    }

    protected override TRTexturePacker CreatePacker()
        => new TR4TexturePacker(Level, TRGroupPackingMode.Object, Data.TextureTileLimit);

    protected override TRDictionary<TR4Type, TRModel> Models
        => Level.Models;

    protected override TRDictionary<TR4Type, TRStaticMesh> StaticMeshes
        => Level.StaticMeshes;

    protected override TRDictionary<TR4Type, TRSpriteSequence> SpriteSequences
        => Level.Sprites;

    protected override List<TRCinematicFrame> CinematicFrames
        => null;
}
