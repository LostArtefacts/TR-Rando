using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model.Enums;

namespace TRLevelReader.Helpers
{
    public static class TR1EntityUtilities
    {
        public static readonly Dictionary<TREntities, Dictionary<TREntities, List<string>>> LevelEntityAliases = new Dictionary<TREntities, Dictionary<TREntities, List<string>>>
        {
            [TREntities.FlyingAtlantean] = new Dictionary<TREntities, List<string>>
            {
                [TREntities.BandagedFlyer] =
                    new List<string> { TRLevelNames.KHAMOON, TRLevelNames.OBELISK },
                [TREntities.MeatyFlyer] =
                    new List<string> { TRLevelNames.SANCTUARY, TRLevelNames.ATLANTIS }
            },
            [TREntities.NonShootingAtlantean_N] = new Dictionary<TREntities, List<string>>
            {
                [TREntities.BandagedAtlantean] =
                    new List<string> { TRLevelNames.KHAMOON, TRLevelNames.OBELISK },
                [TREntities.MeatyAtlantean] =
                    new List<string> { TRLevelNames.SANCTUARY, TRLevelNames.ATLANTIS }
            }
        };

        public static readonly Dictionary<TREntities, List<TREntities>> EntityFamilies = new Dictionary<TREntities, List<TREntities>>
        {
            [TREntities.FlyingAtlantean] = new List<TREntities>
            {
                TREntities.BandagedFlyer, TREntities.MeatyFlyer
            },
            [TREntities.NonShootingAtlantean_N] = new List<TREntities>
            {
                TREntities.BandagedAtlantean, TREntities.MeatyAtlantean
            }
        };

        public static TREntities TranslateEntityAlias(TREntities entity)
        {
            foreach (TREntities parentEntity in EntityFamilies.Keys)
            {
                if (EntityFamilies[parentEntity].Contains(entity))
                {
                    return parentEntity;
                }
            }

            return entity;
        }

        public static TREntities GetAliasForLevel(string lvl, TREntities entity)
        {
            if (LevelEntityAliases.ContainsKey(entity))
            {
                foreach (TREntities alias in LevelEntityAliases[entity].Keys)
                {
                    if (LevelEntityAliases[entity][alias].Contains(lvl))
                    {
                        return alias;
                    }
                }
            }
            return entity;
        }

        public static List<TREntities> GetEntityFamily(TREntities entity)
        {
            foreach (TREntities parentEntity in EntityFamilies.Keys)
            {
                if (EntityFamilies[parentEntity].Contains(entity))
                {
                    return EntityFamilies[parentEntity];
                }
            }

            return new List<TREntities> { entity };
        }

        public static List<TREntities> RemoveAliases(IEnumerable<TREntities> entities)
        {
            List<TREntities> ents = new List<TREntities>();
            foreach (TREntities ent in entities)
            {
                TREntities normalisedEnt = TranslateEntityAlias(ent);
                if (!ents.Contains(normalisedEnt))
                {
                    ents.Add(normalisedEnt);
                }
            }
            return ents;
        }

        public static List<TREntities> GetListOfKeyTypes()
        {
            return new List<TREntities>
            {
                TREntities.Key1_S_P,
                TREntities.Key2_S_P,
                TREntities.Key3_S_P,
                TREntities.Key4_S_P
            };
        }

        public static List<TREntities> GetListOfPuzzleTypes()
        {
            return new List<TREntities>
            {
                TREntities.Puzzle1_S_P,
                TREntities.Puzzle2_S_P,
                TREntities.Puzzle3_S_P,
                TREntities.Puzzle4_S_P
            };
        }

        public static List<TREntities> GetListOfQuestTypes()
        {
            return new List<TREntities>
            {
                TREntities.Quest1_S_P,
                TREntities.Quest2_S_P
            };
        }

        public static List<TREntities> GetListOfLeadTypes()
        {
            return new List<TREntities>
            {
                TREntities.LeadBar_S_P
            };
        }

        public static List<TREntities> GetListOfKeyItemTypes()
        {
            return GetListOfKeyTypes().Concat(GetListOfPuzzleTypes()).Concat(GetListOfQuestTypes()).Concat(GetListOfLeadTypes()).ToList();
        }

