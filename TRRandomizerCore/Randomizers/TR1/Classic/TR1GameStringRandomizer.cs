using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Globalisation;

namespace TRRandomizerCore.Randomizers;

public class TR1GameStringRandomizer : BaseTR1Randomizer
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

        TR1Script script = ScriptEditor.Script as TR1Script;
        foreach (var (key, value) in globalStrings)
        {
            script.Strings[key.ToString()] = value;
        }

        AmendDefaultStrings();

        SaveScript();
        TriggerProgress();
    }

    private void AmendDefaultStrings()
    {
        List<AbstractTRScriptedLevel> levels = ScriptEditor.Levels.ToList();
        AbstractTRScriptedLevel cistern = levels.Find(l => l.Is(TR1LevelNames.CISTERN));
        AbstractTRScriptedLevel mines = levels.Find(l => l.Is(TR1LevelNames.MINES));
        if (cistern == null || mines == null)
        {
            return;
        }

        // Duplicate whatever Cistern has for "Rusty Key" into Mines
        mines.Keys.Add(cistern.Keys.Count > 2 ? cistern.Keys[2] : "Rusty Key");
    }
}
