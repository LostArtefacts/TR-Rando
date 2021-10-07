using System.Collections.Generic;
using System.Linq;
using TRGE.Core.Item.Enums;
using TRLevelReader.Model.Enums;

namespace TR2RandomizerCore.Utilities
{
    public static class ItemUtilities
    {
        public static TRItems ConvertToScriptItem(TR2Entities entity, TRItems defaultItem = TRItems.Pistols)
        {
            if (_entityToScriptItemMap.ContainsKey(entity))
            {
                return _entityToScriptItemMap[entity];
            }
            return defaultItem;
        }

        public static TR2Entities ConvertToEntity(TRItems item, TR2Entities defaultEntity = TR2Entities.Pistols_S_P)
        {
            if (_scriptItemToEntitymap.ContainsKey(item))
            {
                return _scriptItemToEntitymap[item];
            }
            return defaultEntity;
        }

        private static readonly Dictionary<TR2Entities, TRItems> _entityToScriptItemMap = new Dictionary<TR2Entities, TRItems>
        {
            [TR2Entities.Pistols_S_P]
                = TRItems.Pistols,
            [TR2Entities.PistolAmmo_S_P]
                = TRItems.PistolClips,

            [TR2Entities.Shotgun_S_P]
                = TRItems.Shotgun,
            [TR2Entities.ShotgunAmmo_S_P]
                = TRItems.ShotgunShells,

            [TR2Entities.Automags_S_P]
                = TRItems.AutoPistols,
            [TR2Entities.AutoAmmo_S_P]
                = TRItems.AutoClips,

            [TR2Entities.Uzi_S_P]
                = TRItems.Uzis,
            [TR2Entities.UziAmmo_S_P]
                = TRItems.UziClips,

            [TR2Entities.Harpoon_S_P]
                = TRItems.HarpoonGun,
            [TR2Entities.HarpoonAmmo_S_P]
                = TRItems.Harpoons,

            [TR2Entities.M16_S_P]
                = TRItems.M16,
            [TR2Entities.M16Ammo_S_P]
                = TRItems.M16Clips,

            [TR2Entities.GrenadeLauncher_S_P]
                = TRItems.GrenadeLauncher,
            [TR2Entities.Grenades_S_P]
                = TRItems.Grenades,

            [TR2Entities.SmallMed_S_P]
                = TRItems.SmallMedi,
            [TR2Entities.LargeMed_S_P]
                = TRItems.LargeMedi,

            [TR2Entities.Flares_S_P]
                = TRItems.Flare, // Single flare, not a pack

            [TR2Entities.Quest1_S_P]
                = TRItems.Pickup1,
            [TR2Entities.Quest2_S_P]
                 = TRItems.Pickup2,

            [TR2Entities.Puzzle1_S_P]
                = TRItems.Puzzle1,
            [TR2Entities.Puzzle2_S_P]
                = TRItems.Puzzle2,
            [TR2Entities.Puzzle3_S_P]
                = TRItems.Puzzle3,
            [TR2Entities.Puzzle4_S_P]
                = TRItems.Puzzle4,

            [TR2Entities.Key1_S_P]
                = TRItems.Key1,
            [TR2Entities.Key2_S_P]
                = TRItems.Key2,
            [TR2Entities.Key3_S_P]
                = TRItems.Key3,
            [TR2Entities.Key4_S_P]
                = TRItems.Key4,
        };

        // The reverse of above for the sake of ConvertToEntity, but initialised dynamically.
        private static readonly Dictionary<TRItems, TR2Entities> _scriptItemToEntitymap;

        static ItemUtilities()
        {
            _scriptItemToEntitymap = _entityToScriptItemMap.ToDictionary(e => e.Value, e => e.Key);
        }
    }
}