using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR1RDataCache : BaseTRRDataCache<TR1Type, TR1RAlias>
{
    private TR1PDPControl _control;

    protected override TRPDPControlBase<TR1Type> GetPDPControl()
        => _control ??= new();

    public override TR1Type TranslateKey(TR1Type key)
    {
        return key switch
        {
            TR1Type.SecretAnkh_M_H => TR1Type.Puzzle4_M_H,
            TR1Type.SecretGoldBar_M_H or TR1Type.SecretGoldIdol_M_H => TR1Type.Puzzle1_M_H,
            TR1Type.SecretLeadBar_M_H => TR1Type.LeadBar_M_H,
            TR1Type.SecretScion_M_H => TR1Type.ScionPiece_M_H,
            _ => key,
        };
    }

    public override string GetSourceLevel(TR1Type key)
        => _sourceLevels.ContainsKey(key) ? _sourceLevels[key] : null;

    public override TR1RAlias GetAlias(TR1Type key)
        => _mapAliases.ContainsKey(key) ? _mapAliases[key] : default;

    private static readonly Dictionary<TR1Type, string> _sourceLevels = new()
    {
        [TR1Type.SecretAnkh_M_H] = TR1LevelNames.OBELISK,
        [TR1Type.SecretGoldBar_M_H] = TR1LevelNames.MIDAS,
        [TR1Type.SecretGoldIdol_M_H] = TR1LevelNames.VILCABAMBA,
        [TR1Type.SecretLeadBar_M_H] = TR1LevelNames.MIDAS,
        [TR1Type.SecretScion_M_H] = TR1LevelNames.QUALOPEC,

        [TR1Type.Bat] = TR1LevelNames.CAVES,
        [TR1Type.Bear] = TR1LevelNames.CAVES,
        [TR1Type.Wolf] = TR1LevelNames.CAVES,

        [TR1Type.Raptor] = TR1LevelNames.VALLEY,
        [TR1Type.TRex] = TR1LevelNames.VALLEY,
        [TR1Type.LaraMiscAnim_H_Valley] = TR1LevelNames.VALLEY,

        [TR1Type.CrocodileLand] = TR1LevelNames.FOLLY,
        [TR1Type.CrocodileWater] = TR1LevelNames.FOLLY,
        [TR1Type.Gorilla] = TR1LevelNames.FOLLY,
        [TR1Type.Lion] = TR1LevelNames.FOLLY,
        [TR1Type.Lioness] = TR1LevelNames.FOLLY,

        [TR1Type.RatLand] = TR1LevelNames.CISTERN,
        [TR1Type.RatWater] = TR1LevelNames.CISTERN,

        [TR1Type.Centaur] = TR1LevelNames.TIHOCAN,
        [TR1Type.CentaurStatue] = TR1LevelNames.TIHOCAN,
        [TR1Type.Pierre] = TR1LevelNames.TIHOCAN,
        [TR1Type.ScionPiece_M_H] = TR1LevelNames.TIHOCAN,
        [TR1Type.Key1_M_H] = TR1LevelNames.TIHOCAN,

        [TR1Type.Panther] = TR1LevelNames.KHAMOON,
        [TR1Type.NonShootingAtlantean_N] = TR1LevelNames.KHAMOON,

        [TR1Type.BandagedAtlantean] = TR1LevelNames.OBELISK,
        [TR1Type.BandagedFlyer] = TR1LevelNames.OBELISK,
        [TR1Type.Missile2_H] = TR1LevelNames.OBELISK,

        [TR1Type.Missile3_H] = TR1LevelNames.SANCTUARY,
        [TR1Type.MeatyAtlantean] = TR1LevelNames.SANCTUARY,
        [TR1Type.MeatyFlyer] = TR1LevelNames.SANCTUARY,
        [TR1Type.ShootingAtlantean_N] = TR1LevelNames.SANCTUARY,
        [TR1Type.Larson] = TR1LevelNames.SANCTUARY,

        [TR1Type.CowboyOG] = TR1LevelNames.MINES,
        [TR1Type.CowboyHeadless] = TR1LevelNames.MINES,
        [TR1Type.SkateboardKid] = TR1LevelNames.MINES,
        [TR1Type.Skateboard] = TR1LevelNames.MINES,
        [TR1Type.Kold] = TR1LevelNames.MINES,

        [TR1Type.AtlanteanEgg] = TR1LevelNames.ATLANTIS,

        [TR1Type.Adam] = TR1LevelNames.PYRAMID,
        [TR1Type.AdamEgg] = TR1LevelNames.PYRAMID,
        [TR1Type.Natla] = TR1LevelNames.PYRAMID,
        [TR1Type.LaraMiscAnim_H_Pyramid] = TR1LevelNames.PYRAMID,
    };

    private static readonly Dictionary<TR1Type, TR1RAlias> _mapAliases = new()
    {
        [TR1Type.SecretAnkh_M_H] = TR1RAlias.PUZZLE_OPTION4_8A_8B,
        [TR1Type.SecretGoldBar_M_H] = TR1RAlias.PUZZLE_OPTION1_6,
        [TR1Type.SecretGoldIdol_M_H] = TR1RAlias.PUZZLE_OPTION1_2,
        [TR1Type.SecretLeadBar_M_H] = TR1RAlias.LEADBAR_OPTION,
        [TR1Type.SecretScion_M_H] = TR1RAlias.SCION_OPTION,

        [TR1Type.Natla] = TR1RAlias.NATLA_MUTANT,
    };
}
