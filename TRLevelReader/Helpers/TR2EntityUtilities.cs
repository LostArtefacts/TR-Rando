using System.Collections.Generic;
using TRLevelReader.Model.Enums;

namespace TRLevelReader.Helpers
{
    public static class TR2EntityUtilities
    {
        public static readonly Dictionary<TR2Entities, Dictionary<TR2Entities, List<string>>> LevelEntityAliases = new Dictionary<TR2Entities, Dictionary<TR2Entities, List<string>>>
        {
            [TR2Entities.Lara] = new Dictionary<TR2Entities, List<string>>
            {
                [TR2Entities.LaraSun] =
                    new List<string> { TR2LevelNames.GW, TR2LevelNames.GW_CUT, TR2LevelNames.VENICE, TR2LevelNames.BARTOLI, TR2LevelNames.OPERA, TR2LevelNames.OPERA_CUT, TR2LevelNames.RIG, TR2LevelNames.DA, TR2LevelNames.DA_CUT, TR2LevelNames.XIAN, TR2LevelNames.XIAN_CUT, TR2LevelNames.FLOATER, TR2LevelNames.LAIR },
                [TR2Entities.LaraUnwater] =
                    new List<string> { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
                [TR2Entities.LaraSnow] =
                    new List<string> { TR2LevelNames.TIBET, TR2LevelNames.MONASTERY, TR2LevelNames.COT, TR2LevelNames.CHICKEN },
                [TR2Entities.LaraHome] =
                    new List<string> { TR2LevelNames.HOME }
            },
            [TR2Entities.Barracuda] = new Dictionary<TR2Entities, List<string>>
            {
                [TR2Entities.BarracudaIce] =
                    new List<string> { TR2LevelNames.COT, TR2LevelNames.CHICKEN },
                [TR2Entities.BarracudaUnwater] =
                    new List<string> { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
                [TR2Entities.BarracudaXian] =
                    new List<string> { TR2LevelNames.XIAN }
            },
            [TR2Entities.StickWieldingGoon1] = new Dictionary<TR2Entities, List<string>>
            {
                [TR2Entities.StickWieldingGoon1Bandana] =
                    new List<string> { TR2LevelNames.RIG, TR2LevelNames.DA },
                [TR2Entities.StickWieldingGoon1BlackJacket] =
                    new List<string> { TR2LevelNames.HOME },
                [TR2Entities.StickWieldingGoon1BodyWarmer] =
                    new List<string> { TR2LevelNames.VENICE },
                [TR2Entities.StickWieldingGoon1GreenVest] =
                    new List<string> { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
                [TR2Entities.StickWieldingGoon1WhiteVest] =
                    new List<string> { TR2LevelNames.BARTOLI, TR2LevelNames.OPERA }
            },
            [TR2Entities.TigerOrSnowLeopard] = new Dictionary<TR2Entities, List<string>>
            {
                [TR2Entities.BengalTiger] =
                    new List<string> { TR2LevelNames.GW, TR2LevelNames.XIAN },
                [TR2Entities.SnowLeopard] =
                    new List<string> { TR2LevelNames.TIBET, TR2LevelNames.COT },
                [TR2Entities.WhiteTiger] =
                    new List<string> { TR2LevelNames.CHICKEN }
            }
        };

        public static readonly Dictionary<TR2Entities, List<TR2Entities>> EntityFamilies = new Dictionary<TR2Entities, List<TR2Entities>>
        {
            [TR2Entities.Lara] = new List<TR2Entities>
            {
                TR2Entities.LaraSun, TR2Entities.LaraSnow, TR2Entities.LaraUnwater, TR2Entities.LaraHome
            },
            [TR2Entities.Barracuda] = new List<TR2Entities>
            {
                TR2Entities.BarracudaIce, TR2Entities.BarracudaUnwater, TR2Entities.BarracudaXian
            },
            [TR2Entities.StickWieldingGoon1] = new List<TR2Entities>
            {
                TR2Entities.StickWieldingGoon1Bandana, TR2Entities.StickWieldingGoon1BlackJacket, TR2Entities.StickWieldingGoon1BodyWarmer,
                TR2Entities.StickWieldingGoon1GreenVest, TR2Entities.StickWieldingGoon1WhiteVest
            },
            [TR2Entities.TigerOrSnowLeopard] = new List<TR2Entities>
            {
                TR2Entities.BengalTiger, TR2Entities.SnowLeopard, TR2Entities.WhiteTiger
            }
        };

        public static TR2Entities TranslateEntityAlias(TR2Entities entity)
        {
            foreach (TR2Entities parentEntity in EntityFamilies.Keys)
            {
                if (EntityFamilies[parentEntity].Contains(entity))
                {
                    return parentEntity;
                }
            }

            return entity;
        }

        public static TR2Entities GetAliasForLevel(string lvl, TR2Entities entity)
        {
            if (LevelEntityAliases.ContainsKey(entity))
            {
                foreach (TR2Entities alias in LevelEntityAliases[entity].Keys)
                {
                    if (LevelEntityAliases[entity][alias].Contains(lvl))
                    {
                        return alias;
                    }
                }
            }
            return entity;
        }

        public static List<TR2Entities> GetEntityFamily(TR2Entities entity)
        {
            foreach (TR2Entities parentEntity in EntityFamilies.Keys)
            {
                if (EntityFamilies[parentEntity].Contains(entity))
                {
                    return EntityFamilies[parentEntity];
                }
            }

            return new List<TR2Entities> { entity };
        }

        public static List<TR2Entities> RemoveAliases(IEnumerable<TR2Entities> entities)
        {
            List<TR2Entities> ents = new List<TR2Entities>();
            foreach (TR2Entities ent in entities)
            {
                TR2Entities normalisedEnt = TranslateEntityAlias(ent);
                if (!ents.Contains(normalisedEnt))
                {
                    ents.Add(normalisedEnt);
                }
            }
            return ents;
        }

        public static List<TR2Entities> GetLaraTypes()
        {
            return new List<TR2Entities>
            {
                TR2Entities.LaraSun, TR2Entities.LaraUnwater, TR2Entities.LaraSnow, TR2Entities.LaraHome, TR2Entities.LaraInvisible
            };
        }

        public static List<TR2Entities> GetCandidateCrossLevelEnemies()
        {
            return new List<TR2Entities>
            {
                TR2Entities.BarracudaIce,
                TR2Entities.BarracudaUnwater,
                TR2Entities.BarracudaXian,
                TR2Entities.BengalTiger,
                TR2Entities.BirdMonster,
                TR2Entities.BlackMorayEel,
                TR2Entities.Crow,
                TR2Entities.Doberman,
                TR2Entities.Eagle,
                TR2Entities.FlamethrowerGoon,
                TR2Entities.GiantSpider,
                TR2Entities.Gunman1,
                TR2Entities.Gunman2,
                TR2Entities.Knifethrower,
                TR2Entities.MarcoBartoli, // catch-all for Marco, the explosion when the dragon spawns, and the various dragon front/back pieces
                TR2Entities.MaskedGoon1,
                TR2Entities.MaskedGoon2,
                TR2Entities.MaskedGoon3,
                TR2Entities.Mercenary1,
                TR2Entities.Mercenary2,
                TR2Entities.Mercenary3,
                TR2Entities.MercSnowmobDriver,
                TR2Entities.MonkWithKnifeStick,
                TR2Entities.MonkWithLongStick,
                TR2Entities.Rat,
                TR2Entities.ScubaDiver,
                TR2Entities.Shark,
                TR2Entities.ShotgunGoon,
                TR2Entities.SnowLeopard,
                TR2Entities.Spider,
                TR2Entities.StickWieldingGoon1Bandana,
                TR2Entities.StickWieldingGoon1BlackJacket,
                TR2Entities.StickWieldingGoon1BodyWarmer,
                TR2Entities.StickWieldingGoon1GreenVest,
                TR2Entities.StickWieldingGoon1WhiteVest,
                TR2Entities.StickWieldingGoon2,
                TR2Entities.TRex,
                TR2Entities.WhiteTiger,
                TR2Entities.Winston,
                TR2Entities.XianGuardSpear,
                TR2Entities.XianGuardSword,
                TR2Entities.YellowMorayEel,
                TR2Entities.Yeti
            };
        }

        public static List<TR2Entities> GetCrossLevelDroppableEnemies(bool monksAreKillable, bool unconditionalChickens)
        {
            List<TR2Entities> entities = new List<TR2Entities>
            {
                TR2Entities.BengalTiger,
                TR2Entities.Crow,
                TR2Entities.Doberman,
                TR2Entities.Eagle,
                TR2Entities.FlamethrowerGoon,
                TR2Entities.GiantSpider,
                TR2Entities.Gunman1,
                TR2Entities.Gunman2,
                TR2Entities.Knifethrower,
                //TR2Entities.MarcoBartoli, // The dragon can drop items but we are limiting to 1 dragon per level so this is easier than re-allocating drops
                TR2Entities.MaskedGoon1,
                TR2Entities.MaskedGoon2,
                TR2Entities.MaskedGoon3,
                TR2Entities.Mercenary1,
                TR2Entities.Mercenary2,
                TR2Entities.Mercenary3,
                TR2Entities.Rat,
                TR2Entities.ShotgunGoon,
                TR2Entities.SnowLeopard,
                TR2Entities.StickWieldingGoon1Bandana,
                TR2Entities.StickWieldingGoon1BlackJacket,
                TR2Entities.StickWieldingGoon1BodyWarmer,
                TR2Entities.StickWieldingGoon1GreenVest,
                TR2Entities.StickWieldingGoon1WhiteVest,
                TR2Entities.StickWieldingGoon2,
                TR2Entities.TRex,
                TR2Entities.WhiteTiger,
                TR2Entities.Yeti
            };

            // #131 Provides an option to exclude monks as having to be killed
            if (monksAreKillable)
            {
                entities.Add(TR2Entities.MonkWithKnifeStick);
                entities.Add(TR2Entities.MonkWithLongStick);
            }

            if (unconditionalChickens)
            {
                entities.Add(TR2Entities.BirdMonster);
            }

            return entities;
        }

        // This is the full list of enemies including alias duplicates - used for 
        // checking while iterating an entity list to determine if an entity is 
        // an enemy and can be replaced.
        public static List<TR2Entities> GetFullListOfEnemies()
        {
            return new List<TR2Entities>
            {
                TR2Entities.Barracuda,
                TR2Entities.BarracudaIce,
                TR2Entities.BarracudaUnwater,
                TR2Entities.BarracudaXian,
                TR2Entities.BengalTiger,
                TR2Entities.BirdMonster,
                TR2Entities.BlackMorayEel,
                TR2Entities.Crow,
                TR2Entities.Doberman,
                TR2Entities.Eagle,
                TR2Entities.FlamethrowerGoon,
                TR2Entities.GiantSpider,
                TR2Entities.Gunman1,
                TR2Entities.Gunman2,
                TR2Entities.Knifethrower,
                TR2Entities.MarcoBartoli,
                TR2Entities.MaskedGoon1,
                TR2Entities.MaskedGoon2,
                TR2Entities.MaskedGoon3,
                TR2Entities.Mercenary1,
                TR2Entities.Mercenary2,
                TR2Entities.Mercenary3,
                TR2Entities.MercSnowmobDriver,
                TR2Entities.MonkWithKnifeStick,
                TR2Entities.MonkWithLongStick,
                TR2Entities.Rat,
                TR2Entities.ScubaDiver,
                TR2Entities.Shark,
                TR2Entities.ShotgunGoon,
                TR2Entities.SnowLeopard,
                TR2Entities.Spider,
                TR2Entities.StickWieldingGoon1,
                TR2Entities.StickWieldingGoon1Bandana,
                TR2Entities.StickWieldingGoon1BlackJacket,
                TR2Entities.StickWieldingGoon1BodyWarmer,
                TR2Entities.StickWieldingGoon1GreenVest,
                TR2Entities.StickWieldingGoon1WhiteVest,
                TR2Entities.StickWieldingGoon2,
                TR2Entities.TigerOrSnowLeopard,
                TR2Entities.TRex,
                TR2Entities.WhiteTiger,
                TR2Entities.Winston,
                TR2Entities.XianGuardSpear,
                TR2Entities.XianGuardSword,
                TR2Entities.YellowMorayEel,
                TR2Entities.Yeti
            };
        }

        public static bool IsEnemyType(TR2Entities entity)
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

        public static Dictionary<string, List<TR2Entities>> GetEnemyTypeDictionary()
        {
            return new Dictionary<string, List<TR2Entities>>
            {
                { TR2LevelNames.GW,
                    new List<TR2Entities>{ TR2Entities.Crow, TR2Entities.TigerOrSnowLeopard, TR2Entities.Spider, TR2Entities.TRex }
                },

                { TR2LevelNames.VENICE,
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.StickWieldingGoon1, TR2Entities.Rat, TR2Entities.MaskedGoon1 }
                },

                { TR2LevelNames.BARTOLI,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat }
                },

                { TR2LevelNames.OPERA,
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat, TR2Entities.StickWieldingGoon1, TR2Entities.ShotgunGoon }
                },

                { TR2LevelNames.RIG,
                    new List<TR2Entities>{ TR2Entities.Gunman2, TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.Gunman1, TR2Entities.ScubaDiver }
                },

                { TR2LevelNames.DA,
                    new List<TR2Entities>{ TR2Entities.FlamethrowerGoon, TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.Gunman1, TR2Entities.Gunman2, TR2Entities.ScubaDiver }
                },

                { TR2LevelNames.FATHOMS,
                    new List<TR2Entities>{ TR2Entities.Shark, TR2Entities.ScubaDiver, TR2Entities.Gunman1, TR2Entities.Barracuda, TR2Entities.StickWieldingGoon1 }
                },

                { TR2LevelNames.DORIA,
                    new List<TR2Entities>{ TR2Entities.Shark, TR2Entities.ScubaDiver, TR2Entities.Gunman1, TR2Entities.Barracuda, TR2Entities.StickWieldingGoon1, TR2Entities.YellowMorayEel, TR2Entities.Gunman2 }
                },

                { TR2LevelNames.LQ,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon2, TR2Entities.StickWieldingGoon1, TR2Entities.Gunman1, TR2Entities.ScubaDiver, TR2Entities.BlackMorayEel, TR2Entities.Barracuda }
                },

                { TR2LevelNames.DECK,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.FlamethrowerGoon, TR2Entities.Barracuda, TR2Entities.ScubaDiver, TR2Entities.Shark, TR2Entities.Gunman1 }
                },

