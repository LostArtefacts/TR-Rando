using TRLevelControl.Model;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures;

public class TR1TextureRemapGroup : AbstractTextureRemapGroup<TR1Type, TR1Level>
{
    protected override IEnumerable<TR1Type> GetModelTypes(TR1Level level)
    {
        return level.Models.Keys.ToList();
    }

    protected override AbstractTexturePacker<TR1Type, TR1Level> CreatePacker(TR1Level level)
    {
        return new TR1TexturePacker(level);
    }
}
