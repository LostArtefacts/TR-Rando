using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers
{
    public class TR3NightModeRandomizer : BaseTR3Randomizer
    {
        public const uint DarknessRange = 10; // 0 = Dusk, 10 = Night

        private List<TR3ScriptedLevel> _nightLevels;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            Settings.NightModeDarkness = Math.Min(Settings.NightModeDarkness, DarknessRange);

            ChooseNightLevels();

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                if (_nightLevels.Contains(lvl))
                {
                    SetNightMode(_levelInstance);
                    SaveLevelInstance();
                }

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void ChooseNightLevels()
        {
            TR3ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR3LevelNames.ASSAULT));
            ISet<TR3ScriptedLevel> exlusions = new HashSet<TR3ScriptedLevel> { assaultCourse };

            _nightLevels = Levels.RandomSelection(_generator, (int)Settings.NightModeCount, exclusions: exlusions);
            if (Settings.NightModeAssaultCourse)
            {
                _nightLevels.Add(assaultCourse);
            }
        }

        private void SetNightMode(TR3CombinedLevel level)
        {
            DarkenRooms(level.Data);

            if (level.HasCutScene)
            {
                SetNightMode(level.CutSceneLevel);
            }
        }

        private void DarkenRooms(TR3Level level)
        {
            foreach (TR3Room room in level.Rooms)
            {
                room.SetVertexLight((short)(Settings.NightModeDarkness * 10));
            }
        }
    }
}
