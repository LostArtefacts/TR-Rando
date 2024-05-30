using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1ItemRandomizer : BaseTR1Randomizer
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

        foreach (TR1ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            if (Settings.ItemMode == ItemMode.Default && Settings.RandomizeItemTypes)
            {
                RandomizeDefaultItemDrops(_levelInstance);
            }

            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.RemovesWeapons, _levelInstance.Script.OriginalSequence);

            int? hiddenCount = _allocator.EnforceOneLimit(_levelInstance.Name, _levelInstance.Data.Entities, _levelInstance.Script.RemovesWeapons);
            if (hiddenCount.HasValue)
            {
                _levelInstance.Script.UnobtainablePickups ??= 0;
                _levelInstance.Script.UnobtainablePickups += hiddenCount;
            }

            if (Settings.ItemMode == ItemMode.Default && Settings.RandomizeItemPositions)
            {
                UpdateEnemyItemDrops(_levelInstance, _levelInstance.Data.Entities
                    .Where(e => TR1TypeUtilities.IsStandardPickupType(e.TypeID)));
            }

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }

        if (Settings.UseRecommendedCommunitySettings)
        {
            TR1Script script = ScriptEditor.Script as TR1Script;
            script.ConvertDroppedGuns = true;
            ScriptEditor.SaveScript();
        }
    }

    public void FinalizeRandomization()
    {
        foreach (TR1ScriptedLevel lvl in Levels)
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

                if (Settings.AllowEnemyKeyDrops)
                {
                    UpdateEnemyItemDrops(_levelInstance, _levelInstance.Data.Entities
                        .Where(e => TR1TypeUtilities.IsKeyItemType(e.TypeID)));
                }

                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private static void CheckTihocanPierre(TR1CombinedLevel level)
    {
        if (!level.Is(TR1LevelNames.TIHOCAN))
        {
            return;
        }

        // Enemy rando may not be selected or Pierre may have ended up at the end as usual.
        // Remove his key and scion drops and place them as items so they can be randomized.
        if (level.Data.Entities[TR1ItemAllocator.TihocanPierreIndex].TypeID == TR1Type.Pierre)
        {
            level.Script.ItemDrops.Find(d => d.EnemyNum == TR1ItemAllocator.TihocanPierreIndex)?.ObjectIds
                .RemoveAll(e => TR1ItemAllocator.TihocanEndItems.Select(i => ItemUtilities.ConvertToScriptItem(i.TypeID)).Contains(e));
        }
        level.Data.Entities.AddRange(TR1ItemAllocator.TihocanEndItems);
    }

    private void RandomizeDefaultItemDrops(TR1CombinedLevel level)
    {
        List<TR1Type> stdItemTypes = TR1TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR1Type.PistolAmmo_S_P);

        foreach (TR1Entity enemy in level.Data.Entities.Where(e => TR1TypeUtilities.IsEnemyType(e.TypeID)))
        {
            int enemyIndex = level.Data.Entities.IndexOf(enemy);
            TR1ItemDrop drop = level.Script.ItemDrops.Find(d => d.EnemyNum == enemyIndex);
            for (int i = 0; i < drop?.ObjectIds.Count; i++)
            {
                TR1Type dropType = ItemUtilities.ConvertToEntity(drop.ObjectIds[i]);
                if (stdItemTypes.Contains(dropType))
                {
                    drop.ObjectIds[i] = ItemUtilities.ConvertToScriptItem(stdItemTypes[_generator.Next(0, stdItemTypes.Count)]);
                }
            }
        }
    }

    private void UpdateEnemyItemDrops(TR1CombinedLevel level, IEnumerable<TR1Entity> pickups)
    {
        pickups = pickups.Except(_allocator.GetExcludedItems(level.Name).Select(i => level.Data.Entities[i]));

        TRRoomSector sectorFunc(Location loc) => level.Data.GetRoomSector(loc);
        foreach (TR1Entity pickup in pickups)
        {
            // There may be several enemies in one spot e.g. in cloned enemy mode. Pick one
            // at random for each call. Always exclude empty eggs.
            Location floor = pickup.GetFloorLocation(sectorFunc);
            List<TR1Entity> enemies = level.Data.Entities
                .FindAll(e => TR1TypeUtilities.IsEnemyType(e.TypeID) || e.TypeID == TR1Type.AdamEgg)
                .FindAll(e => e.GetFloorLocation(sectorFunc).IsEquivalent(floor));

            if (enemies.Count == 0 || enemies.All(e => !TR1EnemyUtilities.CanDropItems(e, level)))
            {
                continue;
            }

            TR1Entity enemy;
            do
            {
                enemy = enemies[_generator.Next(0, enemies.Count)];
            }
            while (!TR1EnemyUtilities.CanDropItems(enemy, level));

            level.Script.AddItemDrops(level.Data.Entities.IndexOf(enemy), ItemUtilities.ConvertToScriptItem(pickup.TypeID));
            ItemUtilities.HideEntity(pickup);

            // Retain the type for quick lookup in trview, but mark it as OOB for the stats.
            level.Script.UnobtainablePickups ??= 0;
            level.Script.UnobtainablePickups++;
        }
    }
}
