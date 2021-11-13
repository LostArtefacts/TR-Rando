using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRGE.Core;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers.TR3
{
    public class TR3ItemRandomizer : BaseTR3Randomizer
    {
        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                if (Settings.RandomizeItemTypes)
                    RandomizeItemTypes(_levelInstance);

                if (Settings.RandomizeItemPositions)
                    RandomizeItemLocations(_levelInstance);

                if (Settings.IncludeKeyItems)
                    RandomizeKeyItems(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        public void RandomizeItemTypes(TR3CombinedLevel level)
        {

        }

        public void RandomizeItemLocations(TR3CombinedLevel level)
        {

        }

        public void RandomizeKeyItems(TR3CombinedLevel level)
        {

        }
    }
}
