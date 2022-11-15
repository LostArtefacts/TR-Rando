using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using TRGE.Core;
using TRRandomizerCore;
using TRRandomizerCore.Globalisation;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Secrets;

namespace TRRandomizerView.Model
{
    public class ControllerOptions : INotifyPropertyChanged
    {
        public int MaxSeedValue => 1000000000;

        private readonly OptionGenerator _optionRandomizer;

        private readonly ManagedSeed _secretRewardsControl;
        private readonly ManagedSeedNumeric _levelSequencingControl, _unarmedLevelsControl, _ammolessLevelsControl, _sunsetLevelsControl, _nightLevelsControl;
        private readonly ManagedSeedBool _audioTrackControl, _healthLevelsControl;

        private readonly ManagedSeedBool _randomSecretsControl, _randomItemsControl, _randomEnemiesControl, _randomTexturesControl, _randomOutfitsControl, _randomTextControl, _randomStartControl, _randomEnvironmentControl;

        private bool _disableDemos, _autoLaunchGame, _puristMode;

        private BoolItemControlClass _isHardSecrets, _allowGlitched, _useRewardRoomCameras, _useRandomSecretModels;
        private TRSecretCountMode _secretCountMode;
        private uint _minSecretCount, _maxSecretCount;
        private BoolItemControlClass _includeKeyItems, _includeExtraPickups, _randomizeItemTypes, _randomizeItemLocations;
        private BoolItemControlClass _crossLevelEnemies, _protectMonks, _docileWillard, _swapEnemyAppearance, _allowEmptyEggs, _hideEnemies, _removeLevelEndingLarson;
        private BoolItemControlClass _persistTextures, _randomizeWaterColour, _retainLevelTextures, _retainKeySpriteTextures, _retainSecretSpriteTextures;
        private BoolItemControlClass _changeAmbientTracks, _includeBlankTracks, _changeTriggerTracks, _separateSecretTracks, _changeWeaponSFX, _changeCrashSFX, _changeEnemySFX, _changeDoorSFX, _linkCreatureSFX, _randomizeWibble;
        private BoolItemControlClass _persistOutfits, _removeRobeDagger, _allowGymOutfit;
        private BoolItemControlClass _retainKeyItemNames, _retainLevelNames;
        private BoolItemControlClass _rotateStartPosition;
        private BoolItemControlClass _randomizeWaterLevels, _randomizeSlotPositions, _randomizeLadders;
        private BoolItemControlClass _disableHealingBetweenLevels, _disableMedpacks;
        private uint _mirroredLevelCount;
        private bool _mirrorAssaultCourse;
        private uint _haircutLevelCount;
        private bool _assaultCourseHaircut;
        private uint _invisibleLevelCount;
        private bool _assaultCourseInvisible;
        private uint _nightModeDarkness;
        private uint _nightModeDarknessMaximum;
        private uint _wireframeLevelCount;
        private bool _assaultCourseWireframe;
        private bool _useSolidLaraWireframing;
        private bool _useSolidEnemyWireframing;
        private bool _useDifferentWireframeColours;
        private bool _useWireframeLadders;
        private bool _nightModeAssaultCourse;
        private bool _overrideSunsets;
        private Color _vfxFilterColor;
        private bool _vfxRandomize;
        private Color[] _vfxAvailColors;
        private bool _vfxVivid;
        private bool _vfxLevel;
        private bool _vfxRoom;
        private bool _vfxCaustics;
        private bool _vfxWave;
        private uint _uncontrolledSFXCount;
        private bool _uncontrolledSFXAssaultCourse;

        private List<BoolItemControlClass> _secretBoolItemControls, _itemBoolItemControls, _enemyBoolItemControls, _textureBoolItemControls, _audioBoolItemControls, _outfitBoolItemControls, _textBoolItemControls, _startBoolItemControls, _environmentBoolItemControls, _healthBoolItemControls;
        private List<BoolItemIDControlClass> _selectableEnemies;
        private bool _useEnemyExclusions, _showExclusionWarnings;

        private RandoDifficulty _randoEnemyDifficulty;
        private ItemDifficulty _randoItemDifficulty;
        private GlobeDisplayOption _globeDisplayOption;
        private BirdMonsterBehaviour _birdMonsterBehaviour;
        private DragonSpawnType _dragonSpawnType;

        private Language[] _availableLanguages;
        private Language _gameStringLanguage;

        private int _levelCount, _maximumLevelCount, _defaultUnarmedLevelCount, _defaultAmmolessLevelCount, _defaultSunsetCount;

        private uint _minStartingHealth, _maxStartingHealth, _medilessLevelCount;
        private bool _useRecommendedCommunitySettings;

        private SpriteRandoMode _spriteRandoMode;
        private bool _randomizeItemSprites, _randomizeKeyItemSprites, _randomizeSecretSprites;

        #region T1M Sepcifics

        private bool _enableGameModes;
        public bool EnableGameModes
        {
            get => _enableGameModes;
            set
            {
                _enableGameModes = value;
                FirePropertyChanged();
            }
        }

        private bool _enableSaveCrystals;
        public bool EnableSaveCrystals
        {
            get => _enableSaveCrystals;
            set
            {
                _enableSaveCrystals = value;
                FirePropertyChanged();
            }
        }

        private double _demoDelay;
        public double DemoDelay
        {
            get => _demoDelay;
            set
            {
                _demoDelay = value;
                FirePropertyChanged();
            }
        }

        private double _drawDistanceFade;
        public double DrawDistanceFade
        {
            get => _drawDistanceFade;
            set
            {
                _drawDistanceFade = value;
                FirePropertyChanged();
            }
        }

        private double _drawDistanceMax;
        public double DrawDistanceMax
        {
            get => _drawDistanceMax;
            set
            {
                _drawDistanceMax = value;
                FirePropertyChanged();
            }
        }

        private double[] _waterColor;
        public double[] WaterColor
        {
            get => _waterColor;
            set
            {
                _waterColor = value;
                FirePropertyChanged();
            }
        }

        public double WaterColorR
        {
            get => _waterColor[0];
            set
            {
                _waterColor[0] = value;
                FirePropertyChanged(nameof(WaterColor));
            }
        }

        public double WaterColorG
        {
            get => _waterColor[1];
            set
            {
                _waterColor[1] = value;
                FirePropertyChanged(nameof(WaterColor));
            }
        }

        public double WaterColorB
        {
            get => _waterColor[2];
            set
            {
                _waterColor[2] = value;
                FirePropertyChanged(nameof(WaterColor));
            }
        }

        private bool _disableMagnums;
        public bool DisableMagnums
        {
            get => _disableMagnums;
            set
            {
                _disableMagnums = value;
                FirePropertyChanged();
            }
        }

        private bool _disableUzis;
        public bool DisableUzis
        {
            get => _disableUzis;
            set
            {
                _disableUzis = value;
                FirePropertyChanged();
            }
        }

        private bool _disableShotgun;
        public bool DisableShotgun
        {
            get => _disableShotgun;
            set
            {
                _disableShotgun = value;
                FirePropertyChanged();
            }
        }

        private bool _enableDeathsCounter;
        public bool EnableDeathsCounter
        {
            get => _enableDeathsCounter;
            set
            {
                _enableDeathsCounter = value;
                FirePropertyChanged();
            }
        }

        private bool _enableEnemyHealthbar;
        public bool EnableEnemyHealthbar
        {
            get => _enableEnemyHealthbar;
            set
            {
                _enableEnemyHealthbar = value;
                FirePropertyChanged();
            }
        }

        private bool _enableEnhancedLook;
        public bool EnableEnhancedLook
        {
            get => _enableEnhancedLook;
            set
            {
                _enableEnhancedLook = value;
                FirePropertyChanged();
            }
        }

        private bool _enableShotgunFlash;
        public bool EnableShotgunFlash
        {
            get => _enableShotgunFlash;
            set
            {
                _enableShotgunFlash = value;
                FirePropertyChanged();
            }
        }

        private bool _fixShotgunTargeting;
        public bool FixShotgunTargeting
        {
            get => _fixShotgunTargeting;
            set
            {
                _fixShotgunTargeting = value;
                FirePropertyChanged();
            }
        }

        private bool _enableNumericKeys;
        public bool EnableNumericKeys
        {
            get => _enableNumericKeys;
            set
            {
                _enableNumericKeys = value;
                FirePropertyChanged();
            }
        }

        private bool _enableTr3Sidesteps;
        public bool EnableTr3Sidesteps
        {
            get => _enableTr3Sidesteps;
            set
            {
                _enableTr3Sidesteps = value;
                FirePropertyChanged();
            }
        }

        private bool _enableCheats;
        public bool EnableCheats
        {
            get => _enableCheats;
            set
            {
                _enableCheats = value;
                FirePropertyChanged();
            }
        }

        private bool _enableDetailsStats;
        public bool EnableDetailedStats
        {
            get => _enableDetailsStats;
            set
            {
                _enableDetailsStats = value;
                FirePropertyChanged();
            }
        }

        private bool _enableCompassStats;
        public bool EnableCompassStats
        {
            get => _enableCompassStats;
            set
            {
                _enableCompassStats = value;
                FirePropertyChanged();
            }
        }

        private bool _enableTotalStats;
        public bool EnableTotalStats
        {
            get => _enableTotalStats;
            set
            {
                _enableTotalStats = value;
                FirePropertyChanged();
            }
        }

        private bool _enableTimerInInventory;
        public bool EnableTimerInInventory
        {
            get => _enableTimerInInventory;
            set
            {
                _enableTimerInInventory = value;
                FirePropertyChanged();
            }
        }

        private bool _enableSmoothBars;
        public bool EnableSmoothBars
        {
            get => _enableSmoothBars;
            set
            {
                _enableSmoothBars = value;
                FirePropertyChanged();
            }
        }

        private bool _enableFadeEffects;
        public bool EnableFadeEffects
        {
            get => _enableFadeEffects;
            set
            {
                _enableFadeEffects = value;
                FirePropertyChanged();
            }
        }

        private TRMenuStyle _menuStyle;
        public TRMenuStyle MenuStyle
        {
            get => _menuStyle;
            set
            {
                _menuStyle = value;
                FirePropertyChanged();
            }
        }

        private TRHealthbarMode _healthbarShowingMode;
        public TRHealthbarMode HealthbarShowingMode
        {
            get => _healthbarShowingMode;
            set
            {
                _healthbarShowingMode = value;
                FirePropertyChanged();
            }
        }

        private TRUILocation _healthbarLocation;
        public TRUILocation HealthbarLocation
        {
            get => _healthbarLocation;
            set
            {
                _healthbarLocation = value;
                FirePropertyChanged();
            }
        }

        private TRUIColour _healthbarColor;
        public TRUIColour HealthbarColor
        {
            get => _healthbarColor;
            set
            {
                _healthbarColor = value;
                FirePropertyChanged();
            }
        }

        private TRAirbarMode _airbarShowingMode;
        public TRAirbarMode AirbarShowingMode
        {
            get => _airbarShowingMode;
            set
            {
                _airbarShowingMode = value;
                FirePropertyChanged();
            }
        }

        private TRUILocation _airbarLocation;
        public TRUILocation AirbarLocation
        {
            get => _airbarLocation;
            set
            {
                _airbarLocation = value;
            }
        }

        private TRUIColour _airbarColor;
        public TRUIColour AirbarColor
        {
            get => _airbarColor;
            set
            {
                _airbarColor = value;
                FirePropertyChanged();
            }
        }

        private TRUILocation _enemyHealthbarLocation;
        public TRUILocation EnemyHealthbarLocation
        {
            get => _enemyHealthbarLocation;
            set
            {
                _enemyHealthbarLocation = value;
                FirePropertyChanged();
            }
        }

