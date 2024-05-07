using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR3TextureRemapGroup : TRTextureRemapGroup<TR3Type, TR3Level>
{
    protected override TRTexturePacker CreatePacker(TR3Level level)
        => new TR3TexturePacker(level);

    protected override TRMesh GetDummyMesh(TR3Level level)
        => level.Models[TR3Type.Lara].Meshes[0];

    protected override TRDictionary<TR3Type, TRModel> GetModels(TR3Level level)
        => level.Models;

    protected override bool IsMasterType(TR3Type type)
        => type == TR3Type.Lara;
}
