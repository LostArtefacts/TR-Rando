using System.Collections.Generic;
using System.Linq;
using TRGE.Core.Item.Enums;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRRandomizerCore.Utilities
{
    public static class ItemUtilities
    {
        public static TR2Items ConvertToScriptItem(TR2Entities entity, TR2Items defaultItem = TR2Items.Pistols)
        {
            if (_entityToScriptItemMap.ContainsKey(entity))
            {
                return _entityToScriptItemMap[entity];
            }
            return defaultItem;
        }

        public static TR2Entities ConvertToEntity(TR2Items item, TR2Entities defaultEntity = TR2Entities.Pistols_S_P)
        {
            if (_scriptItemToEntitymap.ContainsKey(item))
            {
                return _scriptItemToEntitymap[item];
            }
            return defaultEntity;
        }

        public static void HideEntity(TR2Entity entity)
        {
            // Move the item down, under the floor or into the ceiling of whatever is below
            entity.Y += 128;
            // Marking it invisible means it cannot be picked up, even if the new location can be reached.
            entity.Invisible = true;
        }

        private static readonly Dictionary<TR2Entities, TR2Items> _entityToScriptItemMap = new Dictionary<TR2Entities, TR2Items>
        {
            [TR2Entities.Pistols_S_P]
                = TR2Items.Pistols,
            [TR2Entities.PistolAmmo_S_P]
                = TR2Items.PistolClips,

            [TR2Entities.Shotgun_S_P]
                = TR2Items.Shotgun,
            [TR2Entities.ShotgunAmmo_S_P]
                = TR2Items.ShotgunShells,

            [TR2Entities.Automags_S_P]
                = TR2Items.AutoPistols,
            [TR2Entities.AutoAmmo_S_P]
                = TR2Items.AutoClips,

            [TR2Entities.Uzi_S_P]
                = TR2Items.Uzis,
            [TR2Entities.UziAmmo_S_P]
                = TR2Items.UziClips,

            [TR2Entities.Harpoon_S_P]
                = TR2Items.HarpoonGun,
            [TR2Entities.HarpoonAmmo_S_P]
                = TR2Items.Harpoons,

            [TR2Entities.M16_S_P]
                = TR2Items.M16,
            [TR2Entities.M16Ammo_S_P]
                = TR2Items.M16Clips,

            [TR2Entities.GrenadeLauncher_S_P]
                = TR2Items.GrenadeLauncher,
            [TR2Entities.Grenades_S_P]
                = TR2Items.Grenades,

            [TR2Entities.SmallMed_S_P]
                = TR2Items.SmallMedi,
            [TR2Entities.LargeMed_S_P]
                = TR2Items.LargeMedi,

            [TR2Entities.Flares_S_P]
                = TR2Items.Flare, // Single flare, not a pack

            [TR2Entities.Quest1_S_P]
                = TR2Items.Pickup1,
            [TR2Entities.Quest2_S_P]
                 = TR2Items.Pickup2,

            [TR2Entities.Puzzle1_S_P]
                = TR2Items.Puzzle1,
            [TR2Entities.Puzzle2_S_P]
                = TR2Items.Puzzle2,
            [TR2Entities.Puzzle3_S_P]
                = TR2Items.Puzzle3,
            [TR2Entities.Puzzle4_S_P]
                = TR2Items.Puzzle4,

            [TR2Entities.Key1_S_P]
                = TR2Items.Key1,
            [TR2Entities.Key2_S_P]
                = TR2Items.Key2,
            [TR2Entities.Key3_S_P]
                = TR2Items.Key3,
            [TR2Entities.Key4_S_P]
                = TR2Items.Key4,
        };

        // The reverse of above for the sake of ConvertToEntity, but initialised dynamically.
        private static readonly Dictionary<TR2Items, TR2Entities> _scriptItemToEntitymap;

        static ItemUtilities()
        {
            _scriptItemToEntitymap = _entityToScriptItemMap.ToDictionary(e => e.Value, e => e.Key);
        }
    }
}