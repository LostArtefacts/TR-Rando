using Newtonsoft.Json;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR5DataImporter : TRDataImporter<TR5Level, TR5Type, TR5SFX, TR5Blob>
{
    public TR5DataImporter()
    {
        Data = new TR5DataProvider();
    }

    protected override List<TR5Type> GetExistingTypes()
        => new(Level.Models.Keys.Concat(Level.Sprites.Keys));

    protected override TRTextureRemapper<TR5Level> CreateRemapper(TR5Level level)
        => new TR5TextureRemapper(level);

    protected override TRTextureRemapGroup<TR5Type, TR5Level> GetRemapGroup()
        => JsonConvert.DeserializeObject<TR5TextureRemapGroup>(File.ReadAllText(TextureRemapPath));

    protected override bool IsMasterType(TR5Type type)
        => type == TR5Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR5Type.Lara].Meshes[0];

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

    protected override ushort ImportColour(TR5Blob blob, ushort currentIndex)
        => 0;

    protected override void ImportSound(TR5Blob blob)
    {
        if (blob.SoundEffects == null)
            return;

        foreach (TR5SFX sfx in blob.SoundEffects.Keys)
        {
            if (!Level.SoundEffects.ContainsKey(sfx))
            {
                Level.SoundEffects[sfx] = blob.SoundEffects[sfx];
            }
        }
    }
}
