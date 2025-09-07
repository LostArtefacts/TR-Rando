using TRGE.Core;
using TRGE.Core.Item.Enums;
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

        var script = ScriptEditor.Script as TRXScript;
        foreach (var (key, value) in globalStrings)
        {
            script.BaseStrings[key.ToString()] = value;
        }

        script.ObjectStrings[(int)TR1Items.LaraHomePhoto_M_H].Name = script.AssaultLevel.Name;

        AmendDefaultStrings();

        SaveScript();
        TriggerProgress();
    }

    private void AmendDefaultStrings()
    {
        var cistern = ScriptEditor.Levels.FirstOrDefault(l => l.Is(TR1LevelNames.CISTERN));
        var mines = ScriptEditor.Levels.FirstOrDefault(l => l.Is(TR1LevelNames.MINES));
        if (cistern != null && mines != null)
        {
            // Duplicate whatever Cistern has for "Rusty Key" into Mines
            mines.Keys.Add(cistern.Keys.Count > 2 ? cistern.Keys[2] : "Rusty Key");
        }
    }
}
