using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class TR2RItemRandomizer : BaseTR2RRandomizer
{
    private TR2ItemAllocator _allocator;

    public ItemFactory<TR2Entity> ItemFactory { get; set; }

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
            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void RandomizeKeyItems()
    {
        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            // In OG, all puzzle2 items are switched to puzzle3 to allow the dragon to be imported everywhere.
            // This means routes have been defined to look for these types, so we need to flip them temporarily.
            // See TR2ModelAdjuster and LocationPicker.GetKeyItemID.
            List<TR2Entity> puzzle2Items = _levelInstance.Data.Entities.FindAll(e => e.TypeID == TR2Type.Puzzle2_S_P);
            puzzle2Items.ForEach(e => e.TypeID = TR2Type.Puzzle3_S_P);

            _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.OriginalSequence);
            puzzle2Items.ForEach(e => e.TypeID = TR2Type.Puzzle2_S_P);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }
}
