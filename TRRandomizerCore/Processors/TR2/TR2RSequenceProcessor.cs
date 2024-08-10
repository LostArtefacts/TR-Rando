using System.Diagnostics;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Processors;

public class TR2RSequenceProcessor : TR2RLevelProcessor
{
    private static readonly TR2Entity _dummyItem = new()
    {
        TypeID = TR2Type.CameraTarget_N,
        Invisible = true,
    };

    private static readonly Dictionary<string, Dictionary<int, int>> _itemSwaps = new()
    {
        [TR2LevelNames.XIAN] = new()
        {
            [10] = 16,
            [91] = 89,
            [140] = 138,
            [165] = 199,
            [171] = 170,
            [200] = 44,
        }
    };

    private BaseTRRSequenceProcessor<TR2Entity, TR2Type> _processor;

    public RandomizerSettings Settings { get; set; }

    public void Run()
    {
        _processor = new()
        {
            IsMediType = t => TR2TypeUtilities.IsMediType(t),
        };
        _processor.AdjustStrings(ScriptEditor.Script as TRRScript);
        Process(AdjustLevel);
    }

    private void AdjustLevel(TR2RCombinedLevel level)
    {
        if (level.Script.Sequence == level.Script.OriginalSequence)
        {
            return;
        }

        TRRScriptedLevel mimickedLevelScript = Levels.Find(l => l.OriginalSequence == level.Script.Sequence);
        TR2Level mimickedLevel = LoadLevelData(Path.Combine(BackupPath, mimickedLevelScript.LevelFileBaseName));

        TR2Entity dummyItem = (TR2Entity)_dummyItem.Clone();
        dummyItem.SetLocation(level.Data.Entities.Find(e => e.TypeID == TR2Type.Lara).GetLocation());

        List<int> freeIndices = ReleaseItems(level, dummyItem);

        _processor.AdjustMedipacks(level.Script, level.Data.Entities, mimickedLevel.Entities, dummyItem, level.Data.FloorData, freeIndices);

        if (level.Is(TR2LevelNames.MONASTERY))
        {
            // The Seraph won't be in the inventory so make the final puzzle slot a lever instead.
            // No other level has a Puzzle4 item so it doesn't matter what ends up in that sequence.
            TR2Entity slot = level.Data.Entities.Find(e => e.TypeID == TR2Type.PuzzleHole4);
            Debug.Assert(slot != null);
            slot.TypeID = TR2Type.WallSwitch;
            level.Data.FloorData.GetSwitchKeyTriggers(level.Data.Entities.IndexOf(slot))
                .ForEach(t => t.TrigType = FDTrigType.Switch);
        }
    }

    private static List<int> ReleaseItems(TR2RCombinedLevel level, TR2Entity dummyItem)
    {
        List<int> freeIndices = new();
        if (!_itemSwaps.ContainsKey(level.Name))
        {
            return freeIndices;
        }

        // This generally releases camera targets and makes the triggers look at nearby items
        // instead. Ensures for item heavy levels there is adequate space to populate with
        // dummy medipacks later.
        foreach (var (index, swapIndex) in _itemSwaps[level.Name])
        {
            level.Data.FloorData.GetEntityActionItems(index)
                .ForEach(a => a.Parameter = (short)swapIndex);
            freeIndices.Add(index);
            level.Data.Entities[index] = dummyItem;
        }

        return freeIndices;
    }
}
