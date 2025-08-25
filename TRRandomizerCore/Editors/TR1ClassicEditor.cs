using Newtonsoft.Json;
using System.Drawing;
using TRGE.Coord;
using TRGE.Core;
using TRImageControl;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Randomizers;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Editors;

public class TR1ClassicEditor : TR1LevelEditor, ISettingsProvider
{
    private static readonly Point _regularBadgePos = new(702, 538);
    private static readonly Point _goldBadgePos = new(498, 444);

    public RandomizerSettings Settings { get; private set; }

    public TR1ClassicEditor(TRDirectoryIOArgs args, TREdition edition)
        : base(args, edition) { }

    protected override void ApplyConfig(Config config)
    {
        Settings = new()
        {
            ExcludableEnemies = JsonConvert.DeserializeObject<Dictionary<short, string>>(File.ReadAllText("Resources/TR1/Restrictions/excludable_enemies.json"))
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

        // String rando always runs
        target++;

        // Injection checks
        target += numLevels;

        if (Settings.RandomizeStartingHealth)
        {
            target += numLevels;
        }

        if (Settings.RandomizeSecrets)
        {
            // *3 for multithreaded work
            target += numLevels * 3;
        }

        if (Settings.RandomizeEnemies)
        {
            // *3 for multithreaded work
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

        if (Settings.RandomizeAudio)
        {
            target += numLevels;
        }

        if (Settings.RandomizeOutfits)
        {
            // *2 because of multithreaded approach
            target += numLevels * 2;
        }

        if (Settings.RandomizeTextures)
        {
            // *3 for multithreaded work
            target += numLevels * 3;
        }

        if (Settings.RandomizeStartPosition)
        {
            target += numLevels;
        }

        if (Settings.RandomizeNightMode)
        {
            target += numLevels;
        }

        // Environment randomizer always runs
        target += numLevels * 2;

        return target;
    }

    protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
    {
        var levels = scriptEditor.EnabledScriptedLevels.Cast<TRXScriptedLevel>().ToList();
        if (scriptEditor.GymAvailable)
        {
            levels.Add(scriptEditor.AssaultLevel as TRXScriptedLevel);
        }

        string backupDirectory = _io.BackupDirectory.FullName;
        string wipDirectory = _io.WIPOutputDirectory.FullName;

        if (Settings.DevelopmentMode)
        {
            var script = scriptEditor.Script as TRXScript;
            script.EnforceConfig("enable_cheats", true);
            script.EnforceConfig("enable_console", true);
            scriptEditor.SaveScript();
        }

        ItemFactory<TR1Entity> itemFactory = new("Resources/TR1/Items/repurposable_items.json");
        TR1TextureMonitorBroker textureMonitor = new();

        TR1ItemRandomizer itemRandomizer = new()
        {
            ScriptEditor = scriptEditor,
            Levels = levels,
            BasePath = wipDirectory,
            BackupPath = backupDirectory,
            SaveMonitor = monitor,
            Settings = Settings,
            ItemFactory = itemFactory
        };

        TR1EnvironmentRandomizer environmentRandomizer = new()
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

        
        if (!monitor.IsCancelled)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing text");
            new TR1GameStringRandomizer
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
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Validating data injections");
            new TR1InjectionProcessor
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
            }.Run();
        }

        if (!monitor.IsCancelled && Settings.RandomizeStartingHealth)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing health");
            new TR1HealthRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.HealthSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeSecrets)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secrets");
            new TR1SecretRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                ItemFactory = itemFactory,
                Mirrorer = environmentRandomizer
            }.Randomize(Settings.SecretSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeEnemies)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing enemies");
            new TR1EnemyRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                ItemFactory = itemFactory,
                TextureMonitor = textureMonitor
            }.Randomize(Settings.EnemySeed);
        }

        // Tomp1 doesn't have droppable enmies so item rando takes places after enemy rando
        // - this allows for accounting for newly added items.
        if (!monitor.IsCancelled && Settings.RandomizeItems)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing items");
            itemRandomizer.Randomize(Settings.ItemSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeSecretRewardsPhysical)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secret rewards");
            new TR1SecretRewardRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings,
                ItemFactory = itemFactory,
            }.Randomize(Settings.SecretRewardsPhysicalSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeStartPosition)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing start positions");
            new TR1StartPositionRandomizer
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

        if (!monitor.IsCancelled && Settings.RandomizeOutfits)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing Lara's appearance");
            new TR1OutfitRandomizer
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

        if (!monitor.IsCancelled && Settings.RandomizeAudio)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing audio tracks");
            new TR1AudioRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.AudioSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeNightMode)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode");
            new TR1NightModeRandomizer
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

        if (!monitor.IsCancelled && Settings.RandomizeTextures)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing textures");
            new TR1TextureRandomizer
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

        AmendTitleAndCredits(scriptEditor, monitor);
    }

    private void AmendTitleAndCredits(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
    {
        var script = scriptEditor.Script as TRXScript;

        string mainMenuPic = Path.GetFileName(script.MainMenuPicture);
        string backupTitle = Path.Combine(GetReadBasePath(), mainMenuPic);
        if (File.Exists(backupTitle))
        {
            string editedTitle = Path.Combine(GetWriteBasePath(), "title.png");
            TRImage bg = new(backupTitle);
            TRImage badge = new("Resources/Shared/Graphics/tr1badge-small.png");
            bg.Import(badge, scriptEditor.GameMode == GameMode.Gold ? _goldBadgePos : _regularBadgePos, true);

            if (scriptEditor.GameMode == GameMode.Combined)
            {
                TRImage comboBadge = new("Resources/Shared/Graphics/rando-business.png");
                bg.Import(comboBadge, new(29, 880), true);
            }

            bg.Save(editedTitle);

            string titlePath = "data/title.png";
            script.MainMenuPicture = titlePath;
            script.AddAdditionalBackupFile(titlePath);
        }

        {
            string creditFile = Path.Combine(_io.OutputDirectory.FullName, "trrando.png");
            string creditPath = "data/trrando.png";

            TRImage bg = new(1920, 1080);
            TRImage badge = new("Resources/Shared/Graphics/tr1badge-large.png");
            bg.Fill(Color.Black);
            bg.Import(badge, new(960 - badge.Width / 2, 540 - badge.Height / 2), true);
            bg.Save(creditFile);

            var finalLevel = scriptEditor.Levels.ToList().Find(l => l.IsFinalLevel) as TRXScriptedLevel;
            finalLevel.AddSequenceBefore(LevelSequenceType.Total_Stats, new DisplayPictureSequence
            {
                Type = LevelSequenceType.Display_Picture,
                DisplayTime = 5,
                Path = creditFile,
            });

            script.AddAdditionalBackupFile(creditPath);
        }

        scriptEditor.SaveScript();
        monitor.FireSaveStateChanged(1);
    }
}
