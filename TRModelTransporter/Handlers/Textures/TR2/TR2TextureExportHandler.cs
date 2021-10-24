using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers
{
    public class TR2TextureExportHandler : AbstractTextureExportHandler<TR2Entities, TR2Level, TR2ModelDefinition>
    {
        protected override AbstractTexturePacker<TR2Entities, TR2Level> CreatePacker()
        {
            return new TR2TexturePacker(_level, _classifier);
        }

        protected override TRSpriteSequence GetSprite(TR2Entities entity)
        {
            return _level.SpriteSequences.ToList().Find(s => s.SpriteID == (int)entity);
        }
    }
}