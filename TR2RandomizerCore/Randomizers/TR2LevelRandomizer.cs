using System;
using System.Collections.Generic;
using System.Linq;
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

        internal int SecretSeed { get; set; }
        internal int ItemSeed { get; set; }
        internal int EnemySeed { get; set; }
        internal int TextureSeed { get; set; }
        internal int OutfitSeed { get; set; }
        internal int GameStringsSeed { get; set; }

        internal bool HardSecrets { get; set; }
        internal bool IncludeKeyItems { get; set; }
        internal bool DevelopmentMode { get; set; }
        internal bool PersistTextureVariants { get; set; }
        internal bool RetainKeySpriteTextures { get; set; }
        internal bool CrossLevelEnemies { get; set; }
        internal bool ProtectMonks { get; set; }
        internal bool DocileBirdMonsters { get; set; }
        internal bool GlitchedSecrets { get; set; }
        internal bool PersistOutfits { get; set; }
        internal bool RandomlyCutHair { get; set; }
        internal bool RetainKeyItemNames { get; set; }
        internal bool AutoLaunchGame { get; set; }

        internal bool DeduplicateTextures => RandomizeTextures || (RandomizeEnemies && CrossLevelEnemies) || RandomizeOutfits;

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
            IncludeKeyItems = config.GetBool(nameof(IncludeKeyItems));

            RandomizeEnemies = config.GetBool(nameof(RandomizeEnemies));
            EnemySeed = config.GetInt(nameof(EnemySeed), defaultSeed);
            CrossLevelEnemies = config.GetBool(nameof(CrossLevelEnemies));
            ProtectMonks = config.GetBool(nameof(ProtectMonks));
            DocileBirdMonsters = config.GetBool(nameof(DocileBirdMonsters));

            RandomizeTextures = config.GetBool(nameof(RandomizeTextures));
            TextureSeed = config.GetInt(nameof(TextureSeed), defaultSeed);
            PersistTextureVariants = config.GetBool(nameof(PersistTextureVariants));
            RetainKeySpriteTextures = config.GetBool(nameof(RetainKeySpriteTextures));

            RandomizeOutfits = config.GetBool(nameof(RandomizeOutfits));
            OutfitSeed = config.GetInt(nameof(OutfitSeed), defaultSeed);
            PersistOutfits = config.GetBool(nameof(PersistOutfits));
            RandomlyCutHair = config.GetBool(nameof(RandomlyCutHair));

            RandomizeGameStrings = config.GetBool(nameof(RandomizeGameStrings));
            GameStringsSeed = config.GetInt(nameof(GameStringsSeed), defaultSeed);
            RetainKeyItemNames = config.GetBool(nameof(RetainKeyItemNames));

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

            config[nameof(RandomizeTextures)] = RandomizeTextures;
            config[nameof(TextureSeed)] = TextureSeed;
            config[nameof(PersistTextureVariants)] = PersistTextureVariants;
            config[nameof(RetainKeySpriteTextures)] = RetainKeySpriteTextures;

            config[nameof(RandomizeOutfits)] = RandomizeOutfits;
            config[nameof(OutfitSeed)] = OutfitSeed;
            config[nameof(PersistOutfits)] = PersistOutfits;
            config[nameof(RandomlyCutHair)] = RandomlyCutHair;

            config[nameof(RandomizeGameStrings)] = RandomizeGameStrings;
            config[nameof(GameStringsSeed)] = GameStringsSeed;
            config[nameof(RetainKeyItemNames)] = RetainKeyItemNames;

            config[nameof(DevelopmentMode)] = DevelopmentMode;
            config[nameof(AutoLaunchGame)] = AutoLaunchGame;
        }

        /// <summary>
        /// This is called before the script data is saved so gives us an opportunity to 
        /// customise the script outwith TRGE before the main SaveImpl.
        /// </summary>
        protected override void PreSaveImpl(AbstractTRScriptEditor scriptEditor)
        {
            if (RandomizeGameStrings)
            {
                GameStringRandomizer stringRandomizer = new GameStringRandomizer
                {
                    ScriptEditor = scriptEditor as TR23ScriptEditor,
                    RetainKeyItemNames = RetainKeyItemNames
                };
                stringRandomizer.Randomize(GameStringsSeed);
            }
        }

        protected override int GetSaveTarget(int numLevels)
        {
            // TODO: move these target calculations into the relevant classes - they don't belong here
            int target = base.GetSaveTarget(numLevels);
            if (RandomizeSecrets)    target += numLevels;
            if (RandomizeItems)      target += numLevels * 2; // standard/key rando followed by unarmed logic after enemy rando
            if (DeduplicateTextures) target += numLevels * 2;
            if (RandomizeEnemies)    target += CrossLevelEnemies ? numLevels * 3 : numLevels;
            if (RandomizeTextures)   target += numLevels * 3;
            if (RandomizeOutfits)    target += numLevels * 2;
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
                        TextureMonitor = textureMonitor
                    }.Randomize(EnemySeed);
                }

                // Randomize ammo/weapon in unarmed levels post enemy randomization
                if (!monitor.IsCancelled && RandomizeItems)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing unarmed level items");
                    itemRandomizer.RandomizeAmmo();
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
                        TextureMonitor = textureMonitor
                    }.Randomize(OutfitSeed);
                }

                if (!monitor.IsCancelled && RandomizeTextures)
                {
                    monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing textures");
                    new TextureRandomizer
                    {
                        Levels = levels,
                        BasePath = wipDirectory,
                        SaveMonitor = monitor,
                        PersistVariants = PersistTextureVariants,
                        RetainKeySprites = RetainKeySpriteTextures,
                        TextureMonitor = textureMonitor
                    }.Randomize(TextureSeed);
                }
            }
        }
    }
}