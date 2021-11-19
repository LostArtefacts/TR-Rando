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
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Utilities
{
    public static class TR3EnemyUtilities
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
        public static bool IsEnemyRestricted(string lvlName, TR3Entities entity)
        {
            return (_restrictedEnemyZonesTechnical.ContainsKey(lvlName) && _restrictedEnemyZonesTechnical[lvlName].ContainsKey(entity)) ||
                (_restrictedEnemyZonesDefault.ContainsKey(lvlName) && _restrictedEnemyZonesDefault[lvlName].ContainsKey(entity)) ||
                _restrictedEnemyGameCountsTechnical.ContainsKey(entity) ||
                _restrictedEnemyLevelCountsDefault.ContainsKey(entity);
        }

        // This returns a set of ALLOWED rooms
        public static Dictionary<TR3Entities, List<int>> GetRestrictedEnemyRooms(string lvlName, RandoDifficulty difficulty)
        {
            var technicallyAllowedRooms = GetRestrictedEnemyRooms(lvlName, _restrictedEnemyZonesTechnical);
            var multiDict = new List<Dictionary<TR3Entities, List<int>>>() { technicallyAllowedRooms };

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

        private static Dictionary<TR3Entities, List<int>> GetRestrictedEnemyRooms(string lvlName, Dictionary<string, Dictionary<TR3Entities, List<int>>> restrictions)
        {
            if (restrictions.ContainsKey(lvlName))
                return restrictions[lvlName];
            return null;
        }

        public static int GetRestrictedEnemyLevelCount(TR3Entities entity, RandoDifficulty difficulty)
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

        public static Dictionary<TR3Entities, List<string>> PrepareEnemyGameTracker(RandoDifficulty difficulty)
        {
            Dictionary<TR3Entities, List<string>> tracker = new Dictionary<TR3Entities, List<string>>();

            if (difficulty == RandoDifficulty.Default)
            {
                foreach (TR3Entities entity in _restrictedEnemyGameCountsDefault.Keys)
                {
                    tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsDefault[entity]));
                }
            }

            foreach (TR3Entities entity in _restrictedEnemyGameCountsTechnical.Keys)
            {
                tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsTechnical[entity]));
            }

            // Pre-populate required enemies
            foreach (string level in _requiredEnemies.Keys)
            {
                foreach (TR3Entities enemy in _requiredEnemies[level])
                {
                    if (tracker.ContainsKey(enemy))
                    {
                        tracker[enemy].Add(level);
                    }
                }
            }

            return tracker;
        }

        public static bool IsEnemySupported(string lvlName, TR3Entities entity, RandoDifficulty difficulty)
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

        private static bool IsEnemySupported(string lvlName, TR3Entities entity, Dictionary<string, List<TR3Entities>> dict)
        {
            if (dict.ContainsKey(lvlName))
            {
                // if the dictionaries contain the enemy, the enemy is NOT supported
                return !dict[lvlName].Contains(TR3EntityUtilities.TranslateEntityAlias(entity));
            }
            // all enemies are supported by default
            return true;
        }

        public static bool IsEnemyRequired(string lvlName, TR3Entities entity)
        {
            return _requiredEnemies.ContainsKey(lvlName) && _requiredEnemies[lvlName].Contains(entity);
        }

        public static List<TR3Entities> GetRequiredEnemies(string lvlName)
        {
            List<TR3Entities> entities = new List<TR3Entities>();
            if (_requiredEnemies.ContainsKey(lvlName))
            {
                entities.AddRange(_requiredEnemies[lvlName]);
            }
            return entities;
        }

        public static List<Location> GetAIPathing(string lvlName, TR3Entities entity, short room)
        {
            List<Location> locations = new List<Location>();
            if (_restrictedEnemyPathing.ContainsKey(lvlName) && _restrictedEnemyPathing[lvlName].ContainsKey(entity) && _restrictedEnemyPathing[lvlName][entity].ContainsKey(room))
            {
                locations.AddRange(_restrictedEnemyPathing[lvlName][entity][room]);
            }
            return locations;
        }

        public static bool IsDroppableEnemyRequired(TR3CombinedLevel level)
        {
            foreach (TR2Entity entityInstance in level.Data.Entities)
            {
                List<TR2Entity> sharedItems = new List<TR2Entity>(Array.FindAll
                (
                    level.Data.Entities,
                    e =>
                        e.X == entityInstance.X &&
                        e.Y == entityInstance.Y &&
                        e.Z == entityInstance.Z
                ));
                if (sharedItems.Count > 1)
                {
                    // Are any entities that are sharing a location a droppable pickup?
                    foreach (TR2Entity ent in sharedItems)
                    {
                        if (TR3EntityUtilities.IsAnyPickupType((TR3Entities)ent.TypeID))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static Dictionary<TR3Entities, TR3Entities> GetAliasPriority(string lvlName, List<TR3Entities> importEntities)
        {
            // If the priorities map doesn't contain an entity we are trying to import as a key, TRModelTransporter
            // will assume it always has priority (e.g. DogNevada replacing DogLondon).
            Dictionary<TR3Entities, TR3Entities> priorities = new Dictionary<TR3Entities, TR3Entities>();

            return priorities;
        }

        public static void SetEntityTriggers(TR3Level level, TR2Entity entity)
        {
            if (_oneShotEnemies.Contains((TR3Entities)entity.TypeID))
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

        // We (can) restrict some enemies to specific rooms in levels. Some may also need pathing like Willie.
        private static readonly Dictionary<string, Dictionary<TR3Entities, List<int>>> _restrictedEnemyZonesDefault;
        private static readonly Dictionary<string, Dictionary<TR3Entities, List<int>>> _restrictedEnemyZonesTechnical;
        private static readonly Dictionary<string, Dictionary<TR3Entities, Dictionary<short, List<Location>>>> _restrictedEnemyPathing;

        // We also limit the count per level for some, such as bosses.
        // Winston is an easter egg so maybe keep it low.
        private static readonly Dictionary<TR3Entities, int> _restrictedEnemyLevelCountsTechnical = new Dictionary<TR3Entities, int>
        {
            [TR3Entities.TonyFirehands] = 1,
            [TR3Entities.Puna] = 1,
            [TR3Entities.Winston] = 1,
            [TR3Entities.WinstonInCamoSuit] = 1
        };

        private static readonly Dictionary<TR3Entities, int> _restrictedEnemyLevelCountsDefault = new Dictionary<TR3Entities, int>
        {
            [TR3Entities.TonyFirehands] = 1,
            [TR3Entities.Puna] = 1,
            [TR3Entities.Willie] = 1
        };

        // These enemies are restricted a set number of times throughout the entire game.
        private static readonly Dictionary<TR3Entities, int> _restrictedEnemyGameCountsTechnical = new Dictionary<TR3Entities, int>
        {
            [TR3Entities.Winston] = 2,
            [TR3Entities.WinstonInCamoSuit] = 2
        };

        private static readonly Dictionary<TR3Entities, int> _restrictedEnemyGameCountsDefault = new Dictionary<TR3Entities, int>
        {
            [TR3Entities.Willie] = 2
        };

        // These enemies are unsupported due to technical reasons, NOT difficulty reasons.
        private static readonly Dictionary<string, List<TR3Entities>> _unsupportedEnemiesTechnical = new Dictionary<string, List<TR3Entities>>
        {
            [TR3LevelNames.JUNGLE] =
                new List<TR3Entities> { TR3Entities.TonyFirehands },
            [TR3LevelNames.RUINS] =
                new List<TR3Entities> { TR3Entities.TonyFirehands },
            [TR3LevelNames.CAVES] =
                new List<TR3Entities> { TR3Entities.Willie },
            [TR3LevelNames.COASTAL] =
                new List<TR3Entities> { TR3Entities.TonyFirehands },
            [TR3LevelNames.CRASH] =
                new List<TR3Entities> { TR3Entities.TonyFirehands },
            [TR3LevelNames.MADUBU] =
                new List<TR3Entities> { TR3Entities.TonyFirehands },
            [TR3LevelNames.PUNA] =
                new List<TR3Entities> { TR3Entities.TonyFirehands, TR3Entities.Willie },
            [TR3LevelNames.THAMES] =
                new List<TR3Entities> { TR3Entities.TonyFirehands },
            [TR3LevelNames.ALDWYCH] =
                new List<TR3Entities> { TR3Entities.TonyFirehands, TR3Entities.Willie },
            [TR3LevelNames.LUDS] =
                new List<TR3Entities> { TR3Entities.TonyFirehands, TR3Entities.Willie },
            [TR3LevelNames.CITY] =
                new List<TR3Entities> { TR3Entities.TonyFirehands, TR3Entities.Willie },
            [TR3LevelNames.NEVADA] =
                new List<TR3Entities> { TR3Entities.TonyFirehands },
            [TR3LevelNames.HSC] =
                new List<TR3Entities> { TR3Entities.TonyFirehands, TR3Entities.Willie },
            [TR3LevelNames.AREA51] =
                new List<TR3Entities> { TR3Entities.TonyFirehands, TR3Entities.Willie },
            [TR3LevelNames.RXTECH] =
                new List<TR3Entities> { TR3Entities.TonyFirehands, TR3Entities.Willie },
            [TR3LevelNames.TINNOS] =
                new List<TR3Entities> { TR3Entities.TonyFirehands, TR3Entities.Willie },
            [TR3LevelNames.WILLIE] =
                new List<TR3Entities> { TR3Entities.TonyFirehands }
        };

        private static readonly Dictionary<string, List<TR3Entities>> _unsupportedEnemiesDefault = new Dictionary<string, List<TR3Entities>>
        {
            
        };

        // Any enemies that must remain untouched in a given level
        private static readonly Dictionary<string, List<TR3Entities>> _requiredEnemies = new Dictionary<string, List<TR3Entities>>
        {
            [TR3LevelNames.CAVES] = new List<TR3Entities>
            {
                TR3Entities.TonyFirehands // End room flip-map
            },
            [TR3LevelNames.PUNA] = new List<TR3Entities>
            {
                TR3Entities.Puna, TR3Entities.LizardMan // Complicated spawn points
            },
            [TR3LevelNames.TINNOS] = new List<TR3Entities>
            {
                TR3Entities.TinnosWasp // Complicated spawn points
            }
        };

        // Control the number of types of enemy that appear in levels, so these numbers are added to the
        // current total e.g. Jungle becomes 6 types, Ruins becomes 7 etc.
        private static readonly Dictionary<string, int> _enemyAdjustmentCount = new Dictionary<string, int>
        {
            [TR3LevelNames.JUNGLE]
                = 4, // Defaults: 2 types, 19 enemies
            [TR3LevelNames.RUINS]
                = 4, // Defaults: 3 types, 30 enemies
            [TR3LevelNames.GANGES]
                = 2, // Defaults: 3 types, 37 enemies
            [TR3LevelNames.CAVES]
                = 3, // Defaults: 2 types, 15 enemies
            [TR3LevelNames.COASTAL]
                = 3, // Defaults: 3 types, 26 enemies
            [TR3LevelNames.CRASH]
                = 1, // Defaults: 4 types, 35 enemies (ignoring spawns)
            [TR3LevelNames.MADUBU]
                = 4, // Defaults: 2 types, 16 enemies
            [TR3LevelNames.PUNA]
                = 0, // Defaults: 3 types, 9 enemies
            [TR3LevelNames.THAMES]
                = 1, // Defaults: 4 types, 26 enemies
            [TR3LevelNames.ALDWYCH]
                = 3, // Defaults: 3 types, 29 enemies
            [TR3LevelNames.LUDS]
                = 1, // Defaults: 4 types, 20 enemies
            [TR3LevelNames.CITY]
                = 0, // Defaults: 1 type, 1 enemy
            [TR3LevelNames.NEVADA]
                = 3, // Defaults: 3 types, 29 enemies
            [TR3LevelNames.HSC]
                = 1, // Defaults: 4 types, 29 enemies
            [TR3LevelNames.AREA51]
                = 1, // Defaults: 4 types, 19 enemies
            [TR3LevelNames.ANTARC]
                = 3, // Defaults: 3 types, 31 enemies
            [TR3LevelNames.RXTECH]
                = 2, // Defaults: 3 types, 25 enemies
            [TR3LevelNames.TINNOS]
                = 4, // Defaults: 2 types, 13 enemies (ignoring spawns)
            [TR3LevelNames.WILLIE]
                = 1, // Defaults: 3 types, 13 enemies
            [TR3LevelNames.HALLOWS]
                = 0  // Defaults: 2 types, 2 enemies
        };

        // Enemies who can only spawn once. These are enemies whose triggers in OG are all OneShot throughout.
        private static readonly List<TR3Entities> _oneShotEnemies = new List<TR3Entities>
        {
            TR3Entities.Croc,
            TR3Entities.KillerWhale,
            TR3Entities.Raptor,
            TR3Entities.Rat
        };

        static TR3EnemyUtilities()
        {
            _restrictedEnemyZonesDefault = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR3Entities, List<int>>>>
            (
                File.ReadAllText(@"Resources\TR3\Restrictions\enemy_restrictions_default.json")
            );
            _restrictedEnemyZonesTechnical = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR3Entities, List<int>>>>
            (
                File.ReadAllText(@"Resources\TR3\Restrictions\enemy_restrictions_technical.json")
            );
            _restrictedEnemyPathing = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR3Entities, Dictionary<short, List<Location>>>>>
            (
                File.ReadAllText(@"Resources\TR3\Restrictions\enemy_restrictions_pathing.json")
            );
        }
    }
}