using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR3TextureRemapGroup : AbstractTextureRemapGroup<TR3Type, TR3Level>
{
    protected override IEnumerable<TR3Type> GetModelTypes(TR3Level level)
    {
        return level.Models.Keys.ToList();
    }

    protected override AbstractTexturePacker<TR3Type, TR3Level> CreatePacker(TR3Level level)
    {
        return new TR3TexturePacker(level);
    }
}
