using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class SecretPicker
{
    public RandomizerSettings Settings { get; set; }
    public Random Generator { get; set; }
    public IMirrorControl Mirrorer { get; set; }
    public ItemFactory ItemFactory { get; set; }

    public Queue<Location> GetGuaranteedLocations(IEnumerable<Location> allLocations, bool isMirrored, int totalCount, Func<Location, bool> validCallback = null)
    {
        Queue<Location> locations = new();

        if (Settings.UseSecretPack)
        {
            List<Location> pool = allLocations
                .Where(l => l.PackID == Settings.SecretPack)
                .ToList();

            if (pool.Any(l => l.LevelState == LevelState.Mirrored)
                && pool.Any(l => l.LevelState == LevelState.NotMirrored))
            {
                // Invalid, authors should be aware of this. Default to removing all mirrored locations.
                pool.RemoveAll(l => l.LevelState == LevelState.Mirrored);
            }

            for (int i = 0; i < pool.Count && locations.Count < totalCount; i++)
            {
                locations.Enqueue(pool[i]);
            }

            if (locations.Count == totalCount)
            {
                return locations;
            }
        }

        if (Settings.GuaranteeSecrets)
        {
            int maxCount = Math.Max(1, (int)Math.Floor(totalCount / 2d)) + locations.Count;

            // Create location pools for the categories selected.
            List<IEnumerable<Location>> pools = new();
            if (Settings.HardSecrets)
            {
                pools.Add(FilterLocations(allLocations, isMirrored, Difficulty.Hard, false));
                if (Settings.GlitchedSecrets)
                {
                    pools.Add(FilterLocations(allLocations, isMirrored, Difficulty.Hard, true));
                }
            }
            if (Settings.GlitchedSecrets)
            {
                pools.Add(FilterLocations(allLocations, isMirrored, Difficulty.Easy, true));
            }

            pools.RemoveAll(p => !p.Any());
            if (pools.Count > 0)
            {
                // Select at least one pool.
                List<IEnumerable<Location>> selectionPools = pools.RandomSelection(Generator, Generator.Next(1, pools.Count + 1));
                selectionPools.Shuffle(Generator);

                // And pick one secret from each until we reach the desired count.
                foreach (IEnumerable<Location> pool in selectionPools)
                {
                    List<Location> poolList = pool.ToList();
                    poolList.Shuffle(Generator);
                    Location location;
                    do
                    {
                        // Pools should not overlap, but this covers potential duplicate data issues.
                        location = poolList[Generator.Next(0, poolList.Count)];
                    }
                    while (locations.Contains(location));

                    // Allow callers to perform their own specific validity checks.
                    if (validCallback?.Invoke(location) ?? true)
                    {
                        locations.Enqueue(location);
                    }

                    if (locations.Count == maxCount)
                    {
                        break;
                    }
                }
            }
        }

        return locations;
    }

    public void FinaliseSecretPool(IEnumerable<Location> usedLocations, string level)
    {
        // Secrets in packs are permitted to enforce level state
        if (usedLocations.Any(l => l.LevelState == LevelState.Mirrored))
        {
            Mirrorer.SetIsMirrored(level, true);
        }
        else if (usedLocations.Any(l => l.LevelState == LevelState.NotMirrored))
        {
            Mirrorer.SetIsMirrored(level, false);
        }

        foreach (Location location in usedLocations.Where(l => l.EntityIndex != -1))
        {
            // Indicates that a secret relies on another item to remain in its original position
            ItemFactory.LockItem(level, location.EntityIndex);
        }
    }

    private IEnumerable<Location> FilterLocations(IEnumerable<Location> locations, bool isMirrored, Difficulty difficulty, bool glitched)
    {
        return locations.Where
        (
            loc => loc.Difficulty == difficulty
                && loc.RequiresGlitch == glitched
                && (loc.LevelState == LevelState.Any
                || (isMirrored && loc.LevelState == LevelState.Mirrored)
                || (!isMirrored && loc.LevelState == LevelState.NotMirrored))
        );
    }

    public string DescribeLocations(IEnumerable<Location> locations)
    {
        int glitchlessEasy = FilterLocations(locations, false, Difficulty.Easy, false).Count();
        int glitchlessHard = FilterLocations(locations, false, Difficulty.Hard, false).Count();
        int glitchedEasy = FilterLocations(locations, false, Difficulty.Easy, true).Count();
        int glitchedHard = FilterLocations(locations, false, Difficulty.Hard, true).Count();
        return string.Format("{0} GSE; {1} GSH; {2} GDE; {3} GDH", glitchlessEasy, glitchlessHard, glitchedEasy, glitchedHard);
    }
}