        public static Dictionary<TREntities, TREntities> GetKeyItemMap()
        {
            // Maps pickups to inventory models
            return new Dictionary<TREntities, TREntities>
            {
                [TREntities.Key1_S_P] = TREntities.Key1_M_H,
                [TREntities.Key2_S_P] = TREntities.Key2_M_H,
                [TREntities.Key3_S_P] = TREntities.Key3_M_H,
                [TREntities.Key4_S_P] = TREntities.Key4_M_H,
                [TREntities.Puzzle1_S_P] = TREntities.Puzzle1_M_H,
                [TREntities.Puzzle2_S_P] = TREntities.Puzzle2_M_H,
                [TREntities.Puzzle3_S_P] = TREntities.Puzzle3_M_H,
                [TREntities.Puzzle4_S_P] = TREntities.Puzzle4_M_H,
                [TREntities.Quest1_S_P] = TREntities.Quest1_M_H,
                [TREntities.Quest2_S_P] = TREntities.Quest2_M_H,
                [TREntities.LeadBar_S_P] = TREntities.LeadBar_M_H,
                [TREntities.ScionPiece1_S_P] = TREntities.ScionPiece_M_H,
                [TREntities.ScionPiece2_S_P] = TREntities.ScionPiece_M_H,
            };
        }

        public static bool IsKeyType(TREntities entity)
        {
            return GetListOfKeyTypes().Contains(entity);
        }

        public static bool IsPuzzleType(TREntities entity)
        {
            return GetListOfPuzzleTypes().Contains(entity);
        }

        public static bool IsQuestType(TREntities entity)
        {
            return GetListOfQuestTypes().Contains(entity);
        }

        public static bool IsKeyItemType(TREntities entity)
        {
            return GetListOfKeyItemTypes().Contains(entity);
        }

        public static bool IsTrapdoor(TREntities entity)
        {
            return GetTrapdoorTypes().Contains(entity);
        }

        public static bool IsBridge(TREntities entity)
        {
            return GetBridgeTypes().Contains(entity);
        }

        public static List<TREntities> GetTrapdoorTypes()
        {
            return new List<TREntities>
            {
                TREntities.Trapdoor1, TREntities.Trapdoor2, TREntities.Trapdoor3
            };
        }

        public static List<TREntities> GetBridgeTypes()
        {
            return new List<TREntities>
            {
                TREntities.BridgeFlat, TREntities.BridgeTilt1, TREntities.BridgeTilt2
            };
        }

        public static List<TREntities> GetStandardPickupTypes()
        {
            return new List<TREntities>
            {
                TREntities.Pistols_S_P,
                TREntities.Shotgun_S_P,
                TREntities.Magnums_S_P,
                TREntities.Uzis_S_P,
                TREntities.PistolAmmo_S_P,
                TREntities.ShotgunAmmo_S_P,
                TREntities.MagnumAmmo_S_P,
                TREntities.UziAmmo_S_P,
                TREntities.SmallMed_S_P,
                TREntities.LargeMed_S_P
            };
        }

        public static bool IsStandardPickupType(TREntities entity)
        {
            return GetStandardPickupTypes().Contains(entity);
        }

        public static bool IsWeaponPickup(TREntities entity)
        {
            return GetWeaponPickups().Contains(entity);
        }

        public static List<TREntities> GetWeaponPickups()
        {
            return new List<TREntities>
            {
                TREntities.Pistols_S_P,
                TREntities.Shotgun_S_P,
                TREntities.Magnums_S_P,
                TREntities.Uzis_S_P
            };
        }

        public static bool IsAmmoPickup(TREntities entity)
        {
            return (entity == TREntities.PistolAmmo_S_P)
                || (entity == TREntities.ShotgunAmmo_S_P)
                || (entity == TREntities.MagnumAmmo_S_P)
                || (entity == TREntities.UziAmmo_S_P);
        }

        public static TREntities GetWeaponAmmo(TREntities weapon)
        {
            switch (weapon)
            {
                case TREntities.Shotgun_S_P:
                    return TREntities.ShotgunAmmo_S_P;
                case TREntities.Magnums_S_P:
                    return TREntities.MagnumAmmo_S_P;
                case TREntities.Uzis_S_P:
                    return TREntities.UziAmmo_S_P;
                default:
                    return TREntities.PistolAmmo_S_P;
            }
        }

        public static bool IsCrystalPickup(TREntities entity)
        {
            return entity == TREntities.SavegameCrystal_P;
        }

        public static bool IsUtilityPickup(TREntities entity)
        {
            return (entity == TREntities.SmallMed_S_P)
                || (entity == TREntities.LargeMed_S_P);
        }

