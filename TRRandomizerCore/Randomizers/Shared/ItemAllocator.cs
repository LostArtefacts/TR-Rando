using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public abstract class ItemAllocator<T, E>
    where T : Enum
    where E : TREntity<T>, new()
{
    private const double _fairWeaponChance = 0.4;
    private const double _hardWeaponChance = 0.15;

    protected readonly Dictionary<string, List<Location>> _excludedLocations;
    protected readonly Dictionary<string, List<Location>> _pistolLocations;
    protected readonly Dictionary<string, E> _unarmedPistolCache;
    protected readonly LocationPicker _picker;

    protected Dictionary<string, List<T>> _weaponAllocations;
    protected Dictionary<string, List<ItemSwap>> _itemSwapCache;
    protected ItemSpriteRandomizer<T> _spriteRandomizer;

    public Random Generator { get; set; }
    public RandomizerSettings Settings { get; set; }
    public ItemFactory<E> ItemFactory { get; set; }

    public ItemAllocator(TRGameVersion gameVersion)
    {
        _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText($@"Resources\{gameVersion}\Locations\invalid_item_locations.json"));
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText($@"Resources\{gameVersion}\Locations\unarmed_locations.json"));
        _picker = new($@"Resources\{gameVersion}\Locations\routes.json");
        _unarmedPistolCache = new();
        _weaponAllocations = new();
    }

    public static bool TypeMatch(E item1, E item2)
        => TypeMatch(item1.TypeID, item2.TypeID);

    public static bool TypeMatch(T type1, T type2)
        => EqualityComparer<T>.Default.Equals(type1, type2);

    public E GetUnarmedLevelPistols(string levelName, List<E> items)
    {
        if (!_pistolLocations.ContainsKey(levelName))
        {
            return null;
        }

        if (!_unarmedPistolCache.ContainsKey(levelName))
        {
            List<T> weaponTypes = GetWeaponItemTypes();
            T pistols = GetPistolType();
            _unarmedPistolCache[levelName] = items.Find(e =>
                (weaponTypes.Contains(e.TypeID) || TypeMatch(e.TypeID, pistols))
                && _pistolLocations[levelName].Any(l => l.IsEquivalent(e.GetLocation())));
        }
        
        return _unarmedPistolCache[levelName];
    }

    public void AllocateWeapons<S>(IEnumerable<S> scriptedLevels)
        where S : AbstractTRScriptedLevel
    {
        if (Settings.ItemMode == ItemMode.Shuffled || !Settings.RandomizeItemTypes || Settings.WeaponDifficulty == WeaponDifficulty.Easy)
        {
            return;
        }

        bool hardMode = Settings.WeaponDifficulty == WeaponDifficulty.Hard;

        _weaponAllocations.Clear();
        List<S> levels = new(scriptedLevels);
        levels.Sort((l1, l2) => l1.Sequence.CompareTo(l2.Sequence));

        List<T> weaponTypes = GetWeaponItemTypes();
        weaponTypes.Remove(GetPistolType());

        HashSet<T> gameTracker = new();
        for (int i = 0; i < levels.Count; i++)
        {
            S level = levels[i];
            bool previousUnarmed = i > 0 && levels[i - 1].RemovesWeapons;

            if (level.RemovesWeapons)
            {
                gameTracker.Clear();
            }
            else if (hardMode && gameTracker.Count == weaponTypes.Count)
            {
                continue;
            }
            else if (!previousUnarmed && gameTracker.Count != 0 && Generator.NextDouble() > (hardMode ? _hardWeaponChance : _fairWeaponChance))
            {
                continue;
            }

            List<T> levelWeapons = new();
            _weaponAllocations[level.LevelFileBaseName.ToUpper()] = levelWeapons;

            int levelAllocation = hardMode ? 1 : 2;
            if ((level.RemovesWeapons && !hardMode) || previousUnarmed)
            {
                levelAllocation++;
            }

            levelAllocation = Math.Min(levelAllocation, weaponTypes.Count);

            for (int j = 0; j < levelAllocation; j++)
            {
                T type;
                do
                {
                    type = weaponTypes.RandomItem(Generator);
                }
                while (levelWeapons.Contains(type) || (gameTracker.Contains(type) && gameTracker.Count < weaponTypes.Count));
                levelWeapons.Add(type);
                gameTracker.Add(type);
            }
        }
    }

    public void RandomizeItemTypes(string levelName, List<E> items, bool isUnarmed)
    {
        if (!Settings.RandomizeItemTypes)
        {
            return;
        }

        List<T> stdItemTypes = GetStandardItemTypes();
        List<T> weaponTypes = GetWeaponItemTypes();
        T pistols = GetPistolType();

        if (Settings.WeaponDifficulty != WeaponDifficulty.Easy)
        {
            stdItemTypes.RemoveAll(weaponTypes.Contains);
        }
        if (!stdItemTypes.Contains(pistols))
        {
            stdItemTypes.Add(pistols);
        }

        List<int> excludedItems = GetExcludedItems(levelName);

        bool hasPistols = items.Any(e => TypeMatch(e.TypeID, pistols));
        E unarmedPistols = isUnarmed ? GetUnarmedLevelPistols(levelName, items) : null;

        List<E> pickupItems = items.FindAll(e =>
            (stdItemTypes.Contains(e.TypeID) || weaponTypes.Contains(e.TypeID))
            && !excludedItems.Contains(items.IndexOf(e)));

        if (_weaponAllocations.ContainsKey(levelName))
        {
            List<E> weaponItems = pickupItems.RandomSelection(Generator, _weaponAllocations[levelName].Count, false, new List<E>() { unarmedPistols }.ToHashSet());
            for (int i = 0; i < weaponItems.Count; i++)
            {
                weaponItems[i].TypeID = _weaponAllocations[levelName][i];
            }

            pickupItems.RemoveAll(weaponItems.Contains);
        }

        foreach (E item in pickupItems)
        {
            T itemType = item.TypeID;

            if (isUnarmed && item == unarmedPistols)
            {
                // Enemy rando may have changed this already to something else and allocated
                // ammo to the inventory, so only change pistols.
                if (TypeMatch(itemType, pistols) && Settings.GiveUnarmedItems)
                {
                    item.TypeID = weaponTypes.RandomItem(Generator);
                }
            }
            else
            {
                // Only one pistol pickup per level, and only if it's unarmed
                do
                {
                    item.TypeID = stdItemTypes.RandomItem(Generator);
                }
                while (TypeMatch(item.TypeID, pistols) && (hasPistols || !isUnarmed));
            }

            hasPistols = pickupItems.Any(e => TypeMatch(e.TypeID, pistols));
        }
    }

    public void RandomizeItemLocations(string levelName, List<E> items, bool isUnarmed)
    {
        if (!Settings.RandomizeItemPositions)
        {
            return;
        }    

        List<T> stdItemTypes = GetStandardItemTypes();
        List<int> excludedItems = GetExcludedItems(levelName);
        E unarmedPistols = isUnarmed ? GetUnarmedLevelPistols(levelName, items) : null;

        for (int i = 0; i < items.Count; i++)
        {
            E entity = items[i];
            if (excludedItems.Contains(i)
                || !stdItemTypes.Contains(entity.TypeID)
                || entity == unarmedPistols
                || ItemFactory.IsItemLocked(levelName, i))
            {
                continue;
            }

            _picker.RandomizePickupLocation(entity);
            ItemMoved(entity);
        }
    }

    public void ShuffleItems(string levelName, List<E> items, bool isUnarmed, int levelSequence)
    {
        // Shuffle mode retains all item positions and types, but types are redistributed. This is done in two stages
        // to allow other mods to potentially change types based on original indices. The first stage means any additional
        // items created by mods will not be affected.
        List<ItemSwap> swapCache = new();
        _itemSwapCache ??= new();
        _itemSwapCache[levelName] = swapCache;

        List<E> allPickups = GetPickups(levelName, items, isUnarmed);
        List<T> stdItemTypes = GetStandardItemTypes();
        List<T> keyItemTypes = GetKeyItemTypes();

        List<T> usedStdTypes = new(allPickups.Where(e => stdItemTypes.Contains(e.TypeID)).Select(e => e.TypeID));
        List<E> keyItems = allPickups.FindAll(e => keyItemTypes.Contains(e.TypeID));

        void CacheSwap(E item, T newType)
        {
            swapCache.Add(new()
            {
                Index = items.IndexOf(item),
                OldType = item.TypeID,
                NewType = newType,
            });
            usedStdTypes.Remove(newType);
        }

        List<Location> usedLocations = new();
        bool IsUsed(Location location)
            => usedLocations.Any(l => l.IsEquivalent(location));

        foreach (E keyItem in keyItems)
        {
            if (!allPickups.Contains(keyItem))
            {
                continue;
            }

            // Try to avoid similar key types ending up in another's position.
            int keyItemID = (int)_picker.GetKeyItemID(levelSequence, keyItem);
            Location currentLocation = keyItem.GetLocation();

            List<E> validPool = new(allPickups.Where(e => _picker.IsValidKeyItemLocation(keyItemID, e.GetLocation())));
            if (validPool.Count == 0)
            {
                allPickups.Remove(keyItem);
                continue;
            }

            List<E> clashPool = new(validPool.Where(e => TypeMatch(keyItem, e) || e.GetLocation().IsEquivalent(currentLocation)));
            if (validPool.Except(clashPool).Any())
            {
                validPool.RemoveAll(clashPool.Contains);
            }

            E swapItem;
            Location location;
            do
            {
                swapItem = validPool[Generator.Next(0, validPool.Count)];
                location = swapItem.GetLocation();
            }
            while ((IsUsed(location) || keyItems.Contains(swapItem)) && !validPool.All(e => IsUsed(e.GetLocation()) || keyItems.Contains(e)));

            if (keyItems.Contains(swapItem))
            {
                // Instances where two keys can't be swapped and there are no other pickups before their max rooms.
                keyItemID = (int)_picker.GetKeyItemID(levelSequence, swapItem);
                if (!_picker.IsValidKeyItemLocation(keyItemID, currentLocation))
                {
                    allPickups.Remove(keyItem);
                    continue;
                }
            }

            if (keyItem != swapItem)
            {
                CacheSwap(keyItem, swapItem.TypeID);
                CacheSwap(swapItem, keyItem.TypeID);
            }

            allPickups.Remove(swapItem);
            allPickups.Remove(keyItem);
            usedLocations.Add(location);
        }

        foreach (E pickup in allPickups)
        {
            // Regular items need no placement checks.
            CacheSwap(pickup, usedStdTypes[Generator.Next(0, usedStdTypes.Count)]);
        }
    }

    public void ApplyItemSwaps(string levelName, List<E> items)
    {
        if (_itemSwapCache == null || !_itemSwapCache.ContainsKey(levelName))
        {
            return;
        }

        // Other mods may have already switched item types, so only apply those that are still valid.
        foreach (ItemSwap swap in _itemSwapCache[levelName])
        {
            E pickup = items[swap.Index];
            if (TypeMatch(pickup.TypeID, swap.OldType))
            {
                pickup.TypeID = swap.NewType;
            }
        }

        ExcludeEnemyKeyDrops(items);
    }

    protected List<E> GetPickups(string levelName, List<E> items, bool isUnarmed)
    {
        List<T> stdItemTypes = GetStandardItemTypes();
        List<T> keyItemTypes = GetKeyItemTypes();
        List<int> excludedItems = GetExcludedItems(levelName);
        E unarmedPistols = isUnarmed ? GetUnarmedLevelPistols(levelName, items) : null;

        return items.Where(e =>
            (stdItemTypes.Contains(e.TypeID) || keyItemTypes.Contains(e.TypeID))
            && !excludedItems.Contains(items.IndexOf(e))
            && e != unarmedPistols
            && !ItemFactory.IsItemLocked(levelName, items.IndexOf(e))).ToList();
    }

    public int? EnforceOneLimit(string levelName, List<E> items, bool isUnarmed)
    {
        if (Settings.RandoItemDifficulty != ItemDifficulty.OneLimit || Settings.ItemMode == ItemMode.Shuffled)
        {
            return null;
        }

        List<T> stdItemTypes = GetStandardItemTypes();
        List<int> excludedItems = GetExcludedItems(levelName);
        HashSet<T> uniqueTypes = new();
        if (isUnarmed)
        {
            // These will be excluded, but track their type before looking at other items.
            uniqueTypes.Add(GetPistolType());
        }

        // Look for extra utility/ammo items and hide them
        int hiddenCount = 0;
        E unarmedPistols = isUnarmed ? GetUnarmedLevelPistols(levelName, items) : null;

        for (int i = 0; i < items.Count; i++)
        {
            E entity = items[i];
            if (excludedItems.Contains(i) 
                || entity == unarmedPistols
                || ItemFactory.IsItemLocked(levelName, i))
            {
                continue;
            }

            if ((stdItemTypes.Contains(entity.TypeID) || IsCrystalPickup(entity.TypeID))
                && !uniqueTypes.Add(entity.TypeID))
            {
                ItemUtilities.HideEntity(entity);
                ItemFactory.FreeItem(levelName, i);
                hiddenCount++;
            }
        }

        return hiddenCount;
    }

    public void RandomizeSprites(TRDictionary<T, TRSpriteSequence> sequences, List<T> keyItemTypes, List<T> secretTypes)
    {
        if (!Settings.RandomizeItemSprites)
        {
            return;
        }

        _spriteRandomizer ??= new()
        {
            StandardItemTypes = GetStandardItemTypes(),
            KeyItemTypes = keyItemTypes,
            SecretItemTypes = secretTypes,
            RandomizeKeyItemSprites = Settings.RandomizeKeyItemSprites,
            RandomizeSecretSprites = Settings.RandomizeSecretSprites,
            Mode = Settings.SpriteRandoMode
        };

        _spriteRandomizer.Sequences = sequences;
        _spriteRandomizer.Randomize(Generator);
    }

    public void ExcludeEnemyKeyDrops(List<E> allItems)
    {
        List<T> enemyTypes = GetEnemyTypes();
        List<T> keyItemTypes = GetKeyItemTypes();
        IEnumerable<E> keyEnemies = allItems.Where(enemy => enemyTypes.Contains(enemy.TypeID)
            && allItems.Any(key => keyItemTypes.Contains(key.TypeID)
            && key.GetLocation().IsEquivalent(enemy.GetLocation()))
        );

        foreach (E enemy in keyEnemies)
        {
            enemy.X++;
        }
    }

    protected abstract List<T> GetStandardItemTypes();
    protected abstract List<T> GetWeaponItemTypes();
    protected abstract List<T> GetKeyItemTypes();
    protected abstract List<T> GetEnemyTypes();
    protected abstract T GetPistolType();
    public abstract List<int> GetExcludedItems(string levelName);
    protected abstract bool IsCrystalPickup(T type);
    protected virtual void ItemMoved(E item) { }

    protected class ItemSwap
    {
        public int Index { get; set; }
        public T OldType { get; set; }
        public T NewType { get; set; }
    }
}
