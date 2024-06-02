using TRRandomizerCore.Globalisation;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Randomizers;

public class TR2GameStringRandomizer : BaseTR2Randomizer
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
        ConvertGlobalStrings(globalStrings);

        if (Settings.ReassignPuzzleItems)
        {
            // This is specific to the Dagger of Xian if it appears in other levels with the dragon. We'll just
            // use whatever has already been allocated as the dagger name in Lair.
            string daggerName = ScriptEditor.ScriptedLevels.ToList().Find(l => l.Is(TR2LevelNames.LAIR)).Puzzles[1];
            foreach (AbstractTRScriptedLevel level in ScriptEditor.ScriptedLevels)
            {
                MoveAndReplacePuzzle(level, 1, 2, daggerName);
            }
        }

        SaveScript();
        TriggerProgress();
    }

    private void ConvertGlobalStrings(Dictionary<TRStringKey, string> globalStrings)
    {
        TR23Script script = ScriptEditor.Script as TR23Script;
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

    private static void MoveAndReplacePuzzle(AbstractTRScriptedLevel level, int currentIndex, int newIndex, string replacement)
    {
        if (level.Puzzles[currentIndex] == replacement)
        {
            return;
        }

        if (level.Puzzles[currentIndex] != "P" + (currentIndex + 1))
        {
            level.Puzzles[newIndex] = level.Puzzles[currentIndex];
        }
        level.Puzzles[currentIndex] = replacement;
    }

    private static readonly Dictionary<TRStringKey, int> _gameString1Map = new()
    {
        [TRStringKey.HEADING_ITEMS] = 2,
        [TRStringKey.HEADING_GAME_OVER] = 3,
        
        [TRStringKey.INV_ITEM_PISTOLS] = 36,
        [TRStringKey.INV_ITEM_PISTOL_AMMO] = 44,
        [TRStringKey.INV_ITEM_SHOTGUN] = 37,
        [TRStringKey.INV_ITEM_SHOTGUN_AMMO] = 45,
        [TRStringKey.INV_ITEM_AUTOS] = 38,
        [TRStringKey.INV_ITEM_AUTO_AMMO] = 46,
        [TRStringKey.INV_ITEM_UZI] = 39,
        [TRStringKey.INV_ITEM_UZI_AMMO] = 47,
        [TRStringKey.INV_ITEM_HARPOON] = 40,
        [TRStringKey.INV_ITEM_HARPOON_AMMO] = 48,
        [TRStringKey.INV_ITEM_M16] = 41,
        [TRStringKey.INV_ITEM_M16_AMMO] = 49,
        [TRStringKey.INV_ITEM_GRENADE_LAUNCHER] = 42,
        [TRStringKey.INV_ITEM_GRENADE_AMMO] = 50,

        [TRStringKey.INV_ITEM_FLARES] = 43,
        [TRStringKey.INV_ITEM_MEDI] = 51,
        [TRStringKey.INV_ITEM_BIG_MEDI] = 52,

        [TRStringKey.INV_ITEM_COMPASS] = 35,
        [TRStringKey.INV_ITEM_LARAS_HOME] = 57,
    };

    private static readonly Dictionary<TRStringKey, int> _gameString2Map = new()
    {
        [TRStringKey.MISC_EMPTY_SLOT_FMT] = 15,
    };
}
