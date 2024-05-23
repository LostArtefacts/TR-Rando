using TRDataControl;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;

namespace TRRandomizerCore.Randomizers;

public class TR3REnemyRandomizer : BaseTR3RRandomizer
{
    private TR3EnemyAllocator _allocator;

    public TR3RDataCache DataCache { get; set; }
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
        foreach (TRRScriptedLevel lvl in Levels)
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

        List<TR3RCombinedLevel> levels = new(Levels.Count);
        foreach (TRRScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR3RCombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        SetMessage("Randomizing enemies - importing models");
        foreach (EnemyProcessor processor in processors)
        {
            processor.Start();
        }

        foreach (EnemyProcessor processor in processors)
        {
            processor.Join();
        }

        if (!SaveMonitor.IsCancelled && _processingException == null)
        {
            SetMessage("Randomizing enemies - saving levels");
            foreach (EnemyProcessor processor in processors)
            {
                processor.ApplyRandomization();
            }
        }

        _processingException?.Throw();

        string statusMessage = _allocator.GetExclusionStatusMessage();
        if (statusMessage != null)
        {
            SetWarning(statusMessage);
        }
    }

    private void RandomizeEnemies(TR3RCombinedLevel level, EnemyRandomizationCollection<TR3Type> enemies)
    {
        _allocator.RandomizeEnemies(level.Name, level.Data, level.Sequence, enemies);
        ApplyPostRandomization(level);
    }

    private void ApplyPostRandomization(TR3RCombinedLevel level)
    {
        if (!level.Script.RemovesWeapons)
        {
            return;
        }

        _allocator.AddUnarmedLevelAmmo(level.Name, level.Data, (loc, type) => { });

        // We can't give more ammo because HSC is so close to the limit. Instead just guarantee
        // pistols in the starting area
        List<Location> pistolLocations = _allocator.GetPistolLocations(level.Name);
        Location location;
        do
        {
            location = pistolLocations[_generator.Next(0, pistolLocations.Count)];
        }
        while (location.Room != 7);

        TR3Entity pistols = ItemFactory.CreateItem(level.Name, level.Data.Entities, location);
        pistols.TypeID = TR3Type.Pistols_P;
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR3REnemyRandomizer>
    {
        private readonly Dictionary<TR3RCombinedLevel, EnemyTransportCollection<TR3Type>> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR3REnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR3RCombinedLevel level)
        {
            _enemyMapping.Add(level, null);
        }

        protected override void StartImpl()
        {
            List<TR3RCombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR3RCombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer._allocator.SelectCrossLevelEnemies(level.Name, level.Data, level.Sequence);
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR3RCombinedLevel level in _enemyMapping.Keys)
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
                    };

                    importer.Data.TextureObjectLimit = RandoConsts.TRRTexLimit;
                    importer.Data.TextureTileLimit = RandoConsts.TRRTileLimit;

                    string remapPath = $@"TR3\Textures\Deduplication\{level.Name}-TextureRemap.json";
                    if (_outer.ResourceExists(remapPath))
                    {
                        importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                    }

                    ImportResult<TR3Type> result = importer.Import();
                    _outer.DataCache.Merge(result, level.PDPData, level.MapData);
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        internal void ApplyRandomization()
        {
            foreach (TR3RCombinedLevel level in _enemyMapping.Keys)
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
