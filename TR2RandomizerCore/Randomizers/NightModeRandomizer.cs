using System;
using System.Collections.Generic;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2RandomizerCore.Randomizers
{
    public class NightModeRandomizer : RandomizerBase
    {
        public uint NumLevels { get; set; }

        private List<string> _nightLevels;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            ChooseNightLevels();

            foreach (TR23ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                if (_nightLevels.Contains(_levelInstance.Name))
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
            _nightLevels = new List<string>();
            while (_nightLevels.Count < NumLevels)
            {
                TR23ScriptedLevel level = Levels[_generator.Next(0, Levels.Count)];
                if (level.Is(LevelNames.ASSAULT))
                {
                    continue;
                }

                string id = level.LevelFileBaseName.ToUpper();
                if (!_nightLevels.Contains(id))
                {
                    _nightLevels.Add(id);
                }
            }
        }

        private void SetNightMode(TR2CombinedLevel level)
        {
            foreach (TR2Room room in level.Data.Rooms)
            {
                room.Darken();
            }

            // Replace any entities that don't "make sense" at night
            List<TR2Entity> entities = level.Data.Entities.ToList();

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
            if (_staticMeshesToHide.ContainsKey(level.Name))
            {
                List<TRStaticMesh> staticMeshes = level.Data.StaticMeshes.ToList();
                foreach (uint meshID in _staticMeshesToHide[level.Name])
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