using TRGE.Core.Item.Enums;
using TRLevelControl.Model;

namespace TRRandomizerCore.Utilities;

public static class ItemUtilities
{
    public static TR1Items ConvertToScriptItem(TR1Type entity)
    {
        return (TR1Items)entity;
    }

    public static TR1Type ConvertToEntity(TR1Items item)
    {
        return (TR1Type)item;
    }

    public static TR2Items ConvertToScriptItem(TR2Type entity, TR2Items defaultItem = TR2Items.Pistols)
    {
        return Convert(entity, defaultItem, _tr2EntityToScriptItemMap);
    }

    public static TR2Type ConvertToEntity(TR2Items item, TR2Type defaultEntity = TR2Type.Pistols_S_P)
    {
        return Convert(item, defaultEntity, _tr2ScriptItemToEntitymap);
    }

    public static TR3Items ConvertToScriptItem(TR3Entities entity, TR3Items defaultItem = TR3Items.Pistols)
    {
        return Convert(entity, defaultItem, _tr3EntityToScriptItemMap);
    }

    public static TR3Entities ConvertToEntity(TR3Items item, TR3Entities defaultEntity = TR3Entities.Pistols_P)
    {
        return Convert(item, defaultEntity, _tr3ScriptItemToEntitymap);
    }

    private static T Convert<T, S>(S item, T defaultItem, Dictionary<S, T> dictionary)
    {
        if (dictionary.ContainsKey(item))
        {
            return dictionary[item];
        }
        return defaultItem;
    }

    public static void HideEntity(TREntity entity)
    {
        // Move the item down, under the floor or into the ceiling of whatever is below
        entity.Y += 128;
        // Marking it invisible means it cannot be picked up, even if the new location can be reached.
        entity.Invisible = true;
    }

    public static void HideEntity(TR2Entity entity)
    {
        // Move the item down, under the floor or into the ceiling of whatever is below
        entity.Y += 128;
        // Marking it invisible means it cannot be picked up, even if the new location can be reached.
        entity.Invisible = true;
    }

    private static readonly Dictionary<TR2Type, TR2Items> _tr2EntityToScriptItemMap = new()
    {
        [TR2Type.Pistols_S_P]
            = TR2Items.Pistols,
        [TR2Type.PistolAmmo_S_P]
            = TR2Items.PistolClips,

        [TR2Type.Shotgun_S_P]
            = TR2Items.Shotgun,
        [TR2Type.ShotgunAmmo_S_P]
            = TR2Items.ShotgunShells,

        [TR2Type.Automags_S_P]
            = TR2Items.AutoPistols,
        [TR2Type.AutoAmmo_S_P]
            = TR2Items.AutoClips,

        [TR2Type.Uzi_S_P]
            = TR2Items.Uzis,
        [TR2Type.UziAmmo_S_P]
            = TR2Items.UziClips,

        [TR2Type.Harpoon_S_P]
            = TR2Items.HarpoonGun,
        [TR2Type.HarpoonAmmo_S_P]
            = TR2Items.Harpoons,

        [TR2Type.M16_S_P]
            = TR2Items.M16,
        [TR2Type.M16Ammo_S_P]
            = TR2Items.M16Clips,

        [TR2Type.GrenadeLauncher_S_P]
            = TR2Items.GrenadeLauncher,
        [TR2Type.Grenades_S_P]
            = TR2Items.Grenades,

        [TR2Type.SmallMed_S_P]
            = TR2Items.SmallMedi,
        [TR2Type.LargeMed_S_P]
            = TR2Items.LargeMedi,

        [TR2Type.Flares_S_P]
            = TR2Items.Flare, // Single flare, not a pack

        [TR2Type.Quest1_S_P]
            = TR2Items.Pickup1,
        [TR2Type.Quest2_S_P]
             = TR2Items.Pickup2,

        [TR2Type.Puzzle1_S_P]
            = TR2Items.Puzzle1,
        [TR2Type.Puzzle2_S_P]
            = TR2Items.Puzzle2,
        [TR2Type.Puzzle3_S_P]
            = TR2Items.Puzzle3,
        [TR2Type.Puzzle4_S_P]
            = TR2Items.Puzzle4,

        [TR2Type.Key1_S_P]
            = TR2Items.Key1,
        [TR2Type.Key2_S_P]
            = TR2Items.Key2,
        [TR2Type.Key3_S_P]
            = TR2Items.Key3,
        [TR2Type.Key4_S_P]
            = TR2Items.Key4
    };

    private static readonly Dictionary<TR3Entities, TR3Items> _tr3EntityToScriptItemMap = new()
    {
        [TR3Entities.Pistols_P]
            = TR3Items.Pistols,
        [TR3Entities.PistolAmmo_P]
            = TR3Items.PistolClips,

        [TR3Entities.Shotgun_P]
            = TR3Items.Shotgun,
        [TR3Entities.ShotgunAmmo_P]
            = TR3Items.ShotgunShells,

        [TR3Entities.Deagle_P]
            = TR3Items.DEagle,
        [TR3Entities.DeagleAmmo_P]
            = TR3Items.DEagleClips,

        [TR3Entities.Uzis_P]
            = TR3Items.Uzis,
        [TR3Entities.UziAmmo_P]
            = TR3Items.UziClips,

        [TR3Entities.Harpoon_P]
            = TR3Items.HarpoonGun,
        [TR3Entities.Harpoons_P]
            = TR3Items.Harpoons,

        [TR3Entities.MP5_P]
            = TR3Items.MP5,
        [TR3Entities.MP5Ammo_P]
            = TR3Items.MP5Clips,

        [TR3Entities.RocketLauncher_P]
            = TR3Items.RocketLauncher,
        [TR3Entities.Rockets_P]
            = TR3Items.Rocket,

        [TR3Entities.GrenadeLauncher_P]
            = TR3Items.GrenadeLauncher,
        [TR3Entities.Grenades_P]
            = TR3Items.Grenades,

        [TR3Entities.SmallMed_P]
            = TR3Items.SmallMedi,
        [TR3Entities.LargeMed_P]
            = TR3Items.LargeMedi,

        [TR3Entities.Flares_P]
            = TR3Items.Flare, // Single flare, not a pack

        [TR3Entities.Quest1_P]
            = TR3Items.Pickup1,
        [TR3Entities.Quest2_P]
             = TR3Items.Pickup2,

        [TR3Entities.Puzzle1_P]
            = TR3Items.Puzzle1,
        [TR3Entities.Puzzle2_P]
            = TR3Items.Puzzle2,
        [TR3Entities.Puzzle3_P]
            = TR3Items.Puzzle3,
        [TR3Entities.Puzzle4_P]
            = TR3Items.Puzzle4,

        [TR3Entities.Key1_P]
            = TR3Items.Key1,
        [TR3Entities.Key2_P]
            = TR3Items.Key2,
        [TR3Entities.Key3_P]
            = TR3Items.Key3,
        [TR3Entities.Key4_P]
            = TR3Items.Key4
    };

    // The reverse of above for the sake of ConvertToEntity, but initialised dynamically.
    private static readonly Dictionary<TR2Items, TR2Type> _tr2ScriptItemToEntitymap;
    private static readonly Dictionary<TR3Items, TR3Entities> _tr3ScriptItemToEntitymap;

    static ItemUtilities()
    {
        _tr2ScriptItemToEntitymap = _tr2EntityToScriptItemMap.ToDictionary(e => e.Value, e => e.Key);
        _tr3ScriptItemToEntitymap = _tr3EntityToScriptItemMap.ToDictionary(e => e.Value, e => e.Key);
    }
}
