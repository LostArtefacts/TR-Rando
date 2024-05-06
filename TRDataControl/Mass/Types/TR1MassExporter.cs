using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities;

public class TR1MassExporter : TRMassExporter<TR1Type, TR1Level, TR1Blob>
{
    private static readonly List<string> _sourceLevels = TR1LevelNames.AsListWithAssault.Concat(new List<string>
    {
        // https://trcustoms.org/users/854 by Leoc1995
        "LEOC.TR2"
    }).ToList();

    public override List<string> LevelNames => _sourceLevels;

    public override Dictionary<string, List<TR1Type>> ExportTypes => _exportModelTypes;

    private readonly TR1LevelControl _reader;

    public TR1MassExporter()
    {
        _reader = new();
    }

    protected override TRDataExporter<TR1Type, TR1Level, TR1Blob> CreateExporter()
    {
        return new TR1DataExporter();
    }

    protected override TR1Level ReadLevel(string path)
    {
        return _reader.Read(path);
    }

    private static readonly Dictionary<string, List<TR1Type>> _exportModelTypes = new()
    {
        [TR1LevelNames.CAVES] = new List<TR1Type>
        {
            TR1Type.Pistols_M_H, TR1Type.Shotgun_M_H, TR1Type.Magnums_M_H, TR1Type.Uzis_M_H,
            TR1Type.Lara, TR1Type.Bat, TR1Type.Bear, TR1Type.Wolf,
            TR1Type.FallingBlock, TR1Type.DartEmitter, TR1Type.WallSwitch, TR1Type.LaraMiscAnim_H_General
        },
        [TR1LevelNames.VILCABAMBA] = new List<TR1Type>
        {
            TR1Type.PushBlock1, TR1Type.SwingingBlade, TR1Type.Trapdoor1, TR1Type.UnderwaterSwitch
        },
        [TR1LevelNames.VALLEY] = new List<TR1Type>
        {
            TR1Type.TRex, TR1Type.Raptor, TR1Type.LaraPonytail_H_U
        },
        [TR1LevelNames.QUALOPEC] = new List<TR1Type>
        {
            TR1Type.Mummy, TR1Type.RollingBall, TR1Type.FallingCeiling1, TR1Type.MovingBlock, TR1Type.TeethSpikes
        },
        [TR1LevelNames.FOLLY] = new List<TR1Type>
        {
            TR1Type.CrocodileLand, TR1Type.CrocodileWater, TR1Type.Gorilla, TR1Type.Lion, TR1Type.Lioness,
            TR1Type.ThorHammerHandle, TR1Type.ThorLightning, TR1Type.DamoclesSword
        },
        [TR1LevelNames.COLOSSEUM] = new List<TR1Type>
        {
            
        },
        [TR1LevelNames.MIDAS] = new List<TR1Type>
        {
            TR1Type.PushBlock2, TR1Type.Door7, TR1Type.FlameEmitter_N, TR1Type.MidasHand_N
        },
        [TR1LevelNames.CISTERN] = new List<TR1Type>
        {
            TR1Type.RatLand, TR1Type.RatWater
        },
        [TR1LevelNames.TIHOCAN] = new List<TR1Type>
        {
            TR1Type.CentaurStatue, TR1Type.Centaur, TR1Type.Pierre, TR1Type.ScionPiece_M_H,
            TR1Type.SlammingDoor
        },
        [TR1LevelNames.KHAMOON] = new List<TR1Type>
        {
            TR1Type.Panther
        },
        [TR1LevelNames.OBELISK] = new List<TR1Type>
        {
            TR1Type.BandagedAtlantean
        },
        [TR1LevelNames.SANCTUARY] = new List<TR1Type>
        {
            TR1Type.MeatyFlyer, TR1Type.MeatyAtlantean, TR1Type.ShootingAtlantean_N, TR1Type.Larson
        },
        [TR1LevelNames.MINES] = new List<TR1Type>
        {
            TR1Type.CowboyOG, TR1Type.Kold, TR1Type.SkateboardKid, TR1Type.LavaEmitter_N
        },
        [TR1LevelNames.ATLANTIS] = new List<TR1Type>
        {
            TR1Type.Doppelganger, TR1Type.AtlanteanEgg, TR1Type.AtlanteanLava
        },
        [TR1LevelNames.PYRAMID] = new List<TR1Type>
        {
            TR1Type.Adam, TR1Type.AdamEgg, TR1Type.Natla, TR1Type.Earthquake_N
        },
        ["LEOC.PHD"] = new List<TR1Type>
        {
            TR1Type.CowboyHeadless
        }
    };
}
