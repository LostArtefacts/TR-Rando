using TRDataControl;
using TRGE.Core;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2EnemyRandomizer : BaseTR2Randomizer
{
    private static readonly double _cloneChance = 0.5;

    private TR2EnemyAllocator _allocator;

    internal TR2TextureMonitorBroker TextureMonitor { get; set; }
    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _allocator = new()
        {
            Settings = Settings,
            ItemFactory = ItemFactory,
            Generator = _generator,
            GameLevels = Levels.Select(l => l.LevelFileBaseName),
            DragonLevels = TR2LevelNames.AsList,
        };
        _allocator.Initialise();

        if (Settings.CrossLevelEnemies)
        {
            RandomizeEnemiesCrossLevel();
        }
        else
        {
            RandomizeExistingEnemies();
        }
    }

    private void RandomizeExistingEnemies()
    {
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            RandomizeEnemiesNatively(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void RandomizeEnemiesNatively(TR2CombinedLevel level)
    {
        EnemyRandomizationCollection<TR2Type> enemies = _allocator.RandomizeEnemiesNatively(level.Name, level.Data);
        ApplyPostRandomization(level, enemies);
    }

    private void RandomizeEnemiesCrossLevel()
    {
        SetMessage("Randomizing enemies - loading levels");

        List<EnemyProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new(this));
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

        // Sort the levels so each thread has a fairly equal weight in terms of import cost/time
        levels.Sort(new TR2LevelTextureWeightComparer());

        int processorIndex = 0;
        foreach (TR2CombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        SetMessage("Randomizing enemies - importing models");
        processors.ForEach(p => p.Start());
        processors.ForEach(p => p.Join());

        if (!SaveMonitor.IsCancelled && _processingException == null)
        {
            SetMessage("Randomizing enemies - saving levels");
            processors.ForEach(p => p.ApplyRandomization());
        }

        _processingException?.Throw();

        string statusMessage = _allocator.GetExclusionStatusMessage();
        if (statusMessage != null)
        {
            SetWarning(statusMessage);
        }
    }

    private void RandomizeEnemies(TR2CombinedLevel level, EnemyRandomizationCollection<TR2Type> enemies)
    {
        _allocator.RandomizeEnemies(level.Name, level.Data, enemies);
        ApplyPostRandomization(level, enemies);
    }

    private void ApplyPostRandomization(TR2CombinedLevel level, EnemyRandomizationCollection<TR2Type> enemies)
    {
        MakeChickensUnconditional(level.Data);
        RandomizeEnemyMeshes(level, enemies);
    }

    private void MakeChickensUnconditional(TR2Level level)
    {
        if (!Settings.UnconditionalChickens)
        {
            return;
        }

        // #327 Trick the game into never reaching the final frame of the death animation.
        // This results in a very abrupt death but avoids the level ending. For Ice Palace,
        // environment modifications will be made to enforce an alternative ending.
        TRAnimation birdDeathAnim = level.Models[TR2Type.BirdMonster]?.Animations[20];
        if (birdDeathAnim != null)
        {
            birdDeathAnim.FrameEnd = -1;
        }
    }

    private void RandomizeEnemyMeshes(TR2CombinedLevel level, EnemyRandomizationCollection<TR2Type> enemies)
    {
        if (!Settings.CrossLevelEnemies || !Settings.SwapEnemyAppearance)
        {
            return;
        }
        
        List<TR2Type> laraClones = new();
        if (!Settings.DocileChickens)
        {
            AddRandomLaraClone(enemies, TR2Type.MonkWithKnifeStick, laraClones);
            AddRandomLaraClone(enemies, TR2Type.MonkWithLongStick, laraClones);
        }

        AddRandomLaraClone(enemies, TR2Type.Yeti, laraClones);

        if (laraClones.Count > 0)
        {
            TRModel laraModel = level.Data.Models[TR2Type.Lara];
            foreach (TR2Type enemyType in laraClones)
            {
                TRModel enemyModel = level.Data.Models[enemyType];
                int meshCount = enemyModel.Meshes.Count;
                enemyModel.MeshTrees = new(laraModel.MeshTrees);
                enemyModel.Meshes = new(laraModel.Meshes);

                while (enemyModel.Meshes.Count < meshCount)
                {
                    // Pad with dummy meshes so any hard-coded manipulation in the engine doesn't cause
                    // potential corruption with meshes beyond this model.
                    enemyModel.MeshTrees.Add(new());
                    enemyModel.Meshes.Add(new()
                    {
                        Normals = new(),
                    });
                }
            }

            // Remove texture randomization for this enemy as it's no longer required
            TextureMonitor.ClearMonitor(level.Name, laraClones);
        }

        if (enemies.All.Contains(TR2Type.MarcoBartoli)
            && enemies.All.Contains(TR2Type.Winston)
            && _generator.NextDouble() < _cloneChance)
        {
            // Make Marco look and behave like Winston, until Lara gets too close
            TRModel marcoModel = level.Data.Models[TR2Type.MarcoBartoli];
            TRModel winnieModel = level.Data.Models[TR2Type.Winston];
            marcoModel.Animations = winnieModel.Animations;
            marcoModel.MeshTrees = winnieModel.MeshTrees;
            marcoModel.Meshes = winnieModel.Meshes;
        }
    }

    private void AddRandomLaraClone(EnemyRandomizationCollection<TR2Type> enemies, TR2Type enemyType, List<TR2Type> cloneCollection)
    {
        if (enemies.All.Contains(enemyType) && _generator.NextDouble() < _cloneChance)
        {
            cloneCollection.Add(enemyType);
        }
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR2EnemyRandomizer>
    {
        private const int _maxPackingAttempts = 5;

        private readonly Dictionary<TR2CombinedLevel, List<EnemyTransportCollection<TR2Type>>> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR2EnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR2CombinedLevel level)
        {
            _enemyMapping.Add(level, new());
        }

        protected override void StartImpl()
        {
            List<TR2CombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR2CombinedLevel level in levels)
            {
                for (int i = 0; i < _maxPackingAttempts; i++)
                {
                    _enemyMapping[level].Add(_outer._allocator
                        .SelectCrossLevelEnemies(level.Name, level.Data, i == _maxPackingAttempts - 1 ? 1 : 0));
                }
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR2CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    for (int i = 0; i < _maxPackingAttempts; i++)
                    {
                        EnemyTransportCollection<TR2Type> enemies = _enemyMapping[level][i];
                        if (Import(level, enemies))
                        {
                            enemies.ImportResult = true;
                            break;
                        }
                    }
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        private bool Import(TR2CombinedLevel level, EnemyTransportCollection<TR2Type> enemies)
        {
            try
            {
                TR2DataImporter importer = new()
                {
                    ClearUnusedSprites = true,
                    TypesToImport = enemies.TypesToImport,
                    TypesToRemove = enemies.TypesToRemove,
                    Level = level.Data,
                    LevelName = level.Name,
                    DataFolder = _outer.GetResourcePath(@"TR2\Objects"),
                    TextureRemapPath = _outer.GetResourcePath($@"TR2\Textures\Deduplication\{level.JsonID}-TextureRemap.json"),
                    TextureMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.TypesToImport)
                };

                importer.Data.AliasPriority = TR2EnemyUtilities.GetAliasPriority(level.Name, enemies.TypesToImport);

                importer.Import();
                return true;
            }
            catch (PackingException)
            {
                // We need to reload the level to undo anything that may have changed.
                _outer.ReloadLevelData(level);
                _outer.TextureMonitor.ClearMonitor(level.Name, enemies.TypesToImport);
                return false;
            }
        }

        internal void ApplyRandomization()
        {
            foreach (TR2CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection<TR2Type> importedCollection = null;
                    foreach (EnemyTransportCollection<TR2Type> enemies in _enemyMapping[level])
                    {
                        if (enemies.ImportResult)
                        {
                            importedCollection = enemies;
                            break;
                        }
                    }

                    if (importedCollection == null)
                    {
                        // Cross-level was not possible with the enemy combinations, so just go native.
                        _outer.TextureMonitor.RemoveMonitor(level.Name);
                        _outer.RandomizeEnemiesNatively(level);
                    }
                    else
                    {
                        EnemyRandomizationCollection<TR2Type> enemies = new()
                        {
                            Available = importedCollection.TypesToImport,
                            Droppable = TR2TypeUtilities.FilterDroppableEnemies(importedCollection.TypesToImport, !_outer.Settings.ProtectMonks, _outer.Settings.UnconditionalChickens),
                            Water = TR2TypeUtilities.FilterWaterEnemies(importedCollection.TypesToImport),
                            All = new(importedCollection.TypesToImport)
                        };

                        if (_outer.Settings.DocileChickens && importedCollection.BirdMonsterGuiser != TR2Type.BirdMonster)
                        {
                            TR2EnemyAllocator.DisguiseType(level.Name, level.Data, importedCollection.BirdMonsterGuiser, TR2Type.BirdMonster);
                            enemies.BirdMonsterGuiser = importedCollection.BirdMonsterGuiser;
                        }

                        _outer.RandomizeEnemies(level, enemies);
                        _outer.SaveLevel(level);
                    }
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }
    }
}
