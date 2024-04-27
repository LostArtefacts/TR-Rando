using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR2OutfitRandomizer : BaseTR2Randomizer
{
    internal TR2TextureMonitorBroker TextureMonitor { get; set; }

    private TR2Type _persistentOutfit;

    private TR2CombinedLevel _firstDragonLevel;

    private List<TR2ScriptedLevel> _haircutLevels;
    private List<TR2ScriptedLevel> _invisibleLevels;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        SetPersistentOutfit();
        ChooseFilteredLevels();

        List<OutfitProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new OutfitProcessor(this));
        }

        List<TR2CombinedLevel> levels = new(Levels.Count);
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
            levels.Sort((lvl1, lvl2) =>
            {
                return lvl1.Sequence.CompareTo(lvl2.Sequence);
            });
            _firstDragonLevel = levels.Find(l => l.Data.Entities.Any(e => e.TypeID == TR2Type.MarcoBartoli));
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

        _processingException?.Throw();
    }

    private static List<TR2Type> GetLaraTypes()
    {
        List<TR2Type> allLaras = TR2TypeUtilities.GetLaraTypes();
        allLaras.Remove(TR2Type.LaraInvisible);
        return allLaras;
    }

    private void SetPersistentOutfit()
    {
        if (Settings.PersistOutfits)
        {
            List<TR2Type> allLaras = GetLaraTypes();
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
        private static readonly List<TR2Type> _laraRemovals = new()
        {
            TR2Type.LaraPistolAnim_H,
            TR2Type.LaraAutoAnim_H,
            TR2Type.LaraUziAnim_H,
            TR2Type.Lara
        };

        // Entities to hide for haircuts
        private static readonly List<TR2Type> _invisiblePonytailEntities = new()
        {
            TR2Type.LaraPonytail_H
        };

        // Entities to hide for Lara entirely
        private static readonly List<TR2Type> _invisibleLaraEntities = new()
        {
            TR2Type.Lara, TR2Type.LaraPonytail_H, TR2Type.LaraFlareAnim_H,
            TR2Type.LaraPistolAnim_H, TR2Type.LaraShotgunAnim_H, TR2Type.LaraAutoAnim_H,
            TR2Type.LaraUziAnim_H, TR2Type.LaraM16Anim_H, TR2Type.LaraHarpoonAnim_H,
            TR2Type.LaraGrenadeAnim_H, TR2Type.LaraMiscAnim_H
        };

        private readonly Dictionary<TR2CombinedLevel, List<TR2Type>> _outfitAllocations;

        internal override int LevelCount => _outfitAllocations.Count;

        internal OutfitProcessor(TR2OutfitRandomizer outer)
            : base(outer)
        {
            _outfitAllocations = new Dictionary<TR2CombinedLevel, List<TR2Type>>();
        }

        internal void AddLevel(TR2CombinedLevel level)
        {
            _outfitAllocations.Add(level, new List<TR2Type>());
        }

        protected override void StartImpl()
        {
            // Make the outfit selection outwith the processing thread to ensure consistent RNG.
            // We select all potential Laras including the default for the level as there are
            // only 5 to choose from (there is also Assault Course Lara, but when holstering pistols
            // her trousers disappear, so she is excluded for the time being...).
            List<TR2Type> allLaras = GetLaraTypes();
            List<TR2CombinedLevel> levels = new(_outfitAllocations.Keys);

            foreach (TR2CombinedLevel level in levels)
            {
                // If invisible is chosen for this level, this overrides persistent outfits
                if (_outer.IsInvisibleLevel(level.Script))
                {
                    _outfitAllocations[level].Add(TR2Type.LaraInvisible);
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
                        TR2Type nextLara = allLaras[_outer._generator.Next(0, allLaras.Count)];
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
                foreach (TR2Type lara in _outfitAllocations[level])
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

        private bool Import(TR2CombinedLevel level, TR2Type lara)
        {
            TRModel laraModel = level.Data.Models.Find(m => m.ID == (uint)TR2Type.Lara);
            List<TRModel> laraClones = level.Data.Models.FindAll(m => m.MeshTrees.FirstOrDefault() == laraModel.MeshTrees.FirstOrDefault() && m != laraModel);

            if (lara == TR2Type.LaraInvisible)
            {
                // #314 Ensure cloned Laras remain visible
                CloneLaraMeshes(laraClones, laraModel);
                // No import needed, just clear each of Lara's meshes. A haircut is implied
                // with this and we don't need to alter the outfit.
                HideEntities(level, _invisibleLaraEntities);
                return true;
            }

            List<TR2Type> laraImport = new();
            List<TR2Type> laraRemovals = new();
            if (lara != TR2TypeUtilities.GetAliasForLevel(level.Name, TR2Type.Lara))
            {
                laraImport.Add(lara);
                laraRemovals.AddRange(_laraRemovals);
            }
            
            TR2ModelImporter importer = new()
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
                    laraModel = level.Data.Models.Find(m => m.ID == (uint)TR2Type.Lara);
                    foreach (TRModel model in laraClones)
                    {
                        model.MeshTrees = laraModel.MeshTrees;
                        model.Meshes = laraModel.Meshes;
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

        private static void CloneLaraMeshes(List<TRModel> clones, TRModel laraModel)
        {
            IEnumerable<TRMesh> clonedMeshes = laraModel.Meshes.Select(m => m.Clone());
            foreach (TRModel model in clones)
            {
                model.Meshes = new(clonedMeshes);
            }
        }

        private static void HideEntities(TR2CombinedLevel level, IEnumerable<TR2Type> entities)
        {
            MeshEditor editor = new();
            foreach (TR2Type ent in entities)
            {
                List<TRMesh> meshes = TRMeshUtilities.GetModelMeshes(level.Data, ent);
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

        private void AdjustOutfit(TR2CombinedLevel level, TR2Type lara)
        {
            if (level.Is(TR2LevelNames.HOME) && lara != TR2Type.LaraHome)
            {
                // This ensures that Lara's hips match the new outfit for the starting animation and shower cutscene,
                // otherwise the dressing gown hips are rendered, but the mesh is completely different for this, plus
                // its textures will have been removed.
                TRMesh laraMiscMesh = TRMeshUtilities.GetModelFirstMesh(level.Data, TR2Type.LaraMiscAnim_H);
                TRMesh laraHipsMesh = TRMeshUtilities.GetModelFirstMesh(level.Data, TR2Type.Lara);
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
                    if (lara == TR2Type.LaraHome)
                    {
                        MeshEditor editor = new()
                        {
                            Mesh = TRMeshUtilities.GetModelFirstMesh(level.Data, TR2Type.Lara)
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
                        MeshEditor editor = new()
                        {
                            Mesh = TRMeshUtilities.GetModelMeshes(level.Data, TR2Type.LaraMiscAnim_H)[10]
                        };

                        editor.RemoveTexturedRectangleRange(6, 20);
                        editor.ClearTexturedTriangles();
                        editor.WriteToLevel(level.Data);

                        // And hide it from the inventory
                        foreach (TRMesh mesh in TRMeshUtilities.GetModelMeshes(level.Data, TR2Type.Puzzle1_M_H))
                        {
                            editor.Mesh = mesh;
                            editor.ClearTexturedRectangles();
                            editor.ClearTexturedTriangles();
                            editor.WriteToLevel(level.Data);
                        }
                    }
                }
            }

            if (level.Is(TR2LevelNames.DA_CUT) && lara != TR2Type.LaraSun)
            {
                // There are actually 2 Lara models in this cutscene. Model ID 0 is Lara after she has changed
                // into the diving suit, but model ID 99 is the one before. We always want the cutscene actor to
                // match DA, but this unfortunately means she'll leave the cutscene in the same outfit. She just
                // didn't like the look of any of the alternatives...
                TRModel actorLara = level.Data.Models.Find(m => m.ID == (short)TR2Type.CutsceneActor3);
                TRModel realLara = level.Data.Models.Find(m => m.ID == (short)TR2Type.Lara);

                actorLara.MeshTrees = realLara.MeshTrees;
                actorLara.Meshes = realLara.Meshes;
            }
        }
    }
}
