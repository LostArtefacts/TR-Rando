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

public class TR3OutfitRandomizer : BaseTR3Randomizer
{
    private TR3Type _persistentOutfit;

    private List<TR3ScriptedLevel> _haircutLevels;
    private List<TR3ScriptedLevel> _invisibleLevels;

    internal TR3TextureMonitorBroker TextureMonitor { get; set; }

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

        List<TR3CombinedLevel> levels = new(Levels.Count);
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

        _processingException?.Throw();
    }

    private static List<TR3Type> GetLaraTypes()
    {
        List<TR3Type> allLaras = TR3TypeUtilities.GetLaraTypes();
        allLaras.Remove(TR3Type.LaraInvisible);
        return allLaras;
    }

    private void SetPersistentOutfit()
    {
        if (Settings.PersistOutfits)
        {
            List<TR3Type> allLaras = GetLaraTypes();
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
        private static readonly List<TR3Type> _laraRemovals = new()
        {
            TR3Type.LaraPistolAnimation_H,
            TR3Type.LaraUziAnimation_H,
            TR3Type.LaraDeagleAnimation_H,
            TR3Type.LaraSkin_H,
            TR3Type.Lara
        };

        // Entities to hide for haircuts
        private static readonly List<TR3Type> _invisiblePonytailEntities = new()
        {
            TR3Type.LaraPonytail_H
        };

        // Entities to hide for Lara entirely
        private static readonly List<TR3Type> _invisibleLaraEntities = new()
        {
            TR3Type.Lara, TR3Type.LaraPonytail_H, TR3Type.LaraFlareAnimation_H,
            TR3Type.LaraPistolAnimation_H, TR3Type.LaraShotgunAnimation_H, TR3Type.LaraUziAnimation_H,
            TR3Type.LaraDeagleAnimation_H, TR3Type.LaraMP5Animation_H, TR3Type.LaraGrenadeAnimation_H,
            TR3Type.LaraRocketAnimation_H, TR3Type.LaraExtraAnimation_H, TR3Type.LaraSkin_H
        };

        private readonly Dictionary<TR3CombinedLevel, List<TR3Type>> _outfitAllocations;

        internal override int LevelCount => _outfitAllocations.Count;

        internal OutfitProcessor(TR3OutfitRandomizer outer)
            : base(outer)
        {
            _outfitAllocations = new Dictionary<TR3CombinedLevel, List<TR3Type>>();
        }

        internal void AddLevel(TR3CombinedLevel level)
        {
            _outfitAllocations.Add(level, new List<TR3Type>());
        }

        protected override void StartImpl()
        {
            // Make the outfit selection outwith the processing thread to ensure consistent RNG.
            List<TR3Type> allLaras = GetLaraTypes();
            List<TR3CombinedLevel> levels = new(_outfitAllocations.Keys);

            foreach (TR3CombinedLevel level in levels)
            {
                // If invisible is chosen for this level, this overrides persistent outfits
                if (_outer.IsInvisibleLevel(level.Script))
                {
                    _outfitAllocations[level].Add(TR3Type.LaraInvisible);
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
                        TR3Type nextLara = allLaras[_outer._generator.Next(0, allLaras.Count)];
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
                foreach (TR3Type lara in _outfitAllocations[level])
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

        private bool Import(TR3CombinedLevel level, TR3Type lara)
        {
            if (lara == TR3Type.LaraInvisible)
            {
                // No import needed, just clear each of Lara's meshes.
                HideEntities(level, _invisibleLaraEntities);
                return true;
            }

            List<TR3Type> laraImport = new();
            List<TR3Type> laraRemovals = new();
            if (lara != TR3TypeUtilities.GetAliasForLevel(level.Name, TR3Type.Lara))
            {
                laraImport.Add(lara);
                laraRemovals.AddRange(_laraRemovals);
            }

            TR3DataImporter importer = new()
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

        private static void HideEntities(TR3CombinedLevel level, IEnumerable<TR3Type> entities)
        {
            MeshEditor editor = new();
            foreach (TR3Type ent in entities)
            {
                List<TRMesh> meshes = level.Data.Models[ent]?.Meshes;
                if (meshes != null)
                {
                    foreach (TRMesh mesh in meshes)
                    {
                        editor.Mesh = mesh;
                        editor.ClearAllPolygons();
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
