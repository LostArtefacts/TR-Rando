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

    protected override TRTextureRemapper<TR3Level> CreateRemapper(TR3Level level)
        => new TR3TextureRemapper(level);

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

    protected override void PreCreation(TR3Level level, TR3Type type, TRBlobType blobType)
    {
        switch (type)
        {
            case TR3Type.Quest1_P:
            case TR3Type.Quest1_M_H:
                level.Models.ChangeKey(TR3Type.Puzzle1_P, TR3Type.Quest1_P);
                level.Models.ChangeKey(TR3Type.Puzzle1_M_H, TR3Type.Quest1_M_H);
                break;
            case TR3Type.Quest2_P:
            case TR3Type.Quest2_M_H:
                level.Models.ChangeKey(TR3Type.Puzzle1_P, TR3Type.Quest2_P);
                level.Models.ChangeKey(TR3Type.Puzzle1_M_H, TR3Type.Quest2_M_H);
                break;
        }
    }
}
