using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3ItemAllocator : ItemAllocator<TR3Type, TR3Entity>
{
    public TR3ItemAllocator()
        : base(TRGameVersion.TR3) { }

    protected override List<int> GetExcludedItems(string levelName)
    {
        TRSecretMapping<TR3Entity> mapping = TRSecretMapping<TR3Entity>.Get($@"Resources\TR3\SecretMapping\{levelName}-SecretMapping.json");
        return mapping?.RewardEntities ?? new();
    }

    protected override TR3Type GetPistolType()
        => TR3Type.Pistols_P;

    protected override List<TR3Type> GetStandardItemTypes()
    {
        List<TR3Type> stdItemTypes = TR3TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR3Type.PistolAmmo_P);
        return stdItemTypes;
    }

    protected override List<TR3Type> GetWeaponItemTypes()
        => TR3TypeUtilities.GetWeaponPickups();

    protected override bool IsCrystalPickup(TR3Type type)
        => type == TR3Type.SaveCrystal_P;

    public void RandomizeItems(string levelName, TR3Level level, bool isUnarmed, bool isCold)
    {
        _picker.Initialise(levelName, GetItemLocationPool(levelName, level, false, isCold), Settings, Generator);

        RandomizeItemTypes(levelName, level.Entities, isUnarmed);
        RandomizeItemLocations(levelName, level.Entities, isUnarmed);
    }

    public void RandomizeKeyItems(string levelName, TR3Level level, int originalSequence, bool isCold)
    {
        _picker.TriggerTestAction = location => LocationUtilities.HasAnyTrigger(location, level);
        _picker.KeyItemTestAction = (location, hasPickupTrigger) => TestKeyItemLocation(location, hasPickupTrigger, levelName, level);
        _picker.RoomInfos = new(level.Rooms.Select(r => new ExtRoomInfo(r)));

        _picker.Initialise(levelName, GetItemLocationPool(levelName, level, true, isCold), Settings, Generator);

        for (int i = 0; i < level.Entities.Count; i++)
        {
            TR3Entity entity = level.Entities[i];
            if (!TR3TypeUtilities.IsKeyItemType(entity.TypeID)
                || ItemFactory.IsItemLocked(levelName, i))
            {
                continue;
            }

            _picker.RandomizeKeyItemLocation(
                entity, LocationUtilities.HasPickupTriger(entity, i, level),
                originalSequence, level.Rooms[entity.Room].Info);
        }
    }

    private bool TestKeyItemLocation(Location location, bool hasPickupTrigger, string levelName, TR3Level level)
    {
        // Make sure if we're placing on the same tile as an enemy, that the enemy can drop the item.
        TR3Entity enemy = level.Entities
            .FindAll(e => TR3TypeUtilities.IsEnemyType(e.TypeID))
            .Find(e => e.GetLocation().IsEquivalent(location));

        return enemy == null || (Settings.AllowEnemyKeyDrops && !hasPickupTrigger && TR3TypeUtilities.CanDropPickups
        (
            TR3TypeUtilities.GetAliasForLevel(levelName, enemy.TypeID),
            !Settings.RandomizeEnemies || Settings.ProtectMonks
        ));
    }

    private List<Location> GetItemLocationPool(string levelName, TR3Level level, bool keyItemMode, bool isCold)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(levelName))
        {
            exclusions.AddRange(_excludedLocations[levelName]);
        }

        exclusions.AddRange(level.Entities
            .Where(e => !TR3TypeUtilities.CanSharePickupSpace(e.TypeID))
            .Select(e => e.GetFloorLocation(loc => level.GetRoomSector(loc))));

        if (Settings.RandomizeSecrets
            && level.FloorData.GetActionItems(FDTrigAction.SecretFound).Any()) // && !remastered => make UseRewardRooms setting
        {
            // Make sure to exclude the reward room
            exclusions.Add(new()
            {
                Room = RoomWaterUtilities.DefaultRoomCountDictionary[levelName],
                InvalidatesRoom = true
            });
        }

        if (isCold)
        {
            // Don't put items underwater if it's too cold
            for (short i = 0; i < level.Rooms.Count; i++)
            {
                if (level.Rooms[i].ContainsWater)
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
        return generator.Generate(level, exclusions, keyItemMode);
    }
}
