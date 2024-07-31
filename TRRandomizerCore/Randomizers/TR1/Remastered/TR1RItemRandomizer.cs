using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1RItemRandomizer : BaseTR1RRandomizer
{
    private TR1ItemAllocator _allocator;

    public ItemFactory<TR1Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _allocator = new()
        {
            Generator = _generator,
            Settings = Settings,
            ItemFactory = ItemFactory,
        };

        _allocator.AllocateWeapons(Levels.Where(l => !l.Is(TR1LevelNames.ASSAULT)));

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.RemovesWeapons, _levelInstance.Script.OriginalSequence);
            _allocator.EnforceOneLimit(_levelInstance.Name, _levelInstance.Data.Entities, _levelInstance.Script.RemovesWeapons);

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

                CheckTihocanPierre(_levelInstance);
                if (Settings.ItemMode == ItemMode.Shuffled)
                {
                    _allocator.ApplyItemSwaps(_levelInstance.Name, _levelInstance.Data.Entities);
                }
                else
                {
                    _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.OriginalSequence);
                }

                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private static void CheckTihocanPierre(TR1RCombinedLevel level)
    {
        if (!level.Is(TR1LevelNames.TIHOCAN)
            || level.Data.Entities[TR1ItemAllocator.TihocanPierreIndex].TypeID == TR1Type.Pierre)
        {
            return;
        }

        level.Data.Entities.AddRange(TR1ItemAllocator.TihocanEndItems);
    }
}
