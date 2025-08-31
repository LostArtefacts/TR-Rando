using TRGE.Core.Item.Enums;
using TRLevelControl.Model;

namespace TRRandomizerCore.Utilities;

public static class ItemUtilities
{
    public static TR3Items ConvertToScriptItem(TR3Type entity, TR3Items defaultItem = TR3Items.Pistols)
    {
        return Convert(entity, defaultItem, _tr3EntityToScriptItemMap);
    }

    public static TR3Type ConvertToEntity(TR3Items item, TR3Type defaultEntity = TR3Type.Pistols_P)
    {
        return Convert(item, defaultEntity, _tr3ScriptItemToEntitymap);
    }

    private static T Convert<T, S>(S item, T defaultItem, Dictionary<S, T> dictionary)
    {
        return dictionary.TryGetValue(item, out T value) ? value : defaultItem;
    }

    public static void HideEntity<T>(TREntity<T> entity)
        where T : Enum
    {
        // Move the item down, under the floor or into the ceiling of whatever is below
        entity.Y += 128;
        // Marking it invisible means it cannot be picked up, even if the new location can be reached.
        entity.Invisible = true;
    }

    private static readonly Dictionary<TR3Type, TR3Items> _tr3EntityToScriptItemMap = new()
    {
        [TR3Type.Pistols_P]
            = TR3Items.Pistols,
        [TR3Type.PistolAmmo_P]
            = TR3Items.PistolClips,

        [TR3Type.Shotgun_P]
            = TR3Items.Shotgun,
        [TR3Type.ShotgunAmmo_P]
            = TR3Items.ShotgunShells,

        [TR3Type.Deagle_P]
            = TR3Items.DEagle,
        [TR3Type.DeagleAmmo_P]
            = TR3Items.DEagleClips,

        [TR3Type.Uzis_P]
            = TR3Items.Uzis,
        [TR3Type.UziAmmo_P]
            = TR3Items.UziClips,

        [TR3Type.Harpoon_P]
            = TR3Items.HarpoonGun,
        [TR3Type.Harpoons_P]
            = TR3Items.Harpoons,

        [TR3Type.MP5_P]
            = TR3Items.MP5,
        [TR3Type.MP5Ammo_P]
            = TR3Items.MP5Clips,

        [TR3Type.RocketLauncher_P]
            = TR3Items.RocketLauncher,
        [TR3Type.Rockets_P]
            = TR3Items.Rocket,

        [TR3Type.GrenadeLauncher_P]
            = TR3Items.GrenadeLauncher,
        [TR3Type.Grenades_P]
            = TR3Items.Grenades,

        [TR3Type.SmallMed_P]
            = TR3Items.SmallMedi,
        [TR3Type.LargeMed_P]
            = TR3Items.LargeMedi,

        [TR3Type.Flares_P]
            = TR3Items.Flare, // Single flare, not a pack

        [TR3Type.Quest1_P]
            = TR3Items.Pickup1,
        [TR3Type.Quest2_P]
             = TR3Items.Pickup2,

        [TR3Type.Puzzle1_P]
            = TR3Items.Puzzle1,
        [TR3Type.Puzzle2_P]
            = TR3Items.Puzzle2,
        [TR3Type.Puzzle3_P]
            = TR3Items.Puzzle3,
        [TR3Type.Puzzle4_P]
            = TR3Items.Puzzle4,

        [TR3Type.Key1_P]
            = TR3Items.Key1,
        [TR3Type.Key2_P]
            = TR3Items.Key2,
        [TR3Type.Key3_P]
            = TR3Items.Key3,
        [TR3Type.Key4_P]
            = TR3Items.Key4
    };

    // The reverse of above for the sake of ConvertToEntity, but initialised dynamically.
    private static readonly Dictionary<TR3Items, TR3Type> _tr3ScriptItemToEntitymap;

    static ItemUtilities()
    {
        _tr3ScriptItemToEntitymap = _tr3EntityToScriptItemMap.ToDictionary(e => e.Value, e => e.Key);
    }
}
