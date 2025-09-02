using Newtonsoft.Json;
using System.Diagnostics;
using TRDataControl;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2REnemyRandomizer : BaseTR2RRandomizer
{
    private static readonly List<string> _dragonLevels = new()
    {
        TR2LevelNames.GW,
        TR2LevelNames.DORIA,
        TR2LevelNames.DECK,
        TR2LevelNames.TIBET,
        TR2LevelNames.COT,
        TR2LevelNames.CHICKEN,
        TR2LevelNames.XIAN,
    };

    private const int _hshShellCount = 16;
    private static readonly List<Location> _hshShellLocations = new()
    {
        new() { X = 31232, Y = 256, Z = 66048, Room = 57 },
        new() { X = 31232, Y = 256, Z = 65024, Room = 57 },
        new() { X = 31232, Y = 256, Z = 64000, Room = 52 },
        new() { X = 31232, Y = 256, Z = 62976, Room = 52 },
        new() { X = 31232, Y = 256, Z = 61952, Room = 52 },
        new() { X = 31232, Y = 256, Z = 60928, Room = 52 },
    };

    private Dictionary<string, List<Location>> _pistolLocations;
    private TR2EnemyAllocator _allocator;

    public TR2RDataCache DataCache { get; set; }
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
            DragonLevels = _dragonLevels,
            Remastered = true,
        };
        _allocator.Initialise();

        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource("TR2/Locations/unarmed_locations.json"));
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
            RandomizeEnemiesNatively(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void RandomizeEnemiesNatively(TR2RCombinedLevel level)
    {
        _allocator.RandomizeEnemiesNatively(level.Name, level.Data);
        ApplyPostRandomization(level);
    }

    private void RandomizeEnemiesCrossLevel()
    {
        SetMessage("Randomizing enemies - loading levels");

        List<EnemyProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new(this));
        }

        List<TR2RCombinedLevel> levels = new(Levels.Count);
        foreach (TRRScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR2RCombinedLevel level in levels)
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

    private void RandomizeEnemies(TR2RCombinedLevel level, EnemyRandomizationCollection<TR2Type> enemies)
    {
        _allocator.RandomizeEnemies(level.Name, level.Data, enemies);
        ApplyPostRandomization(level);
    }

    private void ApplyPostRandomization(TR2RCombinedLevel level)
    {
        AdjustDivingAreaEnd(level);
        RestoreHSHDog(level);
        MakeChickensUnconditional(level);
        AddUnarmedItems(level);
    }

    private static void AdjustDivingAreaEnd(TR2RCombinedLevel level)
    {
        if (!level.Is(TR2LevelNames.DA))
        {
            return;
        }

        // TR2R crashes at the end of DA in some cases because of presumably hard-coded dying monk
        // checks. Change the monk's type to allow environment mods to detect the change and make
        // further adjustments.
        level.Data.Entities[117].TypeID = TR2Type.PushButtonSwitch;
    }

    private static void RestoreHSHDog(TR2RCombinedLevel level)
    {
        if (!level.Is(TR2LevelNames.HOME))
        {
            return;
        }

        // This will have been eliminated earlier, but a dummy model is still needed in the PDP.
        level.PDPData[TR2Type.Doberman] = new();
    }

    private void MakeChickensUnconditional(TR2RCombinedLevel level)
    {
        if (level.Is(TR2LevelNames.CHICKEN) || !Settings.UnconditionalChickens)
        {
            return;
        }

        TRAnimation birdDeathAnim = level.Data.Models[TR2Type.BirdMonster]?.Animations[20];
        if (birdDeathAnim != null)
        {
            birdDeathAnim.FrameEnd = -1;
        }

        birdDeathAnim = level.PDPData[TR2Type.BirdMonster]?.Animations[20];
        if (birdDeathAnim != null)
        {
            birdDeathAnim.FrameEnd = -1;
        }
    }

    private void AddUnarmedItems(TR2RCombinedLevel level)
    {
        if (!level.Script.RemovesWeapons)
        {
            return;
        }

        // Only applies to Rig and HSH.
        // - Pistols guaranteed in Rig
        // - Pistols break HSH, so just add a silly amount of shotgun shells
        // - Extra meds loosely based on difficulty
        List<TR2Entity> enemies = level.Data.Entities.FindAll(e => TR2TypeUtilities.GetFullListOfEnemies().Contains(e.TypeID));
        EnemyDifficulty difficulty = TR2EnemyUtilities.GetEnemyDifficulty(enemies);        

        TR2Entity item = level.Data.Entities.Find(e =>
            (e.TypeID == TR2Type.Pistols_S_P || TR2TypeUtilities.IsGunType(e.TypeID))
            && _pistolLocations[level.Name].Any(l => l.IsEquivalent(e.GetLocation())));

        item ??= level.Data.Entities.Find(e => TR2TypeUtilities.IsAnyPickupType(e.TypeID));
        item ??= level.Data.Entities.Find(e => e.TypeID == TR2Type.Lara);

        if (item == null)
        {
            return;
        }

        void AddItem(TR2Type type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                item = (TR2Entity)item.Clone();
                item.TypeID = type;
                level.Data.Entities.Add(item);
            }
        }

        if (level.Is(TR2LevelNames.HOME))
        {
            // The game crashes with more than 32 pickups on a tile, so we need to spread additional shells around.
            int shellCount = _hshShellCount * (int)difficulty;
            for (int i = 0; i < shellCount; i++)
            {
                AddItem(TR2Type.ShotgunAmmo_S_P, 1);
                item.SetLocation(_hshShellLocations[i % _hshShellLocations.Count]);
            }
        }
        else
        {
            TR2Entity pistols = level.Data.Entities.Find(e => e.TypeID == TR2Type.Pistols_S_P);
            if (pistols == null)
            {
                AddItem(TR2Type.Pistols_S_P, 1);
            }
            else
            {
                pistols.SetLocation(item.GetLocation());
            }
        }

        if (Settings.GiveUnarmedItems)
        {
            int smallMeds = 0;
            int largeMeds = 0;

            if (difficulty >= EnemyDifficulty.Medium)
            {
                smallMeds++;
                largeMeds++;
            }
            while (difficulty-- >= EnemyDifficulty.Medium)
            {
                largeMeds++;
            }

            AddItem(TR2Type.SmallMed_S_P, smallMeds);
            AddItem(TR2Type.LargeMed_S_P, largeMeds);
        }
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR2REnemyRandomizer>
    {
        private static readonly List<TR2RAlias> _birdMonsterTypes = new()
        {
            TR2RAlias.BIG_YETI,
            TR2RAlias.BIG_YETI_4_5,
        };

        private readonly Dictionary<TR2RCombinedLevel, EnemyTransportCollection<TR2Type>> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR2REnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR2RCombinedLevel level)
        {
            _enemyMapping.Add(level, new());
        }

        protected override void StartImpl()
        {
            List<TR2RCombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR2RCombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer._allocator.SelectCrossLevelEnemies(level.Name, level.Data);
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR2RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection<TR2Type> enemies = _enemyMapping[level];
                    TR2DataImporter importer = new()
                    {
                        TypesToImport = enemies.TypesToImport,
                        TypesToRemove = enemies.TypesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        DataFolder = _outer.GetResourcePath("TR2/Objects"),
                    };

                    importer.Data.TextureObjectLimit = RandoConsts.TRRTexLimit;
                    importer.Data.TextureTileLimit = RandoConsts.TRRTileLimit;

                    string remapPath = $"TR2/Textures/Deduplication/{level.Name}-TextureRemap.json";
                    if (_outer.ResourceExists(remapPath))
                    {
                        importer.TextureRemapPath = _outer.GetResourcePath(remapPath);
                    }

                    importer.Data.AliasPriority = TR2EnemyUtilities.GetAliasPriority(level.Name, enemies.TypesToImport);
                    
                    ImportResult<TR2Type> result = importer.Import();
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
            foreach (TR2RCombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    EnemyTransportCollection<TR2Type> importedCollection = _enemyMapping[level];
                    EnemyRandomizationCollection<TR2Type> enemies = new()
                    {
                        Available = importedCollection.TypesToImport,
                        Droppable = TR2TypeUtilities.FilterDroppableEnemies(importedCollection.TypesToImport, !_outer.Settings.ProtectMonks, _outer.Settings.UnconditionalChickens),
                        Water = TR2TypeUtilities.FilterWaterEnemies(importedCollection.TypesToImport),
                        All = new(importedCollection.TypesToImport)
                    };

                    if (_outer.Settings.DocileChickens && importedCollection.BirdMonsterGuiser != TR2Type.BirdMonster)
                    {
                        TR2EnemyAllocator.DisguiseType(level.Name, level.Data.Models, importedCollection.BirdMonsterGuiser, TR2Type.BirdMonster);
                        TR2EnemyAllocator.DisguiseType(level.Name, level.PDPData, importedCollection.BirdMonsterGuiser, TR2Type.BirdMonster);
                        level.MapData[importedCollection.BirdMonsterGuiser] = _birdMonsterTypes.RandomItem(_outer._generator);
                        enemies.BirdMonsterGuiser = importedCollection.BirdMonsterGuiser;
                    }

                    _outer.RandomizeEnemies(level, enemies);
                    if (_outer.Settings.DevelopmentMode)
                    {
                        Debug.WriteLine(level.Name + ": " + string.Join(", ", enemies.All));
                    }

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
