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
                    AdjustNGPlusItems(_levelInstance);
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

    private void AdjustNGPlusItems(TR1RCombinedLevel level)
    {
        // If keys have ended up as OG medi-packs, NG+ will convert them to ammo and hence softlock-city.
        // Duplicate the items so the game doesn't know their indices, and hide the originals.
        List<TR1Entity> keyItems = level.Data.Entities.FindAll(e => TR1TypeUtilities.IsKeyItemType(e.TypeID));
        TR1Level ogLevel = _levelControl.Read(Path.Combine(BackupPath, level.Name));
        foreach (TR1Entity item in keyItems)
        {
            int currentIndex = level.Data.Entities.IndexOf(item);
            if (currentIndex < ogLevel.Entities.Count
                && TR1TypeUtilities.IsMediType(ogLevel.Entities[currentIndex].TypeID))
            {
                level.Data.Entities.Add((TR1Entity)item.Clone());
                item.TypeID = TR1Type.CameraTarget_N;
                ItemUtilities.HideEntity(item);

                // Any triggers will need to match the new index
                short newIndex = (short)(level.Data.Entities.Count - 1);
                level.Data.FloorData.GetEntityTriggers(currentIndex)
                    .SelectMany(t => t.Actions.Where(a => a.Action == FDTrigAction.Object && a.Parameter == currentIndex))
                    .ToList().ForEach(a => a.Parameter = newIndex);
            }
        }
    }
}
