using Newtonsoft.Json;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public abstract class ItemAllocator<T, E>
    where T : Enum
    where E : TREntity<T>, new()
{
    protected readonly Dictionary<string, List<Location>> _excludedLocations;
    protected readonly Dictionary<string, List<Location>> _pistolLocations;
    protected readonly Dictionary<string, E> _unarmedPistolCache;
    protected readonly LocationPicker _picker;

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
    }

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
                (weaponTypes.Contains(e.TypeID) || EqualityComparer<T>.Default.Equals(e.TypeID, pistols))
                && _pistolLocations[levelName].Any(l => l.IsEquivalent(e.GetLocation())));
        }
        
        return _unarmedPistolCache[levelName];
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
        List<int> excludedItems = GetExcludedItems(levelName);

        bool hasPistols = items.Any(e => EqualityComparer<T>.Default.Equals(e.TypeID, pistols));
        E unarmedPistols = isUnarmed ? GetUnarmedLevelPistols(levelName, items) : null;

        for (int i = 0; i < items.Count; i++)
        {
            if (excludedItems.Contains(i))
            {
                continue;
            }

            E entity = items[i];
            T entityType = entity.TypeID;

            if (isUnarmed && entity == unarmedPistols)
            {
                // Enemy rando may have changed this already to something else and allocated
                // ammo to the inventory, so only change pistols.
                if (EqualityComparer<T>.Default.Equals(entityType, pistols) && Settings.GiveUnarmedItems)
                {
                    do
                    {
                        entityType = stdItemTypes[Generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!weaponTypes.Contains(entityType));
                    entity.TypeID = entityType;
                }
            }
            else if (stdItemTypes.Contains(entityType))
            {
                T newType = stdItemTypes[Generator.Next(0, stdItemTypes.Count)];
                if (EqualityComparer<T>.Default.Equals(newType, pistols) && (hasPistols || !isUnarmed))
                {
                    // Only one pistol pickup per level, and only if it's unarmed
                    do
                    {
                        newType = stdItemTypes[Generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!weaponTypes.Contains(newType) || EqualityComparer<T>.Default.Equals(newType, pistols));
                }
                entity.TypeID = newType;
            }

            hasPistols = items.Any(e => EqualityComparer<T>.Default.Equals(e.TypeID, pistols));
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

    public int? EnforceOneLimit(string levelName, List<E> items, bool isUnarmed)
    {
        if (Settings.RandoItemDifficulty != ItemDifficulty.OneLimit)
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

    protected abstract List<T> GetStandardItemTypes();
    protected abstract List<T> GetWeaponItemTypes();
    protected abstract T GetPistolType();
    protected abstract List<int> GetExcludedItems(string levelName);
    protected abstract bool IsCrystalPickup(T type);
    protected virtual void ItemMoved(E item) { }
}
