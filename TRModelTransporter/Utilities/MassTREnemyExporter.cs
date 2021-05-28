using System.Collections.Generic;
using System.IO;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities
{
    public class MassTREnemyExporter
    {
        private readonly TRModelExporter _exporter;
        private readonly List<TR2Entities> _processedEntities;
        private readonly TR2LevelReader _reader;

        public MassTREnemyExporter(bool exportIndividualSegments)
        {
            _exporter = new TRModelExporter
            {
                ExportIndividualSegments = exportIndividualSegments
            };
            _processedEntities = new List<TR2Entities>();
            _reader = new TR2LevelReader();
        }

        public void Export(string levelFileDirectory)
        {
            foreach (string lvlName in LevelNames.AsList)
            {
                if (ExportEnemyTypes.ContainsKey(lvlName))
                {
                    string levelPath = Path.Combine(levelFileDirectory, lvlName);
                    foreach (TR2Entities entity in ExportEnemyTypes[lvlName])
                    {
                        Export(levelPath, entity);
                    }
                }
            }
        }

        private void Export(string levelPath, TR2Entities entity)
        {
            if (!_processedEntities.Contains(entity))
            {
                // The level has to be re-read per entity because TextureTransportHandler can modify ObjectTextures
                // which when shared between entities is difficult to undo.
                TR2Level level = _reader.ReadLevel(levelPath);
                _exporter.Level = level;
                _exporter.TextureClassifier = new TRTextureClassifier(levelPath);
                _exporter.Export(entity);
                _processedEntities.Add(entity);

                foreach (TR2Entities dependency in _exporter.Definition.Dependencies)
                {
                    Export(levelPath, dependency);
                }
            }
        }

        public static readonly Dictionary<string, List<TR2Entities>> ExportEnemyTypes = new Dictionary<string, List<TR2Entities>>
        {
            { LevelNames.GW,
                new List<TR2Entities>{ TR2Entities.Crow, TR2Entities.Spider, TR2Entities.BengalTiger, TR2Entities.TRex }
            },

            { LevelNames.VENICE,
                new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat, TR2Entities.StickWieldingGoon1BodyWarmer }
            },

            { LevelNames.BARTOLI,
                new List<TR2Entities>{ TR2Entities.StickWieldingGoon1WhiteVest }
            },

            { LevelNames.OPERA,
                new List<TR2Entities>{ TR2Entities.ShotgunGoon }
            },

            { LevelNames.RIG,
                new List<TR2Entities>{ TR2Entities.Gunman1, TR2Entities.Gunman2, TR2Entities.ScubaDiver, TR2Entities.StickWieldingGoon1Bandana }
            },

            { LevelNames.DA,
                new List<TR2Entities>{ TR2Entities.FlamethrowerGoon }
            },

            { LevelNames.FATHOMS,
                new List<TR2Entities>{ TR2Entities.BarracudaUnwater, TR2Entities.Shark }
            },

            { LevelNames.DORIA,
                new List<TR2Entities>{ TR2Entities.StickWieldingGoon1GreenVest, TR2Entities.YellowMorayEel }
            },

            { LevelNames.LQ,
                new List<TR2Entities>{ TR2Entities.BlackMorayEel, TR2Entities.StickWieldingGoon2 }
            },

            { LevelNames.TIBET,
                new List<TR2Entities>{ TR2Entities.Eagle, TR2Entities.Mercenary2, TR2Entities.Mercenary3, TR2Entities.MercSnowmobDriver, TR2Entities.SnowLeopard }
            },

            { LevelNames.MONASTERY,
                new List<TR2Entities>{ TR2Entities.Mercenary1, TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick }
            },

            { LevelNames.COT,
                new List<TR2Entities>{ TR2Entities.BarracudaIce, TR2Entities.Yeti }
            },

            { LevelNames.CHICKEN,
                new List<TR2Entities>{ TR2Entities.BirdMonster, TR2Entities.WhiteTiger }
            },

            { LevelNames.XIAN,
                new List<TR2Entities>{ TR2Entities.BarracudaXian, TR2Entities.GiantSpider }
            },

            { LevelNames.FLOATER,
                new List<TR2Entities>{ TR2Entities.Knifethrower, TR2Entities.XianGuardSword, TR2Entities.XianGuardSpear }
            },

            { LevelNames.LAIR,
                new List<TR2Entities>{ TR2Entities.MarcoBartoli }
            },

            { LevelNames.HOME,
                new List<TR2Entities>{ TR2Entities.StickWieldingGoon1BlackJacket }
            }
        };
    }
}