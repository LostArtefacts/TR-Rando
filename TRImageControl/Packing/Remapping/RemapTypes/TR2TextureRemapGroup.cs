using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR2TextureRemapGroup : TRTextureRemapGroup<TR2Type, TR2Level>
{
    protected override TRTexturePacker CreatePacker(TR2Level level)
        => new TR2TexturePacker(level);

    protected override TRMesh GetDummyMesh(TR2Level level)
        => level.Models[TR2Type.Lara].Meshes[0];

    protected override TRDictionary<TR2Type, TRModel> GetModels(TR2Level level)
        => level.Models;

    protected override bool IsMasterType(TR2Type type)
        => type == TR2Type.Lara;
}
