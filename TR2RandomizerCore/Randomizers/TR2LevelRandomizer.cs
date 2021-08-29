using System;
using System.Collections.Generic;
using System.Linq;
using TR2RandomizerCore.Globalisation;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Processors;
using TR2RandomizerCore.Utilities;
using TRGE.Coord;
using TRGE.Core;

namespace TR2RandomizerCore.Randomizers
{
    public class TR2LevelRandomizer : TR2LevelEditor
    {
        internal bool RandomizeSecrets { get; set; }
        internal bool RandomizeItems { get; set; }
        internal bool RandomizeEnemies { get; set; }
        internal bool RandomizeTextures { get; set; }
        internal bool RandomizeOutfits { get; set; }
        internal bool RandomizeGameStrings { get; set; }
        internal bool RandomizeNightMode { get; set; }
        internal bool RandomizeAudio { get; set; }
        internal bool RandomizeStartPosition { get; set; }
        internal bool RandomizeEnvironment { get; set; }

        internal int SecretSeed { get; set; }
        internal int ItemSeed { get; set; }
        internal int EnemySeed { get; set; }
        internal int TextureSeed { get; set; }
        internal int OutfitSeed { get; set; }
        internal int GameStringsSeed { get; set; }
        internal int NightModeSeed { get; set; }
        internal int AudioSeed { get; set; }
        internal int StartPositionSeed { get; set; }
        internal int EnvironmentSeed { get; set; }

        internal bool HardSecrets { get; set; }
        internal bool IncludeKeyItems { get; set; }
        internal bool DevelopmentMode { get; set; }
        internal bool PersistTextureVariants { get; set; }
        internal bool RetainKeySpriteTextures { get; set; }
        internal bool RetainSecretSpriteTextures { get; set; }
        internal bool CrossLevelEnemies { get; set; }
        internal bool ProtectMonks { get; set; }
        internal bool DocileBirdMonsters { get; set; }
        internal RandoDifficulty RandoEnemyDifficulty { get; set; }
        internal bool GlitchedSecrets { get; set; }
        internal bool PersistOutfits { get; set; }
        internal bool RandomlyCutHair { get; set; }
        internal bool RemoveRobeDagger { get; set; }
        internal bool EnableInvisibility { get; set; }
        internal bool RetainLevelNames { get; set; }
        internal bool RetainKeyItemNames { get; set; }
        internal Language GameStringLanguage { get; set; }
        internal uint NightModeCount { get; set; }
        internal bool ChangeTriggerTracks { get; set; }
        internal bool RotateStartPositionOnly { get; set; }
        internal bool RandomizeWaterLevels { get; set; }
        internal bool RandomizeSlotPositions { get; set; }
        internal bool AutoLaunchGame { get; set; }

        internal bool DeduplicateTextures => RandomizeTextures || RandomizeNightMode || (RandomizeEnemies && CrossLevelEnemies) || RandomizeOutfits;
        internal bool ReassignPuzzleNames => RandomizeEnemies && CrossLevelEnemies;

        internal TR2LevelRandomizer(TRDirectoryIOArgs args)
            : base(args) { }

