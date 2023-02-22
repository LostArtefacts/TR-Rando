using System.Collections.Generic;
using System.Linq;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities
{
    public class MassTR1ModelExporter : AbstractMassTRModelExporter<TREntities, TRLevel, TR1ModelDefinition>
    {
        private static readonly List<string> _sourceLevels = TRLevelNames.AsListWithAssault.Concat(new List<string>
        {
            // https://trcustoms.org/users/854 by Leoc1995
            "LEOC.TR2"
        }).ToList();

        public override List<string> LevelNames => _sourceLevels;

        public override Dictionary<string, List<TREntities>> ExportTypes => _exportModelTypes;

        private readonly TR1LevelReader _reader;

        public MassTR1ModelExporter()
        {
            _reader = new TR1LevelReader();
        }

        protected override AbstractTRModelExporter<TREntities, TRLevel, TR1ModelDefinition> CreateExporter()
        {
            return new TR1ModelExporter();
        }

        protected override TRLevel ReadLevel(string path)
        {
            return _reader.ReadLevel(path);
        }

        private static readonly Dictionary<string, List<TREntities>> _exportModelTypes = new Dictionary<string, List<TREntities>>
        {
            [TRLevelNames.CAVES] = new List<TREntities>
            {
                TREntities.Pistols_M_H, TREntities.Shotgun_M_H, TREntities.Magnums_M_H, TREntities.Uzis_M_H,
                TREntities.Lara, TREntities.Bat, TREntities.Bear, TREntities.Wolf,
                TREntities.FallingBlock, TREntities.DartEmitter, TREntities.WallSwitch
            },
            [TRLevelNames.VILCABAMBA] = new List<TREntities>
            {
                TREntities.PushBlock1, TREntities.SwingingBlade, TREntities.Trapdoor1, TREntities.UnderwaterSwitch
            },
            [TRLevelNames.VALLEY] = new List<TREntities>
            {
                TREntities.TRex, TREntities.Raptor, TREntities.LaraPonytail_H_U
            },
            [TRLevelNames.QUALOPEC] = new List<TREntities>
            {
                TREntities.Mummy, TREntities.RollingBall, TREntities.FallingCeiling1, TREntities.MovingBlock, TREntities.TeethSpikes
            },
            [TRLevelNames.FOLLY] = new List<TREntities>
            {
                TREntities.CrocodileLand, TREntities.CrocodileWater, TREntities.Gorilla, TREntities.Lion, TREntities.Lioness,
                TREntities.ThorHammerHandle, TREntities.ThorLightning, TREntities.DamoclesSword
            },
            [TRLevelNames.COLOSSEUM] = new List<TREntities>
            {
                
            },
            [TRLevelNames.MIDAS] = new List<TREntities>
            {
                TREntities.PushBlock2
            },
            [TRLevelNames.CISTERN] = new List<TREntities>
            {
                TREntities.RatLand, TREntities.RatWater
            },
            [TRLevelNames.TIHOCAN] = new List<TREntities>
            {
                TREntities.CentaurStatue, TREntities.Centaur, TREntities.Pierre, TREntities.ScionPiece_M_H,
                TREntities.SlammingDoor
            },
            [TRLevelNames.KHAMOON] = new List<TREntities>
            {
                TREntities.Panther
            },
            [TRLevelNames.OBELISK] = new List<TREntities>
            {
                TREntities.BandagedAtlantean
            },
            [TRLevelNames.SANCTUARY] = new List<TREntities>
            {
                TREntities.MeatyFlyer, TREntities.MeatyAtlantean, TREntities.ShootingAtlantean_N, TREntities.Larson
            },
            [TRLevelNames.MINES] = new List<TREntities>
            {
                TREntities.CowboyOG, TREntities.Kold, TREntities.SkateboardKid
            },
            [TRLevelNames.ATLANTIS] = new List<TREntities>
            {
                TREntities.Doppelganger, TREntities.AtlanteanEgg
            },
            [TRLevelNames.PYRAMID] = new List<TREntities>
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