using Newtonsoft.Json;
using TRLevelControl.Model;

namespace TRRandomizerCore.Helpers;

public class ItemFactory<T>
    where T : class, ITREntity, new()
{
    private static readonly int _entityLimit = 256;

    private readonly Dictionary<string, List<int>> _reusableItemDefaults;
    private readonly Dictionary<string, Queue<int>> _availableItems;
    private readonly Dictionary<string, HashSet<int>> _lockedItems;

    public T DefaultItem { get; set; }

    public ItemFactory(string dataPath = null)
    {
        _reusableItemDefaults = dataPath == null
            ? new()
            : JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(File.ReadAllText(dataPath));
        _availableItems = new();
        _lockedItems = new();
    }

    public Queue<int> GetItemPool(string lvl)
    {
        if (!_availableItems.ContainsKey(lvl))
        {
            if (_reusableItemDefaults.ContainsKey(lvl))
            {
                _availableItems[lvl] = new Queue<int>(_reusableItemDefaults[lvl]);
            }
            else
            {
                _availableItems[lvl] = new Queue<int>();
            }
        }
        return _availableItems[lvl];
    }

    public int GetNextIndex(string lvl, List<T> allItems, bool allowLimitBreak = false)
    {
        Queue<int> pool = GetItemPool(lvl);
        if (pool.Count > 0)
        {
            return pool.Peek();
        }

        return (allItems.Count < _entityLimit || allowLimitBreak) ? allItems.Count : -1;
    }

    public bool CanCreateItem(string lvl, List<T> allItems, bool allowLimitBreak = false)
    {
        return GetNextIndex(lvl, allItems, allowLimitBreak) != -1;
    }

    public bool CanCreateItems(string lvl, List<T> allItems, int count, bool allowLimitBreak = false)
    {
        int reusableCount = GetItemPool(lvl).Count;
        count -= Math.Min(count, reusableCount);
        return allItems.Count + count <= _entityLimit || allowLimitBreak;
    }

    public T CreateLockedItem(string lvl, List<T> allItems, Location location = null, bool allowLimitBreak = false)
    {
        T entity = CreateItem(lvl, allItems, location, allowLimitBreak);
        if (entity != null)
        {
            LockItem(lvl, allItems.IndexOf(entity));
        }
        return entity;
    }

    public T CreateItem(string lvl, List<T> allItems, Location location = null, bool allowLimitBreak = false)
    {
        T item = (T)DefaultItem?.Clone() ?? new();
        Queue<int> pool = GetItemPool(lvl);
        if (pool.Count > 0)
        {
            allItems[pool.Dequeue()] = item;
        }
        else if (allItems.Count < _entityLimit || allowLimitBreak)
        {
            allItems.Add(item);
        }
        else
        {
            return null;
        }

        if (location != null)
        {
            item.X = location.X;
            item.Y = location.Y;
            item.Z = location.Z;
            item.Room = (short)location.Room;
            item.Angle = location.Angle;
        }

        return item;
    }

    public void FreeItem(string lvl, int itemIndex)
    {
        GetItemPool(lvl).Enqueue(itemIndex);
    }

    public void LockItem(string level, int itemIndex)
    {
        if (!_lockedItems.ContainsKey(level))
        {
            _lockedItems[level] = new();
        }
        _lockedItems[level].Add(itemIndex);
    }

    public bool IsItemLocked(string level, int itemIndex)
    {
        return _lockedItems.ContainsKey(level) && _lockedItems[level].Contains(itemIndex);
    }
}
