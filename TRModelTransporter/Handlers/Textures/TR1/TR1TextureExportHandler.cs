using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers.Textures;

public class TR1TextureExportHandler : AbstractTextureExportHandler<TREntities, TR1Level, TR1ModelDefinition>
{
    protected override AbstractTexturePacker<TREntities, TR1Level> CreatePacker()
    {
        return new TR1TexturePacker(_level, _classifier);
    }

    protected override TRSpriteSequence GetSprite(TREntities entity)
    {
        return _level.SpriteSequences.ToList().Find(s => s.SpriteID == (int)entity);
    }
}
