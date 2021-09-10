using System;
using System.Collections.Generic;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2RandomizerCore.Randomizers
{
    public class NightModeRandomizer : RandomizerBase
    {
        public const uint DarknessRange = 10; // 0 = Dusk, 10 = Night

        public uint NumLevels { get; set; }
        public uint DarknessScale { get; set; }
        public bool NightModeAssaultCourse { get; set; }

        internal TexturePositionMonitorBroker TextureMonitor { get; set; }

        private List<TR23ScriptedLevel> _nightLevels;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            DarknessScale = Math.Min(DarknessScale, DarknessRange);

            ChooseNightLevels();

            foreach (TR23ScriptedLevel lvl in Levels)
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
            TR23ScriptedLevel assaultCourse = Levels.Find(l => l.Is(LevelNames.ASSAULT));
            ISet<TR23ScriptedLevel> exlusions = new HashSet<TR23ScriptedLevel> { assaultCourse };

            _nightLevels = Levels.RandomSelection(_generator, (int)NumLevels, exclusions: exlusions);
            if (NightModeAssaultCourse)
            {
                _nightLevels.Add(assaultCourse);
            }
        }

        private void SetNightMode(TR2CombinedLevel level)
        {
            DarkenRooms(level.Data);
            HideDaytimeEntities(level.Data, level.Name);

            if (level.HasCutScene)
            {
                SetNightMode(level.CutSceneLevel);
            }

            // Notify the texture monitor that this level is now in night mode
            TexturePositionMonitor monitor = TextureMonitor.CreateMonitor(level.Name);
            monitor.UseNightTextures = true;
        }

        private void DarkenRooms(TR2Level level)
        {
            double scale = (100 - DarknessRange + DarknessScale) / 100d;

            short intensity1 = (short)(TR2Room.DarknessIntensity1 * scale);
            ushort intensity2 = (ushort)(TR2Room.DarknessIntensity2 * (2 - scale));

            foreach (TR2Room room in level.Rooms)
            {
                room.SetAmbient(intensity1);
                room.SetLights(intensity2);
                room.SetStaticMeshLights((ushort)intensity1);
                room.SetVertexLight(intensity1);
            }
        }

        private void HideDaytimeEntities(TR2Level level, string levelName)
        {
            // Replace any entities that don't "make sense" at night
            List<TR2Entity> entities = level.Entities.ToList();

            // A list of item locations to choose from
            List<TR2Entity> items = entities.Where
            (
                e =>
                    TR2EntityUtilities.IsAmmoType((TR2Entities)e.TypeID) ||
                    TR2EntityUtilities.IsGunType((TR2Entities)e.TypeID) ||
                    TR2EntityUtilities.IsUtilityType((TR2Entities)e.TypeID)
            ).ToList();

            foreach (TR2Entities entityToReplace in _entitiesToReplace.Keys)
            {
                IEnumerable<TR2Entity> ents = entities.Where(e => e.TypeID == (short)entityToReplace);
                foreach (TR2Entity entity in ents)
                {
                    TR2Entity item = items[_generator.Next(0, items.Count)];
                    entity.TypeID = (short)_entitiesToReplace[entityToReplace];
                    entity.Room = item.Room;
                    entity.X = item.X;
                    entity.Y = item.Y;
                    entity.Z = item.Z;
                    entity.Intensity1 = item.Intensity1;
                    entity.Intensity2 = item.Intensity2;
                }
            }

            // Hide any static meshes
            if (_staticMeshesToHide.ContainsKey(levelName))
            {
                List<TRStaticMesh> staticMeshes = level.StaticMeshes.ToList();
                foreach (uint meshID in _staticMeshesToHide[levelName])
                {
                    TRStaticMesh mesh = staticMeshes.Find(m => m.ID == meshID);
                    if (mesh != null)
                    {
                        mesh.NonCollidable = true;
                        mesh.Visible = false;
                    }
                }
            }
        }

        private static readonly Dictionary<TR2Entities, TR2Entities> _entitiesToReplace = new Dictionary<TR2Entities, TR2Entities>
        {
            [TR2Entities.SingingBirds_N] = TR2Entities.Flares_S_P // Birds don't sing at night
        };

        private static readonly Dictionary<string, uint[]> _staticMeshesToHide = new Dictionary<string, uint[]>
        {
            // The washing lines come in at night
            [LevelNames.VENICE] = 
                new uint[] { 32, 33 },
            // The monks are washing their prayer flags
            [LevelNames.MONASTERY] = 
                new uint[] { 36 }
        };
    }
}