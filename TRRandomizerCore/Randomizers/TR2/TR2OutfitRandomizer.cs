using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers
{
    public class TR2OutfitRandomizer : BaseTR2Randomizer
    {
        internal TR2TextureMonitorBroker TextureMonitor { get; set; }

        private TR2Entities _persistentOutfit;

        private TR2CombinedLevel _firstDragonLevel;

        private List<TR2ScriptedLevel> _haircutLevels;
        private List<TR2ScriptedLevel> _invisibleLevels;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            SetPersistentOutfit();
            ChooseFilteredLevels();

            List<OutfitProcessor> processors = new List<OutfitProcessor>();
            for (int i = 0; i < _maxThreads; i++)
            {
                processors.Add(new OutfitProcessor(this));
            }

            List<TR2CombinedLevel> levels = new List<TR2CombinedLevel>(Levels.Count);
            foreach (TR2ScriptedLevel lvl in Levels)
            {
                levels.Add(LoadCombinedLevel(lvl));
                if (!TriggerProgress())
                {
                    return;
                }
            }

            if (Settings.RemoveRobeDagger)
            {
                // Keep a reference to the first dragon level if we are removing the dagger from the dressing gown. This needs to be done
                // based on the level sequencing.
                levels.Sort(delegate (TR2CombinedLevel lvl1, TR2CombinedLevel lvl2)
                {
                    return lvl1.Sequence.CompareTo(lvl2.Sequence);
                });
                _firstDragonLevel = levels.Find(l => l.Data.Entities.ToList().FindIndex(e => e.TypeID == (short)TR2Entities.MarcoBartoli) != -1);
            }

            // Sort the levels so each thread has a fairly equal weight in terms of import cost/time
            levels.Sort(new TR2LevelTextureWeightComparer());

            int processorIndex = 0;
            foreach (TR2CombinedLevel level in levels)
            {
                processors[processorIndex].AddLevel(level);
                processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
            }

            foreach (OutfitProcessor processor in processors)
            {
                processor.Start();
            }

            foreach (OutfitProcessor processor in processors)
            {
                processor.Join();
            }

            if (_processingException != null)
            {
                _processingException.Throw();
            }
        }

        private List<TR2Entities> GetLaraTypes()
        {
            List<TR2Entities> allLaras = TR2EntityUtilities.GetLaraTypes();
            allLaras.Remove(TR2Entities.LaraInvisible);
            return allLaras;
        }

        private void SetPersistentOutfit()
        {
            if (Settings.PersistOutfits)
            {
                List<TR2Entities> allLaras = GetLaraTypes();
                _persistentOutfit = allLaras[_generator.Next(0, allLaras.Count)];
            }
        }

        private void ChooseFilteredLevels()
        {
            TR2ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR2LevelNames.ASSAULT));
            ISet<TR2ScriptedLevel> exlusions = new HashSet<TR2ScriptedLevel> { assaultCourse };

            _haircutLevels = Levels.RandomSelection(_generator, (int)Settings.HaircutLevelCount, exclusions: exlusions);
            if (Settings.AssaultCourseHaircut)
            {
                _haircutLevels.Add(assaultCourse);
            }

            _invisibleLevels = Levels.RandomSelection(_generator, (int)Settings.InvisibleLevelCount, exclusions: exlusions);
            if (Settings.AssaultCourseInvisible)
            {
                _invisibleLevels.Add(assaultCourse);
            }
        }

        private bool IsHaircutLevel(TR2ScriptedLevel lvl)
        {
            return _haircutLevels.Contains(lvl);
        }

        private bool IsInvisibleLevel(TR2ScriptedLevel lvl)
        {
            return _invisibleLevels.Contains(lvl);
        }

        internal class OutfitProcessor : AbstractProcessorThread<TR2OutfitRandomizer>
        {
            // Each of these needs to be removed and replaced with the corresponding animation
            // models for the associated outfit.
            private static readonly List<TR2Entities> _laraRemovals = new List<TR2Entities>
            {
                TR2Entities.LaraPistolAnim_H,
                TR2Entities.LaraAutoAnim_H,
                TR2Entities.LaraUziAnim_H,
                TR2Entities.Lara
            };

            // Entities to hide for haircuts
            private static readonly List<TR2Entities> _invisiblePonytailEntities = new List<TR2Entities>
            {
                TR2Entities.LaraPonytail_H
            };

            // Entities to hide for Lara entirely
            private static readonly List<TR2Entities> _invisibleLaraEntities = new List<TR2Entities>
            {
                TR2Entities.Lara, TR2Entities.LaraPonytail_H, TR2Entities.LaraFlareAnim_H,
                TR2Entities.LaraPistolAnim_H, TR2Entities.LaraShotgunAnim_H, TR2Entities.LaraAutoAnim_H,
                TR2Entities.LaraUziAnim_H, TR2Entities.LaraM16Anim_H, TR2Entities.LaraHarpoonAnim_H,
                TR2Entities.LaraGrenadeAnim_H, TR2Entities.LaraMiscAnim_H
            };

            private readonly Dictionary<TR2CombinedLevel, List<TR2Entities>> _outfitAllocations;

            internal override int LevelCount => _outfitAllocations.Count;

            internal OutfitProcessor(TR2OutfitRandomizer outer)
                : base(outer)
            {
                _outfitAllocations = new Dictionary<TR2CombinedLevel, List<TR2Entities>>();
            }

            internal void AddLevel(TR2CombinedLevel level)
            {
                _outfitAllocations.Add(level, new List<TR2Entities>());
            }

            protected override void StartImpl()
            {
                // Make the outfit selection outwith the processing thread to ensure consistent RNG.
                // We select all potential Laras including the default for the level as there are
                // only 5 to choose from (there is also Assault Course Lara, but when holstering pistols
                // her trousers disappear, so she is excluded for the time being...).
                List<TR2Entities> allLaras = _outer.GetLaraTypes();
                List<TR2CombinedLevel> levels = new List<TR2CombinedLevel>(_outfitAllocations.Keys);

                foreach (TR2CombinedLevel level in levels)
                {
                    // If invisible is chosen for this level, this overrides persistent outfits
                    if (_outer.IsInvisibleLevel(level.Script))
                    {
                        _outfitAllocations[level].Add(TR2Entities.LaraInvisible);
                    }
                    else
                    {
                        // Add the persistent outfit first, but we will populate the candidate
                        // list regardless in case a level cannot support this choice.
                        if (_outer.Settings.PersistOutfits)
                        {
                            _outfitAllocations[level].Add(_outer._persistentOutfit);
                        }

                        while (_outfitAllocations[level].Count < allLaras.Count)
                        {
                            TR2Entities nextLara = allLaras[_outer._generator.Next(0, allLaras.Count)];
                            if (!_outfitAllocations[level].Contains(nextLara))
                            {
                                _outfitAllocations[level].Add(nextLara);
                            }
                        }
                    }
                }
            }

            protected override void ProcessImpl()
            {
                foreach (TR2CombinedLevel level in _outfitAllocations.Keys)
                {
                    foreach (TR2Entities lara in _outfitAllocations[level])
                    {
                        if (Import(level, lara))
                        {
                            break;
                        }
                    }

                    if (_outer.IsHaircutLevel(level.Script))
                    {
                        HideEntities(level, _invisiblePonytailEntities);
                    }

                    _outer.SaveLevel(level);

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }

            private bool Import(TR2CombinedLevel level, TR2Entities lara)
            {
                List<TRModel> models = level.Data.Models.ToList();
                TRModel laraModel = models.Find(m => m.ID == (uint)TR2Entities.Lara);
                List<TRModel> laraClones = models.FindAll(m => m.MeshTree == laraModel.MeshTree && m != laraModel);

                if (lara == TR2Entities.LaraInvisible)
                {
                    // #314 Ensure cloned Laras remain visible
                    CloneLaraMeshes(level, laraClones, laraModel);
                    // No import needed, just clear each of Lara's meshes. A haircut is implied
                    // with this and we don't need to alter the outfit.
                    HideEntities(level, _invisibleLaraEntities);
                    return true;
                }

                List<TR2Entities> laraImport = new List<TR2Entities>();
                List<TR2Entities> laraRemovals = new List<TR2Entities>();
                if (lara != TR2EntityUtilities.GetAliasForLevel(level.Name, TR2Entities.Lara))
                {
                    laraImport.Add(lara);
                    laraRemovals.AddRange(_laraRemovals);
                }
                
                TR2ModelImporter importer = new TR2ModelImporter
                {
                    Level = level.Data,
                    LevelName = level.Name,
                    ClearUnusedSprites = false,
                    EntitiesToImport = laraImport,
                    EntitiesToRemove = laraRemovals,
                    TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, laraImport),
                    DataFolder = _outer.GetResourcePath(@"TR2\Models")
                };

                string remapPath = _outer.GetResourcePath(@"TR2\Textures\Deduplication\" + level.JsonID + "-TextureRemap.json");
                if (File.Exists(remapPath))
                {
                    importer.TextureRemapPath = remapPath;
                }

                try
                {
                    // Try to import the selected models into the level.
                    importer.Import();

                    // #314 Any clones of Lara should copy her new style
                    if (laraClones.Count > 0)
                    {
                        models = level.Data.Models.ToList();
                        laraModel = models.Find(m => m.ID == (uint)TR2Entities.Lara);
                        foreach (TRModel model in laraClones)
                        {
                            model.MeshTree = laraModel.MeshTree;
                            model.StartingMesh = laraModel.StartingMesh;
                            model.NumMeshes = laraModel.NumMeshes;
                        }
                    }

                    // Apply any necessary tweaks to the outfit
                    AdjustOutfit(level, lara);

                    // Repeat the process if there is a cutscene after this level.
                    if (level.HasCutScene)
                    {
                        Import(level.CutSceneLevel, lara);
                    }

                    return true;
                }
                catch (PackingException)
                {
                    // We need to reload the level to undo anything that may have changed.
                    _outer.ReloadLevelData(level);
                    // Tell the monitor to no longer track what we tried to import
                    _outer.TextureMonitor.ClearMonitor(level.Name, laraImport);
                    return false;
                }
            }

            private void CloneLaraMeshes(TR2CombinedLevel level, List<TRModel> clones, TRModel laraModel)
            {
                if (clones.Count > 0)
                {
                    MeshEditor editor = new MeshEditor();
                    TRMesh[] meshes = TRMeshUtilities.GetModelMeshes(level.Data, laraModel);
                    int firstMeshIndex = -1;
                    for (int i = 0; i < meshes.Length; i++)
                    {
                        int insertedIndex = TRMeshUtilities.InsertMesh(level.Data, editor.CloneMesh(meshes[i]));
                        if (firstMeshIndex == -1)
                        {
                            firstMeshIndex = insertedIndex;
                        }
                    }

                    foreach (TRModel model in clones)
                    {
                        model.StartingMesh = (ushort)firstMeshIndex;
                    }
                }
            }

            private void HideEntities(TR2CombinedLevel level, IEnumerable<TR2Entities> entities)
            {
                MeshEditor editor = new MeshEditor();
                foreach (TR2Entities ent in entities)
                {
                    TRMesh[] meshes = TRMeshUtilities.GetModelMeshes(level.Data, ent);
                    if (meshes != null)
                    {
                        foreach (TRMesh mesh in meshes)
                        {
                            editor.Mesh = mesh;
                            editor.ClearAllPolygons();
                            editor.WriteToLevel(level.Data);
                        }
                    }
                }

                // Repeat the process if there is a cutscene after this level.
                if (level.HasCutScene)
                {
                    HideEntities(level.CutSceneLevel, entities);
                }
            }

            private void AdjustOutfit(TR2CombinedLevel level, TR2Entities lara)
            {
                if (level.Is(TR2LevelNames.HOME) && lara != TR2Entities.LaraHome)
                {
                    // This ensures that Lara's hips match the new outfit for the starting animation and shower cutscene,
                    // otherwise the dressing gown hips are rendered, but the mesh is completely different for this, plus
                    // its textures will have been removed.
                    TRMesh laraMiscMesh = TRMeshUtilities.GetModelFirstMesh(level.Data, TR2Entities.LaraMiscAnim_H);
                    TRMesh laraHipsMesh = TRMeshUtilities.GetModelFirstMesh(level.Data, TR2Entities.Lara);
                    TRMeshUtilities.DuplicateMesh(level.Data, laraMiscMesh, laraHipsMesh);
                }

                if (_outer.Settings.RemoveRobeDagger)
                {
                    // We will get rid of the dagger from Lara's dressing gown if this level takes place before any other
                    // level that contains a dragon, or if this level itself has the dragon. If it's a cutscene that
                    // immediately follows a level that had a dragon, she will have the dagger on display. Note that a
                    // cutscene level shares its sequence with its parent level.
                    // Note that dragonLevel can be null if the number of levels has been changed (i.e. Lair is not guaranteed).
                    TR2CombinedLevel dragonLevel = _outer._firstDragonLevel;
                    if (dragonLevel == null || (level.IsCutScene && level.Sequence < dragonLevel.Sequence) || (!level.IsCutScene && level.Sequence <= dragonLevel.Sequence))
                    {
                        if (lara == TR2Entities.LaraHome)
                        {
                            MeshEditor editor = new MeshEditor
                            {
                                Mesh = TRMeshUtilities.GetModelFirstMesh(level.Data, TR2Entities.Lara)
                            };

                            editor.RemoveTexturedRectangleRange(9, 43);
                            editor.RemoveTexturedTriangleRange(18, 38);
                            editor.WriteToLevel(level.Data);
                        }

                        // If it's HSH, go one step further and remove the model itself, so Lara is imagining what the dagger
                        // will be like during the cutscene. It will still be present in the inventory but will be invisible.
                        if (level.Is(TR2LevelNames.HOME))
                        {
                            // This removes it from the starting cutscene - mesh 10 is Lara's hand holding the dagger,
                            // so we basically just retain the hand.
                            MeshEditor editor = new MeshEditor
                            {
                                Mesh = TRMeshUtilities.GetModelMeshes(level.Data, TR2Entities.LaraMiscAnim_H)[10]
                            };

                            editor.RemoveTexturedRectangleRange(6, 20);
                            editor.ClearTexturedTriangles();
                            editor.WriteToLevel(level.Data);

                            // And hide it from the inventory
                            foreach (TRMesh mesh in TRMeshUtilities.GetModelMeshes(level.Data, TR2Entities.Puzzle1_M_H))
                            {
                                editor.Mesh = mesh;
                                editor.ClearTexturedRectangles();
                                editor.ClearTexturedTriangles();
                                editor.WriteToLevel(level.Data);
                            }
                        }
                    }
                }

                if (level.Is(TR2LevelNames.DA_CUT) && lara != TR2Entities.LaraSun)
                {
                    // There are actually 2 Lara models in this cutscene. Model ID 0 is Lara after she has changed
                    // into the diving suit, but model ID 99 is the one before. We always want the cutscene actor to
                    // match DA, but this unfortunately means she'll leave the cutscene in the same outfit. She just
                    // didn't like the look of any of the alternatives...
                    List<TRModel> models = level.Data.Models.ToList();
                    TRModel actorLara = models.Find(m => m.ID == (short)TR2Entities.CutsceneActor3);
                    TRModel realLara = models.Find(m => m.ID == (short)TR2Entities.Lara);

                    actorLara.MeshTree = realLara.MeshTree;
                    actorLara.NumMeshes = realLara.NumMeshes;
                    actorLara.StartingMesh = realLara.StartingMesh;
                }
            }
        }
    }
}