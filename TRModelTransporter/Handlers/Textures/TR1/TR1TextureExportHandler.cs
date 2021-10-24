using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers.Textures
{
    public class TR1TextureExportHandler : AbstractTextureExportHandler<TREntities, TRLevel, TR1ModelDefinition>
    {
        protected override AbstractTexturePacker<TREntities, TRLevel> CreatePacker()
        {
            return new TR1TexturePacker(_level, _classifier);
        }

        protected override TRSpriteSequence GetSprite(TREntities entity)
        {
            return _level.SpriteSequences.ToList().Find(s => s.SpriteID == (int)entity);
        }
    }
}