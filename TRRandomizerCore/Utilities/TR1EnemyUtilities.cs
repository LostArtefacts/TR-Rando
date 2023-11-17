using Newtonsoft.Json;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Utilities;

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
    public static bool IsEnemyRestricted(string lvlName, TR1Type entity, RandoDifficulty difficulty = RandoDifficulty.DefaultOrNoRestrictions)
    {
        if (difficulty == RandoDifficulty.Default || difficulty == RandoDifficulty.DefaultOrNoRestrictions)
        {
            return (_restrictedEnemyZonesTechnical.ContainsKey(lvlName) && _restrictedEnemyZonesTechnical[lvlName].ContainsKey(entity)) ||
                (_restrictedEnemyZonesDefault.ContainsKey(lvlName) && _restrictedEnemyZonesDefault[lvlName].ContainsKey(entity)) ||
                _restrictedEnemyGameCountsTechnical.ContainsKey(entity) ||
                _restrictedEnemyLevelCountsTechnical.ContainsKey(entity) ||
                _restrictedEnemyLevelCountsDefault.ContainsKey(entity) ||
                (GetRestrictedEnemyGroup(lvlName, TR1TypeUtilities.TranslateAlias(entity), RandoDifficulty.DefaultOrNoRestrictions) != null);
        }

        return (_restrictedEnemyZonesTechnical.ContainsKey(lvlName) && _restrictedEnemyZonesTechnical[lvlName].ContainsKey(entity))
            || _restrictedEnemyLevelCountsTechnical.ContainsKey(entity);
    }

    // This returns a set of ALLOWED rooms
    public static Dictionary<TR1Type, List<int>> GetRestrictedEnemyRooms(string lvlName, RandoDifficulty difficulty)
    {
        var technicallyAllowedRooms = GetRestrictedEnemyRooms(lvlName, _restrictedEnemyZonesTechnical);
        var multiDict = new List<Dictionary<TR1Type, List<int>>>() { technicallyAllowedRooms };

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

    private static Dictionary<TR1Type, List<int>> GetRestrictedEnemyRooms(string lvlName, Dictionary<string, Dictionary<TR1Type, List<int>>> restrictions)
    {
        if (restrictions.ContainsKey(lvlName))
            return restrictions[lvlName];
        return null;
    }

    public static int GetRestrictedEnemyLevelCount(TR1Type entity, RandoDifficulty difficulty)
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

    public static Dictionary<TR1Type, List<string>> PrepareEnemyGameTracker(RandoDifficulty difficulty)
    {
        Dictionary<TR1Type, List<string>> tracker = new();

        if (difficulty == RandoDifficulty.Default)
        {
            foreach (TR1Type entity in _restrictedEnemyGameCountsDefault.Keys)
            {
                tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsDefault[entity]));
            }
        }

        foreach (TR1Type entity in _restrictedEnemyGameCountsTechnical.Keys)
        {
            tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsTechnical[entity]));
        }

        // Pre-populate required enemies
        foreach (string level in _requiredEnemies.Keys)
        {
            foreach (TR1Type enemy in _requiredEnemies[level])
            {
                if (tracker.ContainsKey(enemy))
                {
                    tracker[enemy].Add(level);
                }
            }
        }

        return tracker;
    }

    public static bool IsEnemySupported(string lvlName, TR1Type entity, RandoDifficulty difficulty)
    {
        bool supported = IsEnemySupported(lvlName, entity, _unsupportedEnemiesTechnical);
        if (difficulty == RandoDifficulty.Default)
        {
            // a level may exist in both technical and difficulty dicts, so we check both
            supported &= IsEnemySupported(lvlName, entity, _unsupportedEnemiesDefault);
        }

        return supported;
    }

    private static bool IsEnemySupported(string lvlName, TR1Type entity, Dictionary<string, List<TR1Type>> dict)
    {
        if (dict.ContainsKey(lvlName))
        {
            // if the dictionaries contain the enemy, the enemy is NOT supported
            return !dict[lvlName].Contains(TR1TypeUtilities.TranslateAlias(entity));
        }
        // all enemies are supported by default
        return true;
    }

    public static bool IsEnemyRequired(string lvlName, TR1Type entity)
    {
        return _requiredEnemies.ContainsKey(lvlName) && _requiredEnemies[lvlName].Contains(entity);
    }

    public static List<TR1Type> GetRequiredEnemies(string lvlName)
    {
        List<TR1Type> entities = new();
        if (_requiredEnemies.ContainsKey(lvlName))
        {
            entities.AddRange(_requiredEnemies[lvlName]);
        }
        return entities;
    }

    public static void SetEntityTriggers(TR1Level level, TR1Entity entity)
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

    public static EnemyDifficulty GetEnemyDifficulty(List<TR1Entity> enemyEntities)
    {
        if (enemyEntities.Count == 0)
        {
            return EnemyDifficulty.VeryEasy;
        }

        int weight = 0;
        foreach (TR1Entity enemyEntity in enemyEntities)
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

    public static uint GetStartingAmmo(TR1Type weaponType)
    {
        if (_startingAmmoToGive.ContainsKey(weaponType))
        {
            return _startingAmmoToGive[weaponType];
        }
        return 0;
    }

    public static Dictionary<TR1Type, TR1Type> GetAliasPriority(string lvlName, List<TR1Type> importEntities)
    {
        Dictionary<TR1Type, TR1Type> priorities = new();

        bool trexPresent = importEntities.Contains(TR1Type.TRex);
        bool adamPresent = importEntities.Contains(TR1Type.Adam);

        if ((trexPresent || adamPresent) && (lvlName == TR1LevelNames.SANCTUARY || lvlName == TR1LevelNames.ATLANTIS))
        {
            // We have to override the scion pickup animation otherwise death-by-adam will cause the level to end.
            // Environment mods will deal with workarounds for the pickups.
            priorities[TR1Type.LaraMiscAnim_H] = trexPresent ? TR1Type.LaraMiscAnim_H_Valley : TR1Type.LaraMiscAnim_H_Pyramid;
        }
        else
        {
            switch (lvlName)
            {
                // Essential MiscAnims - e.g. they contain level end triggers or cinematics.
                // ToQ pickup cinematic works with T-Rex, but not Torso
                case TR1LevelNames.QUALOPEC:
                    priorities[TR1Type.LaraMiscAnim_H] = trexPresent ? TR1Type.LaraMiscAnim_H_Valley : TR1Type.LaraMiscAnim_H_Qualopec;
                    break;
                case TR1LevelNames.MIDAS:
                    priorities[TR1Type.LaraMiscAnim_H] = TR1Type.LaraMiscAnim_H_Midas;
                    break;
                case TR1LevelNames.SANCTUARY:
                    priorities[TR1Type.LaraMiscAnim_H] = TR1Type.LaraMiscAnim_H_Sanctuary;
                    break;
                case TR1LevelNames.ATLANTIS:
                    priorities[TR1Type.LaraMiscAnim_H] = TR1Type.LaraMiscAnim_H_Atlantis;
                    break;

                // Lara's specific deaths:
                //    - Adam + LaraMiscAnim_H_Valley = a fairly wonky death
                //    - TRex + LaraMiscAnim_H_Pyramid = a very wonky death
                // So if both Adam and TRex are present, the TRex anim is chosen,
                // otherwise it's their corresponding anim.
                default:
                    if (trexPresent)
                    {
                        priorities[TR1Type.LaraMiscAnim_H] = TR1Type.LaraMiscAnim_H_Valley;
                    }
                    else if (adamPresent)
                    {
                        priorities[TR1Type.LaraMiscAnim_H] = TR1Type.LaraMiscAnim_H_Pyramid;
                    }
                    else
                    {
                        priorities[TR1Type.LaraMiscAnim_H] = TR1Type.LaraMiscAnim_H_General;
                    }
                    break;
            }
        }

        return priorities;
    }

    public static RestrictedEnemyGroup GetRestrictedEnemyGroup(string lvlName, TR1Type entity, RandoDifficulty difficulty)
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
    private static readonly Dictionary<string, Dictionary<TR1Type, List<int>>> _restrictedEnemyZonesDefault;
    private static readonly Dictionary<string, Dictionary<TR1Type, List<int>>> _restrictedEnemyZonesTechnical;

    // We (can) also limit the count per level for some, such as bosses.
    private static readonly Dictionary<TR1Type, int> _restrictedEnemyLevelCountsTechnical = new()
    {
    };

    private static readonly Dictionary<TR1Type, int> _restrictedEnemyLevelCountsDefault = new()
    {
        [TR1Type.Adam] = 1,
        [TR1Type.Cowboy] = 3,
        [TR1Type.CowboyOG] = 3,
        [TR1Type.CowboyHeadless] = 3,
        [TR1Type.Kold] = 3,
        [TR1Type.Pierre] = 3,
        [TR1Type.Natla] = 1,
        [TR1Type.SkateboardKid] = 2,
    };

    // These enemies are restricted a set number of times throughout the entire game.
    // Perhaps add level-ending larson as an option
    private static readonly Dictionary<TR1Type, int> _restrictedEnemyGameCountsTechnical = new()
    {
    };

    private static readonly Dictionary<TR1Type, int> _restrictedEnemyGameCountsDefault = new()
    {
        [TR1Type.Adam] = 3,
        [TR1Type.Natla] = 2
    };

    private static readonly List<TR1Type> _allAtlanteans = new()
    {
        TR1Type.Adam, TR1Type.Centaur, TR1Type.CentaurStatue, TR1Type.FlyingAtlantean,
        TR1Type.NonShootingAtlantean_N, TR1Type.ShootingAtlantean_N, TR1Type.Natla,
        TR1Type.AtlanteanEgg
    };

    private static readonly Dictionary<string, List<RestrictedEnemyGroup>> _restrictedEnemyGroupCounts = new()
    {
        [TR1LevelNames.CAVES] = new()
        {
            new()
            {
                MaximumCount = 5,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.VILCABAMBA] = new()
        {
            new()
            {
                MaximumCount = 10,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.VALLEY] = new()
        {
            new()
            {
                MaximumCount = 5,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.QUALOPEC] = new()
        {
            new()
            {
                MaximumCount = 0,
                Enemies = new List<TR1Type> { TR1Type.Larson }
            },
            new()
            {
                MaximumCount = 3,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.FOLLY] = new()
        {
            new()
            {
                MaximumCount = 10,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.COLOSSEUM] = new()
        {
            new()
            {
                MaximumCount = 11,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.MIDAS] = new()
        {
            new()
            {
                MaximumCount = 15,
                Enemies = _allAtlanteans
            },
            new()
            {
                MaximumCount = 10,
                Enemies = new() { TR1Type.TRex }
            }
        },
        [TR1LevelNames.CISTERN] = new()
        {
            new()
            {
                MaximumCount = 13,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.TIHOCAN] = new()
        {
            new()
            {
                MaximumCount = 9,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.KHAMOON] = new()
        {
            new()
            {
                MaximumCount = 6,
                Enemies = _allAtlanteans
            }
        },
        [TR1LevelNames.OBELISK] = new()
        {
            new()
            {
                MaximumCount = 6,
                Enemies = _allAtlanteans
            }
        },
    };

    // These enemies are unsupported due to technical reasons, NOT difficulty reasons.
    private static readonly Dictionary<string, List<TR1Type>> _unsupportedEnemiesTechnical = new()
    {
    };

    private static readonly Dictionary<string, List<TR1Type>> _unsupportedEnemiesDefault = new()
    {
        [TR1LevelNames.QUALOPEC] = new List<TR1Type>
        {
            TR1Type.Adam
        }
    };

    // Any enemies that must remain untouched in a given level
    private static readonly Dictionary<string, List<TR1Type>> _requiredEnemies = new()
    {
        [TR1LevelNames.QUALOPEC] = new List<TR1Type>
        {
            TR1Type.Larson // Ends the level
        },
        [TR1LevelNames.PYRAMID] = new List<TR1Type>
        {
            TR1Type.Adam // Heavy trigger
        }
    };

    // Control the number of types of enemy that appear in levels, so these numbers are added to the
    // current total e.g. Caves becomes 5 types, Vilcabamba becomes 4 etc.
    private static readonly Dictionary<string, int> _enemyAdjustmentCount = new()
    {
        [TR1LevelNames.CAVES]
            = 2, // Defaults: 3 types, 14 enemies
        [TR1LevelNames.VILCABAMBA]
            = 1, // Defaults: 3 types, 29 enemies
        [TR1LevelNames.VALLEY]
            = 2, // Defaults: 3 types, 13 enemies
        [TR1LevelNames.QUALOPEC]
            = 0, // Defaults: 3 types, 7 enemies
        [TR1LevelNames.FOLLY]
            = 0, // Defaults: 6 types, 25 enemies
        [TR1LevelNames.COLOSSEUM]
            = 0, // Defaults: 7 types, 29 enemies
        [TR1LevelNames.MIDAS]
            = 0, // Defaults: 6 types, 43 enemies
        [TR1LevelNames.CISTERN]
            = 0, // Defaults: 8 types, 37 enemies
        [TR1LevelNames.TIHOCAN]
            = 0, // Defaults: 7 types, 18 enemies
        [TR1LevelNames.KHAMOON]
            = 1, // Defaults: 4 types, 14 enemies
        [TR1LevelNames.OBELISK]
            = 1, // Defaults: 3 types, 16 enemies
        [TR1LevelNames.SANCTUARY]
            = 0, // Defaults: 5 types, 15 enemies
        [TR1LevelNames.MINES]
            = 0, // Defaults: 3 types, 3 enemies
        [TR1LevelNames.ATLANTIS]
            = 1, // Defaults: 5 types, 32 enemies
        [TR1LevelNames.PYRAMID]
            = 0  // Defaults: 2 types, 4 enemies
    };

    // Enemies who can only spawn once.
    private static readonly List<TR1Type> _oneShotEnemies = new()
    {
        TR1Type.Pierre
    };

    private static readonly Dictionary<EnemyDifficulty, List<TR1Type>> _enemyDifficulties = new()
    {
        [EnemyDifficulty.VeryEasy] = new List<TR1Type>
        {
            TR1Type.Bat
        },
        [EnemyDifficulty.Easy] = new List<TR1Type>
        {
            TR1Type.Bear, TR1Type.Gorilla, TR1Type.RatLand, TR1Type.RatWater,
            TR1Type.Wolf
        },
        [EnemyDifficulty.Medium] = new List<TR1Type>
        {
            TR1Type.Cowboy, TR1Type.Raptor, TR1Type.Lion, TR1Type.Lioness,
            TR1Type.Panther,TR1Type.CrocodileLand, TR1Type.CrocodileWater, TR1Type.Pierre,
            TR1Type.Larson
        },
        [EnemyDifficulty.Hard] = new List<TR1Type>
        {
            TR1Type.TRex, TR1Type.SkateboardKid, TR1Type.Kold, TR1Type.Centaur,
            TR1Type.CentaurStatue, TR1Type.FlyingAtlantean, TR1Type.ShootingAtlantean_N, TR1Type.NonShootingAtlantean_N
        },
        [EnemyDifficulty.VeryHard] = new List<TR1Type>
        {
            TR1Type.Adam, TR1Type.Natla
        }
    };

    private static readonly Dictionary<TR1Type, uint> _startingAmmoToGive = new()
    {
        [TR1Type.Shotgun_S_P] = 10,
        [TR1Type.Magnums_S_P] = 6,
        [TR1Type.Uzis_S_P] = 6
    };

    static TR1EnemyUtilities()
    {
        _restrictedEnemyZonesDefault = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR1Type, List<int>>>>
        (
            File.ReadAllText(@"Resources\TR1\Restrictions\enemy_restrictions_default.json")
        );
        _restrictedEnemyZonesTechnical = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR1Type, List<int>>>>
        (
            File.ReadAllText(@"Resources\TR1\Restrictions\enemy_restrictions_technical.json")
        );
    }

    public static ushort AtlanteanToCodeBits(TR1Type atlantean)
    {
        return atlantean switch
        {
            TR1Type.ShootingAtlantean_N => 1,
            TR1Type.Centaur => 2,
            TR1Type.Adam => 4,
            TR1Type.NonShootingAtlantean_N => 8,
            _ => 0,
        };
    }

    public static TR1Type CodeBitsToAtlantean(ushort codeBits)
    {
        return codeBits switch
        {
            1 => TR1Type.ShootingAtlantean_N,
            2 => TR1Type.Centaur,
            4 => TR1Type.Adam,
            8 => TR1Type.NonShootingAtlantean_N,
            _ => TR1Type.FlyingAtlantean,
        };
    }

    public static bool IsEmptyEgg(TR1Entity entity, TR1CombinedLevel level)
    {
        if (!TR1TypeUtilities.IsEggType(entity.TypeID))
        {
            return false;
        }

        TR1Type type = CodeBitsToAtlantean(entity.CodeBits);
        return Array.Find(level.Data.Models, m => m.ID == (uint)type) == null;
    }

    public static bool CanDropItems(TR1Entity entity, TR1CombinedLevel level, FDControl floorData)
    {
        if (IsEmptyEgg(entity, level))
        {
            return false;
        }

        if (entity.TypeID == TR1Type.Pierre)
        {
            return FDUtilities.GetEntityTriggers(floorData, level.Data.Entities.IndexOf(entity))
                .All(t => t.TrigSetup.OneShot);
        }

        return TR1TypeUtilities.IsEnemyType(entity.TypeID);
    }
}

public class RestrictedEnemyGroup
{
    public List<TR1Type> Enemies { get; set; }
    public int MaximumCount { get; set; }
    public bool IsSealed { get; set; }
}
