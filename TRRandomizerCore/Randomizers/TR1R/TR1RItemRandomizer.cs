using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1RItemRandomizer : BaseTR1RRandomizer
{
    private readonly Dictionary<string, List<Location>> _excludedLocations, _pistolLocations;
    private readonly LocationPicker _picker;

    private TRSecretMapping<TR1Entity> _secretMapping;
    private TR1Entity _unarmedLevelPistols;

    public ItemFactory<TR1Entity> ItemFactory { get; set; }

    public TR1RItemRandomizer()
    {
        _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\invalid_item_locations.json"));
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\unarmed_locations.json"));
        _picker = new(GetResourcePath(@"TR1\Locations\routes.json"));
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            FindUnarmedLevelPistols(_levelInstance);

            _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, false), Settings, _generator);
            _secretMapping = TRSecretMapping<TR1Entity>.Get(GetResourcePath($@"TR1\SecretMapping\{_levelInstance.Name}-SecretMapping.json"));

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

    private void FindUnarmedLevelPistols(TR1RCombinedLevel level)
    {
        if (level.Script.RemovesWeapons)
        {
            _unarmedLevelPistols = level.Data.Entities.Find(
                e => TR1TypeUtilities.IsWeaponPickup(e.TypeID)
                && _pistolLocations[level.Name].Any(l => l.IsEquivalent(e.GetLocation())));
        }
        else
        {
            _unarmedLevelPistols = null;
        }
    }

    private List<Location> GetItemLocationPool(TR1RCombinedLevel level, bool keyItemMode)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(level.Name))
        {
            exclusions.AddRange(_excludedLocations[level.Name]);
        }

        foreach (TR1Entity entity in level.Data.Entities)
        {
            if (!TR1TypeUtilities.CanSharePickupSpace(entity.TypeID))
            {
                exclusions.Add(entity.GetFloorLocation(loc =>
                    level.Data.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room)));
            }
        }

        TR1LocationGenerator generator = new();
        return generator.Generate(level.Data, exclusions, keyItemMode);
    }

    public void RandomizeItemTypes(TR1RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        List<TR1Type> stdItemTypes = TR1TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR1Type.PistolAmmo_S_P);

        bool hasPistols = level.Data.Entities.Any(e => e.TypeID == TR1Type.Pistols_S_P);

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR1Entity entity = level.Data.Entities[i];
            TR1Type entityType = entity.TypeID;
            if (!TR1TypeUtilities.IsStandardPickupType(entityType) || _secretMapping.RewardEntities.Contains(i))
            {
                continue;
            }

            if (entity == _unarmedLevelPistols)
            {
                if (entityType == TR1Type.Pistols_S_P && Settings.GiveUnarmedItems)
                {
                    do
                    {
                        entityType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!TR1TypeUtilities.IsWeaponPickup(entityType));
                    entity.TypeID = entityType;
                }
            }
            else if (TR1TypeUtilities.IsStandardPickupType(entityType))
            {
                TR1Type newType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                if (newType == TR1Type.Pistols_S_P && (hasPistols || !level.Script.RemovesWeapons))
                {
                    do
                    {
                        newType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!TR1TypeUtilities.IsWeaponPickup(newType) || newType == TR1Type.Pistols_S_P);
                }
                entity.TypeID = newType;
            }

            hasPistols = level.Data.Entities.Any(e => e.TypeID == TR1Type.Pistols_S_P);
        }
    }

    public void RandomizeItemLocations(TR1RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR1Entity entity = level.Data.Entities[i];
            if (!TR1TypeUtilities.IsStandardPickupType(entity.TypeID)
                || _secretMapping.RewardEntities.Contains(i)
                || ItemFactory.IsItemLocked(level.Name, i)
                || entity == _unarmedLevelPistols)
            {
                continue;
            }

            _picker.RandomizePickupLocation(entity);
            entity.Intensity = 0;
        }
    }

    public void EnforceOneLimit(TR1RCombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        HashSet<TR1Type> uniqueTypes = new();
        if (_unarmedLevelPistols != null)
        {
            uniqueTypes.Add(_unarmedLevelPistols.TypeID);
        }

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR1Entity entity = level.Data.Entities[i];
            if (_secretMapping.RewardEntities.Contains(i) || entity == _unarmedLevelPistols)
            {
                continue;
            }

            if ((TR1TypeUtilities.IsStandardPickupType(entity.TypeID) || TR1TypeUtilities.IsCrystalPickup(entity.TypeID))
                && !uniqueTypes.Add(entity.TypeID))
            {
                ItemUtilities.HideEntity(entity);
                ItemFactory.FreeItem(level.Name, i);
            }
        }
    }

    private void RandomizeKeyItems(TR1RCombinedLevel level)
    {
        _picker.TriggerTestAction = location => LocationUtilities.HasAnyTrigger(location, level.Data);
        _picker.RoomInfos = level.Data.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();

        _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, true), Settings, _generator);

        if (level.Is(TR1LevelNames.TIHOCAN)
            && level.Data.Entities[TR1ItemRandomizer.TihocanPierreIndex].TypeID != TR1Type.Pierre)
        {
            level.Data.Entities.AddRange(TR1ItemRandomizer.TihocanEndItems);
        }

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR1Entity entity = level.Data.Entities[i];
            if (!IsMovableKeyItem(level, entity)
                || ItemFactory.IsItemLocked(level.Name, i))
            {
                continue;
            }

            bool hasPickupTrigger = LocationUtilities.HasPickupTriger(entity, i, level.Data);
            _picker.RandomizeKeyItemLocation(entity, hasPickupTrigger,
                level.Script.OriginalSequence, level.Data.Rooms[entity.Room].Info);
        }
    }

    private static bool IsMovableKeyItem(TR1RCombinedLevel level, TR1Entity entity)
    {
        return TR1TypeUtilities.IsKeyItemType(entity.TypeID)
            || (level.Is(TR1LevelNames.TIHOCAN) && entity.TypeID == TR1Type.ScionPiece2_S_P);
    }
}
