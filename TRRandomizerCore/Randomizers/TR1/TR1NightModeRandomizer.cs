using System;
using System.Collections.Generic;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers
{
    public class TR1NightModeRandomizer : BaseTR1Randomizer
    {
        public const uint DarknessRange = 10; // 0 = Dusk, 10 = Night

        private static readonly Dictionary<string, List<int>> _excludedRooms = new Dictionary<string, List<int>>
        {
            [TRLevelNames.ATLANTIS]
                = new List<int> { 85, 95, 96 } // We want to retain the flicker effect at the start
        };

        internal TR1TextureMonitorBroker TextureMonitor { get; set; }

        private List<TR1ScriptedLevel> _nightLevels;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            Settings.NightModeDarkness = Math.Min(Settings.NightModeDarkness, DarknessRange);

            ChooseNightLevels();

            foreach (TR1ScriptedLevel lvl in Levels)
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
            TR1ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TRLevelNames.ASSAULT));
            ISet<TR1ScriptedLevel> exlusions = new HashSet<TR1ScriptedLevel> { assaultCourse };

            _nightLevels = Levels.RandomSelection(_generator, (int)Settings.NightModeCount, exclusions: exlusions);
            if (Settings.NightModeAssaultCourse)
            {
                _nightLevels.Add(assaultCourse);
            }
        }

        private void SetNightMode(TR1CombinedLevel level)
        {
            DarkenRooms(level);

            if (level.HasCutScene)
            {
                SetNightMode(level.CutSceneLevel);
            }

            // Notify the texture monitor that this level is now in night mode
            TextureMonitor<TREntities> monitor = TextureMonitor.CreateMonitor(level.Name);
            monitor.UseNightTextures = true;
        }

        private void DarkenRooms(TR1CombinedLevel level)
        {
            double scale = (100 - DarknessRange + Settings.NightModeDarkness) / 100d;

            short intensity1 = (short)(TR2Room.DarknessIntensity1 * scale);
            ushort intensity2 = (ushort)(TR2Room.DarknessIntensity2 * (2 - scale));

            for (int i = 0; i < level.Data.NumRooms; i++)
            {
                if (_excludedRooms.ContainsKey(level.Name) && _excludedRooms[level.Name].Contains(i))
                {
                    continue;
                }

                TRRoom room = level.Data.Rooms[i];
                room.SetAmbient(intensity1);
                room.SetLights(intensity2);
                room.SetStaticMeshLights((ushort)intensity1);
                room.SetVertexLight(intensity1);
            }
        }
    }
}