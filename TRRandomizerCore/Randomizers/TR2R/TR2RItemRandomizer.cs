using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2RItemRandomizer : BaseTR2RRandomizer
{
    private readonly Dictionary<string, List<Location>> _excludedLocations, _pistolLocations;
    private readonly LocationPicker _picker;

    private TR2Entity _unarmedLevelPistols;

    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    public TR2RItemRandomizer()
    {
        _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\invalid_item_locations.json"));
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\unarmed_locations.json"));
        _picker = new(GetResourcePath(@"TR2\Locations\routes.json"));
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            FindUnarmedLevelPistols(_levelInstance);

            _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, false), Settings, _generator);

            if (Settings.RandomizeItemTypes)
            {
                RandomizeItemTypes(_levelInstance);
            }

            if (Settings.RandomizeItemPositions)
            {
                RandomizeItemLocations(_levelInstance);
            }

            if (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
            {
                EnforceOneLimit(_levelInstance);
            }

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
            RandomizeKeyItems(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void FindUnarmedLevelPistols(TR2RCombinedLevel level)
    {
        if (level.Script.RemovesWeapons)
        {
            _unarmedLevelPistols = level.Data.Entities.Find(
                e => (TR2TypeUtilities.IsGunType(e.TypeID) || e.TypeID == TR2Type.Pistols_S_P)
                && _pistolLocations[level.Name].Any(l => l.IsEquivalent(e.GetLocation())));
        }
        else
        {
            _unarmedLevelPistols = null;
        }
    }

    private List<Location> GetItemLocationPool(TR2RCombinedLevel level, bool keyItemMode)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(level.Name))
        {
            exclusions.AddRange(_excludedLocations[level.Name]);
        }

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        foreach (TR2Entity entity in level.Data.Entities)
        {
            if (!TR2TypeUtilities.CanSharePickupSpace(entity.TypeID))
            {
                exclusions.Add(entity.GetFloorLocation(loc =>
                    FDUtilities.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room, level.Data, floorData)));
            }
        }

        TR2LocationGenerator generator = new();
        return generator.Generate(level.Data, exclusions, keyItemMode);
    }

    private void RandomizeItemTypes(TR2RCombinedLevel level)
    {
        if (level.IsAssault
            || (level.Is(TR2LevelNames.HOME) && (level.Script.RemovesWeapons || level.Script.RemovesAmmo)))
        {
            return;
        }

        List<TR2Type> stdItemTypes = TR2TypeUtilities.GetStandardPickupTypes();
        IEnumerable<TR2Entity> pickups = level.Data.Entities.Where(e => e != _unarmedLevelPistols && stdItemTypes.Contains(e.TypeID));

        foreach (TR2Entity entity in pickups)
        {
            entity.TypeID = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
        }
    }

    public void RandomizeItemLocations(TR2RCombinedLevel level)
    {
        if (level.Is(TR2LevelNames.HOME) && (level.Script.RemovesWeapons || level.Script.RemovesAmmo))
        {
            return;
        }

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR2Entity entity = level.Data.Entities[i];
            if (!TR2TypeUtilities.IsStandardPickupType(entity.TypeID)
                || ItemFactory.IsItemLocked(level.Name, i)
                || entity == _unarmedLevelPistols)
            {
                continue;
            }

            _picker.RandomizePickupLocation(entity);
            entity.Intensity1 = entity.Intensity2 = -1;
        }
    }

    private void EnforceOneLimit(TR2RCombinedLevel level)
    {
        HashSet<TR2Type> uniqueTypes = new();

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR2Entity entity = level.Data.Entities[i];
            if (TR2TypeUtilities.IsStandardPickupType(entity.TypeID)
                && !uniqueTypes.Add(entity.TypeID))
            {
                ItemUtilities.HideEntity(entity);
                ItemFactory.FreeItem(level.Name, i);
            }
        }
    }

    private void RandomizeKeyItems(TR2RCombinedLevel level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        _picker.TriggerTestAction = location => LocationUtilities.HasAnyTrigger(location, level.Data, floorData);
        _picker.KeyItemTestAction = (location, hasPickupTrigger) => TestKeyItemLocation(location, hasPickupTrigger, level);
        _picker.RoomInfos = level.Data.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();

        _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, true), Settings, _generator);

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR2Entity entity = level.Data.Entities[i];
            if (!TR2TypeUtilities.IsKeyItemType(entity.TypeID)
                || ItemFactory.IsItemLocked(level.Name, i))
            {
                continue;
            }

            // In OG, all puzzle2 items are switched to puzzle3 to allow the dragon to be imported everywhere.
            // This means routes have been defined to look for these types, so we need to flip them temporarily.
            // See TR2ModelAdjuster and LocationPicker.GetKeyItemID.
            bool flipPuzzle2 = entity.TypeID == TR2Type.Puzzle2_S_P;
            if (flipPuzzle2)
            {
                entity.TypeID = TR2Type.Puzzle3_S_P;
            }

            _picker.RandomizeKeyItemLocation(
                entity, LocationUtilities.HasPickupTriger(entity, i, level.Data, floorData),
                level.Script.OriginalSequence, level.Data.Rooms[entity.Room].Info);
            entity.Intensity1 = entity.Intensity2 = -1;

            if (flipPuzzle2)
            {
                entity.TypeID = TR2Type.Puzzle2_S_P;
            }
        }
    }

    private bool TestKeyItemLocation(Location location, bool hasPickupTrigger, TR2RCombinedLevel level)
    {
        // Make sure if we're placing on the same tile as an enemy, that the
        // enemy can drop the item.
        TR2Entity enemy = level.Data.Entities
            .FindAll(e => TR2TypeUtilities.IsEnemyType(e.TypeID))
            .Find(e => e.GetLocation().IsEquivalent(location));

        return enemy == null || (Settings.AllowEnemyKeyDrops && !hasPickupTrigger && TR2TypeUtilities.CanDropPickups
        (
            TR2TypeUtilities.GetAliasForLevel(level.Name, enemy.TypeID),
            Settings.RandomizeEnemies && !Settings.ProtectMonks,
            Settings.RandomizeEnemies && Settings.UnconditionalChickens
        ));
    }
}
