using TRImageControl.Packing;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers.Textures;

public class TR1TextureExportHandler : AbstractTextureExportHandler<TR1Type, TR1Level, TR1Blob>
{
    protected override TRTexturePacker<TR1Type, TR1Level> CreatePacker()
    {
        return new TR1TexturePacker(_level, _classifier);
    }

    protected override TRSpriteSequence GetSprite(TR1Type entity)
    {
        return _level.Sprites[entity];
    }
}
