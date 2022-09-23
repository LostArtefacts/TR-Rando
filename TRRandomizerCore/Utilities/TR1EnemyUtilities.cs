using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities
{
    public static class TR1EnemyUtilities
    {
        public static int GetEnemyAdjustmentCount(string lvlName)
        {
            if (_enemyAdjustmentCount.ContainsKey(lvlName))
            {
                return _enemyAdjustmentCount[lvlName];
            }
            return 0;
        }

        // Check if an enemy is restricted in any way
        public static bool IsEnemyRestricted(string lvlName, TREntities entity, RandoDifficulty difficulty = RandoDifficulty.DefaultOrNoRestrictions)
        {
            if (difficulty == RandoDifficulty.Default || difficulty == RandoDifficulty.DefaultOrNoRestrictions)
            {
                return (_restrictedEnemyZonesTechnical.ContainsKey(lvlName) && _restrictedEnemyZonesTechnical[lvlName].ContainsKey(entity)) ||
                    (_restrictedEnemyZonesDefault.ContainsKey(lvlName) && _restrictedEnemyZonesDefault[lvlName].ContainsKey(entity)) ||
                    _restrictedEnemyGameCountsTechnical.ContainsKey(entity) ||
                    _restrictedEnemyLevelCountsTechnical.ContainsKey(entity) ||
                    _restrictedEnemyLevelCountsDefault.ContainsKey(entity) ||
                    (GetRestrictedEnemyGroup(lvlName, TR1EntityUtilities.TranslateEntityAlias(entity), RandoDifficulty.DefaultOrNoRestrictions) != null);
            }

            return (_restrictedEnemyZonesTechnical.ContainsKey(lvlName) && _restrictedEnemyZonesTechnical[lvlName].ContainsKey(entity))
                || _restrictedEnemyLevelCountsTechnical.ContainsKey(entity);
        }

        // This returns a set of ALLOWED rooms
        public static Dictionary<TREntities, List<int>> GetRestrictedEnemyRooms(string lvlName, RandoDifficulty difficulty)
        {
            var technicallyAllowedRooms = GetRestrictedEnemyRooms(lvlName, _restrictedEnemyZonesTechnical);
            var multiDict = new List<Dictionary<TREntities, List<int>>>() { technicallyAllowedRooms };

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

        private static Dictionary<TREntities, List<int>> GetRestrictedEnemyRooms(string lvlName, Dictionary<string, Dictionary<TREntities, List<int>>> restrictions)
        {
            if (restrictions.ContainsKey(lvlName))
                return restrictions[lvlName];
            return null;
        }

        public static int GetRestrictedEnemyLevelCount(TREntities entity, RandoDifficulty difficulty)
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

        public static Dictionary<TREntities, List<string>> PrepareEnemyGameTracker(RandoDifficulty difficulty)
        {
            Dictionary<TREntities, List<string>> tracker = new Dictionary<TREntities, List<string>>();

            if (difficulty == RandoDifficulty.Default)
            {
                foreach (TREntities entity in _restrictedEnemyGameCountsDefault.Keys)
                {
                    tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsDefault[entity]));
                }
            }

            foreach (TREntities entity in _restrictedEnemyGameCountsTechnical.Keys)
            {
                tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsTechnical[entity]));
            }

            // Pre-populate required enemies
            foreach (string level in _requiredEnemies.Keys)
            {
                foreach (TREntities enemy in _requiredEnemies[level])
                {
                    if (tracker.ContainsKey(enemy))
                    {
                        tracker[enemy].Add(level);
                    }
                }
            }

            return tracker;
        }

        public static bool IsEnemySupported(string lvlName, TREntities entity, RandoDifficulty difficulty)
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

        private static bool IsEnemySupported(string lvlName, TREntities entity, Dictionary<string, List<TREntities>> dict)
        {
            if (dict.ContainsKey(lvlName))
            {
                // if the dictionaries contain the enemy, the enemy is NOT supported
                return !dict[lvlName].Contains(TR1EntityUtilities.TranslateEntityAlias(entity));
            }
            // all enemies are supported by default
            return true;
        }

        public static bool IsEnemyRequired(string lvlName, TREntities entity)
        {
            return _requiredEnemies.ContainsKey(lvlName) && _requiredEnemies[lvlName].Contains(entity);
        }

        public static List<TREntities> GetRequiredEnemies(string lvlName)
        {
            List<TREntities> entities = new List<TREntities>();
            if (_requiredEnemies.ContainsKey(lvlName))
            {
                entities.AddRange(_requiredEnemies[lvlName]);
            }
            return entities;
        }

        public static void SetEntityTriggers(TRLevel level, TREntity entity)
        {
            if (_oneShotEnemies.Contains((TREntities)entity.TypeID))
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

        public static EnemyDifficulty GetEnemyDifficulty(List<TREntity> enemyEntities)
        {
            if (enemyEntities.Count == 0)
            {
                return EnemyDifficulty.VeryEasy;
            }

            int weight = 0;
            foreach (TREntity enemyEntity in enemyEntities)
            {
                EnemyDifficulty enemyDifficulty = EnemyDifficulty.Medium;
                foreach (EnemyDifficulty difficulty in _enemyDifficulties.Keys)
                {
                    if (_enemyDifficulties[difficulty].Contains((TREntities)enemyEntity.TypeID))
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

        public static uint GetStartingAmmo(TREntities weaponType)
        {
            if (_startingAmmoToGive.ContainsKey(weaponType))
            {
                return _startingAmmoToGive[weaponType];
            }
            return 0;
        }

        public static Dictionary<TREntities, TREntities> GetAliasPriority(string lvlName, List<TREntities> importEntities)
        {
            Dictionary<TREntities, TREntities> priorities = new Dictionary<TREntities, TREntities>();

            bool trexPresent = importEntities.Contains(TREntities.TRex);
            bool adamPresent = importEntities.Contains(TREntities.Adam);

            if ((trexPresent || adamPresent) && (lvlName == TRLevelNames.SANCTUARY || lvlName == TRLevelNames.ATLANTIS))
            {
                // We have to override the scion pickup animation otherwise death-by-adam will cause the level to end.
                // Environment mods will deal with workarounds for the pickups.
                priorities[TREntities.LaraMiscAnim_H] = trexPresent ? TREntities.LaraMiscAnim_H_Valley : TREntities.LaraMiscAnim_H_Pyramid;
            }
            else
            {
                switch (lvlName)
                {
                    // Essential MiscAnims - e.g. they contain level end triggers or cinematics.
                    // ToQ pickup cinematic works with T-Rex, but not Torso
                    case TRLevelNames.QUALOPEC:
                        priorities[TREntities.LaraMiscAnim_H] = trexPresent ? TREntities.LaraMiscAnim_H_Valley : TREntities.LaraMiscAnim_H_Qualopec;
                        break;
                    case TRLevelNames.MIDAS:
                        priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Midas;
                        break;
                    case TRLevelNames.SANCTUARY:
                        priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Sanctuary;
                        break;
                    case TRLevelNames.ATLANTIS:
                        priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Atlantis;
                        break;

                    // Lara's specific deaths:
                    //    - Adam + LaraMiscAnim_H_Valley = a fairly wonky death
                    //    - TRex + LaraMiscAnim_H_Pyramid = a very wonky death
                    // So if both Adam and TRex are present, the TRex anim is chosen,
                    // otherwise it's their corresponding anim.
                    default:
                        if (trexPresent)
                        {
                            priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Valley;
                        }
                        else if (adamPresent)
                        {
                            priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Pyramid;
                        }
                        else
                        {
                            priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_General;
                        }
                        break;
                }
            }

            return priorities;
        }

        public static RestrictedEnemyGroup GetRestrictedEnemyGroup(string lvlName, TREntities entity, RandoDifficulty difficulty)
        {
            if (_restrictedEnemyGroupCounts.ContainsKey(lvlName))
            {
                foreach (RestrictedEnemyGroup group in _restrictedEnemyGroupCounts[lvlName])
                {
                    if (group.Enemies.Contains(entity) && (group.IsSealed || difficulty != RandoDifficulty.NoRestrictions))
                    {
                        return group;
                    }
                }
            }
            return null;
        }

        // We (can) restrict some enemies to specific rooms in levels.
        private static readonly Dictionary<string, Dictionary<TREntities, List<int>>> _restrictedEnemyZonesDefault;
        private static readonly Dictionary<string, Dictionary<TREntities, List<int>>> _restrictedEnemyZonesTechnical;

        // We (can) also limit the count per level for some, such as bosses.
        private static readonly Dictionary<TREntities, int> _restrictedEnemyLevelCountsTechnical = new Dictionary<TREntities, int>
        {
            [TREntities.Natla] = 1,
            [TREntities.SkateboardKid] = 1
        };

        private static readonly Dictionary<TREntities, int> _restrictedEnemyLevelCountsDefault = new Dictionary<TREntities, int>
        {
            [TREntities.Adam] = 1,
            [TREntities.Cowboy] = 3,
            [TREntities.Kold] = 3,
            [TREntities.Pierre] = 3
        };

        // These enemies are restricted a set number of times throughout the entire game.
        // Perhaps add level-ending larson as an option
        private static readonly Dictionary<TREntities, int> _restrictedEnemyGameCountsTechnical = new Dictionary<TREntities, int>
        {
        };

        private static readonly Dictionary<TREntities, int> _restrictedEnemyGameCountsDefault = new Dictionary<TREntities, int>
        {
            [TREntities.Adam] = 3,
            [TREntities.Natla] = 2
        };

        private static readonly List<TREntities> _allAtlanteans = new List<TREntities>
        {
            TREntities.Adam, TREntities.Centaur, TREntities.CentaurStatue, TREntities.FlyingAtlantean,
            TREntities.NonShootingAtlantean_N, TREntities.ShootingAtlantean_N, TREntities.Natla,
            TREntities.AtlanteanEgg
        };

        private static readonly RestrictedEnemyGroup _natlaAndSkater = new RestrictedEnemyGroup
        {
            MaximumCount = 1,
            IsSealed = true, // This cannot be switched off in "no restrictions"
            Enemies = new List<TREntities>
            {
                TREntities.Natla, TREntities.SkateboardKid
            }
        };

        private static readonly Dictionary<string, List<RestrictedEnemyGroup>> _restrictedEnemyGroupCounts = new Dictionary<string, List<RestrictedEnemyGroup>>
        {
            [TRLevelNames.CAVES] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 5,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.VILCABAMBA] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 10,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.VALLEY] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 5,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.QUALOPEC] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 0,
                    Enemies = new List<TREntities> { TREntities.Larson }
                },
                new RestrictedEnemyGroup
                {
                    MaximumCount = 3,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.FOLLY] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 10,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.COLOSSEUM] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 11,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.MIDAS] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 15,
                    Enemies = _allAtlanteans
                },
                new RestrictedEnemyGroup
                {
                    MaximumCount = 10,
                    Enemies = new List<TREntities> { TREntities.TRex }
                }
            },
            [TRLevelNames.CISTERN] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 13,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.TIHOCAN] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 9,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.KHAMOON] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 6,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.OBELISK] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater,
                new RestrictedEnemyGroup
                {
                    MaximumCount = 6,
                    Enemies = _allAtlanteans
                }
            },
            [TRLevelNames.SANCTUARY] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater
            },
            [TRLevelNames.MINES] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater
            },
            [TRLevelNames.ATLANTIS] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater
            },
            [TRLevelNames.PYRAMID] = new List<RestrictedEnemyGroup>
            {
                _natlaAndSkater
            }
        };

        // These enemies are unsupported due to technical reasons, NOT difficulty reasons.
        private static readonly Dictionary<string, List<TREntities>> _unsupportedEnemiesTechnical = new Dictionary<string, List<TREntities>>
        {
        };

        private static readonly Dictionary<string, List<TREntities>> _unsupportedEnemiesDefault = new Dictionary<string, List<TREntities>>
        {
            [TRLevelNames.QUALOPEC] = new List<TREntities>
            {
                TREntities.Adam
            }
        };

        // Any enemies that must remain untouched in a given level
        private static readonly Dictionary<string, List<TREntities>> _requiredEnemies = new Dictionary<string, List<TREntities>>
        {
            [TRLevelNames.QUALOPEC] = new List<TREntities>
            {
                TREntities.Larson // Ends the level
            },
            [TRLevelNames.PYRAMID] = new List<TREntities>
            {
                TREntities.Adam // Heavy trigger
            }
        };

        // Control the number of types of enemy that appear in levels, so these numbers are added to the
        // current total e.g. Caves becomes 5 types, Vilcabamba becomes 4 etc.
        private static readonly Dictionary<string, int> _enemyAdjustmentCount = new Dictionary<string, int>
        {
            [TRLevelNames.CAVES]
                = 2, // Defaults: 3 types, 14 enemies
            [TRLevelNames.VILCABAMBA]
                = 1, // Defaults: 3 types, 29 enemies
            [TRLevelNames.VALLEY]
                = 2, // Defaults: 3 types, 13 enemies
            [TRLevelNames.QUALOPEC]
                = 0, // Defaults: 3 types, 7 enemies
            [TRLevelNames.FOLLY]
                = 0, // Defaults: 6 types, 25 enemies
            [TRLevelNames.COLOSSEUM]
                = 0, // Defaults: 7 types, 29 enemies
            [TRLevelNames.MIDAS]
                = 0, // Defaults: 6 types, 43 enemies
            [TRLevelNames.CISTERN]
                = 0, // Defaults: 8 types, 37 enemies
            [TRLevelNames.TIHOCAN]
                = 0, // Defaults: 7 types, 18 enemies
            [TRLevelNames.KHAMOON]
                = 1, // Defaults: 4 types, 14 enemies
            [TRLevelNames.OBELISK]
                = 1, // Defaults: 3 types, 16 enemies
            [TRLevelNames.SANCTUARY]
                = 0, // Defaults: 5 types, 15 enemies
            [TRLevelNames.MINES]
                = 0, // Defaults: 3 types, 3 enemies
            [TRLevelNames.ATLANTIS]
                = 1, // Defaults: 5 types, 32 enemies
            [TRLevelNames.PYRAMID]
                = 0  // Defaults: 2 types, 4 enemies
        };

        // Enemies who can only spawn once.
        private static readonly List<TREntities> _oneShotEnemies = new List<TREntities>
        {
            TREntities.Pierre
        };

        private static readonly Dictionary<EnemyDifficulty, List<TREntities>> _enemyDifficulties = new Dictionary<EnemyDifficulty, List<TREntities>>
        {
            [EnemyDifficulty.VeryEasy] = new List<TREntities>
            {
                TREntities.Bat
            },
            [EnemyDifficulty.Easy] = new List<TREntities>
            {
                TREntities.Bear, TREntities.Gorilla, TREntities.RatLand, TREntities.RatWater,
                TREntities.Wolf
            },
            [EnemyDifficulty.Medium] = new List<TREntities>
            {
                TREntities.Cowboy, TREntities.Raptor, TREntities.Lion, TREntities.Lioness,
                TREntities.Panther,TREntities.CrocodileLand, TREntities.CrocodileWater, TREntities.Pierre,
                TREntities.Larson
            },
            [EnemyDifficulty.Hard] = new List<TREntities>
            {
                TREntities.TRex, TREntities.SkateboardKid, TREntities.Kold, TREntities.Centaur,
                TREntities.CentaurStatue, TREntities.FlyingAtlantean, TREntities.ShootingAtlantean_N, TREntities.NonShootingAtlantean_N
            },
            [EnemyDifficulty.VeryHard] = new List<TREntities>
            {
                TREntities.Adam, TREntities.Natla
            }
        };

        private static readonly Dictionary<TREntities, uint> _startingAmmoToGive = new Dictionary<TREntities, uint>()
        {
            [TREntities.Shotgun_S_P] = 10,
            [TREntities.Magnums_S_P] = 6,
            [TREntities.Uzis_S_P] = 6
        };

        static TR1EnemyUtilities()
        {
            _restrictedEnemyZonesDefault = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TREntities, List<int>>>>
            (
                File.ReadAllText(@"Resources\TR1\Restrictions\enemy_restrictions_default.json")
            );
            _restrictedEnemyZonesTechnical = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TREntities, List<int>>>>
            (
                File.ReadAllText(@"Resources\TR1\Restrictions\enemy_restrictions_technical.json")
            );
        }
    }

    public class RestrictedEnemyGroup
    {
        public List<TREntities> Enemies { get; set; }
        public int MaximumCount { get; set; }
        public bool IsSealed { get; set; }
    }
}