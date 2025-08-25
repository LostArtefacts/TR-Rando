using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Globalisation;

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

        var script = ScriptEditor.Script as TRXScript;
        foreach (var (key, value) in globalStrings)
        {
            script.BaseStrings[key.ToString()] = value;
        }

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
}
