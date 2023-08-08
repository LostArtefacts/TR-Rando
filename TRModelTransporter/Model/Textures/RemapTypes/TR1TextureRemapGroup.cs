using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures;

public class TR1TextureRemapGroup : AbstractTextureRemapGroup<TREntities, TR1Level>
{
    protected override IEnumerable<TREntities> GetModelTypes(TR1Level level)
    {
        List<TREntities> types = new();
        foreach (TRModel model in level.Models)
        {
            types.Add((TREntities)model.ID);
        }
        return types;
    }

    protected override AbstractTexturePacker<TREntities, TR1Level> CreatePacker(TR1Level level)
    {
        return new TR1TexturePacker(level);
    }
}