        private TRUIColour _enemyHealthbarColor;
        public TRUIColour EnemyHealthbarColor
        {
            get => _enemyHealthbarColor;
            set
            {
                _enemyHealthbarColor = value;
                FirePropertyChanged();
            }
        }

        private bool _fixTihocanSecretSound;
        public bool FixTihocanSecretSound
        {
            get => _fixTihocanSecretSound;
            set
            {
                _fixTihocanSecretSound = value;
                FirePropertyChanged();
            }
        }

        private bool _fixPyramidSecretTrigger;
        public bool FixPyramidSecretTrigger
        {
            get => _fixPyramidSecretTrigger;
            set
            {
                _fixPyramidSecretTrigger = value;
                FirePropertyChanged();
            }
        }

        private bool _fixSecretsKillingMusic;
        public bool FixSecretsKillingMusic
        {
            get => _fixSecretsKillingMusic;
            set
            {
                _fixSecretsKillingMusic = value;
                FirePropertyChanged();
            }
        }

        private bool _fixDescendingGlitch;
        public bool FixDescendingGlitch
        {
            get => _fixDescendingGlitch;
            set
            {
                _fixDescendingGlitch = value;
                FirePropertyChanged();
            }
        }

        private bool _fixWallJumpGlitch;
        public bool FixWallJumpGlitch
        {
            get => _fixWallJumpGlitch;
            set
            {
                _fixWallJumpGlitch = value;
                FirePropertyChanged();
            }
        }

        private bool _fixBridgeCollision;
        public bool FixBridgeCollision
        {
            get => _fixBridgeCollision;
            set
            {
                _fixBridgeCollision = value;
                FirePropertyChanged();
            }
        }

        private bool _fixQwopGlitch;
        public bool FixQwopGlitch
        {
            get => _fixQwopGlitch;
            set
            {
                _fixQwopGlitch = value;
                FirePropertyChanged();
            }
        }

        private bool _fixAlligatorAi;
        public bool FixAlligatorAi
        {
            get => _fixAlligatorAi;
            set
            {
                _fixAlligatorAi = value;
                FirePropertyChanged();
            }
        }

        private bool _changePierreSpawn;
        public bool ChangePierreSpawn
        {
            get => _changePierreSpawn;
            set
            {
                _changePierreSpawn = value;
                FirePropertyChanged();
            }
        }

        private int _fovValue;
        public int FovValue
        {
            get => _fovValue;
            set
            {
                _fovValue = value;
                FirePropertyChanged();
            }
        }

        private bool _fovVertical;
        public bool FovVertical
        {
            get => _fovVertical;
            set
            {
                _fovVertical = value;
                FirePropertyChanged();
            }
        }

        private bool _enableFmv;
        public bool EnableFmv
        {
            get => _enableFmv;
            set
            {
                _enableFmv = value;
            }
        }

        private bool _enableCine;
        public bool EnableCine
        {
            get => _enableCine;
            set
            {
                _enableCine = value;
                FirePropertyChanged();
            }
        }

        private bool _enableMusicInMenu;
        public bool EnableMusicInMenu
        {
            get => _enableMusicInMenu;
            set
            {
                _enableMusicInMenu = value;
                FirePropertyChanged();
            }
        }

        private bool _enableMusicInInventory;
        public bool EnableMusicInInventory
        {
            get => _enableMusicInInventory;
            set
            {
                _enableMusicInInventory = value;
                FirePropertyChanged();
            }
        }

        private bool _disableTRexCollision;
        public bool DisableTRexCollision
        {
            get => _disableTRexCollision;
            set
            {
                _disableTRexCollision = value;
                FirePropertyChanged();
            }
        }

        private double _anisotropyFilter;
        public double AnisotropyFilter
        {
            get => _anisotropyFilter;
            set
            {
                _anisotropyFilter = value;
                FirePropertyChanged();
            }
        }

        private int _resolutionWidth;
        public int ResolutionWidth
        {
            get => _resolutionWidth;
            set
            {
                _resolutionWidth = value;
                FirePropertyChanged();
            }
        }

        private int _resolutionHeight;
        public int ResolutionHeight
        {
            get => _resolutionHeight;
            set
            {
                _resolutionHeight = value;
                FirePropertyChanged();
            }
        }

        private bool _enableRoundShadow;
        public bool EnableRoundShadow
        {
            get => _enableRoundShadow;
            set
            {
                _enableRoundShadow = value;
                FirePropertyChanged();
            }
        }

        private bool _enable3dPickups;
        public bool Enable3dPickups
        {
            get => _enable3dPickups;
            set
            {
                _enable3dPickups = value;
                FirePropertyChanged();
            }
        }

        private TRScreenshotFormat _screenshotFormat;
        public TRScreenshotFormat ScreenshotFormat
        {
            get => _screenshotFormat;
            set
            {
                _screenshotFormat = value;
                FirePropertyChanged();
            }
        }

        private bool _walkToItems;
        public bool WalkToItems
        {
            get => _walkToItems;
            set
            {
                _walkToItems = value;
                FirePropertyChanged();
            }
        }

        private int _maximumSaveSlots;
        public int MaximumSaveSlots
        {
            get => _maximumSaveSlots;
            set
            {
                _maximumSaveSlots = value;
            }
        }

        private bool _revertToPistols;
        public bool RevertToPistols
        {
            get => _revertToPistols;
            set
            {
                _revertToPistols = value;
                FirePropertyChanged();
            }
        }

        private bool _enableEnhancedSaves;
        public bool EnableEnhancedSaves
        {
            get => _enableEnhancedSaves;
            set
            {
                _enableEnhancedSaves = value;
                FirePropertyChanged();
            }
        }

        private bool _enablePitchedSounds;
        public bool EnablePitchedSounds
        {
            get => _enablePitchedSounds;
            set
            {
                _enablePitchedSounds = value;
                FirePropertyChanged();
            }
        }

        #endregion

        public int TotalLevelCount
        {
            get => _levelCount;
            private set
            {
                _levelCount = value;
                FirePropertyChanged();
            }
        }

        public int MaximumLevelCount
        {
            get => _maximumLevelCount;
            private set
            {
                _maximumLevelCount = value;
                FirePropertyChanged();
            }
        }

        public int DefaultUnarmedLevelCount
        {
            get => _defaultUnarmedLevelCount;
            private set
            {
                _defaultUnarmedLevelCount = value;
                FirePropertyChanged();
            }
        }

        public int DefaultAmmolessLevelCount
        {
            get => _defaultAmmolessLevelCount;
            private set
            {
                _defaultAmmolessLevelCount = value;
                FirePropertyChanged();
            }
        }

        public int DefaultSunsetCount
        {
            get => _defaultSunsetCount;
            private set
            {
                _defaultSunsetCount = value;
                FirePropertyChanged();
            }
        }

        private void UpdateMaximumLevelCount()
        {
            MaximumLevelCount = RandomizeLevelSequencing ? (int)PlayableLevelCount : TotalLevelCount;
            UnarmedLevelCount = (uint)Math.Min(UnarmedLevelCount, MaximumLevelCount);
            AmmolessLevelCount = (uint)Math.Min(AmmolessLevelCount, MaximumLevelCount);
            SunsetCount = (uint)Math.Min(SunsetCount, MaximumLevelCount);
            NightModeCount = (uint)Math.Min(NightModeCount, MaximumLevelCount);
            MirroredLevelCount = (uint)Math.Min(MirroredLevelCount, MaximumLevelCount);
            HaircutLevelCount = (uint)Math.Min(HaircutLevelCount, MaximumLevelCount);
            InvisibleLevelCount = (uint)Math.Min(InvisibleLevelCount, MaximumLevelCount);
            WireframeLevelCount = (uint)Math.Min(WireframeLevelCount, MaximumLevelCount);
            UncontrolledSFXCount = (uint)Math.Min(UncontrolledSFXCount, MaximumLevelCount);
            MedilessLevelCount = (uint)Math.Min(MedilessLevelCount, MaximumLevelCount);
        }

        public bool RandomizationPossible
        {
            get => RandomizeLevelSequencing || RandomizeUnarmedLevels || RandomizeAmmolessLevels || RandomizeSecretRewards || RandomizeHealth || RandomizeSunsets ||
                   RandomizeAudioTracks || RandomizeItems || RandomizeEnemies || RandomizeSecrets || RandomizeTextures || RandomizeOutfits ||
                   RandomizeText || RandomizeNightMode || RandomizeStartPosition || RandomizeEnvironment;
        }

        public bool RandomizeLevelSequencing
        {
            get => _levelSequencingControl.IsActive;
            set
            {
                _levelSequencingControl.IsActive = value;
                FirePropertyChanged();
                UpdateMaximumLevelCount();
            }
        }

        public GlobeDisplayOption GlobeDisplay
        {
            get => _globeDisplayOption;
            set
            {
                _globeDisplayOption = value;
                FirePropertyChanged();
            }
        }

