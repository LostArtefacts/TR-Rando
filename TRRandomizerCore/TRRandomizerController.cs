using TRRandomizerCore.Editors;
using TRRandomizerCore.Globalisation;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Randomizers;
using TRGE.Coord;
using TRGE.Core;
using System.Drawing;
using TRRandomizerCore.Utilities;
using TRRandomizerCore.Secrets;
using TRGE.Coord.Helpers;

namespace TRRandomizerCore;

public class TRRandomizerController
{
    private readonly TREditor _editor;

    internal AbstractTRScriptEditor ScriptEditor => _editor.ScriptEditor;
    internal RandomizerSettings LevelRandomizer => (_editor.LevelEditor as ISettingsProvider).Settings;

    internal TRRandomizerController(string directoryPath, bool performChecksumTest)
    {
        // If there is a checksum mismatch, we will just ignore the previous backup and open the folder afresh
        _editor = TRCoord.Instance.Open(directoryPath, TRScriptOpenOption.DiscardBackup, performChecksumTest ? TRBackupChecksumOption.PerformCheck : TRBackupChecksumOption.IgnoreIssues);
        _editor.SaveProgressChanged += Editor_SaveProgressChanged;
        _editor.RestoreProgressChanged += Editor_RestoreProgressChanged;
        StoreExternalOrganisations();

        if (!IsRandomizationSupported())
        {
            throw new NotSupportedException(string.Format("Randomization of {0} is not currently supported.", EditionTitle));
        }
    }

    #region Version Support
    public bool IsRandomizationSupported()
    {
        return TRVersionSupport.IsRandomizationSupported(_editor.Edition);
    }

    public bool IsRandomizationSupported(TRRandomizerType randomizerType)
    {
        return TRVersionSupport.IsRandomizationSupported(_editor.Edition, randomizerType);
    }

    public List<string> GetExecutables(string dataFolder)
    {
        return TRVersionSupport.GetExecutables(_editor.Edition, dataFolder);
    }

    public bool IsTR1 => _editor.Edition.Version == TRVersion.TR1;
    public bool IsTR2 => _editor.Edition.Version == TRVersion.TR2;
    public bool IsTR3 => _editor.Edition.Version == TRVersion.TR3;
    public bool IsCommunityPatch => _editor.Edition.IsCommunityPatch;
    #endregion

    #region ScriptEditor Passthrough
    private Organisation _extUnarmedOrganisation, _extAmmolessOrganisation, _extMedilessOrganisation, _extSecretRewardsOrganisation, _extSunsetOrganisation, _extAudioOrganisation;

    /// <summary>
    /// We need to store any organisation values that aren't random so if randomization is turned off for the 
    /// corresponding functions, they will be reverted to what was previously set externally. If any haven't
    /// been set externally, turning them off will result in standard organisation.
    /// </summary>
    private void StoreExternalOrganisations()
    {
        _extSunsetOrganisation = RandomizeSunsets ? Organisation.Default : ScriptEditor.LevelSunsetOrganisation;
        _extAudioOrganisation = ChangeAmbientTracks ? Organisation.Default : ScriptEditor.GameTrackOrganisation;

        if (ScriptEditor is IUnarmedEditor unarmedEditor)
        {
            _extUnarmedOrganisation = RandomizeUnarmedLevels ? Organisation.Default : unarmedEditor.UnarmedLevelOrganisation;
        }
        if (ScriptEditor is IAmmolessEditor ammolessEditor)
        {
            _extAmmolessOrganisation = RandomizeAmmolessLevels ? Organisation.Default : ammolessEditor.AmmolessLevelOrganisation;
        }
        if (ScriptEditor is IHealthEditor healthEditor)
        {
            _extMedilessOrganisation = RandomizeHealth ? Organisation.Default : healthEditor.MedilessLevelOrganisation;
        }
        if (ScriptEditor is ISecretRewardEditor rewardEditor)
        {
            _extSecretRewardsOrganisation = RandomizeSecretRewards ? Organisation.Default : rewardEditor.SecretBonusOrganisation;
        }
    }

    private void SetScriptSequencing()
    {
        if (RandomizeGameMode)
        {
            ScriptEditor.LevelSequencingOrganisation = (RandomizeSequencing || _editor.Edition.Remastered) ? Organisation.Random : Organisation.Default;
            ScriptEditor.EnabledLevelOrganisation = Organisation.Random;
            ScriptEditor.GameMode = LevelRandomizer.GameMode;
        }
        else
        {
            ScriptEditor.LevelSequencingOrganisation = Organisation.Default;
            ScriptEditor.EnabledLevelOrganisation = Organisation.Default;
            ScriptEditor.GameMode = GameMode.Normal;
        }
    }

    public int GetTotalLevelCount(GameMode mode) => ScriptEditor.GetTotalLevelCount(mode);
    public int GetDefaultUnarmedLevelCount(GameMode mode) => ScriptEditor.GetDefaultUnarmedLevelCount(mode);
    public int GetDefaultAmmolessLevelCount(GameMode mode) => ScriptEditor.GetDefaultAmmolessLevelCount(mode);
    public int GetDefaultSunsetLevelCount(GameMode mode) => ScriptEditor.GetDefaultSunsetLevelCount(mode);

    public bool RandomizeGameMode
    {
        get => LevelRandomizer.RandomizeGameMode;
        set => LevelRandomizer.RandomizeGameMode = value;
    }

    public GameMode GameMode
    {
        get => LevelRandomizer.GameMode;
        set => LevelRandomizer.GameMode = value;
    }

    public bool RandomizeSequencing
    {
        get => LevelRandomizer.RandomizeSequencing;
        set => LevelRandomizer.RandomizeSequencing = value;
    }

    public int LevelSequencingSeed
    {
        get => ScriptEditor.LevelSequencingRNG.Value;
        set
        {
            ScriptEditor.LevelSequencingRNG = new(value);
            ScriptEditor.EnabledLevelRNG = new(value);
        }
    }

    public uint PlayableLevelCount
    {
        get => ScriptEditor.RandomEnabledLevelCount;
        set => ScriptEditor.RandomEnabledLevelCount = value;
    }

    public bool RandomizeUnarmedLevels
    {
        get => ScriptEditor is IUnarmedEditor unarmedEditor && unarmedEditor.UnarmedLevelOrganisation == Organisation.Random;
        set
        {
            if (ScriptEditor is IUnarmedEditor unarmedEditor)
            {
                unarmedEditor.UnarmedLevelOrganisation = value ? Organisation.Random : _extUnarmedOrganisation;
            }
        }
    }

    public int UnarmedLevelsSeed
    {
        get => ScriptEditor is IUnarmedEditor unarmedEditor ? unarmedEditor.UnarmedLevelRNG.Value : -1;
        set
        {
            if (ScriptEditor is IUnarmedEditor unarmedEditor)
            {
                unarmedEditor.UnarmedLevelRNG = new RandomGenerator(value);
            }
        }
    }

