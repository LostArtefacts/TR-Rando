using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TR2TypeUtilities
{
    public static readonly Dictionary<TR2Type, Dictionary<TR2Type, List<string>>> LevelAliases = new()
    {
        [TR2Type.Lara] = new Dictionary<TR2Type, List<string>>
        {
            [TR2Type.LaraSun] =
                new List<string> { TR2LevelNames.GW, TR2LevelNames.GW_CUT, TR2LevelNames.VENICE, TR2LevelNames.BARTOLI, TR2LevelNames.OPERA, TR2LevelNames.OPERA_CUT, TR2LevelNames.RIG, TR2LevelNames.DA, TR2LevelNames.DA_CUT, TR2LevelNames.XIAN, TR2LevelNames.XIAN_CUT, TR2LevelNames.FLOATER, TR2LevelNames.LAIR },
            [TR2Type.LaraUnwater] =
                new List<string> { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
            [TR2Type.LaraSnow] =
                new List<string> { TR2LevelNames.TIBET, TR2LevelNames.MONASTERY, TR2LevelNames.COT, TR2LevelNames.CHICKEN },
            [TR2Type.LaraHome] =
                new List<string> { TR2LevelNames.HOME }
        },
        [TR2Type.Barracuda] = new Dictionary<TR2Type, List<string>>
        {
            [TR2Type.BarracudaIce] =
                new List<string> { TR2LevelNames.COT, TR2LevelNames.CHICKEN },
            [TR2Type.BarracudaUnwater] =
                new List<string> { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
            [TR2Type.BarracudaXian] =
                new List<string> { TR2LevelNames.XIAN }
        },
        [TR2Type.StickWieldingGoon1] = new Dictionary<TR2Type, List<string>>
        {
            [TR2Type.StickWieldingGoon1Bandana] =
                new List<string> { TR2LevelNames.RIG, TR2LevelNames.DA },
            [TR2Type.StickWieldingGoon1BlackJacket] =
                new List<string> { TR2LevelNames.HOME },
            [TR2Type.StickWieldingGoon1BodyWarmer] =
                new List<string> { TR2LevelNames.VENICE },
            [TR2Type.StickWieldingGoon1GreenVest] =
                new List<string> { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
            [TR2Type.StickWieldingGoon1WhiteVest] =
                new List<string> { TR2LevelNames.BARTOLI, TR2LevelNames.OPERA }
        },
        [TR2Type.TigerOrSnowLeopard] = new Dictionary<TR2Type, List<string>>
        {
            [TR2Type.BengalTiger] =
                new List<string> { TR2LevelNames.GW, TR2LevelNames.XIAN },
            [TR2Type.SnowLeopard] =
                new List<string> { TR2LevelNames.TIBET, TR2LevelNames.COT },
            [TR2Type.WhiteTiger] =
                new List<string> { TR2LevelNames.CHICKEN }
        },
        [TR2Type.FlamethrowerGoon] = new Dictionary<TR2Type, List<string>>
        {
            [TR2Type.FlamethrowerGoonOG]
                = new List<string> { TR2LevelNames.DA, TR2LevelNames.DECK },
            [TR2Type.FlamethrowerGoonTopixtor]
                = new List<string> { }
        },
        [TR2Type.Gunman1] = new Dictionary<TR2Type, List<string>>
        {
            [TR2Type.Gunman1OG]
                = new List<string> { TR2LevelNames.RIG, TR2LevelNames.DA, TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
            [TR2Type.Gunman1TopixtorORC]
                = new List<string> { },
            [TR2Type.Gunman1TopixtorCAC]
                = new List<string> { }
        }
    };

    public static readonly Dictionary<TR2Type, List<TR2Type>> TypeFamilies = new()
    {
        [TR2Type.Lara] = new List<TR2Type>
        {
            TR2Type.LaraSun, TR2Type.LaraSnow, TR2Type.LaraUnwater, TR2Type.LaraHome
        },
        [TR2Type.Barracuda] = new List<TR2Type>
        {
            TR2Type.BarracudaIce, TR2Type.BarracudaUnwater, TR2Type.BarracudaXian
        },
        [TR2Type.StickWieldingGoon1] = new List<TR2Type>
        {
            TR2Type.StickWieldingGoon1Bandana, TR2Type.StickWieldingGoon1BlackJacket, TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.StickWieldingGoon1GreenVest, TR2Type.StickWieldingGoon1WhiteVest
        },
        [TR2Type.TigerOrSnowLeopard] = new List<TR2Type>
        {
            TR2Type.BengalTiger, TR2Type.SnowLeopard, TR2Type.WhiteTiger
        },
        [TR2Type.FlamethrowerGoon] = new List<TR2Type>
        {
            TR2Type.FlamethrowerGoonOG, TR2Type.FlamethrowerGoonTopixtor
        },
        [TR2Type.Gunman1] = new List<TR2Type>
        {
            TR2Type.Gunman1OG, TR2Type.Gunman1TopixtorORC, TR2Type.Gunman1TopixtorCAC
        }
    };

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

        return new List<TR2Type> { type };
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
        return new List<TR2Type>
        {
            TR2Type.LaraSun, TR2Type.LaraUnwater, TR2Type.LaraSnow, TR2Type.LaraHome, TR2Type.LaraInvisible
        };
    }

    public static List<TR2Type> GetCandidateCrossLevelEnemies()
    {
        return new List<TR2Type>
        {
            TR2Type.BarracudaIce,
            TR2Type.BarracudaUnwater,
            TR2Type.BarracudaXian,
            TR2Type.BengalTiger,
            TR2Type.BirdMonster,
            TR2Type.BlackMorayEel,
            TR2Type.Crow,
            TR2Type.Doberman,
            TR2Type.Eagle,
            TR2Type.FlamethrowerGoonOG,
            TR2Type.FlamethrowerGoonTopixtor,
            TR2Type.GiantSpider,
            TR2Type.Gunman1OG,
            TR2Type.Gunman1TopixtorORC,
            TR2Type.Gunman1TopixtorCAC,
            TR2Type.Gunman2,
            TR2Type.Knifethrower,
            TR2Type.MarcoBartoli, // catch-all for Marco, the explosion when the dragon spawns, and the various dragon front/back pieces
            TR2Type.MaskedGoon1,
            TR2Type.MaskedGoon2,
            TR2Type.MaskedGoon3,
            TR2Type.Mercenary1,
            TR2Type.Mercenary2,
            TR2Type.Mercenary3,
            TR2Type.MercSnowmobDriver,
            TR2Type.MonkWithKnifeStick,
            TR2Type.MonkWithLongStick,
            TR2Type.Rat,
            TR2Type.ScubaDiver,
            TR2Type.Shark,
            TR2Type.ShotgunGoon,
            TR2Type.SnowLeopard,
            TR2Type.Spider,
            TR2Type.StickWieldingGoon1Bandana,
            TR2Type.StickWieldingGoon1BlackJacket,
            TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.StickWieldingGoon1GreenVest,
            TR2Type.StickWieldingGoon1WhiteVest,
            TR2Type.StickWieldingGoon2,
            TR2Type.TRex,
            TR2Type.WhiteTiger,
            TR2Type.Winston,
            TR2Type.XianGuardSpear,
            TR2Type.XianGuardSword,
            TR2Type.YellowMorayEel,
            TR2Type.Yeti
        };
    }

    public static List<TR2Type> GetCrossLevelDroppableEnemies(bool monksAreKillable, bool unconditionalChickens)
    {
        List<TR2Type> types = new()
        {
            TR2Type.BengalTiger,
            TR2Type.Crow,
            TR2Type.Doberman,
            TR2Type.Eagle,
            TR2Type.FlamethrowerGoonOG,
            TR2Type.FlamethrowerGoonTopixtor,
            TR2Type.GiantSpider,
            TR2Type.Gunman1OG,
            TR2Type.Gunman1TopixtorORC,
            TR2Type.Gunman1TopixtorCAC,
            TR2Type.Gunman2,
            TR2Type.Knifethrower,
            TR2Type.MaskedGoon1,
            TR2Type.MaskedGoon2,
            TR2Type.MaskedGoon3,
            TR2Type.Mercenary1,
            TR2Type.Mercenary2,
            TR2Type.Mercenary3,
            TR2Type.Rat,
            TR2Type.ShotgunGoon,
            TR2Type.SnowLeopard,
            TR2Type.StickWieldingGoon1Bandana,
            TR2Type.StickWieldingGoon1BlackJacket,
            TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.StickWieldingGoon1GreenVest,
            TR2Type.StickWieldingGoon1WhiteVest,
            TR2Type.StickWieldingGoon2,
            TR2Type.TRex,
            TR2Type.WhiteTiger,
            TR2Type.Yeti
        };

        // #131 Provides an option to exclude monks as having to be killed
        if (monksAreKillable)
        {
            types.Add(TR2Type.MonkWithKnifeStick);
            types.Add(TR2Type.MonkWithLongStick);
        }

        if (unconditionalChickens)
        {
            types.Add(TR2Type.BirdMonster);
        }

        return types;
    }

    // This is the full list of enemies including alias duplicates - used for 
    // checking while iterating an type list to determine if an type is 
    // an enemy and can be replaced.
    public static List<TR2Type> GetFullListOfEnemies()
    {
        return new List<TR2Type>
        {
            TR2Type.Barracuda,
            TR2Type.BarracudaIce,
            TR2Type.BarracudaUnwater,
            TR2Type.BarracudaXian,
            TR2Type.BengalTiger,
            TR2Type.BirdMonster,
            TR2Type.BlackMorayEel,
            TR2Type.Crow,
            TR2Type.Doberman,
            TR2Type.Eagle,
            TR2Type.FlamethrowerGoon,
            TR2Type.FlamethrowerGoonOG,
            TR2Type.FlamethrowerGoonTopixtor,
            TR2Type.GiantSpider,
            TR2Type.Gunman1,
            TR2Type.Gunman1OG,
            TR2Type.Gunman1TopixtorORC,
            TR2Type.Gunman1TopixtorCAC,
            TR2Type.Gunman2,
            TR2Type.Knifethrower,
            TR2Type.MarcoBartoli,
            TR2Type.MaskedGoon1,
            TR2Type.MaskedGoon2,
            TR2Type.MaskedGoon3,
            TR2Type.Mercenary1,
            TR2Type.Mercenary2,
            TR2Type.Mercenary3,
            TR2Type.MercSnowmobDriver,
            TR2Type.MonkWithKnifeStick,
            TR2Type.MonkWithLongStick,
            TR2Type.Rat,
            TR2Type.ScubaDiver,
            TR2Type.Shark,
            TR2Type.ShotgunGoon,
            TR2Type.SnowLeopard,
            TR2Type.Spider,
            TR2Type.StickWieldingGoon1,
            TR2Type.StickWieldingGoon1Bandana,
            TR2Type.StickWieldingGoon1BlackJacket,
            TR2Type.StickWieldingGoon1BodyWarmer,
            TR2Type.StickWieldingGoon1GreenVest,
            TR2Type.StickWieldingGoon1WhiteVest,
            TR2Type.StickWieldingGoon2,
            TR2Type.TigerOrSnowLeopard,
            TR2Type.TRex,
            TR2Type.WhiteTiger,
            TR2Type.Winston,
            TR2Type.XianGuardSpear,
            TR2Type.XianGuardSword,
            TR2Type.YellowMorayEel,
            TR2Type.Yeti
        };
    }

    public static bool IsEnemyType(TR2Type type)
    {
        return GetFullListOfEnemies().Contains(type);
    }

    public static Dictionary<string, List<TR2Type>> GetEnemyTypeDictionary()
    {
        return new Dictionary<string, List<TR2Type>>
        {
            { TR2LevelNames.GW,
                new List<TR2Type>{ TR2Type.Crow, TR2Type.TigerOrSnowLeopard, TR2Type.Spider, TR2Type.TRex }
            },

            { TR2LevelNames.VENICE,
                new List<TR2Type>{ TR2Type.Doberman, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.StickWieldingGoon1, TR2Type.Rat, TR2Type.MaskedGoon1 }
            },

            { TR2LevelNames.BARTOLI,
                new List<TR2Type>{ TR2Type.StickWieldingGoon1, TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.Rat }
            },

            { TR2LevelNames.OPERA,
                new List<TR2Type>{ TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.Rat, TR2Type.StickWieldingGoon1, TR2Type.ShotgunGoon }
            },

            { TR2LevelNames.RIG,
                new List<TR2Type>{ TR2Type.Gunman2, TR2Type.StickWieldingGoon1, TR2Type.Doberman, TR2Type.Gunman1, TR2Type.ScubaDiver }
            },

            { TR2LevelNames.DA,
                new List<TR2Type>{ TR2Type.FlamethrowerGoon, TR2Type.StickWieldingGoon1, TR2Type.Doberman, TR2Type.Gunman1, TR2Type.Gunman2, TR2Type.ScubaDiver }
            },

            { TR2LevelNames.FATHOMS,
                new List<TR2Type>{ TR2Type.Shark, TR2Type.ScubaDiver, TR2Type.Gunman1, TR2Type.Barracuda, TR2Type.StickWieldingGoon1 }
            },

            { TR2LevelNames.DORIA,
                new List<TR2Type>{ TR2Type.Shark, TR2Type.ScubaDiver, TR2Type.Gunman1, TR2Type.Barracuda, TR2Type.StickWieldingGoon1, TR2Type.YellowMorayEel, TR2Type.Gunman2 }
            },

            { TR2LevelNames.LQ,
                new List<TR2Type>{ TR2Type.StickWieldingGoon2, TR2Type.StickWieldingGoon1, TR2Type.Gunman1, TR2Type.ScubaDiver, TR2Type.BlackMorayEel, TR2Type.Barracuda }
            },

            { TR2LevelNames.DECK,
                new List<TR2Type>{ TR2Type.StickWieldingGoon1, TR2Type.FlamethrowerGoon, TR2Type.Barracuda, TR2Type.ScubaDiver, TR2Type.Shark, TR2Type.Gunman1 }
            },

            { TR2LevelNames.TIBET,
                new List<TR2Type>{ TR2Type.Eagle, TR2Type.Mercenary2, TR2Type.Mercenary3, TR2Type.TigerOrSnowLeopard, TR2Type.MercSnowmobDriver }
            },

            { TR2LevelNames.MONASTERY,
                new List<TR2Type>{ TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick, TR2Type.Mercenary1, TR2Type.Crow, TR2Type.Mercenary2 }
            },

            { TR2LevelNames.COT,
                new List<TR2Type>{ TR2Type.TigerOrSnowLeopard, TR2Type.Mercenary1, TR2Type.Mercenary2, TR2Type.Yeti, TR2Type.Barracuda }
            },

            { TR2LevelNames.CHICKEN,
                new List<TR2Type>{ TR2Type.TigerOrSnowLeopard, TR2Type.Barracuda, TR2Type.Yeti, TR2Type.BirdMonster }
            },

            { TR2LevelNames.XIAN,
                new List<TR2Type>{ TR2Type.Barracuda, TR2Type.TigerOrSnowLeopard, TR2Type.Eagle, TR2Type.Spider, TR2Type.GiantSpider }
            },

            { TR2LevelNames.FLOATER,
                new List<TR2Type>{ TR2Type.XianGuardSword, TR2Type.XianGuardSpear, TR2Type.Knifethrower }
            },

            { TR2LevelNames.LAIR,
                new List<TR2Type>{ TR2Type.Knifethrower, TR2Type.XianGuardSpear, TR2Type.MarcoBartoli }
            },

            { TR2LevelNames.HOME,
                new List<TR2Type>{ TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.StickWieldingGoon1, TR2Type.ShotgunGoon }
            },

            { TR2LevelNames.ASSAULT,
                new List<TR2Type>{ }
            },

            { "all",
                new List<TR2Type>{ TR2Type.Crow, TR2Type.TigerOrSnowLeopard, TR2Type.Spider, TR2Type.TRex,
                TR2Type.Doberman, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.StickWieldingGoon1, TR2Type.MaskedGoon1, TR2Type.Rat, TR2Type.ShotgunGoon,
                TR2Type.Gunman2, TR2Type.Gunman1, TR2Type.ScubaDiver, TR2Type.FlamethrowerGoon, TR2Type.Shark, TR2Type.Barracuda, TR2Type.YellowMorayEel,
                TR2Type.StickWieldingGoon2, TR2Type.BlackMorayEel, TR2Type.Eagle, TR2Type.Mercenary2, TR2Type.Mercenary3, TR2Type.MercSnowmobDriver,
                TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick, TR2Type.Mercenary1, TR2Type.Yeti, TR2Type.GiantSpider,
                TR2Type.XianGuardSword, TR2Type.XianGuardSpear, TR2Type.Knifethrower
                }
            }
        };
    }

    public static List<TR2Type> GetListOfGunTypes()
    {
        return new List<TR2Type>
        {
            TR2Type.Shotgun_S_P,
            TR2Type.Automags_S_P,
            TR2Type.Uzi_S_P,
            TR2Type.Harpoon_S_P,
            TR2Type.M16_S_P,
            TR2Type.GrenadeLauncher_S_P,
        };
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

    public static List<TR2Type> GetListOfAmmoTypes()
    {
        return new List<TR2Type>
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

    public static List<TR2Type> GetListOfKeyItemTypes()
    {
        return new List<TR2Type>
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

    public static List<TR2Type> GetListOfSecretTypes()
    {
        return new List<TR2Type>
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
        return new List<TR2Type>
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
        return new List<TR2Type>
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
        return new List<TR2Type>
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
        return new List<TR2Type>
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
        return (type == TR2Type.Shark || type == TR2Type.YellowMorayEel || type == TR2Type.BlackMorayEel ||
            type == TR2Type.Barracuda || type == TR2Type.BarracudaIce || type == TR2Type.BarracudaUnwater ||
            type == TR2Type.BarracudaXian || type == TR2Type.ScubaDiver);
    }

    public static List<TR2Type> WaterCreatures()
    {
        return new List<TR2Type>
        {
            TR2Type.Shark,
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
        return new List<TR2Type>
        {
            TR2Type.Shark,
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

    public static bool CanDropPickups(TR2Type type, bool monksAreKillable, bool unconditionalChickens)
    {
        return GetCrossLevelDroppableEnemies(monksAreKillable, unconditionalChickens).Contains(type);
    }

    public static bool IsMonk(TR2Type type)
    {
        return type == TR2Type.MonkWithKnifeStick || type == TR2Type.MonkWithLongStick;
    }

    public static List<TR2Type> FilterDroppableEnemies(List<TR2Type> types, bool monksAreKillable, bool unconditionalChickens)
    {
        List<TR2Type> droppableTypes = new();
        foreach (TR2Type type in types)
        {
            if (CanDropPickups(type, monksAreKillable, unconditionalChickens))
            {
                droppableTypes.Add(type);
            }
        }
        return droppableTypes;
    }

    public static List<TR2Type> DoorTypes()
    {
        return new List<TR2Type>
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

    public static Dictionary<string, List<TR2Type>> DroppableEnemyTypes()
    {
        return new Dictionary<string, List<TR2Type>>
        {
            { TR2LevelNames.GW,
                new List<TR2Type>{ }
            },

            { TR2LevelNames.VENICE,
                new List<TR2Type>{ TR2Type.Doberman, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.StickWieldingGoon1, TR2Type.MaskedGoon1 }
            },

            { TR2LevelNames.BARTOLI,
                new List<TR2Type>{ TR2Type.StickWieldingGoon1, TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.StickWieldingGoon1 }
            },

            { TR2LevelNames.OPERA,
                new List<TR2Type>{ TR2Type.StickWieldingGoon1, TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.Rat, TR2Type.StickWieldingGoon1, TR2Type.ShotgunGoon }
            },

            { TR2LevelNames.RIG,
                new List<TR2Type>{ TR2Type.Gunman2, TR2Type.StickWieldingGoon1, TR2Type.Doberman, TR2Type.Gunman1 }
            },

            { TR2LevelNames.DA,
                new List<TR2Type>{ TR2Type.FlamethrowerGoon, TR2Type.StickWieldingGoon1, TR2Type.Doberman, TR2Type.Gunman1, TR2Type.Gunman2 }
            },

            { TR2LevelNames.FATHOMS,
                new List<TR2Type>{ TR2Type.Gunman1, TR2Type.StickWieldingGoon1 }
            },

            { TR2LevelNames.DORIA,
                new List<TR2Type>{ TR2Type.Gunman1, TR2Type.StickWieldingGoon1, TR2Type.Gunman2 }
            },

            { TR2LevelNames.LQ,
                new List<TR2Type>{ TR2Type.StickWieldingGoon2, TR2Type.StickWieldingGoon1, TR2Type.Gunman1 }
            },

            { TR2LevelNames.DECK,
                new List<TR2Type>{ TR2Type.StickWieldingGoon1, TR2Type.FlamethrowerGoon, TR2Type.Gunman1 }
            },

            { TR2LevelNames.TIBET,
                new List<TR2Type>{ TR2Type.Mercenary2, TR2Type.Mercenary3 }
            },

            { TR2LevelNames.MONASTERY,
                new List<TR2Type>{ TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick, TR2Type.Mercenary1, TR2Type.Mercenary2 }
            },

            { TR2LevelNames.COT,
                new List<TR2Type>{ TR2Type.Mercenary1, TR2Type.Mercenary2 }
            },

            { TR2LevelNames.CHICKEN,
                new List<TR2Type>{ }
            },

            { TR2LevelNames.XIAN,
                new List<TR2Type>{ }
            },

            { TR2LevelNames.FLOATER,
                new List<TR2Type>{ TR2Type.Knifethrower }
            },

            { TR2LevelNames.LAIR,
                new List<TR2Type>{ TR2Type.Knifethrower }
            },

            { TR2LevelNames.HOME,
                new List<TR2Type>{ TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.ShotgunGoon, TR2Type.StickWieldingGoon1 }
            },

            { "all",
                new List<TR2Type>{ TR2Type.Doberman, TR2Type.MaskedGoon1, TR2Type.ShotgunGoon, TR2Type.MaskedGoon2, TR2Type.MaskedGoon3, TR2Type.Rat,
                TR2Type.StickWieldingGoon1, TR2Type.Gunman2, TR2Type.Gunman1, TR2Type.FlamethrowerGoon, TR2Type.StickWieldingGoon2,
                TR2Type.Mercenary2, TR2Type.Mercenary3, TR2Type.MonkWithKnifeStick, TR2Type.MonkWithLongStick, TR2Type.Mercenary1, TR2Type.Knifethrower
                }
            }
        };
    }
}