        public int LevelSequencingSeed
        {
            get => _levelSequencingControl.Seed;
            set
            {
                _levelSequencingControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public uint PlayableLevelCount
        {
            get => (uint)_levelSequencingControl.CustomInt;
            set
            {
                _levelSequencingControl.CustomInt = (int)value;
                FirePropertyChanged();
                UpdateMaximumLevelCount();
            }
        }

        public bool RandomizeUnarmedLevels
        {
            get => _unarmedLevelsControl.IsActive;
            set
            {
                _unarmedLevelsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int UnarmedLevelsSeed
        {
            get => _unarmedLevelsControl.Seed;
            set
            {
                _unarmedLevelsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public uint UnarmedLevelCount
        {
            get => (uint)_unarmedLevelsControl.CustomInt;
            set
            {
                _unarmedLevelsControl.CustomInt = (int)value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeAmmolessLevels
        {
            get => _ammolessLevelsControl.IsActive;
            set
            {
                _ammolessLevelsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int AmmolessLevelsSeed
        {
            get => _ammolessLevelsControl.Seed;
            set
            {
                _ammolessLevelsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public uint AmmolessLevelCount
        {
            get => (uint)_ammolessLevelsControl.CustomInt;
            set
            {
                _ammolessLevelsControl.CustomInt = (int)value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeSecretRewards
        {
            get => _secretRewardsControl.IsActive;
            set
            {
                _secretRewardsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int SecretRewardSeed
        {
            get => _secretRewardsControl.Seed;
            set
            {
                _secretRewardsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeHealth
        {
            get => _healthLevelsControl.IsActive;
            set
            {
                _healthLevelsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int HealthSeed
        {
            get => _healthLevelsControl.Seed;
            set
            {
                _healthLevelsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public uint MedilessLevelCount
        {
            get => _medilessLevelCount;
            set
            {
                _medilessLevelCount = value;
                FirePropertyChanged();
            }
        }

        public uint MinStartingHealth
        {
            get => _minStartingHealth;
            set
            {
                _minStartingHealth = value;
                FirePropertyChanged();
                FirePropertyChanged(nameof(MaxStartingHealth));
            }
        }

        public uint MaxStartingHealth
        {
            get => _maxStartingHealth;
            set
            {
                _maxStartingHealth = value;
                FirePropertyChanged();
                FirePropertyChanged(nameof(MinStartingHealth));
            }
        }

        public BoolItemControlClass DisableHealingBetweenLevels
        {
            get => _disableHealingBetweenLevels;
            set
            {
                _disableHealingBetweenLevels = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass DisableMedpacks
        {
            get => _disableMedpacks;
            set
            {
                _disableMedpacks = value;
                FirePropertyChanged();
            }
        }

        public bool UseRecommendedCommunitySettings
        {
            get => _useRecommendedCommunitySettings;
            set
            {
                _useRecommendedCommunitySettings = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeSunsets
        {
            get => _sunsetLevelsControl.IsActive;
            set
            {
                _sunsetLevelsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int SunsetsSeed
        {
            get => _sunsetLevelsControl.Seed;
            set
            {
                _sunsetLevelsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public uint SunsetCount
        {
            get => (uint)_sunsetLevelsControl.CustomInt;
            set
            {
                _sunsetLevelsControl.CustomInt = (int)value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeNightMode
        {
            get => _nightLevelsControl.IsActive;
            set
            {
                _nightLevelsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int NightModeSeed
        {
            get => _nightLevelsControl.Seed;
            set
            {
                _nightLevelsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public uint NightModeCount
        {
            get => (uint)_nightLevelsControl.CustomInt;
            set
            {
                _nightLevelsControl.CustomInt = (int)value;
                FirePropertyChanged();
            }
        }

        public bool NightModeAssaultCourse
        {
            get => _nightModeAssaultCourse;
            set
            {
                _nightModeAssaultCourse = value;
                FirePropertyChanged();
            }
        }

        public bool OverrideSunsets
        {
            get => _overrideSunsets;
            set
            {
                _overrideSunsets = value;
                FirePropertyChanged();
            }
        }

        public uint NightModeDarkness
        {
            get => _nightModeDarkness;
            set
            {
                _nightModeDarkness = value;
                FirePropertyChanged();
            }
        }

        public uint NightModeDarknessMaximum
        {
            get => _nightModeDarknessMaximum;
            private set
            {
                _nightModeDarknessMaximum = value;
                FirePropertyChanged();
            }
        }

        public Color VfxFilterColor
        {
            get => _vfxFilterColor;
            set
            {
                _vfxFilterColor = value;
                FirePropertyChanged();
            }
        }

        public bool VfxRandomize
        {
            get => _vfxRandomize;
            set
            {
                _vfxRandomize = value;
                FirePropertyChanged();
            }
        }

        public Color[] VfxAvailColors
        {
            get => _vfxAvailColors;
            set
            {
                _vfxAvailColors = value;
                FirePropertyChanged();
            }
        }

        public bool VfxVivid
        {
            get => _vfxVivid;
            set
            {
                _vfxVivid = value;
                FirePropertyChanged();
            }
        }

        public bool VfxRoom
        {
            get => _vfxRoom;
            set
            {
                _vfxRoom = value;
                FirePropertyChanged();
            }
        }

        public bool VfxLevel
        {
            get => _vfxLevel;
            set
            {
                _vfxLevel = value;
                FirePropertyChanged();
            }
        }

        public bool VfxCaustics
        {
            get => _vfxCaustics;
            set
            {
                _vfxCaustics = value;
                FirePropertyChanged();
            }
        }

        public bool VfxWave
        {
            get => _vfxWave;
            set
            {
                _vfxWave = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeAudioTracks
        {
            get => _audioTrackControl.IsActive;
            set
            {
                _audioTrackControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int AudioTracksSeed
        {
            get => _audioTrackControl.Seed;
            set
            {
                _audioTrackControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass ChangeAmbientTracks
        {
            get => _changeAmbientTracks;
            set
            {
                _changeAmbientTracks = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass IncludeBlankTracks
        {
            get => _includeBlankTracks;
            set
            {
                _includeBlankTracks = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass ChangeTriggerTracks
        {
            get => _changeTriggerTracks;
            set
            {
                _changeTriggerTracks = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass SeparateSecretTracks
        {
            get => _separateSecretTracks;
            set
            {
                _separateSecretTracks = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass ChangeWeaponSFX
        {
            get => _changeWeaponSFX;
            set
            {
                _changeWeaponSFX = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass ChangeCrashSFX
        {
            get => _changeCrashSFX;
            set
            {
                _changeCrashSFX = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass ChangeEnemySFX
        {
            get => _changeEnemySFX;
            set
            {
                _changeEnemySFX = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass ChangeDoorSFX
        {
            get => _changeDoorSFX;
            set
            {
                _changeDoorSFX = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass LinkCreatureSFX
        {
            get => _linkCreatureSFX;
            set
            {
                _linkCreatureSFX = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RandomizeWibble
        {
            get => _randomizeWibble;
            set
            {
                _randomizeWibble = value;
                FirePropertyChanged();
            }
        }

        public uint UncontrolledSFXCount
        {
            get => _uncontrolledSFXCount;
            set
            {
                _uncontrolledSFXCount = value;
                FirePropertyChanged();
            }
        }

        public bool UncontrolledSFXAssaultCourse
        {
            get => _uncontrolledSFXAssaultCourse;
            set
            {
                _uncontrolledSFXAssaultCourse = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeSecrets
        {
            get => _randomSecretsControl.IsActive;
            set
            {
                _randomSecretsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int SecretSeed
        {
            get => _randomSecretsControl.Seed;
            set
            {
                _randomSecretsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass IsHardSecrets
        {
            get => _isHardSecrets;
            set
            {
                _isHardSecrets = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeItems
        {
            get => _randomItemsControl.IsActive;
            set
            {
                _randomItemsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int ItemSeed
        {
            get => _randomItemsControl.Seed;
            set
            {
                _randomItemsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass IncludeKeyItems
        {
            get => _includeKeyItems;
            set
            {
                _includeKeyItems = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass IncludeExtraPickups
        {
            get => _includeExtraPickups;
            set
            {
                _includeExtraPickups = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RandomizeItemTypes
        {
            get => _randomizeItemTypes;
            set
            {
                _randomizeItemTypes = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RandomizeItemPositions
        {
            get => _randomizeItemLocations;
            set
            {
                _randomizeItemLocations = value;
                FirePropertyChanged();
            }
        }

        public ItemDifficulty RandoItemDifficulty
        {
            get => _randoItemDifficulty;
            set
            {
                _randoItemDifficulty = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeEnemies
        {
            get => _randomEnemiesControl.IsActive;
            set
            {
                _randomEnemiesControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int EnemySeed
        {
            get => _randomEnemiesControl.Seed;
            set
            {
                _randomEnemiesControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass CrossLevelEnemies
        {
            get => _crossLevelEnemies;
            set
            {
                _crossLevelEnemies = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeTextures
        {
            get => _randomTexturesControl.IsActive;
            set
            {
                _randomTexturesControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int TextureSeed
        {
            get => _randomTexturesControl.Seed;
            set
            {
                _randomTexturesControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass PersistTextures
        {
            get => _persistTextures;
            set
            {
                _persistTextures = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RandomizeWaterColour
        {
            get => _randomizeWaterColour;
            set
            {
                _randomizeWaterColour = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeOutfits
        {
            get => _randomOutfitsControl.IsActive;
            set
            {
                _randomOutfitsControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int OutfitSeed
        {
            get => _randomOutfitsControl.Seed;
            set
            {
                _randomOutfitsControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass PersistOutfits
        {
            get => _persistOutfits;
            set
            {
                _persistOutfits = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RemoveRobeDagger
        {
            get => _removeRobeDagger;
            set
            {
                _removeRobeDagger = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass AllowGymOutfit
        {
            get => _allowGymOutfit;
            set
            {
                _allowGymOutfit = value;
                FirePropertyChanged();
            }
        }

        public uint HaircutLevelCount
        {
            get => _haircutLevelCount;
            set
            {
                _haircutLevelCount = value;
                FirePropertyChanged();
            }
        }

        public bool AssaultCourseHaircut
        {
            get => _assaultCourseHaircut;
            set
            {
                _assaultCourseHaircut = value;
                FirePropertyChanged();
            }
        }

        public uint InvisibleLevelCount
        {
            get => _invisibleLevelCount;
            set
            {
                _invisibleLevelCount = value;
                FirePropertyChanged();
            }
        }

        public bool AssaultCourseInvisible
        {
            get => _assaultCourseInvisible;
            set
            {
                _assaultCourseInvisible = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeText
        {
            get => _randomTextControl.IsActive;
            set
            {
                _randomTextControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int TextSeed
        {
            get => _randomTextControl.Seed;
            set
            {
                _randomTextControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public Language[] AvailableLanguages
        {
            get => _availableLanguages;
            private set
            {
                _availableLanguages = value;
                FirePropertyChanged();
            }
        }

        public Language GameStringLanguage
        {
            get => _gameStringLanguage;
            set
            {
                _gameStringLanguage = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RetainKeyItemNames
        {
            get => _retainKeyItemNames;
            set
            {
                _retainKeyItemNames = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RetainLevelNames
        {
            get => _retainLevelNames;
            set
            {
                _retainLevelNames = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeStartPosition
        {
            get => _randomStartControl.IsActive;
            set
            {
                _randomStartControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int StartPositionSeed
        {
            get => _randomStartControl.Seed;
            set
            {
                _randomStartControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RotateStartPositionOnly
        {
            get => _rotateStartPosition;
            set
            {
                _rotateStartPosition = value;
                FirePropertyChanged();
            }
        }

        public bool RandomizeEnvironment
        {
            get => _randomEnvironmentControl.IsActive;
            set
            {
                _randomEnvironmentControl.IsActive = value;
                FirePropertyChanged();
            }
        }

        public int EnvironmentSeed
        {
            get => _randomEnvironmentControl.Seed;
            set
            {
                _randomEnvironmentControl.Seed = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RandomizeWaterLevels
        {
            get => _randomizeWaterLevels;
            set
            {
                _randomizeWaterLevels = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RandomizeSlotPositions
        {
            get => _randomizeSlotPositions;
            set
            {
                _randomizeSlotPositions = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RandomizeLadders
        {
            get => _randomizeLadders;
            set
            {
                _randomizeLadders = value;
                FirePropertyChanged();
            }
        }

        public uint MirroredLevelCount
        {
            get => _mirroredLevelCount;
            set
            {
                _mirroredLevelCount = value;
                FirePropertyChanged();
            }
        }

        public bool MirrorAssaultCourse
        {
            get => _mirrorAssaultCourse;
            set
            {
                _mirrorAssaultCourse = value;
                FirePropertyChanged();
            }
        }

        private bool _developmentMode;
        public bool DevelopmentMode
        {
            get => _developmentMode;
            set
            {
                _developmentMode = value;
                FirePropertyChanged();
            }
        }

        public bool DisableDemos
        {
            get => _disableDemos;
            set
            {
                _disableDemos = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass ProtectMonks
        {
            get => _protectMonks;
            set
            {
                _protectMonks = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass IsGlitchedSecrets
        {
            get => _allowGlitched;
            set
            {
                _allowGlitched = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass UseRewardRoomCameras
        {
            get => _useRewardRoomCameras;
            set
            {
                _useRewardRoomCameras = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass UseRandomSecretModels
        {
            get => _useRandomSecretModels;
            set
            {
                _useRandomSecretModels = value;
                FirePropertyChanged();
            }
        }

        public TRSecretCountMode SecretCountMode
        {
            get => _secretCountMode;
            set
            {
                _secretCountMode = value;
                FirePropertyChanged();
                FirePropertyChanged(nameof(IsCustomizedSecretModeCount));
            }
        }

        public bool IsCustomizedSecretModeCount => SecretCountMode == TRSecretCountMode.Customized;

        public uint MinSecretCount
        {
            get => _minSecretCount;
            set
            {
                _minSecretCount = value;
                FirePropertyChanged();
            }
        }

        public uint MaxSecretCount
        {
            get => _maxSecretCount;
            set
            {
                _maxSecretCount = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass DocileWillard
        {
            get => _docileWillard;
            set
            {
                _docileWillard = value;
                FirePropertyChanged();
            }
        }

        public BirdMonsterBehaviour BirdMonsterBehaviour
        {
            get => _birdMonsterBehaviour;
            set
            {
                _birdMonsterBehaviour = value;
                FirePropertyChanged();
            }
        }

        public DragonSpawnType DragonSpawnType
        {
            get => _dragonSpawnType;
            set
            {
                _dragonSpawnType = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass SwapEnemyAppearance
        {
            get => _swapEnemyAppearance;
            set
            {
                _swapEnemyAppearance = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass AllowEmptyEggs
        {
            get => _allowEmptyEggs;
            set
            {
                _allowEmptyEggs = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass HideEnemies
        {
            get => _hideEnemies;
            set
            {
                _hideEnemies = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RemoveLevelEndingLarson
        {
            get => _removeLevelEndingLarson;
            set
            {
                _removeLevelEndingLarson = value;
                FirePropertyChanged();
            }
        }

        public RandoDifficulty RandoEnemyDifficulty
        {
            get => _randoEnemyDifficulty;
            set
            {
                _randoEnemyDifficulty = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RetainMainLevelTextures
        {
            get => _retainLevelTextures;
            set
            {
                _retainLevelTextures = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RetainKeySpriteTextures
        {
            get => _retainKeySpriteTextures;
            set
            {
                _retainKeySpriteTextures = value;
                FirePropertyChanged();
            }
        }

        public BoolItemControlClass RetainSecretSpriteTextures
        {
            get => _retainSecretSpriteTextures;
            set
            {
                _retainSecretSpriteTextures = value;
                FirePropertyChanged();
            }
        }

        public uint WireframeLevelCount
        {
            get => _wireframeLevelCount;
            set
            {
                _wireframeLevelCount = value;
                FirePropertyChanged();
            }
        }

        public bool AssaultCourseWireframe
        {
            get => _assaultCourseWireframe;
            set
            {
                _assaultCourseWireframe = value;
                FirePropertyChanged();
            }
        }

        public bool UseSolidLaraWireframing
        {
            get => _useSolidLaraWireframing;
            set
            {
                _useSolidLaraWireframing = value;
                FirePropertyChanged();
            }
        }

        public bool UseSolidEnemyWireframing
        {
            get => _useSolidEnemyWireframing;
            set
            {
                _useSolidEnemyWireframing = value;
                FirePropertyChanged();
            }
        }

        public bool UseDifferentWireframeColours
        {
            get => _useDifferentWireframeColours;
            set
            {
                _useDifferentWireframeColours = value;
                FirePropertyChanged();
            }
        }

        public bool UseWireframeLadders
        {
            get => _useWireframeLadders;
            set
            {
                _useWireframeLadders = value;
                FirePropertyChanged();
            }
        }

        public bool AutoLaunchGame
        {
            get => _autoLaunchGame;
            set
            {
                _autoLaunchGame = value;
                FirePropertyChanged();
            }
        }

        public bool PuristMode
        {
            get => _puristMode;
            set
            {
                _puristMode = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> SecretBoolItemControls
        {
            get => _secretBoolItemControls;
            set
            {
                _secretBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> HealthBoolItemControls
        {
            get => _healthBoolItemControls;
            set
            {
                _healthBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> ItemBoolItemControls
        {
            get => _itemBoolItemControls;
            set
            {
                _itemBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> EnemyBoolItemControls
        {
            get => _enemyBoolItemControls;
            set
            {
                _enemyBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemIDControlClass> SelectableEnemyControls
        {
            get => _selectableEnemies;
            set
            {
                _selectableEnemies = value;
                FirePropertyChanged();
            }
        }

        public bool UseEnemyExclusions
        {
            get => _useEnemyExclusions;
            set
            {
                _useEnemyExclusions = value;
                FirePropertyChanged();
            }
        }

        public bool ShowExclusionWarnings
        {
            get => _showExclusionWarnings;
            set
            {
                _showExclusionWarnings = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> TextureBoolItemControls
        {
            get => _textureBoolItemControls;
            set
            {
                _textureBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> AudioBoolItemControls
        {
            get => _audioBoolItemControls;
            set
            {
                _audioBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> OutfitBoolItemControls
        {
            get => _outfitBoolItemControls;
            set
            {
                _outfitBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> TextBoolItemControls
        {
            get => _textBoolItemControls;
            set
            {
                _textBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> StartBoolItemControls
        {
            get => _startBoolItemControls;
            set
            {
                _startBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public List<BoolItemControlClass> EnvironmentBoolItemControls
        {
            get => _environmentBoolItemControls;
            set
            {
                _environmentBoolItemControls = value;
                FirePropertyChanged();
            }
        }

        public SpriteRandoMode SpriteRandoMode
        {
            get => _spriteRandoMode;
            set
            {
                _spriteRandoMode = value;
                FirePropertyChanged();
            }
        }
        public bool RandomizeItemSprites
        {
            get => _randomizeItemSprites;
            set
            {
                _randomizeItemSprites = value;
                FirePropertyChanged();
            }
        }
        public bool RandomizeKeyItemSprites
        {
            get => _randomizeKeyItemSprites;
            set
            {
                _randomizeKeyItemSprites = value;
                FirePropertyChanged();
            }
        }
        public bool RandomizeSecretSprites
        {
            get => _randomizeSecretSprites;
            set
            {
                _randomizeSecretSprites = value;
                FirePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private TRRandomizerController _controller;

        public ControllerOptions()
        {
            _optionRandomizer = new OptionGenerator(this);

            _levelSequencingControl = new ManagedSeedNumeric();
            _unarmedLevelsControl = new ManagedSeedNumeric();
            _ammolessLevelsControl = new ManagedSeedNumeric();
            _secretRewardsControl = new ManagedSeed();
            _healthLevelsControl = new ManagedSeedBool();
            _sunsetLevelsControl = new ManagedSeedNumeric();
            _nightLevelsControl = new ManagedSeedNumeric();
            _audioTrackControl = new ManagedSeedBool();

            _randomItemsControl = new ManagedSeedBool();
            _randomEnemiesControl = new ManagedSeedBool();
            _randomSecretsControl = new ManagedSeedBool();
            _randomTexturesControl = new ManagedSeedBool();
            _randomOutfitsControl = new ManagedSeedBool();
            _randomTextControl = new ManagedSeedBool();
            _randomStartControl = new ManagedSeedBool();
            _randomEnvironmentControl = new ManagedSeedBool();

            // Secrets
            Binding randomizeSecretsBinding = new Binding(nameof(RandomizeSecrets)) { Source = this };
            IsHardSecrets = new BoolItemControlClass()
            {
                Title = "Enable hard secrets",
                Description = "Locations classed as hard will be included in the randomization pool."
            };
            BindingOperations.SetBinding(IsHardSecrets, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);
            IsGlitchedSecrets = new BoolItemControlClass()
            {
                Title = "Enable glitched secrets",
                Description = "Locations that require glitches to reach will be included in the randomization pool."
            };
            BindingOperations.SetBinding(IsGlitchedSecrets, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);
            UseRewardRoomCameras = new BoolItemControlClass()
            {
                Title = "Enable reward room cameras",
                Description = "When picking up secrets, show a hint where the reward room for the level is located."
            };
            BindingOperations.SetBinding(UseRewardRoomCameras, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);
            UseRandomSecretModels = new BoolItemControlClass()
            {
                Title = "Use random secret types",
                Description = "If enabled, secret types will be randomized across levels; otherwise, pre-defined types will be allocated to each level."
            };
            BindingOperations.SetBinding(UseRandomSecretModels, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);

            // Items
            Binding randomizeItemsBinding = new Binding(nameof(RandomizeItems)) { Source = this };
            RandomizeItemTypes = new BoolItemControlClass
            {
                Title = "Randomize types",
                Description = "The types of standard pickups will be randomized e.g. a small medi may become a shotgun."
            };
            BindingOperations.SetBinding(RandomizeItemTypes, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
            RandomizeItemPositions = new BoolItemControlClass
            {
                Title = "Randomize positions",
                Description = "The positions of standard pickups will be randomized."
            };
            BindingOperations.SetBinding(RandomizeItemPositions, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
            IncludeKeyItems = new BoolItemControlClass()
            {
                Title = "Include key items",
                Description = "Most key item positions will be randomized. Keys will spawn before their respective locks."
            };
            BindingOperations.SetBinding(IncludeKeyItems, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
            IncludeExtraPickups = new BoolItemControlClass
            {
                Title = "Add extra pickups",
                Description = "Add more weapon, ammo and medi items to some levels for Lara to find."
            };
            BindingOperations.SetBinding(IncludeExtraPickups, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);

            // Enemies
            Binding randomizeEnemiesBinding = new Binding(nameof(RandomizeEnemies)) { Source = this };
            CrossLevelEnemies = new BoolItemControlClass()
            {
                Title = "Enable cross-level enemies",
                Description = "Allow enemy types to appear in any level."
            };
            BindingOperations.SetBinding(CrossLevelEnemies, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
            DocileWillard = new BoolItemControlClass()
            {
                Title = "Enable docile Willard",
                Description = "Willard can appear in levels other than Meteorite Cavern but will not attack Lara unless she gets too close."
            };
            BindingOperations.SetBinding(DocileWillard, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
            ProtectMonks = new BoolItemControlClass()
            {
                Title = "Avoid having to kill allies",
                Description = "Allies will not be given pickups or be assigned to end-level triggers."
            };
            BindingOperations.SetBinding(ProtectMonks, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
            SwapEnemyAppearance = new BoolItemControlClass
            {
                Title = "Swap enemy appearances",
                Description = "Allow some enemies to take on the appearance of others."
            };
            BindingOperations.SetBinding(SwapEnemyAppearance, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
            AllowEmptyEggs = new BoolItemControlClass
            {
                Title = "Allow empty Atlantean eggs",
                Description = "Allow some Atlantean eggs to hatch nothing when Lara gets close to them."
            };
            BindingOperations.SetBinding(AllowEmptyEggs, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
            HideEnemies = new BoolItemControlClass
            {
                Title = "Hide enemies until triggered",
                Description = "Most enemies will not be visible until they are triggered."
            };
            BindingOperations.SetBinding(HideEnemies, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
            RemoveLevelEndingLarson = new BoolItemControlClass
            {
                Title = "Remove level-ending Larson",
                Description = "Prevents Larson triggering the end of the level in Tomb of Qualopec."
            };
            BindingOperations.SetBinding(RemoveLevelEndingLarson, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);

            // Textures
            Binding randomizeTexturesBinding = new Binding(nameof(RandomizeTextures)) { Source = this };
            PersistTextures = new BoolItemControlClass()
            {
                Title = "Use persistent textures",
                Description = "Each unique texture will only be randomized once, rather than once per level."
            };
            BindingOperations.SetBinding(PersistTextures, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
            RandomizeWaterColour = new BoolItemControlClass()
            {
                Title = "Randomize water colour",
                Description = "Change the colour of water in each level."
            };
            BindingOperations.SetBinding(RandomizeWaterColour, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
            RetainMainLevelTextures = new BoolItemControlClass
            {
                Title = "Use original main level textures",
                Description = "Texture mapping will not apply to levels as a whole e.g. walls, rock, snow etc."
            };
            BindingOperations.SetBinding(RetainMainLevelTextures, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
            RetainKeySpriteTextures = new BoolItemControlClass()
            {
                Title = "Use original key item textures",
                Description = "Texture mapping will not apply to key items."
            };
            BindingOperations.SetBinding(RetainKeySpriteTextures, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
            RetainSecretSpriteTextures = new BoolItemControlClass
            {
                Title = "Use original secret item textures",
                Description = "Texture mapping will not apply to secrets."
            };
            BindingOperations.SetBinding(RetainSecretSpriteTextures, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);

            // Audio
            Binding randomizeAudioBinding = new Binding(nameof(RandomizeAudioTracks)) { Source = this };
            ChangeAmbientTracks = new BoolItemControlClass
            {
                Title = "Change ambient tracks",
                Description = "Change the title screen track and ambient track that plays in each level."
            };
            BindingOperations.SetBinding(ChangeAmbientTracks, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            IncludeBlankTracks = new BoolItemControlClass()
            {
                Title = "Include blank tracks",
                Description = "Applies only to the title screen and level ambience tracks."
            };
            BindingOperations.SetBinding(IncludeBlankTracks, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            ChangeTriggerTracks = new BoolItemControlClass()
            {
                Title = "Change trigger tracks",
                Description = "Change the tracks in the game that play when crossing triggers, such as the violins in Venice, danger sounds etc."
            };
            BindingOperations.SetBinding(ChangeTriggerTracks, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            SeparateSecretTracks = new BoolItemControlClass
            {
                Title = "Use separate secret tracks",
                Description = "Play a different soundtrack for every secret that is picked up, or use the same soundtrack for all secrets."
            };
            BindingOperations.SetBinding(SeparateSecretTracks, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            ChangeWeaponSFX = new BoolItemControlClass
            {
                Title = "Change weapon sound effects",
                Description = "Randomize the sound made by each weapon (applies to Lara and enemies)."
            };
            BindingOperations.SetBinding(ChangeWeaponSFX, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            ChangeCrashSFX = new BoolItemControlClass
            {
                Title = "Change crash sound effects",
                Description = "Randomize the sound made by crashes and explosions."
            };
            BindingOperations.SetBinding(ChangeCrashSFX, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            ChangeDoorSFX = new BoolItemControlClass
            {
                Title = "Change door sound effects",
                Description = "Randomize door, gate and switch sound effects."
            };
            BindingOperations.SetBinding(ChangeDoorSFX, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            ChangeEnemySFX = new BoolItemControlClass
            {
                Title = "Change enemy sound effects",
                Description = "Randomize enemy footstep, grunting, growling, breathing and death sound effects."
            };
            BindingOperations.SetBinding(ChangeEnemySFX, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            LinkCreatureSFX = new BoolItemControlClass
            {
                Title = "Link creature sound effects",
                Description = "Enforce the use of human sounds for human enemies and animal sounds for animal enemies."
            };
            BindingOperations.SetBinding(LinkCreatureSFX, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            RandomizeWibble = new BoolItemControlClass
            {
                Title = "Apply maximum pitch variance",
                Description = "Allow the engine to randomize the pitch of all sound effects and not just the defaults."
            };
            BindingOperations.SetBinding(RandomizeWibble, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);

            // Outfits
            Binding randomizeOutfitsBinding = new Binding(nameof(RandomizeOutfits)) { Source = this };
            PersistOutfits = new BoolItemControlClass()
            {
                Title = "Use persistent outfit",
                Description = "Lara's outfit will be the same throughout the entire game, when possible."
            };
            BindingOperations.SetBinding(PersistOutfits, BoolItemControlClass.IsActiveProperty, randomizeOutfitsBinding);
            RemoveRobeDagger = new BoolItemControlClass()
            {
                Title = "Remove robe dagger",
                Description = "If Lara is wearing her dressing gown before she has killed a dragon, the dagger will not appear."
            };
            BindingOperations.SetBinding(RemoveRobeDagger, BoolItemControlClass.IsActiveProperty, randomizeOutfitsBinding);
            AllowGymOutfit = new BoolItemControlClass()
            {
                Title = "Allow gym outfit swap",
                Description = "Allow Lara to wear her gym outfit on her adventures (applies only to specific levels)."
            };
            BindingOperations.SetBinding(AllowGymOutfit, BoolItemControlClass.IsActiveProperty, randomizeOutfitsBinding);

            // Text
            Binding randomizeTextBinding = new Binding(nameof(RandomizeText)) { Source = this };
            RetainKeyItemNames = new BoolItemControlClass
            {
                Title = "Use original key item names",
                Description = "The original text from the game will be used for key, pickup and puzzle items."
            };
            BindingOperations.SetBinding(RetainKeyItemNames, BoolItemControlClass.IsActiveProperty, randomizeTextBinding);
            RetainLevelNames = new BoolItemControlClass
            {
                Title = "Use original level names",
                Description = "The original text from the game will be used for level names."
            };
            BindingOperations.SetBinding(RetainLevelNames, BoolItemControlClass.IsActiveProperty, randomizeTextBinding);

            // Start positions
            Binding randomizeStartPositionBinding = new Binding(nameof(RandomizeStartPosition)) { Source = this };
            RotateStartPositionOnly = new BoolItemControlClass
            {
                Title = "Rotate Lara only",
                Description = "Don't change Lara's position, and instead change only the direction she is facing at the start."
            };
            BindingOperations.SetBinding(RotateStartPositionOnly, BoolItemControlClass.IsActiveProperty, randomizeStartPositionBinding);

            // Environment
            Binding randomizeEnvironmentBinding = new Binding(nameof(RandomizeEnvironment)) { Source = this };
            RandomizeWaterLevels = new BoolItemControlClass
            {
                Title = "Change water levels",
                Description = "Flood or drain particular rooms in each level."
            };
            BindingOperations.SetBinding(RandomizeWaterLevels, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);
            RandomizeSlotPositions = new BoolItemControlClass
            {
                Title = "Move keyholes",
                Description = "Change where keyholes, switches and puzzle slots are located in each level."
            };
            BindingOperations.SetBinding(RandomizeSlotPositions, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);
            RandomizeLadders = new BoolItemControlClass
            {
                Title = "Move ladders",
                Description = "Change where ladders are positioned in each level."
            };
            BindingOperations.SetBinding(RandomizeLadders, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);

            Binding randomizeHealthBinding = new Binding(nameof(RandomizeHealth)) { Source = this };
            DisableHealingBetweenLevels = new BoolItemControlClass
            {
                Title = "Disable healing between levels",
                Description = "Lara's health will be carried over from level to level and will not be restored."
            };
            BindingOperations.SetBinding(DisableHealingBetweenLevels, BoolItemControlClass.IsActiveProperty, randomizeHealthBinding);
            DisableMedpacks = new BoolItemControlClass
            {
                Title = "Disable medi-packs",
                Description = "Disable all med-packs throughout the game."
            };
            BindingOperations.SetBinding(DisableMedpacks, BoolItemControlClass.IsActiveProperty, randomizeHealthBinding);

            // all item controls
            SecretBoolItemControls = new List<BoolItemControlClass>()
            {
                _isHardSecrets, _allowGlitched, _useRewardRoomCameras, _useRandomSecretModels
            };
            ItemBoolItemControls = new List<BoolItemControlClass>()
            {
                _randomizeItemTypes, _randomizeItemLocations, _includeKeyItems, _includeExtraPickups
            };
            EnemyBoolItemControls = new List<BoolItemControlClass>()
            {
                _crossLevelEnemies, _docileWillard, _protectMonks, _swapEnemyAppearance, _allowEmptyEggs, _hideEnemies, _removeLevelEndingLarson
            };
            TextureBoolItemControls = new List<BoolItemControlClass>()
            {
                _persistTextures, _randomizeWaterColour, _retainLevelTextures, _retainKeySpriteTextures, _retainSecretSpriteTextures
            };
            AudioBoolItemControls = new List<BoolItemControlClass>()
            {
                _changeAmbientTracks, _includeBlankTracks, _changeTriggerTracks, _separateSecretTracks, _changeWeaponSFX,
                _changeCrashSFX, _changeEnemySFX, _changeDoorSFX, _linkCreatureSFX, _randomizeWibble
            };
            OutfitBoolItemControls = new List<BoolItemControlClass>()
            {
                _persistOutfits, _removeRobeDagger, _allowGymOutfit
            };
            TextBoolItemControls = new List<BoolItemControlClass>
            {
                _retainKeyItemNames, _retainLevelNames
            };
            StartBoolItemControls = new List<BoolItemControlClass>
            {
                _rotateStartPosition
            };
            EnvironmentBoolItemControls = new List<BoolItemControlClass>
            {
                _randomizeWaterLevels, _randomizeSlotPositions, _randomizeLadders
            };
            HealthBoolItemControls = new List<BoolItemControlClass>
            {
                _disableHealingBetweenLevels, _disableMedpacks
            };
        }

        private void AdjustAvailableOptions()
        {
            // Called after the version type has been identified, so allows for customising
            // individual settings based on what's available.
            _removeRobeDagger.IsAvailable = IsOutfitDaggerSupported;
            _retainLevelTextures.IsAvailable = IsDynamicTexturesTypeSupported;
            _persistOutfits.IsAvailable = !IsTR1;
            _allowGymOutfit.IsAvailable = IsGymOutfitTypeSupported;

            _changeAmbientTracks.IsAvailable = IsAmbientTracksTypeSupported;
            _includeBlankTracks.IsAvailable = IsAmbientTracksTypeSupported;
            _separateSecretTracks.IsAvailable = IsSecretAudioSupported;

            _changeWeaponSFX.IsAvailable = _changeCrashSFX.IsAvailable = _changeEnemySFX.IsAvailable = _linkCreatureSFX.IsAvailable = IsSFXSupported;

            _useRewardRoomCameras.IsAvailable = IsRewardRoomsTypeSupported;
            _useRandomSecretModels.IsAvailable = IsSecretModelsTypeSupported;

            _swapEnemyAppearance.IsAvailable = IsMeshSwapsTypeSupported;

            _protectMonks.IsAvailable = !IsTR1;
            _docileWillard.IsAvailable = IsTR3;

            _includeKeyItems.IsAvailable = IsKeyItemTypeSupported;
            _includeExtraPickups.IsAvailable = IsExtraPickupsTypeSupported;

            _allowGlitched.IsAvailable = IsGlitchedSecretsSupported;
            _isHardSecrets.IsAvailable = IsHardSecretsSupported;

            _retainSecretSpriteTextures.IsAvailable = IsSecretTexturesTypeSupported;
            _retainKeySpriteTextures.IsAvailable = IsKeyItemTexturesTypeSupported;
            _randomizeWaterColour.IsAvailable = IsWaterColourTypeSupported;
            _allowEmptyEggs.IsAvailable = IsAtlanteanEggBehaviourTypeSupported;
            _hideEnemies.IsAvailable = IsHiddenEnemiesTypeSupported;
            _removeLevelEndingLarson.IsAvailable = IsLarsonBehaviourTypeSupported;
        }

        public void Load(TRRandomizerController controller)
        {
            _controller = controller;

            TotalLevelCount = _controller.LevelCount;
            DefaultUnarmedLevelCount = _controller.DefaultUnarmedLevelCount;
            DefaultAmmolessLevelCount = _controller.DefaultAmmolessLevelCount;
            DefaultSunsetCount = _controller.DefaultSunsetCount;

            RandomizeLevelSequencing = _controller.RandomizeLevelSequencing;
            LevelSequencingSeed = _controller.LevelSequencingSeed;
            PlayableLevelCount = _controller.PlayableLevelCount;
            GlobeDisplay = _controller.GlobeDisplay;

            RandomizeUnarmedLevels = _controller.RandomizeUnarmedLevels;
            UnarmedLevelsSeed = _controller.UnarmedLevelsSeed;
            UnarmedLevelCount = _controller.UnarmedLevelCount;

            RandomizeAmmolessLevels = _controller.RandomizeAmmolessLevels;
            AmmolessLevelsSeed = _controller.AmmolessLevelsSeed;
            AmmolessLevelCount = _controller.AmmolessLevelCount;

            RandomizeSecretRewards = _controller.RandomizeSecretRewards;
            SecretRewardSeed = _controller.SecretRewardSeed;

            RandomizeHealth = _controller.RandomizeHealth;
            HealthSeed = _controller.HealthSeed;
            MedilessLevelCount = _controller.MedilessLevelCount;
            MinStartingHealth = _controller.MinStartingHealth;
            MaxStartingHealth = _controller.MaxStartingHealth;
            DisableHealingBetweenLevels.Value = _controller.DisableHealingBetweenLevels;
            DisableMedpacks.Value = _controller.DisableMedpacks;

            RandomizeSunsets = _controller.RandomizeSunsets;
            SunsetsSeed = _controller.SunsetsSeed;
            SunsetCount = _controller.SunsetCount;

            RandomizeNightMode = _controller.RandomizeNightMode;
            NightModeSeed = _controller.NightModeSeed;
            NightModeCount = _controller.NightModeCount;
            NightModeAssaultCourse = _controller.NightModeAssaultCourse;
            OverrideSunsets = _controller.OverrideSunsets;
            NightModeDarkness = _controller.NightModeDarkness;
            NightModeDarknessMaximum = _controller.NightModeDarknessRange;
            VfxFilterColor = _controller.VfxFilterColor;
            VfxRandomize = _controller.RandomizeVfx;
            VfxAvailColors = _controller.VfxAvailableColorChoices;
            VfxVivid = _controller.VfxVivid;
            VfxLevel = _controller.VfxLevel;
            VfxRoom = _controller.VfxRoom;
            VfxCaustics = _controller.VfxCaustics;
            VfxWave = _controller.VfxWave;

            RandomizeAudioTracks = _controller.RandomizeAudio;
            AudioTracksSeed = _controller.AudioTracksSeed;
            ChangeAmbientTracks.Value = _controller.ChangeAmbientTracks;
            IncludeBlankTracks.Value = _controller.RandomGameTracksIncludeBlank;
            ChangeTriggerTracks.Value = _controller.ChangeTriggerTracks;
            SeparateSecretTracks.Value = _controller.SeparateSecretTracks;
            ChangeWeaponSFX.Value = _controller.ChangeWeaponSFX;
            ChangeCrashSFX.Value = _controller.ChangeCrashSFX;
            ChangeEnemySFX.Value = _controller.ChangeEnemySFX;
            ChangeDoorSFX.Value = _controller.ChangeDoorSFX;
            LinkCreatureSFX.Value = _controller.LinkCreatureSFX;
            RandomizeWibble.Value = _controller.RandomizeWibble;
            UncontrolledSFXCount = _controller.UncontrolledSFXCount;
            UncontrolledSFXAssaultCourse = _controller.UncontrolledSFXAssaultCourse;

            RandomizeItems = _controller.RandomizeItems;
            ItemSeed = _controller.ItemSeed;
            IncludeKeyItems.Value = _controller.IncludeKeyItems;
            IncludeExtraPickups.Value = _controller.IncludeExtraPickups;
            RandomizeItemTypes.Value = _controller.RandomizeItemTypes;
            RandomizeItemPositions.Value = _controller.RandomizeItemPositions;
            RandoItemDifficulty = _controller.RandoItemDifficulty;

            RandomizeEnemies = _controller.RandomizeEnemies;
            EnemySeed = _controller.EnemySeed;
            CrossLevelEnemies.Value = _controller.CrossLevelEnemies;
            ProtectMonks.Value = _controller.ProtectMonks;
            DocileWillard.Value = _controller.DocileWillard;
            BirdMonsterBehaviour = _controller.BirdMonsterBehaviour;
            DragonSpawnType = _controller.DragonSpawnType;
            SwapEnemyAppearance.Value = _controller.SwapEnemyAppearance;
            AllowEmptyEggs.Value = _controller.AllowEmptyEggs;
            HideEnemies.Value = _controller.HideEnemiesUntilTriggered;
            RemoveLevelEndingLarson.Value = _controller.RemoveLevelEndingLarson;
            RandoEnemyDifficulty = _controller.RandoEnemyDifficulty;
            UseEnemyExclusions = _controller.UseEnemyExclusions;
            ShowExclusionWarnings = _controller.ShowExclusionWarnings;
            LoadEnemyExclusions();

            RandomizeSecrets = _controller.RandomizeSecrets;
            SecretSeed = _controller.SecretSeed;
            IsHardSecrets.Value = _controller.HardSecrets;
            IsGlitchedSecrets.Value = _controller.GlitchedSecrets;
            UseRewardRoomCameras.Value = _controller.UseRewardRoomCameras;
            UseRandomSecretModels.Value = _controller.UseRandomSecretModels;
            SecretCountMode = _controller.SecretCountMode;
            MaxSecretCount = _controller.MaxSecretCount;
            MinSecretCount = _controller.MinSecretCount;

            RandomizeTextures = _controller.RandomizeTextures;
            TextureSeed = _controller.TextureSeed;
            PersistTextures.Value = _controller.PersistTextures;
            RandomizeWaterColour.Value = _controller.RandomizeWaterColour;
            RetainMainLevelTextures.Value = _controller.RetainMainLevelTextures;
            RetainKeySpriteTextures.Value = _controller.RetainKeySpriteTextures;
            RetainSecretSpriteTextures.Value = _controller.RetainSecretSpriteTextures;
            WireframeLevelCount = _controller.WireframeLevelCount;
            AssaultCourseWireframe = _controller.AssaultCourseWireframe;
            UseSolidLaraWireframing = _controller.UseSolidLaraWireframing;
            UseSolidEnemyWireframing = _controller.UseSolidEnemyWireframing;
            UseDifferentWireframeColours = _controller.UseDifferentWireframeColours;
            UseWireframeLadders = _controller.UseWireframeLadders;

            RandomizeOutfits = _controller.RandomizeOutfits;
            OutfitSeed = _controller.OutfitSeed;
            PersistOutfits.Value = _controller.PersistOutfits;
            RemoveRobeDagger.Value = _controller.RemoveRobeDagger;
            AllowGymOutfit.Value = _controller.AllowGymOutfit;
            HaircutLevelCount = _controller.HaircutLevelCount;
            AssaultCourseHaircut = _controller.AssaultCourseHaircut;
            InvisibleLevelCount = _controller.InvisibleLevelCount;
            AssaultCourseInvisible = _controller.AssaultCourseInvisible;

            RandomizeText = _controller.RandomizeGameStrings;
            TextSeed = _controller.GameStringsSeed;
            AvailableLanguages = _controller.AvailableGameStringLanguages;
            GameStringLanguage = _controller.GameStringLanguage;
            RetainKeyItemNames.Value = _controller.RetainKeyItemNames;
            RetainLevelNames.Value = _controller.RetainLevelNames;

            RandomizeStartPosition = _controller.RandomizeStartPosition;
            StartPositionSeed = _controller.StartPositionSeed;
            RotateStartPositionOnly.Value = _controller.RotateStartPositionOnly;

            RandomizeEnvironment = _controller.RandomizeEnvironment;
            EnvironmentSeed = _controller.EnvironmentSeed;
            RandomizeWaterLevels.Value = _controller.RandomizeWaterLevels;
            RandomizeSlotPositions.Value = _controller.RandomizeSlotPositions;
            RandomizeLadders.Value = _controller.RandomizeLadders;
            MirroredLevelCount = _controller.MirroredLevelCount;
            MirrorAssaultCourse = _controller.MirrorAssaultCourse;

            DevelopmentMode = _controller.DevelopmentMode;
            DisableDemos = _controller.DisableDemos;
            AutoLaunchGame = _controller.AutoLaunchGame;
            PuristMode = _controller.PuristMode;
            UseRecommendedCommunitySettings = _controller.UseRecommendedCommunitySettings;

            SpriteRandoMode = _controller.SpriteRandoMode;
            RandomizeItemSprites = _controller.RandomizeItemSprites;
            RandomizeKeyItemSprites = _controller.RandomizeKeyItemSprites;
            RandomizeSecretSprites = _controller.RandomizeSecretSprites;

            if (IsTR1Main)
            {
                EnableGameModes = _controller.EnableGameModes;
                EnableSaveCrystals = _controller.EnableSaveCrystals;
                DemoDelay = _controller.DemoDelay;
                DrawDistanceFade = _controller.DrawDistanceFade;
                DrawDistanceMax = _controller.DrawDistanceMax;
                WaterColor = _controller.WaterColor;
                DisableMagnums = _controller.DisableMagnums;
                DisableUzis = _controller.DisableUzis;
                DisableShotgun = _controller.DisableShotgun;
                EnableDeathsCounter = _controller.EnableDeathsCounter;
                EnableEnemyHealthbar = _controller.EnableEnemyHealthbar;
                EnableEnhancedLook = _controller.EnableEnhancedLook;
                EnableShotgunFlash = _controller.EnableShotgunFlash;
                FixShotgunTargeting = _controller.FixShotgunTargeting;
                EnableNumericKeys = _controller.EnableNumericKeys;
                EnableTr3Sidesteps = _controller.EnableTr3Sidesteps;
                EnableCheats = _controller.EnableCheats;
                EnableDetailedStats = _controller.EnableDetailedStats;
                EnableCompassStats = _controller.EnableCompassStats;
                EnableTotalStats = _controller.EnableTotalStats;
                EnableTimerInInventory = _controller.EnableTimerInInventory;
                EnableSmoothBars = _controller.EnableSmoothBars;
                EnableFadeEffects = _controller.EnableFadeEffects;
                MenuStyle = _controller.MenuStyle;
                HealthbarShowingMode = _controller.HealthbarShowingMode;
                HealthbarLocation = _controller.HealthbarLocation;
                HealthbarColor = _controller.HealthbarColor;
                AirbarShowingMode = _controller.AirbarShowingMode;
                AirbarLocation = _controller.AirbarLocation;
                AirbarColor = _controller.AirbarColor;
                EnemyHealthbarLocation = _controller.EnemyHealthbarLocation;
                EnemyHealthbarColor = _controller.EnemyHealthbarColor;
                FixTihocanSecretSound = _controller.FixTihocanSecretSound;
                FixPyramidSecretTrigger = _controller.FixPyramidSecretTrigger;
                FixSecretsKillingMusic = _controller.FixSecretsKillingMusic;
                FixDescendingGlitch = _controller.FixDescendingGlitch;
                FixWallJumpGlitch = _controller.FixWallJumpGlitch;
                FixBridgeCollision = _controller.FixBridgeCollision;
                FixQwopGlitch = _controller.FixQwopGlitch;
                FixAlligatorAi = _controller.FixAlligatorAi;
                ChangePierreSpawn = _controller.ChangePierreSpawn;
                FovValue = _controller.FovValue;
                FovVertical = _controller.FovVertical;
                EnableFmv = _controller.EnableFmv;
                EnableCine = _controller.EnableCine;
                EnableMusicInMenu = _controller.EnableMusicInMenu;
                EnableMusicInInventory = _controller.EnableMusicInInventory;
                DisableTRexCollision = _controller.DisableTRexCollision;
                AnisotropyFilter = _controller.AnisotropyFilter;
                ResolutionWidth = _controller.ResolutionWidth;
                ResolutionHeight = _controller.ResolutionHeight;
                EnableRoundShadow = _controller.EnableRoundShadow;
                Enable3dPickups = _controller.Enable3dPickups;
                ScreenshotFormat = _controller.ScreenshotFormat;
                WalkToItems = _controller.WalkToItems;
                MaximumSaveSlots = _controller.MaximumSaveSlots;
                RevertToPistols = _controller.RevertToPistols;
                EnableEnhancedSaves = _controller.EnableEnhancedSaves;
                EnablePitchedSounds = _controller.EnablePitchedSounds;
            }

            FireSupportPropertiesChanged();
        }

        public void LoadEnemyExclusions()
        {
            SelectableEnemyControls = new List<BoolItemIDControlClass>();

            // Add exclusions based on priority (i.e. order) followed by the remaining included controls
            _controller.ExcludedEnemies.ForEach(e => SelectableEnemyControls.Add(new BoolItemIDControlClass
            {
                ID = e,
                Title = _controller.ExcludableEnemies[e],
                Value = true
            }));

            _controller.IncludedEnemies.ForEach(e => SelectableEnemyControls.Add(new BoolItemIDControlClass
            {
                ID = e,
                Title = _controller.ExcludableEnemies[e],
                Value = false
            }));
        }

        public void RandomizeActiveSeeds()
        {
            _optionRandomizer.RandomizeActiveSeeds();
        }

        public void RandomizeActiveOptions()
        {
            _optionRandomizer.RandomizeActiveOptions();
        }

        public void SetGlobalSeed(int seed)
        {
            _optionRandomizer.SetSeeds(seed);
        }

        public void Save()
        {
            _controller.RandomizeLevelSequencing = RandomizeLevelSequencing;
            _controller.LevelSequencingSeed = LevelSequencingSeed;

            // While this can be separated into its own option, for now it's combined with sequencing
            _controller.RandomizePlayableLevels = RandomizeLevelSequencing;
            _controller.PlayableLevelsSeed = LevelSequencingSeed;
            _controller.PlayableLevelCount = PlayableLevelCount;
            _controller.GlobeDisplay = GlobeDisplay;

            _controller.RandomizePlayableLevels = RandomizeLevelSequencing;
            _controller.PlayableLevelsSeed = LevelSequencingSeed;
            _controller.PlayableLevelCount = PlayableLevelCount;

            _controller.RandomizeUnarmedLevels = RandomizeUnarmedLevels;
            _controller.UnarmedLevelsSeed = UnarmedLevelsSeed;
            _controller.UnarmedLevelCount = UnarmedLevelCount;

            _controller.RandomizeAmmolessLevels = RandomizeAmmolessLevels;
            _controller.AmmolessLevelsSeed = AmmolessLevelsSeed;
            _controller.AmmolessLevelCount = AmmolessLevelCount;

            _controller.RandomizeSecretRewards = RandomizeSecretRewards;
            _controller.SecretRewardSeed = SecretRewardSeed;

            _controller.RandomizeHealth = RandomizeHealth;
            _controller.HealthSeed = HealthSeed;
            _controller.MedilessLevelCount = MedilessLevelCount;
            _controller.MinStartingHealth = MinStartingHealth;
            _controller.MaxStartingHealth = MaxStartingHealth;
            _controller.DisableHealingBetweenLevels = DisableHealingBetweenLevels.Value;
            _controller.DisableMedpacks = DisableMedpacks.Value;

            _controller.RandomizeSunsets = RandomizeSunsets;
            _controller.SunsetsSeed = SunsetsSeed;
            _controller.SunsetCount = SunsetCount;

            _controller.RandomizeNightMode = RandomizeNightMode;
            _controller.NightModeSeed = NightModeSeed;
            _controller.NightModeCount = NightModeCount;
            _controller.NightModeAssaultCourse = NightModeAssaultCourse;
            _controller.OverrideSunsets = OverrideSunsets;
            _controller.NightModeDarkness = NightModeDarkness;
            _controller.VfxFilterColor = VfxFilterColor;
            _controller.RandomizeVfx = VfxRandomize;
            _controller.VfxVivid = VfxVivid;
            _controller.VfxLevel = VfxLevel;
            _controller.VfxRoom = VfxRoom;
            _controller.VfxCaustics = VfxCaustics;
            _controller.VfxWave = VfxWave;

            _controller.RandomizeAudio = RandomizeAudioTracks;
            _controller.ChangeAmbientTracks = ChangeAmbientTracks.Value;
            _controller.AudioTracksSeed = AudioTracksSeed;
            _controller.RandomGameTracksIncludeBlank = IncludeBlankTracks.Value;
            _controller.ChangeTriggerTracks = ChangeTriggerTracks.Value;
            _controller.SeparateSecretTracks = SeparateSecretTracks.Value;
            _controller.ChangeWeaponSFX = ChangeWeaponSFX.Value;
            _controller.ChangeCrashSFX = ChangeCrashSFX.Value;
            _controller.ChangeEnemySFX = ChangeEnemySFX.Value;
            _controller.ChangeDoorSFX = ChangeDoorSFX.Value;
            _controller.LinkCreatureSFX = LinkCreatureSFX.Value;
            _controller.RandomizeWibble = RandomizeWibble.Value;
            _controller.UncontrolledSFXCount = UncontrolledSFXCount;
            _controller.UncontrolledSFXAssaultCourse = UncontrolledSFXAssaultCourse;

            _controller.RandomizeItems = RandomizeItems;
            _controller.ItemSeed = ItemSeed;
            _controller.IncludeKeyItems = IncludeKeyItems.Value;
            _controller.IncludeExtraPickups = IncludeExtraPickups.Value;
            _controller.RandomizeItemTypes = RandomizeItemTypes.Value;
            _controller.RandomizeItemPositions = RandomizeItemPositions.Value;
            _controller.RandoItemDifficulty = RandoItemDifficulty;

            _controller.RandomizeEnemies = RandomizeEnemies;
            _controller.EnemySeed = EnemySeed;
            _controller.CrossLevelEnemies = CrossLevelEnemies.Value;
            _controller.ProtectMonks = ProtectMonks.Value;
            _controller.DocileWillard = DocileWillard.Value;
            _controller.BirdMonsterBehaviour = BirdMonsterBehaviour;
            _controller.DragonSpawnType = DragonSpawnType;
            _controller.SwapEnemyAppearance = SwapEnemyAppearance.Value;
            _controller.AllowEmptyEggs = AllowEmptyEggs.Value;
            _controller.HideEnemiesUntilTriggered = HideEnemies.Value;
            _controller.RemoveLevelEndingLarson = RemoveLevelEndingLarson.Value;
            _controller.RandoEnemyDifficulty = RandoEnemyDifficulty;
            _controller.UseEnemyExclusions = UseEnemyExclusions;
            _controller.ShowExclusionWarnings = ShowExclusionWarnings;

            List<short> excludedEnemies = new List<short>();
            SelectableEnemyControls.FindAll(c => c.Value).ForEach(c => excludedEnemies.Add((short)c.ID));
            _controller.ExcludedEnemies = excludedEnemies;

            _controller.RandomizeSecrets = RandomizeSecrets;
            _controller.SecretSeed = SecretSeed;
            _controller.HardSecrets = IsHardSecrets.Value;
            _controller.GlitchedSecrets = IsGlitchedSecrets.Value;
            _controller.UseRewardRoomCameras = UseRewardRoomCameras.Value;
            _controller.UseRandomSecretModels = UseRandomSecretModels.Value;
            _controller.SecretCountMode = SecretCountMode;
            _controller.MinSecretCount = MinSecretCount;
            _controller.MaxSecretCount = MaxSecretCount;

            _controller.RandomizeTextures = RandomizeTextures;
            _controller.TextureSeed = TextureSeed;
            _controller.PersistTextures = PersistTextures.Value;
            _controller.RandomizeWaterColour = RandomizeWaterColour.Value;
            _controller.RetainMainLevelTextures = RetainMainLevelTextures.Value;
            _controller.RetainKeySpriteTextures = RetainKeySpriteTextures.Value;
            _controller.RetainSecretSpriteTextures = RetainSecretSpriteTextures.Value;
            _controller.WireframeLevelCount = WireframeLevelCount;
            _controller.AssaultCourseWireframe = AssaultCourseWireframe;
            _controller.UseSolidLaraWireframing = UseSolidLaraWireframing;
            _controller.UseSolidEnemyWireframing = UseSolidEnemyWireframing;
            _controller.UseDifferentWireframeColours = UseDifferentWireframeColours;
            _controller.UseWireframeLadders = UseWireframeLadders;

            _controller.RandomizeOutfits = RandomizeOutfits;
            _controller.OutfitSeed = OutfitSeed;
            _controller.PersistOutfits = PersistOutfits.Value;
            _controller.RemoveRobeDagger = RemoveRobeDagger.Value;
            _controller.AllowGymOutfit = AllowGymOutfit.Value;
            _controller.HaircutLevelCount = HaircutLevelCount;
            _controller.AssaultCourseHaircut = AssaultCourseHaircut;
            _controller.InvisibleLevelCount = InvisibleLevelCount;
            _controller.AssaultCourseInvisible = AssaultCourseInvisible;

            _controller.RandomizeGameStrings = RandomizeText;
            _controller.GameStringsSeed = TextSeed;
            _controller.GameStringLanguage = GameStringLanguage;
            _controller.RetainKeyItemNames = RetainKeyItemNames.Value;
            _controller.RetainLevelNames = RetainLevelNames.Value;

            _controller.RandomizeStartPosition = RandomizeStartPosition;
            _controller.StartPositionSeed = StartPositionSeed;
            _controller.RotateStartPositionOnly = RotateStartPositionOnly.Value;

            _controller.RandomizeEnvironment = RandomizeEnvironment;
            _controller.EnvironmentSeed = EnvironmentSeed;
            _controller.RandomizeWaterLevels = RandomizeWaterLevels.Value;
            _controller.RandomizeSlotPositions = RandomizeSlotPositions.Value;
            _controller.RandomizeLadders = RandomizeLadders.Value;
            _controller.MirroredLevelCount = MirroredLevelCount;
            _controller.MirrorAssaultCourse = MirrorAssaultCourse;

            _controller.DevelopmentMode = DevelopmentMode;
            _controller.DisableDemos = DisableDemos;
            _controller.AutoLaunchGame = AutoLaunchGame;
            _controller.PuristMode = PuristMode;
            _controller.UseRecommendedCommunitySettings = UseRecommendedCommunitySettings;

            _controller.SpriteRandoMode = SpriteRandoMode;
            _controller.RandomizeItemSprites = RandomizeItemSprites;
            _controller.RandomizeKeyItemSprites = RandomizeKeyItemSprites;
            _controller.RandomizeSecretSprites = RandomizeSecretSprites;

            if (IsTR1Main)
            {
                _controller.EnableGameModes = EnableGameModes;
                _controller.EnableSaveCrystals = EnableSaveCrystals;
                _controller.DemoDelay = DemoDelay;
                _controller.DrawDistanceFade = DrawDistanceFade;
                _controller.DrawDistanceMax = DrawDistanceMax;
                _controller.WaterColor = WaterColor;
                _controller.DisableMagnums = DisableMagnums;
                _controller.DisableUzis = DisableUzis;
                _controller.DisableShotgun = DisableShotgun;
                _controller.EnableDeathsCounter = EnableDeathsCounter;
                _controller.EnableEnemyHealthbar = EnableEnemyHealthbar;
                _controller.EnableEnhancedLook = EnableEnhancedLook;
                _controller.EnableShotgunFlash = EnableShotgunFlash;
                _controller.FixShotgunTargeting = FixShotgunTargeting;
                _controller.EnableNumericKeys = EnableNumericKeys;
                _controller.EnableTr3Sidesteps = EnableTr3Sidesteps;
                _controller.EnableCheats = EnableCheats;
                _controller.EnableDetailedStats = EnableDetailedStats;
                _controller.EnableCompassStats = EnableCompassStats;
                _controller.EnableTotalStats = EnableTotalStats;
                _controller.EnableTimerInInventory = EnableTimerInInventory;
                _controller.EnableSmoothBars = EnableSmoothBars;
                _controller.EnableFadeEffects = EnableFadeEffects;
                _controller.MenuStyle = MenuStyle;
                _controller.HealthbarShowingMode = HealthbarShowingMode;
                _controller.HealthbarLocation = HealthbarLocation;
                _controller.HealthbarColor = HealthbarColor;
                _controller.AirbarShowingMode = AirbarShowingMode;
                _controller.AirbarLocation = AirbarLocation;
                _controller.AirbarColor = AirbarColor;
                _controller.EnemyHealthbarLocation = EnemyHealthbarLocation;
                _controller.EnemyHealthbarColor = EnemyHealthbarColor;
                _controller.FixTihocanSecretSound = FixTihocanSecretSound;
                _controller.FixPyramidSecretTrigger = FixPyramidSecretTrigger;
                _controller.FixSecretsKillingMusic = FixSecretsKillingMusic;
                _controller.FixDescendingGlitch = FixDescendingGlitch;
                _controller.FixWallJumpGlitch = FixWallJumpGlitch;
                _controller.FixBridgeCollision = FixBridgeCollision;
                _controller.FixQwopGlitch = FixQwopGlitch;
                _controller.FixAlligatorAi = FixAlligatorAi;
                _controller.ChangePierreSpawn = ChangePierreSpawn;
                _controller.FovValue = FovValue;
                _controller.FovVertical = FovVertical;
                _controller.EnableFmv = EnableFmv;
                _controller.EnableCine = EnableCine;
                _controller.EnableMusicInMenu = EnableMusicInMenu;
                _controller.EnableMusicInInventory = EnableMusicInInventory;
                _controller.DisableTRexCollision = DisableTRexCollision;
                _controller.AnisotropyFilter = AnisotropyFilter;
                _controller.ResolutionWidth = ResolutionWidth;
                _controller.ResolutionHeight = ResolutionHeight;
                _controller.EnableRoundShadow = EnableRoundShadow;
                _controller.Enable3dPickups = Enable3dPickups;
                _controller.ScreenshotFormat = ScreenshotFormat;
                _controller.WalkToItems = WalkToItems;
                _controller.MaximumSaveSlots = MaximumSaveSlots;
                _controller.RevertToPistols = RevertToPistols;
                _controller.EnableEnhancedSaves = EnableEnhancedSaves;
                _controller.EnablePitchedSounds = EnablePitchedSounds;
            }
        }

        public void Unload()
        {
            _controller = null;
        }

        #region Randomizer Type Support
        private static readonly string _supportPropertyFormat = "Is{0}TypeSupported";

        public bool IsTR1 => _controller != null && _controller.IsTR1;
        public bool IsTR1Main => IsTR1 && _controller.IsCommunityPatch;
        public bool IsTR2 => _controller != null && _controller.IsTR2;
        public bool IsTR3 => _controller != null && _controller.IsTR3;
        public bool IsLevelSequenceTypeSupported => IsRandomizationSupported(TRRandomizerType.LevelSequence);
        public bool IsGlobeDisplayTypeSupported => IsRandomizationSupported(TRRandomizerType.GlobeDisplay);
        public bool IsUnarmedTypeSupported => IsRandomizationSupported(TRRandomizerType.Unarmed);
        public bool IsAmmolessTypeSupported => IsRandomizationSupported(TRRandomizerType.Ammoless);
        public bool IsMedilessTypeSupported => IsRandomizationSupported(TRRandomizerType.Mediless);
        public bool IsSunsetTypeSupported => IsRandomizationSupported(TRRandomizerType.Sunset);
        public bool IsHealthTypeSupported => IsRandomizationSupported(TRRandomizerType.Health);
        public bool IsNightModeTypeSupported => IsRandomizationSupported(TRRandomizerType.NightMode);
        public bool IsSecretTypeSupported => IsRandomizationSupported(TRRandomizerType.Secret);
        public bool IsGlitchedSecretsSupported => IsRandomizationSupported(TRRandomizerType.GlitchedSecrets);
        public bool IsHardSecretsSupported => IsRandomizationSupported(TRRandomizerType.HardSecrets);
        public bool IsRewardRoomsTypeSupported => IsRandomizationSupported(TRRandomizerType.RewardRooms);
        public bool IsSecretModelsTypeSupported => IsRandomizationSupported(TRRandomizerType.SecretModels);
        public bool IsSecretCountTypeSupported => IsRandomizationSupported(TRRandomizerType.SecretCount);
        public bool IsSecretRewardTypeSupported => IsRandomizationSupported(TRRandomizerType.SecretReward);
        public bool IsItemTypeSupported => IsRandomizationSupported(TRRandomizerType.Item);
        public bool IsKeyItemTypeSupported => IsRandomizationSupported(TRRandomizerType.KeyItems);
        public bool IsExtraPickupsTypeSupported => IsRandomizationSupported(TRRandomizerType.ExtraPickups);
        public bool IsEnemyTypeSupported => IsRandomizationSupported(TRRandomizerType.Enemy);
        public bool IsTextureTypeSupported => IsRandomizationSupported(TRRandomizerType.Texture);
        public bool IsStartPositionTypeSupported => IsRandomizationSupported(TRRandomizerType.StartPosition);
        public bool IsAudioTypeSupported => IsRandomizationSupported(TRRandomizerType.Audio);
        public bool IsAmbientTracksTypeSupported => IsRandomizationSupported(TRRandomizerType.AmbientTracks);
        public bool IsSecretAudioSupported => IsRandomizationSupported(TRRandomizerType.SecretAudio);
        public bool IsSFXSupported => IsRandomizationSupported(TRRandomizerType.SFX);
        public bool IsVFXTypeSupported => IsRandomizationSupported(TRRandomizerType.VFX);
        public bool IsOutfitTypeSupported => IsRandomizationSupported(TRRandomizerType.Outfit);
        public bool IsGymOutfitTypeSupported => IsRandomizationSupported(TRRandomizerType.GymOutfit);
        public bool IsBraidTypeSupported => IsRandomizationSupported(TRRandomizerType.Braid);
        public bool IsOutfitDaggerSupported => IsRandomizationSupported(TRRandomizerType.OutfitDagger);
        public bool IsDynamicTexturesTypeSupported => IsRandomizationSupported(TRRandomizerType.DynamicTextures);
        public bool IsMeshSwapsTypeSupported => IsRandomizationSupported(TRRandomizerType.MeshSwaps);
        public bool IsTextTypeSupported => IsRandomizationSupported(TRRandomizerType.Text);
        public bool IsEnvironmentTypeSupported => IsRandomizationSupported(TRRandomizerType.Environment);
        public bool IsLaddersTypeSupported => IsRandomizationSupported(TRRandomizerType.Ladders);
        public bool IsWeatherTypeSupported => IsRandomizationSupported(TRRandomizerType.Weather);
        public bool IsBirdMonsterBehaviourTypeSupported => IsRandomizationSupported(TRRandomizerType.BirdMonsterBehaviour);
        public bool IsDragonSpawnTypeSupported => IsRandomizationSupported(TRRandomizerType.DragonSpawn);
        public bool IsSecretTexturesTypeSupported => IsRandomizationSupported(TRRandomizerType.SecretTextures);
        public bool IsKeyItemTexturesTypeSupported => IsRandomizationSupported(TRRandomizerType.KeyItemTextures);
        public bool IsWaterColourTypeSupported => IsRandomizationSupported(TRRandomizerType.WaterColour);
        public bool IsAtlanteanEggBehaviourTypeSupported => IsRandomizationSupported(TRRandomizerType.AtlanteanEggBehaviour);
        public bool IsHiddenEnemiesTypeSupported => IsRandomizationSupported(TRRandomizerType.HiddenEnemies);
        public bool IsLarsonBehaviourTypeSupported => IsRandomizationSupported(TRRandomizerType.LarsonBehaviour);
        public bool IsDisableDemosTypeSupported => IsRandomizationSupported(TRRandomizerType.DisableDemos);
        public bool IsItemSpriteTypeSupported => IsRandomizationSupported(TRRandomizerType.ItemSprite);

        private bool IsRandomizationSupported(TRRandomizerType randomizerType)
        {
            return _controller != null && _controller.IsRandomizationSupported(randomizerType);
        }

        private void FireSupportPropertiesChanged()
        {
            IEnumerable<TRRandomizerType> types = Enum.GetValues(typeof(TRRandomizerType)).Cast<TRRandomizerType>();
            foreach (TRRandomizerType type in types)
            {
                FirePropertyChanged(string.Format(_supportPropertyFormat, type.ToString()));
            }

            FirePropertyChanged(nameof(IsTR1));
            FirePropertyChanged(nameof(IsTR2));
            FirePropertyChanged(nameof(IsTR3));

            FirePropertyChanged(nameof(IsTR1Main));

            AdjustAvailableOptions();
        }

        public void SetAllRandomizationsEnabled(bool enabled)
        {
            if (IsLevelSequenceTypeSupported)
            {
                RandomizeLevelSequencing = enabled;
            }
            if (IsUnarmedTypeSupported)
            {
                RandomizeUnarmedLevels = enabled;
            }
            if (IsAmmolessTypeSupported)
            {
                RandomizeAmmolessLevels = enabled;
            }
            if (IsHealthTypeSupported)
            {
                RandomizeHealth = enabled;
            }
            if (IsSunsetTypeSupported)
            {
                RandomizeSunsets = enabled;
            }
            if (IsNightModeTypeSupported)
            {
                RandomizeNightMode = enabled;
            }
            if (IsSecretTypeSupported)
            {
                RandomizeSecrets = enabled;
            }
            if (IsSecretRewardTypeSupported)
            {
                RandomizeSecretRewards = enabled;
            }
            if (IsItemTypeSupported)
            {
                RandomizeItems = enabled;
            }
            if (IsEnemyTypeSupported)
            {
                RandomizeEnemies = enabled;
            }
            if (IsTextureTypeSupported)
            {
                RandomizeTextures = enabled;
            }
            if (IsStartPositionTypeSupported)
            {
                RandomizeStartPosition = enabled;
            }
            if (IsAudioTypeSupported)
            {
                RandomizeAudioTracks = enabled;
            }
            if (IsOutfitTypeSupported)
            {
                RandomizeOutfits = enabled;
            }
            if (IsTextTypeSupported)
            {
                RandomizeText = enabled;
            }
            if (IsEnvironmentTypeSupported)
            {
                RandomizeEnvironment = enabled;
            }
        }

        public bool AllRandomizationsEnabled()
        {
            bool result = true;

            if (IsLevelSequenceTypeSupported)
            {
                result &= RandomizeLevelSequencing;
            }
            if (IsUnarmedTypeSupported)
            {
                result &= RandomizeUnarmedLevels;
            }
            if (IsAmmolessTypeSupported)
            {
                result &= RandomizeAmmolessLevels;
            }
            if (IsHealthTypeSupported)
            {
                result &= RandomizeHealth;
            }
            if (IsSunsetTypeSupported)
            {
                result &= RandomizeSunsets;
            }
            if (IsNightModeTypeSupported)
            {
                result &= RandomizeNightMode;
            }
            if (IsSecretTypeSupported)
            {
                result &= RandomizeSecrets;
            }
            if (IsSecretRewardTypeSupported)
            {
                result &= RandomizeSecretRewards;
            }
            if (IsItemTypeSupported)
            {
                result &= RandomizeItems;
            }
            if (IsEnemyTypeSupported)
            {
                result &= RandomizeEnemies;
            }
            if (IsTextureTypeSupported)
            {
                result &= RandomizeTextures;
            }
            if (IsStartPositionTypeSupported)
            {
                result &= RandomizeStartPosition;
            }
            if (IsAudioTypeSupported)
            {
                result &= RandomizeAudioTracks;
            }
            if (IsOutfitTypeSupported)
            {
                result &= RandomizeOutfits;
            }
            if (IsTextTypeSupported)
            {
                result &= RandomizeText;
            }
            if (IsEnvironmentTypeSupported)
            {
                result &= RandomizeEnvironment;
            }

            return result;
        }
        #endregion
    }
}