    public uint UnarmedLevelCount
    {
        get => ScriptEditor is IUnarmedEditor unarmedEditor ? unarmedEditor.RandomUnarmedLevelCount : (uint)_editor.Edition.UnarmedLevelCount;
        set
        {
            if (ScriptEditor is IUnarmedEditor unarmedEditor)
            {
                unarmedEditor.RandomUnarmedLevelCount = value;
            }
        }
    }

    public bool RandomizeAmmolessLevels
    {
        get => ScriptEditor is IAmmolessEditor ammolessEditor && ammolessEditor.AmmolessLevelOrganisation == Organisation.Random;
        set
        {
            if (ScriptEditor is IAmmolessEditor ammolessEditor)
            {
                ammolessEditor.AmmolessLevelOrganisation = value ? Organisation.Random : _extAmmolessOrganisation;
            }
        }
    }

    public int AmmolessLevelsSeed
    {
        get => ScriptEditor is IAmmolessEditor ammolessEditor ? ammolessEditor.AmmolessLevelRNG.Value : -1;
        set
        {
            if (ScriptEditor is IAmmolessEditor ammolessEditor)
            {
                ammolessEditor.AmmolessLevelRNG = new RandomGenerator(value);
            }
        }
    }

    public uint AmmolessLevelCount
    {
        get => ScriptEditor is IAmmolessEditor ammolessEditor ? ammolessEditor.RandomAmmolessLevelCount : (uint)_editor.Edition.AmmolessLevelCount;
        set
        {
            if (ScriptEditor is IAmmolessEditor ammolessEditor)
            {
                ammolessEditor.RandomAmmolessLevelCount = value;
            }
        }
    }

    public bool RandomizeSecretRewards
    {
        get
        {
            if (ScriptEditor is ISecretRewardEditor rewardEditor)
            {
                return rewardEditor.SecretBonusOrganisation == Organisation.Random;
            }
            return LevelRandomizer.RandomizeSecretRewardsPhysical;
        }
        set
        {
            if (ScriptEditor is ISecretRewardEditor rewardEditor)
            {
                rewardEditor.SecretBonusOrganisation = value ? Organisation.Random : _extSecretRewardsOrganisation;
            }
            LevelRandomizer.RandomizeSecretRewardsPhysical = value;
        }
    }

    public int SecretRewardSeed
    {
        get
        {
            if (ScriptEditor is ISecretRewardEditor rewardEditor)
            {
                return rewardEditor.SecretBonusRNG.Value;
            }
            return LevelRandomizer.SecretRewardsPhysicalSeed;
        }
        set
        {
            if (ScriptEditor is ISecretRewardEditor rewardEditor)
            {
                rewardEditor.SecretBonusRNG = new RandomGenerator(value);
            }
            LevelRandomizer.SecretRewardsPhysicalSeed = value;
        }
    }

    public bool RandomizeHealth
    {
        get => ScriptEditor is IHealthEditor healthEditor && healthEditor.MedilessLevelOrganisation == Organisation.Random;
        set
        {
            if (ScriptEditor is IHealthEditor healthEditor)
            {
                healthEditor.MedilessLevelOrganisation = value ? Organisation.Random : _extMedilessOrganisation;
            }
            LevelRandomizer.RandomizeStartingHealth = value;
        }
    }

    public int HealthSeed
    {
        get => ScriptEditor is IHealthEditor healthEditor ? healthEditor.MedilessLevelRNG.Value : -1;
        set
        {
            if (ScriptEditor is IHealthEditor healthEditor)
            {
                healthEditor.MedilessLevelRNG = new RandomGenerator(value);
            }
            LevelRandomizer.HealthSeed = value;
        }
    }

    public uint MedilessLevelCount
    {
        get => ScriptEditor is IHealthEditor healthEditor ? healthEditor.RandomMedilessLevelCount : (uint)_editor.Edition.MedilessLevelCount;
        set
        {
            if (ScriptEditor is IHealthEditor healthEditor)
            {
                healthEditor.RandomMedilessLevelCount = value;
            }
        }
    }

    public uint MinStartingHealth
    {
        get => LevelRandomizer.MinStartingHealth;
        set => LevelRandomizer.MinStartingHealth = value;
    }

    public uint MaxStartingHealth
    {
        get => LevelRandomizer.MaxStartingHealth;
        set => LevelRandomizer.MaxStartingHealth = value;
    }

    public bool DisableHealingBetweenLevels
    {
        get => ScriptEditor is IHealthEditor healthEditor && healthEditor.DisableHealingBetweenLevels;
        set
        {
            if (ScriptEditor is IHealthEditor healthEditor)
            {
                healthEditor.DisableHealingBetweenLevels = value && RandomizeHealth;
            }
        }
    }

    public bool DisableMedpacks
    {
        get => ScriptEditor is IHealthEditor healthEditor && healthEditor.DisableMedpacks;
        set
        {
            if (ScriptEditor is IHealthEditor healthEditor)
            {
                healthEditor.DisableMedpacks = value && RandomizeHealth;
            }
        }
    }

    public bool UseRecommendedCommunitySettings
    {
        get => LevelRandomizer.UseRecommendedCommunitySettings;
        set => LevelRandomizer.UseRecommendedCommunitySettings = value;
    }

    public bool RandomizeSunsets
    {
        get => ScriptEditor.LevelSunsetOrganisation == Organisation.Random;
        set => ScriptEditor.LevelSunsetOrganisation = value ? Organisation.Random : _extSunsetOrganisation;
    }

    public int SunsetsSeed
    {
        get => ScriptEditor.LevelSunsetRNG.Value;
        set => ScriptEditor.LevelSunsetRNG = new RandomGenerator(value);
    }

    public uint SunsetCount
    {
        get => ScriptEditor.RandomSunsetLevelCount;
        set => ScriptEditor.RandomSunsetLevelCount = value;
    }

    public bool ChangeAmbientTracks
    {
        get => LevelRandomizer.ChangeAmbientTracks;
        set
        {
            ScriptEditor.GameTrackOrganisation = RandomizeAudio && value ? Organisation.Random : _extAudioOrganisation;
            LevelRandomizer.ChangeAmbientTracks = value;
        }
    }

    public int AudioTracksSeed
    {
        get => ScriptEditor.GameTrackRNG.Value;
        set
        {
            ScriptEditor.GameTrackRNG = new RandomGenerator(value);
            LevelRandomizer.AudioSeed = value;
        }
    }

    public bool RandomGameTracksIncludeBlank
    {
        get => ScriptEditor.RandomGameTracksIncludeBlank;
        set => ScriptEditor.RandomGameTracksIncludeBlank = value;
    }

    public bool DisableDemos
    {
        get => ScriptEditor is IDemoEditor demoEditor && !demoEditor.DemosEnabled;
        set
        {
            if (ScriptEditor is IDemoEditor demoEditor)
            {
                demoEditor.DemosEnabled = !value;
            }
        }
    }

