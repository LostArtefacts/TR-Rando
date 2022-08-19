using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TRLevelReader.Model;

namespace TRRandomizerCore.Helpers
{
    public class ItemFactory
    {
        private static readonly int _entityLimit = 256;

        private readonly Dictionary<string, List<int>> _reusableItemDefaults;
        private readonly Dictionary<string, Queue<int>> _availableItems;

        public ItemFactory(string dataPath)
        {
            _reusableItemDefaults = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(File.ReadAllText(dataPath));
            _availableItems = new Dictionary<string, Queue<int>>();
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

        public int GetNextIndex(string lvl, List<TREntity> allItems, bool allowLimitBreak = false)
        {
            return GetNextIndex(lvl, allItems.Count, allowLimitBreak);
        }

        public int GetNextIndex(string lvl, List<TR2Entity> allItems, bool allowLimitBreak = false)
        {
            return GetNextIndex(lvl, allItems.Count, allowLimitBreak);
        }

        public int GetNextIndex(string lvl, int totalItemCount, bool allowLimitBreak)
        {
            Queue<int> pool = GetItemPool(lvl);
            if (pool.Count > 0)
            {
                return pool.Peek();
            }

            return (totalItemCount < _entityLimit || allowLimitBreak) ? totalItemCount : -1;
        }

        public bool CanCreateItem(string lvl, List<TREntity> allItems, bool allowLimitBreak = false)
        {
            return GetNextIndex(lvl, allItems, allowLimitBreak) != -1;
        }

        public bool CanCreateItem(string lvl, List<TR2Entity> allItems, bool allowLimitBreak = false)
        {
            return GetNextIndex(lvl, allItems, allowLimitBreak) != -1;
        }

        public bool CanCreateItems(string lvl, List<TR2Entity> allItems, int count, bool allowLimitBreak = false)
        {
            int reusableCount = GetItemPool(lvl).Count;
            count -= Math.Min(count, reusableCount);
            return allItems.Count + count <= _entityLimit || allowLimitBreak;
        }

        public TREntity CreateItem(string lvl, List<TREntity> allItems, Location location = null, bool allowLimitBreak = false)
        {
            TREntity item;
            Queue<int> pool = GetItemPool(lvl);
            if (pool.Count > 0)
            {
                item = allItems[pool.Dequeue()];
            }
            else
            {
                if (allItems.Count < _entityLimit || allowLimitBreak)
                {
                    allItems.Add(item = new TREntity());
                }
                else
                {
                    return null;
                }
            }

            if (location != null)
            {
                item.X = location.X;
                item.Y = location.Y;
                item.Z = location.Z;
                item.Room = (short)location.Room;
                item.Angle = location.Angle;
            }

            // Set some defaults
            item.Intensity = 0;
            item.Flags = 0;
            return item;
        }

        public TR2Entity CreateItem(string lvl, List<TR2Entity> allItems, Location location = null, bool allowLimitBreak = false)
        {
            TR2Entity item;
            Queue<int> pool = GetItemPool(lvl);
            if (pool.Count > 0)
            {
                item = allItems[pool.Dequeue()];
            }
            else
            {
                if (allItems.Count < _entityLimit || allowLimitBreak)
                {
                    allItems.Add(item = new TR2Entity());
                }
                else
                {
                    return null;
                }
            }

            if (location != null)
            {
                item.X = location.X;
                item.Y = location.Y;
                item.Z = location.Z;
                item.Room = (short)location.Room;
                item.Angle = location.Angle;
            }

            // Set some defaults
            item.Intensity1 = item.Intensity2 = -1;
            item.Flags = 0;
            return item;
        }

        public void FreeItem(string lvl, int itemIndex)
        {
            GetItemPool(lvl).Enqueue(itemIndex);
        }
    }
}