        public static bool IsAnyPickupType(TREntities entity)
        {
            return IsUtilityPickup(entity)
                || IsAmmoPickup(entity)
                || IsWeaponPickup(entity)
                || IsKeyItemType(entity);
        }

        public static List<TREntities> GetCandidateCrossLevelEnemies()
        {
            return new List<TREntities>
            {
                TREntities.Adam,
                TREntities.AtlanteanEgg,
                TREntities.BandagedAtlantean,
                TREntities.BandagedFlyer,
                TREntities.Bat,
                TREntities.Bear,
                TREntities.Centaur,
                TREntities.CentaurStatue,
                TREntities.Cowboy,
                TREntities.CrocodileLand,
                TREntities.CrocodileWater,                
                TREntities.Gorilla,
                TREntities.Kold,
                TREntities.Larson,
                TREntities.Lion,
                TREntities.Lioness,
                TREntities.MeatyAtlantean,
                TREntities.MeatyFlyer,
                TREntities.Natla,
                TREntities.Panther,
                TREntities.Pierre,
                TREntities.Raptor,
                TREntities.RatLand,
                TREntities.RatWater,
                TREntities.SkateboardKid,
                TREntities.TRex,
                TREntities.Wolf
            };
        }

        public static bool IsEnemyType(TREntities entity)
        {
            return GetFullListOfEnemies().Contains(entity);
        }

        public static List<TREntities> GetFullListOfEnemies()
        {
            List<TREntities> enemies = new List<TREntities>
            {
                TREntities.FlyingAtlantean, TREntities.NonShootingAtlantean_N, TREntities.ShootingAtlantean_N
            };

            enemies.AddRange(GetCandidateCrossLevelEnemies());
            return enemies;
        }

        public static List<TREntities> GetWaterEnemies()
        {
            return new List<TREntities>
            {
                TREntities.CrocodileWater,
                TREntities.RatWater
            };
        }

        public static Dictionary<TREntities, TREntities> GetWaterEnemyLandCreatures()
        {
            return new Dictionary<TREntities, TREntities>
            {
                [TREntities.CrocodileWater] = TREntities.CrocodileLand,
                [TREntities.RatWater] = TREntities.RatLand
            };
        }

        public static TREntities GetWaterEnemyLandCreature(TREntities entity)
        {
            Dictionary<TREntities, TREntities> entities = GetWaterEnemyLandCreatures();
            return entities.ContainsKey(entity) ? entities[entity] : entity;
        }

        public static bool IsWaterCreature(TREntities entity)
        {
            return GetWaterEnemies().Contains(entity);
        }

        public static bool IsWaterLandCreatureEquivalent(TREntities entity)
        {
            return GetWaterEnemyLandCreatures().ContainsValue(entity);
        }

        public static List<TREntities> GetWaterLandCreatures()
        {
            return GetWaterEnemyLandCreatures().Values.ToList();
        }

        public static List<TREntities> FilterWaterEnemies(List<TREntities> entities)
        {
            List<TREntities> waterEntities = new List<TREntities>();
            foreach (TREntities entity in entities)
            {
                if (IsWaterCreature(entity))
                {
                    waterEntities.Add(entity);
                }
            }
            return waterEntities;
        }

        public static List<TREntities> GetAtlanteanEggEnemies()
        {
            return new List<TREntities>
            {
                TREntities.BandagedAtlantean,
                TREntities.BandagedFlyer,
                TREntities.Centaur,
                TREntities.MeatyAtlantean,
                TREntities.MeatyFlyer,
                TREntities.ShootingAtlantean_N
            };
        }

        public static List<TREntities> GetSwitchTypes()
        {
            return new List<TREntities>
            {
                TREntities.WallSwitch,
                TREntities.UnderwaterSwitch
            };
        }

        public static bool IsSwitchType(TREntities entity)
        {
            return GetSwitchTypes().Contains(entity);
        }

        public static List<TREntities> GetKeyholeTypes()
        {
            return new List<TREntities>
            {
                TREntities.Keyhole1,
                TREntities.Keyhole2,
                TREntities.Keyhole3,
                TREntities.Keyhole4
            };
        }

        public static bool IsKeyholeType(TREntities entity)
        {
            return GetKeyholeTypes().Contains(entity);
        }

        public static List<TREntities> GetSlotTypes()
        {
            return new List<TREntities>
            {
                TREntities.PuzzleHole1,
                TREntities.PuzzleHole2,
                TREntities.PuzzleHole3,
                TREntities.PuzzleHole4,
                TREntities.PuzzleDone1,
                TREntities.PuzzleDone2,
                TREntities.PuzzleDone3,
                TREntities.PuzzleDone4
            };
        }

