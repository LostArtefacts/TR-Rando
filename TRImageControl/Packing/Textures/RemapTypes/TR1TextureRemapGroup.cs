using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR1TextureRemapGroup : AbstractTextureRemapGroup<TR1Type, TR1Level>
{
    protected override IEnumerable<TR1Type> GetModelTypes(TR1Level level)
    {
        return level.Models.Keys.ToList();
    }

    protected override TRTexturePacker<TR1Type, TR1Level> CreatePacker(TR1Level level)
    {
        return new TR1TexturePacker(level);
    }
}
