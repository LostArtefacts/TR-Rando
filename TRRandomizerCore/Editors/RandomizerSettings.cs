using System;
using TRRandomizerCore.Globalisation;
using TRRandomizerCore.Helpers;
using TRGE.Core;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Secrets;

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
        public bool IncludeExtraPickups { get; set; }
        public bool DevelopmentMode { get; set; }
        public ItemDifficulty RandoItemDifficulty { get; set; }
        public bool PersistTextureVariants { get; set; }
        public bool RandomizeWaterColour { get; set; }
        public bool RetainMainLevelTextures { get; set; }
        public bool RetainKeySpriteTextures { get; set; }
        public bool RetainSecretSpriteTextures { get; set; }
        public uint WireframeLevelCount { get; set; }
        public bool AssaultCourseWireframe { get; set; }
        public bool UseSolidLaraWireframing { get; set; }
        public bool UseSolidEnemyWireframing { get; set; }
        public bool UseDifferentWireframeColours { get; set; }
        public bool UseWireframeLadders { get; set; }
        public bool CrossLevelEnemies { get; set; }
        public bool ProtectMonks { get; set; }
        public bool DocileWillard { get; set; }
        public BirdMonsterBehaviour BirdMonsterBehaviour { get; set; }
        public RandoDifficulty RandoEnemyDifficulty { get; set; }
        public DragonSpawnType DragonSpawnType { get; set; }
        public bool UseEnemyExclusions { get; set; }
        public List<short> ExcludedEnemies { get; set; }
        public Dictionary<short, string> ExcludableEnemies { get; set; }
        public bool ShowExclusionWarnings { get; set; }
        public List<short> IncludedEnemies => ExcludableEnemies.Keys.Except(ExcludedEnemies).ToList();
        public bool OneEnemyMode => IncludedEnemies.Count == 1;
        public bool SwapEnemyAppearance { get; set; }
        public bool AllowEmptyEggs { get; set; }
        public bool HideEnemiesUntilTriggered { get; set; }
        public bool RemoveLevelEndingLarson { get; set; }
        public bool GlitchedSecrets { get; set; }
        public bool UseRewardRoomCameras { get; set; }
        public bool UseRandomSecretModels { get; set; }
        public TRSecretCountMode SecretCountMode { get; set; }
        public uint MinSecretCount { get; set; }
        public uint MaxSecretCount { get; set; }
        public bool PersistOutfits { get; set; }
        public bool RemoveRobeDagger { get; set; }
        public uint HaircutLevelCount { get; set; }
        public bool AssaultCourseHaircut { get; set; }
        public uint InvisibleLevelCount { get; set; }
        public bool AllowGymOutfit { get; set; }
        public bool AssaultCourseInvisible { get; set; }
        public bool RetainLevelNames { get; set; }
        public bool RetainKeyItemNames { get; set; }
        public Language GameStringLanguage { get; set; }
        public uint NightModeCount { get; set; }
        public uint NightModeDarkness { get; set; }
        public bool NightModeAssaultCourse { get; set; }
        public bool OverrideSunsets { get; set; }
        public bool ChangeAmbientTracks { get; set; }
        public bool ChangeTriggerTracks { get; set; }
        public bool SeparateSecretTracks { get; set; }
        public bool ChangeWeaponSFX { get; set; }
        public bool ChangeCrashSFX { get; set; }
        public bool ChangeEnemySFX { get; set; }
        public bool ChangeDoorSFX { get; set; }
        public bool LinkCreatureSFX { get; set; }
        public uint UncontrolledSFXCount { get; set; }
        public bool UncontrolledSFXAssaultCourse { get; set; }
        public bool RandomizeWibble { get; set; }
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

        public bool RandomizeVfx { get; set; }
        public Color VfxFilterColor { get; set; }
        public bool VfxVivid { get; set; }
        public bool VfxLevel { get; set; }
        public bool VfxRoom { get; set; }
        public bool VfxCaustics { get; set; }
        public bool VfxWave { get; set; }

        public bool RandomizeStartingHealth { get; set; }
        public int HealthSeed { get; set; }
        public uint MinStartingHealth { get; set; }
        public uint MaxStartingHealth { get; set; }

        public bool UseRecommendedCommunitySettings { get; set; }


        /// <summary>
        /// Randomisation mode used in ItemSpriteRandomizer
        /// </summary>
        public SpriteRandoMode SpriteRandoMode { get; set; }
        /// <summary>
        /// Activation of ItemSpriteRandomizer
        /// </summary>
        public bool RandomizeItemSprites { get; set; }
        /// <summary>
        /// Includes key items in ItemSpriteRandomizer
        /// </summary>
        public bool RandomizeKeyItemSprites { get; set; }
        /// <summary>
        /// Includes secret items in ItemSpriteRandomizer
        /// </summary>
        public bool RandomizeSecretSprites { get; set; }

        public void ApplyConfig(Config config)
        {
            int defaultSeed = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            GlobeDisplay = (GlobeDisplayOption)config.GetEnum(nameof(GlobeDisplay), typeof(GlobeDisplayOption), GlobeDisplayOption.Area);

            RandomizeSecrets = config.GetBool(nameof(RandomizeSecrets));
            SecretSeed = config.GetInt(nameof(SecretSeed), defaultSeed);
            HardSecrets = config.GetBool(nameof(HardSecrets));
            GlitchedSecrets = config.GetBool(nameof(GlitchedSecrets));
            UseRewardRoomCameras = config.GetBool(nameof(UseRewardRoomCameras), true);
            UseRandomSecretModels = config.GetBool(nameof(UseRandomSecretModels));
            SecretCountMode = (TRSecretCountMode)config.GetEnum(nameof(SecretCountMode), typeof(TRSecretCountMode), TRSecretCountMode.Default);
            MinSecretCount = config.GetUInt(nameof(MinSecretCount), 1);
            MaxSecretCount = config.GetUInt(nameof(MaxSecretCount), 5);

            RandomizeItems = config.GetBool(nameof(RandomizeItems));
            ItemSeed = config.GetInt(nameof(ItemSeed), defaultSeed);
            IncludeKeyItems = config.GetBool(nameof(IncludeKeyItems), true);
            IncludeExtraPickups = config.GetBool(nameof(IncludeExtraPickups), true);
            RandoItemDifficulty = (ItemDifficulty)config.GetEnum(nameof(RandoItemDifficulty), typeof(ItemDifficulty), ItemDifficulty.Default);
            RandomizeItemTypes = config.GetBool(nameof(RandomizeItemTypes), true);
            RandomizeItemPositions = config.GetBool(nameof(RandomizeItemPositions), true);

            RandomizeEnemies = config.GetBool(nameof(RandomizeEnemies));
            EnemySeed = config.GetInt(nameof(EnemySeed), defaultSeed);
            CrossLevelEnemies = config.GetBool(nameof(CrossLevelEnemies), true);
            ProtectMonks = config.GetBool(nameof(ProtectMonks), true);
            DocileWillard = config.GetBool(nameof(DocileWillard));
            BirdMonsterBehaviour = (BirdMonsterBehaviour)config.GetEnum(nameof(BirdMonsterBehaviour), typeof(BirdMonsterBehaviour), BirdMonsterBehaviour.Default);
            RandoEnemyDifficulty = (RandoDifficulty)config.GetEnum(nameof(RandoEnemyDifficulty), typeof(RandoDifficulty), RandoDifficulty.Default);
            DragonSpawnType = (DragonSpawnType)config.GetEnum(nameof(DragonSpawnType), typeof(DragonSpawnType), DragonSpawnType.Default);
            SwapEnemyAppearance = config.GetBool(nameof(SwapEnemyAppearance), true);
            AllowEmptyEggs = config.GetBool(nameof(AllowEmptyEggs));
            HideEnemiesUntilTriggered = config.GetBool(nameof(HideEnemiesUntilTriggered), true);
            RemoveLevelEndingLarson = config.GetBool(nameof(RemoveLevelEndingLarson), true);
            UseEnemyExclusions = config.GetBool(nameof(UseEnemyExclusions));
            ShowExclusionWarnings = config.GetBool(nameof(ShowExclusionWarnings));
            ExcludedEnemies = config.GetString(nameof(ExcludedEnemies))
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => short.Parse(s))
                .Where(s => ExcludableEnemies.ContainsKey(s))
                .ToList();

            RandomizeTextures = config.GetBool(nameof(RandomizeTextures));
            TextureSeed = config.GetInt(nameof(TextureSeed), defaultSeed);
            PersistTextureVariants = config.GetBool(nameof(PersistTextureVariants));
            RandomizeWaterColour = config.GetBool(nameof(RandomizeWaterColour), true);
            RetainMainLevelTextures = config.GetBool(nameof(RetainMainLevelTextures));
            RetainKeySpriteTextures = config.GetBool(nameof(RetainKeySpriteTextures), true);
            RetainSecretSpriteTextures = config.GetBool(nameof(RetainSecretSpriteTextures), true);
            WireframeLevelCount = config.GetUInt(nameof(WireframeLevelCount));
            AssaultCourseWireframe = config.GetBool(nameof(AssaultCourseWireframe));
            UseSolidLaraWireframing = config.GetBool(nameof(UseSolidLaraWireframing), true);
            UseSolidEnemyWireframing = config.GetBool(nameof(UseSolidEnemyWireframing), true);
            UseDifferentWireframeColours = config.GetBool(nameof(UseDifferentWireframeColours), true);
            UseWireframeLadders = config.GetBool(nameof(UseWireframeLadders), true);

            RandomizeOutfits = config.GetBool(nameof(RandomizeOutfits));
            OutfitSeed = config.GetInt(nameof(OutfitSeed), defaultSeed);
            PersistOutfits = config.GetBool(nameof(PersistOutfits));
            RemoveRobeDagger = config.GetBool(nameof(RemoveRobeDagger), true);
            HaircutLevelCount = config.GetUInt(nameof(HaircutLevelCount), 9);
            AssaultCourseHaircut = config.GetBool(nameof(AssaultCourseHaircut), true);
            InvisibleLevelCount = config.GetUInt(nameof(InvisibleLevelCount), 2);
            AllowGymOutfit = config.GetBool(nameof(AllowGymOutfit), true);
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
            OverrideSunsets = config.GetBool(nameof(OverrideSunsets));

            RandomizeAudio = config.GetBool(nameof(RandomizeAudio));
            ChangeAmbientTracks = config.GetBool(nameof(ChangeAmbientTracks), true);
            ChangeTriggerTracks = config.GetBool(nameof(ChangeTriggerTracks), true);
            SeparateSecretTracks = config.GetBool(nameof(SeparateSecretTracks), true);
            ChangeWeaponSFX = config.GetBool(nameof(ChangeWeaponSFX), true);
            ChangeCrashSFX = config.GetBool(nameof(ChangeCrashSFX), true);
            ChangeEnemySFX = config.GetBool(nameof(ChangeEnemySFX), true);
            ChangeDoorSFX = config.GetBool(nameof(ChangeDoorSFX), true);
            LinkCreatureSFX = config.GetBool(nameof(LinkCreatureSFX));
            UncontrolledSFXCount = config.GetUInt(nameof(UncontrolledSFXCount), 0);
            UncontrolledSFXAssaultCourse = config.GetBool(nameof(UncontrolledSFXAssaultCourse));
            RandomizeWibble = config.GetBool(nameof(RandomizeWibble));

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
            SecretRewardsPhysicalSeed = config.GetInt(nameof(SecretRewardsPhysicalSeed), defaultSeed);

            RandomizeVfx = config.GetBool(nameof(RandomizeVfx));
            VfxFilterColor = Color.FromArgb(config.GetInt(nameof(VfxFilterColor)));
            VfxVivid = config.GetBool(nameof(VfxVivid));
            VfxLevel = config.GetBool(nameof(VfxLevel));
            VfxRoom = config.GetBool(nameof(VfxRoom));
            VfxCaustics = config.GetBool(nameof(VfxCaustics));
            VfxWave = config.GetBool(nameof(VfxWave));

            RandomizeStartingHealth = config.GetBool(nameof(RandomizeStartingHealth));
            HealthSeed = config.GetInt(nameof(HealthSeed), defaultSeed);
            MinStartingHealth = config.GetUInt(nameof(MinStartingHealth), 1000);
            MaxStartingHealth = config.GetUInt(nameof(MaxStartingHealth), 1000);

            UseRecommendedCommunitySettings = config.GetBool(nameof(UseRecommendedCommunitySettings), true);

            SpriteRandoMode = (SpriteRandoMode)config.GetEnum(nameof(SpriteRandoMode), typeof(SpriteRandoMode), SpriteRandoMode.Default);
            RandomizeItemSprites = config.GetBool(nameof(RandomizeItemSprites));
            RandomizeKeyItemSprites = config.GetBool(nameof(RandomizeKeyItemSprites));
            RandomizeSecretSprites = config.GetBool(nameof(RandomizeSecretSprites));

        }

        public void StoreConfig(Config config)
        {
            config[nameof(GlobeDisplay)] = GlobeDisplay;

            config[nameof(RandomizeSecrets)] = RandomizeSecrets;
            config[nameof(SecretSeed)] = SecretSeed;
            config[nameof(HardSecrets)] = HardSecrets;
            config[nameof(GlitchedSecrets)] = GlitchedSecrets;
            config[nameof(UseRewardRoomCameras)] = UseRewardRoomCameras;
            config[nameof(UseRandomSecretModels)] = UseRandomSecretModels;
            config[nameof(SecretCountMode)] = SecretCountMode;
            config[nameof(MinSecretCount)] = MinSecretCount;
            config[nameof(MaxSecretCount)] = MaxSecretCount;

            config[nameof(RandomizeItems)] = RandomizeItems;
            config[nameof(ItemSeed)] = ItemSeed;
            config[nameof(IncludeKeyItems)] = IncludeKeyItems;
            config[nameof(IncludeExtraPickups)] = IncludeExtraPickups;
            config[nameof(RandoItemDifficulty)] = RandoItemDifficulty;
            config[nameof(RandomizeItemTypes)] = RandomizeItemTypes;
            config[nameof(RandomizeItemPositions)] = RandomizeItemPositions;

            config[nameof(RandomizeEnemies)] = RandomizeEnemies;
            config[nameof(EnemySeed)] = EnemySeed;
            config[nameof(CrossLevelEnemies)] = CrossLevelEnemies;
            config[nameof(ProtectMonks)] = ProtectMonks;
            config[nameof(DocileWillard)] = DocileWillard;
            config[nameof(BirdMonsterBehaviour)] = BirdMonsterBehaviour;
            config[nameof(RandoEnemyDifficulty)] = RandoEnemyDifficulty;
            config[nameof(DragonSpawnType)] = DragonSpawnType;
            config[nameof(ExcludedEnemies)] = string.Join(",", ExcludedEnemies);
            config[nameof(UseEnemyExclusions)] = UseEnemyExclusions;
            config[nameof(ShowExclusionWarnings)] = ShowExclusionWarnings;
            config[nameof(SwapEnemyAppearance)] = SwapEnemyAppearance;
            config[nameof(AllowEmptyEggs)] = AllowEmptyEggs;
            config[nameof(HideEnemiesUntilTriggered)] = HideEnemiesUntilTriggered;
            config[nameof(RemoveLevelEndingLarson)] = RemoveLevelEndingLarson;

            config[nameof(RandomizeTextures)] = RandomizeTextures;
            config[nameof(TextureSeed)] = TextureSeed;
            config[nameof(PersistTextureVariants)] = PersistTextureVariants;
            config[nameof(RandomizeWaterColour)] = RandomizeWaterColour;
            config[nameof(RetainMainLevelTextures)] = RetainMainLevelTextures;
            config[nameof(RetainKeySpriteTextures)] = RetainKeySpriteTextures;
            config[nameof(RetainSecretSpriteTextures)] = RetainSecretSpriteTextures;
            config[nameof(WireframeLevelCount)] = WireframeLevelCount;
            config[nameof(AssaultCourseWireframe)] = AssaultCourseWireframe;
            config[nameof(UseSolidLaraWireframing)] = UseSolidLaraWireframing;
            config[nameof(UseSolidEnemyWireframing)] = UseSolidEnemyWireframing;
            config[nameof(UseDifferentWireframeColours)] = UseDifferentWireframeColours;
            config[nameof(UseWireframeLadders)] = UseWireframeLadders;

            config[nameof(RandomizeOutfits)] = RandomizeOutfits;
            config[nameof(OutfitSeed)] = OutfitSeed;
            config[nameof(PersistOutfits)] = PersistOutfits;
            config[nameof(RemoveRobeDagger)] = RemoveRobeDagger;
            config[nameof(HaircutLevelCount)] = HaircutLevelCount;
            config[nameof(AssaultCourseHaircut)] = AssaultCourseHaircut;
            config[nameof(InvisibleLevelCount)] = InvisibleLevelCount;
            config[nameof(AllowGymOutfit)] = AllowGymOutfit;
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
            config[nameof(OverrideSunsets)] = OverrideSunsets;

            config[nameof(RandomizeAudio)] = RandomizeAudio;
            config[nameof(ChangeAmbientTracks)] = ChangeAmbientTracks;
            config[nameof(ChangeTriggerTracks)] = ChangeTriggerTracks;
            config[nameof(SeparateSecretTracks)] = SeparateSecretTracks;
            config[nameof(ChangeWeaponSFX)] = ChangeWeaponSFX;
            config[nameof(ChangeCrashSFX)] = ChangeCrashSFX;
            config[nameof(ChangeEnemySFX)] = ChangeEnemySFX;
            config[nameof(ChangeDoorSFX)] = ChangeDoorSFX;
            config[nameof(LinkCreatureSFX)] = LinkCreatureSFX;
            config[nameof(UncontrolledSFXCount)] = UncontrolledSFXCount;
            config[nameof(UncontrolledSFXAssaultCourse)] = UncontrolledSFXAssaultCourse;
            config[nameof(RandomizeWibble)] = RandomizeWibble;

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

            config[nameof(RandomizeVfx)] = RandomizeVfx;
            config[nameof(VfxFilterColor)] = VfxFilterColor.ToArgb();
            config[nameof(VfxVivid)] = VfxVivid;
            config[nameof(VfxLevel)] = VfxLevel;
            config[nameof(VfxRoom)] = VfxRoom;
            config[nameof(VfxCaustics)] = VfxCaustics;
            config[nameof(VfxWave)] = VfxWave;

            config[nameof(RandomizeStartingHealth)] = RandomizeStartingHealth;
            config[nameof(HealthSeed)] = HealthSeed;
            config[nameof(MinStartingHealth)] = MinStartingHealth;
            config[nameof(MaxStartingHealth)] = MaxStartingHealth;

            config[nameof(UseRecommendedCommunitySettings)] = UseRecommendedCommunitySettings;

            config[nameof(SpriteRandoMode)] = SpriteRandoMode;
            config[nameof(RandomizeItemSprites)] = RandomizeItemSprites;
            config[nameof(RandomizeKeyItemSprites)] = RandomizeKeyItemSprites;
            config[nameof(RandomizeSecretSprites)] = RandomizeSecretSprites;
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