using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TR2TypeUtilities
{
    public static readonly Dictionary<TR2Type, Dictionary<TR2Type, List<string>>> LevelAliases = new()
    {
        [TR2Type.Lara] = new()
        {
            [TR2Type.LaraSun] =
                new() { TR2LevelNames.GW, TR2LevelNames.GW_CUT, TR2LevelNames.VENICE, TR2LevelNames.BARTOLI, TR2LevelNames.OPERA, TR2LevelNames.OPERA_CUT, TR2LevelNames.RIG, TR2LevelNames.DA, TR2LevelNames.DA_CUT, TR2LevelNames.XIAN, TR2LevelNames.XIAN_CUT, TR2LevelNames.FLOATER, TR2LevelNames.LAIR },
            [TR2Type.LaraUnwater] =
                new() { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
            [TR2Type.LaraSnow] =
                new() { TR2LevelNames.TIBET, TR2LevelNames.MONASTERY, TR2LevelNames.COT, TR2LevelNames.CHICKEN },
            [TR2Type.LaraHome] =
                new() { TR2LevelNames.HOME },
            [TR2Type.LaraAssault] =
                new() { TR2LevelNames.ASSAULT },
        },
        [TR2Type.LaraMiscAnim_H] = new()
        {
            [TR2Type.LaraMiscAnim_H_Wall] = 
                new() { TR2LevelNames.GW },
            [TR2Type.LaraMiscAnim_H_Venice] = 
                new() { TR2LevelNames.BARTOLI },
            [TR2Type.LaraMiscAnim_H_Unwater] =
                new() { TR2LevelNames.RIG, TR2LevelNames.DA, TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.DECK },
            [TR2Type.LaraMiscAnim_H_Ice] = 
                new() { TR2LevelNames.COT, TR2LevelNames.CHICKEN },
            [TR2Type.LaraMiscAnim_H_Xian] =
                new() { TR2LevelNames.FLOATER, TR2LevelNames.LAIR },
            [TR2Type.LaraMiscAnim_H_HSH] =
                new() { TR2LevelNames.HOME }
        },
        [TR2Type.Barracuda] = new()
        {
            [TR2Type.BarracudaIce] =
                new() { TR2LevelNames.COT, TR2LevelNames.CHICKEN },
            [TR2Type.BarracudaUnwater] =
                new() { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
            [TR2Type.BarracudaXian] =
                new() { TR2LevelNames.XIAN }
        },
        [TR2Type.Shark] = new()
        {
            [TR2Type.SharkOG] = [TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.DECK],
            [TR2Type.SharkGM] = [TR2LevelNames.COLDWAR],
        },
        [TR2Type.StickWieldingGoon1] = new()
        {
            [TR2Type.StickWieldingGoon1Bandana] =
                new() { TR2LevelNames.RIG, TR2LevelNames.DA },
            [TR2Type.StickWieldingGoon1BlackJacket] =
                new() { TR2LevelNames.HOME },
            [TR2Type.StickWieldingGoon1BodyWarmer] =
                new() { TR2LevelNames.VENICE },
            [TR2Type.StickWieldingGoon1GreenVest] =
                new() { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
            [TR2Type.StickWieldingGoon1WhiteVest] = [TR2LevelNames.BARTOLI, TR2LevelNames.OPERA],
            [TR2Type.StickWieldingGoon1GM] = [TR2LevelNames.FOOLGOLD, TR2LevelNames.VEGAS],
        },
        [TR2Type.TigerOrSnowLeopard] = new()
        {
            [TR2Type.BengalTiger] =
                new() { TR2LevelNames.GW, TR2LevelNames.XIAN },
            [TR2Type.SnowLeopard] =
                new() { TR2LevelNames.TIBET, TR2LevelNames.COT },
            [TR2Type.WhiteTiger] =
                new() { TR2LevelNames.CHICKEN }
        },
        [TR2Type.FlamethrowerGoon] = new()
        {
            [TR2Type.FlamethrowerGoonOG]
                = new() { TR2LevelNames.DA, TR2LevelNames.DECK },
            [TR2Type.FlamethrowerGoonTopixtor]
                = new() { },
            [TR2Type.FlamethrowerGoonGM]
                = [TR2LevelNames.FOOLGOLD],
        },
        [TR2Type.Gunman1] = new()
        {
            [TR2Type.Gunman1OG]
                = new() { TR2LevelNames.RIG, TR2LevelNames.DA, TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
            [TR2Type.Gunman1TopixtorORC]
                = new() { },
            [TR2Type.Gunman1TopixtorCAC]
                = new() { },
            [TR2Type.Gunman1GM]
                = [TR2LevelNames.FOOLGOLD]
        },
        [TR2Type.Mercenary2] = new()
        {
            [TR2Type.Mercenary2OG] = [TR2LevelNames.TIBET, TR2LevelNames.MONASTERY, TR2LevelNames.COT],
            [TR2Type.Mercenary2GM] = [TR2LevelNames.COLDWAR, TR2LevelNames.FOOLGOLD, TR2LevelNames.KINGDOM],
        },
        [TR2Type.Mercenary3] = new()
        {
            [TR2Type.Mercenary3OG] = [TR2LevelNames.TIBET],
            [TR2Type.Mercenary3GM] = [TR2LevelNames.COLDWAR],
        },
        [TR2Type.MercSnowmobDriver] = new()
        {
            [TR2Type.MercSnowmobDriverOG] = [TR2LevelNames.TIBET],
            [TR2Type.MercSnowmobDriverGM] = [TR2LevelNames.COLDWAR, TR2LevelNames.FOOLGOLD],
        },
        [TR2Type.BlackSnowmob] = new()
        {
            [TR2Type.BlackSnowmobOG] = [TR2LevelNames.TIBET],
            [TR2Type.BlackSnowmobGM] = [TR2LevelNames.COLDWAR, TR2LevelNames.FOOLGOLD],
        },
        [TR2Type.MonkWithKnifeStick] = new()
        {
            [TR2Type.MonkWithKnifeStickOG] = [TR2LevelNames.MONASTERY],
            [TR2Type.MonkWithKnifeStickGM] = [TR2LevelNames.FURNACE, TR2LevelNames.KINGDOM],
        },
        [TR2Type.Yeti] = new()
        {
            [TR2Type.YetiOG] = [TR2LevelNames.COT, TR2LevelNames.CHICKEN],
            [TR2Type.YetiGM] = [TR2LevelNames.KINGDOM],
        },
        [TR2Type.BirdMonster] = new()
        {
            [TR2Type.BirdMonsterOG] = [TR2LevelNames.CHICKEN],
            [TR2Type.BirdMonsterGM] = [TR2LevelNames.KINGDOM, TR2LevelNames.VEGAS],
        },
    };

    public static readonly Dictionary<TR2Type, List<TR2Type>> TypeFamilies = new()
    {
        [TR2Type.Lara] = new()
        {
            TR2Type.LaraSun, TR2Type.LaraSnow, TR2Type.LaraUnwater, TR2Type.LaraHome, TR2Type.LaraAssault
        },
        [TR2Type.Barracuda] = new()
        {
            TR2Type.BarracudaIce, TR2Type.BarracudaUnwater, TR2Type.BarracudaXian
        },
        [TR2Type.Shark] = [TR2Type.SharkOG, TR2Type.SharkGM],
        [TR2Type.StickWieldingGoon1] = new()
        {
            TR2Type.StickWieldingGoon1Bandana, TR2Type.StickWieldingGoon1BlackJacket, TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.StickWieldingGoon1GreenVest, TR2Type.StickWieldingGoon1WhiteVest, TR2Type.StickWieldingGoon1GM,
        },
        [TR2Type.TigerOrSnowLeopard] = new()
        {
            TR2Type.BengalTiger, TR2Type.SnowLeopard, TR2Type.WhiteTiger
        },
        [TR2Type.FlamethrowerGoon] = new()
        {
            TR2Type.FlamethrowerGoonOG, TR2Type.FlamethrowerGoonTopixtor, TR2Type.FlamethrowerGoonGM,
        },
        [TR2Type.Gunman1] = new()
        {
            TR2Type.Gunman1OG, TR2Type.Gunman1TopixtorORC, TR2Type.Gunman1TopixtorCAC, TR2Type.Gunman1GM,
        },
        [TR2Type.Mercenary2] = [TR2Type.Mercenary2OG, TR2Type.Mercenary2GM],
        [TR2Type.Mercenary3] = [TR2Type.Mercenary3OG, TR2Type.Mercenary3GM],
        [TR2Type.MercSnowmobDriver] = [TR2Type.MercSnowmobDriverOG, TR2Type.MercSnowmobDriverGM],
        [TR2Type.BlackSnowmob] = [TR2Type.BlackSnowmobOG, TR2Type.BlackSnowmobGM],
        [TR2Type.MonkWithKnifeStick] = [TR2Type.MonkWithKnifeStickOG, TR2Type.MonkWithKnifeStickGM],
        [TR2Type.Yeti] = [TR2Type.YetiOG, TR2Type.YetiGM],
        [TR2Type.BirdMonster] = [TR2Type.BirdMonsterOG, TR2Type.BirdMonsterGM],
    };

    public static string GetName(TR2Type type)
    {
        // TODO: remove once static mesh enum entries are removed
        return type switch
        {
            TR2Type.Bear => "Bear",
            TR2Type.MonkWithNoShadow => "MonkWithNoShadow",
            TR2Type.Wolf => "Wolf",
            _ => type.ToString(),
        };
    }

    public static TR2Type TranslateAlias(TR2Type type)
    {
        foreach (TR2Type parentType in TypeFamilies.Keys)
        {
            if (TypeFamilies[parentType].Contains(type))
            {
                return parentType;
            }
        }

        return type;
    }

    public static TR2Type GetAliasForLevel(string lvl, TR2Type type)
    {
        if (LevelAliases.ContainsKey(type))
        {
            foreach (TR2Type alias in LevelAliases[type].Keys)
            {
                if (LevelAliases[type][alias].Contains(lvl))
                {
                    return alias;
                }
            }
        }
        return type;
    }

    public static List<TR2Type> GetFamily(TR2Type type)
    {
        foreach (TR2Type parentType in TypeFamilies.Keys)
        {
            if (TypeFamilies[parentType].Contains(type))
            {
                return TypeFamilies[parentType];
            }
        }

        return new() { type };
    }

    public static List<TR2Type> RemoveAliases(IEnumerable<TR2Type> types)
    {
        List<TR2Type> normalisedTypes = new();
        foreach (TR2Type ent in types)
        {
            TR2Type normalisedType = TranslateAlias(ent);
            if (!normalisedTypes.Contains(normalisedType))
            {
                normalisedTypes.Add(normalisedType);
            }
        }
        return normalisedTypes;
    }

    public static List<TR2Type> GetLaraTypes()
    {
        return new()
        {
            TR2Type.LaraSun, TR2Type.LaraUnwater, TR2Type.LaraSnow, TR2Type.LaraHome, TR2Type.LaraInvisible, TR2Type.LaraAssault
        };
    }

    public static List<TR2Type> GetCandidateCrossLevelEnemies(bool remastered)
    {
        List<TR2Type> types =
        [
            TR2Type.BarracudaIce,
            TR2Type.BarracudaUnwater,
            TR2Type.BarracudaXian,
            TR2Type.BengalTiger,
            TR2Type.BirdMonsterOG,
            TR2Type.BlackMorayEel,
            TR2Type.Crow,
            TR2Type.Doberman,
            TR2Type.Eagle,
            TR2Type.FlamethrowerGoonOG,
            TR2Type.GiantSpider,
            TR2Type.Gunman1OG,
            TR2Type.Gunman2,
            TR2Type.Knifethrower,
            TR2Type.MarcoBartoli, // catch-all for Marco, the explosion when the dragon spawns, and the various dragon front/back pieces
            TR2Type.MaskedGoon1,
            TR2Type.MaskedGoon2,
            TR2Type.MaskedGoon3,
            TR2Type.Mercenary1,
            TR2Type.Mercenary2OG,
            TR2Type.Mercenary3OG,
            TR2Type.MercSnowmobDriverOG,
            TR2Type.MonkWithKnifeStickOG,
            TR2Type.MonkWithLongStick,
            TR2Type.Rat,
            TR2Type.ScubaDiver,
            TR2Type.SharkOG,
            TR2Type.ShotgunGoon,
            TR2Type.SnowLeopard,
            TR2Type.Spider,
            TR2Type.StickWieldingGoon1Bandana,
            TR2Type.StickWieldingGoon1BlackJacket,
            TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.StickWieldingGoon1GreenVest,
            TR2Type.StickWieldingGoon1WhiteVest,
            TR2Type.StickWieldingGoon1GM,
            TR2Type.StickWieldingGoon2,
            TR2Type.TRex,
            TR2Type.WhiteTiger,
            TR2Type.Winston,
            TR2Type.XianGuardSpear,
            TR2Type.XianGuardSword,
            TR2Type.YellowMorayEel,
            TR2Type.YetiOG,
        ];

        if (!remastered)
        {
            types.Add(TR2Type.Bear);
            types.Add(TR2Type.MonkWithNoShadow);
            types.Add(TR2Type.Wolf);
            types.Add(TR2Type.FlamethrowerGoonTopixtor);
            types.Add(TR2Type.FlamethrowerGoonGM);
            types.Add(TR2Type.Gunman1TopixtorORC);
            types.Add(TR2Type.Gunman1TopixtorCAC);
            types.Add(TR2Type.Gunman1GM);
            types.Add(TR2Type.SharkGM);
            types.Add(TR2Type.Mercenary2GM);
            types.Add(TR2Type.Mercenary3GM);
            types.Add(TR2Type.MercSnowmobDriverGM);
            types.Add(TR2Type.MonkWithKnifeStickGM);
            types.Add(TR2Type.YetiGM);
            types.Add(TR2Type.BirdMonsterGM);
        }

        return types;
    }

    public static List<TR2Type> GetDropperEnemies(bool monksAreKillable, bool unconditionalChickens, bool remastered)
    {
        List<TR2Type> types =
        [
            TR2Type.BengalTiger,
            TR2Type.Doberman,
            TR2Type.FlamethrowerGoonOG,
            TR2Type.GiantSpider,
            TR2Type.Gunman1OG,
            TR2Type.Gunman2,
            TR2Type.Knifethrower,
            TR2Type.MaskedGoon1,
            TR2Type.MaskedGoon2,
            TR2Type.MaskedGoon3,
            TR2Type.Mercenary1,
            TR2Type.Mercenary2OG,
            TR2Type.Mercenary3OG,
            TR2Type.Rat,
            TR2Type.ShotgunGoon,
            TR2Type.SnowLeopard,
            TR2Type.StickWieldingGoon1Bandana,
            TR2Type.StickWieldingGoon1BlackJacket,
            TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.StickWieldingGoon1GreenVest,
            TR2Type.StickWieldingGoon1WhiteVest,
            TR2Type.StickWieldingGoon1GM,
            TR2Type.StickWieldingGoon2,
            TR2Type.TRex,
            TR2Type.WhiteTiger,
            TR2Type.YetiOG,
        ];

        // #131 Provides an option to exclude monks as having to be killed
        if (monksAreKillable)
        {
            types.Add(TR2Type.MonkWithKnifeStickOG);
            types.Add(TR2Type.MonkWithLongStick);
            if (!remastered)
            {
                types.Add(TR2Type.MonkWithKnifeStickGM);
                types.Add(TR2Type.MonkWithNoShadow);
            }
        }

        if (unconditionalChickens)
        {
            types.Add(TR2Type.BirdMonsterOG);
            if (!remastered)
            {
                types.Add(TR2Type.BirdMonsterGM);
            }
        }

        if (!remastered)
        {
            types.Add(TR2Type.BarracudaIce);
            types.Add(TR2Type.BarracudaUnwater);
            types.Add(TR2Type.BarracudaXian);
            types.Add(TR2Type.Bear);
            types.Add(TR2Type.Crow);
            types.Add(TR2Type.Eagle);
            types.Add(TR2Type.MercSnowmobDriverOG);
            types.Add(TR2Type.MercSnowmobDriverGM);
            types.Add(TR2Type.ScubaDiver);
            types.Add(TR2Type.SharkOG);
            types.Add(TR2Type.SharkGM);
            types.Add(TR2Type.Wolf);

            types.Add(TR2Type.FlamethrowerGoonTopixtor);
            types.Add(TR2Type.FlamethrowerGoonGM);
            types.Add(TR2Type.Gunman1TopixtorORC);
            types.Add(TR2Type.Gunman1TopixtorCAC);
            types.Add(TR2Type.Gunman1GM);
            types.Add(TR2Type.Mercenary2GM);
            types.Add(TR2Type.Mercenary3GM);
            types.Add(TR2Type.YetiGM);
        }

        return types;
    }

    // This is the full list of enemies including alias duplicates - used for 
    // checking while iterating an type list to determine if an type is 
    // an enemy and can be replaced.
    public static List<TR2Type> GetFullListOfEnemies()
    {
        return new()
        {
            TR2Type.Barracuda,
            TR2Type.BarracudaIce,
            TR2Type.BarracudaUnwater,
            TR2Type.BarracudaXian,
            TR2Type.BengalTiger,
            TR2Type.BirdMonster,
            TR2Type.BirdMonsterOG,
            TR2Type.BirdMonsterGM,
            TR2Type.BlackMorayEel,
            TR2Type.Crow,
            TR2Type.Doberman,
            TR2Type.Eagle,
            TR2Type.FlamethrowerGoon,
            TR2Type.FlamethrowerGoonOG,
            TR2Type.FlamethrowerGoonTopixtor,
            TR2Type.FlamethrowerGoonGM,
            TR2Type.GiantSpider,
            TR2Type.Gunman1,
            TR2Type.Gunman1OG,
            TR2Type.Gunman1TopixtorORC,
            TR2Type.Gunman1TopixtorCAC,
            TR2Type.Gunman1GM,
            TR2Type.Gunman2,
            TR2Type.Knifethrower,
            TR2Type.MarcoBartoli,
            TR2Type.MaskedGoon1,
            TR2Type.MaskedGoon2,
            TR2Type.MaskedGoon3,
            TR2Type.Mercenary1,
            TR2Type.Mercenary2,
            TR2Type.Mercenary3,
            TR2Type.Mercenary2OG,
            TR2Type.Mercenary2GM,
            TR2Type.Mercenary3OG,
            TR2Type.Mercenary3GM,
            TR2Type.MercSnowmobDriver,
            TR2Type.MercSnowmobDriverOG,
            TR2Type.MercSnowmobDriverGM,
            TR2Type.MonkWithKnifeStick,
            TR2Type.MonkWithKnifeStickOG,
            TR2Type.MonkWithKnifeStickGM,
            TR2Type.MonkWithLongStick,
            TR2Type.Rat,
            TR2Type.ScubaDiver,
            TR2Type.Shark,
            TR2Type.SharkOG,
            TR2Type.SharkGM,
            TR2Type.ShotgunGoon,
            TR2Type.SnowLeopard,
            TR2Type.Spider,
            TR2Type.StickWieldingGoon1,
            TR2Type.StickWieldingGoon1Bandana,
            TR2Type.StickWieldingGoon1BlackJacket,
            TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.StickWieldingGoon1GreenVest,
            TR2Type.StickWieldingGoon1WhiteVest,
            TR2Type.StickWieldingGoon1GM,
            TR2Type.StickWieldingGoon2,
            TR2Type.TigerOrSnowLeopard,
            TR2Type.TRex,
            TR2Type.WhiteTiger,
            TR2Type.Winston,
            TR2Type.XianGuardSpear,
            TR2Type.XianGuardSword,
            TR2Type.YellowMorayEel,
            TR2Type.Yeti,
            TR2Type.YetiOG,
            TR2Type.YetiGM,
        };
    }

    public static bool IsEnemyType(TR2Type type)
    {
        return GetFullListOfEnemies().Contains(type);
    }

    public static List<TR2Type> GetGunTypes()
    {
        return new()
        {
            TR2Type.Shotgun_S_P,
            TR2Type.Automags_S_P,
            TR2Type.Uzi_S_P,
            TR2Type.Harpoon_S_P,
            TR2Type.M16_S_P,
            TR2Type.GrenadeLauncher_S_P,
        };
    }

    public static List<TR2Type> GetGunModels()
    {
        return
        [
            TR2Type.Pistols_M_H,
            TR2Type.Shotgun_M_H,
            TR2Type.Autos_M_H,
            TR2Type.Uzi_M_H,
            TR2Type.Harpoon_M_H,
            TR2Type.M16_M_H,
            TR2Type.GrenadeLauncher_M_H,
        ];
    }

    public static bool IsGunType(TR2Type type)
    {
        return (type == TR2Type.Shotgun_S_P ||
                type == TR2Type.Automags_S_P ||
                type == TR2Type.Uzi_S_P ||
                type == TR2Type.Harpoon_S_P ||
                type == TR2Type.M16_S_P ||
                type == TR2Type.GrenadeLauncher_S_P);
    }

    public static List<TR2Type> GetAmmoTypes()
    {
        return new()
        {
            TR2Type.ShotgunAmmo_S_P,
            TR2Type.AutoAmmo_S_P,
            TR2Type.UziAmmo_S_P,
            TR2Type.HarpoonAmmo_S_P,
            TR2Type.M16Ammo_S_P,
            TR2Type.Grenades_S_P,
            TR2Type.SmallMed_S_P,
            TR2Type.LargeMed_S_P,
            TR2Type.Flares_S_P,
        };
    }

    public static TR2Type GetWeaponAmmo(TR2Type weapon)
    {
        return weapon switch
        {
            TR2Type.Shotgun_S_P => TR2Type.ShotgunAmmo_S_P,
            TR2Type.Automags_S_P => TR2Type.AutoAmmo_S_P,
            TR2Type.Uzi_S_P => TR2Type.UziAmmo_S_P,
            TR2Type.Harpoon_S_P => TR2Type.HarpoonAmmo_S_P,
            TR2Type.M16_S_P => TR2Type.M16Ammo_S_P,
            TR2Type.GrenadeLauncher_S_P => TR2Type.Grenades_S_P,
            _ => TR2Type.PistolAmmo_S_P,
        };
    }

    public static bool IsUtilityType(TR2Type type)
    {
        return (type == TR2Type.ShotgunAmmo_S_P ||
                type == TR2Type.AutoAmmo_S_P ||
                type == TR2Type.UziAmmo_S_P ||
                type == TR2Type.HarpoonAmmo_S_P ||
                type == TR2Type.M16Ammo_S_P ||
                type == TR2Type.Grenades_S_P ||
                type == TR2Type.SmallMed_S_P ||
                type == TR2Type.LargeMed_S_P ||
                type == TR2Type.Flares_S_P);
    }

    public static bool IsSecretType(TR2Type type)
    {
        return type == TR2Type.StoneSecret_S_P || type == TR2Type.JadeSecret_S_P || type == TR2Type.GoldSecret_S_P;
    }

    public static bool IsAmmoType(TR2Type type)
    {
        return (type == TR2Type.ShotgunAmmo_S_P ||
                type == TR2Type.AutoAmmo_S_P ||
                type == TR2Type.UziAmmo_S_P ||
                type == TR2Type.HarpoonAmmo_S_P ||
                type == TR2Type.M16Ammo_S_P ||
                type == TR2Type.Grenades_S_P);
    }

    public static List<TR2Type> GetStandardPickupTypes()
    {
        return GetGunTypes().Concat(GetAmmoTypes()).ToList();
    }

    public static bool IsStandardPickupType(TR2Type type)
    {
        return GetStandardPickupTypes().Contains(type);
    }

    public static bool IsMediType(TR2Type type)
    {
        return type == TR2Type.SmallMed_S_P || type == TR2Type.LargeMed_S_P;
    }

    public static List<TR2Type> GetKeyItemTypes()
    {
        return new()
        {
            TR2Type.Key1_S_P,
            TR2Type.Key2_S_P,
            TR2Type.Key3_S_P,
            TR2Type.Key4_S_P,
            TR2Type.Puzzle1_S_P,
            TR2Type.Puzzle2_S_P,
            TR2Type.Puzzle3_S_P,
            TR2Type.Puzzle4_S_P,
            TR2Type.Quest1_S_P,
            TR2Type.Quest2_S_P
        };
    }

    public static List<TR2Type> GetSecretTypes()
    {
        return new()
        {
            TR2Type.StoneSecret_S_P,
            TR2Type.JadeSecret_S_P,
            TR2Type.GoldSecret_S_P
        };
    }

    public static bool IsKeyItemType(TR2Type type)
    {
        return (type == TR2Type.Key1_S_P ||
                type == TR2Type.Key2_S_P ||
                type == TR2Type.Key3_S_P ||
                type == TR2Type.Key4_S_P ||
                type == TR2Type.Puzzle1_S_P ||
                type == TR2Type.Puzzle2_S_P ||
                type == TR2Type.Puzzle3_S_P ||
                type == TR2Type.Puzzle4_S_P ||
                type == TR2Type.Quest1_S_P ||
                type == TR2Type.Quest2_S_P);
    }

    public static bool IsAnyPickupType(TR2Type type)
    {
        return type == TR2Type.Pistols_S_P ||
            type == TR2Type.PistolAmmo_S_P ||
            IsAmmoType(type) ||
            IsGunType(type) ||
            IsKeyItemType(type) ||
            IsUtilityType(type) ||
            IsSecretType(type);
    }

    public static List<TR2Type> GetSwitchTypes()
    {
        return new()
        {
            TR2Type.WallSwitch,
            TR2Type.UnderwaterSwitch,
            TR2Type.PushButtonSwitch,
            TR2Type.SmallWallSwitch,
            TR2Type.WheelKnob
        };
    }

    public static bool IsSwitchType(TR2Type type)
    {
        return GetSwitchTypes().Contains(type);
    }

    public static List<TR2Type> GetKeyholeTypes()
    {
        return new()
        {
            TR2Type.Keyhole1,
            TR2Type.Keyhole2,
            TR2Type.Keyhole3,
            TR2Type.Keyhole4
        };
    }

    public static bool IsKeyholeType(TR2Type type)
    {
        return GetKeyholeTypes().Contains(type);
    }

    public static List<TR2Type> GetSlotTypes()
    {
        return new()
        {
            TR2Type.PuzzleHole1,
            TR2Type.PuzzleHole2,
            TR2Type.PuzzleHole3,
            TR2Type.PuzzleHole4,
            TR2Type.PuzzleDone1,
            TR2Type.PuzzleDone2,
            TR2Type.PuzzleDone3,
            TR2Type.PuzzleDone4
        };
    }

    public static bool IsSlotType(TR2Type type)
    {
        return GetSlotTypes().Contains(type);
    }

    public static List<TR2Type> GetPushblockTypes()
    {
        return new()
        {
            TR2Type.PushBlock1,
            TR2Type.PushBlock2,
            TR2Type.PushBlock3,
            TR2Type.PushBlock4
        };
    }

    public static bool IsPushblockType(TR2Type type)
    {
        return GetPushblockTypes().Contains(type);
    }

    public static bool IsWaterCreature(TR2Type type)
    {
        return type == TR2Type.Shark || type == TR2Type.SharkOG || type == TR2Type.SharkGM
            || type == TR2Type.YellowMorayEel || type == TR2Type.BlackMorayEel
            || type == TR2Type.Barracuda || type == TR2Type.BarracudaIce || type == TR2Type.BarracudaUnwater
            || type == TR2Type.BarracudaXian || type == TR2Type.ScubaDiver;
    }

    public static List<TR2Type> WaterCreatures()
    {
        return new()
        {
            TR2Type.SharkOG,
            TR2Type.SharkGM,
            TR2Type.BarracudaIce,
            TR2Type.BarracudaUnwater,
            TR2Type.BarracudaXian,
            TR2Type.YellowMorayEel,
            TR2Type.BlackMorayEel,
            TR2Type.ScubaDiver
        };
    }

    public static List<TR2Type> KillableWaterCreatures()
    {
        return new()
        {
            TR2Type.SharkOG,
            TR2Type.SharkGM,
            TR2Type.BarracudaIce,
            TR2Type.BarracudaUnwater,
            TR2Type.BarracudaXian,
            TR2Type.ScubaDiver
        };
    }

    public static bool IsStaticCreature(TR2Type type)
    {
        return type == TR2Type.YellowMorayEel || type == TR2Type.BlackMorayEel;
    }

    public static bool IsHazardCreature(TR2Type type)
    {
        return type == TR2Type.YellowMorayEel || type == TR2Type.BlackMorayEel || type == TR2Type.Winston;
    }

    public static List<TR2Type> FilterWaterEnemies(List<TR2Type> types)
    {
        List<TR2Type> waterTypes = new();
        foreach (TR2Type type in types)
        {
            if (IsWaterCreature(type))
            {
                waterTypes.Add(type);
            }
        }
        return waterTypes;
    }

    public static bool CanDropPickups(TR2Type type, bool monksAreKillable, bool unconditionalChickens, bool remastered)
    {
        return (type == TR2Type.MarcoBartoli && !remastered)
            || GetDropperEnemies(monksAreKillable, unconditionalChickens, remastered).Contains(type);
    }

    public static bool IsMonk(TR2Type type)
    {
        return GetMonkTypes().Contains(type);
    }

    public static List<TR2Type> GetMonkTypes()
    {
        return
        [
            TR2Type.MonkWithKnifeStick,
            TR2Type.MonkWithKnifeStickOG,
            TR2Type.MonkWithKnifeStickGM,
            TR2Type.MonkWithLongStick,
            TR2Type.MonkWithNoShadow,
        ];
    }

    public static bool IsBirdMonsterType(TR2Type type)
    {
        return TranslateAlias(type) == TR2Type.BirdMonster;
    }

    public static List<TR2Type> GetAllies()
    {
        return [TR2Type.Winston, .. GetMonkTypes()];
    }

    public static List<TR2Type> FilterDropperEnemies(List<TR2Type> types, bool monksAreKillable, bool unconditionalChickens, bool remastered)
    {
        return types.FindAll(t => CanDropPickups(t, monksAreKillable, unconditionalChickens, remastered));
    }

    public static List<TR2Type> DoorTypes()
    {
        return new()
        {
            TR2Type.Door1, TR2Type.Door2, TR2Type.Door3,
            TR2Type.Door4, TR2Type.Door5, TR2Type.LiftingDoor1,
            TR2Type.LiftingDoor2, TR2Type.LiftingDoor3
        };
    }

    public static bool IsDoorType(TR2Type type)
    {
        return DoorTypes().Contains(type);
    }

    public static List<TR2Type> TrapdoorTypes()
    {
        return new()
        {
            TR2Type.Trapdoor1,
            TR2Type.Trapdoor2,
            TR2Type.Trapdoor3,
        };
    }

    public static bool IsTrapdoorType(TR2Type type)
    {
        return TrapdoorTypes().Contains(type);
    }

    public static List<TR2Type> BreakableWindows()
    {
        return new()
        {
            TR2Type.BreakableWindow1,
            TR2Type.BreakableWindow2,
            TR2Type.BreakableWindow3,
            TR2Type.BreakableWindow4,
        };
    }

    public static bool IsBridgeType(TR2Type type)
    {
        return Bridges().Contains(type);
    }

    public static List<TR2Type> Bridges()
    {
        return new()
        {
            TR2Type.BridgeFlat,
            TR2Type.BridgeTilt1,
            TR2Type.BridgeTilt2,
        };
    }

    public static bool IsBreakableWindowType(TR2Type type)
    {
        return BreakableWindows().Contains(type);
    }

    public static List<TR2Type> UnrenderedTypes()
    {
        return new()
        {
            TR2Type.CameraTarget_N,
            TR2Type.AlarmBell_N,
            TR2Type.Alarm_N,
            TR2Type.DoorBell_N,
            TR2Type.DrippingWater_N,
            TR2Type.BartoliHideoutClock_N,
            TR2Type.SingingBirds_N,
            TR2Type.DragonExplosionEmitter_N,
        };
    }

    public static bool IsUnrenderedType(TR2Type type)
    {
        return UnrenderedTypes().Contains(type);
    }

    public static List<TR2Type> VehicleTypes()
    {
        return new()
        {
            TR2Type.Boat,
            TR2Type.RedSnowmobile,
            TR2Type.BlackSnowmob,
        };
    }

    public static bool IsVehicleType(TR2Type type)
    {
        return VehicleTypes().Contains(type);
    }

    public static bool CanSharePickupSpace(TR2Type type, bool remastered)
    {
        // Can we place a standard pickup on the same tile as this entity type?
        return IsAnyPickupType(type)
            || CanDropPickups(type, false, false, remastered)
            || (TypeFamilies.ContainsKey(type) && TypeFamilies[type].Any(e => CanDropPickups(e, false, false, remastered)))
            || (IsSwitchType(type) && type != TR2Type.UnderwaterSwitch)
            || IsKeyholeType(type)
            || IsSlotType(type)
            || IsPushblockType(type)
            || IsDoorType(type)
            || IsTrapdoorType(type)
            || IsUnrenderedType(type)
            || IsVehicleType(type)
            || IsBreakableWindowType(type)
            || IsBridgeType(type)
            || type == TR2Type.Lara
            || type == TR2Type.Drawbridge
            || type == TR2Type.FallingBlock
            || type == TR2Type.LooseBoards
            || type == TR2Type.RollingBall
            || type == TR2Type.BouldersOrSnowballs
            || type == TR2Type.RollingStorageDrums
            || type == TR2Type.WaterfallMist_N
            || type == TR2Type.Gondola
            || type == TR2Type.ZiplineHandle
            || type == TR2Type.Discgun
            || type == TR2Type.StatueWithKnifeBlade;
    }
}
