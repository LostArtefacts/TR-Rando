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

        public static List<TR3Entities> GetEntityFamily(TR3Entities entity)
        {
            foreach (TR3Entities parentEntity in LevelEntityAliases.Keys)
            {
                if (LevelEntityAliases[parentEntity].ContainsKey(entity))
                {
                    return LevelEntityAliases[parentEntity].Keys.ToList();
                }
            }

            return new List<TR3Entities> { entity };
        }

        public static TR3Entities TranslateEntityAlias(TR3Entities entity)
        {
            foreach (TR3Entities parentEntity in LevelEntityAliases.Keys)
            {
                if (LevelEntityAliases[parentEntity].ContainsKey(entity))
                {
                    return parentEntity;
                }
            }

            return entity;
        }

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

        public static bool IsBridge(TR3Entities entity)
        {
            return GetBridgeTypes().Contains(entity);
        }

        public static List<TR3Entities> GetTrapdoorTypes()
        {
            return new List<TR3Entities>
            {
                TR3Entities.Trapdoor1, TR3Entities.Trapdoor2, TR3Entities.Trapdoor3
            };
        }

        public static List<TR3Entities> GetBridgeTypes()
        {
            return new List<TR3Entities>
            {
                TR3Entities.BridgeFlat, TR3Entities.BridgeTilt1, TR3Entities.BridgeTilt2
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

        public static bool IsArtefactPickup(TR3Entities entity)
        {
            return entity == TR3Entities.Infada_P
                || entity == TR3Entities.OraDagger_P
                || entity == TR3Entities.EyeOfIsis_P
                || entity == TR3Entities.Element115_P;
        }

        public static bool IsAnyPickupType(TR3Entities entity)
        {
            return IsUtilityPickup(entity)
                || IsAmmoPickup(entity)
                || IsWeaponPickup(entity)
                || IsKeyItemType(entity)
                || IsArtefactPickup(entity);
        }

        public static List<TR3Entities> GetCandidateCrossLevelEnemies()
        {
            return new List<TR3Entities>
            {
                TR3Entities.BruteMutant,
                TR3Entities.CobraIndia,
                TR3Entities.CobraNevada,
                TR3Entities.Compsognathus,
                TR3Entities.Crawler,
                //TR3Entities.CrawlerMutantInCloset, // Dies immediately on activation
                TR3Entities.Croc,
                TR3Entities.Crow,
                TR3Entities.DamGuard,
                TR3Entities.DogAntarc,
                TR3Entities.DogLondon,
                TR3Entities.DogNevada,
                TR3Entities.KillerWhale,
                TR3Entities.LizardMan,
                TR3Entities.LondonGuard,
                TR3Entities.LondonMerc,
                TR3Entities.Mercenary,
                TR3Entities.Monkey,
                TR3Entities.MPWithGun,
                TR3Entities.MPWithStick,
                TR3Entities.Prisoner,
                //TR3Entities.Puna, // Activates Lizard at hardcoded coordinates, which are OOB in all other levels
                TR3Entities.Punk,
                TR3Entities.Raptor,
                TR3Entities.Rat,
                TR3Entities.RXGunLad,
                TR3Entities.RXRedBoi,
                TR3Entities.RXTechFlameLad,
                TR3Entities.ScubaSteve,
                TR3Entities.Shiva,
                TR3Entities.Tiger,
                TR3Entities.TinnosMonster,
                TR3Entities.TinnosWasp,
                TR3Entities.TonyFirehands,
                TR3Entities.TribesmanAxe,
                TR3Entities.TribesmanDart,
                TR3Entities.Tyrannosaur,
                TR3Entities.Vulture,
                TR3Entities.Willie,
                TR3Entities.Winston,
                TR3Entities.WinstonInCamoSuit
            };
        }

        public static List<TR3Entities> GetFullListOfEnemies()
        {
            List<TR3Entities> enemies = new List<TR3Entities>
            {
                TR3Entities.SophiaLee, TR3Entities.Puna, TR3Entities.CrawlerMutantInCloset, TR3Entities.Cobra, TR3Entities.Dog
            };

            enemies.AddRange(GetCandidateCrossLevelEnemies());
            return enemies;
        }

        public static List<TR3Entities> GetWaterEnemies()
        {
            return new List<TR3Entities>
            {
                TR3Entities.Croc,
                TR3Entities.KillerWhale,
                TR3Entities.ScubaSteve
            };
        }

        public static bool IsWaterCreature(TR3Entities entity)
        {
            return GetWaterEnemies().Contains(entity);
        }

        public static List<TR3Entities> FilterWaterEnemies(List<TR3Entities> entities)
        {
            List<TR3Entities> waterEntities = new List<TR3Entities>();
            foreach (TR3Entities entity in entities)
            {
                if (IsWaterCreature(entity))
                {
                    waterEntities.Add(entity);
                }
            }
            return waterEntities;
        }

        public static List<TR3Entities> GetKillableWaterEnemies()
        {
            return new List<TR3Entities>
            {
                TR3Entities.Croc,
                TR3Entities.ScubaSteve
            };
        }

        public static bool CanDropPickups(TR3Entities entity, bool protectFriendlyEnemies)
        {
            return GetDroppableEnemies(protectFriendlyEnemies).Contains(entity);
        }

        public static List<TR3Entities> FilterDroppableEnemies(List<TR3Entities> entities, bool protectFriendlyEnemies)
        {
            List<TR3Entities> droppableEntities = new List<TR3Entities>();
            foreach (TR3Entities entity in entities)
            {
                if (CanDropPickups(entity, protectFriendlyEnemies))
                {
                    droppableEntities.Add(entity);
                }
            }
            return droppableEntities;
        }

        public static List<TR3Entities> GetDroppableEnemies(bool protectFriendlyEnemies)
        {
            List<TR3Entities> enemies = new List<TR3Entities>
            {
                TR3Entities.BruteMutant,
                TR3Entities.CobraIndia,
                TR3Entities.CobraNevada,
                TR3Entities.Cobra,
                TR3Entities.Compsognathus,
                TR3Entities.Crawler,
                TR3Entities.Crow,
                TR3Entities.DamGuard,
                TR3Entities.DogAntarc,
                TR3Entities.DogLondon,
                TR3Entities.DogNevada,
                TR3Entities.Dog,
                TR3Entities.LizardMan,
                TR3Entities.LondonGuard,
                TR3Entities.LondonMerc,
                TR3Entities.Monkey,
                TR3Entities.MPWithGun,
                TR3Entities.MPWithStick,
                TR3Entities.Punk,
                TR3Entities.Raptor,
                TR3Entities.Rat,
                TR3Entities.RXGunLad,
                TR3Entities.RXRedBoi,
                TR3Entities.Shiva,
                TR3Entities.SophiaLee,
                TR3Entities.Tiger,
                TR3Entities.TinnosMonster,
                TR3Entities.TinnosWasp,
                TR3Entities.TonyFirehands,
                TR3Entities.TribesmanAxe,
                TR3Entities.TribesmanDart,
                TR3Entities.Tyrannosaur,
                TR3Entities.Vulture
            };

            if (!protectFriendlyEnemies)
            {
                enemies.Add(TR3Entities.Mercenary);
                enemies.Add(TR3Entities.Prisoner);
                enemies.Add(TR3Entities.RXTechFlameLad); // NB Unfriendly if Willie sequence
            }

            return enemies;
        }
    }
}