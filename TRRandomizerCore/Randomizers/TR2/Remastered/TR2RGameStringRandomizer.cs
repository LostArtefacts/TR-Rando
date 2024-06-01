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
        
    };

    private static readonly Dictionary<string, Dictionary<TRKeyItemKey, string>> _keyItemMap = new()
    {

    };
}