                { TR2LevelNames.TIBET,
                    new List<TR2Entities>{ TR2Entities.Eagle, TR2Entities.Mercenary2, TR2Entities.Mercenary3, TR2Entities.TigerOrSnowLeopard, TR2Entities.MercSnowmobDriver }
                },

                { TR2LevelNames.MONASTERY,
                    new List<TR2Entities>{ TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick, TR2Entities.Mercenary1, TR2Entities.Crow, TR2Entities.Mercenary2 }
                },

                { TR2LevelNames.COT,
                    new List<TR2Entities>{ TR2Entities.TigerOrSnowLeopard, TR2Entities.Mercenary1, TR2Entities.Mercenary2, TR2Entities.Yeti, TR2Entities.Barracuda }
                },

                { TR2LevelNames.CHICKEN,
                    new List<TR2Entities>{ TR2Entities.TigerOrSnowLeopard, TR2Entities.Barracuda, TR2Entities.Yeti, TR2Entities.BirdMonster }
                },

                { TR2LevelNames.XIAN,
                    new List<TR2Entities>{ TR2Entities.Barracuda, TR2Entities.TigerOrSnowLeopard, TR2Entities.Eagle, TR2Entities.Spider, TR2Entities.GiantSpider }
                },

                { TR2LevelNames.FLOATER,
                    new List<TR2Entities>{ TR2Entities.XianGuardSword, TR2Entities.XianGuardSpear, TR2Entities.Knifethrower }
                },

