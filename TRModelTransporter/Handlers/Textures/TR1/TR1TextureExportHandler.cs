using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers.Textures;

public class TR1TextureExportHandler : AbstractTextureExportHandler<TR1Type, TR1Level, TR1ModelDefinition>
{
    protected override AbstractTexturePacker<TR1Type, TR1Level> CreatePacker()
    {
        return new TR1TexturePacker(_level, _classifier);
    }

    protected override TRSpriteSequence GetSprite(TR1Type entity)
    {
        return _level.SpriteSequences.ToList().Find(s => s.SpriteID == (int)entity);
    }
}
