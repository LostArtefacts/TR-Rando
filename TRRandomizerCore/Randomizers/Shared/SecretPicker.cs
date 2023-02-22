using System;
using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers
{
    public class SecretPicker
    {
        public RandomizerSettings Settings { get; set; }
        public Random Generator { get; set; }

        public Queue<Location> GetGuaranteedLocations(IEnumerable<Location> allLocations, bool isMirrored, int totalCount, Func<Location, bool> validCallback = null)
        {
            Queue<Location> locations = new Queue<Location>();

            if (Settings.GuaranteeSecrets)
            {
                int maxCount = Math.Max(1, (int)Math.Floor(totalCount / 2d));

                // Create location pools for the categories selected.
                List<IEnumerable<Location>> pools = new List<IEnumerable<Location>>();
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

                pools.RemoveAll(p => p.Count() == 0);
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
}
