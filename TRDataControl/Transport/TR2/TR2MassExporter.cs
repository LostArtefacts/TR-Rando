using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR2MassExporter : TRMassExporter<TR2Level, TR2Type, TR2SFX, TR2Blob>
{
    public override Dictionary<string, List<TR2Type>> Data => _data;

    protected override TRDataExporter<TR2Level, TR2Type, TR2SFX, TR2Blob> CreateExporter()
        => new TR2DataExporter();

    protected override TR2Level ReadLevel(string path)
        => new TR2LevelControl().Read(path);

    private static readonly Dictionary<string, List<TR2Type>> _data = new()
    {
        [TR2LevelNames.GW] = new()
        {
            TR2Type.Pistols_M_H, TR2Type.Shotgun_M_H, TR2Type.Uzi_M_H, TR2Type.Autos_M_H, TR2Type.Harpoon_M_H, TR2Type.M16_M_H, TR2Type.GrenadeLauncher_M_H,
            TR2Type.LaraSun, TR2Type.Crow, TR2Type.Spider, TR2Type.BengalTiger, TR2Type.TRex,
            TR2Type.RollingBall, TR2Type.PushBlock1,
        },
        [TR2LevelNames.VENICE] = new()
        {
            TR2Type.Boat, TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.Rat, TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.Trapdoor1,
        },
        [TR2LevelNames.BARTOLI] = new()
        {
            TR2Type.StickWieldingGoon1WhiteVest, TR2Type.Key1_M_H, TR2Type.LargeMed_M_H, TR2Type.SmallMed_M_H
        },
        [TR2LevelNames.OPERA] = new()
        {
            TR2Type.ShotgunGoon
        },
        [TR2LevelNames.RIG] = new()
        {
            TR2Type.Gunman1OG, TR2Type.Gunman2, TR2Type.ScubaDiver, TR2Type.StickWieldingGoon1Bandana
        },
        [TR2LevelNames.DA] = new()
        {
            TR2Type.FlamethrowerGoonOG
        },
        [TR2LevelNames.FATHOMS] = new()
        {
            TR2Type.LaraUnwater, TR2Type.BarracudaUnwater, TR2Type.SharkOG
        },
        [TR2LevelNames.DORIA] = new()
        {
            TR2Type.StickWieldingGoon1GreenVest, TR2Type.YellowMorayEel
        },
        [TR2LevelNames.LQ] = new()
        {
            TR2Type.BlackMorayEel, TR2Type.StickWieldingGoon2
        },
        [TR2LevelNames.TIBET] = new()
        {
            TR2Type.LaraSnow, TR2Type.Eagle, TR2Type.Mercenary2OG, TR2Type.Mercenary3OG, TR2Type.MercSnowmobDriver, TR2Type.SnowLeopard
        },
        [TR2LevelNames.MONASTERY] = new()
        {
            TR2Type.Mercenary1, TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick
        },
        [TR2LevelNames.COT] = new()
        {
            TR2Type.BarracudaIce, TR2Type.Yeti
        },
        [TR2LevelNames.CHICKEN] = new()
        {
            TR2Type.BirdMonster, TR2Type.WhiteTiger, TR2Type.TibetanBell,
        },
        [TR2LevelNames.XIAN] = new()
        {
            TR2Type.BarracudaXian, TR2Type.GiantSpider
        },
        [TR2LevelNames.FLOATER] = new()
        {
            TR2Type.Knifethrower, TR2Type.XianGuardSword, TR2Type.XianGuardSpear
        },
        [TR2LevelNames.LAIR] = new()
        {
            TR2Type.MarcoBartoli
        },
        [TR2LevelNames.HOME] = new()
        {
            TR2Type.LaraHome, TR2Type.StickWieldingGoon1BlackJacket
        },
        [TR2LevelNames.ASSAULT] = new()
        {
            TR2Type.Winston, TR2Type.LaraAssault
        },
        ["TOPIORC.TR2"] = new()
        {
            TR2Type.FlamethrowerGoonTopixtor, TR2Type.Gunman1TopixtorORC
        },
        ["TOPICAC.TR2"] = new()
        {
            TR2Type.Gunman1TopixtorCAC
        },
        [TR2LevelNames.COLDWAR] = [TR2Type.MonkWithNoShadow, TR2Type.SharkGM, TR2Type.Mercenary2GM, TR2Type.Mercenary3GM],
        [TR2LevelNames.FOOLGOLD] = [TR2Type.FlamethrowerGoonGM, TR2Type.Gunman1GM],
        [TR2LevelNames.FURNACE] = [TR2Type.Wolf, TR2Type.Bear],
    };
}
