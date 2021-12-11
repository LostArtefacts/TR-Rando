using System;
using System.Collections.Generic;
using System.Drawing;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers
{
    public class TR3VfxRandomizer : BaseTR3Randomizer
    {
        private List<TR3ScriptedLevel> _filterLevels;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            ChooseFilterLevels();

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                if (_filterLevels.Contains(lvl))
                {
                    SetVertexFilterMode(_levelInstance);
                    SaveLevelInstance();
                }

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void ChooseFilterLevels()
        {
            TR3ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR3LevelNames.ASSAULT));
            ISet<TR3ScriptedLevel> exlusions = new HashSet<TR3ScriptedLevel> { assaultCourse };

            _filterLevels = Levels.RandomSelection(_generator, (int)Settings.NightModeCount, exclusions: exlusions);
            if (Settings.NightModeAssaultCourse)
            {
                _filterLevels.Add(assaultCourse);
            }
        }

        private void SetVertexFilterMode(TR3CombinedLevel level)
        {
            FilterVertices(level.Data);

            if (level.HasCutScene)
            {
                SetVertexFilterMode(level.CutSceneLevel);
            }
        }

        private void FilterVertices(TR3Level level)
        {
            foreach (TR3Room room in level.Rooms)
            {
                room.SetColourFilter(Settings.VfxFilterColor);
            }
        }
    }
}
