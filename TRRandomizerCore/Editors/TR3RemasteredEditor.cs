using TRDataControl;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Randomizers;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Editors;

public class TR3RemasteredEditor : TR3ClassicEditor
{
    public TR3RemasteredEditor(TRDirectoryIOArgs io, TREdition edition)
        : base(io, edition) { }

    protected override void ApplyConfig(Config config)
    {
        base.ApplyConfig(config);
        Settings.IsRemastered = true;
        Settings.AllowReturnPathLocations = false;
        Settings.AddReturnPaths = false;
        Settings.FixOGBugs = false;
        Settings.ReplaceRequiredEnemies = false;
        Settings.SwapEnemyAppearance = false;
        Settings.SecretRewardMode = TRSecretRewardMode.Stack;
    }

    protected override int GetSaveTarget(int numLevels)
    {
        int target = 0;

        if (Settings.RandomizeGameStrings)
        {
            target++;
        }

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
            target += 2 * numLevels;
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

        if (Settings.RandomizeTextures)
        {
            target += numLevels * 2;
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

        ItemFactory<TR3Entity> itemFactory = new("Resources/TR3/Items/repurposable_items.json")
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

        if (!monitor.IsCancelled && Settings.RandomizeGameStrings)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing text");
            new TR3RGameStringRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.GameStringsSeed);
        }

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
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing items");
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

        Task titleTask = Task.Run(() => TRRTitleEditor.Stamp(scriptEditor.Script as TRRScript, _io));

        if (!monitor.IsCancelled && Settings.RandomizeTextures)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing textures");
            new TR3RTextureRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
            }.Randomize(Settings.TextureSeed);
        }

        monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Finalizing tasks - please wait");
        titleTask.Wait();

        TRRCheatEditor<TR3Type> cheatEditor = new(wipDirectory, new TR3PDPControl());
        levels.ForEach(l => cheatEditor.AddCheats(l));
    }
}
