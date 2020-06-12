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
                { LevelNames.GW, new List<TR2Entities>{} }
            };
        }

        public static List<TR2Entities> GetListOfGunAmmoTypes()
        {
            return new List<TR2Entities>
            {
                TR2Entities.Pistols_S_P,
                TR2Entities.Shotgun_S_P,
                TR2Entities.Automags_S_P,
                TR2Entities.Uzi_S_P,
                TR2Entities.Harpoon_S_P,
                TR2Entities.M16_S_P,
                TR2Entities.GrenadeLauncher_S_P,
                TR2Entities.PistolAmmo_S_P,
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
                TR2Entities.Puzzle4_S_P
            };
        }
    }
}
