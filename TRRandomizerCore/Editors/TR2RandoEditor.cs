using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Randomizers;
using TRRandomizerCore.Utilities;
using TRGE.Coord;
using TRGE.Core;

namespace TRRandomizerCore.Editors
{
    public class TR2RandoEditor : TR2LevelEditor, ISettingsProvider
    {
        public RandomizerSettings Settings { get; private set; }

        public TR2RandoEditor(TRDirectoryIOArgs args, TREdition edition)
            : base(args, edition) { }

        protected override void ApplyConfig(Config config)
        {
            Settings = new RandomizerSettings();
            Settings.ApplyConfig(config);
        }

        protected override void StoreConfig(Config config)
        {
            Settings.StoreConfig(config);
        }

        protected override int GetSaveTarget(int numLevels)
        {
            return base.GetSaveTarget(numLevels) + Settings.GetSaveTarget(numLevels);
        }

        protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
        {
            List<TR2ScriptedLevel> levels = new List<TR2ScriptedLevel>
            (
                scriptEditor.EnabledScriptedLevels.Cast<TR2ScriptedLevel>().ToList()
            );

            if (scriptEditor.GymAvailable)
            {
                levels.Add(scriptEditor.AssaultLevel as TR2ScriptedLevel);
            }

            // Each processor will have a reference to the script editor, so can
            // make on-the-fly changes as required.
            TR23ScriptEditor tr23ScriptEditor = scriptEditor as TR23ScriptEditor;
            string wipDirectory = _io.WIPOutputDirectory.FullName;

            // Texture monitoring is needed between enemy and texture randomization
            // to track where imported enemies are placed.
            using (TexturePositionMonitorBroker textureMonitor = new TexturePositionMonitorBroker())
            {
                if (!monitor.IsCancelled && (Settings.RandomizeGameStrings || Settings.ReassignPuzzleNames))
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Adjusting game strings");
                    new TR2GameStringRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings
                    }.Randomize(Settings.GameStringsSeed);
                }

                if (!monitor.IsCancelled && Settings.DeduplicateTextures)
                {
                    // This is needed to make as much space as possible available for cross-level enemies.
                    // We do this if we are implementing cross-level enemies OR if randomizing textures,
                    // as the texture mapping is optimised for levels that have been deduplicated.
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Deduplicating textures");
                    new TR2TextureDeduplicator
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor
                    }.Deduplicate();
                }

                if (!monitor.IsCancelled && Settings.RandomizeSecrets)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secrets");
                    new TR2SecretRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings
                    }.Randomize(Settings.SecretSeed);
                }

                TR2ItemRandomizer itemRandomizer = null;
                if (!monitor.IsCancelled && Settings.RandomizeItems)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, string.Format("Randomizing standard{0} items", Settings.IncludeKeyItems ? " and key" : string.Empty));
                    (itemRandomizer = new TR2ItemRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        TextureMonitor = textureMonitor
                    }).Randomize(Settings.ItemSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeEnemies)
                {
                    if (Settings.CrossLevelEnemies)
                    {
                        // For now all P2 items become P3 to avoid dragon issues. This must take place after Item
                        // randomization for P2/3 zoning because P2 entities will become P3.
                        monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Adjusting level models");
                        new TR2ModelAdjuster
                        {
                            ScriptEditor = tr23ScriptEditor,
                            Levels = levels,
                            BasePath = wipDirectory,
                            SaveMonitor = monitor
                        }.AdjustModels();
                    }

                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing enemies");
                    new TR2EnemyRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        TextureMonitor = textureMonitor
                    }.Randomize(Settings.EnemySeed);
                }

                // Randomize ammo/weapon in unarmed levels post enemy randomization
                if (!monitor.IsCancelled && Settings.RandomizeItems)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing unarmed level items");
                    itemRandomizer.RandomizeAmmo();
                }

                if (!monitor.IsCancelled && Settings.RandomizeStartPosition)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing start positions");
                    new TR2StartPositionRandomizer
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
                    new TR2EnvironmentRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        Settings = Settings,
                        TextureMonitor = textureMonitor
                    }.Randomize(Settings.EnvironmentSeed);
                }

                if (!monitor.IsCancelled && Settings.RandomizeAudio)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing audio tracks");
                    new TR2AudioRandomizer
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
                    new TR2OutfitRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
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
                    new TR2NightModeRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
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
                            ScriptEditor = tr23ScriptEditor,
                            Levels = levels,
                            BasePath = wipDirectory,
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