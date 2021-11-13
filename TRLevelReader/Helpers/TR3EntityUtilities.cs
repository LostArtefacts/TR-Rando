using System.Collections.Generic;
using System.Linq;
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

        public static List<TR3Entities> GetListOfKeyTypes()
        {
            return new List<TR3Entities>
            {
                TR3Entities.Key1_P,
                TR3Entities.Key2_P,
                TR3Entities.Key3_P,
                TR3Entities.Key4_P
            };
        }

        public static List<TR3Entities> GetListOfPuzzleTypes()
        {
            return new List<TR3Entities>
            {
                TR3Entities.Puzzle1_P,
                TR3Entities.Puzzle2_P,
                TR3Entities.Puzzle3_P,
                TR3Entities.Puzzle4_P
            };
        }

        public static List<TR3Entities> GetListOfQuestTypes()
        {
            return new List<TR3Entities>
            {
                TR3Entities.Quest1_P,
                TR3Entities.Quest2_P
            };
        }

        public static List<TR3Entities> GetListOfKeyItemTypes()
        {
            return GetListOfKeyTypes().Concat(GetListOfPuzzleTypes()).Concat(GetListOfQuestTypes()).ToList();
        }

        public static bool IsKeyType(TR3Entities entity)
        {
            return GetListOfKeyTypes().Contains(entity);
        }

        public static bool IsPuzzleType(TR3Entities entity)
        {
            return GetListOfPuzzleTypes().Contains(entity);
        }

        public static bool IsQuestType(TR3Entities entity)
        {
            return GetListOfQuestTypes().Contains(entity);
        }

        public static bool IsKeyItemType(TR3Entities entity)
        {
            return GetListOfKeyItemTypes().Contains(entity);
        }

        public static Dictionary<TR3Entities, TR3Entities> GetArtefactPickups()
        {
            return new Dictionary<TR3Entities, TR3Entities>
            {
                [TR3Entities.Infada_P] = TR3Entities.Infada_M_H,
                [TR3Entities.OraDagger_P] = TR3Entities.OraDagger_M_H,
                [TR3Entities.EyeOfIsis_P] = TR3Entities.EyeOfIsis_M_H,
                [TR3Entities.Element115_P] = TR3Entities.Element115_M_H,
                [TR3Entities.Quest1_P] = TR3Entities.Quest1_M_H, // Serpent Stone
                [TR3Entities.Quest2_P] = TR3Entities.Quest2_M_H  // Hand of Rathmore
            };
        }

        public static Dictionary<TR3Entities, TR3Entities> GetArtefactReplacements()
        {
            return new Dictionary<TR3Entities, TR3Entities>
            {
                [TR3Entities.Puzzle1_P] = TR3Entities.Puzzle1_M_H,
                [TR3Entities.Puzzle2_P] = TR3Entities.Puzzle2_M_H,
                [TR3Entities.Puzzle3_P] = TR3Entities.Puzzle3_M_H,
                [TR3Entities.Puzzle4_P] = TR3Entities.Puzzle4_M_H,
                [TR3Entities.Key1_P] = TR3Entities.Key1_M_H,
                [TR3Entities.Key2_P] = TR3Entities.Key2_M_H,
                [TR3Entities.Key3_P] = TR3Entities.Key3_M_H,
                [TR3Entities.Key4_P] = TR3Entities.Key4_M_H,
                [TR3Entities.Quest1_P] = TR3Entities.Quest1_M_H,
                [TR3Entities.Quest2_P] = TR3Entities.Quest2_M_H
            };
        }

        public static bool IsTrapdoor(TR3Entities entity)
        {
            return GetTrapdoorTypes().Contains(entity);
        }

        public static List<TR3Entities> GetTrapdoorTypes()
        {
            return new List<TR3Entities>
            {
                TR3Entities.Trapdoor1, TR3Entities.Trapdoor2, TR3Entities.Trapdoor3
            };
        }

        public static List<TR3Entities> GetStandardPickupTypes()
        {
            return new List<TR3Entities>
            {
                TR3Entities.Pistols_P,
                TR3Entities.Shotgun_P,
                TR3Entities.Deagle_P,
                TR3Entities.Uzis_P,
                TR3Entities.Harpoon_P,
                TR3Entities.MP5_P,
                TR3Entities.RocketLauncher_P,
                TR3Entities.GrenadeLauncher_P,
                TR3Entities.PistolAmmo_P,
                TR3Entities.ShotgunAmmo_P,
                TR3Entities.DeagleAmmo_P,
                TR3Entities.UziAmmo_P,
                TR3Entities.Harpoons_P,
                TR3Entities.MP5Ammo_P,
                TR3Entities.Rockets_P,
                TR3Entities.Grenades_P,
                TR3Entities.SmallMed_P,
                TR3Entities.LargeMed_P,
                TR3Entities.Flares_P,
            };
        }

        public static bool IsStandardPickupType(TR3Entities entity)
        {
            return GetStandardPickupTypes().Contains(entity);
        }

        public static bool IsWeaponPickup(TR3Entities entity)
        {
            return (entity == TR3Entities.Pistols_P)
                || (entity == TR3Entities.Shotgun_P)
                || (entity == TR3Entities.Deagle_P)
                || (entity == TR3Entities.Uzis_P)
                || (entity == TR3Entities.Harpoon_P)
                || (entity == TR3Entities.MP5_P)
                || (entity == TR3Entities.RocketLauncher_P)
                || (entity == TR3Entities.GrenadeLauncher_P);
        }

        public static bool IsAmmoPickup(TR3Entities entity)
        {
            return (entity == TR3Entities.PistolAmmo_P)
                || (entity == TR3Entities.ShotgunAmmo_P)
                || (entity == TR3Entities.DeagleAmmo_P)
                || (entity == TR3Entities.UziAmmo_P)
                || (entity == TR3Entities.Harpoons_P)
                || (entity == TR3Entities.MP5Ammo_P)
                || (entity == TR3Entities.Rockets_P)
                || (entity == TR3Entities.Grenades_P);
        }

        public static bool IsCrystalPickup(TR3Entities entity)
        {
            return (entity == TR3Entities.SaveCrystal_P);
        }

        public static bool IsUtilityPickup(TR3Entities entity)
        {
            return (entity == TR3Entities.SmallMed_P)
                || (entity == TR3Entities.LargeMed_P)
                || (entity == TR3Entities.Flares_P);
        }
    }
}