using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Globalisation;

namespace TRRandomizerCore.Randomizers;

public class TR1RGameStringRandomizer : BaseTR1RRandomizer
{
    public override void Randomize(int seed)
    {
        _generator = new(seed);
        GameStringAllocator allocator = new()
        {
            Settings = Settings,
            Generator = _generator,
        };

        Dictionary<TRStringKey, string> globalStrings = allocator.Allocate(TRGameVersion.TR1, ScriptEditor);

        TRRScript script = ScriptEditor.Script as TRRScript;
        GameStringAllocator.ApplyTRRGlobalStrings(script, globalStrings, _gameMap);
        allocator.ApplyTRRLevelStrings(script, _keyItemMap);

        SaveScript();
        TriggerProgress();
    }

    private static readonly Dictionary<TRStringKey, string> _gameMap = new()
    {
        [TRStringKey.INV_ITEM_COMPASS] = "COMPASS",
        [TRStringKey.INV_ITEM_PISTOLS] = "PISTOLS",
        [TRStringKey.INV_ITEM_SHOTGUN] = "SHOTGUN",
        [TRStringKey.INV_ITEM_MAGNUM] = "MAGNUM",
        [TRStringKey.INV_ITEM_UZI] = "UZIS",
        [TRStringKey.INV_ITEM_SHOTGUN_AMMO] = "SHOTGUNAMMO",
        [TRStringKey.INV_ITEM_MAGNUM_AMMO] = "MAGNUMAMMO",
        [TRStringKey.INV_ITEM_UZI_AMMO] = "UZIAMMO",
        [TRStringKey.INV_ITEM_LEADBAR] = "LEADBAR",
        [TRStringKey.INV_ITEM_SCION] = "SCION",
    };

    private static readonly Dictionary<string, Dictionary<TRKeyItemKey, string>> _keyItemMap = new()
    {
        [TR1LevelNames.VILCABAMBA] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_SILVER_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_GOLD_IDOL",
        },
        [TR1LevelNames.VALLEY] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_MACHINE_COG",
        },
        [TR1LevelNames.FOLLY] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_NEPTUNE_KEY",
            [TRKeyItemKey.Key2] = "KEY_ATLAS_KEY",
            [TRKeyItemKey.Key3] = "KEY_DAMOCLES_KEY",
            [TRKeyItemKey.Key4] = "KEY_THOR_KEY",
        },
        [TR1LevelNames.COLOSSEUM] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_RUSTY_KEY",
        },
        [TR1LevelNames.MIDAS] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_GOLD_BAR",
        },
        [TR1LevelNames.CISTERN] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GOLD_KEY",
            [TRKeyItemKey.Key2] = "KEY_SILVER_KEY",
            [TRKeyItemKey.Key3] = "KEY_RUSTY_KEY",
        },
        [TR1LevelNames.TIHOCAN] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GOLD_KEY",
            [TRKeyItemKey.Key2] = "KEY_RUSTY_KEY",
            [TRKeyItemKey.Key3] = "KEY_RUSTY_KEY",
        },
        [TR1LevelNames.KHAMOON] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_SAPHIRE_KEY",
        },
        [TR1LevelNames.OBELISK] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_SAPHIRE_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_EYE_OF_HORUS",
            [TRKeyItemKey.Puzzle2] = "PUZ_SCARAB",
            [TRKeyItemKey.Puzzle3] = "PUZ_SEAL_OF_ANUBIS",
            [TRKeyItemKey.Puzzle4] = "PUZ_ANKH",
        },
        [TR1LevelNames.SANCTUARY] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GOLD_KEY",
            [TRKeyItemKey.Puzzle1] = "PUZ_ANKH",
            [TRKeyItemKey.Puzzle2] = "PUZ_SCARAB",
        },
        [TR1LevelNames.MINES] = new()
        {
            [TRKeyItemKey.Puzzle1] = "PUZ_FUSE",
            [TRKeyItemKey.Puzzle2] = "PUZ_PYRAMID_KEY",
        },
        [TR1LevelNames.EGYPT] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_GOLD_KEY",
        },
        [TR1LevelNames.CAT] = new()
        {
            [TRKeyItemKey.Key1] = "KEY_ORNATE_KEY",
        },
    };
}
