namespace TRRandomizerCore.Helpers;

public static class CollectionExtensions
{
    private const int _defaultShuffleCount = 5;

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

        List<T> iterList = new(list);
        if (exclusions != null && exclusions.Count > 0)
        {
            foreach (T excludeItem in exclusions)
            {
                iterList.Remove(excludeItem);
            }
        }

        List<T> resultSet = new();
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

    public static void Shuffle<T>(this List<T> list, Random rand, int count = _defaultShuffleCount)
    {
        for (int i = 0; i < count; i++)
        {
            ShuffleImpl(list, rand);
        }
    }

    private static void ShuffleImpl<T>(List<T> list, Random rand)
    {
        List<T> iterList = new(list);
        list.Clear();

        int count = iterList.Count;
        while (list.Count < count)
        {
            T item = iterList[rand.Next(0, iterList.Count)];
            list.Add(item);
            iterList.Remove(item);
        }
    }

    public static List<List<T>> Split<T>(this List<T> list, int parts)
    {
        int boundary = (int)Math.Ceiling(list.Count / (double)parts);
        List<List<T>> splits = new();
        for (int i = 0; i < list.Count; i++)
        {
            if (i % boundary == 0)
            {
                splits.Add(new());
            }
            splits[^1].Add(list[i]);
        }
        return splits;
    }

    public static List<T>[] Cluster<T>(this IEnumerable<T> list, int clusterCount)
    {
        List<T>[] clusters = new List<T>[clusterCount];
        for (int i = 0; i < clusterCount; i++)
        {
            clusters[i] = new();
        }

        int j = 0;
        foreach (T item in list)
        {
            clusters[j++ % clusterCount].Add(item);
        }
        return clusters;
    }
}
