using Newtonsoft.Json;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Utilities;

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
    public static int GetTargetEnemyAdjustmentCount(string lvlName, TR2Type enemy)
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
            TR2Type entity = entityInstance.TypeID;
            if (TR2TypeUtilities.IsWaterCreature(entity))
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
        List<TR2Entity> enemies = level.Data.Entities.FindAll(e => TR2TypeUtilities.IsEnemyType(e.TypeID));
        foreach (TR2Entity entityInstance in enemies)
        {
            List<TR2Entity> sharedItems = level.Data.Entities.FindAll(e =>
                e.X == entityInstance.X
                && e.Y == entityInstance.Y
                && e.Z == entityInstance.Z
            );
            if (sharedItems.Count > 1)
            {
                // Are any entities that are sharing a location a droppable pickup?
                foreach (TR2Entity ent in sharedItems)
                {
                    TR2Type EntType = ent.TypeID;

                    if
                    (
                        TR2TypeUtilities.IsUtilityType(EntType) ||
                        TR2TypeUtilities.IsGunType(EntType) ||
                        TR2TypeUtilities.IsKeyItemType(EntType)
                    )
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static bool IsEnemySupported(string lvlName, TR2Type entity, RandoDifficulty difficulty, bool protectMonks)
    {
        if (lvlName == TR2LevelNames.HOME && TR2TypeUtilities.IsMonk(entity))
        {
            // Monks are excluded from HSH by default unless the player is happy
            // with having to kill them.
            return !protectMonks;
        }

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
    private static bool IsEnemySupported(string lvlName, TR2Type entity, Dictionary<string, List<TR2Type>> dict)
    {
        if (dict.ContainsKey(lvlName))
        {
            // if the dictionaries contain the enemy, the enemy is NOT supported
            return !dict[lvlName].Contains(TR2TypeUtilities.TranslateAlias(entity));
        }
        // all enemies are supported by default
        return true;
    }

    public static bool IsEnemyRequired(string lvlName, TR2Type entity)
    {
        return _requiredEnemies.ContainsKey(lvlName) && _requiredEnemies[lvlName].Contains(entity);
    }

    public static List<TR2Type> GetRequiredEnemies(string lvlName)
    {
        List<TR2Type> entities = new();
        if (_requiredEnemies.ContainsKey(lvlName))
        {
            entities.AddRange(_requiredEnemies[lvlName]);
        }
        return entities;
    }

    // this returns a set of ALLOWED rooms
    public static Dictionary<TR2Type, List<int>> GetRestrictedEnemyRooms(string lvlName, RandoDifficulty difficulty)
    {
        var technicallyAllowedRooms = GetRestrictedEnemyRooms(lvlName, _restrictedEnemyZonesTechnical);
        var multiDict = new List<Dictionary<TR2Type, List<int>>>() { technicallyAllowedRooms };

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
    private static Dictionary<TR2Type, List<int>> GetRestrictedEnemyRooms(string lvlName, Dictionary<string, Dictionary<TR2Type, List<int>>> restrictions)
    {
        if (restrictions.ContainsKey(lvlName))
            return restrictions[lvlName];
        return null;
    }

    public static int GetRestrictedEnemyLevelCount(TR2Type entity, RandoDifficulty difficulty)
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

    public static List<List<TR2Type>> GetPermittedCombinations(string lvl, TR2Type entity, RandoDifficulty difficulty)
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

    public static Dictionary<TR2Type, List<string>> PrepareEnemyGameTracker(bool docileBirdMonster, RandoDifficulty difficulty)
    {
        Dictionary<TR2Type, List<string>> tracker = new();

        if (difficulty == RandoDifficulty.Default)
        {
            foreach (TR2Type entity in _restrictedEnemyGameCountsDefault.Keys)
            {
                if (!docileBirdMonster || entity != TR2Type.BirdMonster)
                {
                    tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsDefault[entity]));
                }
            }
        }
        foreach (TR2Type entity in _restrictedEnemyGameCountsTechnical.Keys)
        {
            if (!docileBirdMonster || entity != TR2Type.BirdMonster)
            {
                tracker.Add(entity, new List<string>(_restrictedEnemyGameCountsTechnical[entity]));
            }
        }

        // Pre-populate required enemies
        foreach (string level in _requiredEnemies.Keys)
        {
            foreach (TR2Type enemy in _requiredEnemies[level])
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

        ISet<TR2Type> enemyEntities = new HashSet<TR2Type>();
        enemies.ForEach(e => enemyEntities.Add(e.TypeID));

        int weight = 0;
        foreach (TR2Type enemyEntity in enemyEntities)
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

    // These enemies are unsupported due to technical reasons, NOT difficulty reasons.
    private static readonly Dictionary<string, List<TR2Type>> _unsupportedEnemiesTechnical = new()
    {
        // #192 The Barkhang/Opera House freeze appears to be caused by dead floating water creatures, so they're all banished
        [TR2LevelNames.OPERA] =
            new List<TR2Type>
            {
                TR2Type.Barracuda, TR2Type.BlackMorayEel, TR2Type.ScubaDiver,
                TR2Type.Shark, TR2Type.YellowMorayEel
            },
        [TR2LevelNames.MONASTERY] =
            new List<TR2Type>
            {
                TR2Type.Barracuda, TR2Type.BlackMorayEel, TR2Type.ScubaDiver,
                TR2Type.Shark, TR2Type.YellowMorayEel
            },
        [TR2LevelNames.HOME] =
            // #148 Although we say here that the Doberman, MaskedGoons and StickGoons
            // aren't supported, this is only for cross-level purposes because we
            // are making placeholder entities to prevent breaking the kill counter.
            new List<TR2Type>
            {
                TR2Type.BlackMorayEel, TR2Type.Doberman, TR2Type.MaskedGoon1,
                TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.MercSnowmobDriver,
                TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick, TR2Type.StickWieldingGoon1,
                TR2Type.StickWieldingGoon2, TR2Type.Winston, TR2Type.YellowMorayEel, TR2Type.ShotgunGoon
            }
    };

    private static readonly Dictionary<string, List<TR2Type>> _unsupportedEnemiesDefault = new()
    {
        [TR2LevelNames.LAIR] =
            new List<TR2Type> { TR2Type.MercSnowmobDriver },
        [TR2LevelNames.HOME] =
            new List<TR2Type> { TR2Type.Spider, TR2Type.Rat }
    };

    private static readonly Dictionary<string, List<TR2Type>> _requiredEnemies = new()
    {
        [TR2LevelNames.CHICKEN] =
            new List<TR2Type> { TR2Type.BirdMonster },  // #60 - Ice Palace chicken man must remain to avoid softlock.
        [TR2LevelNames.LAIR] =
            new List<TR2Type> { TR2Type.MarcoBartoli }, // #97 - Marco/Dragon to remain in the same place to trigger door opening
        [TR2LevelNames.HOME] =
            new List<TR2Type> { TR2Type.ShotgunGoon }  // #62 - Avoid randomizing shotgun goon in HSH
    };

    // We restrict some enemies to specific rooms in levels, for example the dragon does not work well in small
    // rooms, and the likes of SnowmobDriver at the beginning of Bartoli's is practically impossible to pass.
    private static readonly Dictionary<string, Dictionary<TR2Type, List<int>>> _restrictedEnemyZonesDefault;
    private static readonly Dictionary<string, Dictionary<TR2Type, List<int>>> _restrictedEnemyZonesTechnical;

    // This allows us to define specific combinations of enemies if the leader is chosen for the rando pool. For
    // example, the dragon in HSH will only work with 20 possible combinations.
    private static readonly Dictionary<string, Dictionary<TR2Type, Dictionary<RandoDifficulty, List<List<TR2Type>>>>> _specialEnemyCombinations;

    // We also limit the count for some - more than 1 dragon tends to cause crashes if they spawn close together.
    // Winston is an easter egg so maybe keep it low.
    private static readonly Dictionary<TR2Type, int> _restrictedEnemyLevelCountsTechnical = new()
    {
        [TR2Type.MarcoBartoli] = 1,
        [TR2Type.Winston] = 2
    };
    private static readonly Dictionary<TR2Type, int> _restrictedEnemyLevelCountsDefault = new()
    {
        [TR2Type.MercSnowmobDriver] = 2,
    };

    // These enemies are restricted a set number of times throughout the entire game.
    private static readonly Dictionary<TR2Type, int> _restrictedEnemyGameCountsTechnical = new()
    {
        [TR2Type.Winston] = 2,
    };
    private static readonly Dictionary<TR2Type, int> _restrictedEnemyGameCountsDefault = new()
    {
        [TR2Type.BirdMonster] = 3,
    };

    // Predefined absolute limits for skidoo drivers
    private static readonly Dictionary<string, int> _skidooLimits = new()
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
        _restrictedEnemyZonesDefault = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR2Type, List<int>>>>
        (
            File.ReadAllText(@"Resources\TR2\Restrictions\enemy_restrictions_default.json")
        );
        _restrictedEnemyZonesTechnical = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR2Type, List<int>>>>
        (
            File.ReadAllText(@"Resources\TR2\Restrictions\enemy_restrictions_technical.json")
        );
        _specialEnemyCombinations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR2Type, Dictionary<RandoDifficulty, List<List<TR2Type>>>>>>
        (
            File.ReadAllText(@"Resources\TR2\Restrictions\enemy_restrictions_special.json")
        );
    }

    private static readonly Dictionary<EnemyDifficulty, List<TR2Type>> _enemyDifficulties = new()
    {
        [EnemyDifficulty.VeryEasy] = new List<TR2Type>
        {
            TR2Type.Barracuda, TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick,
            TR2Type.Rat, TR2Type.Spider, TR2Type.Winston
        },
        [EnemyDifficulty.Easy] = new List<TR2Type>
        {
            TR2Type.Crow, TR2Type.Eagle, TR2Type.ScubaDiver,
            TR2Type.YellowMorayEel
        },
        [EnemyDifficulty.Medium] = new List<TR2Type>
        {
            TR2Type.Doberman, TR2Type.GiantSpider, TR2Type.Gunman1,
            TR2Type.Gunman2, TR2Type.Knifethrower, TR2Type.MaskedGoon1,
            TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.Shark,
            TR2Type.StickWieldingGoon1, TR2Type.StickWieldingGoon2, TR2Type.TigerOrSnowLeopard,
            TR2Type.TRex
        },
        [EnemyDifficulty.Hard] = new List<TR2Type>
        {
            TR2Type.BlackMorayEel, TR2Type.FlamethrowerGoon, TR2Type.Mercenary1,
            TR2Type.Mercenary2, TR2Type.Mercenary3, TR2Type.ShotgunGoon,
            TR2Type.XianGuardSpear, TR2Type.XianGuardSword, TR2Type.Yeti
        },
        [EnemyDifficulty.VeryHard] = new List<TR2Type>
        {
            TR2Type.BirdMonster, TR2Type.MarcoBartoli, TR2Type.MercSnowmobDriver
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
    private static readonly Dictionary<string, int> _enemyAdjustmentCount = new()
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
    private static readonly Dictionary<TR2Type, Dictionary<string, int>> _targetEnemyAdjustmentCount = new()
    {
        [TR2Type.MarcoBartoli] = new Dictionary<string, int>
        {
            [TR2LevelNames.VENICE] = -2,
            [TR2LevelNames.BARTOLI] = -2,
            [TR2LevelNames.OPERA] = -2,
            [TR2LevelNames.DA] = -1,
            [TR2LevelNames.TIBET] = -1,
            [TR2LevelNames.FLOATER] = -1,
        }
    };

    public static List<TR2Type> GetEnemyGuisers(TR2Type entity)
    {
        List<TR2Type> entities = new();
        if (_enemyGuisers.ContainsKey(entity))
        {
            entities.AddRange(_enemyGuisers[entity]);
        }
        return entities;
    }

    private static readonly Dictionary<TR2Type, List<TR2Type>> _enemyGuisers = new()
    {
        [TR2Type.BirdMonster] = new List<TR2Type>
        {
            TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick
        }
    };

    public static List<TR2Type> GetFriendlyEnemies()
    {
        return new List<TR2Type>(_friendlyEnemies);
    }

    private static readonly List<TR2Type> _friendlyEnemies = new()
    {
        TR2Type.Winston, TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick
    };

    // #146 Ensure Marco is spawned only once
    private static readonly List<TR2Type> _oneShotEnemies = new()
    {
        TR2Type.MarcoBartoli
    };

    public static void SetEntityTriggers(TR2Level level, TR2Entity entity)
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

    public static Dictionary<TR2Type, TR2Type> GetAliasPriority(string lvlName, List<TR2Type> importEntities)
    {
        // If the priorities map doesn't contain an entity we are trying to import as a key, TRModelTransporter
        // will assume it always has priority (e.g. BengalTiger replacing SnowLeopard).
        Dictionary<TR2Type, TR2Type> priorities = new();

        // If the dragon is being imported, we want the matching dagger cutscene to be available via misc anim
        // and the dagger model for the inventory. Otherwise, we need to ensure the existing misc anim matches
        // level specifics. This is currently targeted at offshore wheel door levels, Ice Palace gong action and
        // HSH starting cutscene. For others we sacrifice the specific enemy death animations. So for example,
        // if XianGuardSpear is imported into Wreck, the existing misc anim will remain for the wheel door animation.
        // But if Marco is imported, the wheel door animation will also be sacrificed.
        if (importEntities.Contains(TR2Type.MarcoBartoli) && lvlName != TR2LevelNames.HOME)
        {
            priorities[TR2Type.Puzzle2_M_H] = TR2Type.Puzzle2_M_H_Dagger;
            priorities[TR2Type.LaraMiscAnim_H] = TR2Type.LaraMiscAnim_H_Xian;
        }
        else
        {
            switch (lvlName)
            {
                case TR2LevelNames.BARTOLI:
                    priorities[TR2Type.LaraMiscAnim_H] = TR2Type.LaraMiscAnim_H_Venice;
                    break;
                case TR2LevelNames.RIG:
                case TR2LevelNames.DA:
                case TR2LevelNames.DORIA:
                    priorities[TR2Type.LaraMiscAnim_H] = TR2Type.LaraMiscAnim_H_Unwater;
                    break;
                case TR2LevelNames.CHICKEN:
                    priorities[TR2Type.LaraMiscAnim_H] = TR2Type.LaraMiscAnim_H_Ice;
                    break;
                case TR2LevelNames.HOME:
                    priorities[TR2Type.LaraMiscAnim_H] = TR2Type.LaraMiscAnim_H_HSH;
                    break;
            }
        }

        return priorities;
    }
}
