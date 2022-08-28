using System.Collections.Generic;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures
{
    public class TR1TextureRemapGroup : AbstractTextureRemapGroup<TREntities, TRLevel>
    {
        protected override IEnumerable<TREntities> GetModelTypes(TRLevel level)
        {
            List<TREntities> types = new List<TREntities>();
            foreach (TRModel model in level.Models)
            {
                types.Add((TREntities)model.ID);
            }
            return types;
        }

        protected override AbstractTexturePacker<TREntities, TRLevel> CreatePacker(TRLevel level)
        {
            return new TR1TexturePacker(level);
        }
    }
}