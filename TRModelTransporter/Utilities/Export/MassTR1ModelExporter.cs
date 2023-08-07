using System.Collections.Generic;
using System.Linq;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities
{
    public class MassTR1ModelExporter : AbstractMassTRModelExporter<TREntities, TR1Level, TR1ModelDefinition>
    {
        private static readonly List<string> _sourceLevels = TR1LevelNames.AsListWithAssault.Concat(new List<string>
        {
            // https://trcustoms.org/users/854 by Leoc1995
            "LEOC.TR2"
        }).ToList();

        public override List<string> LevelNames => _sourceLevels;

        public override Dictionary<string, List<TREntities>> ExportTypes => _exportModelTypes;

        private readonly TR1LevelControl _reader;

        public MassTR1ModelExporter()
        {
            _reader = new();
        }

        protected override AbstractTRModelExporter<TREntities, TR1Level, TR1ModelDefinition> CreateExporter()
        {
            return new TR1ModelExporter();
        }

        protected override TR1Level ReadLevel(string path)
        {
            return _reader.Read(path);
        }

        private static readonly Dictionary<string, List<TREntities>> _exportModelTypes = new Dictionary<string, List<TREntities>>
        {
            [TR1LevelNames.CAVES] = new List<TREntities>
            {
                TREntities.Pistols_M_H, TREntities.Shotgun_M_H, TREntities.Magnums_M_H, TREntities.Uzis_M_H,
                TREntities.Lara, TREntities.Bat, TREntities.Bear, TREntities.Wolf,
                TREntities.FallingBlock, TREntities.DartEmitter, TREntities.WallSwitch, TREntities.LaraMiscAnim_H_General
            },
            [TR1LevelNames.VILCABAMBA] = new List<TREntities>
            {
                TREntities.PushBlock1, TREntities.SwingingBlade, TREntities.Trapdoor1, TREntities.UnderwaterSwitch
            },
            [TR1LevelNames.VALLEY] = new List<TREntities>
            {
                TREntities.TRex, TREntities.Raptor, TREntities.LaraPonytail_H_U
            },
            [TR1LevelNames.QUALOPEC] = new List<TREntities>
            {
                TREntities.Mummy, TREntities.RollingBall, TREntities.FallingCeiling1, TREntities.MovingBlock, TREntities.TeethSpikes
            },
            [TR1LevelNames.FOLLY] = new List<TREntities>
            {
                TREntities.CrocodileLand, TREntities.CrocodileWater, TREntities.Gorilla, TREntities.Lion, TREntities.Lioness,
                TREntities.ThorHammerHandle, TREntities.ThorLightning, TREntities.DamoclesSword
            },
            [TR1LevelNames.COLOSSEUM] = new List<TREntities>
            {
                
            },
            [TR1LevelNames.MIDAS] = new List<TREntities>
            {
                TREntities.PushBlock2
            },
            [TR1LevelNames.CISTERN] = new List<TREntities>
            {
                TREntities.RatLand, TREntities.RatWater
            },
            [TR1LevelNames.TIHOCAN] = new List<TREntities>
            {
                TREntities.CentaurStatue, TREntities.Centaur, TREntities.Pierre, TREntities.ScionPiece_M_H,
                TREntities.SlammingDoor
            },
            [TR1LevelNames.KHAMOON] = new List<TREntities>
            {
                TREntities.Panther
            },
            [TR1LevelNames.OBELISK] = new List<TREntities>
            {
                TREntities.BandagedAtlantean
            },
            [TR1LevelNames.SANCTUARY] = new List<TREntities>
            {
                TREntities.MeatyFlyer, TREntities.MeatyAtlantean, TREntities.ShootingAtlantean_N, TREntities.Larson
            },
            [TR1LevelNames.MINES] = new List<TREntities>
            {
                TREntities.CowboyOG, TREntities.Kold, TREntities.SkateboardKid
            },
            [TR1LevelNames.ATLANTIS] = new List<TREntities>
            {
                TREntities.Doppelganger, TREntities.AtlanteanEgg
            },
            [TR1LevelNames.PYRAMID] = new List<TREntities>
            {
                TREntities.Adam, TREntities.AdamEgg, TREntities.Natla
            },
            ["LEOC.PHD"] = new List<TREntities>
            {
                TREntities.CowboyHeadless
            }
        };
    }
}