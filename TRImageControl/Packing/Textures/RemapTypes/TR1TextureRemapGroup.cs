using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR1TextureRemapGroup : AbstractTextureRemapGroup<TR1Type, TR1Level>
{
    protected override TRTexturePacker CreatePacker(TR1Level level)
        => new TR1TexturePacker(level);

    protected override TRMesh GetDummyMesh(TR1Level level)
        => level.Models[TR1Type.Lara].Meshes[0];

    protected override TRDictionary<TR1Type, TRModel> GetModels(TR1Level level)
        => level.Models;

    protected override bool IsMasterType(TR1Type type)
        => type == TR1Type.Lara;
}
