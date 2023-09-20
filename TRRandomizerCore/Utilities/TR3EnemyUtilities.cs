using Newtonsoft.Json;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Utilities;

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
    public static bool IsEnemyRestricted(string lvlName, TR3Type entity)
    {
        return (_restrictedEnemyZonesTechnical.ContainsKey(lvlName) && _restrictedEnemyZonesTechnical[lvlName].ContainsKey(entity)) ||
            (_restrictedEnemyZonesDefault.ContainsKey(lvlName) && _restrictedEnemyZonesDefault[lvlName].ContainsKey(entity)) ||
            _restrictedEnemyGameCountsTechnical.ContainsKey(entity) ||
            _restrictedEnemyLevelCountsDefault.ContainsKey(entity);
    }

    // This returns a set of ALLOWED rooms
    public static Dictionary<TR3Type, List<int>> GetRestrictedEnemyRooms(string lvlName, RandoDifficulty difficulty)
    {
        var technicallyAllowedRooms = GetRestrictedEnemyRooms(lvlName, _restrictedEnemyZonesTechnical);
        var multiDict = new List<Dictionary<TR3Type, List<int>>>() { technicallyAllowedRooms };

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

    private static Dictionary<TR3Type, List<int>> GetRestrictedEnemyRooms(string lvlName, Dictionary<string, Dictionary<TR3Type, List<int>>> restrictions)
    {
        if (restrictions.ContainsKey(lvlName))
            return restrictions[lvlName];
        return null;
    }

    public static int GetRestrictedEnemyLevelCount(TR3Type entity, RandoDifficulty difficulty)
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

    public static Dictionary<TR3Type, List<string>> PrepareEnemyGameTracker(RandoDifficulty difficulty)
    {
        Dictionary<TR3Type, List<string>> tracker = new();

        if (difficulty == RandoDifficulty.Default)
        {
            foreach (TR3Type entity in _restrictedEnemyGameCountsDefault.Keys)
            {
                tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsDefault[entity]));
            }
        }

        foreach (TR3Type entity in _restrictedEnemyGameCountsTechnical.Keys)
        {
            tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsTechnical[entity]));
        }

        // Pre-populate required enemies
        foreach (string level in _requiredEnemies.Keys)
        {
            foreach (TR3Type enemy in _requiredEnemies[level])
            {
                if (tracker.ContainsKey(enemy))
                {
                    tracker[enemy].Add(level);
                }
            }
        }

        return tracker;
    }

    public static bool IsEnemySupported(string lvlName, TR3Type entity, RandoDifficulty difficulty)
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

    private static bool IsEnemySupported(string lvlName, TR3Type entity, Dictionary<string, List<TR3Type>> dict)
    {
        if (dict.ContainsKey(lvlName))
        {
            // if the dictionaries contain the enemy, the enemy is NOT supported
            return !dict[lvlName].Contains(TR3TypeUtilities.TranslateAlias(entity));
        }
        // all enemies are supported by default
        return true;
    }

    public static bool IsEnemyRequired(string lvlName, TR3Type entity)
    {
        return _requiredEnemies.ContainsKey(lvlName) && _requiredEnemies[lvlName].Contains(entity);
    }

    public static List<TR3Type> GetRequiredEnemies(string lvlName)
    {
        List<TR3Type> entities = new();
        if (_requiredEnemies.ContainsKey(lvlName))
        {
            entities.AddRange(_requiredEnemies[lvlName]);
        }
        return entities;
    }

    public static List<Location> GetAIPathing(string lvlName, TR3Type entity, short room)
    {
        List<Location> locations = new();
        if (_restrictedEnemyPathing.ContainsKey(lvlName) && _restrictedEnemyPathing[lvlName].ContainsKey(entity) && _restrictedEnemyPathing[lvlName][entity].ContainsKey(room))
        {
            locations.AddRange(_restrictedEnemyPathing[lvlName][entity][room]);
        }
        return locations;
    }

    public static bool IsDroppableEnemyRequired(TR3CombinedLevel level)
    {
        List<TR3Entity> enemies = level.Data.Entities.FindAll(e => TR3TypeUtilities.IsEnemyType(e.TypeID));
        foreach (TR3Entity entityInstance in enemies)
        {
            List<TR3Entity> sharedItems = level.Data.Entities.FindAll(e =>
                e.X == entityInstance.X
                && e.Y == entityInstance.Y
                && e.Z == entityInstance.Z
            );
            if (sharedItems.Count > 1)
            {
                // Are any entities that are sharing a location a droppable pickup?
                foreach (TR3Entity ent in sharedItems)
                {
                    if (TR3TypeUtilities.IsAnyPickupType(ent.TypeID))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static void SetEntityTriggers(TR3Level level, TR3Entity entity)
    {
        if (_oneShotEnemies.Contains(entity.TypeID))
        {
            int entityID = level.Entities.IndexOf(entity);

            FDControl fdControl = new();
            fdControl.ParseFromLevel(level);

            List<FDTriggerEntry> triggers = FDUtilities.GetEntityTriggers(fdControl, entityID);
            foreach (FDTriggerEntry trigger in triggers)
            {
                trigger.TrigSetup.OneShot = true;
            }

            fdControl.WriteToLevel(level);
        }
    }

    public static EnemyDifficulty GetEnemyDifficulty(List<TR3Entity> enemyEntities)
    {
        if (enemyEntities.Count == 0)
        {
            return EnemyDifficulty.VeryEasy;
        }

        int weight = 0;
        foreach (TR3Entity enemyEntity in enemyEntities)
        {
            EnemyDifficulty enemyDifficulty = EnemyDifficulty.Medium;
            foreach (EnemyDifficulty difficulty in _enemyDifficulties.Keys)
            {
                if (_enemyDifficulties[difficulty].Contains(enemyEntity.TypeID))
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

        List<EnemyDifficulty> allDifficulties = new(Enum.GetValues(typeof(EnemyDifficulty)).Cast<EnemyDifficulty>());

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

    public static uint GetStartingAmmo(TR3Type weaponType)
    {
        if (_startingAmmoToGive.ContainsKey(weaponType))
        {
            return _startingAmmoToGive[weaponType];
        }
        return 0;
    }

    // We (can) restrict some enemies to specific rooms in levels. Some may also need pathing like Willie.
    private static readonly Dictionary<string, Dictionary<TR3Type, List<int>>> _restrictedEnemyZonesDefault;
    private static readonly Dictionary<string, Dictionary<TR3Type, List<int>>> _restrictedEnemyZonesTechnical;
    private static readonly Dictionary<string, Dictionary<TR3Type, Dictionary<short, List<Location>>>> _restrictedEnemyPathing;

    // We also limit the count per level for some, such as bosses.
    // Winston is an easter egg so maybe keep it low.
    private static readonly Dictionary<TR3Type, int> _restrictedEnemyLevelCountsTechnical = new()
    {
        [TR3Type.TonyFirehands] = 1,
        [TR3Type.Puna] = 1,
        [TR3Type.Winston] = 1,
        [TR3Type.WinstonInCamoSuit] = 1
    };

    private static readonly Dictionary<TR3Type, int> _restrictedEnemyLevelCountsDefault = new()
    {
        [TR3Type.TonyFirehands] = 1,
        [TR3Type.Puna] = 1,
        [TR3Type.Willie] = 1
    };

    // These enemies are restricted a set number of times throughout the entire game.
    private static readonly Dictionary<TR3Type, int> _restrictedEnemyGameCountsTechnical = new()
    {
        [TR3Type.Winston] = 2,
        [TR3Type.WinstonInCamoSuit] = 2
    };

    private static readonly Dictionary<TR3Type, int> _restrictedEnemyGameCountsDefault = new()
    {
        [TR3Type.Willie] = 2
    };

    // These enemies are unsupported due to technical reasons, NOT difficulty reasons.
    private static readonly Dictionary<string, List<TR3Type>> _unsupportedEnemiesTechnical = new()
    {
        [TR3LevelNames.JUNGLE] =
            new List<TR3Type> { TR3Type.TonyFirehands },
        [TR3LevelNames.RUINS] =
            new List<TR3Type> { TR3Type.TonyFirehands },
        [TR3LevelNames.CAVES] =
            new List<TR3Type> { TR3Type.Willie },
        [TR3LevelNames.COASTAL] =
            new List<TR3Type> { TR3Type.TonyFirehands },
        [TR3LevelNames.CRASH] =
            new List<TR3Type> { TR3Type.TonyFirehands },
        [TR3LevelNames.MADUBU] =
            new List<TR3Type> { TR3Type.TonyFirehands },
        [TR3LevelNames.PUNA] =
            new List<TR3Type> { TR3Type.TonyFirehands, TR3Type.Willie },
        [TR3LevelNames.THAMES] =
            new List<TR3Type> { TR3Type.TonyFirehands },
        [TR3LevelNames.ALDWYCH] =
            new List<TR3Type> { TR3Type.TonyFirehands, TR3Type.Willie },
        [TR3LevelNames.LUDS] =
            new List<TR3Type> { TR3Type.TonyFirehands, TR3Type.Willie },
        [TR3LevelNames.CITY] =
            new List<TR3Type> { TR3Type.TonyFirehands, TR3Type.Willie },
        [TR3LevelNames.NEVADA] =
            new List<TR3Type> { TR3Type.TonyFirehands },
        [TR3LevelNames.HSC] =
            new List<TR3Type> { TR3Type.TonyFirehands, TR3Type.Willie },
        [TR3LevelNames.AREA51] =
            new List<TR3Type> { TR3Type.TonyFirehands, TR3Type.Willie },
        [TR3LevelNames.RXTECH] =
            new List<TR3Type> { TR3Type.TonyFirehands, TR3Type.Willie },
        [TR3LevelNames.TINNOS] =
            new List<TR3Type> { TR3Type.TonyFirehands, TR3Type.Willie },
        [TR3LevelNames.WILLIE] =
            new List<TR3Type> { TR3Type.TonyFirehands }
    };

    private static readonly Dictionary<string, List<TR3Type>> _unsupportedEnemiesDefault = new()
    {
        [TR3LevelNames.HALLOWS] =
            new List<TR3Type> { TR3Type.Willie }
    };

    // Any enemies that must remain untouched in a given level
    private static readonly Dictionary<string, List<TR3Type>> _requiredEnemies = new()
    {
        [TR3LevelNames.CAVES] = new List<TR3Type>
        {
            TR3Type.TonyFirehands // End room flip-map
        },
        [TR3LevelNames.PUNA] = new List<TR3Type>
        {
            TR3Type.Puna, TR3Type.LizardMan // Complicated spawn points
        },
        [TR3LevelNames.TINNOS] = new List<TR3Type>
        {
            TR3Type.TinnosWasp // Complicated spawn points
        }
    };

    // Control the number of types of enemy that appear in levels, so these numbers are added to the
    // current total e.g. Jungle becomes 6 types, Ruins becomes 7 etc.
    private static readonly Dictionary<string, int> _enemyAdjustmentCount = new()
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
    private static readonly List<TR3Type> _oneShotEnemies = new()
    {
        TR3Type.Croc,
        TR3Type.KillerWhale,
        TR3Type.Raptor,
        TR3Type.Rat
    };

    private static readonly Dictionary<EnemyDifficulty, List<TR3Type>> _enemyDifficulties = new()
    {
        [EnemyDifficulty.VeryEasy] = new List<TR3Type>
        {
            TR3Type.KillerWhale, TR3Type.Winston, TR3Type.WinstonInCamoSuit,
            TR3Type.Rat, TR3Type.Compsognathus
        },
        [EnemyDifficulty.Easy] = new List<TR3Type>
        {
            TR3Type.Cobra, TR3Type.Crow, TR3Type.Vulture, TR3Type.TinnosWasp,
            TR3Type.RXTechFlameLad, TR3Type.Prisoner, TR3Type.Monkey, TR3Type.Mercenary
        },
        [EnemyDifficulty.Medium] = new List<TR3Type>
        {
            TR3Type.Crawler, TR3Type.CrawlerMutantInCloset, TR3Type.Croc, TR3Type.DamGuard,
            TR3Type.DogAntarc, TR3Type.Dog, TR3Type.TribesmanAxe, TR3Type.TribesmanDart,
            TR3Type.Tiger, TR3Type.ScubaSteve, TR3Type.RXRedBoi, TR3Type.RXGunLad,
            TR3Type.Punk, TR3Type.MPWithStick, TR3Type.MPWithGun, TR3Type.LondonMerc,
            TR3Type.LondonGuard, TR3Type.LizardMan
        },
        [EnemyDifficulty.Hard] = new List<TR3Type>
        {
            TR3Type.BruteMutant, TR3Type.TonyFirehands, TR3Type.TinnosMonster, TR3Type.Shiva,
            TR3Type.Raptor, TR3Type.Puna, TR3Type.SophiaLee
        },
        [EnemyDifficulty.VeryHard] = new List<TR3Type>
        {
            TR3Type.Tyrannosaur, TR3Type.Willie
        }
    };

    private static readonly Dictionary<TR3Type, uint> _startingAmmoToGive = new()
    {
        [TR3Type.Shotgun_P] = 8,
        [TR3Type.Deagle_P] = 4,
        [TR3Type.Uzis_P] = 6,
        [TR3Type.Harpoon_P] = 10,
        [TR3Type.MP5_P] = 4,
        [TR3Type.GrenadeLauncher_P] = 6,
        [TR3Type.RocketLauncher_P] = 6
    };

    static TR3EnemyUtilities()
    {
        _restrictedEnemyZonesDefault = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR3Type, List<int>>>>
        (
            File.ReadAllText(@"Resources\TR3\Restrictions\enemy_restrictions_default.json")
        );
        _restrictedEnemyZonesTechnical = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR3Type, List<int>>>>
        (
            File.ReadAllText(@"Resources\TR3\Restrictions\enemy_restrictions_technical.json")
        );
        _restrictedEnemyPathing = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR3Type, Dictionary<short, List<Location>>>>>
        (
            File.ReadAllText(@"Resources\TR3\Restrictions\enemy_restrictions_pathing.json")
        );
    }
}
