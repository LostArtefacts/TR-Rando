using TRDataControl;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1REnemyRandomizer : BaseTR1RRandomizer
{
    private static readonly List<int> _tihocanEndEnemies = new() { 73, 74, 82 };
    private const int _trexDeathAnimation = 10;

    private TR1EnemyAllocator _allocator;

    public TR1RDataCache DataCache { get; set; }
    public ItemFactory<TR1Entity> ItemFactory { get; set; }

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
            EnemyRandomizationCollection<TR1Type> enemies = _allocator.RandomizeEnemiesNatively(_levelInstance.Name, _levelInstance.Data);
            ApplyPostRandomization(_levelInstance, enemies);

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

        List<TR1RCombinedLevel> levels = new(Levels.Count);
        foreach (TRRScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR1RCombinedLevel level in levels)
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

    private void RandomizeEnemies(TR1RCombinedLevel level, EnemyRandomizationCollection<TR1Type> enemies)
    {
        _allocator.RandomizeEnemies(level.Name, level.Data, enemies);
        ApplyPostRandomization(level, enemies);
    }

    private void ApplyPostRandomization(TR1RCombinedLevel level, EnemyRandomizationCollection<TR1Type> enemies)
    {
        UpdateAtlanteanPDP(level, enemies);
        HideTrexDeath(level);
        AdjustTihocanEnding(level);
        AdjustScionEnding(level);
        AddUnarmedLevelAmmo(level);
    }

    private void UpdateAtlanteanPDP(TR1RCombinedLevel level, EnemyRandomizationCollection<TR1Type> enemies)
    {
        if (!enemies.Available.Contains(TR1Type.ShootingAtlantean_N) || level.PDPData.ContainsKey(TR1Type.ShootingAtlantean_N))
        {
            return;
        }

        // The allocator may have cloned non-shooters, so copy into the PDP as well
        DataCache.SetPDPData(level.PDPData, TR1Type.ShootingAtlantean_N, TR1Type.ShootingAtlantean_N);
    }

    private void HideTrexDeath(TR1RCombinedLevel level)
    {
        if (!Settings.HideDeadTrexes || !level.Data.Models.ContainsKey(TR1Type.TRex))
        {
            return;
        }

        // Push T-rexes down on death, which ultimately disables their collision. Shift the final frame
        // to the absolute maximum so it's not visible.
        TRSetPositionCommand cmd = new()
        {
            Y = (short)level.Data.Rooms.Max(r => Math.Abs(r.Info.YBottom - r.Info.YTop)),
        };

        void UpdateModel(TRModel model)
        {
            TRAnimation deathAnimation = model.Animations[_trexDeathAnimation];
            deathAnimation.Commands.Add(cmd);
            deathAnimation.Frames[^1].OffsetY = short.MaxValue;
        }

        UpdateModel(level.Data.Models[TR1Type.TRex]);
        UpdateModel(level.PDPData[TR1Type.TRex]);
    }

    private void AdjustTihocanEnding(TR1RCombinedLevel level)
    {
        if (!level.Is(TR1LevelNames.TIHOCAN)
            || _tihocanEndEnemies.Any(e => level.Data.Entities[e].TypeID == TR1Type.Pierre)
            || (Settings.RandomizeItems && Settings.IncludeKeyItems))
        {
            return;
        }

        // Add Pierre's pickups in a default place. Allows pacifist runs effectively.
        level.Data.Entities.AddRange(TR1ItemAllocator.TihocanEndItems);
    }

    private static void AdjustScionEnding(TR1RCombinedLevel level)
    {
        if (level.Data.Models.ContainsKey(TR1Type.ScionPiece4_S_P)
            && (level.Data.Models.ContainsKey(TR1Type.TRex) || level.Data.Models.ContainsKey(TR1Type.Adam)))
        {
            // Ensure the scion is shootable in Atlantis. This is handled in OG with an environment condition,
            // but support for PDP isn't there yet.
            level.PDPData.ChangeKey(TR1Type.ScionPiece4_S_P, TR1Type.ScionPiece3_S_P);
        }
    }

    private void AddUnarmedLevelAmmo(TR1RCombinedLevel level)
    {
        if (!level.Script.RemovesWeapons)
        {
            return;
        }

        _allocator.AddUnarmedLevelAmmo(level.Name, level.Data, (loc, type) =>
        {
            if (ItemFactory.CanCreateItem(level.Name, level.Data.Entities))
            {
                TR1Entity item = ItemFactory.CreateItem(level.Name, level.Data.Entities, loc);
                item.TypeID = type;
            }
        });
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR1REnemyRandomizer>
    {
        private readonly Dictionary<TR1RCombinedLevel, EnemyTransportCollection<TR1Type>> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR1REnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR1RCombinedLevel level)
        {
            _enemyMapping.Add(level, null);
        }

        protected override void StartImpl()
        {
            List<TR1RCombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR1RCombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer._allocator.SelectCrossLevelEnemies(level.Name, level.Data);
            }
        }

        // Executed in parallel, so just store the import result to process later synchronously.
        protected override void ProcessImpl()
        {
            foreach (TR1RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection<TR1Type> enemies = _enemyMapping[level];
                    List<TR1Type> importModels = new(enemies.TypesToImport);
                    if (level.Is(TR1LevelNames.KHAMOON) && (importModels.Contains(TR1Type.BandagedAtlantean) || importModels.Contains(TR1Type.BandagedFlyer)))
                    {
                        // Mummies may become shooters in Khamoon, but the missiles won't be available by default, so ensure they do get imported.
                        importModels.Add(TR1Type.Missile2_H);
                        importModels.Add(TR1Type.Missile3_H);
                    }

                    TR1DataImporter importer = new(true)
                    {
                        TypesToImport = importModels,
                        TypesToRemove = enemies.TypesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        DataFolder = _outer.GetResourcePath(@"TR1\Objects"),
                    };

                    importer.Data.TextureObjectLimit = RandoConsts.TRRTexLimit;
                    importer.Data.TextureTileLimit = RandoConsts.TRRTileLimit;

                    string remapPath = @"TR1\Textures\Deduplication\" + level.Name + "-TextureRemap.json";
                    if (_outer.ResourceExists(remapPath))
                    {
                        importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                    }

                    importer.Data.AliasPriority = TR1EnemyUtilities.GetAliasPriority(level.Name, enemies.TypesToImport);
                    
                    ImportResult<TR1Type> result = importer.Import();
                    _outer.DataCache.Merge(result, level.PDPData, level.MapData);
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
            foreach (TR1RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyRandomizationCollection<TR1Type> enemies = new()
                    {
                        Available = _enemyMapping[level].TypesToImport,
                        Water = TR1TypeUtilities.FilterWaterEnemies(_enemyMapping[level].TypesToImport)
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
