using System.Collections.Generic;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures
{
    public class TR2TextureRemapGroup : AbstractTextureRemapGroup<TR2Entities, TR2Level>
    {
        protected override IEnumerable<TR2Entities> GetModelTypes(TR2Level level)
        {
            List<TR2Entities> types = new List<TR2Entities>();
            foreach (TRModel model in level.Models)
            {
                types.Add((TR2Entities)model.ID);
            }
            return types;
        }

        protected override AbstractTexturePacker<TR2Entities, TR2Level> CreatePacker(TR2Level level)
        {
            return new TR2TexturePacker(level);
        }
    }
}