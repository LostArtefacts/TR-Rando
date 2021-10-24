using System.Collections.Generic;
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
        public override List<string> LevelNames => TRLevelNames.AsList;

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
                TREntities.Lara, TREntities.Bat, TREntities.Bear, TREntities.Wolf
            },
            [TRLevelNames.VILCABAMBA] = new List<TREntities>
            {
                
            },
            [TRLevelNames.VALLEY] = new List<TREntities>
            {
                TREntities.TRex, TREntities.Raptor
            },
            [TRLevelNames.QUALOPEC] = new List<TREntities>
            {
                TREntities.Larson, TREntities.ScionPiece_M_H
            },
            [TRLevelNames.FOLLY] = new List<TREntities>
            {
                TREntities.CrocodileWater, TREntities.Gorilla, TREntities.LionFemale, TREntities.LionMale, TREntities.Pierre
            },
            [TRLevelNames.COLOSSEUM] = new List<TREntities>
            {
                TREntities.CrocodileLand
            },
            [TRLevelNames.MIDAS] = new List<TREntities>
            {
                TREntities.RatWater
            },
            [TRLevelNames.CISTERN] = new List<TREntities>
            {
                TREntities.RatLand
            },
            [TRLevelNames.TIHOCAN] = new List<TREntities>
            {
                TREntities.CentaurStatue
            },
            [TRLevelNames.KHAMOON] = new List<TREntities>
            {
                TREntities.NonShootingAtlantean_N, TREntities.Panther
            },
            [TRLevelNames.OBELISK] = new List<TREntities>
            {
                
            },
            [TRLevelNames.SANCTUARY] = new List<TREntities>
            {
                TREntities.FlyingAtlantean, TREntities.ShootingAtlantean_N
            },
            [TRLevelNames.MINES] = new List<TREntities>
            {
                TREntities.Cowboy, TREntities.Kold, TREntities.SkateboardKid
            },
            [TRLevelNames.ATLANTIS] = new List<TREntities>
            {
                TREntities.Doppelganger, TREntities.AdamEgg, TREntities.AtlanteanEgg
            },
            [TRLevelNames.PYRAMID] = new List<TREntities>
            {
                TREntities.Natla
            }
        };
    }
}