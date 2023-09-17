using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TR3EntityUtilities
{
    public static readonly Dictionary<TR3Type, Dictionary<TR3Type, List<string>>> LevelEntityAliases = new()
    {
        [TR3Type.Lara] = new Dictionary<TR3Type, List<string>>
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
                = new List<string> { TR3LevelNames.ASSAULT }
        },
        [TR3Type.LaraSkin_H] = new Dictionary<TR3Type, List<string>>
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
                = new List<string> { TR3LevelNames.ASSAULT }
        },
        [TR3Type.Cobra] = new Dictionary<TR3Type, List<string>>
        {
            [TR3Type.CobraIndia]
                = new List<string> { TR3LevelNames.RUINS, TR3LevelNames.GANGES, TR3LevelNames.CAVES },
            [TR3Type.CobraNevada]
                = new List<string> { TR3LevelNames.NEVADA },
        },
        [TR3Type.Dog] = new Dictionary<TR3Type, List<string>>
        {
            [TR3Type.DogLondon] 
                = new List<string> { TR3LevelNames.ALDWYCH },
            [TR3Type.DogNevada]
                = new List<string> { TR3LevelNames.HSC, TR3LevelNames.HALLOWS }
        },
    };

    public static List<TR3Type> GetEntityFamily(TR3Type entity)
    {
        foreach (TR3Type parentEntity in LevelEntityAliases.Keys)
        {
            if (LevelEntityAliases[parentEntity].ContainsKey(entity))
            {
                return LevelEntityAliases[parentEntity].Keys.ToList();
            }
        }

        return new List<TR3Type> { entity };
    }

    public static TR3Type TranslateEntityAlias(TR3Type entity)
    {
        foreach (TR3Type parentEntity in LevelEntityAliases.Keys)
        {
            if (LevelEntityAliases[parentEntity].ContainsKey(entity))
            {
                return parentEntity;
            }
        }

        return entity;
    }

    public static TR3Type GetAliasForLevel(string lvl, TR3Type entity)
    {
        if (LevelEntityAliases.ContainsKey(entity))
        {
            foreach (TR3Type alias in LevelEntityAliases[entity].Keys)
            {
                if (LevelEntityAliases[entity][alias].Contains(lvl))
                {
                    return alias;
                }
            }
        }
        return entity;
    }

    public static List<TR3Type> RemoveAliases(IEnumerable<TR3Type> entities)
    {
        List<TR3Type> ents = new();
        foreach (TR3Type ent in entities)
        {
            TR3Type normalisedEnt = TranslateEntityAlias(ent);
            if (!ents.Contains(normalisedEnt))
            {
                ents.Add(normalisedEnt);
            }
        }
        return ents;
    }

    public static List<TR3Type> GetLaraTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.LaraIndia, TR3Type.LaraCoastal, TR3Type.LaraLondon, TR3Type.LaraNevada, TR3Type.LaraAntarc, TR3Type.LaraInvisible
        };
    }

    public static List<TR3Type> GetListOfKeyTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.Key1_P,
            TR3Type.Key2_P,
            TR3Type.Key3_P,
            TR3Type.Key4_P
        };
    }

    public static List<TR3Type> GetListOfPuzzleTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.Puzzle1_P,
            TR3Type.Puzzle2_P,
            TR3Type.Puzzle3_P,
            TR3Type.Puzzle4_P
        };
    }

    public static List<TR3Type> GetListOfQuestTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.Quest1_P,
            TR3Type.Quest2_P
        };
    }

    public static List<TR3Type> GetListOfKeyItemTypes()
    {
        return GetListOfKeyTypes().Concat(GetListOfPuzzleTypes()).Concat(GetListOfQuestTypes()).ToList();
    }

    public static bool IsKeyType(TR3Type entity)
    {
        return GetListOfKeyTypes().Contains(entity);
    }

    public static bool IsPuzzleType(TR3Type entity)
    {
        return GetListOfPuzzleTypes().Contains(entity);
    }

    public static bool IsQuestType(TR3Type entity)
    {
        return GetListOfQuestTypes().Contains(entity);
    }

    public static bool IsKeyItemType(TR3Type entity)
    {
        return GetListOfKeyItemTypes().Contains(entity);
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
        return new List<TR3Type>
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

    public static bool IsTrapdoor(TR3Type entity)
    {
        return GetTrapdoorTypes().Contains(entity);
    }

    public static bool IsBridge(TR3Type entity)
    {
        return GetBridgeTypes().Contains(entity);
    }

    public static List<TR3Type> GetTrapdoorTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.Trapdoor1, TR3Type.Trapdoor2, TR3Type.Trapdoor3
        };
    }

    public static List<TR3Type> GetBridgeTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.BridgeFlat, TR3Type.BridgeTilt1, TR3Type.BridgeTilt2
        };
    }

    public static List<TR3Type> GetStandardPickupTypes()
    {
        return new List<TR3Type>
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

    public static bool IsStandardPickupType(TR3Type entity)
    {
        return GetStandardPickupTypes().Contains(entity);
    }

    public static bool IsWeaponPickup(TR3Type entity)
    {
        return (entity == TR3Type.Pistols_P)
            || (entity == TR3Type.Shotgun_P)
            || (entity == TR3Type.Deagle_P)
            || (entity == TR3Type.Uzis_P)
            || (entity == TR3Type.Harpoon_P)
            || (entity == TR3Type.MP5_P)
            || (entity == TR3Type.RocketLauncher_P)
            || (entity == TR3Type.GrenadeLauncher_P);
    }

    public static List<TR3Type> GetWeaponPickups()
    {
        return new List<TR3Type>
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

    public static bool IsAmmoPickup(TR3Type entity)
    {
        return (entity == TR3Type.PistolAmmo_P)
            || (entity == TR3Type.ShotgunAmmo_P)
            || (entity == TR3Type.DeagleAmmo_P)
            || (entity == TR3Type.UziAmmo_P)
            || (entity == TR3Type.Harpoons_P)
            || (entity == TR3Type.MP5Ammo_P)
            || (entity == TR3Type.Rockets_P)
            || (entity == TR3Type.Grenades_P);
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

    public static bool IsCrystalPickup(TR3Type entity)
    {
        return (entity == TR3Type.SaveCrystal_P);
    }

    public static bool IsUtilityPickup(TR3Type entity)
    {
        return (entity == TR3Type.SmallMed_P)
            || (entity == TR3Type.LargeMed_P)
            || (entity == TR3Type.Flares_P);
    }

    public static bool IsArtefactPickup(TR3Type entity)
    {
        return entity == TR3Type.Infada_P
            || entity == TR3Type.OraDagger_P
            || entity == TR3Type.EyeOfIsis_P
            || entity == TR3Type.Element115_P;
    }

    public static bool IsAnyPickupType(TR3Type entity)
    {
        return IsUtilityPickup(entity)
            || IsAmmoPickup(entity)
            || IsWeaponPickup(entity)
            || IsKeyItemType(entity)
            || IsArtefactPickup(entity);
    }

    public static bool IsVehicleType(TR3Type entityType)
    {
        return entityType == TR3Type.Quad
            || entityType == TR3Type.Kayak
            || entityType == TR3Type.UPV
            || entityType == TR3Type.Boat
            || entityType == TR3Type.MineCart;
    }

    public static List<TR3Type> GetCandidateCrossLevelEnemies()
    {
        return new List<TR3Type>
        {
            TR3Type.BruteMutant,
            TR3Type.CobraIndia,
            TR3Type.CobraNevada,
            TR3Type.Compsognathus,
            TR3Type.Crawler,
            //TR3Entities.CrawlerMutantInCloset, // Dies immediately on activation
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
            //TR3Entities.Puna, // Activates Lizard at hardcoded coordinates, which are OOB in all other levels
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

    public static bool IsEnemyType(TR3Type entity)
    {
        return GetFullListOfEnemies().Contains(entity);
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
        return new List<TR3Type>
        {
            TR3Type.Croc,
            TR3Type.KillerWhale,
            TR3Type.ScubaSteve
        };
    }

    public static bool IsWaterCreature(TR3Type entity)
    {
        return GetWaterEnemies().Contains(entity);
    }

    public static List<TR3Type> FilterWaterEnemies(List<TR3Type> entities)
    {
        List<TR3Type> waterEntities = new();
        foreach (TR3Type entity in entities)
        {
            if (IsWaterCreature(entity))
            {
                waterEntities.Add(entity);
            }
        }
        return waterEntities;
    }

    public static List<TR3Type> GetKillableWaterEnemies()
    {
        return new List<TR3Type>
        {
            TR3Type.Croc,
            TR3Type.ScubaSteve
        };
    }

    public static bool CanDropPickups(TR3Type entity, bool protectFriendlyEnemies)
    {
        return GetDroppableEnemies(protectFriendlyEnemies).Contains(entity);
    }

    public static List<TR3Type> FilterDroppableEnemies(List<TR3Type> entities, bool protectFriendlyEnemies)
    {
        List<TR3Type> droppableEntities = new();
        foreach (TR3Type entity in entities)
        {
            if (CanDropPickups(entity, protectFriendlyEnemies))
            {
                droppableEntities.Add(entity);
            }
        }
        return droppableEntities;
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
            TR3Type.Crow,
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
            TR3Type.TinnosWasp,
            TR3Type.TonyFirehands,
            TR3Type.TribesmanAxe,
            TR3Type.TribesmanDart,
            TR3Type.Tyrannosaur,
            TR3Type.Vulture
        };

        if (!protectFriendlyEnemies)
        {
            enemies.Add(TR3Type.Mercenary);
            enemies.Add(TR3Type.Prisoner);
            enemies.Add(TR3Type.RXTechFlameLad); // NB Unfriendly if Willie sequence
        }

        return enemies;
    }

    public static List<TR3Type> GetUnrenderedEntities()
    {
        return new List<TR3Type>
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

    public static bool IsUnrenderedEntity(TR3Type entity)
    {
        return GetUnrenderedEntities().Contains(entity);
    }

    public static List<TR3Type> GetSwitchTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.SmallWallSwitch,
            TR3Type.PushButtonSwitch,
            TR3Type.WallSwitch,
            TR3Type.UnderwaterSwitch
        };
    }

    public static bool IsSwitchType(TR3Type entity)
    {
        return GetSwitchTypes().Contains(entity);
    }

    public static List<TR3Type> GetKeyholeTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.Keyhole1,
            TR3Type.Keyhole2,
            TR3Type.Keyhole3,
            TR3Type.Keyhole4
        };
    }

    public static bool IsKeyholeType(TR3Type entity)
    {
        return GetKeyholeTypes().Contains(entity);
    }

    public static List<TR3Type> GetSlotTypes()
    {
        return new List<TR3Type>
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

    public static bool IsSlotType(TR3Type entity)
    {
        return GetSlotTypes().Contains(entity);
    }

    public static List<TR3Type> GetPushblockTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.PushableBlock1,
            TR3Type.PushableBlock2
        };
    }

    public static bool IsPushblockType(TR3Type entity)
    {
        return GetPushblockTypes().Contains(entity);
    }

    public static List<TR3Type> GetLightTypes()
    {
        return new List<TR3Type>
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

    public static bool IsLightType(TR3Type entity)
    {
        return GetLightTypes().Contains(entity);
    }

    public static bool CanSharePickupSpace(TR3Type entity)
    {
        // Can we place a standard pickup on the same tile as this entity?
        return IsStandardPickupType(entity)
            || IsCrystalPickup(entity)
            || IsUnrenderedEntity(entity)
            || CanDropPickups(entity, true)
            || IsSwitchType(entity)
            || IsKeyholeType(entity)
            || IsSlotType(entity)
            || IsLightType(entity)
            || entity == TR3Type.Lara;
    }

    public static List<TR3Type> DoorTypes()
    {
        return new List<TR3Type>
        {
            TR3Type.Door1, TR3Type.Door2, TR3Type.Door3,
            TR3Type.Door4, TR3Type.Door5, TR3Type.Door6,
            TR3Type.Door7, TR3Type.Door8
        };
    }

    public static bool IsDoorType(TR3Type entity)
    {
        return DoorTypes().Contains(entity);
    }
}
