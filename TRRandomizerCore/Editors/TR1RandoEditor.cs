using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRGE.Coord;
using TRGE.Core;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Randomizers;

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
                    ItemFactory = itemFactory
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

            if (!monitor.IsCancelled)
            {
                monitor.FireSaveStateBeginning(TRSaveCategory.Custom, Settings.RandomizeEnvironment ? "Randomizing environment" : "Applying default environment packs");
                new TR1EnvironmentRandomizer
                {
                    ScriptEditor = scriptEditor,
                    Levels = levels,
                    BasePath = wipDirectory,
                    BackupPath = backupDirectory,
                    SaveMonitor = monitor,
                    Settings = Settings
                }.Randomize(Settings.EnvironmentSeed);
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
        }
    }
}