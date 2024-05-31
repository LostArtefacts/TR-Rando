using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class TR3RItemRandomizer : BaseTR3RRandomizer
{
    private TR3ItemAllocator _allocator;

    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _allocator = new()
        {
            Generator = _generator,
            Settings = Settings,
            ItemFactory = ItemFactory,
        };

        _allocator.AllocateWeapons(Levels.Where(l => !l.Is(TR3LevelNames.ASSAULT)));

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            if (_levelInstance.IsAssault)
            {
                TriggerProgress();
                continue;
            }

            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data,
                _levelInstance.Script.RemovesWeapons, _levelInstance.Script.OriginalSequence, _levelInstance.HasExposureMeter);

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
                }
                else
                {
                    _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data,
                        _levelInstance.Script.OriginalSequence, _levelInstance.HasExposureMeter);
                }

                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }
}
