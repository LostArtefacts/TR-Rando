using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class TR2RItemRandomizer : BaseTR2RRandomizer
{
    private TR2ItemAllocator _allocator;

    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    private List<TR2Entity> _puzzle2Items;

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _allocator = new()
        {
            Generator = _generator,
            Settings = Settings,
            ItemFactory = ItemFactory,
        };

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            ConvertPuzzle2Items();

            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script);
            
            RevertPuzzle2Items();
            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void FinalizeRandomization()
    {
        foreach (TRRScriptedLevel lvl in Levels)
        {
            if (Settings.ItemMode == ItemMode.Shuffled || Settings.IncludeKeyItems)
            {
                LoadLevelInstance(lvl);

                if (Settings.ItemMode == ItemMode.Shuffled)
                {
                    _allocator.ApplyItemSwaps(_levelInstance.Name, _levelInstance.Data.Entities);
                    _puzzle2Items = _levelInstance.Data.Entities.FindAll(e => e.TypeID == TR2Type.Puzzle3_S_P); // Already converted in first shuffling stage
                }
                else
                {
                    ConvertPuzzle2Items();
                    _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.OriginalSequence);
                }

                RevertPuzzle2Items();
                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void ConvertPuzzle2Items()
    {
        // In OG, all puzzle2 items are switched to puzzle3 to allow the dragon to be imported everywhere.
        // This means routes have been defined to look for these types, so we need to flip them temporarily.
        // See TR2ModelAdjuster and LocationPicker.GetKeyItemID.
        _puzzle2Items = _levelInstance.Data.Entities.FindAll(e => e.TypeID == TR2Type.Puzzle2_S_P);
        _puzzle2Items.ForEach(e => e.TypeID = TR2Type.Puzzle3_S_P);
    }

    private void RevertPuzzle2Items()
        => _puzzle2Items.ForEach(e => e.TypeID = TR2Type.Puzzle2_S_P);
}
