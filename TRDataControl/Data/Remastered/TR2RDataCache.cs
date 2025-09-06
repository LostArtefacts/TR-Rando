using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR2RDataCache : BaseTRRDataCache<TR2Type, TR2RAlias>
{
    private TR2PDPControl _control;
    private TR2DataProvider _dataProvider;

    protected override TRPDPControlBase<TR2Type> GetPDPControl()
        => _control ??= new();

    public override TR2Type TranslateKey(TR2Type key)
        => key;

    public override TR2Type TranslateAlias(TR2Type key)
        => (_dataProvider ??= new()).TranslateAlias(key);

    public override string GetSourceLevel(TR2Type key)
        => _sourceLevels.ContainsKey(key) ? _sourceLevels[key] : null;

    public override TR2RAlias GetAlias(TR2Type key)
        => _mapAliases.ContainsKey(key) ? _mapAliases[key] : default;

    protected override Dictionary<TR2Type, TR2RAlias> GetMapDependencies(TR2Type key)
        => key == TR2Type.RedSnowmobile ? _skidooDependencies : null;

    private static readonly Dictionary<TR2Type, TR2RAlias> _skidooDependencies = new()
    {
        [TR2Type.CutsceneActor4] = TR2RAlias.SKIDOO_TRACK_1,
        [TR2Type.CutsceneActor5] = TR2RAlias.SKIDOO_TRACK_2,
        [TR2Type.CutsceneActor6] = TR2RAlias.SKIDOO_TRACK_3,
    };

    private static readonly Dictionary<TR2Type, string> _sourceLevels = new()
    {
        [TR2Type.Crow] = TR2LevelNames.GW,
        [TR2Type.Spider] = TR2LevelNames.GW,
        [TR2Type.BengalTiger] = TR2LevelNames.GW,
        [TR2Type.TRex] = TR2LevelNames.GW,
        [TR2Type.LaraMiscAnim_H_Wall] = TR2LevelNames.GW,

        [TR2Type.Doberman] = TR2LevelNames.VENICE,
        [TR2Type.MaskedGoon1] = TR2LevelNames.VENICE,
        [TR2Type.MaskedGoon2] = TR2LevelNames.VENICE,
        [TR2Type.MaskedGoon3] = TR2LevelNames.VENICE,
        [TR2Type.Rat] = TR2LevelNames.VENICE,
        [TR2Type.StickWieldingGoon1BodyWarmer] = TR2LevelNames.VENICE,

        [TR2Type.StickWieldingGoon1WhiteVest] = TR2LevelNames.BARTOLI,
        [TR2Type.StickWieldingGoon1GM] = TR2LevelNames.BARTOLI,

        [TR2Type.ShotgunGoon] = TR2LevelNames.OPERA,

        [TR2Type.Gunman1OG] = TR2LevelNames.RIG,
        [TR2Type.Gunman2] = TR2LevelNames.RIG,
        [TR2Type.ScubaDiver] = TR2LevelNames.RIG,
        [TR2Type.ScubaHarpoonProjectile_H] = TR2LevelNames.RIG,
        [TR2Type.StickWieldingGoon1Bandana] = TR2LevelNames.RIG,

        [TR2Type.FlamethrowerGoonOG] = TR2LevelNames.DA,

        [TR2Type.BarracudaUnwater] = TR2LevelNames.FATHOMS,
        [TR2Type.SharkOG] = TR2LevelNames.FATHOMS,
        [TR2Type.LaraMiscAnim_H_Unwater] = TR2LevelNames.FATHOMS,

        [TR2Type.StickWieldingGoon1GreenVest] = TR2LevelNames.DORIA,
        [TR2Type.YellowMorayEel] = TR2LevelNames.DORIA,

        [TR2Type.BlackMorayEel] = TR2LevelNames.LQ,
        [TR2Type.StickWieldingGoon2] = TR2LevelNames.LQ,

        [TR2Type.Eagle] = TR2LevelNames.TIBET,
        [TR2Type.Mercenary2OG] = TR2LevelNames.TIBET,
        [TR2Type.Mercenary3] = TR2LevelNames.TIBET,
        [TR2Type.MercSnowmobDriverOG] = TR2LevelNames.TIBET,
        [TR2Type.BlackSnowmobOG] = TR2LevelNames.TIBET,
        [TR2Type.RedSnowmobile] = TR2LevelNames.TIBET,
        [TR2Type.SnowmobileBelt] = TR2LevelNames.TIBET,
        [TR2Type.LaraSnowmobAnim_H] = TR2LevelNames.TIBET,
        [TR2Type.SnowLeopard] = TR2LevelNames.TIBET,

        [TR2Type.Mercenary1] = TR2LevelNames.MONASTERY,
        [TR2Type.MonkWithKnifeStickOG] = TR2LevelNames.MONASTERY,
        [TR2Type.MonkWithLongStick] = TR2LevelNames.MONASTERY,

        [TR2Type.BarracudaIce] = TR2LevelNames.COT,
        [TR2Type.YetiOG] = TR2LevelNames.COT,
        [TR2Type.LaraMiscAnim_H_Ice] = TR2LevelNames.COT,

        [TR2Type.BirdMonster] = TR2LevelNames.CHICKEN,
        [TR2Type.WhiteTiger] = TR2LevelNames.CHICKEN,

        [TR2Type.BarracudaXian] = TR2LevelNames.XIAN,
        [TR2Type.GiantSpider] = TR2LevelNames.XIAN,

        [TR2Type.Knifethrower] = TR2LevelNames.FLOATER,
        [TR2Type.KnifeProjectile_H] = TR2LevelNames.FLOATER,
        [TR2Type.XianGuardSword] = TR2LevelNames.FLOATER,
        [TR2Type.XianGuardSpear] = TR2LevelNames.FLOATER,
        [TR2Type.XianGuardSpearStatue] = TR2LevelNames.FLOATER,
        [TR2Type.XianGuardSwordStatue] = TR2LevelNames.FLOATER,

        [TR2Type.MarcoBartoli] = TR2LevelNames.LAIR,
        [TR2Type.DragonExplosionEmitter_N] = TR2LevelNames.LAIR,
        [TR2Type.DragonExplosion1_H] = TR2LevelNames.LAIR,
        [TR2Type.DragonExplosion2_H] = TR2LevelNames.LAIR,
        [TR2Type.DragonExplosion3_H] = TR2LevelNames.LAIR,
        [TR2Type.DragonFront_H] = TR2LevelNames.LAIR,
        [TR2Type.DragonBack_H] = TR2LevelNames.LAIR,
        [TR2Type.DragonBonesFront_H] = TR2LevelNames.LAIR,
        [TR2Type.DragonBonesBack_H] = TR2LevelNames.LAIR,
        [TR2Type.LaraMiscAnim_H_Xian] = TR2LevelNames.LAIR,
        [TR2Type.Puzzle2_M_H_Dagger] = TR2LevelNames.LAIR,

        [TR2Type.StickWieldingGoon1BlackJacket] = TR2LevelNames.HOME,

        [TR2Type.Winston] = TR2LevelNames.ASSAULT,
    };

    private static readonly Dictionary<TR2Type, TR2RAlias> _mapAliases = new()
    {
        [TR2Type.BengalTiger] = TR2RAlias.TIGER_EMPRTOMB_WALL,
        [TR2Type.StickWieldingGoon1BodyWarmer] = TR2RAlias.WORKER3_BOAT,
        [TR2Type.StickWieldingGoon1WhiteVest] = TR2RAlias.WORKER3_VENICE_OPERA,
        [TR2Type.ShotgunGoon] = TR2RAlias.CULT3_OPERA,
        [TR2Type.StickWieldingGoon1Bandana] = TR2RAlias.WORKER3_PLATFORM_RIG,
        [TR2Type.BarracudaUnwater] = TR2RAlias.BARACUDDA_DECK_LIVING_KEEL_UNWATER,
        [TR2Type.StickWieldingGoon1GreenVest] = TR2RAlias.WORKER3_DECK_LIVING_KEEL_UNWATER,
        [TR2Type.SnowLeopard] = TR2RAlias.TIGER_CATACOMB_SKIDOO,
        [TR2Type.BarracudaIce] = TR2RAlias.BARACUDDA_ICECAVE_CATACOMB,
        [TR2Type.WhiteTiger] = TR2RAlias.TIGER_ICECAVE,
        [TR2Type.BarracudaXian] = TR2RAlias.BARACUDDA_EMPRTOMB,
        [TR2Type.StickWieldingGoon1BlackJacket] = TR2RAlias.WORKER3_HOUSE,
        [TR2Type.StickWieldingGoon1GM] = TR2RAlias.WORKER3_BOAT,
    };
}
