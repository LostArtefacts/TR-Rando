using System.Diagnostics;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class SecretPicker<T>
    where T : class, ITREntity, new()
{
    private const int _maxRetryLimit = 500;
    private const float _distanceStep = 1.0f;
    private const float _minDistanceDivisor = _distanceStep * 3;
    private static readonly List<int> _retryTolerances = new()
    {
        10, 25, 50
    };

    private int _proxEvaluationCount;
    private float _distanceDivisor;

    public RandomizerSettings Settings { get; set; }
    public Random Generator { get; set; }
    public IMirrorControl Mirrorer { get; set; }
    public ItemFactory<T> ItemFactory { get; set; }
    public IRouteManager RouteManager { get; set; }
    public Func<Location, TRRoomSector> SectorAction { get; set; }
    public Func<Location, bool> PlacementTestAction { get; set; }

    public List<Location> GetLocations(List<Location> allLocations, bool isMirrored, int totalCount)
    {
        ResetEvaluators(totalCount);

        List<Location> locations = new();
        Queue<Location> guaranteedLocations = GetGuaranteedLocations(allLocations, isMirrored, totalCount, location =>
        {
            bool result = TestLocation(location, locations, isMirrored, totalCount);
            if (result)
            {
                locations.Add(location);
            }
            return result;
        });

        for (int i = 0; i < totalCount; i++)
        {
            Location location;
            if (guaranteedLocations.Count > 0)
            {
                location = guaranteedLocations.Dequeue();
            }
            else
            {
                do
                {
                    location = allLocations[Generator.Next(0, allLocations.Count)];
                }
                while (!TestLocation(location, locations, isMirrored, totalCount));
            }

            if (!locations.Contains(location))
            {
                locations.Add(location);
            }
        }

        return locations;
    }

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

    public bool TestLocation(Location location, List<Location> usedLocations, bool isMirrored, int totalCount)
    {
        if (location.Difficulty == Difficulty.Hard && !Settings.HardSecrets)
        {
            return false;
        }

        if (location.RequiresGlitch && !Settings.GlitchedSecrets)
        {
            return false;
        }

        if (isMirrored && location.LevelState == LevelState.NotMirrored)
        {
            return false;
        }

        if (!isMirrored && location.LevelState == LevelState.Mirrored)
        {
            return false;
        }

        if (!PlacementTestAction?.Invoke(location) ?? false)
        {
            return false;
        }

        if (usedLocations.Count == 0 || usedLocations == null)
        {
            ResetEvaluators(totalCount);
            return true;
        }

        _proxEvaluationCount++;
        if (_retryTolerances.Contains(_proxEvaluationCount) || _proxEvaluationCount > _retryTolerances.Max())
        {
            _distanceDivisor += _distanceStep;
        }

        float minProximity = RouteManager.LevelSize / _distanceDivisor;

        TRRoomSector newSector = SectorAction(location);
        foreach (Location used in usedLocations)
        {
            int proximity = RouteManager.GetProximity(location, used);
            if ((proximity >= 0 && proximity < minProximity)
                || newSector == SectorAction(used)
                || (used.Room == location.Room && _proxEvaluationCount < _maxRetryLimit))
            {
                return false;
            }
        }

        ResetEvaluators(totalCount);
        return true;
    }

    private void ResetEvaluators(int totalCount)
    {
        _proxEvaluationCount = 0;
        _distanceDivisor = Math.Max(_minDistanceDivisor, totalCount);
    }

    public void FinaliseSecretPool(IEnumerable<Location> usedLocations, string level, Func<int, List<int>> lockedItemAction)
    {
        // Secrets in packs are permitted to enforce level state
        if (usedLocations.Any(l => l.LevelState == LevelState.Mirrored))
        {
            Mirrorer?.SetIsMirrored(level, true);
        }
        else if (usedLocations.Any(l => l.LevelState == LevelState.NotMirrored))
        {
            Mirrorer?.SetIsMirrored(level, false);
        }

        foreach (Location location in usedLocations.Where(l => l.EntityIndex != -1))
        {
            // Indicates that a secret relies on another item to remain in its original position
            foreach (int itemIndex in lockedItemAction(location.EntityIndex))
            {
                ItemFactory.LockItem(level, itemIndex);
            }
        }

#if DEBUG
        Debug.WriteLine(level + ": " + DescribeLocations(usedLocations));
#endif
    }

    private static IEnumerable<Location> FilterLocations(IEnumerable<Location> locations, bool isMirrored, Difficulty difficulty, bool glitched)
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

    public static string DescribeLocations(IEnumerable<Location> locations)
    {
        int glitchlessEasy = FilterLocations(locations, false, Difficulty.Easy, false).Count();
        int glitchlessHard = FilterLocations(locations, false, Difficulty.Hard, false).Count();
        int glitchedEasy = FilterLocations(locations, false, Difficulty.Easy, true).Count();
        int glitchedHard = FilterLocations(locations, false, Difficulty.Hard, true).Count();
        return string.Format("{0} GSE; {1} GSH; {2} GDE; {3} GDH", glitchlessEasy, glitchlessHard, glitchedEasy, glitchedHard);
    }
}
