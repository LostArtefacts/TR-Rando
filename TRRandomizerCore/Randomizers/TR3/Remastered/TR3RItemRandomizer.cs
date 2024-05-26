using TRGE.Core;
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
        _allocator = new(true)
        {
            Generator = _generator,
            Settings = Settings,
            ItemFactory = ItemFactory,
        };

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            if (_levelInstance.IsAssault)
            {
                TriggerProgress();
                continue;
            }

            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data,
                _levelInstance.Script.RemovesWeapons, _levelInstance.HasExposureMeter);

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
            _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data,
                _levelInstance.Script.OriginalSequence, _levelInstance.HasExposureMeter);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }
}
