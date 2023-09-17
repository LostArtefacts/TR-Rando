using TRLevelControl.Model;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures;

public class TR2TextureRemapGroup : AbstractTextureRemapGroup<TR2Type, TR2Level>
{
    protected override IEnumerable<TR2Type> GetModelTypes(TR2Level level)
    {
        List<TR2Type> types = new();
        foreach (TRModel model in level.Models)
        {
            types.Add((TR2Type)model.ID);
        }
        return types;
    }

    protected override AbstractTexturePacker<TR2Type, TR2Level> CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }
}
