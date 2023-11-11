using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TR1TypeUtilities
{
    public static readonly Dictionary<TR1Type, Dictionary<TR1Type, List<string>>> LevelAliases = new()
    {
        [TR1Type.FlyingAtlantean] = new()
        {
            [TR1Type.BandagedFlyer] =
                new() { TR1LevelNames.KHAMOON, TR1LevelNames.OBELISK },
            [TR1Type.MeatyFlyer] =
                new() { TR1LevelNames.SANCTUARY, TR1LevelNames.ATLANTIS }
        },
        [TR1Type.NonShootingAtlantean_N] = new()
        {
            [TR1Type.BandagedAtlantean] =
                new() { TR1LevelNames.KHAMOON, TR1LevelNames.OBELISK },
            [TR1Type.MeatyAtlantean] =
                new() { TR1LevelNames.SANCTUARY, TR1LevelNames.ATLANTIS }
        },
        [TR1Type.Cowboy] = new()
        {
            [TR1Type.CowboyOG] =
                new() { TR1LevelNames.MINES },
            [TR1Type.CowboyHeadless] =
                new()
        }
    };

    public static readonly Dictionary<TR1Type, List<TR1Type>> TypeFamilies = new()
    {
        [TR1Type.FlyingAtlantean] = new()
        {
            TR1Type.BandagedFlyer, TR1Type.MeatyFlyer
        },
        [TR1Type.NonShootingAtlantean_N] = new()
        {
            TR1Type.BandagedAtlantean, TR1Type.MeatyAtlantean
        },
        [TR1Type.Cowboy] = new()
        {
            TR1Type.CowboyOG, TR1Type.CowboyHeadless
        }
    };

    public static TR1Type TranslateAlias(TR1Type type)
    {
        foreach (TR1Type parentType in TypeFamilies.Keys)
        {
            if (TypeFamilies[parentType].Contains(type))
            {
                return parentType;
            }
        }

        return type;
    }

    public static TR1Type GetAliasForLevel(string lvl, TR1Type type)
    {
        if (LevelAliases.ContainsKey(type))
        {
            foreach (TR1Type alias in LevelAliases[type].Keys)
            {
                if (LevelAliases[type][alias].Contains(lvl))
                {
                    return alias;
                }
            }
        }
        return type;
    }

    public static List<TR1Type> GetFamily(TR1Type type)
    {
        foreach (TR1Type parentType in TypeFamilies.Keys)
        {
            if (TypeFamilies[parentType].Contains(type))
            {
                return TypeFamilies[parentType];
            }
        }

        return new() { type };
    }

    public static List<TR1Type> RemoveAliases(IEnumerable<TR1Type> types)
    {
        List<TR1Type> normalisedTypes = new();
        foreach (TR1Type type in types)
        {
            TR1Type normalisedType = TranslateAlias(type);
            if (!normalisedTypes.Contains(normalisedType))
            {
                normalisedTypes.Add(normalisedType);
            }
        }
        return normalisedTypes;
    }

    public static List<TR1Type> GetKeyTypes()
    {
        return new()
        {
            TR1Type.Key1_S_P,
            TR1Type.Key2_S_P,
            TR1Type.Key3_S_P,
            TR1Type.Key4_S_P
        };
    }

    public static List<TR1Type> GetPuzzleTypes()
    {
        return new()
        {
            TR1Type.Puzzle1_S_P,
            TR1Type.Puzzle2_S_P,
            TR1Type.Puzzle3_S_P,
            TR1Type.Puzzle4_S_P
        };
    }

    public static List<TR1Type> GetQuestTypes()
    {
        return new()
        {
            TR1Type.Quest1_S_P,
            TR1Type.Quest2_S_P
        };
    }

    public static List<TR1Type> GetLeadTypes()
    {
        return new()
        {
            TR1Type.LeadBar_S_P
        };
    }

    public static List<TR1Type> GetKeyItemTypes()
    {
        return GetKeyTypes()
                .Concat(GetPuzzleTypes())
                .Concat(GetQuestTypes())
                .Concat(GetLeadTypes())
                .ToList();
    }

    public static Dictionary<TR1Type, TR1Type> GetKeyItemMap()
    {
        // Maps pickups to inventory models
        return new()
        {
            [TR1Type.Key1_S_P] = TR1Type.Key1_M_H,
            [TR1Type.Key2_S_P] = TR1Type.Key2_M_H,
            [TR1Type.Key3_S_P] = TR1Type.Key3_M_H,
            [TR1Type.Key4_S_P] = TR1Type.Key4_M_H,
            [TR1Type.Puzzle1_S_P] = TR1Type.Puzzle1_M_H,
            [TR1Type.Puzzle2_S_P] = TR1Type.Puzzle2_M_H,
            [TR1Type.Puzzle3_S_P] = TR1Type.Puzzle3_M_H,
            [TR1Type.Puzzle4_S_P] = TR1Type.Puzzle4_M_H,
            [TR1Type.Quest1_S_P] = TR1Type.Quest1_M_H,
            [TR1Type.Quest2_S_P] = TR1Type.Quest2_M_H,
            [TR1Type.LeadBar_S_P] = TR1Type.LeadBar_M_H,
            [TR1Type.ScionPiece1_S_P] = TR1Type.ScionPiece_M_H,
            [TR1Type.ScionPiece2_S_P] = TR1Type.ScionPiece_M_H,
        };
    }

    public static bool IsKeyType(TR1Type type)
    {
        return GetKeyTypes().Contains(type);
    }

    public static bool IsPuzzleType(TR1Type type)
    {
        return GetPuzzleTypes().Contains(type);
    }

    public static bool IsQuestType(TR1Type type)
    {
        return GetQuestTypes().Contains(type);
    }

    public static bool IsKeyItemType(TR1Type type)
    {
        return GetKeyItemTypes().Contains(type);
    }

    public static bool IsTrapdoor(TR1Type type)
    {
        return GetTrapdoorTypes().Contains(type);
    }

    public static bool IsBridge(TR1Type type)
    {
        return GetBridgeTypes().Contains(type);
    }

    public static List<TR1Type> GetTrapdoorTypes()
    {
        return new()
        {
            TR1Type.Trapdoor1, TR1Type.Trapdoor2, TR1Type.Trapdoor3
        };
    }

    public static List<TR1Type> GetBridgeTypes()
    {
        return new()
        {
            TR1Type.BridgeFlat, TR1Type.BridgeTilt1, TR1Type.BridgeTilt2
        };
    }

    public static List<TR1Type> GetStandardPickupTypes()
    {
        return new()
        {
            TR1Type.Pistols_S_P,
            TR1Type.Shotgun_S_P,
            TR1Type.Magnums_S_P,
            TR1Type.Uzis_S_P,
            TR1Type.PistolAmmo_S_P,
            TR1Type.ShotgunAmmo_S_P,
            TR1Type.MagnumAmmo_S_P,
            TR1Type.UziAmmo_S_P,
            TR1Type.SmallMed_S_P,
            TR1Type.LargeMed_S_P
        };
    }

    public static bool IsStandardPickupType(TR1Type type)
    {
        return GetStandardPickupTypes().Contains(type);
    }

    public static bool IsWeaponPickup(TR1Type type)
    {
        return GetWeaponPickups().Contains(type);
    }

    public static List<TR1Type> GetWeaponPickups()
    {
        return new()
        {
            TR1Type.Pistols_S_P,
            TR1Type.Shotgun_S_P,
            TR1Type.Magnums_S_P,
            TR1Type.Uzis_S_P
        };
    }

    public static bool IsAmmoPickup(TR1Type type)
    {
        return type == TR1Type.PistolAmmo_S_P
            || type == TR1Type.ShotgunAmmo_S_P
            || type == TR1Type.MagnumAmmo_S_P
            || type == TR1Type.UziAmmo_S_P;
    }

    public static TR1Type GetWeaponAmmo(TR1Type weapon)
    {
        return weapon switch
        {
            TR1Type.Shotgun_S_P => TR1Type.ShotgunAmmo_S_P,
            TR1Type.Magnums_S_P => TR1Type.MagnumAmmo_S_P,
            TR1Type.Uzis_S_P => TR1Type.UziAmmo_S_P,
            _ => TR1Type.PistolAmmo_S_P,
        };
    }

    public static bool IsCrystalPickup(TR1Type type)
    {
        return type == TR1Type.SavegameCrystal_P;
    }

    public static bool IsUtilityPickup(TR1Type type)
    {
        return type == TR1Type.SmallMed_S_P
            || type == TR1Type.LargeMed_S_P;
    }

    public static bool IsAnyPickupType(TR1Type type)
    {
        return IsUtilityPickup(type)
            || IsAmmoPickup(type)
            || IsWeaponPickup(type)
            || IsKeyItemType(type);
    }

    public static List<TR1Type> GetCandidateCrossLevelEnemies()
    {
        return new()
        {
            TR1Type.Adam,
            TR1Type.AtlanteanEgg,
            TR1Type.BandagedAtlantean,
            TR1Type.BandagedFlyer,
            TR1Type.Bat,
            TR1Type.Bear,
            TR1Type.Centaur,
            TR1Type.CentaurStatue,
            TR1Type.CowboyOG,
            TR1Type.CowboyHeadless,
            TR1Type.CrocodileLand,
            TR1Type.CrocodileWater,                
            TR1Type.Gorilla,
            TR1Type.Kold,
            TR1Type.Larson,
            TR1Type.Lion,
            TR1Type.Lioness,
            TR1Type.MeatyAtlantean,
            TR1Type.MeatyFlyer,
            TR1Type.Natla,
            TR1Type.Panther,
            TR1Type.Pierre,
            TR1Type.Raptor,
            TR1Type.RatLand,
            TR1Type.RatWater,
            TR1Type.SkateboardKid,
            TR1Type.TRex,
            TR1Type.Wolf
        };
    }

    public static bool IsEnemyType(TR1Type type)
    {
        return GetFullListOfEnemies().Contains(type);
    }

    public static List<TR1Type> GetFullListOfEnemies()
    {
        List<TR1Type> enemies = new()
        {
            // This ensures aliases are covered
            TR1Type.FlyingAtlantean, TR1Type.NonShootingAtlantean_N, TR1Type.ShootingAtlantean_N, TR1Type.Cowboy
        };

        enemies.AddRange(GetCandidateCrossLevelEnemies());
        return enemies;
    }

    public static List<TR1Type> GetWaterEnemies()
    {
        return new()
        {
            TR1Type.CrocodileWater,
            TR1Type.RatWater
        };
    }

    public static Dictionary<TR1Type, TR1Type> GetWaterEnemyLandCreatures()
    {
        return new()
        {
            [TR1Type.CrocodileWater] = TR1Type.CrocodileLand,
            [TR1Type.RatWater] = TR1Type.RatLand
        };
    }

    public static TR1Type GetWaterEnemyLandCreature(TR1Type type)
    {
        Dictionary<TR1Type, TR1Type> types = GetWaterEnemyLandCreatures();
        return types.ContainsKey(type) ? types[type] : type;
    }

    public static bool IsWaterCreature(TR1Type type)
    {
        return GetWaterEnemies().Contains(type);
    }

    public static bool IsWaterLandCreatureEquivalent(TR1Type type)
    {
        return GetWaterEnemyLandCreatures().ContainsValue(type);
    }

    public static List<TR1Type> GetWaterLandCreatures()
    {
        return GetWaterEnemyLandCreatures().Values.ToList();
    }

    public static List<TR1Type> FilterWaterEnemies(List<TR1Type> types)
    {
        List<TR1Type> waterTypes = new();
        foreach (TR1Type type in types)
        {
            if (IsWaterCreature(type))
            {
                waterTypes.Add(type);
            }
        }
        return waterTypes;
    }

    public static List<TR1Type> GetAtlanteanEggEnemies()
    {
        return new()
        {
            TR1Type.BandagedAtlantean,
            TR1Type.BandagedFlyer,
            TR1Type.Centaur,
            TR1Type.MeatyAtlantean,
            TR1Type.MeatyFlyer,
            TR1Type.ShootingAtlantean_N
        };
    }

    public static List<TR1Type> GetSwitchTypes()
    {
        return new()
        {
            TR1Type.WallSwitch,
            TR1Type.UnderwaterSwitch
        };
    }

    public static bool IsSwitchType(TR1Type type)
    {
        return GetSwitchTypes().Contains(type);
    }

    public static List<TR1Type> GetKeyholeTypes()
    {
        return new()
        {
            TR1Type.Keyhole1,
            TR1Type.Keyhole2,
            TR1Type.Keyhole3,
            TR1Type.Keyhole4
        };
    }

    public static bool IsKeyholeType(TR1Type type)
    {
        return GetKeyholeTypes().Contains(type);
    }

    public static List<TR1Type> GetSlotTypes()
    {
        return new()
        {
            TR1Type.PuzzleHole1,
            TR1Type.PuzzleHole2,
            TR1Type.PuzzleHole3,
            TR1Type.PuzzleHole4,
            TR1Type.PuzzleDone1,
            TR1Type.PuzzleDone2,
            TR1Type.PuzzleDone3,
            TR1Type.PuzzleDone4
        };
    }

    public static bool IsSlotType(TR1Type type)
    {
        return GetSlotTypes().Contains(type);
    }

    public static List<TR1Type> GetPushblockTypes()
    {
        return new()
        {
            TR1Type.PushBlock1,
            TR1Type.PushBlock2,
            TR1Type.PushBlock3,
            TR1Type.PushBlock4
        };
    }

    public static bool IsPushblockType(TR1Type type)
    {
        return GetPushblockTypes().Contains(type);
    }

    public static List<TR1Type> GetCogMachineTypes()
    {
        return new()
        {
            TR1Type.Animating1,
            TR1Type.Animating2,
            TR1Type.Animating3,
        };
    }

    public static bool IsCogMachineType(TR1Type type)
    {
        return GetCogMachineTypes().Contains(type);
    }

    public static bool CanSharePickupSpace(TR1Type type)
    {
        // Can we place a standard pickup on the same tile as this type?
        return IsAnyPickupType(type)
            || IsCrystalPickup(type)
            || IsSwitchType(type)
            || IsKeyholeType(type)
            || IsSlotType(type)
            || IsEnemyType(type)
            || IsBridge(type)
            || IsTrapdoor(type)
            || IsDoorType(type)
            || IsPushblockType(type)
            || IsCogMachineType(type)
            || type == TR1Type.CameraTarget_N
            || type == TR1Type.Earthquake_N
            || type == TR1Type.WaterfallMist_N
            || type == TR1Type.FallingBlock
            || type == TR1Type.Barricade
            || type == TR1Type.RollingBall
            || type == TR1Type.MovingBlock
            || type == TR1Type.SwingingBlade
            || type == TR1Type.Lara;
    }

    public static List<TR1Type> DoorTypes()
    {
        return new()
        {
            TR1Type.Door1, TR1Type.Door2, TR1Type.Door3,
            TR1Type.Door4, TR1Type.Door5, TR1Type.Door6,
            TR1Type.Door7, TR1Type.Door8
        };
    }

    public static bool IsTrapdoorType(TR1Type type)
    {
        return TrapdoorTypes().Contains(type);
    }

    public static List<TR1Type> TrapdoorTypes()
    {
        return new()
        {
            TR1Type.Trapdoor1,
            TR1Type.Trapdoor2,
            TR1Type.Trapdoor3,
        };
    }

    public static bool IsDoorType(TR1Type type)
    {
        return DoorTypes().Contains(type);
    }

    public static Dictionary<TR1Type, TR1Type> GetSecretModels()
    {
        return new()
        {
            [TR1Type.SecretAnkh_M_H] = TR1Type.ScionPiece4_S_P,
            [TR1Type.SecretGoldBar_M_H] = TR1Type.ScionPiece4_S_P,
            [TR1Type.SecretGoldIdol_M_H] = TR1Type.ScionPiece4_S_P,
            [TR1Type.SecretLeadBar_M_H] = TR1Type.ScionPiece4_S_P,
            [TR1Type.SecretScion_M_H] = TR1Type.ScionPiece4_S_P
        };
    }

    public static Dictionary<TR1Type, TR1Type> GetSecretReplacements()
    {
        // Note Key1 is omitted because of Pierre
        return new()
        {
            [TR1Type.Puzzle1_M_H] = TR1Type.Puzzle1_S_P,
            [TR1Type.Puzzle2_M_H] = TR1Type.Puzzle2_S_P,
            [TR1Type.Puzzle3_M_H] = TR1Type.Puzzle3_S_P,
            [TR1Type.Puzzle4_M_H] = TR1Type.Puzzle4_S_P,
            [TR1Type.Key2_M_H] = TR1Type.Key2_S_P,
            [TR1Type.Key3_M_H] = TR1Type.Key3_S_P,
            [TR1Type.Key4_M_H] = TR1Type.Key4_S_P,
            [TR1Type.Quest1_M_H] = TR1Type.Quest1_S_P,
            [TR1Type.Quest2_M_H] = TR1Type.Quest2_S_P
        };
    }

    public static TR1Type GetBestLevelSecretModel(string lvl)
    {
        return _levelSecretModels.ContainsKey(lvl)
            ? _levelSecretModels[lvl]
            : GetSecretModels().Keys.FirstOrDefault();
    }

    private static readonly Dictionary<string, TR1Type> _levelSecretModels = new()
    {
        [TR1LevelNames.CAVES]
            = TR1Type.SecretScion_M_H,
        [TR1LevelNames.VILCABAMBA]
            = TR1Type.SecretGoldBar_M_H,
        [TR1LevelNames.VALLEY]
            = TR1Type.SecretGoldIdol_M_H,
        [TR1LevelNames.QUALOPEC]
            = TR1Type.SecretGoldIdol_M_H,
        [TR1LevelNames.FOLLY]
            = TR1Type.SecretLeadBar_M_H,
        [TR1LevelNames.COLOSSEUM]
            = TR1Type.SecretLeadBar_M_H,
        [TR1LevelNames.MIDAS]
            = TR1Type.SecretAnkh_M_H,
        [TR1LevelNames.CISTERN]
            = TR1Type.SecretScion_M_H,
        [TR1LevelNames.TIHOCAN]
            = TR1Type.SecretGoldIdol_M_H,
        [TR1LevelNames.KHAMOON]
            = TR1Type.SecretLeadBar_M_H,
        [TR1LevelNames.OBELISK]
            = TR1Type.SecretLeadBar_M_H,
        [TR1LevelNames.SANCTUARY]
            = TR1Type.SecretAnkh_M_H,
        [TR1LevelNames.MINES]
            = TR1Type.SecretGoldBar_M_H,
        [TR1LevelNames.ATLANTIS]
            = TR1Type.SecretGoldIdol_M_H,
        [TR1LevelNames.PYRAMID]
            = TR1Type.SecretGoldIdol_M_H
    };
}
