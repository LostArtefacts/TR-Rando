using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Globalisation;

namespace TRRandomizerCore.Randomizers;

public class TR2RGameStringRandomizer : BaseTR2RRandomizer
{
    public override void Randomize(int seed)
    {
        _generator = new(seed);
        GameStringAllocator allocator = new()
        {
            Settings = Settings,
            Generator = _generator,
        };

        Dictionary<TRStringKey, string> globalStrings = allocator.Allocate(TRGameVersion.TR2, ScriptEditor);

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
        [TRStringKey.INV_ITEM_AUTOS] = "AUTOS",
        [TRStringKey.INV_ITEM_UZI] = "UZIS",
        [TRStringKey.INV_ITEM_HARPOON] = "HARPOON",
        [TRStringKey.INV_ITEM_M16] = "M16",
        [TRStringKey.INV_ITEM_GRENADE_LAUNCHER] = "GRENADE",
        [TRStringKey.INV_ITEM_PISTOL_AMMO] = "PISTOLAMMO",
        [TRStringKey.INV_ITEM_SHOTGUN_AMMO] = "SHOTGUNAMMO",
        [TRStringKey.INV_ITEM_AUTO_AMMO] = "AUTOSAMMO",
        [TRStringKey.INV_ITEM_UZI_AMMO] = "UZIAMMO",
        [TRStringKey.INV_ITEM_HARPOON_AMMO] = "HARPOONAMMO",
        [TRStringKey.INV_ITEM_M16_AMMO] = "M16AMMO",
        [TRStringKey.INV_ITEM_GRENADE_AMMO] = "GRENADEAMMO",
    };

    private static readonly Dictionary<string, Dictionary<TRKeyItemKey, string>> _keyItemMap = new()
    {
        [TR2LevelNames.GW] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GUARDHOUSE_KEY",
            [TRKeyItemKey.Key2] = "KEY_RUSTY_KEY",
        },
        [TR2LevelNames.VENICE] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_BOATHOUSE_KEY",
            [TRKeyItemKey.Key2] = "KEY_STEEL_KEY",
            [TRKeyItemKey.Key3] = "KEY_IRON_KEY",
        },
        [TR2LevelNames.BARTOLI] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_LIBRARY_KEY",
            [TRKeyItemKey.Key2] = "KEY_DETONATOR_KEY",
        },
        [TR2LevelNames.OPERA] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_ORNATE_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_RELAY_BOX",
            [TRKeyItemKey.Puzzle2] = "PUZ_CIRCUIT_BOARD",
        },
        [TR2LevelNames.RIG] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_RED_PASS_CARD",
            [TRKeyItemKey.Key2] = "KEY_YELLOW_PASS_CARD",
            [TRKeyItemKey.Key3] = "KEY_GREEN_PASS_CARD",
        },
        [TR2LevelNames.DA] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_RED_PASS_CARD",
            [TRKeyItemKey.Key4] = "KEY_BLUE_PASS_CARD",
            [TRKeyItemKey.Puzzle1] = "PUZ_MACHINE_CHIP",
        },
        [TR2LevelNames.DORIA] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_REST_ROOM_KEY",
            [TRKeyItemKey.Key2] = "KEY_RUSTY_KEY",
            [TRKeyItemKey.Key3] = "KEY_CABIN_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_CIRCUIT_BREAKER",
        },
        [TR2LevelNames.LQ] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_THEATRE_KEY",
        },
        [TR2LevelNames.DECK] = new()
        {
            [TRKeyItemKey.Key2] = "KEY_STERN_KEY",
            [TRKeyItemKey.Key3] = "KEY_STORAGE_KEY",
            [TRKeyItemKey.Key4] = "KEY_CABIN_KEY",
            [TRKeyItemKey.Puzzle4] = "PUZ_THE_SERAPH",
        },
        [TR2LevelNames.TIBET] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_DRAWBRIDGE_KEY",
            [TRKeyItemKey.Key2] = "KEY_HUT_KEY",
            [TRKeyItemKey.Puzzle4] = "PUZ_THE_SERAPH",
        },
        [TR2LevelNames.MONASTERY] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_STRONGROOM_KEY",
            [TRKeyItemKey.Key2] = "KEY_TRAPDOOR_KEY",
            [TRKeyItemKey.Key3] = "KEY_ROOFTOPS_KEY",
            [TRKeyItemKey.Key4] = "KEY_MAIN_HALL_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_PRAYER_WHEELS",
            [TRKeyItemKey.Puzzle2] = "PUZ_GEMSTONES",
            [TRKeyItemKey.Puzzle4] = "PUZ_THE_SERAPH",
        },
        [TR2LevelNames.COT] = new()
        {
            [TRKeyItemKey.Pickup1] = "KEY_GONG_HAMMER",
            [TRKeyItemKey.Puzzle1] = "PUZ_TIBETAN_MASK",
        },
        [TR2LevelNames.CHICKEN] = new()
        {
            [TRKeyItemKey.Key2] = "KEY_GONG_HAMMER",
            [TRKeyItemKey.Pickup2] = "PUZ_TALION",
            [TRKeyItemKey.Puzzle1] = "PUZ_TIBETAN_MASK",
        },
        [TR2LevelNames.XIAN] = new()
        {
            [TRKeyItemKey.Key2] = "KEY_GOLD_KEY",
            [TRKeyItemKey.Key3] = "KEY_SILVER_KEY",
            [TRKeyItemKey.Key4] = "KEY_MAIN_CHAMBER_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_THE_DRAGON_SEAL",
        },
        [TR2LevelNames.FLOATER] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_MYSTIC_PLAQUE",
            [TRKeyItemKey.Puzzle2] = "PUZ_MYSTIC_PLAQUE",
        },
        [TR2LevelNames.LAIR] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_MYSTIC_PLAQUE",
            [TRKeyItemKey.Puzzle2] = "PUZ_DAGGER_OF_XIAN",
        },
        [TR2LevelNames.HOME] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GUN_CUPBOARD_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_DAGGER_OF_XIAN",
        },
        [TR2LevelNames.COLDWAR] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GUARDROOM_KEY",
            [TRKeyItemKey.Key2] = "KEY_SHAFT_B_KEY",
        },
        [TR2LevelNames.FOOLGOLD] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_CARDKEY_1",
            [TRKeyItemKey.Key4] = "KEY_CARDKEY_2",
            [TRKeyItemKey.Puzzle1] = "PUZ_CIRCUIT_BOARD",
        },
        [TR2LevelNames.FURNACE] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_MASK_OF_TORNARSUK",
            [TRKeyItemKey.Puzzle2] = "PUZ_GOLD_NUGGET",
        },
        [TR2LevelNames.VEGAS] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_HOTEL_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_ELEVATOR_JUNCTION",
            [TRKeyItemKey.Puzzle2] = "PUZ_DOOR_CIRCUIT",
        },
    };
}
