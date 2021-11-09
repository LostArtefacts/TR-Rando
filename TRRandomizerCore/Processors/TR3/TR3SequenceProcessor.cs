using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors
{
    public class TR3SequenceProcessor : TR3LevelProcessor
    {
        private static readonly int _entityLimit = 256;

        private static readonly Dictionary<TR3Entities, TR3Entities> _artefactAssignment = new Dictionary<TR3Entities, TR3Entities>
        {
            [TR3Entities.Infada_P] = TR3Entities.Key1_P,
            [TR3Entities.OraDagger_P] = TR3Entities.Key2_P,
            [TR3Entities.EyeOfIsis_P] = TR3Entities.Key3_P,
            [TR3Entities.Element115_P] = TR3Entities.Key4_P,
            [TR3Entities.Infada_M_H] = TR3Entities.Key1_M_H,
            [TR3Entities.OraDagger_M_H] = TR3Entities.Key2_M_H,
            [TR3Entities.EyeOfIsis_M_H] = TR3Entities.Key3_M_H,
            [TR3Entities.Element115_M_H] = TR3Entities.Key4_M_H,
        };

        private Dictionary<string, List<Location>> _upvLocations;

        public void Run()
        {
            _upvLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\upv_locations.json"));

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                AdjustLevel(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void AdjustLevel(TR3CombinedLevel level)
        {
            if (level.HasExposureMeter)
            {
                if (!level.Is(TR3LevelNames.ANTARC) && !level.Is(TR3LevelNames.RXTECH))
                {
                    // The UPV keeps Lara warm underwater so make this available for
                    // those levels that have long underwater sections.
                    ImportUPV(level);
                }
            }

            if (level.Is(TR3LevelNames.WILLIE))
            {
                if (!level.IsWillardSequence)
                {
                    // Picking-up the artefacts will end the level for all sequences != 19 so
                    // re-assign the items as keys, which Cavern doesn't use.
                    AmendWillardBoss(level);
                }
            }
            else if (level.IsWillardSequence || (level.Is(TR3LevelNames.CITY) && !level.IsSophiaSequence))
            {
                // Because the stones don't end the level on sequence 19, make any required mods
                // to make end level triggers.
                // #231 the electric fields in City can't be triggered when it's off-sequence, so
                // the mods are applied in this instance too.
                AmendBossFight(level);
            }
        }

        private void ImportUPV(TR3CombinedLevel level)
        {
            if (!_upvLocations.ContainsKey(level.Name) || _upvLocations[level.Name].Count == 0)
            {
                return;
            }

            TR3ModelImporter importer = new TR3ModelImporter
            {
                Level = level.Data,
                LevelName = level.Name,
                EntitiesToImport = new List<TR3Entities> { TR3Entities.UPV },
                DataFolder = GetResourcePath(@"TR3\Models")
            };

            importer.Import();

            List<TR2Entity> entities = level.Data.Entities.ToList();
            foreach (Location location in _upvLocations[level.Name])
            {
                if (entities.Count == _entityLimit)
                {
                    break;
                }

                entities.Add(new TR2Entity
                {
                    TypeID = (short)TR3Entities.UPV,
                    Room = (short)location.Room,
                    X = location.X,
                    Y = location.Y,
                    Z = location.Z,
                    Angle = location.Angle,
                    Flags = 0,
                    Intensity1 = -1,
                    Intensity2 = -1
                });
            }

            level.Data.Entities = entities.ToArray();
            level.Data.NumEntities = (uint)entities.Count;
        }

        private void AmendWillardBoss(TR3CombinedLevel level)
        {
            List<TR2Entity> entities = level.Data.Entities.ToList();
            List<TRModel> models = level.Data.Models.ToList();

            // Add new duplicate models for keys, so secret rando doesn't replace the originals.
            foreach (TR3Entities artefact in _artefactAssignment.Keys)
            {
                TR3Entities replacement = _artefactAssignment[artefact];
                TRModel artefactModel = models.Find(m => m.ID == (uint)artefact);
                models.Add(new TRModel
                {
                    Animation = artefactModel.Animation,
                    FrameOffset = artefactModel.FrameOffset,
                    ID = (uint)replacement,
                    MeshTree = artefactModel.MeshTree,
                    NumMeshes = artefactModel.NumMeshes,
                    StartingMesh = artefactModel.StartingMesh
                });

                entities.FindAll(e => e.TypeID == (short)artefact).ForEach(e => e.TypeID = (short)replacement);
            }

            // Copy the artefact names into the keys
            for (int i = 0; i < 4; i++)
            {
                level.Script.Keys[i] = ScriptEditor.Script.GameStrings1[80 + i];
            }

            level.Data.Models = models.ToArray();
            level.Data.NumModels = (uint)models.Count;

            // Apply any changes needed for the boss fight
            AmendBossFight(level);
        }

        private void AmendBossFight(TR3CombinedLevel level)
        {
            string mappingPath = @"TR3\BossMapping\" + level.Name + "-BossMapping.json";
            if (ResourceExists(mappingPath))
            {
                EMEditorSet mods = JsonConvert.DeserializeObject<EMEditorSet>(ReadResource(mappingPath), EMEditorMapping.Converter);
                mods.ApplyToLevel(level.Data);
            }
        }
    }
}