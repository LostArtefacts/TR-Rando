using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Processors;

public class TR1RSequenceProcessor : TR1RLevelProcessor
{
    private static readonly TR1Entity _dummyItem = new()
    {
        TypeID = TR1Type.CameraTarget_N,
        Invisible = true,
    };

    private BaseTRRSequenceProcessor<TR1Entity, TR1Type> _processor;

    public RandomizerSettings Settings { get; set; }

    public void Run()
    {
        _processor = new()
        {
            IsMediType = t => TR1TypeUtilities.IsMediType(t),
        };
        _processor.AdjustStrings(ScriptEditor.Script as TRRScript);
        Process(AdjustLevel);
    }

    private void AdjustLevel(TR1RCombinedLevel level)
    {
        TRRScriptedLevel mimickedLevelScript = Levels.Find(l => l.OriginalSequence == level.Script.Sequence);
        TR1Level mimickedLevel = LoadLevelData(Path.Combine(BackupPath, mimickedLevelScript.LevelFileBaseName));

        TR1Entity dummyItem = (TR1Entity)_dummyItem.Clone();
        dummyItem.SetLocation(level.Data.Entities.Find(e => e.TypeID == TR1Type.Lara).GetLocation());

        _processor.AdjustMedipacks(level.Script, level.Data.Entities, mimickedLevel.Entities, dummyItem, level.Data.FloorData);
    }
}
