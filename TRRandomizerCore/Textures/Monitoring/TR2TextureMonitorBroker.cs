using TRImageControl.Textures;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Textures;

public class TR2TextureMonitorBroker : AbstractTextureMonitorBroker<TR2Type>
{
    private static readonly Dictionary<TR2Type, TR2Type> _expandedMonitorMap = new()
    {
        [TR2Type.MercSnowmobDriver] = TR2Type.RedSnowmobile,
        [TR2Type.FlamethrowerGoonOG] = TR2Type.Flame_S_H,
        [TR2Type.FlamethrowerGoonTopixtor] = TR2Type.Flame_S_H,
        [TR2Type.FlamethrowerGoonGM] = TR2Type.Flame_S_H,
        [TR2Type.MarcoBartoli] = TR2Type.Flame_S_H
    };

    protected override Dictionary<TR2Type, TR2Type> ExpandedMonitorMap => _expandedMonitorMap;

    protected override TextureDatabase<TR2Type> CreateDatabase()
    {
        return new TR2TextureDatabase();
    }

    protected override TR2Type TranslateAlias(string lvlName, TR2Type type)
    {
        return TR2TypeUtilities.GetAliasForLevel(lvlName, type);
    }
}
