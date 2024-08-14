using TRDataControl;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1EnemyRandomizer : BaseTR1Randomizer
{
    public static readonly uint MaxClones = 8;

    private TR1EnemyAllocator _allocator;

    internal TR1TextureMonitorBroker TextureMonitor { get; set; }
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

        TR1Script script = ScriptEditor.Script as TR1Script;
        script.DisableTrexCollision = true;
        if (Settings.UseRecommendedCommunitySettings)
        {
            script.ConvertDroppedGuns = true;
        }
    }

    private void RandomizeExistingEnemies()
    {
        foreach (TR1ScriptedLevel lvl in Levels)
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
            processors.Add(new EnemyProcessor(this));
        }

        List<TR1CombinedLevel> levels = new(Levels.Count);
        foreach (TR1ScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR1CombinedLevel level in levels)
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

    private void RandomizeEnemies(TR1CombinedLevel level, EnemyRandomizationCollection<TR1Type> enemies)
    {
        level.Script.ItemDrops.Clear();

        _allocator.RandomizeEnemies(level.Name, level.Data, enemies);

        ApplyPostRandomization(level, enemies);
    }

    private void ApplyPostRandomization(TR1CombinedLevel level, EnemyRandomizationCollection<TR1Type> enemies)
    {
        if (enemies == null)
        {
            return;
        }

        IEnumerable<TR1Type> missingDependencies = TR1EnemyAllocator.GetMissingDependencies(level.Data, enemies.Available);
        if (missingDependencies.Any())
        {
            ExecuteImport(level, new()
            {
                TypesToImport = new(missingDependencies),
            });
        }

        level.Script.UnobtainableKills = null;

        FixColosseumBats(level);
        AdjustTihocanEnding(level);
        FixEnemyAnimations(level);
        CloneEnemies(level);
        AddUnarmedLevelAmmo(level);
        RandomizeMeshes(level, enemies.Available);
    }

    private void FixColosseumBats(TR1CombinedLevel level)
    {
        if (!level.Is(TR1LevelNames.COLOSSEUM) || !Settings.FixOGBugs)
        {
            return;
        }

        // Fix the bat trigger in Colosseum. Done outside of environment mods to allow for cloning.
        // Item 74 is duplicated in each trigger.
        foreach (FDTriggerEntry trigger in level.Data.FloorData.GetEntityTriggers(74))
        {
            List<FDActionItem> actions = trigger.Actions
                .FindAll(a => a.Action == FDTrigAction.Object && a.Parameter == 74);
            if (actions.Count == 2)
            {
                actions[0].Parameter = 73;
            }
        }
    }

    private void AdjustTihocanEnding(TR1CombinedLevel level)
    {
        if (!level.Is(TR1LevelNames.TIHOCAN)
            || (Settings.RandomizeItems && (Settings.ItemMode == ItemMode.Shuffled || Settings.IncludeKeyItems)))
        {
            return;
        }

        TR1Entity pierreReplacement = level.Data.Entities[TR1ItemAllocator.TihocanPierreIndex];
        if (Settings.AllowEnemyKeyDrops
            && TR1EnemyUtilities.CanDropItems(pierreReplacement, level))
        {
            // Whichever enemy has taken Pierre's place will drop the items. Move the pickups to the enemy for trview lookup.
            level.Script.AddItemDrops(TR1ItemAllocator.TihocanPierreIndex, TR1ItemAllocator.TihocanEndItems
                .Select(e => ItemUtilities.ConvertToScriptItem(e.TypeID)));
            foreach (TR1Entity drop in TR1ItemAllocator.TihocanEndItems)
            {
                level.Data.Entities.Add(new()
                {
                    TypeID = drop.TypeID,
                    X = pierreReplacement.X,
                    Y = pierreReplacement.Y,
                    Z = pierreReplacement.Z,
                    Room = pierreReplacement.Room,
                });
                ItemUtilities.HideEntity(level.Data.Entities[^1]);
            }
        }
        else
        {
            // Add Pierre's pickups in a default place. Allows pacifist runs effectively.
            level.Data.Entities.AddRange(TR1ItemAllocator.TihocanEndItems);
        }
    }

    private void FixEnemyAnimations(TR1CombinedLevel level)
    {
        // Model transport will handle these missing SFX by default, but we need to fix them in
        // the levels where these enemies already exist.
        if (level.Data.Models.ContainsKey(TR1Type.Pierre)
            && (level.Is(TR1LevelNames.FOLLY) || level.Is(TR1LevelNames.COLOSSEUM) || level.Is(TR1LevelNames.CISTERN) || level.Is(TR1LevelNames.TIHOCAN)))
        {
            TR1DataExporter.AmendPierreGunshot(level.Data);
            TR1DataExporter.AmendPierreDeath(level.Data);

            // Non one-shot-Pierre levels won't have the death sound by default, so borrow it from ToT.
            if (!level.Data.SoundEffects.ContainsKey(TR1SFX.PierreDeath))
            {
                TR1Level tihocan = new TR1LevelControl().Read(Path.Combine(BackupPath, TR1LevelNames.TIHOCAN));
                level.Data.SoundEffects[TR1SFX.PierreDeath] = tihocan.SoundEffects[TR1SFX.PierreDeath];
            }
        }

        if (level.Data.Models.ContainsKey(TR1Type.Larson) && level.Is(TR1LevelNames.SANCTUARY))
        {
            TR1DataExporter.AmendLarsonDeath(level.Data);
        }

        if (level.Data.Models.ContainsKey(TR1Type.SkateboardKid) && level.Is(TR1LevelNames.MINES))
        {
            TR1DataExporter.AmendSkaterBoyDeath(level.Data);
        }

        if (level.Data.Models.ContainsKey(TR1Type.Natla) && level.Is(TR1LevelNames.PYRAMID))
        {
            TR1DataExporter.AmendNatlaDeath(level.Data);
        }
    }

    private void CloneEnemies(TR1CombinedLevel level)
    {
        if (!Settings.UseEnemyClones)
        {
            return;
        }

        List<TR1Type> enemyTypes = TR1TypeUtilities.GetFullListOfEnemies();
        List<TR1Entity> enemies = level.Data.Entities.FindAll(e => enemyTypes.Contains(e.TypeID));

        // If Adam is still in his egg, clone the egg as well. Otherwise there will be separate
        // entities inside the egg that will have already been accounted for.
        TR1Entity adamEgg = level.Data.Entities.Find(e => e.TypeID == TR1Type.AdamEgg);
        if (adamEgg != null
            && TR1EnemyUtilities.CodeBitsToAtlantean(adamEgg.CodeBits) == TR1Type.Adam
            && level.Data.Models.ContainsKey(TR1Type.Adam))
        {
            enemies.Add(adamEgg);
        }
        
        uint cloneCount = Math.Max(2, Math.Min(MaxClones, Settings.EnemyMultiplier)) - 1;
        short angleDiff = (short)Math.Ceiling(ushort.MaxValue / (cloneCount + 1d));

        foreach (TR1Entity enemy in enemies)
        {
            List<FDTriggerEntry> triggers = level.Data.FloorData.GetEntityTriggers(level.Data.Entities.IndexOf(enemy));
            if (Settings.UseKillableClonePierres && enemy.TypeID == TR1Type.Pierre)
            {
                // Ensure OneShot, otherwise only ever one runaway Pierre
                triggers.ForEach(t => t.OneShot = true);
            }

            for (int i = 0; i < cloneCount; i++)
            {
                foreach (FDTriggerEntry trigger in triggers)
                {
                    trigger.Actions.Add(new()
                    {
                        Parameter = (short)level.Data.Entities.Count
                    });
                }

                TR1Entity clone = (TR1Entity)enemy.Clone();
                level.Data.Entities.Add(clone);

                if (enemy.TypeID != TR1Type.AtlanteanEgg
                    && enemy.TypeID != TR1Type.AdamEgg)
                {
                    clone.Angle -= (short)((i + 1) * angleDiff);
                }
            }
        }
    }

    private void AddUnarmedLevelAmmo(TR1CombinedLevel level)
    {
        if (!level.Script.RemovesWeapons)
        {
            return;
        }

        _allocator.AddUnarmedLevelAmmo(level.Name, level.Data, true, (loc, type) =>
        {
            level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(type));
        });
    }

    private void RandomizeMeshes(TR1CombinedLevel level, List<TR1Type> availableEnemies)
    {
        if (!Settings.SwapEnemyAppearance)
        {
            return;
        }

        if (level.Is(TR1LevelNames.ATLANTIS))
        {
            // Atlantis scion swap - Model => Mesh index
            Dictionary<TR1Type, int> scionSwaps = new()
            {
                [TR1Type.Lara] = 3,
                [TR1Type.Pistols_M_H] = 1,
                [TR1Type.Shotgun_M_H] = 0,
                [TR1Type.ShotgunAmmo_M_H] = 0,
                [TR1Type.Magnums_M_H] = 1,
                [TR1Type.Uzis_M_H] = 1,
                [TR1Type.Dart_H] = 0,
                [TR1Type.Sunglasses_M_H] = 0,
                [TR1Type.CassettePlayer_M_H] = 1
            };

            List<TRMesh> scion = level.Data.Models[TR1Type.ScionPiece4_S_P].Meshes;
            List<TR1Type> replacementKeys = scionSwaps.Keys.ToList();
            TR1Type replacement = replacementKeys[_generator.Next(0, replacementKeys.Count)];

            List<TRMesh> replacementMeshes = level.Data.Models[replacement].Meshes;
            int colRadius = scion[0].CollRadius;
            replacementMeshes[scionSwaps[replacement]].CopyInto(scion[0]);
            scion[0].CollRadius = colRadius; // Retain original as Lara may need to shoot it

            // Cutscene head swaps
            List<TRMesh> lara = level.CutSceneLevel.Data.Models[TR1Type.CutsceneActor1].Meshes;
            List<TRMesh> natla = level.CutSceneLevel.Data.Models[TR1Type.CutsceneActor3].Meshes;
            List<TRMesh> pierre = level.CutSceneLevel.Data.Models[TR1Type.Pierre].Meshes;

            switch (_generator.Next(0, 6))
            {
                case 0:
                    // Natla becomes Lara
                    lara[14].CopyInto(natla[8]);
                    break;
                case 1:
                    // Lara becomes Natla
                    natla[8].CopyInto(lara[14]);
                    break;
                case 2:
                    // Switch Lara and Natla
                    (natla[8], lara[14]) = (lara[14], natla[8]);
                    break;
                case 3:
                    // Natla becomes Pierre
                    pierre[8].CopyInto(natla[8]);
                    break;
                case 4:
                    // Lara becomes Pierre
                    pierre[8].CopyInto(lara[14]);
                    break;
                case 5:
                    // Two Pierres
                    pierre[8].CopyInto(natla[8]);
                    pierre[8].CopyInto(lara[14]);
                    break;
            }
        }

        if (availableEnemies.Contains(TR1Type.Adam) && _generator.NextDouble() < 0.4)
        {
            // Replace Adam's head with a much larger version of Natla's, Larson's or normal/angry Lara's.
            List<TRMesh> adam = level.Data.Models[TR1Type.Adam].Meshes;
            TRMesh replacement;
            if (availableEnemies.Contains(TR1Type.Natla) && _generator.NextDouble() < 0.5)
            {
                replacement = level.Data.Models[TR1Type.Natla].Meshes[2];
            }
            else if (availableEnemies.Contains(TR1Type.Larson) && _generator.NextDouble() < 0.5)
            {
                replacement = level.Data.Models[TR1Type.Larson].Meshes[8];
            }
            else if (availableEnemies.Contains(TR1Type.Pierre) && _generator.NextDouble() < 0.5)
            {
                replacement = level.Data.Models[TR1Type.Pierre].Meshes[8];
            }
            else
            {
                TR1Type laraSwapType = _generator.NextDouble() < 0.5 ? TR1Type.LaraUziAnimation_H : TR1Type.Lara;
                replacement = level.Data.Models[laraSwapType].Meshes[14];
            }

            adam[3] = replacement.Clone();

            // Enlarge and rotate about Y
            foreach (TRVertex vertex in adam[3].Vertices)
            {
                vertex.X = (short)(vertex.X * -6);
                vertex.Y = (short)(vertex.Y * 6);
                vertex.Z = (short)(vertex.Z * -6);
            }

            adam[3].CollRadius *= 6;

            // Replace the neck texture to suit the head
            for (int i = 1; i < 3; i++)
            {
                foreach (TRMeshFace face in adam[i].TexturedTriangles)
                {
                    face.Texture = adam[0].TexturedTriangles[0].Texture;
                }
                foreach (TRMeshFace face in adam[i].TexturedRectangles)
                {
                    face.Texture = adam[0].TexturedRectangles[0].Texture;
                }
            }
        }

        if (availableEnemies.Contains(TR1Type.Pierre) && _generator.NextDouble() < 0.25)
        {
            // Replace Pierre's head with a slightly bigger version of Lara's (either angry Lara or normal Lara)
            List<TRMesh> pierre = level.Data.Models[TR1Type.Pierre].Meshes;
            List<TRMesh> lara = level.Data.Models[TR1Type.Lara].Meshes;
            List<TRMesh> laraUziAnim = level.Data.Models[TR1Type.LaraUziAnimation_H].Meshes;

            pierre[8] = (_generator.NextDouble() < 0.5 ? laraUziAnim[14] : lara[14]).Clone();
            foreach (TRVertex vertex in pierre[8].Vertices)
            {
                vertex.X = (short)(vertex.X * 1.5 + 6);
                vertex.Y = (short)(vertex.Y * 1.5);
                vertex.Z = (short)(vertex.Z * 1.5);
            }

            pierre[8].CollRadius = (short)(lara[14].CollRadius * 1.5);
        }
    }

    private void ExecuteImport(TR1CombinedLevel level, TR1DataImporter importer)
    {
        importer.Level = level.Data;
        importer.LevelName = level.Name;
        importer.DataFolder = GetResourcePath(@"TR1\Objects");

        importer.Data.TextureObjectLimit = RandoConsts.TRRTexLimit;
        importer.Data.TextureTileLimit = RandoConsts.TRRTileLimit;

        string remapPath = $@"TR1\Textures\Deduplication\{level.Name}-TextureRemap.json";
        if (ResourceExists(remapPath))
        {
            importer.TextureRemapPath = GetResourcePath(remapPath);
        }

        importer.Import();
    }

    internal class EnemyProcessor : AbstractProcessorThread<TR1EnemyRandomizer>
    {
        private readonly Dictionary<TR1CombinedLevel, EnemyTransportCollection<TR1Type>> _enemyMapping;

        internal override int LevelCount => _enemyMapping.Count;

        internal EnemyProcessor(TR1EnemyRandomizer outer)
            : base(outer)
        {
            _enemyMapping = new();
        }

        internal void AddLevel(TR1CombinedLevel level)
        {
            _enemyMapping.Add(level, null);
        }

        protected override void StartImpl()
        {
            // Load initially outwith the processor thread to ensure the RNG selected for each
            // level/enemy group remains consistent between randomization sessions.
            List<TR1CombinedLevel> levels = new(_enemyMapping.Keys);
            foreach (TR1CombinedLevel level in levels)
            {
                _enemyMapping[level] = _outer._allocator.SelectCrossLevelEnemies(level.Name, level.Data);
            }
        }

        // Executed in parallel, so just store the import result to process later synchronously.
        protected override void ProcessImpl()
        {
            foreach (TR1CombinedLevel level in _enemyMapping.Keys)
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
                        TextureMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, enemies.TypesToImport)
                    };

                    importer.Data.AliasPriority = TR1EnemyUtilities.GetAliasPriority(level.Name, enemies.TypesToImport);
                    _outer.ExecuteImport(level, importer);
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
            foreach (TR1CombinedLevel level in _enemyMapping.Keys)
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
