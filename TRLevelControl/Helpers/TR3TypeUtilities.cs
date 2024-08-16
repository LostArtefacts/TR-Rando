using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TR3TypeUtilities
{
    public static readonly Dictionary<TR3Type, Dictionary<TR3Type, List<string>>> LevelAliases = new()
    {
        [TR3Type.Lara] = new()
        {
            [TR3Type.LaraIndia]
                = TR3LevelNames.IndiaWithCutscenes,
            [TR3Type.LaraCoastal]
                = TR3LevelNames.SouthPacificWithCutscenes,
            [TR3Type.LaraLondon]
                = TR3LevelNames.LondonWithCutscenes,
            [TR3Type.LaraNevada]
                = TR3LevelNames.NevadaWithCutscenes,
            [TR3Type.LaraAntarc]
                = TR3LevelNames.AntarcticaWithCutscenes,
            [TR3Type.LaraHome]
                = new() { TR3LevelNames.ASSAULT }
        },
        [TR3Type.LaraSkin_H] = new()
        {
            [TR3Type.LaraSkin_H_India]
                = TR3LevelNames.IndiaWithCutscenes,
            [TR3Type.LaraSkin_H_Coastal]
                = TR3LevelNames.SouthPacificWithCutscenes,
            [TR3Type.LaraSkin_H_London]
                = TR3LevelNames.LondonWithCutscenes,
            [TR3Type.LaraSkin_H_Nevada]
                = TR3LevelNames.NevadaWithCutscenes,
            [TR3Type.LaraSkin_H_Antarc]
                = TR3LevelNames.AntarcticaWithCutscenes,
            [TR3Type.LaraSkin_H_Home]
                = new() { TR3LevelNames.ASSAULT }
        },
        [TR3Type.Cobra] = new()
        {
            [TR3Type.CobraIndia]
                = new() { TR3LevelNames.RUINS, TR3LevelNames.GANGES, TR3LevelNames.CAVES },
            [TR3Type.CobraNevada]
                = new() { TR3LevelNames.NEVADA },
        },
        [TR3Type.Dog] = new()
        {
            [TR3Type.DogLondon] 
                = new() { TR3LevelNames.ALDWYCH },
            [TR3Type.DogNevada]
                = new() { TR3LevelNames.HSC, TR3LevelNames.HALLOWS }
        },
    };

    public static List<TR3Type> GetFamily(TR3Type type)
    {
        foreach (TR3Type parentType in LevelAliases.Keys)
        {
            if (LevelAliases[parentType].ContainsKey(type))
            {
                return LevelAliases[parentType].Keys.ToList();
            }
        }

        return new() { type };
    }

    public static TR3Type TranslateAlias(TR3Type type)
    {
        foreach (TR3Type parentType in LevelAliases.Keys)
        {
            if (LevelAliases[parentType].ContainsKey(type))
            {
                return parentType;
            }
        }

        return type;
    }

    public static TR3Type GetAliasForLevel(string lvl, TR3Type type)
    {
        if (LevelAliases.ContainsKey(type))
        {
            foreach (TR3Type alias in LevelAliases[type].Keys)
            {
                if (LevelAliases[type][alias].Contains(lvl))
                {
                    return alias;
                }
            }
        }
        return type;
    }

    public static List<TR3Type> RemoveAliases(IEnumerable<TR3Type> types)
    {
        List<TR3Type> normalisedTypes = new();
        foreach (TR3Type type in types)
        {
            TR3Type normalisedType = TranslateAlias(type);
            if (!normalisedTypes.Contains(normalisedType))
            {
                normalisedTypes.Add(normalisedType);
            }
        }
        return normalisedTypes;
    }

    public static List<TR3Type> GetLaraTypes()
    {
        return new()
        {
            TR3Type.LaraIndia, TR3Type.LaraCoastal, TR3Type.LaraLondon, TR3Type.LaraNevada, TR3Type.LaraAntarc, TR3Type.LaraInvisible
        };
    }

    public static List<TR3Type> GetKeyTypes()
    {
        return new()
        {
            TR3Type.Key1_P,
            TR3Type.Key2_P,
            TR3Type.Key3_P,
            TR3Type.Key4_P
        };
    }

    public static List<TR3Type> GetPuzzleTypes()
    {
        return new()
        {
            TR3Type.Puzzle1_P,
            TR3Type.Puzzle2_P,
            TR3Type.Puzzle3_P,
            TR3Type.Puzzle4_P
        };
    }

    public static List<TR3Type> GetQuestTypes()
    {
        return new()
        {
            TR3Type.Quest1_P,
            TR3Type.Quest2_P
        };
    }

    public static List<TR3Type> GetKeyItemTypes()
    {
        return GetKeyTypes().Concat(GetPuzzleTypes()).Concat(GetQuestTypes()).ToList();
    }

    public static bool IsKeyType(TR3Type type)
    {
        return GetKeyTypes().Contains(type);
    }

    public static bool IsPuzzleType(TR3Type type)
    {
        return GetPuzzleTypes().Contains(type);
    }

    public static bool IsQuestType(TR3Type type)
    {
        return GetQuestTypes().Contains(type);
    }

    public static bool IsKeyItemType(TR3Type type)
    {
        return GetKeyItemTypes().Contains(type);
    }

    public static Dictionary<TR3Type, TR3Type> GetArtefactPickups()
    {
        return new Dictionary<TR3Type, TR3Type>
        {
            [TR3Type.Infada_P] = TR3Type.Infada_M_H,
            [TR3Type.OraDagger_P] = TR3Type.OraDagger_M_H,
            [TR3Type.EyeOfIsis_P] = TR3Type.EyeOfIsis_M_H,
            [TR3Type.Element115_P] = TR3Type.Element115_M_H,
            [TR3Type.Quest1_P] = TR3Type.Quest1_M_H, // Serpent Stone
            [TR3Type.Quest2_P] = TR3Type.Quest2_M_H  // Hand of Rathmore
        };
    }

    public static List<TR3Type> GetArtefactMenuModels()
    {
        return new()
        {
            TR3Type.Infada_M_H, TR3Type.OraDagger_M_H, TR3Type.EyeOfIsis_M_H, TR3Type.Element115_M_H
        };
    }

    public static Dictionary<TR3Type, TR3Type> GetArtefactReplacements()
    {
        return new Dictionary<TR3Type, TR3Type>
        {
            [TR3Type.Puzzle1_P] = TR3Type.Puzzle1_M_H,
            [TR3Type.Puzzle2_P] = TR3Type.Puzzle2_M_H,
            [TR3Type.Puzzle3_P] = TR3Type.Puzzle3_M_H,
            [TR3Type.Puzzle4_P] = TR3Type.Puzzle4_M_H,
            [TR3Type.Key1_P] = TR3Type.Key1_M_H,
            [TR3Type.Key2_P] = TR3Type.Key2_M_H,
            [TR3Type.Key3_P] = TR3Type.Key3_M_H,
            [TR3Type.Key4_P] = TR3Type.Key4_M_H,
            [TR3Type.Quest1_P] = TR3Type.Quest1_M_H,
            [TR3Type.Quest2_P] = TR3Type.Quest2_M_H
        };
    }

    public static bool IsTrapdoor(TR3Type type)
    {
        return GetTrapdoorTypes().Contains(type);
    }

    public static bool IsBridge(TR3Type type)
    {
        return GetBridgeTypes().Contains(type);
    }

    public static List<TR3Type> GetTrapdoorTypes()
    {
        return new()
        {
            TR3Type.Trapdoor1, TR3Type.Trapdoor2, TR3Type.Trapdoor3
        };
    }

    public static List<TR3Type> GetBridgeTypes()
    {
        return new()
        {
            TR3Type.BridgeFlat, TR3Type.BridgeTilt1, TR3Type.BridgeTilt2
        };
    }

    public static List<TR3Type> GetStandardPickupTypes()
    {
        return new()
        {
            TR3Type.Pistols_P,
            TR3Type.Shotgun_P,
            TR3Type.Deagle_P,
            TR3Type.Uzis_P,
            TR3Type.Harpoon_P,
            TR3Type.MP5_P,
            TR3Type.RocketLauncher_P,
            TR3Type.GrenadeLauncher_P,
            TR3Type.PistolAmmo_P,
            TR3Type.ShotgunAmmo_P,
            TR3Type.DeagleAmmo_P,
            TR3Type.UziAmmo_P,
            TR3Type.Harpoons_P,
            TR3Type.MP5Ammo_P,
            TR3Type.Rockets_P,
            TR3Type.Grenades_P,
            TR3Type.SmallMed_P,
            TR3Type.LargeMed_P,
            TR3Type.Flares_P,
        };
    }

    public static bool IsStandardPickupType(TR3Type type)
    {
        return GetStandardPickupTypes().Contains(type);
    }

    public static bool IsWeaponPickup(TR3Type type)
    {
        return (type == TR3Type.Pistols_P)
            || (type == TR3Type.Shotgun_P)
            || (type == TR3Type.Deagle_P)
            || (type == TR3Type.Uzis_P)
            || (type == TR3Type.Harpoon_P)
            || (type == TR3Type.MP5_P)
            || (type == TR3Type.RocketLauncher_P)
            || (type == TR3Type.GrenadeLauncher_P);
    }

    public static List<TR3Type> GetWeaponPickups()
    {
        return new()
        {
            TR3Type.Pistols_P,
            TR3Type.Shotgun_P,
            TR3Type.Deagle_P,
            TR3Type.Uzis_P,
            TR3Type.Harpoon_P,
            TR3Type.MP5_P,
            TR3Type.RocketLauncher_P,
            TR3Type.GrenadeLauncher_P
        };
    }

    public static bool IsAmmoPickup(TR3Type type)
    {
        return (type == TR3Type.PistolAmmo_P)
            || (type == TR3Type.ShotgunAmmo_P)
            || (type == TR3Type.DeagleAmmo_P)
            || (type == TR3Type.UziAmmo_P)
            || (type == TR3Type.Harpoons_P)
            || (type == TR3Type.MP5Ammo_P)
            || (type == TR3Type.Rockets_P)
            || (type == TR3Type.Grenades_P);
    }

    public static TR3Type GetWeaponAmmo(TR3Type weapon)
    {
        return weapon switch
        {
            TR3Type.Shotgun_P => TR3Type.ShotgunAmmo_P,
            TR3Type.Deagle_P => TR3Type.DeagleAmmo_P,
            TR3Type.Uzis_P => TR3Type.UziAmmo_P,
            TR3Type.Harpoon_P => TR3Type.Harpoons_P,
            TR3Type.MP5_P => TR3Type.MP5Ammo_P,
            TR3Type.GrenadeLauncher_P => TR3Type.Grenades_P,
            TR3Type.RocketLauncher_P => TR3Type.Rockets_P,
            _ => TR3Type.PistolAmmo_P,
        };
    }

    public static bool IsCrystalPickup(TR3Type type)
    {
        return (type == TR3Type.SaveCrystal_P);
    }

    public static bool IsUtilityPickup(TR3Type type)
    {
        return (type == TR3Type.SmallMed_P)
            || (type == TR3Type.LargeMed_P)
            || (type == TR3Type.Flares_P);
    }

    public static bool IsArtefactPickup(TR3Type type)
    {
        return type == TR3Type.Infada_P
            || type == TR3Type.OraDagger_P
            || type == TR3Type.EyeOfIsis_P
            || type == TR3Type.Element115_P;
    }

    public static bool IsAnyPickupType(TR3Type type)
    {
        return IsUtilityPickup(type)
            || IsAmmoPickup(type)
            || IsWeaponPickup(type)
            || IsKeyItemType(type)
            || IsArtefactPickup(type);
    }

    public static bool IsVehicleType(TR3Type typeType)
    {
        return typeType == TR3Type.Quad
            || typeType == TR3Type.Kayak
            || typeType == TR3Type.UPV
            || typeType == TR3Type.Boat
            || typeType == TR3Type.MineCart;
    }

    public static List<TR3Type> GetCandidateCrossLevelEnemies()
    {
        return new()
        {
            TR3Type.BruteMutant,
            TR3Type.CobraIndia,
            TR3Type.CobraNevada,
            TR3Type.Compsognathus,
            TR3Type.Crawler,
            TR3Type.Croc,
            TR3Type.Crow,
            TR3Type.DamGuard,
            TR3Type.DogAntarc,
            TR3Type.DogLondon,
            TR3Type.DogNevada,
            TR3Type.KillerWhale,
            TR3Type.LizardMan,
            TR3Type.LondonGuard,
            TR3Type.LondonMerc,
            TR3Type.Mercenary,
            TR3Type.Monkey,
            TR3Type.MPWithGun,
            TR3Type.MPWithMP5,
            TR3Type.MPWithStick,
            TR3Type.Prisoner,
            TR3Type.Punk,
            TR3Type.Raptor,
            TR3Type.Rat,
            TR3Type.RXGunLad,
            TR3Type.RXRedBoi,
            TR3Type.RXTechFlameLad,
            TR3Type.ScubaSteve,
            TR3Type.Shiva,
            TR3Type.Tiger,
            TR3Type.TinnosMonster,
            TR3Type.TinnosWasp,
            TR3Type.TonyFirehands,
            TR3Type.TribesmanAxe,
            TR3Type.TribesmanDart,
            TR3Type.Tyrannosaur,
            TR3Type.Vulture,
            TR3Type.Willie,
            TR3Type.Winston,
            TR3Type.WinstonInCamoSuit
        };
    }

    public static bool IsEnemyType(TR3Type type)
    {
        return GetFullListOfEnemies().Contains(type);
    }

    public static List<TR3Type> GetFullListOfEnemies()
    {
        List<TR3Type> enemies = new()
        {
            TR3Type.SophiaLee, TR3Type.Puna, TR3Type.CrawlerMutantInCloset, TR3Type.Cobra, TR3Type.Dog
        };

        enemies.AddRange(GetCandidateCrossLevelEnemies());
        return enemies;
    }

    public static List<TR3Type> GetWaterEnemies()
    {
        return new()
        {
            TR3Type.Croc,
            TR3Type.KillerWhale,
            TR3Type.ScubaSteve
        };
    }

    public static bool IsWaterCreature(TR3Type type)
    {
        return GetWaterEnemies().Contains(type);
    }

    public static List<TR3Type> FilterWaterEnemies(List<TR3Type> types)
    {
        List<TR3Type> waterTypes = new();
        foreach (TR3Type type in types)
        {
            if (IsWaterCreature(type))
            {
                waterTypes.Add(type);
            }
        }
        return waterTypes;
    }

    public static List<TR3Type> GetKillableWaterEnemies()
    {
        return new()
        {
            TR3Type.Croc,
            TR3Type.ScubaSteve
        };
    }

    public static bool CanDropPickups(TR3Type type, bool protectFriendlyEnemies)
    {
        return GetDroppableEnemies(protectFriendlyEnemies).Contains(type);
    }

    public static List<TR3Type> FilterDroppableEnemies(List<TR3Type> types, bool protectFriendlyEnemies)
    {
        List<TR3Type> droppableTypes = new();
        foreach (TR3Type type in types)
        {
            if (CanDropPickups(type, protectFriendlyEnemies))
            {
                droppableTypes.Add(type);
            }
        }
        return droppableTypes;
    }

    public static List<TR3Type> GetDroppableEnemies(bool protectFriendlyEnemies)
    {
        List<TR3Type> enemies = new()
        {
            TR3Type.BruteMutant,
            TR3Type.CobraIndia,
            TR3Type.CobraNevada,
            TR3Type.Cobra,
            TR3Type.Compsognathus,
            TR3Type.Crawler,
            TR3Type.DamGuard,
            TR3Type.DogAntarc,
            TR3Type.DogLondon,
            TR3Type.DogNevada,
            TR3Type.Dog,
            TR3Type.LizardMan,
            TR3Type.LondonGuard,
            TR3Type.LondonMerc,
            TR3Type.Monkey,
            TR3Type.MPWithGun,
            TR3Type.MPWithMP5,
            TR3Type.MPWithStick,
            TR3Type.Punk,
            TR3Type.Raptor,
            TR3Type.Rat,
            TR3Type.RXGunLad,
            TR3Type.RXRedBoi,
            TR3Type.Shiva,
            TR3Type.SophiaLee,
            TR3Type.Tiger,
            TR3Type.TinnosMonster,
            TR3Type.TonyFirehands,
            TR3Type.TribesmanAxe,
            TR3Type.TribesmanDart,
            TR3Type.Tyrannosaur,
        };

        if (!protectFriendlyEnemies)
        {
            enemies.Add(TR3Type.Mercenary);
            enemies.Add(TR3Type.Prisoner);
            enemies.Add(TR3Type.RXTechFlameLad); // NB Unfriendly if Willie sequence
        }

        return enemies;
    }

    public static List<TR3Type> GetUnrenderedTypes()
    {
        return new()
        {
            TR3Type.AIAmbush_N,
            TR3Type.AICheck_N,
            TR3Type.AIFollow_N,
            TR3Type.AIGuard_N,
            TR3Type.AIModify_N,
            TR3Type.AIPath_N,
            TR3Type.AIPatrol1_N,
            TR3Type.AIPatrol2_N,
            TR3Type.LookAtItem_H,
            TR3Type.KillAllTriggers_N,
            TR3Type.RaptorRespawnPoint_N,
            TR3Type.TinnosWaspRespawnPoint_N,
            TR3Type.EarthQuake_N,
            TR3Type.BatSwarm_N
        };
    }

    public static bool IsUnrenderedType(TR3Type type)
    {
        return GetUnrenderedTypes().Contains(type);
    }

    public static List<TR3Type> GetSwitchTypes()
    {
        return new()
        {
            TR3Type.SmallWallSwitch,
            TR3Type.PushButtonSwitch,
            TR3Type.WallSwitch,
            TR3Type.UnderwaterSwitch,
            TR3Type.ValveWheelOrPulley,
        };
    }

    public static bool IsSwitchType(TR3Type type)
    {
        return GetSwitchTypes().Contains(type);
    }

    public static List<TR3Type> GetKeyholeTypes()
    {
        return new()
        {
            TR3Type.Keyhole1,
            TR3Type.Keyhole2,
            TR3Type.Keyhole3,
            TR3Type.Keyhole4
        };
    }

    public static bool IsKeyholeType(TR3Type type)
    {
        return GetKeyholeTypes().Contains(type);
    }

    public static List<TR3Type> GetSlotTypes()
    {
        return new()
        {
            TR3Type.Slot1Empty,
            TR3Type.Slot2Empty,
            TR3Type.Slot3Empty,
            TR3Type.Slot4Empty,
            TR3Type.Slot1Full,
            TR3Type.Slot2Full,
            TR3Type.Slot3Full,
            TR3Type.Slot4Full
        };
    }

    public static bool IsSlotType(TR3Type type)
    {
        return GetSlotTypes().Contains(type);
    }

    public static List<TR3Type> GetPushblockTypes()
    {
        return new()
        {
            TR3Type.PushableBlock1,
            TR3Type.PushableBlock2
        };
    }

    public static bool IsPushblockType(TR3Type type)
    {
        return GetPushblockTypes().Contains(type);
    }

    public static List<TR3Type> GetLightTypes()
    {
        return new()
        {
            TR3Type.Light_N,
            TR3Type.Light2_N,
            TR3Type.Light3_N,
            TR3Type.Light4_N,
            TR3Type.AlarmLight,
            TR3Type.BlueLight_N,
            TR3Type.GreenLight_N,
            TR3Type.RedLight_N,
            TR3Type.PulsatingLight_N
        };
    }

    public static bool IsLightType(TR3Type type)
    {
        return GetLightTypes().Contains(type);
    }

    public static bool CanSharePickupSpace(TR3Type type)
    {
        // Can we place a standard pickup on the same tile as this type?
        return IsAnyPickupType(type)
            || IsCrystalPickup(type)
            || IsUnrenderedType(type)
            || CanDropPickups(type, true)
            || IsSwitchType(type)
            || IsKeyholeType(type)
            || IsSlotType(type)
            || IsLightType(type)
            || IsBridge(type)
            || IsTrapdoor(type)
            || IsDoorType(type)
            || IsPushblockType(type)
            || (IsVehicleType(type) && type != TR3Type.MineCart)
            || type == TR3Type.FallingBlock
            || type == TR3Type.RollingBallOrBarrel
            || type == TR3Type.ZiplineHandle
            || type == TR3Type.WaterfallMist_H
            || type == TR3Type.DestroyableBoardedUpWindow
            || type == TR3Type.Lara;
    }

    public static List<TR3Type> DoorTypes()
    {
        return new()
        {
            TR3Type.Door1, TR3Type.Door2, TR3Type.Door3,
            TR3Type.Door4, TR3Type.Door5, TR3Type.Door6,
            TR3Type.Door7, TR3Type.Door8
        };
    }

    public static bool IsDoorType(TR3Type type)
    {
        return DoorTypes().Contains(type);
    }
}
