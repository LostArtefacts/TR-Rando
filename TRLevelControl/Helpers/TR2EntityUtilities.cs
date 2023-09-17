using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TR2EntityUtilities
{
    public static readonly Dictionary<TR2Type, Dictionary<TR2Type, List<string>>> LevelEntityAliases = new()
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

    public static readonly Dictionary<TR2Type, List<TR2Type>> EntityFamilies = new()
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

    public static TR2Type TranslateEntityAlias(TR2Type entity)
    {
        foreach (TR2Type parentEntity in EntityFamilies.Keys)
        {
            if (EntityFamilies[parentEntity].Contains(entity))
            {
                return parentEntity;
            }
        }

        return entity;
    }

    public static TR2Type GetAliasForLevel(string lvl, TR2Type entity)
    {
        if (LevelEntityAliases.ContainsKey(entity))
        {
            foreach (TR2Type alias in LevelEntityAliases[entity].Keys)
            {
                if (LevelEntityAliases[entity][alias].Contains(lvl))
                {
                    return alias;
                }
            }
        }
        return entity;
    }

    public static List<TR2Type> GetEntityFamily(TR2Type entity)
    {
        foreach (TR2Type parentEntity in EntityFamilies.Keys)
        {
            if (EntityFamilies[parentEntity].Contains(entity))
            {
                return EntityFamilies[parentEntity];
            }
        }

        return new List<TR2Type> { entity };
    }

    public static List<TR2Type> RemoveAliases(IEnumerable<TR2Type> entities)
    {
        List<TR2Type> ents = new();
        foreach (TR2Type ent in entities)
        {
            TR2Type normalisedEnt = TranslateEntityAlias(ent);
            if (!ents.Contains(normalisedEnt))
            {
                ents.Add(normalisedEnt);
            }
        }
        return ents;
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
        List<TR2Type> entities = new()
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
            //TR2Entities.MarcoBartoli, // The dragon can drop items but we are limiting to 1 dragon per level so this is easier than re-allocating drops
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
            entities.Add(TR2Type.MonkWithKnifeStick);
            entities.Add(TR2Type.MonkWithLongStick);
        }

        if (unconditionalChickens)
        {
            entities.Add(TR2Type.BirdMonster);
        }

        return entities;
    }

    // This is the full list of enemies including alias duplicates - used for 
    // checking while iterating an entity list to determine if an entity is 
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

    public static bool IsEnemyType(TR2Type entity)
    {
        return GetFullListOfEnemies().Contains(entity);
    }

    /*public static List<TR2Entities> GetListOfEnemyTypes()
    {
        return new List<TR2Entities>
        {
            TR2Entities.Doberman,
            TR2Entities.MaskedGoon1,
            TR2Entities.MaskedGoon2,
            TR2Entities.MaskedGoon3,
            TR2Entities.Knifethrower,
            TR2Entities.ShotgunGoon,
            TR2Entities.Rat,
            TR2Entities.Shark,
            TR2Entities.YellowMorayEel,
            TR2Entities.BlackMorayEel,
            TR2Entities.Barracuda,
            TR2Entities.ScubaDiver,
            TR2Entities.Gunman1,
            TR2Entities.Gunman2,
            TR2Entities.StickWieldingGoon1,
            TR2Entities.StickWieldingGoon2,
            TR2Entities.FlamethrowerGoon,
            TR2Entities.Spider,
            TR2Entities.GiantSpider,
            TR2Entities.Crow,
            TR2Entities.TigerOrSnowLeopard,
            TR2Entities.MarcoBartoli,
            TR2Entities.XianGuardSpear,
            TR2Entities.XianGuardSpearStatue,
            TR2Entities.XianGuardSword,
            TR2Entities.XianGuardSwordStatue,
            TR2Entities.Yeti,
            TR2Entities.BirdMonster,
            TR2Entities.Eagle,
            TR2Entities.Mercenary1,
            TR2Entities.Mercenary2,
            TR2Entities.Mercenary3,
            TR2Entities.MercSnowmobDriver,
            TR2Entities.MonkWithLongStick,
            TR2Entities.MonkWithKnifeStick,
            TR2Entities.TRex,
            TR2Entities.Monk,
            TR2Entities.Winston
        };
    }*/

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

    public static bool IsGunType(TR2Type entity)
    {
        return (entity == TR2Type.Shotgun_S_P ||
                entity == TR2Type.Automags_S_P ||
                entity == TR2Type.Uzi_S_P ||
                entity == TR2Type.Harpoon_S_P ||
                entity == TR2Type.M16_S_P ||
                entity == TR2Type.GrenadeLauncher_S_P);
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

    public static bool IsUtilityType(TR2Type entity)
    {
        return (entity == TR2Type.ShotgunAmmo_S_P ||
                entity == TR2Type.AutoAmmo_S_P ||
                entity == TR2Type.UziAmmo_S_P ||
                entity == TR2Type.HarpoonAmmo_S_P ||
                entity == TR2Type.M16Ammo_S_P ||
                entity == TR2Type.Grenades_S_P ||
                entity == TR2Type.SmallMed_S_P ||
                entity == TR2Type.LargeMed_S_P ||
                entity == TR2Type.Flares_S_P);
    }

    /// <summary>
    /// returns true if the parameter provided is of one of the 3 secrets type 
    /// </summary>
    /// <param name="entity"><see cref="TR2Type"/></param>
    /// <returns>entity == TR2Entities.StoneSecret_S_P || entity == TR2Entities.JadeSecret_S_P || entity == TR2Entities.GoldSecret_S_P;</returns>
    public static bool IsSecretType(TR2Type entity)
    {
        return entity == TR2Type.StoneSecret_S_P || entity == TR2Type.JadeSecret_S_P || entity == TR2Type.GoldSecret_S_P;
    }

    public static bool IsAmmoType(TR2Type entity)
    {
        return (entity == TR2Type.ShotgunAmmo_S_P ||
                entity == TR2Type.AutoAmmo_S_P ||
                entity == TR2Type.UziAmmo_S_P ||
                entity == TR2Type.HarpoonAmmo_S_P ||
                entity == TR2Type.M16Ammo_S_P ||
                entity == TR2Type.Grenades_S_P);
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

    public static bool IsKeyItemType(TR2Type entity)
    {
        return (entity == TR2Type.Key1_S_P ||
                entity == TR2Type.Key2_S_P ||
                entity == TR2Type.Key3_S_P ||
                entity == TR2Type.Key4_S_P ||
                entity == TR2Type.Puzzle1_S_P ||
                entity == TR2Type.Puzzle2_S_P ||
                entity == TR2Type.Puzzle3_S_P ||
                entity == TR2Type.Puzzle4_S_P ||
                entity == TR2Type.Quest1_S_P ||
                entity == TR2Type.Quest2_S_P);
    }

    public static bool IsAnyPickupType(TR2Type entity)
    {
        return entity == TR2Type.Pistols_S_P ||
            entity == TR2Type.PistolAmmo_S_P ||
            IsAmmoType(entity) ||
            IsGunType(entity) ||
            IsKeyItemType(entity) ||
            IsUtilityType(entity) ||
            IsSecretType(entity);
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

    public static bool IsSwitchType(TR2Type entity)
    {
        return GetSwitchTypes().Contains(entity);
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

    public static bool IsKeyholeType(TR2Type entity)
    {
        return GetKeyholeTypes().Contains(entity);
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

    public static bool IsSlotType(TR2Type entity)
    {
        return GetSlotTypes().Contains(entity);
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

    public static bool IsPushblockType(TR2Type entity)
    {
        return GetPushblockTypes().Contains(entity);
    }

    public static bool IsWaterCreature(TR2Type entity)
    {
        return (entity == TR2Type.Shark || entity == TR2Type.YellowMorayEel || entity == TR2Type.BlackMorayEel ||
            entity == TR2Type.Barracuda || entity == TR2Type.BarracudaIce || entity == TR2Type.BarracudaUnwater ||
            entity == TR2Type.BarracudaXian || entity == TR2Type.ScubaDiver);
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

    public static bool IsStaticCreature(TR2Type entity)
    {
        return entity == TR2Type.YellowMorayEel || entity == TR2Type.BlackMorayEel;
    }

    public static bool IsHazardCreature(TR2Type entity)
    {
        return entity == TR2Type.YellowMorayEel || entity == TR2Type.BlackMorayEel || entity == TR2Type.Winston;
    }

    public static List<TR2Type> FilterWaterEnemies(List<TR2Type> entities)
    {
        List<TR2Type> waterEntities = new();
        foreach (TR2Type entity in entities)
        {
            if (IsWaterCreature(entity))
            {
                waterEntities.Add(entity);
            }
        }
        return waterEntities;
    }

    public static bool CanDropPickups(TR2Type entity, bool monksAreKillable, bool unconditionalChickens)
    {
        return GetCrossLevelDroppableEnemies(monksAreKillable, unconditionalChickens).Contains(entity);
        /*return (entity == TR2Entities.Doberman ||
                entity == TR2Entities.MaskedGoon1 ||
                entity == TR2Entities.MaskedGoon2 ||
                entity == TR2Entities.MaskedGoon3 ||
                entity == TR2Entities.Knifethrower ||
                entity == TR2Entities.ShotgunGoon ||
                entity == TR2Entities.Gunman1 ||
                entity == TR2Entities.Gunman2 ||
                entity == TR2Entities.StickWieldingGoon1 ||
                entity == TR2Entities.StickWieldingGoon2 ||
                entity == TR2Entities.FlamethrowerGoon ||
                entity == TR2Entities.Mercenary1 ||
                entity == TR2Entities.Mercenary2 ||
                entity == TR2Entities.Mercenary3 ||
                entity == TR2Entities.MonkWithLongStick ||
                entity == TR2Entities.MonkWithKnifeStick);*/
    }

    public static bool IsMonk(TR2Type entity)
    {
        return entity == TR2Type.MonkWithKnifeStick || entity == TR2Type.MonkWithLongStick;
    }

    public static List<TR2Type> FilterDroppableEnemies(List<TR2Type> entities, bool monksAreKillable, bool unconditionalChickens)
    {
        List<TR2Type> droppableEntities = new();
        foreach (TR2Type entity in entities)
        {
            if (CanDropPickups(entity, monksAreKillable, unconditionalChickens))
            {
                droppableEntities.Add(entity);
            }
        }
        return droppableEntities;
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

    public static bool IsDoorType(TR2Type entity)
    {
        return DoorTypes().Contains(entity);
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
