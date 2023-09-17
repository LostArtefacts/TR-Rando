using TRLevelControl.Model;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures;

public class TR3TextureRemapGroup : AbstractTextureRemapGroup<TR3Entities, TR3Level>
{
    protected override IEnumerable<TR3Entities> GetModelTypes(TR3Level level)
    {
        List<TR3Entities> types = new();
        foreach (TRModel model in level.Models)
        {
            types.Add((TR3Entities)model.ID);
        }
        return types;
    }

    protected override AbstractTexturePacker<TR3Entities, TR3Level> CreatePacker(TR3Level level)
    {
        return new TR3TexturePacker(level);
    }
}
