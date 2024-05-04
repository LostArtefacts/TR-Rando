using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRModelTransporter.Model.Textures;

public class TR2TextureRemapGroup : AbstractTextureRemapGroup<TR2Type, TR2Level>
{
    protected override IEnumerable<TR2Type> GetModelTypes(TR2Level level)
    {
        return level.Models.Keys.ToList();
    }

    protected override AbstractTexturePacker<TR2Type, TR2Level> CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }
}
