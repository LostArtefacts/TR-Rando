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
        internal ItemDifficulty RandoItemDifficulty { get; set; }
        internal bool PersistTextureVariants { get; set; }
        internal bool RetainKeySpriteTextures { get; set; }
        internal bool RetainSecretSpriteTextures { get; set; }
        internal bool CrossLevelEnemies { get; set; }
        internal bool ProtectMonks { get; set; }
        internal bool DocileBirdMonsters { get; set; }
        internal RandoDifficulty RandoEnemyDifficulty { get; set; }
        internal bool GlitchedSecrets { get; set; }
        internal bool PersistOutfits { get; set; }
        internal bool RemoveRobeDagger { get; set; }
        internal uint HaircutLevelCount { get; set; }
        internal bool AssaultCourseHaircut { get; set; }
        internal uint InvisibleLevelCount { get; set; }
        internal bool AssaultCourseInvisible { get; set; }
        internal bool RetainLevelNames { get; set; }
        internal bool RetainKeyItemNames { get; set; }
        internal Language GameStringLanguage { get; set; }
        internal uint NightModeCount { get; set; }
        internal uint NightModeDarkness { get; set; }
        internal bool NightModeAssaultCourse { get; set; }
        internal bool ChangeTriggerTracks { get; set; }
        internal bool RotateStartPositionOnly { get; set; }
        internal bool RandomizeWaterLevels { get; set; }
        internal bool RandomizeSlotPositions { get; set; }
        internal bool RandomizeLadders { get; set; }
        internal uint MirroredLevelCount { get; set; }
        internal bool MirrorAssaultCourse { get; set; }
        internal bool AutoLaunchGame { get; set; }

        internal bool DeduplicateTextures => RandomizeTextures || RandomizeNightMode || (RandomizeEnemies && CrossLevelEnemies) || RandomizeOutfits;// || RandomizeEnvironment; // Not needed until trap model import takes place
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
            RandoItemDifficulty = (ItemDifficulty)config.GetEnum(nameof(RandoItemDifficulty), typeof(ItemDifficulty), ItemDifficulty.Default);

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
            RemoveRobeDagger = config.GetBool(nameof(RemoveRobeDagger), true);
            HaircutLevelCount = config.GetUInt(nameof(HaircutLevelCount), 9);
            AssaultCourseHaircut = config.GetBool(nameof(AssaultCourseHaircut), true);
            InvisibleLevelCount = config.GetUInt(nameof(InvisibleLevelCount), 2);
            AssaultCourseInvisible = config.GetBool(nameof(AssaultCourseInvisible));

            RandomizeGameStrings = config.GetBool(nameof(RandomizeGameStrings));
            GameStringsSeed = config.GetInt(nameof(GameStringsSeed), defaultSeed);
            RetainKeyItemNames = config.GetBool(nameof(RetainKeyItemNames));
            RetainLevelNames = config.GetBool(nameof(RetainLevelNames));
            GameStringLanguage = G11N.Instance.GetLanguage(config.GetString(nameof(GameStringLanguage), Language.DefaultTag));

            RandomizeNightMode = config.GetBool(nameof(RandomizeNightMode));
            NightModeSeed = config.GetInt(nameof(NightModeSeed), defaultSeed);
            NightModeCount = config.GetUInt(nameof(NightModeCount), 1);
            NightModeDarkness = config.GetUInt(nameof(NightModeDarkness), 4);
            NightModeAssaultCourse = config.GetBool(nameof(NightModeAssaultCourse), true);

            // Note that the main audio config options are held in TRGE for now
            ChangeTriggerTracks = config.GetBool(nameof(ChangeTriggerTracks), true);

            RandomizeStartPosition = config.GetBool(nameof(RandomizeStartPosition));
            StartPositionSeed = config.GetInt(nameof(StartPositionSeed), defaultSeed);
            RotateStartPositionOnly = config.GetBool(nameof(RotateStartPositionOnly));

            RandomizeEnvironment = config.GetBool(nameof(RandomizeEnvironment));
            EnvironmentSeed = config.GetInt(nameof(EnvironmentSeed), defaultSeed);
            RandomizeWaterLevels = config.GetBool(nameof(RandomizeWaterLevels), true);
            RandomizeSlotPositions = config.GetBool(nameof(RandomizeSlotPositions), true);
            RandomizeLadders = config.GetBool(nameof(RandomizeLadders), true);
            MirroredLevelCount = config.GetUInt(nameof(MirroredLevelCount), 9);
            MirrorAssaultCourse = config.GetBool(nameof(MirrorAssaultCourse), true);

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
            config[nameof(RandoItemDifficulty)] = RandoItemDifficulty;

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
            config[nameof(RemoveRobeDagger)] = RemoveRobeDagger;
            config[nameof(HaircutLevelCount)] = HaircutLevelCount;
            config[nameof(AssaultCourseHaircut)] = AssaultCourseHaircut;
            config[nameof(InvisibleLevelCount)] = InvisibleLevelCount;
            config[nameof(AssaultCourseInvisible)] = AssaultCourseInvisible;

            config[nameof(RandomizeGameStrings)] = RandomizeGameStrings;
            config[nameof(GameStringsSeed)] = GameStringsSeed;
            config[nameof(RetainKeyItemNames)] = RetainKeyItemNames;
            config[nameof(RetainLevelNames)] = RetainLevelNames;
            config[nameof(GameStringLanguage)] = GameStringLanguage.Tag;

            config[nameof(RandomizeNightMode)] = RandomizeNightMode;
            config[nameof(NightModeSeed)] = NightModeSeed;
            config[nameof(NightModeCount)] = NightModeCount;
            config[nameof(NightModeDarkness)] = NightModeDarkness;
            config[nameof(NightModeAssaultCourse)] = NightModeAssaultCourse;

            config[nameof(ChangeTriggerTracks)] = ChangeTriggerTracks;

            config[nameof(RandomizeStartPosition)] = RandomizeStartPosition;
            config[nameof(StartPositionSeed)] = StartPositionSeed;
            config[nameof(RotateStartPositionOnly)] = RotateStartPositionOnly;

            config[nameof(RandomizeEnvironment)] = RandomizeEnvironment;
            config[nameof(EnvironmentSeed)] = EnvironmentSeed;
            config[nameof(RandomizeWaterLevels)] = RandomizeWaterLevels;
            config[nameof(RandomizeSlotPositions)] = RandomizeSlotPositions;
            config[nameof(RandomizeLadders)] = RandomizeLadders;
            config[nameof(MirroredLevelCount)] = MirroredLevelCount;
            config[nameof(MirrorAssaultCourse)] = MirrorAssaultCourse;

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
            if (RandomizeEnemies) target += CrossLevelEnemies ? numLevels * 4 : numLevels; // 4 => 3 for multithreading work, 1 for ModelAdjuster
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

            // Each processor will have a reference to the script editor, so can
            // make on-the-fly changes as required.
            TR23ScriptEditor tr23ScriptEditor = scriptEditor as TR23ScriptEditor;
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
                        ScriptEditor = tr23ScriptEditor,
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
                        ScriptEditor = tr23ScriptEditor,
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
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        IncludeKeyItems = IncludeKeyItems,
                        Difficulty = RandoItemDifficulty,
                        PerformEnemyWeighting = RandomizeEnemies && CrossLevelEnemies,
                        TextureMonitor = textureMonitor,
                        IsDevelopmentModeOn = DevelopmentMode
                    }).Randomize(ItemSeed);
                }

                if (!monitor.IsCancelled && RandomizeEnemies)
                {
                    if (CrossLevelEnemies)
                    {
                        // For now all P2 items become P3 to avoid dragon issues. This must take place after Item
                        // randomization for P2/3 zoning because P2 entities will become P3.
                        monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Adjusting level models");
                        new ModelAdjuster
                        {
                            ScriptEditor = tr23ScriptEditor,
                            Levels = levels,
                            BasePath = wipDirectory,
                            SaveMonitor = monitor
                        }.AdjustModels();
                    }

                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing enemies");
                    new EnemyRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
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

                if (!monitor.IsCancelled && RandomizeStartPosition)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing start positions");
                    new StartPositionRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        RotateOnly = RotateStartPositionOnly,
                        DevelopmentMode = DevelopmentMode
                    }.Randomize(StartPositionSeed);
                }

                if (!monitor.IsCancelled)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, RandomizeEnvironment ? "Randomizing environment" : "Applying default environment packs");
                    new EnvironmentRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        EnforcedModeOnly = !RandomizeEnvironment,
                        NumMirrorLevels = MirroredLevelCount,
                        MirrorAssaultCourse = MirrorAssaultCourse,
                        RandomizeWater = RandomizeWaterLevels,
                        RandomizeSlots = RandomizeSlotPositions,
                        RandomizeLadders = RandomizeLadders,
                        TextureMonitor = textureMonitor
                    }.Randomize(EnvironmentSeed);
                }

                if (!monitor.IsCancelled && RandomizeAudio)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing audio tracks");
                    new AudioRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        ChangeTriggerTracks = ChangeTriggerTracks
                    }.Randomize(AudioSeed);
                }

                if (!monitor.IsCancelled && RandomizeOutfits)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing outfits");
                    new OutfitRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        PersistOutfits = PersistOutfits,
                        RemoveRobeDagger = RemoveRobeDagger,
                        NumHaircutLevels = HaircutLevelCount,
                        AssaultCourseHaircut = AssaultCourseHaircut,
                        NumInvisibleLevels = InvisibleLevelCount,
                        AssaultCourseInvisible = AssaultCourseInvisible,
                        TextureMonitor = textureMonitor
                    }.Randomize(OutfitSeed);
                }

                if (!monitor.IsCancelled && RandomizeNightMode)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing night mode");
                    new NightModeRandomizer
                    {
                        ScriptEditor = tr23ScriptEditor,
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        NumLevels = NightModeCount,
                        DarknessScale = NightModeDarkness,
                        NightModeAssaultCourse = NightModeAssaultCourse,
                        TextureMonitor = textureMonitor
                    }.Randomize(NightModeSeed);
                }

                if (!monitor.IsCancelled)
                {
                    if (RandomizeTextures)
                    {
                        monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing textures");
                        new TextureRandomizer
                        {
                            ScriptEditor = tr23ScriptEditor,
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
                            ScriptEditor = tr23ScriptEditor,
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