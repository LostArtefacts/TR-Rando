using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1ItemAllocator : ItemAllocator<TR1Type, TR1Entity>
{
    public const int TihocanPierreIndex = 82;

    public static readonly List<TR1Entity> TihocanEndItems = new()
    {
        new()
        {
            TypeID = TR1Type.Key1_S_P,
            X = 30208,
            Y = 2560,
            Z = 91648,
            Room = 110,
            Intensity = 6144
        },
        new ()
        {
            TypeID = TR1Type.ScionPiece2_S_P,
            X = 34304,
            Y = 2560,
            Z = 91648,
            Room = 110,
            Intensity = 6144
        }
    };

    private static readonly Dictionary<string, int> _extraItemCounts = new()
    {
        [TR1LevelNames.CAVES]
            = 10, // Default = 4
        [TR1LevelNames.VILCABAMBA]
            = 9,  // Default = 7
        [TR1LevelNames.VALLEY]
            = 15, // Default = 2
        [TR1LevelNames.QUALOPEC]
            = 6,  // Default = 5
        [TR1LevelNames.FOLLY]
            = 8,  // Default = 8
        [TR1LevelNames.COLOSSEUM]
            = 11, // Default = 7
        [TR1LevelNames.MIDAS]
            = 4,  // Default = 12
    };

    public TR1ItemAllocator()
        : base(TRGameVersion.TR1) { }

    protected override List<int> GetExcludedItems(string levelName)
    {
        TRSecretMapping<TR1Entity> mapping = TRSecretMapping<TR1Entity>.Get($@"Resources\TR1\SecretMapping\{levelName}-SecretMapping.json");
        return mapping?.RewardEntities ?? new();
    }

    protected override TR1Type GetPistolType()
        => TR1Type.Pistols_S_P;

    protected override List<TR1Type> GetStandardItemTypes()
    {
        List<TR1Type> stdItemTypes = TR1TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR1Type.PistolAmmo_S_P);
        return stdItemTypes;
    }

    protected override List<TR1Type> GetWeaponItemTypes()
        => TR1TypeUtilities.GetWeaponPickups();

    protected override List<TR1Type> GetKeyItemTypes()
        => TR1TypeUtilities.GetKeyItemTypes();

    protected override bool IsCrystalPickup(TR1Type type)
        => type == TR1Type.SavegameCrystal_P;

    protected override void ItemMoved(TR1Entity item)
        => item.Intensity = 0;

    public void RandomizeItems(string levelName, TR1Level level, bool isUnarmed, int originalSequence)
    {
        InitialisePicker(levelName, level, Settings.ItemMode == ItemMode.Default ? LocationMode.Default : LocationMode.ExistingItems);
        AddExtraPickups(levelName, level.Entities);

        if (Settings.ItemMode == ItemMode.Default)
        {
            RandomizeItemTypes(levelName, level.Entities, isUnarmed);
            RandomizeItemLocations(levelName, level.Entities, isUnarmed);
        }
        else
        {
            ShuffleItems(levelName, level.Entities, isUnarmed, GetKeyItemLevelSequence(levelName, originalSequence));
        }
        
        RandomizeSprites(level);
    }

    public void RandomizeKeyItems(string levelName, TR1Level level, int originalSequence)
    {
        InitialisePicker(levelName, level, LocationMode.KeyItems);
        originalSequence = GetKeyItemLevelSequence(levelName, originalSequence);

        for (int i = 0; i < level.Entities.Count; i++)
        {
            TR1Entity entity = level.Entities[i];
            if (!IsMovableKeyItem(levelName, entity)
                || ItemFactory.IsItemLocked(levelName, i))
            {
                continue;
            }

            bool hasPickupTrigger = LocationUtilities.HasPickupTriger(entity, i, level);
            _picker.RandomizeKeyItemLocation(entity, hasPickupTrigger, originalSequence);
            ItemMoved(entity);
        }
    }

    private void InitialisePicker(string levelName, TR1Level level, LocationMode locationMode)
    {
        _picker.TriggerTestAction = locationMode == LocationMode.KeyItems
            ? location => LocationUtilities.HasAnyTrigger(location, level)
            : null;
        _picker.RoomInfos = new(level.Rooms.Select(r => new ExtRoomInfo(r)));

        List<Location> pool = GetItemLocationPool(levelName, level, locationMode != LocationMode.Default);
        if (locationMode == LocationMode.ExistingItems)
        {
            // OG items may not be centre-tile, plus we want to exclude such things as the unreachable Midas medipack.
            IEnumerable<Location> itemLocations = GetPickups(levelName, level.Entities, true)
                .Select(e => e.GetLocation())
                .DistinctBy(l => level.GetRoomSector(l));
            pool = new(itemLocations.Where(i => pool.Any(e => level.GetRoomSector(i) == level.GetRoomSector(e))));
        }
        _picker.Initialise(levelName, pool, Settings, Generator);
    }

    private int GetKeyItemLevelSequence(string levelName, int originalSequence)
    {
        if (Settings.GameMode != GameMode.Normal && TR1LevelNames.AsListGold.Contains(levelName))
        {
            // The original sequence is always 1-based regardless of how we have
            // combined, so we need to manually shift. This ensures there are no
            // clashes in TR1Type between regular and expansion levels.
            originalSequence += TR1LevelNames.AsList.Count;
        }
        return originalSequence;
    }

    private static bool IsMovableKeyItem(string levelName, TR1Entity entity)
    {
        return TR1TypeUtilities.IsKeyItemType(entity.TypeID)
            || (levelName == TR1LevelNames.TIHOCAN && entity.TypeID == TR1Type.ScionPiece2_S_P);
    }

    private List<Location> GetItemLocationPool(string levelName, TR1Level level, bool keyItemMode)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(levelName))
        {
            exclusions.AddRange(_excludedLocations[levelName]);
        }

        exclusions.AddRange(level.Entities
            .Where(e => !TR1TypeUtilities.CanSharePickupSpace(e.TypeID))
            .Select(e => e.GetFloorLocation(loc => level.GetRoomSector(loc))));

        if (Settings.RandomizeSecrets
            && Settings.SecretRewardMode == TRSecretRewardMode.Room
            && level.FloorData.GetActionItems(FDTrigAction.SecretFound).Any())
        {
            // Make sure to exclude the reward room
            exclusions.Add(new()
            {
                Room = RoomWaterUtilities.DefaultRoomCountDictionary[levelName],
                InvalidatesRoom = true
            });
        }

        TR1LocationGenerator generator = new();
        return generator.Generate(level, exclusions, keyItemMode);
    }

    private void AddExtraPickups(string levelName, List<TR1Entity> allItems)
    {
        if (!Settings.IncludeExtraPickups || !_extraItemCounts.ContainsKey(levelName))
        {
            return;
        }

        List<TR1Type> stdItemTypes = GetStandardItemTypes();
        stdItemTypes.Remove(GetPistolType());

        // Add what we can to the level. The locations and types may be further randomized depending on the selected options.
        for (int i = 0; i < _extraItemCounts[levelName]; i++)
        {
            if (!ItemFactory.CanCreateItem(levelName, allItems))
            {
                break;
            }

            TR1Entity newItem = ItemFactory.CreateItem(levelName, allItems, _picker.GetRandomLocation());
            newItem.TypeID = stdItemTypes[Generator.Next(0, stdItemTypes.Count)];
        }
    }

    public void RandomizeSprites(TR1Level level)
    {
        if (!Settings.RandomizeItemSprites)
        {
            return;
        }

        List<TR1Type> secretTypes = new();
        List<TR1Type> keyItemTypes = new();

        foreach (TR1Type type in TR1TypeUtilities.GetKeyItemTypes())
        {
            int instanceIndex = level.Entities.FindIndex(e => e.TypeID == type);
            if (instanceIndex != -1)
            {
                if (IsSecretItem(level.Entities[instanceIndex], instanceIndex, level))
                {
                    secretTypes.Add(type);
                }
                else
                {
                    keyItemTypes.Add(type);
                }
            }
        }

        RandomizeSprites(level.Sprites, keyItemTypes, secretTypes);
    }

    private static bool IsSecretItem(TR1Entity entity, int entityIndex, TR1Level level)
    {
        TRRoomSector sector = level.GetRoomSector(entity);
        if (sector.FDIndex != 0)
        {
            return level.FloorData[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                && trigger.TrigType == FDTrigType.Pickup
                && trigger.Actions[0].Parameter == entityIndex
                && trigger.Actions.Any(a => a.Action == FDTrigAction.SecretFound);
        }

        return false;
    }
}
