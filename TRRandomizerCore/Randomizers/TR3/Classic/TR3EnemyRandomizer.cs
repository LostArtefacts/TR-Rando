using TRDataControl;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3EnemyRandomizer : BaseTR3Randomizer
{
    private TR3EnemyAllocator _allocator;

    internal TR3TextureMonitorBroker TextureMonitor { get; set; }
    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _allocator = new()
        {
            Settings = Settings,
            ItemFactory = ItemFactory,
            Generator = _generator,
            GameLevels = Levels.Select(l => l.LevelFileBaseName),
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
        foreach (TR3ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            _allocator.RandomizeEnemiesNatively(_levelInstance.Name, _levelInstance.Data, _levelInstance.Sequence);
            ApplyPostRandomization(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void RandomizeEnemiesCrossLevel()
    {
        SetMessage("Randomizing enemies - loading levels");

        List<EnemyProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new(this));
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

    private void RandomizeEnemies(TR3CombinedLevel level, EnemyRandomizationCollection<TR3Type> enemies)
    {
        _allocator.RandomizeEnemies(level.Name, level.Data, level.Sequence, enemies);
        ApplyPostRandomization(level);
    }

    private void ApplyPostRandomization(TR3CombinedLevel level)
    {
        if (!level.Script.RemovesWeapons)
        {
            return;
        }

        _allocator.AddUnarmedLevelAmmo(level.Name, level.Data, (loc, type) =>
        {
            level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(type));
        });
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR3EnemyRandomizer>
    {
        private readonly Dictionary<TR3CombinedLevel, EnemyTransportCollection<TR3Type>> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR3EnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR3CombinedLevel level)
        {
            _enemyMapping.Add(level, null);
        }

        protected override void StartImpl()
        {
            List<TR3CombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR3CombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer._allocator.SelectCrossLevelEnemies(level.Name, level.Data, level.Sequence, false);
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR3CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection<TR3Type> enemies = _enemyMapping[level];
                    TR3DataImporter importer = new()
                    {
                        TypesToImport = enemies.TypesToImport,
                        TypesToRemove = enemies.TypesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        DataFolder = _outer.GetResourcePath(@"TR3\Objects"),
                        TextureMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.TypesToImport)
                    };

                    string remapPath = $@"TR3\Textures\Deduplication\{level.Name}-TextureRemap.json";
                    if (_outer.ResourceExists(remapPath))
                    {
                        importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                    }

                    importer.Import();

                    // Remove stale tiger model if present to avoid friendly monkeys causing vehicle crashes.
                    if (level.HasVehicle
                        && enemies.TypesToImport.Contains(TR3Type.Monkey))
                    {
                        level.Data.Models.Remove(TR3Type.Tiger);
                    }
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        // This is triggered synchronously after the import work to ensure the RNG remains consistent
        internal void ApplyRandomization()
        {
            foreach (TR3CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyRandomizationCollection<TR3Type> enemies = new()
                    {
                        Available = _enemyMapping[level].TypesToImport,
                        Droppable = TR3TypeUtilities.FilterDroppableEnemies(_enemyMapping[level].TypesToImport, _outer.Settings.ProtectMonks),
                        Water = TR3TypeUtilities.FilterWaterEnemies(_enemyMapping[level].TypesToImport)
                    };

                    _outer.RandomizeEnemies(level, enemies);
                    _outer.SaveLevel(level);
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }
    }
}
