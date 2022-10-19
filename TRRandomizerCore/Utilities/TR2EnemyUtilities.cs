using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRRandomizerCore.Utilities
{
    public static class TR2EnemyUtilities
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

        // Similar to above, but allows for a collection to be resized if a specific
        // enemy model is chosen.
        public static int GetTargetEnemyAdjustmentCount(string lvlName, TR2Entities enemy)
        {
            if (_targetEnemyAdjustmentCount.ContainsKey(enemy) && _targetEnemyAdjustmentCount[enemy].ContainsKey(lvlName))
            {
                return _targetEnemyAdjustmentCount[enemy][lvlName];
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
            TR2Entity[] enemies = Array.FindAll(level.Data.Entities, e => TR2EntityUtilities.IsEnemyType((TR2Entities)e.TypeID));
            foreach (TR2Entity entityInstance in enemies)
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

        public static bool IsEnemySupported(string lvlName, TR2Entities entity, RandoDifficulty difficulty)
        {
            bool isEnemyTechnicallySupported = IsEnemySupported(lvlName, entity, _unsupportedEnemiesTechnical);
            bool isEnemySupported = isEnemyTechnicallySupported;

            if (difficulty == RandoDifficulty.Default)
            {
                bool isEnemyDefaultSupported = IsEnemySupported(lvlName, entity, _unsupportedEnemiesDefault);

                // a level may exist in both technical and difficulty dicts, so we check both
                isEnemySupported &= isEnemyDefaultSupported;
            }

            return isEnemySupported;
        }
        private static bool IsEnemySupported(string lvlName, TR2Entities entity, Dictionary<string, List<TR2Entities>> dict)
        {
            if (dict.ContainsKey(lvlName))
            {
                // if the dictionaries contain the enemy, the enemy is NOT supported
                return !dict[lvlName].Contains(TR2EntityUtilities.TranslateEntityAlias(entity));
            }
            // all enemies are supported by default
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

        // this returns a set of ALLOWED rooms
        public static Dictionary<TR2Entities, List<int>> GetRestrictedEnemyRooms(string lvlName, RandoDifficulty difficulty)
        {
            var technicallyAllowedRooms = GetRestrictedEnemyRooms(lvlName, _restrictedEnemyZonesTechnical);
            var multiDict = new List<Dictionary<TR2Entities, List<int>>>() { technicallyAllowedRooms };

            // we need to merge dictionaries in order to get the complete set of allowed rooms, per level and per enemy
            if (difficulty == RandoDifficulty.Default)
            {
                multiDict.Add(GetRestrictedEnemyRooms(lvlName, _restrictedEnemyZonesDefault));
                return multiDict.Where(dict => dict != null)
                                .SelectMany(dict => dict)
                                .ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            else if (difficulty == RandoDifficulty.NoRestrictions)
                return technicallyAllowedRooms;
            return null;
        }
        private static Dictionary<TR2Entities, List<int>> GetRestrictedEnemyRooms(string lvlName, Dictionary<string, Dictionary<TR2Entities, List<int>>> restrictions)
        {
            if (restrictions.ContainsKey(lvlName))
                return restrictions[lvlName];
            return null;
        }

        public static int GetRestrictedEnemyLevelCount(TR2Entities entity, RandoDifficulty difficulty)
        {
            // Remember that technical count is MAXIMUM allowed, and there may be overlap.
            // For example, maybe technically Dragon is allowed once, but an Easy difficulty might have that set to 0.
            // So we check difficulties first, then check technical last.
            if (difficulty == RandoDifficulty.Default)
            {
                if (_restrictedEnemyLevelCountsDefault.ContainsKey(entity))
                    return _restrictedEnemyLevelCountsDefault[entity];
            }

            if (_restrictedEnemyLevelCountsTechnical.ContainsKey(entity))
                return _restrictedEnemyLevelCountsTechnical[entity];

            return -1;
        }

        public static int GetRestrictedEnemyTotalTypeCount(RandoDifficulty difficulty)
        {
            if (difficulty == RandoDifficulty.Default)
            {
                return _restrictedEnemyLevelCountsDefault.Count;
            }

            return _restrictedEnemyLevelCountsTechnical.Count;
        }

        public static List<List<TR2Entities>> GetPermittedCombinations(string lvl, TR2Entities entity, RandoDifficulty difficulty)
        {
            if (_specialEnemyCombinations.ContainsKey(lvl) && _specialEnemyCombinations[lvl].ContainsKey(entity))
            {
                if (_specialEnemyCombinations[lvl][entity].ContainsKey(difficulty))
                {
                    return _specialEnemyCombinations[lvl][entity][difficulty];
                }
                else if (_specialEnemyCombinations[lvl][entity].ContainsKey(RandoDifficulty.DefaultOrNoRestrictions))
                {
                    return _specialEnemyCombinations[lvl][entity][RandoDifficulty.DefaultOrNoRestrictions];
                }
            }
            return null;
        }

        public static Dictionary<TR2Entities, List<string>> PrepareEnemyGameTracker(bool docileBirdMonster, RandoDifficulty difficulty)
        {
            Dictionary<TR2Entities, List<string>> tracker = new Dictionary<TR2Entities, List<string>>();

            if (difficulty == RandoDifficulty.Default)
            {
                foreach (TR2Entities entity in _restrictedEnemyGameCountsDefault.Keys)
                {
                    if (!docileBirdMonster || entity != TR2Entities.BirdMonster)
                    {
                        tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsDefault[entity]));
                    }
                }
            }
            foreach (TR2Entities entity in _restrictedEnemyGameCountsTechnical.Keys)
            {
                if (!docileBirdMonster || entity != TR2Entities.BirdMonster)
                {
                    tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsTechnical[entity]));
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

        // These enemies are unsupported due to technical reasons, NOT difficulty reasons.
        private static readonly Dictionary<string, List<TR2Entities>> _unsupportedEnemiesTechnical = new Dictionary<string, List<TR2Entities>>
        {
            // #192 The Barkhang/Opera House freeze appears to be caused by dead floating water creatures, so they're all banished
            [TR2LevelNames.OPERA] =
                new List<TR2Entities>
                {
                    TR2Entities.Barracuda, TR2Entities.BlackMorayEel, TR2Entities.ScubaDiver,
                    TR2Entities.Shark, TR2Entities.YellowMorayEel
                },
            [TR2LevelNames.MONASTERY] =
                new List<TR2Entities>
                {
                    TR2Entities.Barracuda, TR2Entities.BlackMorayEel, TR2Entities.ScubaDiver,
                    TR2Entities.Shark, TR2Entities.YellowMorayEel
                },
            [TR2LevelNames.HOME] =
                // #148 Although we say here that the Doberman, MaskedGoons and StickGoons
                // aren't supported, this is only for cross-level purposes because we
                // are making placeholder entities to prevent breaking the kill counter.
                new List<TR2Entities>
                {
                    TR2Entities.BlackMorayEel, TR2Entities.Doberman, TR2Entities.MaskedGoon1,
                    TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.MercSnowmobDriver,
                    TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick, TR2Entities.StickWieldingGoon1,
                    TR2Entities.StickWieldingGoon2, TR2Entities.Winston, TR2Entities.YellowMorayEel, TR2Entities.ShotgunGoon
                }
        };

        private static readonly Dictionary<string, List<TR2Entities>> _unsupportedEnemiesDefault = new Dictionary<string, List<TR2Entities>>
        {
            [TR2LevelNames.LAIR] =
                new List<TR2Entities> { TR2Entities.MercSnowmobDriver },
            [TR2LevelNames.HOME] =
                new List<TR2Entities> { TR2Entities.Spider, TR2Entities.Rat }
        };

        private static readonly Dictionary<string, List<TR2Entities>> _requiredEnemies = new Dictionary<string, List<TR2Entities>>
        {
            [TR2LevelNames.CHICKEN] =
                new List<TR2Entities> { TR2Entities.BirdMonster },  // #60 - Ice Palace chicken man must remain to avoid softlock.
            [TR2LevelNames.LAIR] =
                new List<TR2Entities> { TR2Entities.MarcoBartoli }, // #97 - Marco/Dragon to remain in the same place to trigger door opening
            [TR2LevelNames.HOME] =
                new List<TR2Entities> { TR2Entities.ShotgunGoon }  // #62 - Avoid randomizing shotgun goon in HSH
        };

        // We restrict some enemies to specific rooms in levels, for example the dragon does not work well in small
        // rooms, and the likes of SnowmobDriver at the beginning of Bartoli's is practically impossible to pass.
        private static readonly Dictionary<string, Dictionary<TR2Entities, List<int>>> _restrictedEnemyZonesDefault;
        private static readonly Dictionary<string, Dictionary<TR2Entities, List<int>>> _restrictedEnemyZonesTechnical;

        // This allows us to define specific combinations of enemies if the leader is chosen for the rando pool. For
        // example, the dragon in HSH will only work with 20 possible combinations.
        private static readonly Dictionary<string, Dictionary<TR2Entities, Dictionary<RandoDifficulty, List<List<TR2Entities>>>>> _specialEnemyCombinations;

        // We also limit the count for some - more than 1 dragon tends to cause crashes if they spawn close together.
        // Winston is an easter egg so maybe keep it low.
        private static readonly Dictionary<TR2Entities, int> _restrictedEnemyLevelCountsTechnical = new Dictionary<TR2Entities, int>
        {
            [TR2Entities.MarcoBartoli] = 1,
            [TR2Entities.Winston] = 2
        };
        private static readonly Dictionary<TR2Entities, int> _restrictedEnemyLevelCountsDefault = new Dictionary<TR2Entities, int>
        {
            [TR2Entities.MercSnowmobDriver] = 2,
        };

        // These enemies are restricted a set number of times throughout the entire game.
        private static readonly Dictionary<TR2Entities, int> _restrictedEnemyGameCountsTechnical = new Dictionary<TR2Entities, int>
        {
            [TR2Entities.Winston] = 2,
        };
        private static readonly Dictionary<TR2Entities, int> _restrictedEnemyGameCountsDefault = new Dictionary<TR2Entities, int>
        {
            [TR2Entities.BirdMonster] = 3,
        };

        // Predefined absolute limits for skidoo drivers
        private static readonly Dictionary<string, int> _skidooLimits = new Dictionary<string, int>
        {
            [TR2LevelNames.OPERA] = 18,
            [TR2LevelNames.MONASTERY] = 22,
            [TR2LevelNames.XIAN] = 10
        };

        public static int GetSkidooDriverLimit(string lvl)
        {
            return _skidooLimits.ContainsKey(lvl) ? _skidooLimits[lvl] : -1;
        }

        static TR2EnemyUtilities()
        {
            _restrictedEnemyZonesDefault = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR2Entities, List<int>>>>
            (
                File.ReadAllText(@"Resources\TR2\Restrictions\enemy_restrictions_default.json")
            );
            _restrictedEnemyZonesTechnical = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR2Entities, List<int>>>>
            (
                File.ReadAllText(@"Resources\TR2\Restrictions\enemy_restrictions_technical.json")
            );
            _specialEnemyCombinations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR2Entities, Dictionary<RandoDifficulty, List<List<TR2Entities>>>>>>
            (
                File.ReadAllText(@"Resources\TR2\Restrictions\enemy_restrictions_special.json")
            );
        }

        private static readonly Dictionary<EnemyDifficulty, List<TR2Entities>> _enemyDifficulties = new Dictionary<EnemyDifficulty, List<TR2Entities>>
        {
            [EnemyDifficulty.VeryEasy] = new List<TR2Entities>
            {
                TR2Entities.Barracuda, TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick,
                TR2Entities.Rat, TR2Entities.Spider, TR2Entities.Winston
            },
            [EnemyDifficulty.Easy] = new List<TR2Entities>
            {
                TR2Entities.Crow, TR2Entities.Eagle, TR2Entities.ScubaDiver,
                TR2Entities.YellowMorayEel
            },
            [EnemyDifficulty.Medium] = new List<TR2Entities>
            {
                TR2Entities.Doberman, TR2Entities.GiantSpider, TR2Entities.Gunman1,
                TR2Entities.Gunman2, TR2Entities.Knifethrower, TR2Entities.MaskedGoon1,
                TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Shark,
                TR2Entities.StickWieldingGoon1, TR2Entities.StickWieldingGoon2, TR2Entities.TigerOrSnowLeopard,
                TR2Entities.TRex
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
            [TR2LevelNames.GW] = 2,
            [TR2LevelNames.OPERA] = -1,
            [TR2LevelNames.DA] = -1,
            [TR2LevelNames.FATHOMS] = 1,
            [TR2LevelNames.TIBET] = 1,
            [TR2LevelNames.CHICKEN] = 1,
            [TR2LevelNames.FLOATER] = 1,
            [TR2LevelNames.LAIR] = 1
        };

        // Trigger a redim of the imported enemy count if one of these entities is selected
        private static readonly Dictionary<TR2Entities, Dictionary<string, int>> _targetEnemyAdjustmentCount = new Dictionary<TR2Entities, Dictionary<string, int>>
        {
            [TR2Entities.MarcoBartoli] = new Dictionary<string, int>
            {
                [TR2LevelNames.VENICE] = -2,
                [TR2LevelNames.BARTOLI] = -2,
                [TR2LevelNames.OPERA] = -2,
                [TR2LevelNames.DA] = -1,
                [TR2LevelNames.TIBET] = -1,
                [TR2LevelNames.FLOATER] = -1,
            }
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

        public static List<TR2Entities> GetFriendlyEnemies()
        {
            return new List<TR2Entities>(_friendlyEnemies);
        }

        private static readonly List<TR2Entities> _friendlyEnemies = new List<TR2Entities>
        {
            TR2Entities.Winston, TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick
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
                    trigger.TrigSetup.OneShot = true;
                }

                fdControl.WriteToLevel(level);
            }
        }

        public static Dictionary<TR2Entities, TR2Entities> GetAliasPriority(string lvlName, List<TR2Entities> importEntities)
        {
            // If the priorities map doesn't contain an entity we are trying to import as a key, TRModelTransporter
            // will assume it always has priority (e.g. BengalTiger replacing SnowLeopard).
            Dictionary<TR2Entities, TR2Entities> priorities = new Dictionary<TR2Entities, TR2Entities>();

            // If the dragon is being imported, we want the matching dagger cutscene to be available via misc anim
            // and the dagger model for the inventory. Otherwise, we need to ensure the existing misc anim matches
            // level specifics. This is currently targeted at offshore wheel door levels, Ice Palace gong action and
            // HSH starting cutscene. For others we sacrifice the specific enemy death animations. So for example,
            // if XianGuardSpear is imported into Wreck, the existing misc anim will remain for the wheel door animation.
            // But if Marco is imported, the wheel door animation will also be sacrificed.
            if (importEntities.Contains(TR2Entities.MarcoBartoli) && lvlName != TR2LevelNames.HOME)
            {
                priorities[TR2Entities.Puzzle2_M_H] = TR2Entities.Puzzle2_M_H_Dagger;
                priorities[TR2Entities.LaraMiscAnim_H] = TR2Entities.LaraMiscAnim_H_Xian;
            }
            else
            {
                switch (lvlName)
                {
                    case TR2LevelNames.BARTOLI:
                        priorities[TR2Entities.LaraMiscAnim_H] = TR2Entities.LaraMiscAnim_H_Venice;
                        break;
                    case TR2LevelNames.RIG:
                    case TR2LevelNames.DA:
                    case TR2LevelNames.DORIA:
                        priorities[TR2Entities.LaraMiscAnim_H] = TR2Entities.LaraMiscAnim_H_Unwater;
                        break;
                    case TR2LevelNames.CHICKEN:
                        priorities[TR2Entities.LaraMiscAnim_H] = TR2Entities.LaraMiscAnim_H_Ice;
                        break;
                    case TR2LevelNames.HOME:
                        priorities[TR2Entities.LaraMiscAnim_H] = TR2Entities.LaraMiscAnim_H_HSH;
                        break;
                }
            }

            return priorities;
        }
    }
}