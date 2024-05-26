using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers;

public class TR1RItemRandomizer : BaseTR1RRandomizer
{
    private TR1ItemAllocator _allocator;

    public ItemFactory<TR1Entity> ItemFactory { get; set; }

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

            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.RemovesWeapons);
            _allocator.EnforceOneLimit(_levelInstance.Name, _levelInstance.Data.Entities, _levelInstance.Script.RemovesWeapons);

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

            CheckTihocanPierre(_levelInstance);
            _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.OriginalSequence);

            SaveLevelInstance();
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
