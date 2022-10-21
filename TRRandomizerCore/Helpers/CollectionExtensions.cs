using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace TRRandomizerCore.Helpers
{
    public static class CollectionExtensions
    {
        public static List<T> RandomSelection<T>(this List<T> list, Random rand, int count, bool allowDuplicates = false, ISet<T> exclusions = null)
        {
            count = Math.Abs(count);
            if (count > list.Count && !allowDuplicates)
            {
                throw new ArgumentException(string.Format("The given count ({0}) is larger than that of the provided list ({1}).", count, list.Count));
            }

            if (count == list.Count)
            {
                return list;
            }

            List<T> iterList = new List<T>(list);
            if (exclusions != null && exclusions.Count > 0)
            {
                foreach (T excludeItem in exclusions)
                {
                    iterList.Remove(excludeItem);
                }
            }

            List<T> resultSet = new List<T>();
            if (iterList.Count > 0)
            {
                int maxIter = allowDuplicates ? count : Math.Min(count, iterList.Count);
                for (int i = 0; i < maxIter; i++)
                {
                    T item;
                    do
                    {
                        item = iterList[rand.Next(0, iterList.Count)];
                    }
                    while (!allowDuplicates && resultSet.Contains(item));
                    resultSet.Add(item);
                }
            }

            return resultSet;
        }

        public static void Shuffle<T>(this List<T> list, Random rand)
        {
            List<T> iterList = new List<T>(list);
            list.Clear();

            int count = iterList.Count;
            while (list.Count < count)
            {
                T item = iterList[rand.Next(0, iterList.Count)];
                list.Add(item);
                iterList.Remove(item);
            }
        }
    }
}