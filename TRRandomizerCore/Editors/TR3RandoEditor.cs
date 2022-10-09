using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRGE.Coord;
using TRGE.Core;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Randomizers;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Editors
{
    public class TR3RandoEditor : TR3LevelEditor, ISettingsProvider
    {
        public RandomizerSettings Settings { get; private set; }

        public TR3RandoEditor(TRDirectoryIOArgs args, TREdition edition)
            : base(args, edition) { }

        protected override void ApplyConfig(Config config)
        {
            Settings = new RandomizerSettings
            {
                ExcludableEnemies = JsonConvert.DeserializeObject<Dictionary<short, string>>(File.ReadAllText(@"Resources\TR3\Restrictions\excludable_enemies.json"))
            };
            Settings.ApplyConfig(config);
        }

        protected override void StoreConfig(Config config)
        {
            Settings.StoreConfig(config);
        }

        protected override int GetSaveTarget(int numLevels)
        {
            // Add to the target as appropriate when each randomizer is implemented. Once all
            // randomizers are implemented, just call Settings.GetSaveTarget(numLevels) per TR2.
            int target = base.GetSaveTarget(numLevels);

            if (Settings.RandomizeGameStrings || Settings.ReassignPuzzleNames)
            {
                target++;
            }

            if (Settings.RandomizeSequencing)
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

            if (Settings.RandomizeAudio)
            {
                target += numLevels;
            }

            if (Settings.RandomizeOutfits)
            {
                // *2 because of multithreaded approach
                target += numLevels * 2;
            }

            if (Settings.RandomizeItems)
            {
                target += numLevels;
            }

            if (Settings.RandomizeSecretRewardsPhysical)
            {
                target += numLevels;
            }

            if (Settings.RandomizeNightMode)
            {
                target += numLevels;
            }

            if (Settings.RandomizeTextures)
            {
                // *3 because of multithreaded approach
                target += numLevels * 3;
            }

            if (Settings.RandomizeStartPosition)
            {
                target += numLevels;
            }

            // Environment randomizer always runs
            target += numLevels;

            return target;
        }

        protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
        {
            List<TR3ScriptedLevel> levels = new List<TR3ScriptedLevel>
            (
                scriptEditor.EnabledScriptedLevels.Cast<TR3ScriptedLevel>().ToList()
            );

            if (scriptEditor.GymAvailable)
            {
                levels.Add(scriptEditor.AssaultLevel as TR3ScriptedLevel);
            }

            // Each processor will have a reference to the script editor, so can
            // make on-the-fly changes as required.
            TR23ScriptEditor tr23ScriptEditor = scriptEditor as TR23ScriptEditor;
            string wipDirectory = _io.WIPOutputDirectory.FullName;

            if (Settings.DevelopmentMode)
            {
                (tr23ScriptEditor.Script as TR23Script).LevelSelectEnabled = true;
                scriptEditor.SaveScript();
            }

            // Shared tracker objects between randomizers
            ItemFactory itemFactory = new ItemFactory(@"Resources\TR3\Items\repurposable_items.json");
            TR3TextureMonitorBroker textureMonitor = new TR3TextureMonitorBroker();

            using (textureMonitor)
            {
                if (!monitor.IsCancelled && (Settings.RandomizeGameStrings || Settings.ReassignPuzzleNames))
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Adjusting game strings");
                    new TR3GameStringRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings
                    }.Randomize(Settings.GameStringsSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeSequencing)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Running level sequence checks");
                    new TR3SequenceProcessor
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        GlobeDisplay = Settings.GlobeDisplay,
                        TextureMonitor = textureMonitor,
                        ItemFactory = itemFactory
                    }.Run();
                }

                TR3EnvironmentRandomizer environmentRandomizer = new TR3EnvironmentRandomizer
                {
                    ScriptEditor = tr23ScriptEditor,
                    Levels = levels,
                    BasePath = wipDirectory,
                    SaveMonitor = monitor,
                    Settings = Settings,
                    TextureMonitor = textureMonitor
                };

                if (!monitor.IsCancelled && Settings.RandomizeSecrets)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secrets");
                    new TR3SecretRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        TextureMonitor = textureMonitor,
                        ItemFactory = itemFactory,
                        MirrorLevels = environmentRandomizer.AllocateMirroredLevels(Settings.EnvironmentSeed)
                    }.Randomize(Settings.SecretSeed);
                }

                //Ensure item rando executes before enemy rando - as enemies may be assigned key items
                //so we need to make sure the enemy rando can know this in advance.
                if (!monitor.IsCancelled && Settings.RandomizeItems)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing items");
                    new TR3ItemRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
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
                    new TR3SecretRewardRandomizer
                    {
                        ScriptEditor = scriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings
                    }.Randomize(Settings.SecretRewardsPhysicalSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeEnemies)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing enemies");
                    new TR3EnemyRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        TextureMonitor = textureMonitor,
                        ItemFactory = itemFactory
                    }.Randomize(Settings.EnemySeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeStartPosition)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing start positions");
                    new TR3StartPositionRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
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
                    new TR3AudioRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings
                    }.Randomize(Settings.AudioSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeOutfits)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing outfits");
                    new TR3OutfitRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        TextureMonitor = textureMonitor
                    }.Randomize(Settings.OutfitSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeNightMode && !Settings.RandomizeVfx)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode");
                    new TR3NightModeRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        TextureMonitor = textureMonitor
                    }.Randomize(Settings.NightModeSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeNightMode && Settings.RandomizeVfx)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Applying Filter to Random Levels");
                    new TR3VfxRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings
                    }.Randomize(Settings.NightModeSeed);
                }

                if (!monitor.IsCancelled)
                {
                    if (Settings.RandomizeTextures)
                    {
                        monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing textures");
                        new TR3TextureRandomizer
                        {
                            ScriptEditor = tr23ScriptEditor,
                            Levels = levels,
                            BasePath = wipDirectory,
                            SaveMonitor = monitor,
                            Settings = Settings,
                            TextureMonitor = textureMonitor
                        }.Randomize(Settings.TextureSeed);
                    }
                    else if (Settings.RandomizeNightMode && !Settings.RandomizeVfx)
                    {
                        monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode textures");
                        new TR3TextureRandomizer
                        {
                            ScriptEditor = tr23ScriptEditor,
                            Levels = levels,
                            BasePath = wipDirectory,
                            SaveMonitor = monitor,
                            Settings = Settings,
                            TextureMonitor = textureMonitor
                        }.Randomize(Settings.NightModeSeed);
                    }
                }
            }
        }
    }
}