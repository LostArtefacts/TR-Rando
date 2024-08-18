using System.Diagnostics;
using TRGE.Core;
using TRLevelControl.Model;

namespace TRRandomizerCore.Processors;

public class BaseTRRSequenceProcessor<E, T>
    where E : TREntity<T>
    where T : Enum
{
    public Func<T, bool> IsMediType { get; set; }

    public void AdjustStrings(TRRScript script)
    {
        // 90% of key items will default to "Select Level", which itself it not used
        // anywhere else in the game. This makes it look a little better.
        script.CommonStrings["SELECTLEV"] = "Key Item";
    }

    public void AdjustMedipacks(TRRScriptedLevel levelScript, List<E> currentItems, List<E> originalItems,
        E dummyItem, FDControl floorData, List<int> freeIndices = null)
    {
        // In NG+, the game will convert medipacks to ammo, but this is based on the items' indices
        // in the level's original slot. So we need to guarantee that the items match up in the new
        // slot to avoid the wrong things being converted.
        if (levelScript.Sequence == levelScript.OriginalSequence)
        {
            return;
        }

        List<int> ogIndices = GetMediIndices(originalItems);
        Queue<int> swappableIndices = new(GetMediIndices(currentItems).Except(ogIndices));
        freeIndices?.ForEach(i => swappableIndices.Enqueue(i));

        foreach (int index in ogIndices)
        {
            while (currentItems.Count <= index)
            {
                currentItems.Add(dummyItem);
            }

            E entity = currentItems[index];
            if (IsMediType(entity.TypeID))
            {
                continue;
            }

            if (swappableIndices.Count == 0)
            {
                swappableIndices.Enqueue(currentItems.Count);
                currentItems.Add(dummyItem);
            }
            int swapIndex = swappableIndices.Dequeue();
            currentItems[index] = currentItems[swapIndex];
            currentItems[swapIndex] = entity;

            floorData.GetEntityActionItems(index)
                .ForEach(a => a.Parameter = (short)swapIndex);

            floorData.GetSwitchKeyTriggers(index)
                .ForEach(t => t.SwitchOrKeyRef = (short)swapIndex);
        }

        // Sanity check
        ogIndices.ForEach(i => Debug.Assert(currentItems[i] == dummyItem
            || IsMediType(currentItems[i].TypeID)));
    }

    private List<int> GetMediIndices(List<E> items)
    {
        return items.Where(e => IsMediType(e.TypeID))
            .Select(e => items.IndexOf(e))
            .ToList();
    }
}
