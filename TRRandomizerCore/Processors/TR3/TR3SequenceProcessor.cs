using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor;
using TREnvironmentEditor.Helpers;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Types;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Processors
{
    public class TR3SequenceProcessor : TR3LevelProcessor
    {
        private static readonly int _spikeHeightChange = -768;

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

        private static readonly Dictionary<TR3Adventure, int> _adventureStringSequences = new Dictionary<TR3Adventure, int>
        {
            [TR3Adventure.SouthPacific] = 87,
            [TR3Adventure.London] = 85,
            [TR3Adventure.Nevada] = 86,
            [TR3Adventure.Antarctica] = 88
        };

        private Dictionary<string, List<Location>> _upvLocations;

        private Dictionary<TR3Adventure, string> _adventureNames;
        private List<string> _gameStrings;

        public GlobeDisplayOption GlobeDisplay { get; set; }
        public TR3TextureMonitorBroker TextureMonitor { get; set; }
        public ItemFactory ItemFactory { get; set; }

        public void Run()
        {
            _upvLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\upv_locations.json"));

            _gameStrings = new List<string>(ScriptEditor.Script.GameStrings1);
            _adventureNames = new Dictionary<TR3Adventure, string>
            {
                [TR3Adventure.India] = "India" // Not stored in script
            };

            foreach (TR3Adventure sequence in _adventureStringSequences.Keys)
            {
                _adventureNames[sequence] = ScriptEditor.Script.GameStrings1[_adventureStringSequences[sequence]];
            }

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

            ScriptEditor.Script.GameStrings1 = _gameStrings.ToArray();
            SaveScript();
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
            else if (level.Is(TR3LevelNames.ANTARC))
            {
                // Add a KillLara FD entry to room 185, where she would normally freeze
                // to death anyway.
                AmendAntarctica(level);
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
            else if ((level.Is(TR3LevelNames.COASTAL) && !level.IsCoastalSequence) || (level.Is(TR3LevelNames.MADUBU) && !level.IsMadubuSequence))
            {
                // Coastal Village and Madubu spikes are raised on initialisation in the game, based
                // on the level sequencing. So if out of sequence, perform the raising here.
                AmendSouthPacificSpikes(level);
            }

            // #277 Make sure levels have artefact menu models because these vary based on original sequencing.
            ImportArtefactMenuModels(level);

            // If this level is the first in an adventure, update the globe string to match
            if (_adventureStringSequences.ContainsKey((TR3Adventure)level.Sequence))
            {
                switch (GlobeDisplay)
                {
                    case GlobeDisplayOption.Area:
                        _gameStrings[_adventureStringSequences[(TR3Adventure)level.Sequence]] = _adventureNames[level.Adventure];
                        break;
                    case GlobeDisplayOption.Level:
                        _gameStrings[_adventureStringSequences[(TR3Adventure)level.Sequence]] = level.Script.Name;
                        break;
                }
            }
        }

        private void ImportUPV(TR3CombinedLevel level)
        {
            if (!_upvLocations.ContainsKey(level.Name) || _upvLocations[level.Name].Count == 0)
            {
                return;
            }

            List<TR3Entities> upvImport = new List<TR3Entities> { TR3Entities.UPV };
            TR3ModelImporter importer = new TR3ModelImporter
            {
                Level = level.Data,
                LevelName = level.Name,
                EntitiesToImport = upvImport,
                DataFolder = GetResourcePath(@"TR3\Models"),
                TexturePositionMonitor = TextureMonitor.CreateMonitor(level.Name, upvImport)
            };

            importer.Import();

            List<TR2Entity> entities = level.Data.Entities.ToList();
            foreach (Location location in _upvLocations[level.Name])
            {
                TR2Entity entity = ItemFactory.CreateItem(level.Name, entities, location);
                if (entity == null)
                {
                    break;
                }

                entity.TypeID = (short)TR3Entities.UPV;
            }

            level.Data.Entities = entities.ToArray();
            level.Data.NumEntities = (uint)entities.Count;

            // We can only have one vehicle type per level because LaraVehicleAnimation_H is tied to
            // each, so for the likes of Nevada, replace the quad with another UPV to fly into HSC.
            List<TR2Entity> quads = entities.FindAll(e => e.TypeID == (short)TR3Entities.Quad);
            foreach (TR2Entity quad in quads)
            {
                quad.TypeID = (short)TR3Entities.UPV;
            }
        }

        private void AmendAntarctica(TR3CombinedLevel level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);
            TRRoomSector sector = FDUtilities.GetRoomSector(53760, -3328, 28160, 185, level.Data, floorData);
            if (sector.FDIndex == 0)
            {
                floorData.CreateFloorData(sector);
            }

            floorData.Entries[sector.FDIndex].Add(new FDKillLaraEntry
            {
                Setup = new FDSetup(FDFunctions.KillLara)
            });

            floorData.WriteToLevel(level.Data);
        }

        private void ImportArtefactMenuModels(TR3CombinedLevel level)
        {
            List<TRModel> models = level.Data.Models.ToList();
            List<TR3Entities> imports = new List<TR3Entities>();
            foreach (TR3Entities artefactMenuModel in TR3EntityUtilities.GetArtefactMenuModels())
            {
                if (models.Find(m => m.ID == (uint)artefactMenuModel) == null)
                {
                    imports.Add(artefactMenuModel);
                }
            }

            if (imports.Count > 0)
            {
                TR3ModelImporter importer = new TR3ModelImporter
                {
                    Level = level.Data,
                    LevelName = level.Name,
                    EntitiesToImport = imports,
                    DataFolder = GetResourcePath(@"TR3\Models")
                };

                importer.Import();
            }
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

            // Hide the old Willie AI pathing
            for (int i = 0; i < level.Data.NumEntities; i++)
            {
                TR2Entity entity = level.Data.Entities[i];
                TR3Entities type = (TR3Entities)entity.TypeID;
                if (type == TR3Entities.AIPath_N || type == TR3Entities.AICheck_N)
                {
                    entity.TypeID = (short)TR3Entities.PistolAmmo_M_H;
                    entity.X = 66048;
                    entity.Y = 768;
                    entity.Z = 67072;
                    entity.Room = 5;
                    ItemUtilities.HideEntity(entity);
                    ItemFactory.FreeItem(level.Name, i);
                }
            }
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

        private void AmendSouthPacificSpikes(TR3CombinedLevel level)
        {
            short spikes = (short)TR3Entities.TeethSpikesOrBarbedWire;
            List<TR2Entity> entities = level.Data.Entities.ToList().FindAll(e => e.TypeID == spikes);
            foreach (TR2Entity entity in entities)
            {
                entity.Y += _spikeHeightChange;
            }
        }
    }
}