using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public class TR3TextureMonitorBroker : AbstractTextureMonitorBroker<TR3Entities>
    {
        protected override Dictionary<TR3Entities, TR3Entities> ExpandedMonitorMap => null;

        protected override TextureDatabase<TR3Entities> CreateDatabase()
        {
            return new TR3TextureDatabase();
        }

        protected override TR3Entities TranslateAlias(string lvlName, TR3Entities entity)
        {
            return TR3EntityUtilities.GetAliasForLevel(lvlName, entity);
        }
    }
}