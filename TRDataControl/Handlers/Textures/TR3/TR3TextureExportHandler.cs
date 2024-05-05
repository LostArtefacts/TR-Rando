using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class TR3TextureExportHandler : AbstractTextureExportHandler<TR3Type, TR3Level, TR3Blob>
{
    //protected override TRTexturePacker<TR3Type, TR3Level> CreatePacker()
    //{
    //    return new TR3TexturePacker(_level, _classifier);
    //}
    //protected override TRSpriteSequence GetSprite(TR3Type entity)
    //{
    //    return _level.Sprites[entity];
    //}
}