        public static bool IsSlotType(TREntities entity)
        {
            return GetSlotTypes().Contains(entity);
        }

        public static bool CanSharePickupSpace(TREntities entity)
        {
            // Can we place a standard pickup on the same tile as this entity?
            return IsStandardPickupType(entity)
                || IsCrystalPickup(entity)
                || IsSwitchType(entity)
                || IsKeyholeType(entity)
                || IsSlotType(entity)
                || IsEnemyType(entity)
                || entity == TREntities.CameraTarget_N
                || entity == TREntities.Earthquake_N
                || entity == TREntities.Lara;
        }

        public static List<TREntities> DoorTypes()
        {
            return new List<TREntities>
            {
                TREntities.Door1, TREntities.Door2, TREntities.Door3,
                TREntities.Door4, TREntities.Door5, TREntities.Door6,
                TREntities.Door7, TREntities.Door8
            };
        }

        public static bool IsDoorType(TREntities entity)
        {
            return DoorTypes().Contains(entity);
        }

        public static Dictionary<TREntities, TREntities> GetSecretModels()
        {
            return new Dictionary<TREntities, TREntities>
            {
                [TREntities.SecretAnkh_M_H] = TREntities.ScionPiece4_S_P,
                [TREntities.SecretGoldBar_M_H] = TREntities.ScionPiece4_S_P,
                [TREntities.SecretGoldIdol_M_H] = TREntities.ScionPiece4_S_P,
                [TREntities.SecretLeadBar_M_H] = TREntities.ScionPiece4_S_P,
                [TREntities.SecretScion_M_H] = TREntities.ScionPiece4_S_P
            };
        }

        public static Dictionary<TREntities, TREntities> GetSecretReplacements()
        {
            // Note Key1 is omitted because of Pierre
            return new Dictionary<TREntities, TREntities>
            {
                [TREntities.Puzzle1_M_H] = TREntities.Puzzle1_S_P,
                [TREntities.Puzzle2_M_H] = TREntities.Puzzle2_S_P,
                [TREntities.Puzzle3_M_H] = TREntities.Puzzle3_S_P,
                [TREntities.Puzzle4_M_H] = TREntities.Puzzle4_S_P,
                [TREntities.Key2_M_H] = TREntities.Key2_S_P,
                [TREntities.Key3_M_H] = TREntities.Key3_S_P,
                [TREntities.Key4_M_H] = TREntities.Key4_S_P,
                [TREntities.Quest1_M_H] = TREntities.Quest1_S_P,
                [TREntities.Quest2_M_H] = TREntities.Quest2_S_P
            };
        }

        public static TREntities GetBestLevelSecretModel(string lvl)
        {
            return _levelSecretModels.ContainsKey(lvl) ? _levelSecretModels[lvl] : GetSecretModels().Keys.FirstOrDefault();
        }

        private static readonly Dictionary<string, TREntities> _levelSecretModels = new Dictionary<string, TREntities>
        {
            [TRLevelNames.CAVES]
                = TREntities.SecretScion_M_H,
            [TRLevelNames.VILCABAMBA]
                = TREntities.SecretGoldBar_M_H,
            [TRLevelNames.VALLEY]
                = TREntities.SecretGoldIdol_M_H,
            [TRLevelNames.QUALOPEC]
                = TREntities.SecretGoldIdol_M_H,
            [TRLevelNames.FOLLY]
                = TREntities.SecretLeadBar_M_H,
            [TRLevelNames.COLOSSEUM]
                = TREntities.SecretLeadBar_M_H,
            [TRLevelNames.MIDAS]
                = TREntities.SecretAnkh_M_H,
            [TRLevelNames.CISTERN]
                = TREntities.SecretScion_M_H,
            [TRLevelNames.TIHOCAN]
                = TREntities.SecretGoldIdol_M_H,
            [TRLevelNames.KHAMOON]
                = TREntities.SecretLeadBar_M_H,
            [TRLevelNames.OBELISK]
                = TREntities.SecretLeadBar_M_H,
            [TRLevelNames.SANCTUARY]
                = TREntities.SecretAnkh_M_H,
            [TRLevelNames.MINES]
                = TREntities.SecretGoldBar_M_H,
            [TRLevelNames.ATLANTIS]
                = TREntities.SecretGoldIdol_M_H,
            [TRLevelNames.PYRAMID]
                = TREntities.SecretGoldIdol_M_H
        };
    }
}