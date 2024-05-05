using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR4TextureRemapGroup : AbstractTextureRemapGroup<TR4Type, TR4Level>
{
    protected override TRTexturePacker CreatePacker(TR4Level level)
        => new TR4TexturePacker(level, TRGroupPackingMode.All);

    protected override TRMesh GetDummyMesh(TR4Level level)
        => level.Models[TR4Type.Lara].Meshes[0];

    protected override TRDictionary<TR4Type, TRModel> GetModels(TR4Level level)
        => level.Models;

    protected override bool IsMasterType(TR4Type type)
        => type == TR4Type.Lara;
}
