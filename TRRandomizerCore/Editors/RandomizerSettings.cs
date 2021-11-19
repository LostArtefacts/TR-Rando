using System;
using TRRandomizerCore.Globalisation;
using TRRandomizerCore.Helpers;
using TRGE.Core;

namespace TRRandomizerCore.Editors
{
    public class RandomizerSettings
    {
        public bool RandomizeSecrets { get; set; }
        public bool RandomizeItems { get; set; }
        public bool RandomizeEnemies { get; set; }
        public bool RandomizeTextures { get; set; }
        public bool RandomizeOutfits { get; set; }
        public bool RandomizeGameStrings { get; set; }
        public bool RandomizeNightMode { get; set; }
        public bool RandomizeAudio { get; set; }
        public bool RandomizeStartPosition { get; set; }
        public bool RandomizeEnvironment { get; set; }
        public bool RandomizeSecretRewardsPhysical { get; set; }

        public int SecretSeed { get; set; }
        public int ItemSeed { get; set; }
        public int EnemySeed { get; set; }
        public int TextureSeed { get; set; }
        public int OutfitSeed { get; set; }
        public int GameStringsSeed { get; set; }
        public int NightModeSeed { get; set; }
        public int AudioSeed { get; set; }
        public int StartPositionSeed { get; set; }
        public int EnvironmentSeed { get; set; }
        public int SecretRewardsPhysicalSeed { get; set; }

        // Although stored in the script config, this is needed in case sequencing
        // mods are needed per level.
        public bool RandomizeSequencing { get; set; }
        public GlobeDisplayOption GlobeDisplay { get; set; }
        public bool HardSecrets { get; set; }
        public bool IncludeKeyItems { get; set; }
        public bool DevelopmentMode { get; set; }
        public ItemDifficulty RandoItemDifficulty { get; set; }
        public bool PersistTextureVariants { get; set; }
        public bool RetainKeySpriteTextures { get; set; }
        public bool RetainSecretSpriteTextures { get; set; }
        public bool CrossLevelEnemies { get; set; }
        public bool ProtectMonks { get; set; }
        public bool DocileBirdMonsters { get; set; }
        public RandoDifficulty RandoEnemyDifficulty { get; set; }
        public bool GlitchedSecrets { get; set; }
        public bool UseRewardRoomCameras { get; set; }
        public bool PersistOutfits { get; set; }
        public bool RemoveRobeDagger { get; set; }
        public uint HaircutLevelCount { get; set; }
        public bool AssaultCourseHaircut { get; set; }
        public uint InvisibleLevelCount { get; set; }
        public bool AssaultCourseInvisible { get; set; }
        public bool RetainLevelNames { get; set; }
        public bool RetainKeyItemNames { get; set; }
        public Language GameStringLanguage { get; set; }
        public uint NightModeCount { get; set; }
        public uint NightModeDarkness { get; set; }
        public bool NightModeAssaultCourse { get; set; }
        public bool ChangeTriggerTracks { get; set; }
        public bool SeparateSecretTracks { get; set; }
        public bool ChangeWeaponSFX { get; set; }
        public bool ChangeCrashSFX { get; set; }
        public bool ChangeEnemySFX { get; set; }
        public bool LinkCreatureSFX { get; set; }
        public bool RotateStartPositionOnly { get; set; }
        public bool RandomizeWaterLevels { get; set; }
        public bool RandomizeSlotPositions { get; set; }
        public bool RandomizeLadders { get; set; }
        public uint MirroredLevelCount { get; set; }
        public bool MirrorAssaultCourse { get; set; }
        public bool AutoLaunchGame { get; set; }
        public bool PuristMode { get; set; }

        public bool RandomizeItemTypes { get; set; }
        public bool RandomizeItemPositions { get; set; }

        public bool DeduplicateTextures => RandomizeTextures || RandomizeNightMode || (RandomizeEnemies && CrossLevelEnemies) || RandomizeOutfits;// || RandomizeEnvironment; // Not needed until trap model import takes place
        public bool ReassignPuzzleNames => RandomizeEnemies && CrossLevelEnemies;

