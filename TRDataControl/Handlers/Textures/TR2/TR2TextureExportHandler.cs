using TRImageControl.Packing;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class TR2TextureExportHandler : AbstractTextureExportHandler<TR2Type, TR2Level, TR2Blob>
{
    protected override AbstractTexturePacker<TR2Type, TR2Level> CreatePacker()
    {
        return new TR2TexturePacker(_level, _classifier);
    }

    protected override TRSpriteSequence GetSprite(TR2Type entity)
    {
        return _level.Sprites[entity];
    }
}
