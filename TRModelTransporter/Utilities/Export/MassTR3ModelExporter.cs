using System.Collections.Generic;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Transport;

namespace TRModelTransporter.Utilities
{
    public class MassTR3ModelExporter : AbstractMassTRModelExporter<TR3Entities, TR3Level, TR3ModelDefinition>
    {
        public override List<string> LevelNames => TR3LevelNames.AsListWithAssault;

        public override Dictionary<string, List<TR3Entities>> ExportTypes => _exportModelTypes;

        private readonly TR3LevelReader _reader;

        public MassTR3ModelExporter()
        {
            _reader = new TR3LevelReader();
        }

        protected override AbstractTRModelExporter<TR3Entities, TR3Level, TR3ModelDefinition> CreateExporter()
        {
            return new TR3ModelExporter();
        }

        protected override TR3Level ReadLevel(string path)
        {
            return _reader.ReadLevel(path);
        }

        private static readonly Dictionary<string, List<TR3Entities>> _exportModelTypes = new Dictionary<string, List<TR3Entities>>
        {
            [TR3LevelNames.JUNGLE] = new List<TR3Entities>
            {
                TR3Entities.LaraIndia, TR3Entities.Monkey, TR3Entities.Tiger, TR3Entities.Door1
            },
            [TR3LevelNames.RUINS] = new List<TR3Entities>
            {
                TR3Entities.Shiva, TR3Entities.CobraIndia
            },
            [TR3LevelNames.GANGES] = new List<TR3Entities>
            {
                TR3Entities.Quad, TR3Entities.Vulture
            },
            [TR3LevelNames.CAVES] = new List<TR3Entities>
            {
                TR3Entities.TonyFirehands, TR3Entities.Infada_P
            },
            [TR3LevelNames.COASTAL] = new List<TR3Entities>
            {
                TR3Entities.LaraCoastal, TR3Entities.Croc, TR3Entities.TribesmanAxe, TR3Entities.TribesmanDart
            },
            [TR3LevelNames.CRASH] = new List<TR3Entities>
            {
                TR3Entities.Compsognathus, TR3Entities.Mercenary, TR3Entities.Raptor, TR3Entities.Tyrannosaur
            },
            [TR3LevelNames.MADUBU] = new List<TR3Entities>
            {
                TR3Entities.Kayak, TR3Entities.LizardMan
            },
            [TR3LevelNames.PUNA] = new List<TR3Entities>
            {
                TR3Entities.Puna, TR3Entities.OraDagger_P
            },
            [TR3LevelNames.THAMES] = new List<TR3Entities>
            {
                TR3Entities.LaraLondon, TR3Entities.Crow, TR3Entities.LondonGuard, TR3Entities.LondonMerc, TR3Entities.Rat
            },
            [TR3LevelNames.ALDWYCH] = new List<TR3Entities>
            {
                TR3Entities.Punk, TR3Entities.DogLondon
            },
            [TR3LevelNames.LUDS] = new List<TR3Entities>
            {
                TR3Entities.ScubaSteve, TR3Entities.UPV
            },
            [TR3LevelNames.CITY] = new List<TR3Entities>
            {
                TR3Entities.SophiaLee, TR3Entities.EyeOfIsis_P
            },
            [TR3LevelNames.NEVADA] = new List<TR3Entities>
            {
                TR3Entities.LaraNevada, TR3Entities.DamGuard, TR3Entities.CobraNevada
            },
            [TR3LevelNames.HSC] = new List<TR3Entities>
            {
                TR3Entities.MPWithStick, TR3Entities.MPWithGun, TR3Entities.Prisoner, TR3Entities.DogNevada
            },
            [TR3LevelNames.AREA51] = new List<TR3Entities>
            {
                TR3Entities.KillerWhale, TR3Entities.MPWithMP5, TR3Entities.Element115_P
            },
            [TR3LevelNames.ANTARC] = new List<TR3Entities>
            {
                TR3Entities.LaraAntarc, TR3Entities.CrawlerMutantInCloset, TR3Entities.Boat, TR3Entities.RXRedBoi, TR3Entities.DogAntarc
            }
            ,
            [TR3LevelNames.RXTECH] = new List<TR3Entities>
            {
                TR3Entities.Crawler, TR3Entities.RXTechFlameLad, TR3Entities.BruteMutant
            },
            [TR3LevelNames.TINNOS] = new List<TR3Entities>
            {
                TR3Entities.TinnosMonster, TR3Entities.TinnosWasp, TR3Entities.Door4
            },
            [TR3LevelNames.WILLIE] = new List<TR3Entities>
            {
                TR3Entities.Willie, TR3Entities.RXGunLad
            },
            [TR3LevelNames.HALLOWS] = new List<TR3Entities>
            {
                
            },
            [TR3LevelNames.ASSAULT] = new List<TR3Entities>
            {
                TR3Entities.LaraHome, TR3Entities.Winston, TR3Entities.WinstonInCamoSuit
            }
        };
    }
}