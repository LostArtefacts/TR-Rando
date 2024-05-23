using TRDataControl;
using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Randomizers;

namespace TRRandomizerCore.Editors;

public class TR3RemasteredEditor : TR3ClassicEditor
{
    public TR3RemasteredEditor(TRDirectoryIOArgs io, TREdition edition)
        : base(io, edition) { }

    protected override void ApplyConfig(Config config)
    {
        base.ApplyConfig(config);
        Settings.AllowReturnPathLocations = false;
        Settings.AddReturnPaths = false;
        Settings.FixOGBugs = false;
        Settings.ReplaceRequiredEnemies = false;
        Settings.SwapEnemyAppearance = false;
    }

    protected override int GetSaveTarget(int numLevels)
    {
        int target = 0;

        if (Settings.RandomizeSecrets)
        {
            target += numLevels * 3;
        }

        if (Settings.RandomizeEnemies)
        {
            target += Settings.CrossLevelEnemies ? numLevels * 3 : numLevels;
        }

        if (Settings.RandomizeItems)
        {
            target += numLevels;
            if (Settings.IncludeKeyItems)
            {
                target += numLevels;
            }
        }

        if (Settings.RandomizeSecretRewardsPhysical)
        {
            target += numLevels;
        }

        if (Settings.RandomizeStartPosition)
        {
            target += numLevels;
        }

        if (Settings.RandomizeAudio)
        {
            target += numLevels;
        }

        // Environment randomizer always runs
        target += numLevels * 2;

        return target;
    }

    protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
    {
        List<TRRScriptedLevel> levels = new(
            scriptEditor.EnabledScriptedLevels.Cast<TRRScriptedLevel>().ToList()
        );

        if (scriptEditor.GymAvailable)
        {
            levels.Add(scriptEditor.AssaultLevel as TRRScriptedLevel);
        }

        string backupDirectory = _io.BackupDirectory.FullName;
        string wipDirectory = _io.WIPOutputDirectory.FullName;

        TR3RDataCache dataCache = new()
        {
            PDPFolder = backupDirectory,
        };

        ItemFactory<TR3Entity> itemFactory = new(@"Resources\TR3\Items\repurposable_items.json")
        {
            DefaultItem = new() { Intensity1 = -1, Intensity2 = -1 }
        };

        TR3RItemRandomizer itemRandomizer = new()
        {
            ScriptEditor = scriptEditor,
            Levels = levels,
            BasePath = wipDirectory,
            BackupPath = backupDirectory,
            SaveMonitor = monitor,
            Settings = Settings,
            ItemFactory = itemFactory,
        };

        TR3REnvironmentRandomizer environmentRandomizer = new()
        {
            ScriptEditor = scriptEditor,
            Levels = levels,
            BasePath = wipDirectory,
            BackupPath = backupDirectory,
            SaveMonitor = monitor,
            Settings = Settings,
        };

        if (!monitor.IsCancelled && Settings.RandomizeSecrets)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secrets");
            new TR3RSecretRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                ItemFactory = itemFactory,
                DataCache = dataCache,
            }.Randomize(Settings.SecretSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeItems)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing standard items");
            itemRandomizer.Randomize(Settings.ItemSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeSecretRewardsPhysical)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secret rewards");
            new TR3RSecretRewardRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.SecretRewardsPhysicalSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeEnemies)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing enemies");
            new TR3REnemyRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                ItemFactory = itemFactory,
                DataCache = dataCache,
            }.Randomize(Settings.EnemySeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeStartPosition)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing start positions");
            new TR3RStartPositionRandomizer
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

        if (!monitor.IsCancelled && Settings.RandomizeItems && Settings.IncludeKeyItems)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing key items");
            itemRandomizer.RandomizeKeyItems();
        }

        if (!monitor.IsCancelled)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Finalizing environment changes");
            environmentRandomizer.FinalizeEnvironment();
        }

        if (!monitor.IsCancelled && Settings.RandomizeAudio)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing audio tracks");
            new TR3RAudioRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.AudioSeed);
        }
    }
}