        protected override void ApplyConfig(Config config)
        {
            int defaultSeed = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            RandomizeSecrets = config.GetBool(nameof(RandomizeSecrets));
            SecretSeed = config.GetInt(nameof(SecretSeed), defaultSeed);
            HardSecrets = config.GetBool(nameof(HardSecrets));
            GlitchedSecrets = config.GetBool(nameof(GlitchedSecrets));

            RandomizeItems = config.GetBool(nameof(RandomizeItems));
            ItemSeed = config.GetInt(nameof(ItemSeed), defaultSeed);
            IncludeKeyItems = config.GetBool(nameof(IncludeKeyItems), true);

            RandomizeEnemies = config.GetBool(nameof(RandomizeEnemies));
            EnemySeed = config.GetInt(nameof(EnemySeed), defaultSeed);
            CrossLevelEnemies = config.GetBool(nameof(CrossLevelEnemies), true);
            ProtectMonks = config.GetBool(nameof(ProtectMonks), true);
            DocileBirdMonsters = config.GetBool(nameof(DocileBirdMonsters));
            RandoEnemyDifficulty = (RandoDifficulty)config.GetEnum(nameof(RandoEnemyDifficulty), typeof(RandoDifficulty), RandoDifficulty.Default);

            RandomizeTextures = config.GetBool(nameof(RandomizeTextures));
            TextureSeed = config.GetInt(nameof(TextureSeed), defaultSeed);
            PersistTextureVariants = config.GetBool(nameof(PersistTextureVariants));
            RetainKeySpriteTextures = config.GetBool(nameof(RetainKeySpriteTextures), true);
            RetainSecretSpriteTextures = config.GetBool(nameof(RetainSecretSpriteTextures), true);

            RandomizeOutfits = config.GetBool(nameof(RandomizeOutfits));
            OutfitSeed = config.GetInt(nameof(OutfitSeed), defaultSeed);
            PersistOutfits = config.GetBool(nameof(PersistOutfits));
            RandomlyCutHair = config.GetBool(nameof(RandomlyCutHair), true);
            RemoveRobeDagger = config.GetBool(nameof(RemoveRobeDagger), true);
            EnableInvisibility = config.GetBool(nameof(EnableInvisibility));

            RandomizeGameStrings = config.GetBool(nameof(RandomizeGameStrings));
            GameStringsSeed = config.GetInt(nameof(GameStringsSeed), defaultSeed);
            RetainKeyItemNames = config.GetBool(nameof(RetainKeyItemNames));
            RetainLevelNames = config.GetBool(nameof(RetainLevelNames));
            GameStringLanguage = G11N.Instance.GetLanguage(config.GetString(nameof(GameStringLanguage), Language.DefaultTag));

            RandomizeNightMode = config.GetBool(nameof(RandomizeNightMode));
            NightModeSeed = config.GetInt(nameof(NightModeSeed), defaultSeed);
            NightModeCount = config.GetUInt(nameof(NightModeCount), 1);

            // Note that the main audio config options are held in TRGE for now
            ChangeTriggerTracks = config.GetBool(nameof(ChangeTriggerTracks), true);

            RandomizeStartPosition = config.GetBool(nameof(RandomizeStartPosition));
            StartPositionSeed = config.GetInt(nameof(StartPositionSeed), defaultSeed);
            RotateStartPositionOnly = config.GetBool(nameof(RotateStartPositionOnly));

            RandomizeEnvironment = config.GetBool(nameof(RandomizeEnvironment));
            EnvironmentSeed = config.GetInt(nameof(EnvironmentSeed), defaultSeed);
            RandomizeWaterLevels = config.GetBool(nameof(RandomizeWaterLevels), true);
            RandomizeSlotPositions = config.GetBool(nameof(RandomizeSlotPositions), true);

            DevelopmentMode = config.GetBool(nameof(DevelopmentMode));
            AutoLaunchGame = config.GetBool(nameof(AutoLaunchGame));
        }

        protected override void StoreConfig(Config config)
        {
            config[nameof(RandomizeSecrets)] = RandomizeSecrets;
            config[nameof(SecretSeed)] = SecretSeed;
            config[nameof(HardSecrets)] = HardSecrets;
            config[nameof(GlitchedSecrets)] = GlitchedSecrets;

            config[nameof(RandomizeItems)] = RandomizeItems;
            config[nameof(ItemSeed)] = ItemSeed;
            config[nameof(IncludeKeyItems)] = IncludeKeyItems;

            config[nameof(RandomizeEnemies)] = RandomizeEnemies;
            config[nameof(EnemySeed)] = EnemySeed;
            config[nameof(CrossLevelEnemies)] = CrossLevelEnemies;
            config[nameof(ProtectMonks)] = ProtectMonks;
            config[nameof(DocileBirdMonsters)] = DocileBirdMonsters;
            config[nameof(RandoEnemyDifficulty)] = RandoEnemyDifficulty;

            config[nameof(RandomizeTextures)] = RandomizeTextures;
            config[nameof(TextureSeed)] = TextureSeed;
            config[nameof(PersistTextureVariants)] = PersistTextureVariants;
            config[nameof(RetainKeySpriteTextures)] = RetainKeySpriteTextures;
            config[nameof(RetainSecretSpriteTextures)] = RetainSecretSpriteTextures;

            config[nameof(RandomizeOutfits)] = RandomizeOutfits;
            config[nameof(OutfitSeed)] = OutfitSeed;
            config[nameof(PersistOutfits)] = PersistOutfits;
            config[nameof(RandomlyCutHair)] = RandomlyCutHair;
            config[nameof(RemoveRobeDagger)] = RemoveRobeDagger;
            config[nameof(EnableInvisibility)] = EnableInvisibility;

            config[nameof(RandomizeGameStrings)] = RandomizeGameStrings;
            config[nameof(GameStringsSeed)] = GameStringsSeed;
            config[nameof(RetainKeyItemNames)] = RetainKeyItemNames;
            config[nameof(RetainLevelNames)] = RetainLevelNames;
            config[nameof(GameStringLanguage)] = GameStringLanguage.Tag;

            config[nameof(RandomizeNightMode)] = RandomizeNightMode;
            config[nameof(NightModeSeed)] = NightModeSeed;
            config[nameof(NightModeCount)] = NightModeCount;

            config[nameof(ChangeTriggerTracks)] = ChangeTriggerTracks;

            config[nameof(RandomizeStartPosition)] = RandomizeStartPosition;
            config[nameof(StartPositionSeed)] = StartPositionSeed;
            config[nameof(RotateStartPositionOnly)] = RotateStartPositionOnly;

            config[nameof(RandomizeEnvironment)] = RandomizeEnvironment;
            config[nameof(EnvironmentSeed)] = EnvironmentSeed;
            config[nameof(RandomizeWaterLevels)] = RandomizeWaterLevels;
            config[nameof(RandomizeSlotPositions)] = RandomizeSlotPositions;

            config[nameof(DevelopmentMode)] = DevelopmentMode;
            config[nameof(AutoLaunchGame)] = AutoLaunchGame;
        }

