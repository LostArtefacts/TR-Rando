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
            throw new NotImplementedException();
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
                TR2Entities.Key4_S_P
            };
        }
    }
}
