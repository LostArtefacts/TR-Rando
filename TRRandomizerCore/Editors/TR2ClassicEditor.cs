using Newtonsoft.Json;
using TRGE.Coord;
using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Randomizers;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Editors;

public class TR2ClassicEditor : TR2LevelEditor, ISettingsProvider
{
    public RandomizerSettings Settings { get; private set; }

    public TR2ClassicEditor(TRDirectoryIOArgs args, TREdition edition)
        : base(args, edition) { }

    protected override void ApplyConfig(Config config)
    {
        Settings = new()
        {
            ExcludableEnemies = JsonConvert.DeserializeObject<Dictionary<short, string>>(File.ReadAllText("Resources/TR2/Restrictions/excludable_enemies.json"))
        };
        Settings.ApplyConfig(config);
    }

    protected override void StoreConfig(Config config)
    {
        Settings.StoreConfig(config);
    }

    protected override int GetSaveTarget(int numLevels)
    {
        int target = base.GetSaveTarget(numLevels);

        if (Settings.RandomizeGameStrings || Settings.ReassignPuzzleItems)
        {
            target++;
        }

        if (Settings.RandomizeNightMode)
        {
            target += numLevels;
            if (!Settings.RandomizeTextures)
            {
                // Texture randomizer will run if night mode is on to ensure skyboxes and such like match
                target += numLevels;
            }
        }

        if (Settings.RandomizeSecrets)
        {
            target += numLevels;
        }

        if (Settings.RandomizeAudio)
        {
            target += numLevels;
        }

        if (Settings.RandomizeItems)
        {
            target += 2 * numLevels;
            if (Settings.RandomizeItemSprites)
            {
                target += numLevels;
            }
        }

        if (Settings.RandomizeStartPosition)
        {
            target += numLevels;
        }

        // Deduplication
        target += numLevels * 2;

        if (Settings.ReassignPuzzleItems)
        {
            // For TR2ModelAdjuster
            target += numLevels;
        }

        if (Settings.RandomizeEnemies)
        {
            // 3 for multithreading cross-level work
            target += Settings.CrossLevelEnemies ? numLevels * 3 : numLevels;
        }

        if (Settings.RandomizeTextures)
        {
            // *3 because of multithreaded approach
            target += numLevels * 3;
        }

        if (Settings.RandomizeOutfits)
        {
            // *2 because of multithreaded approach
            target += numLevels * 2;
        }

        // Environment randomizer always runs
        target += numLevels * 2;

        // Enemy adjuster always runs
        target += numLevels;

        return target;
    }

    protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
    {
        var levels = scriptEditor.EnabledScriptedLevels.Cast<TRXScriptedLevel>().ToList();
        if (scriptEditor.GymAvailable)
        {
            levels.Add(scriptEditor.AssaultLevel as TRXScriptedLevel);
        }

        // Each processor will have a reference to the script editor, so can
        // make on-the-fly changes as required.
        string backupDirectory = _io.BackupDirectory.FullName;
        string wipDirectory = _io.WIPOutputDirectory.FullName;

        if (Settings.DevelopmentMode)
        {
            var script = scriptEditor.Script as TRXScript;
            script.EnforceConfig("enable_cheats", true);
            script.EnforceConfig("enable_console", true);
            scriptEditor.SaveScript();
        }

        ItemFactory<TR2Entity> itemFactory = new()
        {
            DefaultItem = new() { Intensity1 = -1, Intensity2 = -1 }
        };
        TR2TextureMonitorBroker textureMonitor = new();

        TR2ItemRandomizer itemRandomizer = new()
        {
            ScriptEditor = scriptEditor,
            Levels = levels,
            BasePath = wipDirectory,
            BackupPath = backupDirectory,
            SaveMonitor = monitor,
            Settings = Settings,
            TextureMonitor = textureMonitor,
            ItemFactory = itemFactory,
        };

        TR2EnvironmentRandomizer environmentRandomizer = new()
        {
            ScriptEditor = scriptEditor,
            Levels = levels,
            BasePath = wipDirectory,
            BackupPath = backupDirectory,
            SaveMonitor = monitor,
            Settings = Settings,
            TextureMonitor = textureMonitor
        };
        environmentRandomizer.AllocateMirroredLevels(Settings.EnvironmentSeed);

        // Texture monitoring is needed between enemy and texture randomization
        // to track where imported enemies are placed.
        if (!monitor.IsCancelled)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Adjusting enemy entities");
            new TR2EnemyAdjuster
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                ItemFactory = itemFactory,
            }.AdjustEnemies();
        }

        if (!monitor.IsCancelled && (Settings.RandomizeGameStrings || Settings.ReassignPuzzleItems))
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Adjusting game strings");
            new TR2GameStringRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.GameStringsSeed);
        }

        if (!monitor.IsCancelled)
        {
            // Deduplication is required so any further mods can reliably set texture data
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Deduplicating textures");
            new TR2TextureDeduplicator
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor
            }.Deduplicate();
        }

        if (!monitor.IsCancelled && Settings.RandomizeSecrets)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secrets");
            new TR2SecretRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                Mirrorer = environmentRandomizer,
                ItemFactory = itemFactory,
            }.Randomize(Settings.SecretSeed);
        }

        if (!monitor.IsCancelled && Settings.ReassignPuzzleItems)
        {
            // P2 items are converted to P3 in case the dragon is present as the dagger type is hardcoded.
            // Must take place before enemy randomization. OG P2 key items must be zoned based on being P3.
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Adjusting level models");
            new TR2ModelAdjuster
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor
            }.AdjustModels();
        }

        if (!monitor.IsCancelled && Settings.RandomizeItems)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing items");
            itemRandomizer.Randomize(Settings.ItemSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeEnemies)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing enemies");
            new TR2EnemyRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                TextureMonitor = textureMonitor,
                ItemFactory = itemFactory,
            }.Randomize(Settings.EnemySeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeStartPosition)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing start positions");
            new TR2StartPositionRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.StartPositionSeed);
        }

        if (!monitor.IsCancelled)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, Settings.RandomizeEnvironment ? "Randomizing environment" : "Applying default environment packs");
            environmentRandomizer.Randomize(Settings.EnvironmentSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeItems)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Finalizing item randomization");
            itemRandomizer.FinalizeRandomization();
        }

        if (!monitor.IsCancelled)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Finalizing environment changes");
            environmentRandomizer.FinalizeEnvironment();
        }

        if (!monitor.IsCancelled && Settings.RandomizeAudio)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing audio tracks");
            new TR2AudioRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.AudioSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeOutfits)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing outfits");
            new TR2OutfitRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                TextureMonitor = textureMonitor
            }.Randomize(Settings.OutfitSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeNightMode)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode");
            new TR2NightModeRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                TextureMonitor = textureMonitor
            }.Randomize(Settings.NightModeSeed);
        }

        if (!monitor.IsCancelled)
        {
            if (Settings.RandomizeTextures)
            {
                monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing textures");
                new TR2TextureRandomizer
                {
                    ScriptEditor = scriptEditor,
                    Levels = levels,
                    BasePath = wipDirectory,
                    BackupPath = backupDirectory,
                    SaveMonitor = monitor,
                    Settings = Settings,
                    TextureMonitor = textureMonitor
                }.Randomize(Settings.TextureSeed);
            }
            else if (Settings.RandomizeNightMode)
            {
                monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode textures");
                new TR2TextureRandomizer
                {
                    ScriptEditor = scriptEditor,
                    Levels = levels,
                    BasePath = wipDirectory,
                    BackupPath = backupDirectory,
                    SaveMonitor = monitor,
                    Settings = Settings,
                    TextureMonitor = textureMonitor
                }.Randomize(Settings.NightModeSeed);
            }
        }

        if (!monitor.IsCancelled && Settings.RandomizeItems && Settings.RandomizeItemSprites)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing Sprites");
            itemRandomizer.RandomizeSprites();
        }
    }
}
