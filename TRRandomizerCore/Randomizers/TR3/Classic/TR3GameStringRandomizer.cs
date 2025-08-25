using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Globalisation;

namespace TRRandomizerCore.Randomizers;

public class TR3GameStringRandomizer : BaseTR3Randomizer
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
        ConvertGlobalStrings(globalStrings);

        SaveScript();
        TriggerProgress();
    }

    private void ConvertGlobalStrings(Dictionary<TRStringKey, string> globalStrings)
    {
        var script = ScriptEditor.Script as TR3Script;
        List<string> gameStrings1 = new(script.GameStrings1);
        List<string> gameStrings2 = new(script.GameStrings2);

        foreach (var (key, value) in globalStrings)
        {
            if (_gameString1Map.ContainsKey(key))
            {
                gameStrings1[_gameString1Map[key]] = value;
            }
            else if (_gameString2Map.ContainsKey(key))
            {
                gameStrings2[_gameString2Map[key]] = value;
            }
        }

        script.GameStrings1 = gameStrings1.ToArray();
        script.GameStrings2 = gameStrings2.ToArray();
    }

    private static readonly Dictionary<TRStringKey, int> _gameString1Map = new()
    {
        [TRStringKey.HEADING_ITEMS] = 2,
        [TRStringKey.HEADING_GAME_OVER] = 3,

        [TRStringKey.INV_ITEM_PISTOLS] = 36,
        [TRStringKey.INV_ITEM_PISTOL_AMMO] = 45,
        [TRStringKey.INV_ITEM_SHOTGUN] = 37,
        [TRStringKey.INV_ITEM_SHOTGUN_AMMO] = 46,
        [TRStringKey.INV_ITEM_DEAGLE] = 38,
        [TRStringKey.INV_ITEM_DEAGLE_AMMO] = 47,
        [TRStringKey.INV_ITEM_UZI] = 39,
        [TRStringKey.INV_ITEM_UZI_AMMO] = 48,
        [TRStringKey.INV_ITEM_HARPOON] = 40,
        [TRStringKey.INV_ITEM_HARPOON_AMMO] = 49,
        [TRStringKey.INV_ITEM_M16] = 41,
        [TRStringKey.INV_ITEM_M16_AMMO] = 50,
        [TRStringKey.INV_ITEM_ROCKET_LAUNCHER] = 42,
        [TRStringKey.INV_ITEM_ROCKETS] = 51,
        [TRStringKey.INV_ITEM_GRENADE_LAUNCHER] = 43,
        [TRStringKey.INV_ITEM_GRENADE_AMMO] = 52,

        [TRStringKey.INV_ITEM_FLARES] = 44,
        [TRStringKey.INV_ITEM_MEDI] = 53,
        [TRStringKey.INV_ITEM_BIG_MEDI] = 54,

        [TRStringKey.INV_ITEM_COMPASS] = 35,
        [TRStringKey.INV_ITEM_LARAS_HOME] = 59,

        [TRStringKey.INV_GLOBE_LONDON] = 85,
        [TRStringKey.INV_GLOBE_NEVADA] = 86,
        [TRStringKey.INV_GLOBE_SOUTH_PACIFIC] = 87,
        [TRStringKey.INV_GLOBE_ANTARCTICA] = 88,
    };

    private static readonly Dictionary<TRStringKey, int> _gameString2Map = new()
    {
        [TRStringKey.MISC_EMPTY_SLOT_FMT] = 15,
    };
}
