using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model.Enums;

namespace TRLevelReader.Helpers
{
    public static class TR2EntityUtilities
    {
        public static List<TR2Entities> GetListOfEnemyTypes()
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
        }

        public static Dictionary<string, List<TR2Entities>> GetEnemyTypeDictionary()
        {
            return new Dictionary<string, List<TR2Entities>>
            {
                { LevelNames.GW, 
                    new List<TR2Entities>{ TR2Entities.Crow, TR2Entities.TigerOrSnowLeopard, TR2Entities.Spider, TR2Entities.TRex } 
                },

                { LevelNames.VENICE, 
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.StickWieldingGoon1, TR2Entities.Rat, TR2Entities.MaskedGoon1 } 
                },

                { LevelNames.BARTOLI, 
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat, TR2Entities.StickWieldingGoon1 } 
                },

                { LevelNames.OPERA, 
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat, TR2Entities.StickWieldingGoon1, TR2Entities.ShotgunGoon } 
                },

                { LevelNames.RIG, 
                    new List<TR2Entities>{ TR2Entities.Gunman2, TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.Gunman1, TR2Entities.ScubaDiver } 
                },

                { LevelNames.DA, 
                    new List<TR2Entities>{ TR2Entities.FlamethrowerGoon, TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.Gunman1, TR2Entities.Gunman2, TR2Entities.ScubaDiver } 
                },

                { LevelNames.FATHOMS, 
                    new List<TR2Entities>{ TR2Entities.Shark, TR2Entities.ScubaDiver, TR2Entities.Gunman1, TR2Entities.Barracuda, TR2Entities.StickWieldingGoon1 }
                },

                { LevelNames.DORIA, 
                    new List<TR2Entities>{ TR2Entities.Shark, TR2Entities.ScubaDiver, TR2Entities.Gunman1, TR2Entities.Barracuda, TR2Entities.StickWieldingGoon1, TR2Entities.YellowMorayEel, TR2Entities.Gunman2 }
                },

                { LevelNames.LQ, 
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon2, TR2Entities.StickWieldingGoon1, TR2Entities.Gunman1, TR2Entities.ScubaDiver, TR2Entities.BlackMorayEel, TR2Entities.Barracuda }
                },

                { LevelNames.DECK, 
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.FlamethrowerGoon, TR2Entities.Barracuda, TR2Entities.ScubaDiver, TR2Entities.Shark, TR2Entities.Gunman1 } 
                },

                { LevelNames.TIBET, 
                    new List<TR2Entities>{ TR2Entities.Eagle, TR2Entities.Mercenary2, TR2Entities.Mercenary3, TR2Entities.TigerOrSnowLeopard, TR2Entities.MercSnowmobDriver } 
                },

                { LevelNames.MONASTERY, 
                    new List<TR2Entities>{ TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick, TR2Entities.Mercenary1, TR2Entities.Crow, TR2Entities.Mercenary2 } 
                },

                { LevelNames.COT, 
                    new List<TR2Entities>{ TR2Entities.TigerOrSnowLeopard, TR2Entities.Mercenary1, TR2Entities.Mercenary2, TR2Entities.Yeti, TR2Entities.Barracuda } 
                },

                { LevelNames.CHICKEN, 
                    new List<TR2Entities>{ TR2Entities.TigerOrSnowLeopard, TR2Entities.Barracuda, TR2Entities.Yeti, TR2Entities.BirdMonster } 
                },

                { LevelNames.XIAN, 
                    new List<TR2Entities>{ TR2Entities.Barracuda, TR2Entities.TigerOrSnowLeopard, TR2Entities.Eagle, TR2Entities.Spider, TR2Entities.GiantSpider } 
                },

                { LevelNames.FLOATER, 
                    new List<TR2Entities>{ TR2Entities.XianGuardSword, TR2Entities.XianGuardSpear, TR2Entities.Knifethrower } 
                },

                { LevelNames.LAIR, 
                    new List<TR2Entities>{ TR2Entities.Knifethrower, TR2Entities.XianGuardSpear } 
                },

                { LevelNames.HOME, 
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.ShotgunGoon, TR2Entities.StickWieldingGoon1 } 
                },
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

        public static bool IsAmmoType(TR2Entities entity)
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

        public static bool IsWaterCreature(TR2Entities entity)
        {
            return (entity == TR2Entities.Shark || entity == TR2Entities.YellowMorayEel || entity == TR2Entities.BlackMorayEel ||
                entity == TR2Entities.Barracuda || entity == TR2Entities.ScubaDiver);
        }

        public static List<TR2Entities> WaterCreatures()
        {
            return new List<TR2Entities>
            {
                TR2Entities.Shark,
                TR2Entities.Barracuda,
                TR2Entities.YellowMorayEel,
                TR2Entities.BlackMorayEel,
                TR2Entities.ScubaDiver
            };
        }

        public static bool CanDropPickups(TR2Entities entity)
        {
            return (entity == TR2Entities.Doberman ||
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
                    entity == TR2Entities.MonkWithKnifeStick);
        }

        public static Dictionary<string, List<TR2Entities>> DroppableEnemyTypes()
        {
            return new Dictionary<string, List<TR2Entities>>
            {
                { LevelNames.GW,
                    new List<TR2Entities>{ }
                },

                { LevelNames.VENICE,
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.StickWieldingGoon1, TR2Entities.MaskedGoon1 }
                },

                { LevelNames.BARTOLI,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.StickWieldingGoon1 }
                },

                { LevelNames.OPERA,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.MaskedGoon2, TR2Entities.MaskedGoon3, TR2Entities.Rat, TR2Entities.StickWieldingGoon1, TR2Entities.ShotgunGoon }
                },

                { LevelNames.RIG,
                    new List<TR2Entities>{ TR2Entities.Gunman2, TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.Gunman1 }
                },

                { LevelNames.DA,
                    new List<TR2Entities>{ TR2Entities.FlamethrowerGoon, TR2Entities.StickWieldingGoon1, TR2Entities.Doberman, TR2Entities.Gunman1, TR2Entities.Gunman2 }
                },

                { LevelNames.FATHOMS,
                    new List<TR2Entities>{ TR2Entities.Gunman1, TR2Entities.StickWieldingGoon1 }
                },

                { LevelNames.DORIA,
                    new List<TR2Entities>{ TR2Entities.Gunman1, TR2Entities.StickWieldingGoon1, TR2Entities.Gunman2 }
                },

                { LevelNames.LQ,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon2, TR2Entities.StickWieldingGoon1, TR2Entities.Gunman1 }
                },

                { LevelNames.DECK,
                    new List<TR2Entities>{ TR2Entities.StickWieldingGoon1, TR2Entities.FlamethrowerGoon, TR2Entities.Gunman1 }
                },

                { LevelNames.TIBET,
                    new List<TR2Entities>{ TR2Entities.Mercenary2, TR2Entities.Mercenary3 }
                },

                { LevelNames.MONASTERY,
                    new List<TR2Entities>{ TR2Entities.MonkWithKnifeStick, TR2Entities.MonkWithLongStick, TR2Entities.Mercenary1, TR2Entities.Mercenary2 }
                },

                { LevelNames.COT,
                    new List<TR2Entities>{ TR2Entities.Mercenary1, TR2Entities.Mercenary2 }
                },

                { LevelNames.CHICKEN,
                    new List<TR2Entities>{ }
                },

                { LevelNames.XIAN,
                    new List<TR2Entities>{ }
                },

                { LevelNames.FLOATER,
                    new List<TR2Entities>{ TR2Entities.XianGuardSword, TR2Entities.XianGuardSpear, TR2Entities.Knifethrower }
                },

                { LevelNames.LAIR,
                    new List<TR2Entities>{ TR2Entities.Knifethrower, TR2Entities.XianGuardSpear }
                },

                { LevelNames.HOME,
                    new List<TR2Entities>{ TR2Entities.Doberman, TR2Entities.MaskedGoon1, TR2Entities.ShotgunGoon, TR2Entities.StickWieldingGoon1 }
                },
            };
        }
    }
}