        /// <summary>
        /// This is called before the script data is saved so gives us an opportunity to 
        /// customise the script outwith TRGE before the main SaveImpl.
        /// </summary>
        protected override void PreSaveImpl(AbstractTRScriptEditor scriptEditor)
        {
            // Either fully randomize the gamestrings, or allow for specific strings
            // to be replaced if models are being moved around as a result of cross-
            // level enemies (e.g. Dagger of Xian).
            if (RandomizeGameStrings || ReassignPuzzleNames)
            {
                GameStringRandomizer stringRandomizer = new GameStringRandomizer
                {
                    ScriptEditor = scriptEditor as TR23ScriptEditor,
                    RetainKeyItemNames = RetainKeyItemNames,
                    RetainLevelNames = RetainLevelNames,
                    Language = GameStringLanguage,
                    RandomizeAllStrings = RandomizeGameStrings,
                    ReassignPuzzleNames = ReassignPuzzleNames
                };
                stringRandomizer.Randomize(GameStringsSeed);
            }
        }

        protected override int GetSaveTarget(int numLevels)
        {
            // TODO: move these target calculations into the relevant classes - they don't belong here
            int target = base.GetSaveTarget(numLevels);
            if (RandomizeNightMode)
            {
                target += numLevels;
                if (!RandomizeTextures)
                {
                    target += numLevels;
                }
            }
            if (RandomizeSecrets) target += numLevels;
            if (RandomizeAudio) target += numLevels;
            if (RandomizeItems) target += numLevels * 2; // standard/key rando followed by unarmed logic after enemy rando
            if (RandomizeStartPosition) target += numLevels;
            if (DeduplicateTextures) target += numLevels * 2;
            if (RandomizeEnemies) target += CrossLevelEnemies ? numLevels * 3 : numLevels;
            if (RandomizeTextures) target += numLevels * 3;
            if (RandomizeOutfits) target += numLevels * 2;

            target += numLevels; // Environment randomizer always runs

            return target;
        }

        /// <summary>
        /// The scripting randomization will be complete at this stage, so access to which levels
        /// are available, which are unarmed, ammoless etc is accurate at this point. The list of
        /// levels is always in the original sequence when accessed here, but can be sorted by level
        /// sequence if it's preferred to add an additional randomization factor. The save monitor
        /// is used to report progress and checks should be performed regularly on IsCancelled.
        /// </summary>
        protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
        {
            List<TR23ScriptedLevel> levels = new List<TR23ScriptedLevel>
            (
                scriptEditor.EnabledScriptedLevels.Cast<TR23ScriptedLevel>().ToList()
            );

            if (scriptEditor.GymAvailable)
            {
                levels.Add(scriptEditor.AssaultLevel as TR23ScriptedLevel);
            }

            // Optionally sort based on randomized sequencing. Perhaps this should be an option for users?
            /*levels.Sort(delegate (TR23ScriptedLevel lvl1, TR23ScriptedLevel lvl2)
            {
                return lvl1.Sequence.CompareTo(lvl2.Sequence);
            });*/

            string wipDirectory = _io.WIPOutputDirectory.FullName;

            // Texture monitoring is needed between enemy and texture randomization
            // to track where imported enemies are placed.
            using (TexturePositionMonitorBroker textureMonitor = new TexturePositionMonitorBroker())
            {
                if (!monitor.IsCancelled && DeduplicateTextures)
                {
                    // This is needed to make as much space as possible available for cross-level enemies.
                    // We do this if we are implementing cross-level enemies OR if randomizing textures,
                    // as the texture mapping is optimised for levels that have been deduplicated.
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Deduplicating textures");
                    new TextureDeduplicator
                    {
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor
                    }.Deduplicate();
                }

                if (!monitor.IsCancelled && RandomizeSecrets)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secrets");
                    new SecretReplacer
                    {
                        AllowHard = HardSecrets,
                        AllowGlitched = GlitchedSecrets,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        IsDevelopmentModeOn = DevelopmentMode
                    }.Randomize(SecretSeed);
                }

                ItemRandomizer itemRandomizer = null;
                if (!monitor.IsCancelled && RandomizeItems)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, string.Format("Randomizing standard{0} items", IncludeKeyItems ? " and key" : string.Empty));
                    (itemRandomizer = new ItemRandomizer
                    {
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        IncludeKeyItems = IncludeKeyItems,
                        PerformEnemyWeighting = RandomizeEnemies && CrossLevelEnemies,
                        TextureMonitor = textureMonitor,
                        IsDevelopmentModeOn = DevelopmentMode
                    }).Randomize(ItemSeed);
                }

