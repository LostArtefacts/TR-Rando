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
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers
{
    public class TR3VfxRandomizer : BaseTR3Randomizer
    {
        private List<TR3ScriptedLevel> _filterLevels;

        private Color[] _colors;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            _colors = ColorUtilities.GetAvailableColors();

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
            if (Settings.VfxRoom)
            {
                //Change every room
                FilterVerticesRandomRoom(level.Data);
            }
            else if (Settings.VfxLevel)
            {
                //Change every level
                FilterVerticesRandomLevel(level.Data, _colors[_generator.Next(0, _colors.Length - 1)]);
            }
            else
            {
                //Normal filter
                FilterVertices(level.Data);
            } 

            if (level.HasCutScene)
            {
                SetVertexFilterMode(level.CutSceneLevel);
            }
        }

        private void FilterVertices(TR3Level level)
        {
            foreach (TR3Room room in level.Rooms)
            {
                room.SetColourFilter(Settings.VfxFilterColor, Settings.VfxVivid, Settings.VfxCaustics, Settings.VfxWave);
            }
        }

        private void FilterVerticesRandomLevel(TR3Level level, Color col)
        {
            foreach (TR3Room room in level.Rooms)
            {
                room.SetColourFilter(col, Settings.VfxVivid, Settings.VfxCaustics, Settings.VfxWave);
            }
        }

        private void FilterVerticesRandomRoom(TR3Level level)
        {
            foreach (TR3Room room in level.Rooms)
            {
                Color col = _colors[_generator.Next(0, _colors.Length - 1)];

                room.SetColourFilter(col, Settings.VfxVivid, Settings.VfxCaustics, Settings.VfxWave);
            }
        }
    }
}
