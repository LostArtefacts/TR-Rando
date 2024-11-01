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

    public override TR3Type TranslateAlias(TR3Type key)
        => (_dataProvider ??= new()).TranslateAlias(key);

    public override string GetSourceLevel(TR3Type key)
        => _sourceLevels.ContainsKey(key) ? _sourceLevels[key] : null;

    public override TR3RAlias GetAlias(TR3Type key)
        => _mapAliases.ContainsKey(key) ? _mapAliases[key] : default;

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

        [TR3Type.Monkey] = TR3LevelNames.JUNGLE,
        [TR3Type.MonkeyMedMeshswap] = TR3LevelNames.JUNGLE,
        [TR3Type.MonkeyKeyMeshswap] = TR3LevelNames.JUNGLE,
        [TR3Type.Tiger] = TR3LevelNames.JUNGLE,

        [TR3Type.Shiva] = TR3LevelNames.RUINS,
        [TR3Type.ShivaStatue] = TR3LevelNames.RUINS,
        [TR3Type.LaraExtraAnimation_H] = TR3LevelNames.RUINS,
        [TR3Type.CobraIndia] = TR3LevelNames.RUINS,

        [TR3Type.Quad] = TR3LevelNames.GANGES,
        [TR3Type.LaraVehicleAnimation_H_Quad] = TR3LevelNames.GANGES,
        [TR3Type.Vulture] = TR3LevelNames.GANGES,

        [TR3Type.TonyFirehands] = TR3LevelNames.CAVES,

        [TR3Type.Croc] = TR3LevelNames.COASTAL,
        [TR3Type.TribesmanAxe] = TR3LevelNames.COASTAL,
        [TR3Type.TribesmanDart] = TR3LevelNames.COASTAL,

        [TR3Type.Compsognathus] = TR3LevelNames.CRASH,
        [TR3Type.Mercenary] = TR3LevelNames.CRASH,
        [TR3Type.Raptor] = TR3LevelNames.CRASH,
        [TR3Type.Tyrannosaur] = TR3LevelNames.CRASH,

        [TR3Type.Kayak] = TR3LevelNames.MADUBU,
        [TR3Type.LaraVehicleAnimation_H_Kayak] = TR3LevelNames.MADUBU,
        [TR3Type.LizardMan] = TR3LevelNames.MADUBU,

        [TR3Type.Puna] = TR3LevelNames.PUNA,

        [TR3Type.Crow] = TR3LevelNames.THAMES,
        [TR3Type.LondonGuard] = TR3LevelNames.THAMES,
        [TR3Type.LondonMerc] = TR3LevelNames.THAMES,
        [TR3Type.Rat] = TR3LevelNames.THAMES,

        [TR3Type.Punk] = TR3LevelNames.ALDWYCH,
        [TR3Type.DogLondon] = TR3LevelNames.ALDWYCH,

        [TR3Type.ScubaSteve] = TR3LevelNames.LUDS,
        [TR3Type.UPV] = TR3LevelNames.LUDS,
        [TR3Type.LaraVehicleAnimation_H_UPV] = TR3LevelNames.LUDS,

        [TR3Type.SophiaLee] = TR3LevelNames.CITY,

        [TR3Type.DamGuard] = TR3LevelNames.NEVADA,
        [TR3Type.CobraNevada] = TR3LevelNames.NEVADA,

        [TR3Type.MPWithStick] = TR3LevelNames.HSC,
        [TR3Type.MPWithGun] = TR3LevelNames.HSC,
        [TR3Type.Prisoner] = TR3LevelNames.HSC,
        [TR3Type.DogNevada] = TR3LevelNames.HSC,

        [TR3Type.KillerWhale] = TR3LevelNames.AREA51,
        [TR3Type.MPWithMP5] = TR3LevelNames.AREA51,

        [TR3Type.CrawlerMutantInCloset] = TR3LevelNames.ANTARC,
        [TR3Type.Boat] = TR3LevelNames.ANTARC,
        [TR3Type.LaraVehicleAnimation_H_Boat] = TR3LevelNames.ANTARC,
        [TR3Type.RXRedBoi] = TR3LevelNames.ANTARC,
        [TR3Type.DogAntarc] = TR3LevelNames.ANTARC,

        [TR3Type.Crawler] = TR3LevelNames.RXTECH,
        [TR3Type.RXTechFlameLad] = TR3LevelNames.RXTECH,
        [TR3Type.BruteMutant] = TR3LevelNames.RXTECH,

        [TR3Type.TinnosMonster] = TR3LevelNames.TINNOS,
        [TR3Type.TinnosWasp] = TR3LevelNames.TINNOS,

        [TR3Type.Willie] = TR3LevelNames.WILLIE,
        [TR3Type.AIPath_N] = TR3LevelNames.WILLIE,
        [TR3Type.RXGunLad] = TR3LevelNames.WILLIE,

        [TR3Type.Winston] = TR3LevelNames.ASSAULT,
        [TR3Type.WinstonInCamoSuit] = TR3LevelNames.ASSAULT,
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

        [TR3Type.DogLondon] = TR3RAlias.DOG_SEWER,        
        [TR3Type.CobraNevada] = TR3RAlias.COBRA_NEVADA,        
        [TR3Type.CobraIndia] = TR3RAlias.COBRA,
    };
}
