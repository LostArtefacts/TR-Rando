using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRGE.Coord;
using TRGE.Core;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Randomizers;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Editors
{
    public class TR1RandoEditor : TR1LevelEditor, ISettingsProvider
    {
        public RandomizerSettings Settings { get; private set; }

        public TR1RandoEditor(TRDirectoryIOArgs args, TREdition edition)
            : base(args, edition) { }

        protected override void ApplyConfig(Config config)
        {
            Settings = new RandomizerSettings()
            {
                ExcludableEnemies = JsonConvert.DeserializeObject<Dictionary<short, string>>(File.ReadAllText(@"Resources\TR1\Restrictions\excludable_enemies.json"))
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
                target += numLevels;
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
            target += numLevels;

            return target;
        }

        protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
        {
            List<TR1ScriptedLevel> levels = new List<TR1ScriptedLevel>
            (
                scriptEditor.EnabledScriptedLevels.Cast<TR1ScriptedLevel>().ToList()
            );

            if (scriptEditor.GymAvailable)
            {
                levels.Add(scriptEditor.AssaultLevel as TR1ScriptedLevel);
            }

            string backupDirectory = _io.BackupDirectory.FullName;
            string wipDirectory = _io.WIPOutputDirectory.FullName;

            bool isTomb1Main = scriptEditor.Edition.IsCommunityPatch;
            if (Settings.DevelopmentMode && isTomb1Main)
            {
                (scriptEditor as TR1ScriptEditor).EnableCheats = true;
                scriptEditor.SaveScript();
            }

            ItemFactory itemFactory = new ItemFactory(@"Resources\TR1\Items\repurposable_items.json");
            TR1TextureMonitorBroker textureMonitor = new TR1TextureMonitorBroker();

            TR1EnvironmentRandomizer environmentRandomizer = new TR1EnvironmentRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            };

            using (textureMonitor)
            {
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
                        MirrorLevels = environmentRandomizer.AllocateMirroredLevels(Settings.EnvironmentSeed)
                    }.Randomize(Settings.EnemySeed);
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
                    new TR1ItemRandomizer
                    {
                        ScriptEditor = scriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        ItemFactory = itemFactory
                    }.Randomize(Settings.ItemSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeSecretRewardsPhysical)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secret rewards");
                    new TR1SecretRewardRandomizer
                    {
                        ScriptEditor = scriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings
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
                        SaveMonitor = monitor,
                        Settings = Settings
                    }.Randomize(Settings.StartPositionSeed);
                }

                if (!monitor.IsCancelled)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, Settings.RandomizeEnvironment ? "Randomizing environment" : "Applying default environment packs");
                    environmentRandomizer.Randomize(Settings.EnvironmentSeed);
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

                if (!monitor.IsCancelled && Settings.RandomizeOutfits)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing Lara's appearance");
                    new TR1OutfitRandomizer
                    {
                        ScriptEditor = scriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        TextureMonitor = textureMonitor
                    }.Randomize(Settings.OutfitSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeNightMode)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode");
                    new TR1NightModeRandomizer
                    {
                        ScriptEditor = scriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
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
            }
        }
    }
}