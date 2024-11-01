using TRGE.Core;
using TRLevelControl;
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

            if (lvl.Is(TR3LevelNames.HSC))
            {
                // These can't be picked up if they are moved out of these rooms.
                _levelInstance.Data.Entities.Where(e => TR3TypeUtilities.IsAnyPickupType(e.TypeID) && (e.Room == 167 || e.Room == 168))
                    .ToList()
                    .ForEach(e => ItemFactory.LockItem(_levelInstance.Name, _levelInstance.Data.Entities.IndexOf(e)));
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

                if (_levelInstance.Is(TR3LevelNames.JUNGLE)
                    && _levelInstance.Data.Entities.Find(e => e.TypeID == TR3Type.Key4_P) is TR3Entity key)
                {
                    // Counteract the shift done by the game in patch 3. Nudge it slightly further in case
                    // we land on an enemy.
                    key.X += TRConsts.Step2 * 3 + 1;
                    key.Z += TRConsts.Step2;
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
