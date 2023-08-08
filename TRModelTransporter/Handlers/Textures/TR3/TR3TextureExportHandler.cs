using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers;

public class TR3TextureExportHandler : AbstractTextureExportHandler<TR3Entities, TR3Level, TR3ModelDefinition>
{
    protected override AbstractTexturePacker<TR3Entities, TR3Level> CreatePacker()
    {
        return new TR3TexturePacker(_level, _classifier);
    }
    protected override TRSpriteSequence GetSprite(TR3Entities entity)
    {
        return _level.SpriteSequences.ToList().Find(s => s.SpriteID == (int)entity);
    }
}
