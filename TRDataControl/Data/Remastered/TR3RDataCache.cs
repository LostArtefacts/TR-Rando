using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR3RDataCache : BaseTRRDataCache<TR3Type, TR3RAlias>
{
    private TR3PDPControl _control;
    private TR3DataProvider _dataProvider;

    protected override TRPDPControlBase<TR3Type> GetPDPControl()
        => _control ??= new();

    public override TR3Type TranslateKey(TR3Type key)
    {
        return key switch
        {
            TR3Type.Quest1_P or TR3Type.Quest2_P => TR3Type.Puzzle1_P,
            TR3Type.Quest1_M_H or TR3Type.Quest2_M_H => TR3Type.Puzzle1_M_H,
            _ => key,
        };
    }

    public override string GetSourceLevel(TR3Type key)
    {
        _dataProvider ??= new();
        return _sourceLevels.ContainsKey(key) ? _sourceLevels[key] : null;
    }

    public override TR3RAlias GetAlias(TR3Type key)
    {
        _dataProvider ??= new();
        TR3Type translatedType = _dataProvider.TranslateAlias(key);
        return _mapAliases.ContainsKey(translatedType) ? _mapAliases[translatedType] : default;
    }

    private static readonly Dictionary<TR3Type, string> _sourceLevels = new()
    {
        [TR3Type.Infada_P] = TR3LevelNames.CAVES,
        [TR3Type.Infada_M_H] = TR3LevelNames.CAVES,
        [TR3Type.OraDagger_P] = TR3LevelNames.PUNA,
        [TR3Type.OraDagger_M_H] = TR3LevelNames.PUNA,
        [TR3Type.EyeOfIsis_P] = TR3LevelNames.CITY,
        [TR3Type.EyeOfIsis_M_H] = TR3LevelNames.CITY,
        [TR3Type.Element115_P] = TR3LevelNames.AREA51,
        [TR3Type.Element115_M_H] = TR3LevelNames.AREA51,
        [TR3Type.Quest1_P] = TR3LevelNames.COASTAL,
        [TR3Type.Quest1_M_H] = TR3LevelNames.COASTAL,
        [TR3Type.Quest2_P] = TR3LevelNames.MADHOUSE,
        [TR3Type.Quest2_M_H] = TR3LevelNames.MADHOUSE,
    };

    private static readonly Dictionary<TR3Type, TR3RAlias> _mapAliases = new()
    {
        [TR3Type.Infada_P] = TR3RAlias.ICON_PICKUP1_ITEM,
        [TR3Type.Infada_M_H] = TR3RAlias.ICON_PICKUP1_OPTION,
        [TR3Type.OraDagger_P] = TR3RAlias.ICON_PICKUP4_ITEM,
        [TR3Type.OraDagger_M_H] = TR3RAlias.ICON_PICKUP4_OPTION,
        [TR3Type.EyeOfIsis_P] = TR3RAlias.ICON_PICKUP3_ITEM,
        [TR3Type.EyeOfIsis_M_H] = TR3RAlias.ICON_PICKUP3_OPTION,
        [TR3Type.Element115_P] = TR3RAlias.ICON_PICKUP2_ITEM,
        [TR3Type.Element115_M_H] = TR3RAlias.ICON_PICKUP2_OPTION,
        [TR3Type.Quest1_P] = TR3RAlias.PUZZLE_ITEM1_SHORE,
        [TR3Type.Quest1_M_H] = TR3RAlias.PUZZLE_OPTION1_SHORE,
        [TR3Type.Quest2_P] = TR3RAlias.PUZZLE_ITEM1_HAND,
        [TR3Type.Quest2_M_H] = TR3RAlias.PUZZLE_OPTION1_HAND,
    };
}
