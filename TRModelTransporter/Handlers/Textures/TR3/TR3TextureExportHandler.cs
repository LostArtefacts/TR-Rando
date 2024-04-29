using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers;

public class TR3TextureExportHandler : AbstractTextureExportHandler<TR3Type, TR3Level, TR3ModelDefinition>
{
    protected override AbstractTexturePacker<TR3Type, TR3Level> CreatePacker()
    {
        return new TR3TexturePacker(_level, _classifier);
    }
    protected override TRSpriteSequence GetSprite(TR3Type entity)
    {
        return _level.Sprites[entity];
    }
}
