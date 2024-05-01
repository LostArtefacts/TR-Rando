using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3RItemRandomizer : BaseTR3RRandomizer
{
    private readonly Dictionary<string, List<Location>> _excludedLocations, _pistolLocations;
    private readonly LocationPicker _picker;

    private TRSecretMapping<TR3Entity> _secretMapping;
    private TR3Entity _unarmedLevelPistols;

    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public TR3RItemRandomizer()
    {
        _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\invalid_item_locations.json"));
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\unarmed_locations.json"));
        _picker = new(GetResourcePath(@"TR3\Locations\routes.json"));
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            FindUnarmedLevelPistols(_levelInstance);

            _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, false), Settings, _generator);
            _secretMapping = TRSecretMapping<TR3Entity>.Get(GetResourcePath($@"TR3\SecretMapping\{_levelInstance.Name}-SecretMapping.json"));

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

    private void FindUnarmedLevelPistols(TR3RCombinedLevel level)
    {
        if (level.Script.RemovesWeapons)
        {
            _unarmedLevelPistols = level.Data.Entities.Find(
                e => e.TypeID == TR3Type.Pistols_P
                && _pistolLocations[level.Name].Any(l => l.IsEquivalent(e.GetLocation())));
        }
        else
        {
            _unarmedLevelPistols = null;
        }
    }

    private List<Location> GetItemLocationPool(TR3RCombinedLevel level, bool keyItemMode)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(level.Name))
        {
            exclusions.AddRange(_excludedLocations[level.Name]);
        }

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        foreach (TR3Entity entity in level.Data.Entities)
        {
            if (!TR3TypeUtilities.CanSharePickupSpace(entity.TypeID))
            {
                exclusions.Add(entity.GetFloorLocation(loc =>
                    FDUtilities.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room, level.Data, floorData)));
            }
        }

        if (level.Script.HasColdWater)
        {
            // Don't put items underwater if it's too cold
            for (int i = 0; i < level.Data.Rooms.Count; i++)
            {
                if (level.Data.Rooms[i].ContainsWater)
                {
                    exclusions.Add(new()
                    {
                        Room = i,
                        InvalidatesRoom = true
                    });
                }
            }
        }

        TR3LocationGenerator generator = new();
        return generator.Generate(level.Data, exclusions, keyItemMode);
    }

    public void RandomizeItemTypes(TR3RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        List<TR3Type> stdItemTypes = TR3TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR3Type.PistolAmmo_P);

        bool hasPistols = level.Data.Entities.Any(e => e.TypeID == TR3Type.Pistols_P);

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR3Entity entity = level.Data.Entities[i];
            TR3Type entityType = entity.TypeID;
            if (!TR3TypeUtilities.IsStandardPickupType(entityType) || _secretMapping.RewardEntities.Contains(i))
            {
                continue;
            }

            if (entity == _unarmedLevelPistols)
            {
                if (entityType == TR3Type.Pistols_P && Settings.GiveUnarmedItems)
                {
                    do
                    {
                        entityType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!TR3TypeUtilities.IsWeaponPickup(entityType));
                    entity.TypeID = entityType;
                }
            }
            else if (TR3TypeUtilities.IsStandardPickupType(entityType))
            {
                TR3Type newType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                if (newType == TR3Type.Pistols_P && (hasPistols || !level.Script.RemovesWeapons))
                {
                    do
                    {
                        newType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!TR3TypeUtilities.IsWeaponPickup(newType) || newType == TR3Type.Pistols_P);
                }
                entity.TypeID = newType;
            }

            hasPistols = level.Data.Entities.Any(e => e.TypeID == TR3Type.Pistols_P);
        }
    }

    public void RandomizeItemLocations(TR3RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR3Entity entity = level.Data.Entities[i];
            if (!TR3TypeUtilities.IsStandardPickupType(entity.TypeID)
                || _secretMapping.RewardEntities.Contains(i)
                || ItemFactory.IsItemLocked(level.Name, i)
                || entity == _unarmedLevelPistols)
            {
                continue;
            }

            _picker.RandomizePickupLocation(entity);
        }
    }

    public void EnforceOneLimit(TR3RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        HashSet<TR3Type> uniqueTypes = new();
        if (_unarmedLevelPistols != null)
        {
            uniqueTypes.Add(_unarmedLevelPistols.TypeID);
        }

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR3Entity entity = level.Data.Entities[i];
            if (_secretMapping.RewardEntities.Contains(i) || entity == _unarmedLevelPistols)
            {
                continue;
            }

            if ((TR3TypeUtilities.IsStandardPickupType(entity.TypeID) || TR3TypeUtilities.IsCrystalPickup(entity.TypeID))
                && !uniqueTypes.Add(entity.TypeID))
            {
                ItemUtilities.HideEntity(entity);
                ItemFactory.FreeItem(level.Name, i);
            }
        }
    }

    private void RandomizeKeyItems(TR3RCombinedLevel level)
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
            TR3Entity entity = level.Data.Entities[i];
            if (!TR3TypeUtilities.IsKeyItemType(entity.TypeID)
                || ItemFactory.IsItemLocked(level.Name, i))
            {
                continue;
            }

            _picker.RandomizeKeyItemLocation(
                entity, LocationUtilities.HasPickupTriger(entity, i, level.Data, floorData),
                level.Script.OriginalSequence, level.Data.Rooms[entity.Room].Info);
        }
    }

    private bool TestKeyItemLocation(Location location, bool hasPickupTrigger, TR3RCombinedLevel level)
    {
        // Make sure if we're placing on the same tile as an enemy, that the
        // enemy can drop the item.
        TR3Entity enemy = level.Data.Entities
            .FindAll(e => TR3TypeUtilities.IsEnemyType(e.TypeID))
            .Find(e => e.GetLocation().IsEquivalent(location));

        return enemy == null || (Settings.AllowEnemyKeyDrops && !hasPickupTrigger && TR3TypeUtilities.CanDropPickups
        (
            TR3TypeUtilities.GetAliasForLevel(level.Name, enemy.TypeID),
            !Settings.RandomizeEnemies || Settings.ProtectMonks
        ));
    }
}
