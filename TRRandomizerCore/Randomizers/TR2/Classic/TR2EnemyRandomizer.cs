using Newtonsoft.Json;
using TRDataControl;
using TRDataControl.Environment;
using TRLevelControl;
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
    private readonly Dictionary<TR2Type, uint> _unarmedAmmoCounts = new()
    {
        [TR2Type.Pistols_S_P] = 0,
        [TR2Type.Shotgun_S_P] =  8,
        [TR2Type.Automags_S_P] = 4,
        [TR2Type.Uzi_S_P] = 4,
        [TR2Type.Harpoon_S_P] = 4,
        [TR2Type.M16_S_P] = 2,
        [TR2Type.GrenadeLauncher_S_P] = 4,
    };

    private static readonly double _cloneChance = 0.5;
    private static readonly double _easyPistolChance = 0.2;
    private static readonly uint _maxClones = 8;

    private Dictionary<string, List<Location>> _pistolLocations;
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
            DragonLevels = TR2LevelNames.AsListWithGold,
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
        foreach (var lvl in Levels)
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
        foreach (var lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

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
        AmendChickenBehaviour(level.Data, TR2Type.BirdMonster);
        if (enemies.BirdMonsterGuiser != TR2Type.BirdMonster)
        {
            AmendChickenBehaviour(level.Data, TR2TypeUtilities.TranslateAlias(enemies.BirdMonsterGuiser));
        }

        AdjustFurnaceBlockRoom(level);
        RandomizeEnemyMeshes(level, enemies);
        CloneEnemies(level);
        AddUnarmedItems(level);
    }

    private void AmendChickenBehaviour(TR2Level level, TR2Type type)
    {
        if (level.Models[type]?.Animations[20] is not TRAnimation endAnim)
        {
            return;
        }

        var endCommand = endAnim.Commands
            .OfType<TRFXCommand>()
            .FirstOrDefault(c => c.EffectID == (short)TR2FX.EndLevel);
        if (Settings.DefaultChickens)
        {
            if (endCommand == null)
            {
                endAnim.Commands.Add(new TRFXCommand
                {
                    EffectID = (short)TR2FX.EndLevel,
                    FrameNumber = endAnim.FrameEnd,
                });
            }
        }
        else if (endCommand != null)
        {
            endAnim.Commands.Remove(endCommand);
        }
    }

    private record TriggerSpec(List<FDActionItem> Actions,
        List<TRRoomSector> Sectors,
        FDTrigType TrigType = FDTrigType.Trigger);

    private void AdjustFurnaceBlockRoom(TR2CombinedLevel level)
    {
        if (!level.Is(TR2LevelNames.FURNACE) || !Settings.CrossLevelEnemies
            || Settings.RandoEnemyDifficulty == RandoDifficulty.NoRestrictions)
        {
            return;
        }

        var enemies = level.Data.Entities
            .Select((entity, idx) => (entity, idx))
            .Where(x => x.entity.Room == 11 && TR2TypeUtilities.IsEnemyType(x.entity.TypeID));
        foreach (var enemy in enemies.Select(x => x.entity))
        {
            var x = (enemy.X - level.Data.Rooms[11].Info.X) / TRConsts.Step4;
            var z = (enemy.Z - level.Data.Rooms[11].Info.Z) / TRConsts.Step4;
            enemy.Angle = (x, z) switch
            {
                (1, _) => -16384,
                (_, 1) => -32768,
                _ => 0,
            };
        }

        var enemyTrigActions = enemies.Select(x => new FDActionItem
        {
            Action = FDTrigAction.Object,
            Parameter = (short)x.idx,
        }).ToList();
        enemyTrigActions.Shuffle(_generator);
        var actionGroups = enemyTrigActions.Split(4);

        // Clear all existing triggers
        new EMRemoveTriggerFunction { Rooms = [11] }.ApplyToLevel(level.Data);

        var specs = new[]
        {
            // Trigger first group when emerging from the pool
            new TriggerSpec
            (
                actionGroups[0],
                level.Data.Rooms[14].GetPerimeterSectors(1, 1, 3, 3)
            ),
            // Trigger second group on the pool perimiter
            new TriggerSpec
            (
                actionGroups[1],
                level.Data.Rooms[11].GetPerimeterSectors(2, 2, 6, 6)
            ),
            // Trigger third group after pulling the exit block once
            new TriggerSpec
            (
                actionGroups[2],
                [level.Data.Rooms[11].GetSector(7, 4, TRUnit.Sector)],
                FDTrigType.HeavyTrigger
            ),
            // Trigger fourth group after pushing the block to either side
            new TriggerSpec
            (
                actionGroups[3],
                [
                    level.Data.Rooms[11].GetSector(7, 3, TRUnit.Sector),
                    level.Data.Rooms[11].GetSector(7, 5, TRUnit.Sector),
                ],
                FDTrigType.HeavyTrigger
            ),
        };

        foreach (var spec in specs)
        {
            var trigger = new FDTriggerEntry
            {
                TrigType = spec.TrigType,
                Actions = spec.Actions,
            };
            foreach (var sector in spec.Sectors)
            {
                if (sector.FDIndex == 0)
                {
                    level.Data.FloorData.CreateFloorData(sector);
                }
                level.Data.FloorData[sector.FDIndex].Add(trigger.Clone());
            }
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
            level.Data.Models[TR2Type.MarcoBartoli] = level.Data.Models[TR2Type.Winston].Clone();
        }
    }

    private void AddRandomLaraClone(EnemyRandomizationCollection<TR2Type> enemies, TR2Type enemyType, List<TR2Type> cloneCollection)
    {
        if (enemies.All.Contains(enemyType) && _generator.NextDouble() < _cloneChance)
        {
            cloneCollection.Add(enemyType);
        }
    }

    private void CloneEnemies(TR2CombinedLevel level)
    {
        if (!Settings.UseEnemyClones)
        {
            return;
        }

        var enemies = level.Data.Entities
            .Where(e => TR2TypeUtilities.IsEnemyType(e.TypeID))
            .Where(e => Settings.RandoEnemyDifficulty == RandoDifficulty.NoRestrictions
                        || e.TypeID != TR2Type.MarcoBartoli)
            .ToList();

        var cloneCount = Math.Max(2, Math.Min(_maxClones, Settings.EnemyMultiplier)) - 1;
        var rotationStep = (short)Math.Ceiling(ushort.MaxValue / (cloneCount + 1d));

        foreach (var enemy in enemies)
        {
            var triggers = level.Data.FloorData.GetEntityTriggers(level.Data.Entities.IndexOf(enemy));
            if (triggers.Count == 0)
            {
                continue;
            }

            for (int i = 0; i < cloneCount; i++)
            {
                var action = new FDActionItem
                {
                    Parameter = (short)level.Data.Entities.Count,
                };
                triggers.ForEach(t => t.Actions.Add(action.Clone()));

                var clone = enemy.Clone() as TR2Entity;
                level.Data.Entities.Add(clone);
                clone.Angle -= (short)((i + 1) * rotationStep);
            }
        }
    }

    private void AddUnarmedItems(TR2CombinedLevel level)
    {
        if (!level.Script.RemovesWeapons || !Settings.CrossLevelEnemies || !Settings.GiveUnarmedItems)
        {
            return;
        }

        var weaponTypes = TR2TypeUtilities.GetGunTypes();
        var weaponItem = level.Data.Entities.Find(e =>
            weaponTypes.Contains(e.TypeID)
            && _pistolLocations[level.Name].Any(l => l.IsEquivalent(e.GetLocation())));

        if (weaponItem == null)
        {
            return;
        }

        if (level.Is(TR2LevelNames.HOME) && Settings.RandomizeItems && Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
        {
            weaponItem.TypeID = TR2Type.Pistols_S_P;
            return;
        }

        weaponItem.TypeID = weaponTypes.RandomItem(_generator);

        var difficulty = TR2EnemyUtilities.GetEnemyDifficulty(level.GetEnemyEntities());
        if (difficulty > EnemyDifficulty.Easy
            || weaponItem.TypeID == TR2Type.Harpoon_S_P
            || weaponItem.TypeID == TR2Type.GrenadeLauncher_S_P
            || _generator.NextDouble() < _easyPistolChance)
        {            
            if (!level.Data.Entities.Any(e => e.TypeID == TR2Type.Pistols_S_P))
            {
                var item = ItemFactory.CreateItem(level.Name, level.Data.Entities,
                weaponItem.GetLocation(), allowLimitBreak: true);
                item.TypeID = TR2Type.Pistols_S_P;
            }
        }

        if (difficulty == EnemyDifficulty.Medium || difficulty == EnemyDifficulty.Hard)
        {
            level.Script.AddStartInventoryItem(TR2Type.SmallMed_S_P);
        }
        if (difficulty > EnemyDifficulty.Medium)
        {
            level.Script.AddStartInventoryItem(TR2Type.LargeMed_S_P);
        }
        if (difficulty == EnemyDifficulty.VeryHard)
        {
            level.Script.AddStartInventoryItem(TR2Type.LargeMed_S_P, 2);
        }

        var ammoCount = _unarmedAmmoCounts[weaponItem.TypeID];
        ammoCount *= (uint)difficulty;
        if (ammoCount == 0)
        {
            return;
        }
        
        if (level.Is(TR2LevelNames.LAIR))
        {
            ammoCount *= 6;
        }

        var ammoType = TR2TypeUtilities.GetWeaponAmmo(weaponItem.TypeID);
        if (level.Is(TR2LevelNames.HOME))
        {
            level.Data.Entities.FindAll(e => TR2TypeUtilities.IsAmmoType(e.TypeID))
                .ForEach(e => e.TypeID = ammoType);
        }
        else
        {
            level.Script.AddStartInventoryItem(ammoType, ammoCount);
        }
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR2EnemyRandomizer>
    {
        private readonly Dictionary<TR2CombinedLevel, EnemyTransportCollection<TR2Type>> _enemyMapping;

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
            foreach (var level in _enemyMapping.Keys.ToList())
            {
                _enemyMapping[level] = _outer._allocator
                        .SelectCrossLevelEnemies(level.Name, level.Data);
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR2CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    var enemies = _enemyMapping[level];
                    var importer = new TR2DataImporter(isCommunityPatch: true)
                    {
                        ClearUnusedSprites = true,
                        TypesToImport = enemies.TypesToImport,
                        TypesToRemove = enemies.TypesToRemove,
                        Level = level.Data,
                        LevelName = level.Name,
                        DataFolder = _outer.GetResourcePath("TR2/Objects"),
                        TextureRemapPath = _outer.GetResourcePath($"TR2/Textures/Deduplication/{level.JsonID}-TextureRemap.json"),
                        TextureMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.TypesToImport),
                    };

                    importer.Data.AliasPriority = TR2EnemyUtilities.GetAliasPriority(level.Name, enemies.TypesToImport);
                    importer.Import();
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        internal void ApplyRandomization()
        {
            foreach (TR2CombinedLevel level in _enemyMapping.Keys)
            {
                if (!level.IsAssault)
                {
                    var importedCollection = _enemyMapping[level];
                    var settings = _outer.Settings;
                    var enemies = new EnemyRandomizationCollection<TR2Type>
                    {
                        Available = importedCollection.TypesToImport,
                        Droppable = TR2TypeUtilities.FilterDropperEnemies(importedCollection.TypesToImport, !settings.ProtectMonks, settings.UnconditionalChickens, settings.IsRemastered),
                        Water = TR2TypeUtilities.FilterWaterEnemies(importedCollection.TypesToImport),
                        All = [.. importedCollection.TypesToImport],
                    };

                    if (_outer.Settings.DocileChickens && importedCollection.BirdMonsterGuiser != TR2Type.BirdMonster)
                    {
                        TR2EnemyAllocator.DisguiseType(level.Name, level.Data.Models, importedCollection.BirdMonsterGuiser, TR2Type.BirdMonster);
                        enemies.BirdMonsterGuiser = importedCollection.BirdMonsterGuiser;
                    }

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
