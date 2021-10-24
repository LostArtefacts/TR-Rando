using System.Collections.Generic;
using TRLevelReader.Model.Enums;

namespace TRLevelReader.Helpers
{
    public static class TR3EntityUtilities
    {
        public static readonly Dictionary<TR3Entities, Dictionary<TR3Entities, List<string>>> LevelEntityAliases = new Dictionary<TR3Entities, Dictionary<TR3Entities, List<string>>>
        {
            [TR3Entities.Lara] = new Dictionary<TR3Entities, List<string>>
            {
                [TR3Entities.LaraIndia]
                    = new List<string> { TR3LevelNames.JUNGLE, TR3LevelNames.RUINS, TR3LevelNames.GANGES, TR3LevelNames.CAVES },
                [TR3Entities.LaraCoastal]
                    = new List<string> { TR3LevelNames.COASTAL, TR3LevelNames.CRASH, TR3LevelNames.MADUBU, TR3LevelNames.PUNA },
                [TR3Entities.LaraLondon]
                    = new List<string> { TR3LevelNames.THAMES, TR3LevelNames.ALDWYCH, TR3LevelNames.LUDS, TR3LevelNames.CITY, TR3LevelNames.HALLOWS },
                [TR3Entities.LaraNevada]
                    = new List<string> { TR3LevelNames.NEVADA, TR3LevelNames.HSC, TR3LevelNames.AREA51 },
                [TR3Entities.LaraAntarc]
                    = new List<string> { TR3LevelNames.ANTARC, TR3LevelNames.RXTECH, TR3LevelNames.TINNOS, TR3LevelNames.WILLIE }
            },
            [TR3Entities.Cobra] = new Dictionary<TR3Entities, List<string>>
            {
                [TR3Entities.CobraIndia]
                    = new List<string> { TR3LevelNames.RUINS, TR3LevelNames.GANGES, TR3LevelNames.CAVES },
                [TR3Entities.CobraNevada]
                    = new List<string> { TR3LevelNames.NEVADA },
            },
            [TR3Entities.Dog] = new Dictionary<TR3Entities, List<string>>
            {
                [TR3Entities.DogLondon] 
                    = new List<string> { TR3LevelNames.ALDWYCH },
                [TR3Entities.DogNevada]
                    = new List<string> { TR3LevelNames.HSC, TR3LevelNames.HALLOWS }
            },
        };

        public static TR3Entities GetAliasForLevel(string lvl, TR3Entities entity)
        {
            if (LevelEntityAliases.ContainsKey(entity))
            {
                foreach (TR3Entities alias in LevelEntityAliases[entity].Keys)
                {
                    if (LevelEntityAliases[entity][alias].Contains(lvl))
                    {
                        return alias;
                    }
                }
            }
            return entity;
        }

        public static List<TR3Entities> GetLaraTypes()
        {
            return new List<TR3Entities>
            {
                TR3Entities.LaraIndia, TR3Entities.LaraCoastal, TR3Entities.LaraLondon, TR3Entities.LaraNevada, TR3Entities.LaraAntarc, TR3Entities.LaraInvisible
            };
        }
    }
}