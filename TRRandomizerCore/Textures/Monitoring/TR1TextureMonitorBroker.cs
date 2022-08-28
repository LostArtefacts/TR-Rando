using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public class TR1TextureMonitorBroker : AbstractTextureMonitorBroker<TREntities>
    {
        private static readonly Dictionary<TREntities, TREntities> _expandedMonitorMap = new Dictionary<TREntities, TREntities>
        {
            [TREntities.TRex] = TREntities.LaraMiscAnim_H_Valley
        };

        protected override Dictionary<TREntities, TREntities> ExpandedMonitorMap => _expandedMonitorMap;

        protected override TextureDatabase<TREntities> CreateDatabase()
        {
            return new TR1TextureDatabase();
        }

        protected override TREntities TranslateAlias(string lvlName, TREntities entity)
        {
            return TR1EntityUtilities.GetAliasForLevel(lvlName, entity);
        }
    }
}