        public void ApplyConfig(Config config)
        {
            int defaultSeed = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            GlobeDisplay = (GlobeDisplayOption)config.GetEnum(nameof(GlobeDisplay), typeof(GlobeDisplayOption), GlobeDisplayOption.Area);

            RandomizeSecrets = config.GetBool(nameof(RandomizeSecrets));
            SecretSeed = config.GetInt(nameof(SecretSeed), defaultSeed);
            HardSecrets = config.GetBool(nameof(HardSecrets));
            GlitchedSecrets = config.GetBool(nameof(GlitchedSecrets));
            UseRewardRoomCameras = config.GetBool(nameof(UseRewardRoomCameras));

            RandomizeItems = config.GetBool(nameof(RandomizeItems));
            ItemSeed = config.GetInt(nameof(ItemSeed), defaultSeed);
            IncludeKeyItems = config.GetBool(nameof(IncludeKeyItems), true);
            RandoItemDifficulty = (ItemDifficulty)config.GetEnum(nameof(RandoItemDifficulty), typeof(ItemDifficulty), ItemDifficulty.Default);
            RandomizeItemTypes = config.GetBool(nameof(RandomizeItemTypes));
            RandomizeItemPositions = config.GetBool(nameof(RandomizeItemPositions));

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

            // Note that the main audio config options (on/off and seed) are held in TRGE for now
            ChangeTriggerTracks = config.GetBool(nameof(ChangeTriggerTracks), true);
            SeparateSecretTracks = config.GetBool(nameof(SeparateSecretTracks), true);
            ChangeWeaponSFX = config.GetBool(nameof(ChangeWeaponSFX), true);
            ChangeCrashSFX = config.GetBool(nameof(ChangeCrashSFX), true);
            ChangeEnemySFX = config.GetBool(nameof(ChangeEnemySFX), true);
            LinkCreatureSFX = config.GetBool(nameof(LinkCreatureSFX));

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
            PuristMode = config.GetBool(nameof(PuristMode));

            RandomizeSecretRewardsPhysical = config.GetBool(nameof(RandomizeSecretRewardsPhysical));
            SecretRewardsPhysicalSeed = config.GetInt(nameof(SecretRewardsPhysicalSeed));
        }

        public void StoreConfig(Config config)
        {
            config[nameof(GlobeDisplay)] = GlobeDisplay;

            config[nameof(RandomizeSecrets)] = RandomizeSecrets;
            config[nameof(SecretSeed)] = SecretSeed;
            config[nameof(HardSecrets)] = HardSecrets;
            config[nameof(GlitchedSecrets)] = GlitchedSecrets;
            config[nameof(UseRewardRoomCameras)] = UseRewardRoomCameras;

            config[nameof(RandomizeItems)] = RandomizeItems;
            config[nameof(ItemSeed)] = ItemSeed;
            config[nameof(IncludeKeyItems)] = IncludeKeyItems;
            config[nameof(RandoItemDifficulty)] = RandoItemDifficulty;
            config[nameof(RandomizeItemTypes)] = RandomizeItemTypes;
            config[nameof(RandomizeItemPositions)] = RandomizeItemPositions;

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
            config[nameof(SeparateSecretTracks)] = SeparateSecretTracks;
            config[nameof(ChangeWeaponSFX)] = ChangeWeaponSFX;
            config[nameof(ChangeCrashSFX)] = ChangeCrashSFX;
            config[nameof(ChangeEnemySFX)] = ChangeEnemySFX;
            config[nameof(LinkCreatureSFX)] = LinkCreatureSFX;

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
            config[nameof(PuristMode)] = PuristMode;

            config[nameof(RandomizeSecretRewardsPhysical)] = RandomizeSecretRewardsPhysical;
            config[nameof(SecretRewardsPhysicalSeed)] = SecretRewardsPhysicalSeed;
        }

        public int GetSaveTarget(int numLevels)
        {
            int target = 0;

            if (RandomizeGameStrings || ReassignPuzzleNames)
            {
                target++;
            }

            if (RandomizeNightMode)
            {
                target += numLevels;
                if (!RandomizeTextures)
                {
                    // Texture randomizer will run if night mode is on to ensure skyboxes and such like match
                    target += numLevels;
                }
            }

            if (RandomizeSecrets)
            {
                target += numLevels;
            }

            if (RandomizeAudio)
            {
                target += numLevels;
            }

            if (RandomizeItems)
            {
                // Standard/key item rando followed by unarmed logic after enemy rando
                target += numLevels * 2;
            }

            if (RandomizeStartPosition)
            {
                target += numLevels;
            }

            if (DeduplicateTextures)
            {
                // *2 because of multithreaded approach
                target += numLevels * 2;
            }

            if (RandomizeEnemies)
            {
                // *4 => 3 for multithreading work, 1 for ModelAdjuster
                target += CrossLevelEnemies ? numLevels * 4 : numLevels;
            }

            if (RandomizeTextures)
            {
                // *3 because of multithreaded approach
                target += numLevels * 3;
            }

            if (RandomizeOutfits)
            {
                // *2 because of multithreaded approach
                target += numLevels * 2;
            }

            // Environment randomizer always runs
            target += numLevels;

            return target;
        }
    }
}