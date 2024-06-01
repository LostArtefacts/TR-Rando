using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Globalisation;

namespace TRRandomizerCore.Randomizers;

public class TR3RGameStringRandomizer : BaseTR3RRandomizer
{
    public override void Randomize(int seed)
    {
        _generator = new(seed);
        GameStringAllocator allocator = new()
        {
            Settings = Settings,
            Generator = _generator,
        };

        Dictionary<TRStringKey, string> globalStrings = allocator.Allocate(TRGameVersion.TR3, ScriptEditor);

        TRRScript script = ScriptEditor.Script as TRRScript;
        GameStringAllocator.ApplyTRRGlobalStrings(script, globalStrings, _gameMap);
        allocator.ApplyTRRLevelStrings(script, _keyItemMap);

        SaveScript();
        TriggerProgress();
    }

    private static readonly Dictionary<TRStringKey, string> _gameMap = new()
    {
        [TRStringKey.INV_ITEM_COMPASS] = "STATS",
        [TRStringKey.INV_ITEM_FLARES] = "FLARE",
        [TRStringKey.INV_ITEM_PISTOLS] = "PISTOLS",
        [TRStringKey.INV_ITEM_SHOTGUN] = "SHOTGUN",
        [TRStringKey.INV_ITEM_DEAGLE] = "DEAGLE",
        [TRStringKey.INV_ITEM_UZI] = "UZIS",
        [TRStringKey.INV_ITEM_HARPOON] = "HARPOON",
        [TRStringKey.INV_ITEM_M16] = "MP5",
        [TRStringKey.INV_ITEM_ROCKET_LAUNCHER] = "ROCKET",
        [TRStringKey.INV_ITEM_GRENADE_LAUNCHER] = "GRENADE",
        [TRStringKey.INV_ITEM_PISTOL_AMMO] = "PISTOLAMMO",
        [TRStringKey.INV_ITEM_SHOTGUN_AMMO] = "SHOTGUNAMMO",
        [TRStringKey.INV_ITEM_DEAGLE_AMMO] = "DEAGLEAMMO",
        [TRStringKey.INV_ITEM_UZI_AMMO] = "UZIAMMO",
        [TRStringKey.INV_ITEM_HARPOON_AMMO] = "HARPOONAMMO",
        [TRStringKey.INV_ITEM_M16_AMMO] = "MP5AMMO",
        [TRStringKey.INV_ITEM_ROCKETS] = "ROCKETAMMO",
        [TRStringKey.INV_ITEM_GRENADE_AMMO] = "GRENADEAMMO",
        [TRStringKey.INV_GLOBE_LONDON] = "LOC_LONDON",
        [TRStringKey.INV_GLOBE_NEVADA] = "LOC_NEVADA",
        [TRStringKey.INV_GLOBE_SOUTH_PACIFIC] = "LOC_SOUTHPAC",
        [TRStringKey.INV_GLOBE_ANTARCTICA] = "LOC_ANTARC",
    };

    private static readonly Dictionary<string, Dictionary<TRKeyItemKey, string>> _keyItemMap = new()
    {
        [TR3LevelNames.ASSAULT] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_RACETRACK_KEY",
        },
        [TR3LevelNames.JUNGLE] = new()
        {
            [TRKeyItemKey.Key4] = "KEY_INDRA_KEY",
        },
        [TR3LevelNames.RUINS] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_KEY_OF_GANESHA",
            [TRKeyItemKey.Puzzle1] = "PUZ_SCIMITAR",
            [TRKeyItemKey.Puzzle2] = "PUZ_SCIMITAR",
        },
        [TR3LevelNames.GANGES] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GATE_KEY",
        },
        [TR3LevelNames.COASTAL] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_SMUGGLERS_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_SERPENT_STONE",
        },
        [TR3LevelNames.CRASH] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_COMMANDER_BISHOPS_KEY",
            [TRKeyItemKey.Key2] = "KEY_LT_TUCKERMANS_KEY",
            [TRKeyItemKey.Pickup1] = "PUP_SWAMP_MAP",
        },
        [TR3LevelNames.THAMES] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_FLUE_ROOM_KEY",
            [TRKeyItemKey.Key2] = "KEY_CATHEDRAL_KEY",
        },
        [TR3LevelNames.ALDWYCH] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_MAINTENANCE_KEY",
            [TRKeyItemKey.Key2] = "KEY_SOLOMONS_KEY",
            [TRKeyItemKey.Key3] = "KEY_SOLOMONS_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_OLD_PENNY",
            [TRKeyItemKey.Puzzle2] = "PUZ_TICKET",
            [TRKeyItemKey.Puzzle3] = "PUZ_MASONIC_MALLET",
            [TRKeyItemKey.Puzzle4] = "PUZ_ORNATE_STAR",
        },
        [TR3LevelNames.LUDS] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_BOILER_ROOM_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_EMBALMING_FLUID",
        },
        [TR3LevelNames.NEVADA] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GENERATOR_ACCESS",
            [TRKeyItemKey.Key2] = "KEY_DETONATOR_SWITCH",
        },
        [TR3LevelNames.HSC] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_KEYCARD_TYPE_A",
            [TRKeyItemKey.Key2] = "KEY_KEYCARD_TYPE_B",
            [TRKeyItemKey.Puzzle1] = "PUZ_BLUE_SECURITY_PASS",
            [TRKeyItemKey.Puzzle2] = "PUZ_YELLOW_SECURITY_PASS",
        },
        [TR3LevelNames.AREA51] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_LAUNCH_CODE_PASS",
            [TRKeyItemKey.Puzzle2] = "PUZ_TOWER_ACCESS_KEY",
            [TRKeyItemKey.Puzzle3] = "PUZ_CODE_CLEARANCE_DISK",
            [TRKeyItemKey.Puzzle4] = "PUZ_YELLOW_SECURITY_PASS",
        },
        [TR3LevelNames.ANTARC] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_HUT_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_CROWBAR",
            [TRKeyItemKey.Puzzle2] = "PUZ_GATE_CONTROL_KEY",
        },
        [TR3LevelNames.RXTECH] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_CROWBAR",
            [TRKeyItemKey.Puzzle2] = "PUZ_LEAD_ACID_BATTERY",
            [TRKeyItemKey.Puzzle3] = "PUZ_WINCH_STARTER",
        },
        [TR3LevelNames.TINNOS] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_ULI_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_OCEANIC_MASK",
        },
        [TR3LevelNames.HALLOWS] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_VAULT_KEY",
        },
        [TR3LevelNames.FLING] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_CROWBAR",
            [TRKeyItemKey.Puzzle2] = "PUZ_THISTLE_STONE",
        },
        [TR3LevelNames.LAIR] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_CAIRN_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_CROWBAR",
        },
        [TR3LevelNames.CLIFF] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_DRILL_ACTIVATOR_CARD",
            [TRKeyItemKey.Puzzle1] = "PUZ_PUMP_ACCESS_DISK",
        },
        [TR3LevelNames.FISHES] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_CIRCUIT_BULB",
            [TRKeyItemKey.Puzzle2] = "PUZ_MUTANT_SAMPLE",
            [TRKeyItemKey.Puzzle3] = "PUZ_MUTANT_SAMPLE",
            [TRKeyItemKey.Puzzle4] = "PUZ_CIRCUIT_BULB",
        },
        [TR3LevelNames.MADHOUSE] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_ZOO_KEY",
            [TRKeyItemKey.Key4] = "KEY_AVIARY_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_THE_HAND_OF_RATHMORE",
        },
        [TR3LevelNames.REUNION] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_THE_HAND_OF_RATHMORE",
        },
    };
}
