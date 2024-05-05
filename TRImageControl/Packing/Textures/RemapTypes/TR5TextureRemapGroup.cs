using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR5TextureRemapGroup : AbstractTextureRemapGroup<TR5Type, TR5Level>
{
    protected override TRTexturePacker CreatePacker(TR5Level level)
        => new TR5TexturePacker(level, TRGroupPackingMode.All);

    protected override TRMesh GetDummyMesh(TR5Level level)
        => level.Models[TR5Type.Lara].Meshes[0];

    protected override TRDictionary<TR5Type, TRModel> GetModels(TR5Level level)
        => level.Models;

    protected override bool IsMasterType(TR5Type type)
        => type == TR5Type.Lara;
}
