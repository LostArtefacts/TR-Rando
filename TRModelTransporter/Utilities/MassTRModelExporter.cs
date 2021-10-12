using System.Collections.Generic;
using System.IO;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities
{
    public class MassTRModelExporter
    {
        private readonly TRModelExporter _exporter;
        private readonly List<TR2Entities> _processedEntities;
        private readonly TR2LevelReader _reader;

        public MassTRModelExporter(bool exportIndividualSegments)
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
            List<string> allLevels = TR2LevelNames.AsListWithAssault;
            foreach (string lvlName in allLevels)
            {
                if (ExportModelTypes.ContainsKey(lvlName))
                {
                    string levelPath = Path.Combine(levelFileDirectory, lvlName);
                    foreach (TR2Entities entity in ExportModelTypes[lvlName])
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

        public static readonly Dictionary<string, List<TR2Entities>> ExportModelTypes = new Dictionary<string, List<TR2Entities>>
        {
            { TR2LevelNames.GW,
                new List<TR2Entities>
                {
                    TR2Entities.Pistols_M_H, TR2Entities.Shotgun_M_H, TR2Entities.Uzi_M_H, TR2Entities.Autos_M_H, TR2Entities.Harpoon_M_H, TR2Entities.M16_M_H, TR2Entities.GrenadeLauncher_M_H,
                    TR2Entities.LaraSun, TR2Entities.Crow, TR2Entities.Spider, TR2Entities.BengalTiger, TR2Entities.TRex
                }
            },

            { TR2LevelNames.VENICE,
                new List<TR2Entities>{ TR2Entities.Boat, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat, TR2Entities.StickWieldingGoon1BodyWarmer }
            },

            { TR2LevelNames.BARTOLI,
                new List<TR2Entities>{ TR2Entities.StickWieldingGoon1WhiteVest }
            },

            { TR2LevelNames.OPERA,
                new List<TR2Entities>{ TR2Entities.ShotgunGoon }
            },

            { TR2LevelNames.RIG,
                new List<TR2Entities>{ TR2Entities.Gunman1, TR2Entities.Gunman2, TR2Entities.ScubaDiver, TR2Entities.StickWieldingGoon1Bandana }
            },

            { TR2LevelNames.DA,
                new List<TR2Entities>{ TR2Entities.FlamethrowerGoon }
            },

            { TR2LevelNames.FATHOMS,
                new List<TR2Entities>{ TR2Entities.LaraUnwater, TR2Entities.BarracudaUnwater, TR2Entities.Shark }
            },

            { TR2LevelNames.DORIA,
                new List<TR2Entities>{ TR2Entities.StickWieldingGoon1GreenVest, TR2Entities.YellowMorayEel }
            },

            { TR2LevelNames.LQ,
                new List<TR2Entities>{ TR2Entities.BlackMorayEel, TR2Entities.StickWieldingGoon2 }
            },

            { TR2LevelNames.TIBET,
                new List<TR2Entities>{ TR2Entities.LaraSnow, TR2Entities.Eagle, TR2Entities.Mercenary2, TR2Entities.Mercenary3, TR2Entities.MercSnowmobDriver, TR2Entities.SnowLeopard }
            },

            { TR2LevelNames.MONASTERY,
                new List<TR2Entities>{ TR2Entities.Mercenary1, TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick }
            },

            { TR2LevelNames.COT,
                new List<TR2Entities>{ TR2Entities.BarracudaIce, TR2Entities.Yeti }
            },

            { TR2LevelNames.CHICKEN,
                new List<TR2Entities>{ TR2Entities.BirdMonster, TR2Entities.WhiteTiger }
            },

            { TR2LevelNames.XIAN,
                new List<TR2Entities>{ TR2Entities.BarracudaXian, TR2Entities.GiantSpider }
            },

            { TR2LevelNames.FLOATER,
                new List<TR2Entities>{ TR2Entities.Knifethrower, TR2Entities.XianGuardSword, TR2Entities.XianGuardSpear }
            },

            { TR2LevelNames.LAIR,
                new List<TR2Entities>{ TR2Entities.MarcoBartoli }
            },

            { TR2LevelNames.HOME,
                new List<TR2Entities>{ TR2Entities.LaraHome, TR2Entities.StickWieldingGoon1BlackJacket }
            },

            { TR2LevelNames.ASSAULT,
                new List<TR2Entities>{ TR2Entities.Winston }
            }
        };
    }
}