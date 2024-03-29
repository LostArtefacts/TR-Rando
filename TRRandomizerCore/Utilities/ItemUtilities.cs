﻿using TRGE.Core.Item.Enums;
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
        if (dictionary.ContainsKey(item))
        {
            return dictionary[item];
        }
        return defaultItem;
    }

    public static void HideEntity<T>(TREntity<T> entity)
        where T : Enum
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
    private static readonly Dictionary<TR2Items, TR2Type> _tr2ScriptItemToEntitymap;
    private static readonly Dictionary<TR3Items, TR3Type> _tr3ScriptItemToEntitymap;

    static ItemUtilities()
    {
        _tr2ScriptItemToEntitymap = _tr2EntityToScriptItemMap.ToDictionary(e => e.Value, e => e.Key);
        _tr3ScriptItemToEntitymap = _tr3EntityToScriptItemMap.ToDictionary(e => e.Value, e => e.Key);
    }
}
