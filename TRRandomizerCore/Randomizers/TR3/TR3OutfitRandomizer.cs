using System;
using System.Collections.Generic;
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
    public class TR3OutfitRandomizer : BaseTR3Randomizer
    {
        private TR3Entities _persistentOutfit;

        private List<TR3ScriptedLevel> _haircutLevels;
        private List<TR3ScriptedLevel> _invisibleLevels;

        internal TR3TextureMonitorBroker TextureMonitor { get; set; }

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

            List<TR3CombinedLevel> levels = new List<TR3CombinedLevel>(Levels.Count);
            foreach (TR3ScriptedLevel lvl in Levels)
            {
                levels.Add(LoadCombinedLevel(lvl));
                if (!TriggerProgress())
                {
                    return;
                }
            }

            int processorIndex = 0;
            foreach (TR3CombinedLevel level in levels)
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

        private List<TR3Entities> GetLaraTypes()
        {
            List<TR3Entities> allLaras = TR3EntityUtilities.GetLaraTypes();
            allLaras.Remove(TR3Entities.LaraInvisible);
            return allLaras;
        }

        private void SetPersistentOutfit()
        {
            if (Settings.PersistOutfits)
            {
                List<TR3Entities> allLaras = GetLaraTypes();
                _persistentOutfit = allLaras[_generator.Next(0, allLaras.Count)];
            }
        }

        private void ChooseFilteredLevels()
        {
            TR3ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR3LevelNames.ASSAULT));
            ISet<TR3ScriptedLevel> exlusions = new HashSet<TR3ScriptedLevel> { assaultCourse };

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

        private bool IsHaircutLevel(TR3ScriptedLevel lvl)
        {
            return _haircutLevels.Contains(lvl);
        }

        private bool IsInvisibleLevel(TR3ScriptedLevel lvl)
        {
            return _invisibleLevels.Contains(lvl);
        }

        internal class OutfitProcessor : AbstractProcessorThread<TR3OutfitRandomizer>
        {
            // Each of these needs to be removed and replaced with the corresponding animation
            // or skin models for the associated outfit.
            private static readonly List<TR3Entities> _laraRemovals = new List<TR3Entities>
            {
                TR3Entities.LaraPistolAnimation_H,
                TR3Entities.LaraUziAnimation_H,
                TR3Entities.LaraDeagleAnimation_H,
                TR3Entities.LaraSkin_H,
                TR3Entities.Lara
            };

            // Entities to hide for haircuts
            private static readonly List<TR3Entities> _invisiblePonytailEntities = new List<TR3Entities>
            {
                TR3Entities.LaraPonytail_H
            };

            // Entities to hide for Lara entirely
            private static readonly List<TR3Entities> _invisibleLaraEntities = new List<TR3Entities>
            {
                TR3Entities.Lara, TR3Entities.LaraPonytail_H, TR3Entities.LaraFlareAnimation_H,
                TR3Entities.LaraPistolAnimation_H, TR3Entities.LaraShotgunAnimation_H, TR3Entities.LaraUziAnimation_H,
                TR3Entities.LaraDeagleAnimation_H, TR3Entities.LaraMP5Animation_H, TR3Entities.LaraGrenadeAnimation_H,
                TR3Entities.LaraRocketAnimation_H, TR3Entities.LaraExtraAnimation_H, TR3Entities.LaraSkin_H
            };

            private readonly Dictionary<TR3CombinedLevel, List<TR3Entities>> _outfitAllocations;

            internal override int LevelCount => _outfitAllocations.Count;

            internal OutfitProcessor(TR3OutfitRandomizer outer)
                : base(outer)
            {
                _outfitAllocations = new Dictionary<TR3CombinedLevel, List<TR3Entities>>();
            }

            internal void AddLevel(TR3CombinedLevel level)
            {
                _outfitAllocations.Add(level, new List<TR3Entities>());
            }

            protected override void StartImpl()
            {
                // Make the outfit selection outwith the processing thread to ensure consistent RNG.
                List<TR3Entities> allLaras = _outer.GetLaraTypes();
                List<TR3CombinedLevel> levels = new List<TR3CombinedLevel>(_outfitAllocations.Keys);

                foreach (TR3CombinedLevel level in levels)
                {
                    // If invisible is chosen for this level, this overrides persistent outfits
                    if (_outer.IsInvisibleLevel(level.Script))
                    {
                        _outfitAllocations[level].Add(TR3Entities.LaraInvisible);
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
                            TR3Entities nextLara = allLaras[_outer._generator.Next(0, allLaras.Count)];
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
                foreach (TR3CombinedLevel level in _outfitAllocations.Keys)
                {
                    foreach (TR3Entities lara in _outfitAllocations[level])
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

            private bool Import(TR3CombinedLevel level, TR3Entities lara)
            {
                if (lara == TR3Entities.LaraInvisible)
                {
                    // No import needed, just clear each of Lara's meshes.
                    HideEntities(level, _invisibleLaraEntities);
                    return true;
                }

                List<TR3Entities> laraImport = new List<TR3Entities>();
                List<TR3Entities> laraRemovals = new List<TR3Entities>();
                if (lara != TR3EntityUtilities.GetAliasForLevel(level.Name, TR3Entities.Lara))
                {
                    laraImport.Add(lara);
                    laraRemovals.AddRange(_laraRemovals);
                }

                TR3ModelImporter importer = new TR3ModelImporter
                {
                    Level = level.Data,
                    LevelName = level.Name,
                    ClearUnusedSprites = false,
                    EntitiesToImport = laraImport,
                    EntitiesToRemove = laraRemovals,
                    TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, laraImport),
                    DataFolder = _outer.GetResourcePath(@"TR3\Models")
                };

                string remapPath = @"TR3\Textures\Deduplication\" + level.Name + "-TextureRemap.json";
                if (_outer.ResourceExists(remapPath))
                {
                    importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                }

                try
                {
                    // Try to import the selected models into the level.
                    importer.Import();

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

            private void HideEntities(TR3CombinedLevel level, IEnumerable<TR3Entities> entities)
            {
                MeshEditor editor = new MeshEditor();
                foreach (TR3Entities ent in entities)
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
        }
    }
}