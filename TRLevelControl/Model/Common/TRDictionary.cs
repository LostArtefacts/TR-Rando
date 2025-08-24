namespace TRLevelControl.Model;

public class TRDictionary<TKey, TValue> : SortedDictionary<TKey, TValue>
    where TValue : class
{
    public new TValue this[TKey key]
    {
        get => ContainsKey(key) ? base[key] : null;
        set => base[key] = value;
    }

    public bool ChangeKey(TKey oldKey, TKey newKey)
    {
        if (!TryGetValue(oldKey, out TValue value))
        {
            return false;
        }

        Remove(oldKey);
        base[newKey] = value;
        return true;
    }

    public void RemoveAll(Func<TKey, bool> predicate)
    {
        Keys.Where(predicate)
            .ToList()
            .ForEach(r => Remove(r));
    }
}
