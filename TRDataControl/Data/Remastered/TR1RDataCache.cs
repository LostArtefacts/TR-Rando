using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR1RDataCache : BaseTRRDataCache<TR1Type, TR1RAlias>
{
    private TR1PDPControl _control;
    private TR1DataProvider _dataProvider;

    protected override TRPDPControlBase<TR1Type> GetPDPControl()
        => _control ??= new();

    public override TR1Type TranslateKey(TR1Type key)
        => TR1TypeUtilities.TranslateSourceType(key);

    public override string GetSourceLevel(TR1Type key)
    {
        _dataProvider ??= new();
        TR1Type translatedType = _dataProvider.TranslateAlias(key);
        return _sourceLevels.ContainsKey(translatedType) ? _sourceLevels[translatedType] : null;
    }

    public override TR1RAlias GetAlias(TR1Type key)
    {
        _dataProvider ??= new();
        TR1Type translatedType = _dataProvider.TranslateAlias(key);
        return _mapAliases.ContainsKey(translatedType) ? _mapAliases[translatedType] : default;
    }

    private static readonly Dictionary<TR1Type, string> _sourceLevels = new()
    {
        [TR1Type.SecretAnkh_M_H] = TR1LevelNames.OBELISK,
        [TR1Type.SecretGoldBar_M_H] = TR1LevelNames.MIDAS,
        [TR1Type.SecretGoldIdol_M_H] = TR1LevelNames.VILCABAMBA,
        [TR1Type.SecretLeadBar_M_H] = TR1LevelNames.MIDAS,
        [TR1Type.SecretScion_M_H] = TR1LevelNames.QUALOPEC,
    };

    private static readonly Dictionary<TR1Type, TR1RAlias> _mapAliases = new()
    {
        [TR1Type.SecretAnkh_M_H] = TR1RAlias.PUZZLE_OPTION4_8A_8B,
        [TR1Type.SecretGoldBar_M_H] = TR1RAlias.PUZZLE_OPTION1_6,
        [TR1Type.SecretGoldIdol_M_H] = TR1RAlias.PUZZLE_OPTION1_2,
        [TR1Type.SecretLeadBar_M_H] = TR1RAlias.LEADBAR_OPTION,
        [TR1Type.SecretScion_M_H] = TR1RAlias.SCION_OPTION,
    };
}
