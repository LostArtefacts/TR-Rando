using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2RandomizerCore.Utilities
{
    public static class EnemyUtilities
    {
        // This allows us to alter the default number of enemy types per level
        // given that some can support many more but others have difficulty 
        // when it comes to cross-level texture packing. The return value is
        // a signed int and should be used to adjust the current count.
        public static int GetEnemyAdjustmentCount(string lvlName)
        {
            if (_enemyAdjustmentCount.ContainsKey(lvlName))
            {
                return _enemyAdjustmentCount[lvlName];
            }
            return 0;
        }

        public static bool IsWaterEnemyRequired(TR2CombinedLevel level)
        {
            foreach (TR2Entity entityInstance in level.Data.Entities)
            {
                TR2Entities entity = (TR2Entities)entityInstance.TypeID;
                if (TR2EntityUtilities.IsWaterCreature(entity))
                {
                    if (!level.CanPerformDraining(entityInstance.Room))
                    {
                        // Draining cannot be performed so we need to ensure we get at least one water enemy
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsDroppableEnemyRequired(TR2CombinedLevel level)
        {
            foreach (TR2Entity entityInstance in level.Data.Entities)
            {
                List<TR2Entity> sharedItems = new List<TR2Entity>(Array.FindAll
                (
                    level.Data.Entities,
                    e =>
                    (
                        e.X == entityInstance.X &&
                        e.Y == entityInstance.Y &&
                        e.Z == entityInstance.Z
                    )
                ));
                if (sharedItems.Count > 1)
                {
                    // Are any entities that are sharing a location a droppable pickup?
                    foreach (TR2Entity ent in sharedItems)
                    {
                        TR2Entities EntType = (TR2Entities)ent.TypeID;

                        if
                        (
                            TR2EntityUtilities.IsUtilityType(EntType) ||
                            TR2EntityUtilities.IsGunType(EntType) ||
                            TR2EntityUtilities.IsKeyItemType(EntType)
                        )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsEnemySupported(string lvlName, TR2Entities entity)
        {
            if (_unsupportedEnemies.ContainsKey(lvlName))
            {
                return !_unsupportedEnemies[lvlName].Contains(TR2EntityUtilities.TranslateEntityAlias(entity));
            }

            return true;
        }

        public static bool IsEnemyRequired(string lvlName, TR2Entities entity)
        {
            return _requiredEnemies.ContainsKey(lvlName) && _requiredEnemies[lvlName].Contains(entity);
        }

        public static List<TR2Entities> GetRequiredEnemies(string lvlName)
        {
            List<TR2Entities> entities = new List<TR2Entities>();
            if (_requiredEnemies.ContainsKey(lvlName))
            {
                entities.AddRange(_requiredEnemies[lvlName]);
            }
            return entities;
        }

        public static List<TR2Entities> GetRestrictedEnemies(string lvlName)
        {
            List<TR2Entities> enemies = new List<TR2Entities>();
            if (_restrictedEnemyZones.ContainsKey(lvlName))
            {
                enemies.AddRange(_restrictedEnemyZones[lvlName].Keys);
            }
            return enemies;
        }

        public static Dictionary<TR2Entities, List<int>> GetRestrictedEnemyRooms(string lvlName)
        {
            if (_restrictedEnemyZones.ContainsKey(lvlName))
            {
                return _restrictedEnemyZones[lvlName];
            }
            return null;
        }

        public static int GetRestrictedEnemyLevelCount(TR2Entities entity)
        {
            if (_restrictedEnemyLevelCounts.ContainsKey(entity))
            {
                return _restrictedEnemyLevelCounts[entity];
            }
            return -1;
        }

        public static Dictionary<TR2Entities, List<string>> PrepareEnemyGameTracker(bool docileBirdMonster)
        {
            Dictionary<TR2Entities, List<string>> tracker = new Dictionary<TR2Entities, List<string>>();
            foreach (TR2Entities entity in _restrictedEnemyGameCounts.Keys)
            {
                if (!docileBirdMonster || entity != TR2Entities.BirdMonster)
                {
                    tracker.Add(entity, new List<string>(_restrictedEnemyGameCounts[entity]));
                }
            }

            // Pre-populate required enemies
            foreach (string level in _requiredEnemies.Keys)
            {
                foreach (TR2Entities enemy in _requiredEnemies[level])
                {
                    if (tracker.ContainsKey(enemy))
                    {
                        tracker[enemy].Add(level);
                    }
                }
            }

            return tracker;
        }

        public static EnemyDifficulty GetEnemyDifficulty(List<TR2Entity> enemies)
        {
            if (enemies.Count == 0)
            {
                return EnemyDifficulty.VeryEasy;
            }

            ISet<TR2Entities> enemyEntities = new HashSet<TR2Entities>();
            enemies.ForEach(e => enemyEntities.Add((TR2Entities)e.TypeID));

            int weight = 0;
            foreach (TR2Entities enemyEntity in enemyEntities)
            {
                EnemyDifficulty enemyDifficulty = EnemyDifficulty.Medium;
                foreach (EnemyDifficulty difficulty in _enemyDifficulties.Keys)
                {
                    if (_enemyDifficulties[difficulty].Contains(enemyEntity))
                    {
                        enemyDifficulty = difficulty;
                        break;
                    }
                }
                weight += (int)enemyDifficulty;
            }

            // What's the average?
            double average = (double)weight / enemyEntities.Count;
            weight = Convert.ToInt32(Math.Round(average, 0, MidpointRounding.AwayFromZero));

            List<EnemyDifficulty> allDifficulties = new List<EnemyDifficulty>(Enum.GetValues(typeof(EnemyDifficulty)).Cast<EnemyDifficulty>());

            if (weight > 0)
            {
                weight--;
            }

            if (weight >= allDifficulties.Count)
            {
                weight = allDifficulties.Count - 1;
            }

            return allDifficulties[weight];
        }

        // These enemies will either not fit into the given levels, or there is no suitable
        // room in the levels to support them, or for HSH they can't be killed or are too
        // awkward, such as the small spiders.
        private static readonly Dictionary<string, List<TR2Entities>> _unsupportedEnemies = new Dictionary<string, List<TR2Entities>>
        {
            [LevelNames.VENICE] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.BARTOLI] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.RIG] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.DA] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.FATHOMS] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.LQ] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.MONASTERY] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.CHICKEN] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.XIAN] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.FLOATER] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli },
            [LevelNames.LAIR] =
                new List<TR2Entities> { TR2Entities.MercSnowmobDriver },
            [LevelNames.HOME] =
                // #148 Although we say here that the Doberman, MaskedGoons and StickGoons
                // aren't supported, this is only for cross-level purposes because we
                // are making placeholder entities to prevent breaking the kill counter.
                new List<TR2Entities>
                {
                    TR2Entities.BlackMorayEel, TR2Entities.Doberman, TR2Entities.Eagle, TR2Entities.MaskedGoon1, 
                    TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.MarcoBartoli, TR2Entities.MercSnowmobDriver, 
                    TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick, TR2Entities.Shark, TR2Entities.StickWieldingGoon1, 
                    TR2Entities.StickWieldingGoon2, TR2Entities.Spider, TR2Entities.TRex, TR2Entities.YellowMorayEel
                }
        };
        
        private static readonly Dictionary<string, List<TR2Entities>> _requiredEnemies = new Dictionary<string, List<TR2Entities>>
        {
            [LevelNames.MONASTERY] =
                new List<TR2Entities> { TR2Entities.Mercenary1, TR2Entities.Mercenary2, TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick }, // #157 Barkhang entity freezing issues
            [LevelNames.CHICKEN] =
                new List<TR2Entities> { TR2Entities.BirdMonster },  // #60 - Ice Palace chicken man must remain to avoid softlock.
            [LevelNames.LAIR] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli }, // #97 - Marco/Dragon to remain in the same place to trigger door opening
            [LevelNames.HOME] =
                new List<TR2Entities> { TR2Entities.ShotgunGoon }  // #62 - Avoid randomizing shotgun goon in HSH
        };

        // We restrict some enemies to specific rooms in levels, for example the dragon does not work well in small
        // rooms, and the likes of SnowmobDriver at the beginning of Bartoli's is practically impossible to pass.
        private static readonly Dictionary<string, Dictionary<TR2Entities, List<int>>> _restrictedEnemyZones;

        // We also limit the count for some - more than 1 dragon tends to cause crashes if they spawn close together. For
        // others, perhaps once a difficulty option is implemented this can be adjusted.
        private static readonly Dictionary<TR2Entities, int> _restrictedEnemyLevelCounts = new Dictionary<TR2Entities, int>
        {
            [TR2Entities.MarcoBartoli] = 1,
            [TR2Entities.MercSnowmobDriver] = 2
        };

        // We restrict the chicken to appearing 3 times throughout the game
        private static readonly Dictionary<TR2Entities, int> _restrictedEnemyGameCounts = new Dictionary<TR2Entities, int>
        {
            [TR2Entities.BirdMonster] = 3
        };

        static EnemyUtilities()
        {
            _restrictedEnemyZones = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR2Entities, List<int>>>>
            (
                File.ReadAllText(@"Resources\enemy_restrictions.json")
            );
        }

        private static readonly Dictionary<EnemyDifficulty, List<TR2Entities>> _enemyDifficulties = new Dictionary<EnemyDifficulty, List<TR2Entities>>
        {
            [EnemyDifficulty.VeryEasy] = new List<TR2Entities>
            {
                TR2Entities.Barracuda, TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick,
                TR2Entities.Rat, TR2Entities.Spider
            },
            [EnemyDifficulty.Easy] = new List<TR2Entities>
            {
                TR2Entities.Crow, TR2Entities.Eagle, TR2Entities.ScubaDiver,
                TR2Entities.TRex, TR2Entities.YellowMorayEel
            },
            [EnemyDifficulty.Medium] = new List<TR2Entities>
            {
                TR2Entities.Doberman, TR2Entities.GiantSpider, TR2Entities.Gunman1, 
                TR2Entities.Gunman2, TR2Entities.Knifethrower, TR2Entities.MaskedGoon1, 
                TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Shark, 
                TR2Entities.StickWieldingGoon1, TR2Entities.StickWieldingGoon2, TR2Entities.TigerOrSnowLeopard
            },
            [EnemyDifficulty.Hard] = new List<TR2Entities>
            {
                TR2Entities.BlackMorayEel, TR2Entities.FlamethrowerGoon, TR2Entities.Mercenary1,
                TR2Entities.Mercenary2, TR2Entities.Mercenary3, TR2Entities.ShotgunGoon,
                TR2Entities.XianGuardSpear, TR2Entities.XianGuardSword, TR2Entities.Yeti
            },
            [EnemyDifficulty.VeryHard] = new List<TR2Entities>
            {
                TR2Entities.BirdMonster, TR2Entities.MarcoBartoli, TR2Entities.MercSnowmobDriver
            }
        };

        /**
         * This is based loosely on the number of used tiles and the object texture count of each level.
         * LVL : CurrentEnemies UsedTiles   TextureCount
         * ---------------------------------------------
         * WALL: 4, 11, 1357
         * VENI: 6, 15, 1730
         * BART: 6, 16, 1775
         * OPER: 7, 15, 1898
         * RIG : 5, 15, 1748
         * PLAT: 6, 16, 2022
         * FATH: 5, 11, 1492
         * KEEL: 7, 15, 1896
         * LQRT: 6, 13, 1702
         * DECK: 6, 14, 1734
         * SKID: 5, 12, 1510
         * BARK: 5, 16, 1822
         * CATA: 5, 13, 1473
         * ICEP: 4, 14, 1576
         * XIAN: 5, 16, 1745
         * FLOA: 3, 15, 1878
         * LAIR: 3, 12, 1517
         * HSH : N/A
         */
        private static readonly Dictionary<string, int> _enemyAdjustmentCount = new Dictionary<string, int>
        {
            [LevelNames.GW] = 2,
            [LevelNames.OPERA] = -1,
            [LevelNames.DA] = -1,
            [LevelNames.FATHOMS] = 1,
            [LevelNames.TIBET] = 1,
            [LevelNames.CHICKEN] = 1,
            [LevelNames.FLOATER] = 1,
            [LevelNames.LAIR] = 1
        };

        public static List<TR2Entities> GetEnemyGuisers(TR2Entities entity)
        {
            List<TR2Entities> entities = new List<TR2Entities>();
            if (_enemyGuisers.ContainsKey(entity))
            {
                entities.AddRange(_enemyGuisers[entity]);
            }
            return entities;
        }

        private static readonly Dictionary<TR2Entities, List<TR2Entities>> _enemyGuisers = new Dictionary<TR2Entities, List<TR2Entities>>
        {
            [TR2Entities.BirdMonster] = new List<TR2Entities>
            {
                TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick
            }
        };

        // #146 Ensure Marco is spawned only once
        private static readonly List<TR2Entities> _oneShotEnemies = new List<TR2Entities>
        {
            TR2Entities.MarcoBartoli
        };

        public static void SetEntityTriggers(TR2Level level, TR2Entity entity)
        {
            if (_oneShotEnemies.Contains((TR2Entities)entity.TypeID))
            {
                int entityID = level.Entities.ToList().IndexOf(entity);

                FDControl fdControl = new FDControl();
                fdControl.ParseFromLevel(level);

                List<FDTriggerEntry> triggers = FDUtilities.GetEntityTriggers(fdControl, entityID);
                foreach (FDTriggerEntry trigger in triggers)
                {
                    trigger.TrigSetup.SetOneShot();
                }

                fdControl.WriteToLevel(level);
            }
        }
    }
}