                if (!monitor.IsCancelled && RandomizeEnemies)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing enemies");
                    new EnemyRandomizer
                    {
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        CrossLevelEnemies = CrossLevelEnemies,
                        ProtectMonks = ProtectMonks,
                        DocileBirdMonsters = DocileBirdMonsters,
                        TextureMonitor = textureMonitor,
                        RandoEnemyDifficulty = RandoEnemyDifficulty
                    }.Randomize(EnemySeed);
                }

                // Randomize ammo/weapon in unarmed levels post enemy randomization
                if (!monitor.IsCancelled && RandomizeItems)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing unarmed level items");
                    itemRandomizer.RandomizeAmmo();
                }

                if (!monitor.IsCancelled)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, RandomizeEnvironment ? "Randomizing environment" : "Applying default environment packs");
                    new EnvironmentRandomizer
                    {
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        EnforcedModeOnly = !RandomizeEnvironment,
                        RandomizeWater = RandomizeWaterLevels,
                        RandomizeSlots = RandomizeSlotPositions
                    }.Randomize(EnvironmentSeed);
                }

                if (!monitor.IsCancelled && RandomizeAudio)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing audio tracks");
                    new AudioRandomizer
                    {
                        ScriptEditor = scriptEditor as TR23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        ChangeTriggerTracks = ChangeTriggerTracks
                    }.Randomize(AudioSeed);
                }

                if (!monitor.IsCancelled && RandomizeStartPosition)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing start positions");
                    new StartPositionRandomizer
                    {
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        RotateOnly = RotateStartPositionOnly,
                        DevelopmentMode = DevelopmentMode
                    }.Randomize(StartPositionSeed);
                }

                if (!monitor.IsCancelled && RandomizeOutfits)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing outfits");
                    new OutfitRandomizer
                    {
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        PersistOutfits = PersistOutfits,
                        RandomlyCutHair = RandomlyCutHair,
                        RemoveRobeDagger = RemoveRobeDagger,
                        EnableInvisibility = EnableInvisibility,
                        TextureMonitor = textureMonitor
                    }.Randomize(OutfitSeed);
                }

                if (!monitor.IsCancelled && RandomizeNightMode)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode");
                    new NightModeRandomizer
                    {
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        NumLevels = NightModeCount
                    }.Randomize(NightModeSeed);
                }

                if (!monitor.IsCancelled)
                {
                    if (RandomizeTextures)
                    {
                        monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing textures");
                        new TextureRandomizer
                        {
                            Levels = levels,
                            BasePath = wipDirectory,
                            SaveMonitor = monitor,
                            PersistVariants = PersistTextureVariants,
                            RetainKeySprites = RetainKeySpriteTextures,
                            RetainSecretSprites = RetainSecretSpriteTextures,
                            NightModeOnly = !RandomizeTextures,
                            TextureMonitor = textureMonitor
                        }.Randomize(TextureSeed);
                    }
                    else if (RandomizeNightMode)
                    {
                        monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode textures");
                        new TextureRandomizer
                        {
                            Levels = levels,
                            BasePath = wipDirectory,
                            SaveMonitor = monitor,
                            NightModeOnly = true,
                            TextureMonitor = textureMonitor
                        }.Randomize(NightModeSeed);
                    }
                }
            }
        }
    }
}