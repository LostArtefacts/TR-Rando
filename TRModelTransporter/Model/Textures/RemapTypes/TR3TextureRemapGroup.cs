using TRLevelControl.Model;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures;

public class TR3TextureRemapGroup : AbstractTextureRemapGroup<TR3Type, TR3Level>
{
    protected override IEnumerable<TR3Type> GetModelTypes(TR3Level level)
    {
        List<TR3Type> types = new();
        foreach (TRModel model in level.Models)
        {
            types.Add((TR3Type)model.ID);
        }
        return types;
    }

    protected override AbstractTexturePacker<TR3Type, TR3Level> CreatePacker(TR3Level level)
    {
        return new TR3TexturePacker(level);
    }
}
