using TRDataControl.Remapping;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR4DataImporter : TRDataImporter<TR4Level, TR4Type, TR4SFX, TR4Blob>
{
    public TR4DataImporter()
    {
        Data = new TR4DataProvider();
    }

    protected override List<TR4Type> GetExistingTypes()
        => new(Level.Models.Keys.Concat(Level.Sprites.Keys));

    protected override TRTextureRemapper<TR4Level> CreateRemapper()
        => new TR4TextureRemapper();

    protected override bool IsMasterType(TR4Type type)
        => type == TR4Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR4Type.Lara].Meshes[0];

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

    protected override List<TRObjectTexture> ObjectTextures
        => Level.ObjectTextures;

    protected override ushort ImportColour(TR4Blob blob, ushort currentIndex)
        => 0;

    protected override void ImportSound(TR4Blob blob)
    {
        if (blob.SoundEffects == null)
            return;

        foreach (TR4SFX sfx in blob.SoundEffects.Keys)
        {
            if (!Level.SoundEffects.ContainsKey(sfx))
            {
                Level.SoundEffects[sfx] = blob.SoundEffects[sfx];
            }
        }
    }
}