                { TR2LevelNames.LAIR,
                    new List<TR2Entities>{ TR2Entities.Knifethrower, TR2Entities.XianGuardSpear, TR2Entities.MarcoBartoli }
                },

                { TR2LevelNames.HOME,
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.StickWieldingGoon1, TR2Entities.ShotgunGoon }
                },

                { TR2LevelNames.ASSAULT,
                    new List<TR2Entities>{ }
                },

                { "all",
                    new List<TR2Entities>{ TR2Entities.Crow, TR2Entities.TigerOrSnowLeopard, TR2Entities.Spider, TR2Entities.TRex,
                    TR2Entities.Doberman, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.StickWieldingGoon1, TR2Entities.MaskedGoon1, TR2Entities.Rat, TR2Entities.ShotgunGoon,
                    TR2Entities.Gunman2, TR2Entities.Gunman1, TR2Entities.ScubaDiver, TR2Entities.FlamethrowerGoon, TR2Entities.Shark, TR2Entities.Barracuda, TR2Entities.YellowMorayEel,
                    TR2Entities.StickWieldingGoon2, TR2Entities.BlackMorayEel, TR2Entities.Eagle, TR2Entities.Mercenary2, TR2Entities.Mercenary3, TR2Entities.MercSnowmobDriver,
                    TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick, TR2Entities.Mercenary1, TR2Entities.Yeti, TR2Entities.GiantSpider,
                    TR2Entities.XianGuardSword, TR2Entities.XianGuardSpear, TR2Entities.Knifethrower
                    }
                }
            };
        }

        public static List<TR2Entities> GetListOfGunTypes()
        {
            return new List<TR2Entities>
            {
                TR2Entities.Shotgun_S_P,
                TR2Entities.Automags_S_P,
                TR2Entities.Uzi_S_P,
                TR2Entities.Harpoon_S_P,
                TR2Entities.M16_S_P,
                TR2Entities.GrenadeLauncher_S_P,
            };
        }

        public static bool IsGunType(TR2Entities entity)
        {
            return (entity == TR2Entities.Shotgun_S_P ||
                    entity == TR2Entities.Automags_S_P ||
                    entity == TR2Entities.Uzi_S_P ||
                    entity == TR2Entities.Harpoon_S_P ||
                    entity == TR2Entities.M16_S_P ||
                    entity == TR2Entities.GrenadeLauncher_S_P);
        }

        public static List<TR2Entities> GetListOfAmmoTypes()
        {
            return new List<TR2Entities>
            {
                TR2Entities.ShotgunAmmo_S_P,
                TR2Entities.AutoAmmo_S_P,
                TR2Entities.UziAmmo_S_P,
                TR2Entities.HarpoonAmmo_S_P,
                TR2Entities.M16Ammo_S_P,
                TR2Entities.Grenades_S_P,
                TR2Entities.SmallMed_S_P,
                TR2Entities.LargeMed_S_P,
                TR2Entities.Flares_S_P,
            };
        }

        public static bool IsUtilityType(TR2Entities entity)
        {
            return (entity == TR2Entities.ShotgunAmmo_S_P ||
                    entity == TR2Entities.AutoAmmo_S_P ||
                    entity == TR2Entities.UziAmmo_S_P ||
                    entity == TR2Entities.HarpoonAmmo_S_P ||
                    entity == TR2Entities.M16Ammo_S_P ||
                    entity == TR2Entities.Grenades_S_P ||
                    entity == TR2Entities.SmallMed_S_P ||
                    entity == TR2Entities.LargeMed_S_P ||
                    entity == TR2Entities.Flares_S_P);
        }

        /// <summary>
        /// returns true if the parameter provided is of one of the 3 secrets type 
        /// </summary>
        /// <param name="entity"><see cref="TR2Entities"/></param>
        /// <returns>entity == TR2Entities.StoneSecret_S_P || entity == TR2Entities.JadeSecret_S_P || entity == TR2Entities.GoldSecret_S_P;</returns>
        public static bool IsSecretType(TR2Entities entity)
        {
            return entity == TR2Entities.StoneSecret_S_P || entity == TR2Entities.JadeSecret_S_P || entity == TR2Entities.GoldSecret_S_P;
        }

        public static bool IsAmmoType(TR2Entities entity)
        {
            return (entity == TR2Entities.ShotgunAmmo_S_P ||
                    entity == TR2Entities.AutoAmmo_S_P ||
                    entity == TR2Entities.UziAmmo_S_P ||
                    entity == TR2Entities.HarpoonAmmo_S_P ||
                    entity == TR2Entities.M16Ammo_S_P ||
                    entity == TR2Entities.Grenades_S_P);
        }

        public static List<TR2Entities> GetListOfKeyItemTypes()
        {
            return new List<TR2Entities>
            {
                TR2Entities.Key1_S_P,
                TR2Entities.Key2_S_P,
                TR2Entities.Key3_S_P,
                TR2Entities.Key4_S_P,
                TR2Entities.Puzzle1_S_P,
                TR2Entities.Puzzle2_S_P,
                TR2Entities.Puzzle3_S_P,
                TR2Entities.Puzzle4_S_P,
                TR2Entities.Quest1_S_P,
                TR2Entities.Quest2_S_P
            };
        }

        public static List<TR2Entities> GetListOfSecretTypes()
        {
            return new List<TR2Entities>
            {
                TR2Entities.StoneSecret_S_P,
                TR2Entities.JadeSecret_S_P,
                TR2Entities.GoldSecret_S_P
            };
        }

        public static bool IsKeyItemType(TR2Entities entity)
        {
            return (entity == TR2Entities.Key1_S_P ||
                    entity == TR2Entities.Key2_S_P ||
                    entity == TR2Entities.Key3_S_P ||
                    entity == TR2Entities.Key4_S_P ||
                    entity == TR2Entities.Puzzle1_S_P ||
                    entity == TR2Entities.Puzzle2_S_P ||
                    entity == TR2Entities.Puzzle3_S_P ||
                    entity == TR2Entities.Puzzle4_S_P ||
                    entity == TR2Entities.Quest1_S_P ||
                    entity == TR2Entities.Quest2_S_P);
        }

        public static bool IsAnyPickupType(TR2Entities entity)
        {
            return entity == TR2Entities.Pistols_S_P ||
                entity == TR2Entities.PistolAmmo_S_P ||
                IsAmmoType(entity) ||
                IsGunType(entity) ||
                IsKeyItemType(entity) ||
                IsUtilityType(entity) ||
                IsSecretType(entity);
        }

        public static bool IsWaterCreature(TR2Entities entity)
        {
            return (entity == TR2Entities.Shark || entity == TR2Entities.YellowMorayEel || entity == TR2Entities.BlackMorayEel ||
                entity == TR2Entities.Barracuda || entity == TR2Entities.BarracudaIce || entity == TR2Entities.BarracudaUnwater ||
                entity == TR2Entities.BarracudaXian || entity == TR2Entities.ScubaDiver);
        }

        public static List<TR2Entities> WaterCreatures()
        {
            return new List<TR2Entities>
            {
                TR2Entities.Shark,
                TR2Entities.BarracudaIce,
                TR2Entities.BarracudaUnwater,
                TR2Entities.BarracudaXian,
                TR2Entities.YellowMorayEel,
                TR2Entities.BlackMorayEel,
                TR2Entities.ScubaDiver
            };
        }

        public static List<TR2Entities> KillableWaterCreatures()
        {
            return new List<TR2Entities>
            {
                TR2Entities.Shark,
                TR2Entities.BarracudaIce,
                TR2Entities.BarracudaUnwater,
                TR2Entities.BarracudaXian,
                TR2Entities.ScubaDiver
            };
        }

        public static bool IsStaticCreature(TR2Entities entity)
        {
            return entity == TR2Entities.YellowMorayEel || entity == TR2Entities.BlackMorayEel;
        }

        public static bool IsHazardCreature(TR2Entities entity)
        {
            return entity == TR2Entities.YellowMorayEel || entity == TR2Entities.BlackMorayEel || entity == TR2Entities.Winston;
        }

        public static List<TR2Entities> FilterWaterEnemies(List<TR2Entities> entities)
        {
            List<TR2Entities> waterEntities = new List<TR2Entities>();
            foreach (TR2Entities entity in entities)
            {
                if (IsWaterCreature(entity))
                {
                    waterEntities.Add(entity);
                }
            }
            return waterEntities;
        }

        public static bool CanDropPickups(TR2Entities entity, bool monksAreKillable, bool unconditionalChickens)
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

        public static bool IsMonk(TR2Entities entity)
        {
            return entity == TR2Entities.MonkWithKnifeStick || entity == TR2Entities.MonkWithLongStick;
        }

        public static List<TR2Entities> FilterDroppableEnemies(List<TR2Entities> entities, bool monksAreKillable, bool unconditionalChickens)
        {
            List<TR2Entities> droppableEntities = new List<TR2Entities>();
            foreach (TR2Entities entity in entities)
            {
                if (CanDropPickups(entity, monksAreKillable, unconditionalChickens))
                {
                    droppableEntities.Add(entity);
                }
            }
            return droppableEntities;
        }

        public static List<TR2Entities> DoorTypes()
        {
            return new List<TR2Entities>
            {
                TR2Entities.Door1, TR2Entities.Door2, TR2Entities.Door3,
                TR2Entities.Door4, TR2Entities.Door5, TR2Entities.LiftingDoor1,
                TR2Entities.LiftingDoor2, TR2Entities.LiftingDoor3
            };
        }

        public static bool IsDoorType(TR2Entities entity)
        {
            return DoorTypes().Contains(entity);
        }

        public static Dictionary<string, List<TR2Entities>> DroppableEnemyTypes()
        {
            return new Dictionary<string, List<TR2Entities>>
            {
                { TR2LevelNames.GW,
                    new List<TR2Entities>{ }
                },

                { TR2LevelNames.VENICE,
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.StickWieldingGoon1, TR2Entities.MaskedGoon1 }
                },

                { TR2LevelNames.BARTOLI,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.StickWieldingGoon1 }
                },

                { TR2LevelNames.OPERA,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat, TR2Entities.StickWieldingGoon1, TR2Entities.ShotgunGoon }
                },

                { TR2LevelNames.RIG,
                    new List<TR2Entities>{ TR2Entities.Gunman2, TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.Gunman1 }
                },

                { TR2LevelNames.DA,
                    new List<TR2Entities>{ TR2Entities.FlamethrowerGoon, TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.Gunman1, TR2Entities.Gunman2 }
                },

                { TR2LevelNames.FATHOMS,
                    new List<TR2Entities>{ TR2Entities.Gunman1, TR2Entities.StickWieldingGoon1 }
                },

                { TR2LevelNames.DORIA,
                    new List<TR2Entities>{ TR2Entities.Gunman1, TR2Entities.StickWieldingGoon1, TR2Entities.Gunman2 }
                },

                { TR2LevelNames.LQ,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon2, TR2Entities.StickWieldingGoon1, TR2Entities.Gunman1 }
                },

                { TR2LevelNames.DECK,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.FlamethrowerGoon, TR2Entities.Gunman1 }
                },

                { TR2LevelNames.TIBET,
                    new List<TR2Entities>{ TR2Entities.Mercenary2, TR2Entities.Mercenary3 }
                },

                { TR2LevelNames.MONASTERY,
                    new List<TR2Entities>{ TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick, TR2Entities.Mercenary1, TR2Entities.Mercenary2 }
                },

                { TR2LevelNames.COT,
                    new List<TR2Entities>{ TR2Entities.Mercenary1, TR2Entities.Mercenary2 }
                },

                { TR2LevelNames.CHICKEN,
                    new List<TR2Entities>{ }
                },

                { TR2LevelNames.XIAN,
                    new List<TR2Entities>{ }
                },

                { TR2LevelNames.FLOATER,
                    new List<TR2Entities>{ TR2Entities.Knifethrower }
                },

                { TR2LevelNames.LAIR,
                    new List<TR2Entities>{ TR2Entities.Knifethrower }
                },

                { TR2LevelNames.HOME,
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.ShotgunGoon, TR2Entities.StickWieldingGoon1 }
                },

                { "all",
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.ShotgunGoon, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat,
                    TR2Entities.StickWieldingGoon1, TR2Entities.Gunman2, TR2Entities.Gunman1, TR2Entities.FlamethrowerGoon, TR2Entities.StickWieldingGoon2,
                    TR2Entities.Mercenary2, TR2Entities.Mercenary3, TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick, TR2Entities.Mercenary1, TR2Entities.Knifethrower
                    }
                }
            };
        }
    }
}