using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public class TR2TextureMonitorBroker : AbstractTextureMonitorBroker<TR2Entities>
    {
        private static readonly Dictionary<TR2Entities, TR2Entities> _expandedMonitorMap = new Dictionary<TR2Entities, TR2Entities>
        {
            [TR2Entities.MercSnowmobDriver] = TR2Entities.RedSnowmobile,
            [TR2Entities.FlamethrowerGoon] = TR2Entities.Flame_S_H,
            [TR2Entities.MarcoBartoli] = TR2Entities.Flame_S_H
        };

        protected override Dictionary<TR2Entities, TR2Entities> ExpandedMonitorMap => _expandedMonitorMap;

        protected override TextureDatabase<TR2Entities> CreateDatabase()
        {
            return new TR2TextureDatabase();
        }

        protected override TR2Entities TranslateAlias(string lvlName, TR2Entities entity)
        {
            return TR2EntityUtilities.GetAliasForLevel(lvlName, entity);
        }
    }
}