using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities;

public class MassTR2ModelExporter : AbstractMassTRModelExporter<TR2Type, TR2Level, TR2ModelDefinition>
{
    private static readonly List<string> _sourceLevels = TR2LevelNames.AsListWithAssault.Concat(new List<string>
    {
        // https://trcustoms.org/levels/3013 by Topixtor
        "TOPIORC.TR2",
        "TOPICAC.TR2",
    }).ToList();

    public override List<string> LevelNames => _sourceLevels;

    public override Dictionary<string, List<TR2Type>> ExportTypes => _exportModelTypes;

    private readonly TR2LevelControl _reader;

    public MassTR2ModelExporter()
    {
        _reader = new();
    }

    protected override AbstractTRModelExporter<TR2Type, TR2Level, TR2ModelDefinition> CreateExporter()
    {
        return new TR2ModelExporter();
    }

    protected override TR2Level ReadLevel(string path)
    {
        return _reader.Read(path);
    }

    private static readonly Dictionary<string, List<TR2Type>> _exportModelTypes = new()
    {
        [TR2LevelNames.GW] = new List<TR2Type>
        {
            TR2Type.Pistols_M_H, TR2Type.Shotgun_M_H, TR2Type.Uzi_M_H, TR2Type.Autos_M_H, TR2Type.Harpoon_M_H, TR2Type.M16_M_H, TR2Type.GrenadeLauncher_M_H,
            TR2Type.LaraSun, TR2Type.Crow, TR2Type.Spider, TR2Type.BengalTiger, TR2Type.TRex,
            TR2Type.RollingBall
        },
        [TR2LevelNames.VENICE] = new List<TR2Type>
        {
            TR2Type.Boat, TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.Rat, TR2Type.StickWieldingGoon1BodyWarmer
        },
        [TR2LevelNames.BARTOLI] = new List<TR2Type>
        {
            TR2Type.StickWieldingGoon1WhiteVest
        },
        [TR2LevelNames.OPERA] = new List<TR2Type>
        {
            TR2Type.ShotgunGoon
        },
        [TR2LevelNames.RIG] = new List<TR2Type>
        {
            TR2Type.Gunman1OG, TR2Type.Gunman2, TR2Type.ScubaDiver, TR2Type.StickWieldingGoon1Bandana
        },
        [TR2LevelNames.DA] = new List<TR2Type>
        {
            TR2Type.FlamethrowerGoonOG
        },
        [TR2LevelNames.FATHOMS] = new List<TR2Type>
        {
            TR2Type.LaraUnwater, TR2Type.BarracudaUnwater, TR2Type.Shark
        },
        [TR2LevelNames.DORIA] = new List<TR2Type>
        {
            TR2Type.StickWieldingGoon1GreenVest, TR2Type.YellowMorayEel
        },
        [TR2LevelNames.LQ] = new List<TR2Type>
        {
            TR2Type.BlackMorayEel, TR2Type.StickWieldingGoon2
        },
        [TR2LevelNames.TIBET] = new List<TR2Type>
        {
            TR2Type.LaraSnow, TR2Type.Eagle, TR2Type.Mercenary2, TR2Type.Mercenary3, TR2Type.MercSnowmobDriver, TR2Type.SnowLeopard
        },
        [TR2LevelNames.MONASTERY] = new List<TR2Type>
        {
            TR2Type.Mercenary1, TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick
        },
        [TR2LevelNames.COT] = new List<TR2Type>
        {
            TR2Type.BarracudaIce, TR2Type.Yeti
        },
        [TR2LevelNames.CHICKEN] = new List<TR2Type>
        {
            TR2Type.BirdMonster, TR2Type.WhiteTiger
        },
        [TR2LevelNames.XIAN] = new List<TR2Type>
        {
            TR2Type.BarracudaXian, TR2Type.GiantSpider
        },
        [TR2LevelNames.FLOATER] = new List<TR2Type>
        {
            TR2Type.Knifethrower, TR2Type.XianGuardSword, TR2Type.XianGuardSpear
        },
        [TR2LevelNames.LAIR] = new List<TR2Type>
        {
            TR2Type.MarcoBartoli
        },
        [TR2LevelNames.HOME] = new List<TR2Type>
        {
            TR2Type.LaraHome, TR2Type.StickWieldingGoon1BlackJacket
        },
        [TR2LevelNames.ASSAULT] = new List<TR2Type>
        {
            TR2Type.Winston
        },
        ["TOPIORC.TR2"] = new List<TR2Type>
        {
            TR2Type.FlamethrowerGoonTopixtor, TR2Type.Gunman1TopixtorORC
        },
        ["TOPICAC.TR2"] = new List<TR2Type>
        {
            TR2Type.Gunman1TopixtorCAC
        }
    };
}