    public bool AutoLaunchGame
    {
        get => LevelRandomizer.AutoLaunchGame;
        set => LevelRandomizer.AutoLaunchGame = value;
    }
    #endregion

    #region TR1X Specifics
    public bool EnableGameModes
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableGameModes;
        set => (ScriptEditor as TR1ScriptEditor).EnableGameModes = value;
    }

    public bool EnableSaveCrystals
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableSaveCrystals;
        set => (ScriptEditor as TR1ScriptEditor).EnableSaveCrystals = value;
    }

    public double DemoDelay
    {
        get => (ScriptEditor as TR1ScriptEditor).DemoTime;
        set => (ScriptEditor as TR1ScriptEditor).DemoTime = value;
    }

    public double DrawDistanceFade
    {
        get => (ScriptEditor as TR1ScriptEditor).DrawDistanceFade;
        set => (ScriptEditor as TR1ScriptEditor).DrawDistanceFade = value;
    }

    public double DrawDistanceMax
    {
        get => (ScriptEditor as TR1ScriptEditor).DrawDistanceMax;
        set => (ScriptEditor as TR1ScriptEditor).DrawDistanceMax = value;
    }

    public double[] WaterColor
    {
        get => (ScriptEditor as TR1ScriptEditor).WaterColor;
        set
        {
            if (value.Length == 3)
            {
                (ScriptEditor as TR1ScriptEditor).WaterColor = new double[]
                {
                    Math.Round(value[0], 2),
                    Math.Round(value[1], 2),
                    Math.Round(value[2], 2),
                };
            }
        }
    }

    public bool DisableMagnums
    {
        get => (ScriptEditor as TR1ScriptEditor).DisableMagnums;
        set => (ScriptEditor as TR1ScriptEditor).DisableMagnums = value;
    }

    public bool DisableUzis
    {
        get => (ScriptEditor as TR1ScriptEditor).DisableUzis;
        set => (ScriptEditor as TR1ScriptEditor).DisableUzis = value;
    }

    public bool DisableShotgun
    {
        get => (ScriptEditor as TR1ScriptEditor).DisableShotgun;
        set => (ScriptEditor as TR1ScriptEditor).DisableShotgun = value;
    }

    public bool EnableDeathsCounter
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableDeathsCounter;
        set => (ScriptEditor as TR1ScriptEditor).EnableDeathsCounter = value;
    }

    public bool EnableEnemyHealthbar
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableEnemyHealthbar;
        set => (ScriptEditor as TR1ScriptEditor).EnableEnemyHealthbar = value;
    }

    public bool EnableEnhancedLook
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableEnhancedLook;
        set => (ScriptEditor as TR1ScriptEditor).EnableEnhancedLook = value;
    }

    public bool EnableShotgunFlash
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableShotgunFlash;
        set => (ScriptEditor as TR1ScriptEditor).EnableShotgunFlash = value;
    }

    public bool FixShotgunTargeting
    {
        get => (ScriptEditor as TR1ScriptEditor).FixShotgunTargeting;
        set => (ScriptEditor as TR1ScriptEditor).FixShotgunTargeting = value;
    }

    public bool EnableNumericKeys
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableNumericKeys;
        set => (ScriptEditor as TR1ScriptEditor).EnableNumericKeys = value;
    }

    public bool EnableTr3Sidesteps
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableTr3Sidesteps;
        set => (ScriptEditor as TR1ScriptEditor).EnableTr3Sidesteps = value;
    }

    public bool EnableCheats
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableCheats;
        set => (ScriptEditor as TR1ScriptEditor).EnableCheats = value;
    }

    public bool EnableDetailedStats
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableDetailedStats;
        set => (ScriptEditor as TR1ScriptEditor).EnableDetailedStats = value;
    }

    public bool EnableCompassStats
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableCompassStats;
        set => (ScriptEditor as TR1ScriptEditor).EnableCompassStats = value;
    }

    public bool EnableTotalStats
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableTotalStats;
        set => (ScriptEditor as TR1ScriptEditor).EnableTotalStats = value;
    }

    public bool EnableTimerInInventory
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableTimerInInventory;
        set => (ScriptEditor as TR1ScriptEditor).EnableTimerInInventory = value;
    }

    public bool EnableSmoothBars
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableSmoothBars;
        set => (ScriptEditor as TR1ScriptEditor).EnableSmoothBars = value;
    }

    public bool EnableFadeEffects
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableFadeEffects;
        set => (ScriptEditor as TR1ScriptEditor).EnableFadeEffects = value;
    }

    public TRMenuStyle MenuStyle
    {
        get => (ScriptEditor as TR1ScriptEditor).MenuStyle;
        set => (ScriptEditor as TR1ScriptEditor).MenuStyle = value;
    }

    public TRHealthbarMode HealthbarShowingMode
    {
        get => (ScriptEditor as TR1ScriptEditor).HealthbarShowingMode;
        set => (ScriptEditor as TR1ScriptEditor).HealthbarShowingMode = value;
    }

    public TRUILocation HealthbarLocation
    {
        get => (ScriptEditor as TR1ScriptEditor).HealthbarLocation;
        set => (ScriptEditor as TR1ScriptEditor).HealthbarLocation = value;
    }

    public TRUIColour HealthbarColor
    {
        get => (ScriptEditor as TR1ScriptEditor).HealthbarColor;
        set => (ScriptEditor as TR1ScriptEditor).HealthbarColor = value;
    }

    public TRAirbarMode AirbarShowingMode
    {
        get => (ScriptEditor as TR1ScriptEditor).AirbarShowingMode;
        set => (ScriptEditor as TR1ScriptEditor).AirbarShowingMode = value;
    }

    public TRUILocation AirbarLocation
    {
        get => (ScriptEditor as TR1ScriptEditor).AirbarLocation;
        set => (ScriptEditor as TR1ScriptEditor).AirbarLocation = value;
    }

    public TRUIColour AirbarColor
    {
        get => (ScriptEditor as TR1ScriptEditor).AirbarColor;
        set => (ScriptEditor as TR1ScriptEditor).AirbarColor = value;
    }

    public TRUILocation EnemyHealthbarLocation
    {
        get => (ScriptEditor as TR1ScriptEditor).EnemyHealthbarLocation;
        set => (ScriptEditor as TR1ScriptEditor).EnemyHealthbarLocation = value;
    }

    public TRUIColour EnemyHealthbarColor
    {
        get => (ScriptEditor as TR1ScriptEditor).EnemyHealthbarColor;
        set => (ScriptEditor as TR1ScriptEditor).EnemyHealthbarColor = value;
    }

    public bool FixTihocanSecretSound
    {
        get => (ScriptEditor as TR1ScriptEditor).FixTihocanSecretSound;
        set => (ScriptEditor as TR1ScriptEditor).FixTihocanSecretSound = value;
    }

    public bool FixPyramidSecretTrigger
    {
        get => (ScriptEditor as TR1ScriptEditor).FixPyramidSecretTrigger;
        set => (ScriptEditor as TR1ScriptEditor).FixPyramidSecretTrigger = value;
    }

    public bool FixSecretsKillingMusic
    {
        get => (ScriptEditor as TR1ScriptEditor).FixSecretsKillingMusic;
        set => (ScriptEditor as TR1ScriptEditor).FixSecretsKillingMusic = value;
    }

    public bool FixSpeechesKillingMusic
    {
        get => (ScriptEditor as TR1ScriptEditor).FixSpeechesKillingMusic;
        set => (ScriptEditor as TR1ScriptEditor).FixSpeechesKillingMusic = value;
    }

    public bool FixDescendingGlitch
    {
        get => (ScriptEditor as TR1ScriptEditor).FixDescendingGlitch;
        set => (ScriptEditor as TR1ScriptEditor).FixDescendingGlitch = value;
    }

    public bool FixWallJumpGlitch
    {
        get => (ScriptEditor as TR1ScriptEditor).FixWallJumpGlitch;
        set => (ScriptEditor as TR1ScriptEditor).FixWallJumpGlitch = value;
    }

    public bool FixBridgeCollision
    {
        get => (ScriptEditor as TR1ScriptEditor).FixBridgeCollision;
        set => (ScriptEditor as TR1ScriptEditor).FixBridgeCollision = value;
    }

    public bool FixQwopGlitch
    {
        get => (ScriptEditor as TR1ScriptEditor).FixQwopGlitch;
        set => (ScriptEditor as TR1ScriptEditor).FixQwopGlitch = value;
    }

    public bool FixAlligatorAi
    {
        get => (ScriptEditor as TR1ScriptEditor).FixAlligatorAi;
        set => (ScriptEditor as TR1ScriptEditor).FixAlligatorAi = value;
    }

    public bool ChangePierreSpawn
    {
        get => (ScriptEditor as TR1ScriptEditor).ChangePierreSpawn;
        set => (ScriptEditor as TR1ScriptEditor).ChangePierreSpawn = value;
    }

    public int FovValue
    {
        get => (ScriptEditor as TR1ScriptEditor).FovValue;
        set => (ScriptEditor as TR1ScriptEditor).FovValue = value;
    }

    public bool FovVertical
    {
        get => (ScriptEditor as TR1ScriptEditor).FovVertical;
        set => (ScriptEditor as TR1ScriptEditor).FovVertical = value;
    }

    public bool EnableFmv
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableFmv;
        set => (ScriptEditor as TR1ScriptEditor).EnableFmv = value;
    }

    public bool EnableCine
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableCine;
        set => (ScriptEditor as TR1ScriptEditor).EnableCine = value;
    }

    public bool EnableMusicInMenu
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableMusicInMenu;
        set => (ScriptEditor as TR1ScriptEditor).EnableMusicInMenu = value;
    }

    public bool EnableMusicInInventory
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableMusicInInventory;
        set => (ScriptEditor as TR1ScriptEditor).EnableMusicInInventory = value;
    }

    public bool DisableTRexCollision
    {
        get => (ScriptEditor as TR1ScriptEditor).DisableTrexCollision;
        set => (ScriptEditor as TR1ScriptEditor).DisableTrexCollision = value;
    }

    public double AnisotropyFilter
    {
        get => (ScriptEditor as TR1ScriptEditor).AnisotropyFilter;
        set => (ScriptEditor as TR1ScriptEditor).AnisotropyFilter = value;
    }

    public int ResolutionWidth
    {
        get => (ScriptEditor as TR1ScriptEditor).ResolutionWidth;
        set => (ScriptEditor as TR1ScriptEditor).ResolutionWidth = value;
    }

    public int ResolutionHeight
    {
        get => (ScriptEditor as TR1ScriptEditor).ResolutionHeight;
        set => (ScriptEditor as TR1ScriptEditor).ResolutionHeight = value;
    }

    public bool EnableRoundShadow
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableRoundShadow;
        set => (ScriptEditor as TR1ScriptEditor).EnableRoundShadow = value;
    }

    public bool Enable3dPickups
    {
        get => (ScriptEditor as TR1ScriptEditor).Enable3dPickups;
        set => (ScriptEditor as TR1ScriptEditor).Enable3dPickups = value;
    }

    public TRScreenshotFormat ScreenshotFormat
    {
        get => (ScriptEditor as TR1ScriptEditor).ScreenshotFormat;
        set => (ScriptEditor as TR1ScriptEditor).ScreenshotFormat = value;
    }

    public bool WalkToItems
    {
        get => (ScriptEditor as TR1ScriptEditor).WalkToItems;
        set => (ScriptEditor as TR1ScriptEditor).WalkToItems = value;
    }

    public int MaximumSaveSlots
    {
        get => (ScriptEditor as TR1ScriptEditor).MaximumSaveSlots;
        set => (ScriptEditor as TR1ScriptEditor).MaximumSaveSlots = value;
    }

    public bool RevertToPistols
    {
        get => (ScriptEditor as TR1ScriptEditor).RevertToPistols;
        set => (ScriptEditor as TR1ScriptEditor).RevertToPistols = value;
    }

    public bool EnableEnhancedSaves
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableEnhancedSaves;
        set => (ScriptEditor as TR1ScriptEditor).EnableEnhancedSaves = value;
    }

    public bool EnablePitchedSounds
    {
        get => (ScriptEditor as TR1ScriptEditor).EnablePitchedSounds;
        set => (ScriptEditor as TR1ScriptEditor).EnablePitchedSounds = value;
    }

    public bool EnableJumpTwists
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableJumpTwists;
        set => (ScriptEditor as TR1ScriptEditor).EnableJumpTwists = value;
    }

    public bool EnableInvertedLook
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableInvertedLook;
        set => (ScriptEditor as TR1ScriptEditor).EnableInvertedLook = value;
    }

    public int CameraSpeed
    {
        get => (ScriptEditor as TR1ScriptEditor).CameraSpeed;
        set => (ScriptEditor as TR1ScriptEditor).CameraSpeed = value;
    }

    public bool EnableSwingCancel
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableSwingCancel;
        set => (ScriptEditor as TR1ScriptEditor).EnableSwingCancel = value;
    }

    public bool EnableTr2Jumping
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableTr2Jumping;
        set => (ScriptEditor as TR1ScriptEditor).EnableTr2Jumping = value;
    }

    public bool FixBearAi
    {
        get => (ScriptEditor as TR1ScriptEditor).FixBearAi;
        set => (ScriptEditor as TR1ScriptEditor).FixBearAi = value;
    }

    public bool LoadCurrentMusic
    {
        get => (ScriptEditor as TR1ScriptEditor).LoadCurrentMusic;
        set => (ScriptEditor as TR1ScriptEditor).LoadCurrentMusic = value;
    }

    public bool LoadMusicTriggers
    {
        get => (ScriptEditor as TR1ScriptEditor).LoadMusicTriggers;
        set => (ScriptEditor as TR1ScriptEditor).LoadMusicTriggers = value;
    }

    public bool ConvertDroppedGuns
    {
        get => (ScriptEditor as TR1ScriptEditor).ConvertDroppedGuns;
        set => (ScriptEditor as TR1ScriptEditor).ConvertDroppedGuns = value;
    }

    public bool EnableUwRoll
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableUwRoll;
        set => (ScriptEditor as TR1ScriptEditor).EnableUwRoll = value;
    }

    public bool EnableEidosLogo
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableEidosLogo;
        set => (ScriptEditor as TR1ScriptEditor).EnableEidosLogo = value;
    }

    public bool EnableBuffering
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableBuffering;
        set => (ScriptEditor as TR1ScriptEditor).EnableBuffering = value;
    }

    public bool EnableLeanJumping
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableLeanJumping;
        set => (ScriptEditor as TR1ScriptEditor).EnableLeanJumping = value;
    }

    public bool EnableConsole
    {
        get => (ScriptEditor as TR1ScriptEditor).EnableConsole;
        set => (ScriptEditor as TR1ScriptEditor).EnableConsole = value;
    }
    #endregion

    #region LevelRandomizer Passthrough
    public GlobeDisplayOption GlobeDisplay
    {
        get => LevelRandomizer.GlobeDisplay;
        set => LevelRandomizer.GlobeDisplay = value;
    }

    public bool RandomizeSecrets
    {
        get => LevelRandomizer.RandomizeSecrets;
        set
        {
            LevelRandomizer.RandomizeSecrets = value;
            if (value)
            {
                // If we are going to randomize secrets in all levels, we can tell TRGE that 
                // Lair and HSH will have secrets so that rewards can be allocated for collecting
                // them all, plus the stats screen will be accurate.
                ScriptEditor.AllLevelsHaveSecrets = true;
            }
            else
            {
                // Otherwise, just tell the script to use the defaults.
                ScriptEditor.LevelSecretSupportOrganisation = Organisation.Default;
            }
        }
    }

    public bool RandomizeItems
    {
        get => LevelRandomizer.RandomizeItems;
        set => LevelRandomizer.RandomizeItems = value;
    }

    public bool RandomizeEnemies
    {
        get => LevelRandomizer.RandomizeEnemies;
        set => LevelRandomizer.RandomizeEnemies = value;
    }

    public bool RandomizeTextures
    {
        get => LevelRandomizer.RandomizeTextures;
        set => LevelRandomizer.RandomizeTextures = value;
    }

    public int SecretSeed
    {
        get => LevelRandomizer.SecretSeed;
        set => LevelRandomizer.SecretSeed = value;
    }

    public int ItemSeed
    {
        get => LevelRandomizer.ItemSeed;
        set => LevelRandomizer.ItemSeed = value;
    }

    public int EnemySeed
    {
        get => LevelRandomizer.EnemySeed;
        set => LevelRandomizer.EnemySeed = value;
    }

    public int TextureSeed
    {
        get => LevelRandomizer.TextureSeed;
        set => LevelRandomizer.TextureSeed = value;
    }

    public bool PersistTextures
    {
        get => LevelRandomizer.PersistTextureVariants;
        set => LevelRandomizer.PersistTextureVariants = value;
    }

    public bool RandomizeWaterColour
    {
        get => LevelRandomizer.RandomizeWaterColour;
        set => LevelRandomizer.RandomizeWaterColour = value;
    }

    public bool RetainMainLevelTextures
    {
        get => LevelRandomizer.RetainMainLevelTextures;
        set => LevelRandomizer.RetainMainLevelTextures = value;
    }

    public bool RetainKeySpriteTextures
    {
        get => LevelRandomizer.RetainKeySpriteTextures;
        set => LevelRandomizer.RetainKeySpriteTextures = value;
    }

    public bool RetainSecretSpriteTextures
    {
        get => LevelRandomizer.RetainSecretSpriteTextures;
        set => LevelRandomizer.RetainSecretSpriteTextures = value;
    }

    public bool RetainEnemyTextures
    {
        get => LevelRandomizer.RetainEnemyTextures;
        set => LevelRandomizer.RetainEnemyTextures = value;
    }

    public bool RetainLaraTextures
    {
        get => LevelRandomizer.RetainLaraTextures;
        set => LevelRandomizer.RetainLaraTextures = value;
    }

    public uint WireframeLevelCount
    {
        get => LevelRandomizer.WireframeLevelCount;
        set => LevelRandomizer.WireframeLevelCount = value;
    }

    public bool AssaultCourseWireframe
    {
        get => LevelRandomizer.AssaultCourseWireframe;
        set => LevelRandomizer.AssaultCourseWireframe = value;
    }

    public bool UseSolidLaraWireframing
    {
        get => LevelRandomizer.UseSolidLaraWireframing;
        set => LevelRandomizer.UseSolidLaraWireframing = value;
    }

    public bool UseSolidEnemyWireframing
    {
        get => LevelRandomizer.UseSolidEnemyWireframing;
        set => LevelRandomizer.UseSolidEnemyWireframing = value;
    }

    public bool UseSolidInteractableWireframing
    {
        get => LevelRandomizer.UseSolidInteractableWireframing;
        set => LevelRandomizer.UseSolidInteractableWireframing = value;
    }

    public bool UseDifferentWireframeColours
    {
        get => LevelRandomizer.UseDifferentWireframeColours;
        set => LevelRandomizer.UseDifferentWireframeColours = value;
    }

    public bool UseWireframeLadders
    {
        get => LevelRandomizer.UseWireframeLadders;
        set => LevelRandomizer.UseWireframeLadders = value;
    }

    public bool ShowWireframeTriggers
    {
        get => LevelRandomizer.ShowWireframeTriggers;
        set => LevelRandomizer.ShowWireframeTriggers = value;
    }

    public bool ShowWireframeTriggerColours
    {
        get => LevelRandomizer.ShowWireframeTriggerColours;
        set => LevelRandomizer.ShowWireframeTriggerColours = value;
    }

    public bool HardSecrets
    {
        get => LevelRandomizer.HardSecrets;
        set => LevelRandomizer.HardSecrets = value;
    }

    public bool GlitchedSecrets
    {
        get => LevelRandomizer.GlitchedSecrets;
        set => LevelRandomizer.GlitchedSecrets = value;
    }

    public bool GuaranteeSecrets
    {
        get => LevelRandomizer.GuaranteeSecrets;
        set => LevelRandomizer.GuaranteeSecrets = value;
    }

    public bool UseRewardRoomCameras
    {
        get => LevelRandomizer.UseRewardRoomCameras;
        set => LevelRandomizer.UseRewardRoomCameras = value;
    }

    public bool UseRandomSecretModels
    {
        get => LevelRandomizer.UseRandomSecretModels;
        set => LevelRandomizer.UseRandomSecretModels = value;
    }

    public TRSecretCountMode SecretCountMode
    {
        get => LevelRandomizer.SecretCountMode;
        set => LevelRandomizer.SecretCountMode = value;
    }

    public uint MinSecretCount
    {
        get => LevelRandomizer.MinSecretCount;
        set => LevelRandomizer.MinSecretCount = value;
    }

    public uint MaxSecretCount
    {
        get => LevelRandomizer.MaxSecretCount;
        set => LevelRandomizer.MaxSecretCount = value;
    }

    public bool UseSecretPack
    {
        get => LevelRandomizer.UseSecretPack;
        set => LevelRandomizer.UseSecretPack = value;
    }

    public string SecretPack
    {
        get => LevelRandomizer.SecretPack;
        set => LevelRandomizer.SecretPack = value;
    }

    public string[] AvailableSecretPacks
    {
        get
        {
            ISecretRandomizer randomizer = _editor.Edition.Version switch
            {
                TRVersion.TR1 => new TR1SecretRandomizer(),
                TRVersion.TR2 => new TR2SecretAllocator(),
                TRVersion.TR3 => new TR3SecretRandomizer(),
                _ => throw new Exception(),
            };
            return randomizer.GetPacks()
                .OrderBy(a => a.ToLower())
                .ToArray();
        }
    }

    public bool IncludeKeyItems
    {
        get => LevelRandomizer.IncludeKeyItems;
        set => LevelRandomizer.IncludeKeyItems = value;
    }

    public bool AllowReturnPathLocations
    {
        get => LevelRandomizer.AllowReturnPathLocations;
        set => LevelRandomizer.AllowReturnPathLocations = value;
    }

    public ItemRange KeyItemRange
    {
        get => LevelRandomizer.KeyItemRange;
        set => LevelRandomizer.KeyItemRange = value;
    }

    public bool AllowEnemyKeyDrops
    {
        get => LevelRandomizer.AllowEnemyKeyDrops;
        set => LevelRandomizer.AllowEnemyKeyDrops = value;
    }

    public bool MaintainKeyContinuity
    {
        get => LevelRandomizer.MaintainKeyContinuity;
        set => LevelRandomizer.MaintainKeyContinuity = value;
    }

    public bool IncludeExtraPickups
    {
        get => LevelRandomizer.IncludeExtraPickups;
        set => LevelRandomizer.IncludeExtraPickups = value;
    }

    public bool RandomizeItemTypes
    {
        get => LevelRandomizer.RandomizeItemTypes;
        set => LevelRandomizer.RandomizeItemTypes = value;
    }

    public bool RandomizeItemPositions
    {
        get => LevelRandomizer.RandomizeItemPositions;
        set => LevelRandomizer.RandomizeItemPositions = value;
    }

    public SpriteRandoMode SpriteRandoMode
    {
        get => LevelRandomizer.SpriteRandoMode;
        set => LevelRandomizer.SpriteRandoMode = value;
    }
    public bool RandomizeItemSprites
    {
        get => LevelRandomizer.RandomizeItemSprites;
        set => LevelRandomizer.RandomizeItemSprites = value;
    }
    public bool RandomizeKeyItemSprites
    {
        get => LevelRandomizer.RandomizeKeyItemSprites;
        set => LevelRandomizer.RandomizeKeyItemSprites = value;
    }
    public bool RandomizeSecretSprites
    {
        get => LevelRandomizer.RandomizeSecretSprites;
        set => LevelRandomizer.RandomizeSecretSprites = value;
    }

    public bool DevelopmentMode
    {
        get => LevelRandomizer.DevelopmentMode;
        set => LevelRandomizer.DevelopmentMode = value;
    }

    public ItemDifficulty RandoItemDifficulty
    {
        get => LevelRandomizer.RandoItemDifficulty;
        set => LevelRandomizer.RandoItemDifficulty = value;
    }

    public bool CrossLevelEnemies
    {
        get => LevelRandomizer.CrossLevelEnemies;
        set => LevelRandomizer.CrossLevelEnemies = value;
    }

    public bool ProtectMonks
    {
        get => LevelRandomizer.ProtectMonks;
        set => LevelRandomizer.ProtectMonks = value;
    }

    public bool DocileWillard
    {
        get => LevelRandomizer.DocileWillard;
        set => LevelRandomizer.DocileWillard = value;
    }

    public BirdMonsterBehaviour BirdMonsterBehaviour
    {
        get => LevelRandomizer.BirdMonsterBehaviour;
        set => LevelRandomizer.BirdMonsterBehaviour = value;
    }

    public RandoDifficulty RandoEnemyDifficulty
    {
        get => LevelRandomizer.RandoEnemyDifficulty;
        set => LevelRandomizer.RandoEnemyDifficulty = value;
    }

    public DragonSpawnType DragonSpawnType
    {
        get => LevelRandomizer.DragonSpawnType;
        set => LevelRandomizer.DragonSpawnType = value;
    }

    public bool UseEnemyExclusions
    {
        get => LevelRandomizer.UseEnemyExclusions;
        set => LevelRandomizer.UseEnemyExclusions = value;
    }

    public bool ShowExclusionWarnings
    {
        get => LevelRandomizer.ShowExclusionWarnings;
        set => LevelRandomizer.ShowExclusionWarnings = value;
    }

    public List<short> ExcludedEnemies
    {
        get => LevelRandomizer.ExcludedEnemies;
        set => LevelRandomizer.ExcludedEnemies = value;
    }

    public Dictionary<short, string> ExcludableEnemies
    {
        get => LevelRandomizer.ExcludableEnemies;
    }

    public List<short> IncludedEnemies
    {
        get => LevelRandomizer.IncludedEnemies;
    }

    public bool SwapEnemyAppearance
    {
        get => LevelRandomizer.SwapEnemyAppearance;
        set => LevelRandomizer.SwapEnemyAppearance = value;
    }

    public bool AllowEmptyEggs
    {
        get => LevelRandomizer.AllowEmptyEggs;
        set => LevelRandomizer.AllowEmptyEggs = value;
    }

    public bool HideEnemiesUntilTriggered
    {
        get => LevelRandomizer.HideEnemiesUntilTriggered;
        set => LevelRandomizer.HideEnemiesUntilTriggered = value;
    }

    public bool RemoveLevelEndingLarson
    {
        get => LevelRandomizer.ReplaceRequiredEnemies;
        set => LevelRandomizer.ReplaceRequiredEnemies = value;
    }

    public bool GiveUnarmedItems
    {
        get => LevelRandomizer.GiveUnarmedItems;
        set => LevelRandomizer.GiveUnarmedItems = value;
    }

    public bool UseEnemyClones
    {
        get => LevelRandomizer.UseEnemyClones;
        set => LevelRandomizer.UseEnemyClones = value;
    }

    public uint EnemyMultiplier
    {
        get => LevelRandomizer.EnemyMultiplier;
        set => LevelRandomizer.EnemyMultiplier = value;
    }

    public static uint MaxEnemyMultiplier => TR1EnemyRandomizer.MaxClones;

    public bool CloneOriginalEnemies
    {
        get => LevelRandomizer.CloneOriginalEnemies;
        set => LevelRandomizer.CloneOriginalEnemies = value;
    }

    public bool UseKillableClonePierres
    {
        get => LevelRandomizer.UseKillableClonePierres;
        set => LevelRandomizer.UseKillableClonePierres = value;
    }

    public bool RandomizeOutfits
    {
        get => LevelRandomizer.RandomizeOutfits;
        set => LevelRandomizer.RandomizeOutfits = value;
    }

    public bool PersistOutfits
    {
        get => LevelRandomizer.PersistOutfits;
        set => LevelRandomizer.PersistOutfits = value;
    }

    public bool RemoveRobeDagger
    {
        get => LevelRandomizer.RemoveRobeDagger;
        set => LevelRandomizer.RemoveRobeDagger = value;
    }

    public uint HaircutLevelCount
    {
        get => LevelRandomizer.HaircutLevelCount;
        set => LevelRandomizer.HaircutLevelCount = value;
    }

    public bool AssaultCourseHaircut
    {
        get => LevelRandomizer.AssaultCourseHaircut;
        set => LevelRandomizer.AssaultCourseHaircut = value;
    }

    public uint InvisibleLevelCount
    {
        get => LevelRandomizer.InvisibleLevelCount;
        set => LevelRandomizer.InvisibleLevelCount = value;
    }

    public bool AllowGymOutfit
    {
        get => LevelRandomizer.AllowGymOutfit;
        set => LevelRandomizer.AllowGymOutfit = value;
    }

    public bool AssaultCourseInvisible
    {
        get => LevelRandomizer.AssaultCourseInvisible;
        set => LevelRandomizer.AssaultCourseInvisible = value;
    }

    public int OutfitSeed
    {
        get => LevelRandomizer.OutfitSeed;
        set => LevelRandomizer.OutfitSeed = value;
    }

    public bool RandomizeGameStrings
    {
        get => LevelRandomizer.RandomizeGameStrings;
        set => LevelRandomizer.RandomizeGameStrings = value;
    }

    public Language GameStringLanguage
    {
        get => LevelRandomizer.GameStringLanguage;
        set => LevelRandomizer.GameStringLanguage = value;
    }

    public Language[] AvailableGameStringLanguages
    {
        get
        {
            G11NGame game;
            switch (_editor.Edition.Version)
            {
                case TRVersion.TR1:
                    game = G11NGame.TR1;
                    break;
                case TRVersion.TR2:
                    game = G11NGame.TR2;
                    break;
                case TRVersion.TR3:
                    game = G11NGame.TR3;
                    break;
                default:
                    return null;
            }

            return G11N.GetSupportedLanguages(game).ToArray();
        }
    }

    public bool RetainKeyItemNames
    {
        get => LevelRandomizer.RetainKeyItemNames;
        set => LevelRandomizer.RetainKeyItemNames = value;
    }

    public bool RetainLevelNames
    {
        get => LevelRandomizer.RetainLevelNames;
        set => LevelRandomizer.RetainLevelNames = value;
    }

    public int GameStringsSeed
    {
        get => LevelRandomizer.GameStringsSeed;
        set => LevelRandomizer.GameStringsSeed = value;
    }

    public bool RandomizeNightMode
    {
        get => LevelRandomizer.RandomizeNightMode;
        set => LevelRandomizer.RandomizeNightMode = value;
    }

    public int NightModeSeed
    {
        get => LevelRandomizer.NightModeSeed;
        set => LevelRandomizer.NightModeSeed = value;
    }

    public uint NightModeCount
    {
        get => LevelRandomizer.NightModeCount;
        set => LevelRandomizer.NightModeCount = value;
    }

    public uint NightModeDarkness
    {
        get => LevelRandomizer.NightModeDarkness;
        set => LevelRandomizer.NightModeDarkness = value;
    }

    public static uint NightModeDarknessRange => RandoConsts.DarknessRange;

    public bool NightModeAssaultCourse
    {
        get => LevelRandomizer.NightModeAssaultCourse;
        set => LevelRandomizer.NightModeAssaultCourse = value;
    }

    public bool OverrideSunsets
    {
        get => LevelRandomizer.OverrideSunsets;
        set => LevelRandomizer.OverrideSunsets = value;
    }

    public bool RandomizeAudio
    {
        get => LevelRandomizer.RandomizeAudio;
        set => LevelRandomizer.RandomizeAudio = value;
    }

    public bool ChangeTriggerTracks
    {
        get => LevelRandomizer.ChangeTriggerTracks;
        set => LevelRandomizer.ChangeTriggerTracks = value;
    }

    public bool SeparateSecretTracks
    {
        get => LevelRandomizer.SeparateSecretTracks;
        set => LevelRandomizer.SeparateSecretTracks = value;
    }

    public bool ChangeWeaponSFX
    {
        get => LevelRandomizer.ChangeWeaponSFX;
        set => LevelRandomizer.ChangeWeaponSFX = value;
    }

    public bool ChangeCrashSFX
    {
        get => LevelRandomizer.ChangeCrashSFX;
        set => LevelRandomizer.ChangeCrashSFX = value;
    }

    public bool ChangeEnemySFX
    {
        get => LevelRandomizer.ChangeEnemySFX;
        set => LevelRandomizer.ChangeEnemySFX = value;
    }

    public bool ChangeDoorSFX
    {
        get => LevelRandomizer.ChangeDoorSFX;
        set => LevelRandomizer.ChangeDoorSFX = value;
    }

    public bool LinkCreatureSFX
    {
        get => LevelRandomizer.LinkCreatureSFX;
        set => LevelRandomizer.LinkCreatureSFX = value;
    }

    public uint UncontrolledSFXCount
    {
        get => LevelRandomizer.UncontrolledSFXCount;
        set => LevelRandomizer.UncontrolledSFXCount = value;
    }

    public bool UncontrolledSFXAssaultCourse
    {
        get => LevelRandomizer.UncontrolledSFXAssaultCourse;
        set => LevelRandomizer.UncontrolledSFXAssaultCourse = value;
    }

    public bool RandomizeWibble
    {
        get => LevelRandomizer.RandomizeWibble;
        set => LevelRandomizer.RandomizeWibble = value;
    }

    public bool RandomizeStartPosition
    {
        get => LevelRandomizer.RandomizeStartPosition;
        set => LevelRandomizer.RandomizeStartPosition = value;
    }

    public bool RotateStartPositionOnly
    {
        get => LevelRandomizer.RotateStartPositionOnly;
        set => LevelRandomizer.RotateStartPositionOnly = value;
    }

    public int StartPositionSeed
    {
        get => LevelRandomizer.StartPositionSeed;
        set => LevelRandomizer.StartPositionSeed = value;
    }

    public bool RandomizeEnvironment
    {
        get => LevelRandomizer.RandomizeEnvironment;
        set => LevelRandomizer.RandomizeEnvironment = value;
    }

    public bool RandomizeWaterLevels
    {
        get => LevelRandomizer.RandomizeWaterLevels;
        set => LevelRandomizer.RandomizeWaterLevels = value;
    }

    public bool RandomizeSlotPositions
    {
        get => LevelRandomizer.RandomizeSlotPositions;
        set => LevelRandomizer.RandomizeSlotPositions = value;
    }

    public bool RandomizeLadders
    {
        get => LevelRandomizer.RandomizeLadders;
        set => LevelRandomizer.RandomizeLadders = value;
    }

    public bool RandomizeTraps
    {
        get => LevelRandomizer.RandomizeTraps;
        set => LevelRandomizer.RandomizeTraps = value;
    }

    public bool RandomizeChallengeRooms
    {
        get => LevelRandomizer.RandomizeChallengeRooms;
        set => LevelRandomizer.RandomizeChallengeRooms = value;
    }

    public bool HardEnvironmentMode
    {
        get => LevelRandomizer.HardEnvironmentMode;
        set => LevelRandomizer.HardEnvironmentMode = value;
    }

    public bool BlockShortcuts
    {
        get => LevelRandomizer.BlockShortcuts;
        set => LevelRandomizer.BlockShortcuts = value;
    }

    public uint MirroredLevelCount
    {
        get => LevelRandomizer.MirroredLevelCount;
        set => LevelRandomizer.MirroredLevelCount = value;
    }

    public bool MirrorAssaultCourse
    {
        get => LevelRandomizer.MirrorAssaultCourse;
        set => LevelRandomizer.MirrorAssaultCourse = value;
    }

    public int EnvironmentSeed
    {
        get => LevelRandomizer.EnvironmentSeed;
        set => LevelRandomizer.EnvironmentSeed = value;
    }

    public bool AddReturnPaths
    {
        get => LevelRandomizer.AddReturnPaths;
        set => LevelRandomizer.AddReturnPaths = value;
    }

    public bool FixOGBugs
    {
        get => LevelRandomizer.FixOGBugs;
        set => LevelRandomizer.FixOGBugs = value;
    }

    public bool RandomizeVfx
    {
        get => LevelRandomizer.RandomizeVfx;
        set => LevelRandomizer.RandomizeVfx = value;
    }

    public Color VfxFilterColor
    {
        get => LevelRandomizer.VfxFilterColor;
        set => LevelRandomizer.VfxFilterColor = value;
    }

    public static Color[] VfxAvailableColorChoices
    {
        get
        {
            return ColorUtilities.GetAvailableColors();
        }
    }

    public bool VfxVivid
    {
        get => LevelRandomizer.VfxVivid;
        set => LevelRandomizer.VfxVivid = value;
    }

    public bool VfxLevel
    {
        get => LevelRandomizer.VfxLevel;
        set => LevelRandomizer.VfxLevel = value;
    }

    public bool VfxRoom
    {
        get => LevelRandomizer.VfxRoom;
        set => LevelRandomizer.VfxRoom = value;
    }

    public bool VfxCaustics
    {
        get => LevelRandomizer.VfxCaustics;
        set => LevelRandomizer.VfxCaustics = value;
    }

    public bool VfxWave
    {
        get => LevelRandomizer.VfxWave;
        set => LevelRandomizer.VfxWave = value;
    }

    public bool RandomizeWeather
    {
        get => LevelRandomizer.RandomizeWeather;
        set => LevelRandomizer.RandomizeWeather = value;
    }

    public int WeatherSeed
    {
        get => LevelRandomizer.WeatherSeed;
        set => LevelRandomizer.WeatherSeed = value;
    }

    public uint RainLevelCount
    {
        get => LevelRandomizer.RainLevelCount;
        set => LevelRandomizer.RainLevelCount = value;
    }

    public uint SnowLevelCount
    {
        get => LevelRandomizer.SnowLevelCount;
        set => LevelRandomizer.SnowLevelCount = value;
    }

    public uint ColdLevelCount
    {
        get => LevelRandomizer.ColdLevelCount;
        set => LevelRandomizer.ColdLevelCount = value;
    }

    public bool RainyAssaultCourse
    {
        get => LevelRandomizer.RainyAssaultCourse;
        set => LevelRandomizer.RainyAssaultCourse = value;
    }

    public bool SnowyAssaultCourse
    {
        get => LevelRandomizer.SnowyAssaultCourse;
        set => LevelRandomizer.SnowyAssaultCourse = value;
    }

    public bool ColdAssaultCourse
    {
        get => LevelRandomizer.ColdAssaultCourse;
        set => LevelRandomizer.ColdAssaultCourse = value;
    }
    #endregion

    #region TREditor Passthrough
    public string EditionTitle => _editor.Edition.Title;
    public bool IsExportPossible => _editor.IsExportPossible;
    public string ErrorDirectory => _editor.ErrorDirectory;
    public string BackupDirectory => _editor.BackupDirectory;
    public string TargetDirectory => _editor.TargetDirectory;

    public event EventHandler<FileSystemEventArgs> ConfigExternallyChanged
    {
        add => _editor.ConfigExternallyChanged += value;
        remove => _editor.ConfigExternallyChanged -= value;
    }

    public event EventHandler<TRRandomizationEventArgs> RandomizationProgressChanged;

    private TRRandomizationEventArgs _randoEventArgs;

    public void Randomize()
    {
        SetScriptSequencing();
        _randoEventArgs = new();
        _editor.Save();
    }

    private void Editor_SaveProgressChanged(object sender, TRSaveEventArgs e)
    {
        _randoEventArgs.Copy(e);
        RandomizationProgressChanged?.Invoke(this, _randoEventArgs);
    }

    public event EventHandler<TROpenRestoreEventArgs> RestoreProgressChanged;
    private TROpenRestoreEventArgs _restoreEventArgs;

    public void Restore()
    {
        _restoreEventArgs = new TROpenRestoreEventArgs();
        _editor.Restore();
    }

    private void Editor_RestoreProgressChanged(object sender, TRBackupRestoreEventArgs e)
    {
        _restoreEventArgs.Copy(e);
        RestoreProgressChanged?.Invoke(this, _restoreEventArgs);
    }

    public void ImportSettings(string filePath)
    {
        _editor.ImportSettings(filePath);
    }

    public void ExportSettings(string filePath)
    {
        _editor.ExportSettings(filePath);
    }

    public void ResetSettings()
    {
        _editor.ResetSettings();
    }

    public void Unload()
    {
        _editor.Unload();
    }
    #endregion
}
