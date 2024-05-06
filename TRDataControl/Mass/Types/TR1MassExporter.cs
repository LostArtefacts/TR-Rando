using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl.Utils;

public class TR1MassExporter : TRMassExporter<TR1Level, TR1Type, TR1SFX, TR1Blob>
{
    public override Dictionary<string, List<TR1Type>> Data => _data;

    protected override TRDataExporter<TR1Level, TR1Type, TR1SFX, TR1Blob> CreateExporter()
        => new TR1DataExporter();

    protected override TR1Level ReadLevel(string path)
        => new TR1LevelControl().Read(path);

    private static readonly Dictionary<string, List<TR1Type>> _data = new()
    {
        [TR1LevelNames.CAVES] = new()
        {
            TR1Type.Pistols_M_H, TR1Type.Shotgun_M_H, TR1Type.Magnums_M_H, TR1Type.Uzis_M_H,
            TR1Type.Lara, TR1Type.Bat, TR1Type.Bear, TR1Type.Wolf,
            TR1Type.FallingBlock, TR1Type.DartEmitter, TR1Type.WallSwitch, TR1Type.LaraMiscAnim_H_General
        },
        [TR1LevelNames.VILCABAMBA] = new()
        {
            TR1Type.PushBlock1, TR1Type.SwingingBlade, TR1Type.Trapdoor1, TR1Type.UnderwaterSwitch
        },
        [TR1LevelNames.VALLEY] = new()
        {
            TR1Type.TRex, TR1Type.Raptor, TR1Type.LaraPonytail_H_U
        },
        [TR1LevelNames.QUALOPEC] = new()
        {
            TR1Type.Mummy, TR1Type.RollingBall, TR1Type.FallingCeiling1, TR1Type.MovingBlock, TR1Type.TeethSpikes
        },
        [TR1LevelNames.FOLLY] = new()
        {
            TR1Type.CrocodileLand, TR1Type.CrocodileWater, TR1Type.Gorilla, TR1Type.Lion, TR1Type.Lioness,
            TR1Type.ThorHammerHandle, TR1Type.ThorLightning, TR1Type.DamoclesSword
        },
        [TR1LevelNames.MIDAS] = new()
        {
            TR1Type.PushBlock2, TR1Type.Door7, TR1Type.FlameEmitter_N, TR1Type.MidasHand_N
        },
        [TR1LevelNames.CISTERN] = new()
        {
            TR1Type.RatLand, TR1Type.RatWater
        },
        [TR1LevelNames.KHAMOON] = new()
        {
            TR1Type.Panther
        },
        [TR1LevelNames.OBELISK] = new()
        {
            TR1Type.BandagedAtlantean
        },
        [TR1LevelNames.SANCTUARY] = new()
        {
            TR1Type.MeatyFlyer, TR1Type.MeatyAtlantean, TR1Type.ShootingAtlantean_N, TR1Type.Larson
        },
        [TR1LevelNames.TIHOCAN] = new()
        {
            // Off sequence so we get the normal meatballs from Sanctuary first
            TR1Type.CentaurStatue, TR1Type.Centaur, TR1Type.Pierre, TR1Type.ScionPiece_M_H,
            TR1Type.SlammingDoor
        },
        [TR1LevelNames.MINES] = new()
        {
            TR1Type.CowboyOG, TR1Type.Kold, TR1Type.SkateboardKid, TR1Type.LavaEmitter_N
        },
        [TR1LevelNames.ATLANTIS] = new()
        {
            TR1Type.Doppelganger, TR1Type.AtlanteanEgg, TR1Type.AtlanteanLava
        },
        [TR1LevelNames.PYRAMID] = new()
        {
            TR1Type.Adam, TR1Type.AdamEgg, TR1Type.Natla, TR1Type.Earthquake_N
        },
        ["LEOC.PHD"] = new()
        {
            TR1Type.CowboyHeadless
        }
    };
}
