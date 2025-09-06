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
using TRRandomizerCore.Textures;

namespace TRRandomizerView.Model;

public class ControllerOptions : INotifyPropertyChanged
{
    public static readonly int MaxSeedValue = 1000000000;

    private readonly OptionGenerator _optionRandomizer;

    private readonly ManagedSeed _secretRewardsControl;
    private readonly ManagedSeedNumeric _unarmedLevelsControl, _ammolessLevelsControl, _sunsetLevelsControl, _nightLevelsControl;
    private readonly ManagedSeedBool _gameModeControl, _audioTrackControl, _healthLevelsControl, _weatherControl;

    private readonly ManagedSeedBool _randomSecretsControl, _randomItemsControl, _randomEnemiesControl, _randomTexturesControl, _randomOutfitsControl, _randomTextControl, _randomStartControl, _randomEnvironmentControl;

    private GameMode _gameMode;
    private GameMode[] _gameModes;
    private bool _addReturnPaths, _fixOGBugs, _disableDemos, _autoLaunchGame;

    private BoolItemControlClass _randomizeLevelSequencing;
    private BoolItemControlClass _isHardSecrets, _allowGlitched, _guaranteeSecrets, _useRandomSecretModels, _enableUWCornerSecrets;
    private TRSecretRewardMode[] _secretRewardModes;
    private TRSecretRewardMode _secretRewardMode;
    private TRSecretCountMode[] _secretCountModes;
    private TRSecretCountMode _secretCountMode;
    private bool _useRewardRoomCameras;
    private uint _minSecretCount, _maxSecretCount;
    private BoolItemControlClass _includeKeyItems, _randomizeVehicles, _allowReturnPathLocations, _includeExtraPickups, _randomizeItemTypes, _randomizeItemLocations, _allowEnemyKeyDrops, _maintainKeyContinuity, _oneItemDifficulty;
    private BoolItemControlClass _crossLevelEnemies, _protectMonks, _docileWillard, _swapEnemyAppearance, _allowEmptyEggs, _hideEnemies, _replaceRequiredEnemies, _giveUnarmedItems, _relocateAwkwardEnemies, _hideDeadTrexes, _unrestrictedEnemyDifficulty;
    private BoolItemControlClass _persistTextures, _randomizeWaterColour, _retainLevelTextures, _retainKeySpriteTextures, _retainSecretSpriteTextures, _retainEnemyTextures, _retainLaraTextures;
    private BoolItemControlClass _changeAmbientTracks, _includeBlankTracks, _changeTriggerTracks, _separateSecretTracks, _changeWeaponSFX, _changeCrashSFX, _changeEnemySFX, _changeDoorSFX, _linkCreatureSFX, _randomizeWibble;
    private BoolItemControlClass _persistOutfits, _removeRobeDagger, _allowGymOutfit;
    private BoolItemControlClass _retainKeyItemNames, _retainLevelNames;
    private BoolItemControlClass _rotateStartPosition;
    private BoolItemControlClass _randomizeWaterLevels, _randomizeSlotPositions, _randomizeLadders, _randomizeTraps, _randomizeChallengeRooms, _hardEnvironmentMode, _blockShortcuts;
    private BoolItemControlClass _disableHealingBetweenLevels, _disableMedpacks;
    private BoolItemControlClass _rainyAssaultCourse, _snowyAssaultCourse, _coldAssaultCourse;
    private uint _playableLevelCount;
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
    private bool _useSolidInteractableWireframing;
    private bool _useDifferentWireframeColours;
    private bool _useWireframeLadders, _showWireframeTriggers, _showWireframeTriggerColours;
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
    private uint _rainLevelCount, _snowLevelCount, _coldLevelCount;
    private bool _useSecretPack;
    private string _secretPack;
    private string[] _availableSecretPacks;

    private bool _useEnemyClones;
    private uint _enemyMultiplier;
    private uint _maxEnemyMultiplier;
    private bool _cloneOriginalEnemies;
    private bool _useKillableClonePierres;

    private List<BoolItemControlClass> _gameOrderBoolItemControls, _secretBoolItemControls, _itemBoolItemControls, _enemyBoolItemControls, _textureBoolItemControls, _audioBoolItemControls, _outfitBoolItemControls, _textBoolItemControls, _startBoolItemControls, _environmentBoolItemControls, _healthBoolItemControls, _weatherBoolItemControls;
    private List<BoolItemIDControlClass> _selectableEnemies;
    private bool _useEnemyExclusions, _showExclusionWarnings;

    private ItemRange[] _itemRanges;
    private ItemRange _keyItemRange;

    private GlobeDisplayOption[] _globeDisplayOptions;
    private GlobeDisplayOption _globeDisplayOption;

    private BirdMonsterBehaviour[] _birdMonsterBehaviours;
    private BirdMonsterBehaviour _birdMonsterBehaviour;

    private DragonSpawnType[] _dragonSpawnTypes;
    private DragonSpawnType _dragonSpawnType;

    private Language[] _availableLanguages;
    private Language _gameStringLanguage;

    private int _maximumLevelCount;

    private uint _minStartingHealth, _maxStartingHealth, _medilessLevelCount;

    private SpriteRandoMode[] _spriteRandoModes;
    private SpriteRandoMode _spriteRandoMode;
    private bool _randomizeItemSprites, _randomizeKeyItemSprites, _randomizeSecretSprites;

    private ItemMode _itemMode;
    private ItemMode[] _itemModes;

    private WeaponDifficulty _weaponDifficulty;
    private WeaponDifficulty[] _weaponDifficulties;

    private BoolItemControlClass _matchTextureTypes, _matchTextureItems;
    private TextureMode[] _textureModes;
    private TextureMode _textureMode;
    private bool _filterSourceTextures;
    private List<BoolItemIDControlClass> _sourceTextureAreas;

    public int TotalLevelCount
    {
        get => _controller?.GetTotalLevelCount(RandomizeGameMode ? _gameMode : GameMode.Normal) ?? 0;
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
        get => _controller?.GetDefaultUnarmedLevelCount(_gameMode) ?? 0;
    }

    public int DefaultAmmolessLevelCount
    {
        get => _controller?.GetDefaultAmmolessLevelCount(_gameMode) ?? 0;
    }

    public int DefaultSunsetCount
    {
        get => _controller?.GetDefaultSunsetLevelCount(_gameMode) ?? 0;
    }

    private void UpdateMaximumLevelCount()
    {
        MaximumLevelCount = RandomizeGameMode ? (int)PlayableLevelCount : TotalLevelCount;
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
        RainLevelCount = (uint)Math.Min(RainLevelCount, MaximumLevelCount);
        SnowLevelCount = (uint)Math.Min(SnowLevelCount, MaximumLevelCount);
        ColdLevelCount = (uint)Math.Min(ColdLevelCount, MaximumLevelCount);
    }

    private void UpdateSeparateSecretTracks()
    {
        bool available = IsTR2 || !RandomizeSecrets || SecretRewardMode == TRSecretRewardMode.Stack;
        if (available != _separateSecretTracks.IsActive)
        {
            _separateSecretTracks.IsActive = available;
            FirePropertyChanged(nameof(SeparateSecretTracks));
        }
    }

    private void UpdateItemMode()
    {
        bool defaultMode = ItemMode == ItemMode.Default;
        RandomizeItemTypes.IsActive = defaultMode;
        RandomizeItemPositions.IsActive = defaultMode;
        IncludeKeyItems.IsActive = defaultMode;
        OneItemDifficulty.IsActive = defaultMode;
        MaintainKeyContinuity.IsActive = defaultMode;

        AllowReturnPathLocations.IsActive = !defaultMode || IncludeKeyItems.Value;
        AllowEnemyKeyDrops.IsActive = !defaultMode || IncludeKeyItems.Value;

        FirePropertyChanged(nameof(WeaponDifficultyAvailable));
        FirePropertyChanged(nameof(IncludeKeyItemsImplied));
    }

    public bool RandomizationPossible
    {
        get => RandomizeGameMode || RandomizeUnarmedLevels || RandomizeAmmolessLevels || RandomizeSecretRewards || RandomizeHealth || RandomizeSunsets ||
               RandomizeAudioTracks || RandomizeItems || RandomizeEnemies || RandomizeSecrets || RandomizeTextures || RandomizeOutfits ||
               RandomizeText || RandomizeNightMode || RandomizeStartPosition || RandomizeEnvironment || RandomizeWeather;
    }

    public bool RandomizeGameMode
    {
        get => _gameModeControl.IsActive;
        set
        {
            _gameModeControl.IsActive = value;
            FirePropertyChanged();
            UpdateMaximumLevelCount();
        }
    }

    public BoolItemControlClass RandomizeLevelSequencing
    {
        get => _randomizeLevelSequencing;
        set
        {
            _randomizeLevelSequencing = value;
        }
    }

    public GameMode GameMode
    {
        get => _gameMode;
        set
        {
            _gameMode = value;
            FirePropertyChanged();
            FirePropertyChanged(nameof(TotalLevelCount));
            FirePropertyChanged(nameof(DefaultUnarmedLevelCount));
            FirePropertyChanged(nameof(DefaultAmmolessLevelCount));
            FirePropertyChanged(nameof(DefaultSunsetCount));
            PlayableLevelCount = (uint)TotalLevelCount;
        }
    }

    public GameMode[] GameModes
    {
        get => _gameModes;
        set
        {
            _gameModes = value;
            FirePropertyChanged();
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

    public GlobeDisplayOption[] GlobeDisplayOptions
    {
        get => _globeDisplayOptions;
        private set
        {
            _globeDisplayOptions = value;
            FirePropertyChanged();
        }
    }

    public int LevelSequencingSeed
    {
        get => _gameModeControl.Seed;
        set
        {
            _gameModeControl.Seed = value;
            FirePropertyChanged();
        }
    }

    public uint PlayableLevelCount
    {
        get => _playableLevelCount;
        set
        {
            _playableLevelCount = value;
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

    public bool RandomizeWeather
    {
        get => _weatherControl.IsActive;
        set
        {
            _weatherControl.IsActive = value;
            FirePropertyChanged();
        }
    }

    public int WeatherSeed
    {
        get => _weatherControl.Seed;
        set
        {
            _weatherControl.Seed = value;
            FirePropertyChanged();
        }
    }

    public uint RainLevelCount
    {
        get => _rainLevelCount;
        set
        {
            _rainLevelCount = value;
            FirePropertyChanged();
        }
    }

    public uint SnowLevelCount
    {
        get => _snowLevelCount;
        set
        {
            _snowLevelCount = value;
            FirePropertyChanged();
        }
    }

    public uint ColdLevelCount
    {
        get => _coldLevelCount;
        set
        {
            _coldLevelCount = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass RainyAssaultCourse
    {
        get => _rainyAssaultCourse;
        set
        {
            _rainyAssaultCourse = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass SnowyAssaultCourse
    {
        get => _snowyAssaultCourse;
        set
        {
            _snowyAssaultCourse = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass ColdAssaultCourse
    {
        get => _coldAssaultCourse;
        set
        {
            _coldAssaultCourse = value;
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
            UpdateSeparateSecretTracks();
        }
    }

    public string RandomizeSecretsText
    {
        get => _randomSecretsControl.Description;
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

    public BoolItemControlClass EnableUWCornerSecrets
    {
        get => _enableUWCornerSecrets;
        set
        {
            _enableUWCornerSecrets = value;
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

    public ItemMode ItemMode
    {
        get => _itemMode;
        set
        {
            _itemMode = value;
            FirePropertyChanged();
            UpdateItemMode();
        }
    }

    public ItemMode[] ItemModes
    {
        get => _itemModes;
        private set
        {
            _itemModes = value;
            FirePropertyChanged();
        }
    }

    public WeaponDifficulty WeaponDifficulty
    {
        get => _weaponDifficulty;
        set
        {
            _weaponDifficulty = value;
            FirePropertyChanged();
        }
    }

    public WeaponDifficulty[] WeaponDifficulties
    {
        get => _weaponDifficulties;
        private set
        {
            _weaponDifficulties = value;
            FirePropertyChanged();
        }
    }

    public bool WeaponDifficultyAvailable
    {
        get => ItemMode == ItemMode.Default && RandomizeItemTypes.Value;
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

    public bool IncludeKeyItemsImplied
    {
        get => ItemMode == ItemMode.Shuffled || IncludeKeyItems.Value;
    }

    public BoolItemControlClass RandomizeVehicles
    {
        get => _randomizeVehicles;
        set
        {
            _randomizeVehicles = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass AllowReturnPathLocations
    {
        get => _allowReturnPathLocations;
        set
        {
            _allowReturnPathLocations = value;
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

    public BoolItemControlClass OneItemDifficulty
    {
        get => _oneItemDifficulty;
        set
        {
            _oneItemDifficulty = value;
            FirePropertyChanged();
        }
    }

    public ItemRange KeyItemRange
    {
        get => _keyItemRange;
        set
        {
            _keyItemRange = value;
            FirePropertyChanged();
        }
    }

    public ItemRange[] ItemRanges
    {
        get => _itemRanges;
        set
        {
            _itemRanges = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass AllowEnemyKeyDrops
    {
        get => _allowEnemyKeyDrops;
        set
        {
            _allowEnemyKeyDrops = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass MaintainKeyContinuity
    {
        get => _maintainKeyContinuity;
        set
        {
            _maintainKeyContinuity = value;
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

    public BoolItemControlClass MatchTextureTypes
    {
        get => _matchTextureTypes;
        set
        {
            _matchTextureTypes = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass MatchTextureItems
    {
        get => _matchTextureItems;
        set
        {
            _matchTextureItems = value;
            FirePropertyChanged();
        }
    }

    public bool FilterSourceTextures
    {
        get => _filterSourceTextures;
        set
        {
            _filterSourceTextures = value;
            FirePropertyChanged();
            FirePropertyChanged(nameof(CanFilterTextures));
        }
    }

    public List<BoolItemIDControlClass> SourceTextureAreas
    {
        get => _sourceTextureAreas;
        set
        {
            _sourceTextureAreas = value;
            FirePropertyChanged();
        }
    }

    public TextureMode[] TextureModes
    {
        get => _textureModes;
        private set
        {
            _textureModes = value;
            FirePropertyChanged();
        }
    }

    public TextureMode TextureMode
    {
        get => _textureMode;
        set
        {
            _textureMode = value;
            FirePropertyChanged();

            MatchTextureItems.IsActive = value == TextureMode.Game;
            FirePropertyChanged(nameof(MatchTextureItems));
            FirePropertyChanged(nameof(CanFilterTextures));
        }
    }

    public bool CanFilterTextures
    {
        get => TextureMode != TextureMode.Level && FilterSourceTextures;
    }

    private void SourceTextureArea_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        FirePropertyChanged(nameof(SourceTextureAreas));
        UpdateSourceTextureAreas();
    }

    private void UpdateSourceTextureAreas()
    {
        SourceTextureAreas.ForEach(a => a.IsActive = true);
        if (SourceTextureAreas.Count(a => a.Value) == 1)
        {
            SourceTextureAreas.Find(a => a.Value).IsActive = false;
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

    public BoolItemControlClass RandomizeTraps
    {
        get => _randomizeTraps;
        set
        {
            _randomizeTraps = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass RandomizeChallengeRooms
    {
        get => _randomizeChallengeRooms;
        set
        {
            _randomizeChallengeRooms = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass HardEnvironmentMode
    {
        get => _hardEnvironmentMode;
        set
        {
            _hardEnvironmentMode = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass BlockShortcuts
    {
        get => _blockShortcuts;
        set
        {
            _blockShortcuts = value;
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

    public bool AddReturnPaths
    {
        get => _addReturnPaths;
        set
        {
            _addReturnPaths = value;
            FirePropertyChanged();
        }
    }

    public bool FixOGBugs
    {
        get => _fixOGBugs;
        set
        {
            _fixOGBugs = value;
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

    public BoolItemControlClass GuaranteeSecrets
    {
        get => _guaranteeSecrets;
        set
        {
            _guaranteeSecrets = value;
            FirePropertyChanged();
        }
    }

    public TRSecretRewardMode[] SecretRewardModes
    {
        get => _secretRewardModes;
        private set
        {
            _secretRewardModes = value;
            FirePropertyChanged();
        }
    }

    public TRSecretRewardMode SecretRewardMode
    {
        get => _secretRewardMode;
        set
        {
            _secretRewardMode = value;
            FirePropertyChanged();
            UpdateSeparateSecretTracks();
        }
    }

    public bool UseRewardRoomCameras
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

    public TRSecretCountMode[] SecretCountModes
    {
        get => _secretCountModes;
        private set
        {
            _secretCountModes = value;
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
        }
    }

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

    public bool UseSecretPack
    {
        get => _useSecretPack;
        set
        {
            _useSecretPack = value;
            FirePropertyChanged();
            FirePropertyChanged(nameof(UseGenericSecrets));
        }
    }

    public bool UseGenericSecrets
    {
        get => !IsSecretPackTypeSupported || !UseSecretPack;
    }

    public string SecretPack
    {
        get => _secretPack;
        set
        {
            _secretPack = value;
            FirePropertyChanged();
        }
    }

    public string[] AvailableSecretPacks
    {
        get => _availableSecretPacks;
        set
        {
            _availableSecretPacks = value;
            FirePropertyChanged();
            FirePropertyChanged(nameof(IsSecretPackTypeSupported));
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

    public BoolItemControlClass RelocateAwkwardEnemies
    {
        get => _relocateAwkwardEnemies;
        set
        {
            _relocateAwkwardEnemies = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass HideDeadTrexes
    {
        get => _hideDeadTrexes;
        set
        {
            _hideDeadTrexes = value;
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

    public BirdMonsterBehaviour[] BirdMonsterBehaviours
    {
        get => _birdMonsterBehaviours;
        private set
        {
            _birdMonsterBehaviours = value;
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

    public DragonSpawnType[] DragonSpawnTypes
    {
        get => _dragonSpawnTypes;
        private set
        {
            _dragonSpawnTypes = value;
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

    public BoolItemControlClass ReplaceRequiredEnemies
    {
        get => _replaceRequiredEnemies;
        set
        {
            _replaceRequiredEnemies = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass GiveUnarmedItems
    {
        get => _giveUnarmedItems;
        set
        {
            _giveUnarmedItems = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass UnrestrictedEnemyDifficulty
    {
        get => _unrestrictedEnemyDifficulty;
        set
        {
            _unrestrictedEnemyDifficulty = value;
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

    public BoolItemControlClass RetainEnemyTextures
    {
        get => _retainEnemyTextures;
        set
        {
            _retainEnemyTextures = value;
            FirePropertyChanged();
        }
    }

    public BoolItemControlClass RetainLaraTextures
    {
        get => _retainLaraTextures;
        set
        {
            _retainLaraTextures = value;
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

    public bool UseSolidInteractableWireframing
    {
        get => _useSolidInteractableWireframing;
        set
        {
            _useSolidInteractableWireframing = value;
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

    public bool ShowWireframeTriggers
    {
        get => _showWireframeTriggers;
        set
        {
            _showWireframeTriggers = value;
            FirePropertyChanged();
        }
    }

    public bool ShowWireframeTriggerColours
    {
        get => _showWireframeTriggerColours;
        set
        {
            _showWireframeTriggerColours = value;
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

    public List<BoolItemControlClass> GameOrderBoolItemControls
    {
        get => _gameOrderBoolItemControls;
        set
        {
            _gameOrderBoolItemControls = value;
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

    public List<BoolItemControlClass> WeatherBoolItemControls
    {
        get => _weatherBoolItemControls;
        set
        {
            _weatherBoolItemControls = value;
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
            FirePropertyChanged(nameof(CanSelectEnemyExclusions));
        }
    }

    public bool CanSelectEnemyExclusions
    {
        get => _crossLevelEnemies.Value && _useEnemyExclusions;
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

    public bool UseEnemyClones
    {
        get => _useEnemyClones;
        set
        {
            _useEnemyClones = value;
            FirePropertyChanged();
        }
    }

    public uint EnemyMultiplier
    {
        get => _enemyMultiplier;
        set
        {
            _enemyMultiplier = value;
            FirePropertyChanged();
        }
    }

    public uint MaxEnemyMultiplier
    {
        get => _maxEnemyMultiplier;
        private set
        {
            _maxEnemyMultiplier = value;
            FirePropertyChanged();
        }
    }

    public bool CloneOriginalEnemies
    {
        get => _cloneOriginalEnemies;
        set
        {
            _cloneOriginalEnemies = value;
            FirePropertyChanged();
        }
    }

    public bool UseKillableClonePierres
    {
        get => _useKillableClonePierres;
        set
        {
            _useKillableClonePierres = value;
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

    public SpriteRandoMode[] SpriteRandoModes
    {
        get => _spriteRandoModes;
        private set
        {
            _spriteRandoModes = value;
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

        _gameModeControl = new ManagedSeedBool();
        _unarmedLevelsControl = new ManagedSeedNumeric();
        _ammolessLevelsControl = new ManagedSeedNumeric();
        _secretRewardsControl = new ManagedSeed();
        _healthLevelsControl = new ManagedSeedBool();
        _sunsetLevelsControl = new ManagedSeedNumeric();
        _weatherControl = new ManagedSeedBool();
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

        // Game mode
        Binding randomizeGameModeBinding = new(nameof(RandomizeGameMode)) { Source = this };
        RandomizeLevelSequencing = new BoolItemControlClass()
        {
            Title = "Randomize level order",
            Description = "Shuffle the order in which levels are played."
        };
        BindingOperations.SetBinding(RandomizeLevelSequencing, BoolItemControlClass.IsActiveProperty, randomizeGameModeBinding);

        // Secrets
        Binding randomizeSecretsBinding = new(nameof(RandomizeSecrets)) { Source = this };
        IsHardSecrets = new BoolItemControlClass()
        {
            Title = "Enable hard secrets",
            Description = "Locations classed as hard will be included in the randomization pool."
        };
        BindingOperations.SetBinding(IsHardSecrets, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);
        EnableUWCornerSecrets = new()
        {
            Title = "Enable underwater corner secrets",
            Description = "Trickier underwater corner locations will be included in the randomization pool.",
        };
        BindingOperations.SetBinding(EnableUWCornerSecrets, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);
        IsGlitchedSecrets = new BoolItemControlClass()
        {
            Title = "Enable glitched secrets",
            Description = "Locations that require glitches to reach will be included in the randomization pool."
        };
        BindingOperations.SetBinding(IsGlitchedSecrets, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);
        GuaranteeSecrets = new BoolItemControlClass
        {
            Title = "Use guaranteed spawn mode",
            Description = "Guarantees that at least one hard and/or glitched secret (based on above selection) will appear in each level where possible."
        };
        BindingOperations.SetBinding(GuaranteeSecrets, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);
        UseRandomSecretModels = new BoolItemControlClass()
        {
            Title = "Use random secret types",
            Description = "If enabled, secret types will be randomized across levels; otherwise, pre-defined types will be allocated to each level."
        };
        BindingOperations.SetBinding(UseRandomSecretModels, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);

        IsHardSecrets.PropertyChanged += SecretCategory_PropertyChanged;
        IsGlitchedSecrets.PropertyChanged += SecretCategory_PropertyChanged;

        // Items
        Binding randomizeItemsBinding = new(nameof(RandomizeItems)) { Source = this };
        RandomizeItemTypes = new BoolItemControlClass
        {
            Title = "Randomize types",
            Description = "The types of standard pickups will be randomized e.g. a small medi may become a shotgun."
        };
        BindingOperations.SetBinding(RandomizeItemTypes, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);

        RandomizeItemTypes.PropertyChanged += (s, e) => FirePropertyChanged(nameof(WeaponDifficultyAvailable));

        RandomizeItemPositions = new BoolItemControlClass
        {
            Title = "Randomize positions",
            Description = "The positions of standard pickups will be randomized."
        };
        BindingOperations.SetBinding(RandomizeItemPositions, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
        IncludeKeyItems = new BoolItemControlClass()
        {
            Title = "Include key items",
            Description = "The positions of key item pickups will be randomized. Items will be placed before their respective locks/slots."
        };
        BindingOperations.SetBinding(IncludeKeyItems, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
        RandomizeVehicles = new()
        {
            Title = "Include vehicles",
            Description = "The positions of vehicles may change within a level, and they may appear in different levels."
        };
        BindingOperations.SetBinding(RandomizeVehicles, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
        AllowReturnPathLocations = new()
        {
            Title = "Allow return path locations",
            Description = "Allows key items to be placed before points of no return, provided that return paths are enabled (see Global Settings)."
        };
        BindingOperations.SetBinding(AllowReturnPathLocations, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
        AllowEnemyKeyDrops = new()
        {
            Title = "Allow enemy key item drops",
            Description = "Allows key items to be allocated to enemies who will drop them when killed."
        };
        BindingOperations.SetBinding(AllowEnemyKeyDrops, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
        MaintainKeyContinuity = new()
        {
            Title = "Maintain key item continuity",
            Description = $"Maintains continuity for key items when level sequencing changes.{Environment.NewLine}e.g. The Seraph will become a pickup in Barkhang Monastery if The Deck has not yet been visited."
        };
        BindingOperations.SetBinding(MaintainKeyContinuity, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
        OneItemDifficulty = new()
        {
            Title = "Use one-item difficulty mode",
            Description = "Each item type will spawn a maximum of once per level."
        };
        BindingOperations.SetBinding(OneItemDifficulty, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);
        IncludeExtraPickups = new BoolItemControlClass
        {
            Title = "Add extra pickups",
            Description = "Add more weapon, ammo and medi items to some levels for Lara to find."
        };
        BindingOperations.SetBinding(IncludeExtraPickups, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);

        IncludeKeyItems.PropertyChanged += IncludeKeyItems_PropertyChanged;

        // Enemies
        Binding randomizeEnemiesBinding = new(nameof(RandomizeEnemies)) { Source = this };
        CrossLevelEnemies = new BoolItemControlClass()
        {
            Title = "Enable cross-level enemies",
            Description = "Allow enemy types to appear in any level."
        };
        BindingOperations.SetBinding(CrossLevelEnemies, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
        CrossLevelEnemies.PropertyChanged += (e, s) => FirePropertyChanged(nameof(CanSelectEnemyExclusions));

        DocileWillard = new BoolItemControlClass()
        {
            Title = "Enable docile Willard",
            Description = "Willard can appear in levels other than Meteorite Cavern but will not attack Lara unless she gets too close."
        };
        BindingOperations.SetBinding(DocileWillard, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
        RelocateAwkwardEnemies = new BoolItemControlClass()
        {
            Title = "Move awkward enemies",
            Description = "Some enemies will be moved to avoid forced damage or overly difficult situations.",
            HelpURL = "https://github.com/LostArtefacts/TR-Rando/blob/master/Resources/Documentation/ENEMIES.md#awkward-enemies",
        };
        BindingOperations.SetBinding(RelocateAwkwardEnemies, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
        HideDeadTrexes = new()
        {
            Title = "Hide dead T-rexes",
            Description = "T-rexes retain collision on death in TR1R, so this option will move them out of the way when killed.",
        };
        BindingOperations.SetBinding(HideDeadTrexes, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
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
        ReplaceRequiredEnemies = new BoolItemControlClass
        {
            Title = "Replace required enemies",
        };
        BindingOperations.SetBinding(ReplaceRequiredEnemies, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
        GiveUnarmedItems = new()
        {
            Title = "Give unarmed level items",
            Description = "If a level is unarmed, give extra ammo, medipacks and a weapon other than the pistols (based on enemy difficulty)."
        };
        BindingOperations.SetBinding(GiveUnarmedItems, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
        UnrestrictedEnemyDifficulty = new()
        {
            Title = "Unrestricted difficulty",
            Description = "Disables virtually all cross-level enemy restrictions except for technical ones.",
            HelpURL = "https://github.com/LostArtefacts/TR-Rando/blob/master/Resources/Documentation/ENEMIES.md",
        };
        BindingOperations.SetBinding(UnrestrictedEnemyDifficulty, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);

        // Textures
        Binding randomizeTexturesBinding = new(nameof(RandomizeTextures)) { Source = this };
        MatchTextureTypes = new()
        {
            Title = "Match texture types",
            Description = "Textures for such things as levers, windows and ladders will be matched where possible with those from the import set."
        };
        BindingOperations.SetBinding(MatchTextureTypes, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
        MatchTextureItems = new()
        {
            Title = "Match item types",
            Description = "Movable items such as doors, levers and traps will be matched where possible with those from the import set."
        };
        BindingOperations.SetBinding(MatchTextureItems, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
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
        RetainEnemyTextures = new BoolItemControlClass
        {
            Title = "Use original enemy textures",
            Description = "Texture mapping will not apply to enemies."
        };
        BindingOperations.SetBinding(RetainEnemyTextures, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
        RetainLaraTextures = new BoolItemControlClass
        {
            Title = "Use original outfit textures",
            Description = "Texture mapping will not apply to Lara's outfits."
        };
        BindingOperations.SetBinding(RetainLaraTextures, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
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
        Binding randomizeAudioBinding = new(nameof(RandomizeAudioTracks)) { Source = this };
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
        Binding randomizeOutfitsBinding = new(nameof(RandomizeOutfits)) { Source = this };
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
        Binding randomizeTextBinding = new(nameof(RandomizeText)) { Source = this };
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
        Binding randomizeStartPositionBinding = new(nameof(RandomizeStartPosition)) { Source = this };
        RotateStartPositionOnly = new BoolItemControlClass
        {
            Title = "Rotate Lara only",
            Description = "Don't change Lara's position, and instead change only the direction she is facing at the start."
        };
        BindingOperations.SetBinding(RotateStartPositionOnly, BoolItemControlClass.IsActiveProperty, randomizeStartPositionBinding);

        // Environment
        Binding randomizeEnvironmentBinding = new(nameof(RandomizeEnvironment)) { Source = this };
        RandomizeWaterLevels = new BoolItemControlClass
        {
            Title = "Change water levels",
            Description = "Flood or drain particular rooms in each level."
        };
        BindingOperations.SetBinding(RandomizeWaterLevels, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);
        RandomizeSlotPositions = new BoolItemControlClass
        {
            Title = "Move keyholes/switches/puzzle slots",
            Description = "Change where keyholes, switches and puzzle slots are located in each level."
        };
        BindingOperations.SetBinding(RandomizeSlotPositions, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);
        RandomizeLadders = new BoolItemControlClass
        {
            Title = "Move ladders",
            Description = "Change where ladders are positioned in each level."
        };
        BindingOperations.SetBinding(RandomizeLadders, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);
        RandomizeTraps = new BoolItemControlClass
        {
            Title = "Randomize traps",
            Description = "Change where traps appear in each level."
        };
        BindingOperations.SetBinding(RandomizeTraps, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);
        RandomizeChallengeRooms = new BoolItemControlClass
        {
            Title = "Randomize puzzles/challenge rooms",
            Description = "Allow rooms and areas to be created with puzzles to solve or challenges to complete."
        };
        BindingOperations.SetBinding(RandomizeChallengeRooms, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);
        HardEnvironmentMode = new BoolItemControlClass
        {
            Title = "Enable hard mode",
            Description = "Allow more difficult puzzles, rooms and other environmental changes."
        };
        BindingOperations.SetBinding(HardEnvironmentMode, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);
        BlockShortcuts = new()
        {
            Title = "Allow shortcut fixes",
            Description = "Allow environment changes that may block classic level shortcuts.",
        };
        BindingOperations.SetBinding(BlockShortcuts, BoolItemControlClass.IsActiveProperty, randomizeEnvironmentBinding);

        // Health
        Binding randomizeHealthBinding = new(nameof(RandomizeHealth)) { Source = this };
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

        // Weather
        Binding randomizeWeatherBinding = new(nameof(RandomizeWeather)) { Source = this };
        RainyAssaultCourse = new BoolItemControlClass
        {
            Title = "Rainy assault course",
            Description = "Add rain to Lara's home."
        };
        BindingOperations.SetBinding(RainyAssaultCourse, BoolItemControlClass.IsActiveProperty, randomizeWeatherBinding);
        SnowyAssaultCourse = new BoolItemControlClass
        {
            Title = "Snowy assault course",
            Description = "Add snow to Lara's home."
        };
        BindingOperations.SetBinding(SnowyAssaultCourse, BoolItemControlClass.IsActiveProperty, randomizeWeatherBinding);
        ColdAssaultCourse = new BoolItemControlClass
        {
            Title = "Cold assault course",
            Description = "Make the water in Lara's home cold."
        };
        BindingOperations.SetBinding(ColdAssaultCourse, BoolItemControlClass.IsActiveProperty, randomizeWeatherBinding);

        // all item controls
        GameOrderBoolItemControls = new()
        {
            _randomizeLevelSequencing
        };
        SecretBoolItemControls = new()
        {
            _isHardSecrets, _allowGlitched, _enableUWCornerSecrets, _guaranteeSecrets, _useRandomSecretModels
        };
        ItemBoolItemControls = new()
        {
            _randomizeItemTypes, _randomizeItemLocations, _includeKeyItems, _allowReturnPathLocations, _allowEnemyKeyDrops, _randomizeVehicles, _oneItemDifficulty, _includeExtraPickups, _maintainKeyContinuity
        };
        EnemyBoolItemControls = new()
        {
            _crossLevelEnemies, _docileWillard, _protectMonks, _swapEnemyAppearance, _allowEmptyEggs, _hideEnemies, _relocateAwkwardEnemies, _hideDeadTrexes, _replaceRequiredEnemies, _giveUnarmedItems,_allowEnemyKeyDrops, _unrestrictedEnemyDifficulty
        };
        TextureBoolItemControls = new()
        {
            _matchTextureTypes, _matchTextureItems, _persistTextures, _randomizeWaterColour, _retainLevelTextures, _retainLaraTextures, _retainEnemyTextures, _retainKeySpriteTextures, _retainSecretSpriteTextures
        };
        AudioBoolItemControls = new()
        {
            _changeAmbientTracks, _includeBlankTracks, _changeTriggerTracks, _separateSecretTracks, _changeWeaponSFX,
            _changeCrashSFX, _changeEnemySFX, _changeDoorSFX, _linkCreatureSFX, _randomizeWibble
        };
        OutfitBoolItemControls = new()
        {
            _persistOutfits, _removeRobeDagger, _allowGymOutfit
        };
        TextBoolItemControls = new()
        {
            _retainKeyItemNames, _retainLevelNames
        };
        StartBoolItemControls = new()
        {
            _rotateStartPosition
        };
        EnvironmentBoolItemControls = new()
        {
            _randomizeWaterLevels, _randomizeSlotPositions, _randomizeLadders, _randomizeTraps,
            _randomizeChallengeRooms, _hardEnvironmentMode, _blockShortcuts
        };
        HealthBoolItemControls = new()
        {
            _disableHealingBetweenLevels, _disableMedpacks
        };
        WeatherBoolItemControls = new()
        {
            _rainyAssaultCourse, _snowyAssaultCourse, _coldAssaultCourse
        };

        IEnumerable<BoolItemControlClass> controls = GameOrderBoolItemControls
            .Concat(SecretBoolItemControls)
            .Concat(ItemBoolItemControls)
            .Concat(EnemyBoolItemControls)
            .Concat(TextureBoolItemControls)
            .Concat(AudioBoolItemControls)
            .Concat(OutfitBoolItemControls)
            .Concat(TextureBoolItemControls)
            .Concat(StartBoolItemControls)
            .Concat(EnvironmentBoolItemControls)
            .Concat(HealthBoolItemControls)
            .Concat(WeatherBoolItemControls);

        foreach (BoolItemControlClass control in controls)
        {
            control.PropertyChanged += BoolControl_PropertyChanged;
        }
    }

    private void BoolControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BoolItemControlClass.Value))
        {
            FirePropertyChanged(e.PropertyName);
        }
    }

    private void IncludeKeyItems_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        bool defaultMode = ItemMode == ItemMode.Default;
        AllowReturnPathLocations.IsActive = !defaultMode || IncludeKeyItems.Value;
        AllowEnemyKeyDrops.IsActive = !defaultMode || IncludeKeyItems.Value;
        FirePropertyChanged(nameof(IncludeKeyItemsImplied));
    }

    private void SecretCategory_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        GuaranteeSecrets.IsActive = IsGlitchedSecrets.Value || IsHardSecrets.Value;
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
        _includeBlankTracks.IsAvailable = IsBlankTracksTypeSupported;
        _separateSecretTracks.IsAvailable = IsSecretAudioSupported;

        _changeWeaponSFX.IsAvailable = _changeCrashSFX.IsAvailable = _changeEnemySFX.IsAvailable = _linkCreatureSFX.IsAvailable = _changeDoorSFX.IsAvailable = IsSFXTypeSupported;

        _useRandomSecretModels.IsAvailable = IsSecretModelsTypeSupported;

        _swapEnemyAppearance.IsAvailable = IsMeshSwapsTypeSupported;

        _protectMonks.IsAvailable = !IsTR1;
        _docileWillard.IsAvailable = IsTR3;
        _hideDeadTrexes.IsAvailable = IsHideDeadTrexesTypeSupported;

        _includeKeyItems.IsAvailable = IsKeyItemTypeSupported;
        _randomizeVehicles.IsAvailable = IsVehiclesTypeSupported;
        _maintainKeyContinuity.IsAvailable = IsKeyContinuityTypeSupported;
        _includeExtraPickups.IsAvailable = IsExtraPickupsTypeSupported;
        _allowEnemyKeyDrops.IsAvailable = IsItemDropsTypeSupported;
        _allowReturnPathLocations.IsAvailable = IsReturnPathsTypeSupported;

        _allowGlitched.IsAvailable = IsGlitchedSecretsSupported;
        _isHardSecrets.IsAvailable = IsHardSecretsSupported;
        _guaranteeSecrets.IsAvailable = IsGlitchedSecretsSupported || IsHardSecretsSupported;

        _retainSecretSpriteTextures.IsAvailable = IsSecretTexturesTypeSupported;
        _retainKeySpriteTextures.IsAvailable = IsKeyItemTexturesTypeSupported;
        _randomizeWaterColour.IsAvailable = IsWaterColourTypeSupported;
        _retainEnemyTextures.IsAvailable = IsDynamicEnemyTexturesTypeSupported;
        _allowEmptyEggs.IsAvailable = IsAtlanteanEggBehaviourTypeSupported;
        _hideEnemies.IsAvailable = IsHiddenEnemiesTypeSupported;
        _replaceRequiredEnemies.IsAvailable = IsReplaceRequiredEnemyTypeSupported;

        _randomizeLadders.IsAvailable = !IsTR1;
        _randomizeTraps.IsAvailable = IsTrapsTypeSupported;
        _randomizeChallengeRooms.IsAvailable = IsChallengeRoomsTypeSupported;
        _hardEnvironmentMode.IsAvailable = IsHardEnvironmentTypeSupported;

        _matchTextureTypes.IsAvailable = IsTextureSwapTypeSupported;
        _matchTextureItems.IsAvailable = IsTextureSwapTypeSupported;
        _persistTextures.IsAvailable = !IsTextureSwapTypeSupported;
        _retainLaraTextures.IsAvailable = !IsTextureSwapTypeSupported;
    }

    public void Load(TRRandomizerController controller)
    {
        _controller = controller;

        RandomizeGameMode = _controller.RandomizeGameMode;
        GameModes = Enum.GetValues<GameMode>();
        GameMode = _controller.GameMode;
        RandomizeLevelSequencing.Value = _controller.RandomizeSequencing;
        LevelSequencingSeed = _controller.LevelSequencingSeed;
        PlayableLevelCount = _controller.PlayableLevelCount;
        GlobeDisplayOptions = Enum.GetValues<GlobeDisplayOption>();
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

        RandomizeWeather = _controller.RandomizeWeather;
        WeatherSeed = _controller.WeatherSeed;
        RainyAssaultCourse.Value = _controller.RainyAssaultCourse;
        SnowyAssaultCourse.Value = _controller.SnowyAssaultCourse;
        ColdAssaultCourse.Value = _controller.ColdAssaultCourse;
        RainLevelCount = _controller.RainLevelCount;
        SnowLevelCount = _controller.SnowLevelCount;
        ColdLevelCount = _controller.ColdLevelCount;

        RandomizeNightMode = _controller.RandomizeNightMode;
        NightModeSeed = _controller.NightModeSeed;
        NightModeCount = _controller.NightModeCount;
        NightModeAssaultCourse = _controller.NightModeAssaultCourse;
        OverrideSunsets = _controller.OverrideSunsets;
        NightModeDarkness = _controller.NightModeDarkness;
        NightModeDarknessMaximum = TRRandomizerController.NightModeDarknessRange;
        VfxFilterColor = _controller.VfxFilterColor;
        VfxRandomize = _controller.RandomizeVfx;
        VfxAvailColors = TRRandomizerController.VfxAvailableColorChoices;
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
        ItemModes = Enum.GetValues<ItemMode>();
        ItemMode = _controller.ItemMode;
        WeaponDifficulties = Enum.GetValues<WeaponDifficulty>();
        WeaponDifficulty = _controller.WeaponDifficulty;
        IncludeKeyItems.Value = _controller.IncludeKeyItems;
        RandomizeVehicles.Value = _controller.RandomizeVehicles;
        AllowReturnPathLocations.Value = _controller.AllowReturnPathLocations;
        IncludeExtraPickups.Value = _controller.IncludeExtraPickups;
        RandomizeItemTypes.Value = _controller.RandomizeItemTypes;
        RandomizeItemPositions.Value = _controller.RandomizeItemPositions;
        OneItemDifficulty.Value = _controller.RandoItemDifficulty == ItemDifficulty.OneLimit;
        ItemRanges = Enum.GetValues<ItemRange>();
        KeyItemRange = _controller.KeyItemRange;
        AllowEnemyKeyDrops.Value = _controller.AllowEnemyKeyDrops;
        MaintainKeyContinuity.Value = _controller.MaintainKeyContinuity;

        RandomizeEnemies = _controller.RandomizeEnemies;
        EnemySeed = _controller.EnemySeed;
        CrossLevelEnemies.Value = _controller.CrossLevelEnemies;
        ProtectMonks.Value = _controller.ProtectMonks;
        DocileWillard.Value = _controller.DocileWillard;
        RelocateAwkwardEnemies.Value = _controller.RelocateAwkwardEnemies;
        HideDeadTrexes.Value = _controller.HideDeadTrexes;
        BirdMonsterBehaviours = Enum.GetValues<BirdMonsterBehaviour>();
        BirdMonsterBehaviour = _controller.BirdMonsterBehaviour;
        DragonSpawnTypes = Enum.GetValues<DragonSpawnType>();
        DragonSpawnType = _controller.DragonSpawnType;
        SwapEnemyAppearance.Value = _controller.SwapEnemyAppearance;
        AllowEmptyEggs.Value = _controller.AllowEmptyEggs;
        HideEnemies.Value = _controller.HideEnemiesUntilTriggered;
        ReplaceRequiredEnemies.Value = _controller.ReplaceRequiredEnemies;
        GiveUnarmedItems.Value = _controller.GiveUnarmedItems;
        UnrestrictedEnemyDifficulty.Value = _controller.RandoEnemyDifficulty == RandoDifficulty.NoRestrictions;
        UseEnemyExclusions = _controller.UseEnemyExclusions;
        ShowExclusionWarnings = _controller.ShowExclusionWarnings;
        LoadEnemyExclusions();

        UseEnemyClones = _controller.UseEnemyClones;
        MaxEnemyMultiplier = TRRandomizerController.MaxEnemyMultiplier;
        EnemyMultiplier = _controller.EnemyMultiplier;        
        CloneOriginalEnemies = _controller.CloneOriginalEnemies;
        UseKillableClonePierres = _controller.UseKillableClonePierres;

        RandomizeSecrets = _controller.RandomizeSecrets;
        SecretSeed = _controller.SecretSeed;
        IsHardSecrets.Value = _controller.HardSecrets;
        EnableUWCornerSecrets.Value = _controller.EnableUWCornerSecrets;
        IsGlitchedSecrets.Value = _controller.GlitchedSecrets;
        GuaranteeSecrets.Value = _controller.GuaranteeSecrets;
        SecretRewardModes = Enum.GetValues<TRSecretRewardMode>();
        SecretRewardMode = _controller.SecretRewardMode;
        UseRewardRoomCameras = _controller.UseRewardRoomCameras;
        UseRandomSecretModels.Value = _controller.UseRandomSecretModels;
        SecretCountModes = Enum.GetValues<TRSecretCountMode>();
        SecretCountMode = _controller.SecretCountMode;
        MaxSecretCount = _controller.MaxSecretCount;
        MinSecretCount = _controller.MinSecretCount;            
        AvailableSecretPacks = _controller.AvailableSecretPacks;
        SecretPack = _controller.SecretPack;
        UseSecretPack = _controller.UseSecretPack;
        if ((SecretPack == null || SecretPack == string.Empty) && AvailableSecretPacks.Length > 0)
        {
            SecretPack = AvailableSecretPacks[0];
        }

        RandomizeTextures = _controller.RandomizeTextures;
        TextureSeed = _controller.TextureSeed;
        TextureModes = Enum.GetValues<TextureMode>();
        TextureMode = _controller.TextureMode;
        MatchTextureTypes.Value = _controller.MatchTextureTypes;
        MatchTextureItems.Value = _controller.MatchTextureItems;
        SourceTextureAreas = _controller.AvailableSourceTextureAreas
            .Select(a => new BoolItemIDControlClass
            {
                ID = (int)a,
                Title = a.ToString(),
                Value = _controller.SourceTextureAreas.Contains(a),
            }).ToList();
        SourceTextureAreas.ForEach(a => a.PropertyChanged += SourceTextureArea_PropertyChanged);
        FilterSourceTextures = _controller.FilterSourceTextures;
        UpdateSourceTextureAreas();

        PersistTextures.Value = _controller.PersistTextures;
        RandomizeWaterColour.Value = _controller.RandomizeWaterColour;
        RetainMainLevelTextures.Value = _controller.RetainMainLevelTextures;
        RetainEnemyTextures.Value = _controller.RetainEnemyTextures;
        RetainLaraTextures.Value = _controller.RetainLaraTextures;
        RetainKeySpriteTextures.Value = _controller.RetainKeySpriteTextures;
        RetainSecretSpriteTextures.Value = _controller.RetainSecretSpriteTextures;
        WireframeLevelCount = _controller.WireframeLevelCount;
        AssaultCourseWireframe = _controller.AssaultCourseWireframe;
        UseSolidLaraWireframing = _controller.UseSolidLaraWireframing;
        UseSolidEnemyWireframing = _controller.UseSolidEnemyWireframing;
        UseSolidInteractableWireframing = _controller.UseSolidInteractableWireframing;
        UseDifferentWireframeColours = _controller.UseDifferentWireframeColours;
        UseWireframeLadders = _controller.UseWireframeLadders;
        ShowWireframeTriggers = _controller.ShowWireframeTriggers;
        ShowWireframeTriggerColours = _controller.ShowWireframeTriggerColours;

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
        RandomizeTraps.Value = _controller.RandomizeTraps;
        RandomizeChallengeRooms.Value = _controller.RandomizeChallengeRooms;
        HardEnvironmentMode.Value = _controller.HardEnvironmentMode;
        BlockShortcuts.Value = _controller.BlockShortcuts;
        MirroredLevelCount = _controller.MirroredLevelCount;
        MirrorAssaultCourse = _controller.MirrorAssaultCourse;

        DevelopmentMode = _controller.DevelopmentMode;
        DisableDemos = _controller.DisableDemos;
        AutoLaunchGame = _controller.AutoLaunchGame;
        AddReturnPaths = _controller.AddReturnPaths;
        FixOGBugs = _controller.FixOGBugs;

        SpriteRandoModes = Enum.GetValues<SpriteRandoMode>();
        SpriteRandoMode = _controller.SpriteRandoMode;
        RandomizeItemSprites = _controller.RandomizeItemSprites;
        RandomizeKeyItemSprites = _controller.RandomizeKeyItemSprites;
        RandomizeSecretSprites = _controller.RandomizeSecretSprites;

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
        _controller.RandomizeGameMode = RandomizeGameMode;
        _controller.GameMode = GameMode;
        _controller.RandomizeSequencing = RandomizeLevelSequencing.Value;
        _controller.LevelSequencingSeed = LevelSequencingSeed;
        _controller.PlayableLevelCount = PlayableLevelCount;
        _controller.GlobeDisplay = GlobeDisplay;

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

        _controller.RandomizeWeather = RandomizeWeather;
        _controller.WeatherSeed = WeatherSeed;
        _controller.RainyAssaultCourse = RainyAssaultCourse.Value;
        _controller.SnowyAssaultCourse = SnowyAssaultCourse.Value;
        _controller.ColdAssaultCourse = ColdAssaultCourse.Value;
        _controller.RainLevelCount = RainLevelCount;
        _controller.SnowLevelCount = SnowLevelCount;
        _controller.ColdLevelCount = ColdLevelCount;

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
        _controller.ItemMode = ItemMode;
        _controller.WeaponDifficulty = WeaponDifficulty;
        _controller.IncludeKeyItems = IncludeKeyItems.Value;
        _controller.RandomizeVehicles = RandomizeVehicles.Value;
        _controller.AllowReturnPathLocations = AllowReturnPathLocations.Value;
        _controller.IncludeExtraPickups = IncludeExtraPickups.Value;
        _controller.RandomizeItemTypes = RandomizeItemTypes.Value;
        _controller.RandomizeItemPositions = RandomizeItemPositions.Value;
        _controller.RandoItemDifficulty = OneItemDifficulty.Value ? ItemDifficulty.OneLimit : ItemDifficulty.Default;
        _controller.KeyItemRange = KeyItemRange;
        _controller.AllowEnemyKeyDrops = AllowEnemyKeyDrops.Value;
        _controller.MaintainKeyContinuity = MaintainKeyContinuity.Value;

        _controller.RandomizeEnemies = RandomizeEnemies;
        _controller.EnemySeed = EnemySeed;
        _controller.CrossLevelEnemies = CrossLevelEnemies.Value;
        _controller.ProtectMonks = ProtectMonks.Value;
        _controller.DocileWillard = DocileWillard.Value;
        _controller.RelocateAwkwardEnemies = RelocateAwkwardEnemies.Value;
        _controller.HideDeadTrexes = HideDeadTrexes.Value;
        _controller.BirdMonsterBehaviour = BirdMonsterBehaviour;
        _controller.DragonSpawnType = DragonSpawnType;
        _controller.SwapEnemyAppearance = SwapEnemyAppearance.Value;
        _controller.AllowEmptyEggs = AllowEmptyEggs.Value;
        _controller.HideEnemiesUntilTriggered = HideEnemies.Value;
        _controller.ReplaceRequiredEnemies = ReplaceRequiredEnemies.Value;
        _controller.GiveUnarmedItems = GiveUnarmedItems.Value;
        _controller.RandoEnemyDifficulty = UnrestrictedEnemyDifficulty.Value ? RandoDifficulty.NoRestrictions : RandoDifficulty.Default;
        _controller.UseEnemyExclusions = UseEnemyExclusions;
        _controller.ShowExclusionWarnings = ShowExclusionWarnings;

        List<short> excludedEnemies = new();
        SelectableEnemyControls.FindAll(c => c.Value).ForEach(c => excludedEnemies.Add((short)c.ID));
        _controller.ExcludedEnemies = excludedEnemies;

        _controller.UseEnemyClones = UseEnemyClones;
        _controller.EnemyMultiplier = EnemyMultiplier;
        _controller.CloneOriginalEnemies = CloneOriginalEnemies;
        _controller.UseKillableClonePierres = UseKillableClonePierres;

        _controller.RandomizeSecrets = RandomizeSecrets;
        _controller.SecretSeed = SecretSeed;
        _controller.HardSecrets = IsHardSecrets.Value;
        _controller.EnableUWCornerSecrets = EnableUWCornerSecrets.Value;
        _controller.GlitchedSecrets = IsGlitchedSecrets.Value;
        _controller.GuaranteeSecrets = GuaranteeSecrets.Value;
        _controller.SecretRewardMode = SecretRewardMode;
        _controller.UseRewardRoomCameras = UseRewardRoomCameras;
        _controller.UseRandomSecretModels = UseRandomSecretModels.Value;
        _controller.SecretCountMode = SecretCountMode;
        _controller.MinSecretCount = MinSecretCount;
        _controller.MaxSecretCount = MaxSecretCount;
        _controller.UseSecretPack = UseSecretPack;
        _controller.SecretPack = SecretPack;

        _controller.RandomizeTextures = RandomizeTextures;
        _controller.TextureSeed = TextureSeed;
        _controller.TextureMode = TextureMode;
        _controller.MatchTextureTypes = MatchTextureTypes.Value;
        _controller.MatchTextureItems = MatchTextureItems.Value;
        _controller.FilterSourceTextures = FilterSourceTextures;
        _controller.SourceTextureAreas = new(SourceTextureAreas.Where(a => a.Value).Select(a => (TRArea)a.ID));
        _controller.PersistTextures = PersistTextures.Value;
        _controller.RandomizeWaterColour = RandomizeWaterColour.Value;
        _controller.RetainMainLevelTextures = RetainMainLevelTextures.Value;
        _controller.RetainEnemyTextures = RetainEnemyTextures.Value;
        _controller.RetainLaraTextures = RetainLaraTextures.Value;
        _controller.RetainKeySpriteTextures = RetainKeySpriteTextures.Value;
        _controller.RetainSecretSpriteTextures = RetainSecretSpriteTextures.Value;
        _controller.WireframeLevelCount = WireframeLevelCount;
        _controller.AssaultCourseWireframe = AssaultCourseWireframe;
        _controller.UseSolidLaraWireframing = UseSolidLaraWireframing;
        _controller.UseSolidEnemyWireframing = UseSolidEnemyWireframing;
        _controller.UseSolidInteractableWireframing = UseSolidInteractableWireframing;
        _controller.UseDifferentWireframeColours = UseDifferentWireframeColours;
        _controller.UseWireframeLadders = UseWireframeLadders;
        _controller.ShowWireframeTriggers = ShowWireframeTriggers;
        _controller.ShowWireframeTriggerColours = ShowWireframeTriggerColours;

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
        _controller.RandomizeTraps = RandomizeTraps.Value;
        _controller.RandomizeChallengeRooms = RandomizeChallengeRooms.Value;
        _controller.HardEnvironmentMode = HardEnvironmentMode.Value;
        _controller.BlockShortcuts = BlockShortcuts.Value;
        _controller.MirroredLevelCount = MirroredLevelCount;
        _controller.MirrorAssaultCourse = MirrorAssaultCourse;

        _controller.DevelopmentMode = DevelopmentMode;
        _controller.DisableDemos = DisableDemos;
        _controller.AutoLaunchGame = AutoLaunchGame;
        _controller.AddReturnPaths = AddReturnPaths;
        _controller.FixOGBugs = FixOGBugs;

        _controller.SpriteRandoMode = SpriteRandoMode;
        _controller.RandomizeItemSprites = RandomizeItemSprites;
        _controller.RandomizeKeyItemSprites = RandomizeKeyItemSprites;
        _controller.RandomizeSecretSprites = RandomizeSecretSprites;
    }

    public void Unload()
    {
        _controller = null;
    }

    #region Randomizer Type Support
    private static readonly string _supportPropertyFormat = "Is{0}TypeSupported";

    public bool IsTR1 => _controller != null && _controller.IsTR1;
    public bool IsTR2 => _controller != null && _controller.IsTR2;
    public bool IsTR3 => _controller != null && _controller.IsTR3;
    public bool IsTR1Main => IsTR1 && _controller.IsCommunityPatch;
    public bool IsTR3Main => IsTR3 && _controller.IsCommunityPatch;
    public bool IsGameModeTypeSupported => IsRandomizationSupported(TRRandomizerType.GameMode);
    public bool IsLevelSequenceTypeSupported => IsRandomizationSupported(TRRandomizerType.LevelSequence);
    public bool IsLevelCountTypeSupported => IsRandomizationSupported(TRRandomizerType.LevelCount);
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
    public bool IsSecretPackTypeSupported => AvailableSecretPacks?.Length > 0;
    public bool IsSecretRewardTypeSupported => IsRandomizationSupported(TRRandomizerType.SecretReward);
    public bool IsItemTypeSupported => IsRandomizationSupported(TRRandomizerType.Item);
    public bool IsItemDropsTypeSupported => IsRandomizationSupported(TRRandomizerType.ItemDrops);
    public bool IsKeyItemTypeSupported => IsRandomizationSupported(TRRandomizerType.KeyItems);
    public bool IsVehiclesTypeSupported => IsRandomizationSupported(TRRandomizerType.Vehicles);
    public bool IsKeyContinuityTypeSupported => IsRandomizationSupported(TRRandomizerType.KeyContinuity);
    public bool IsExtraPickupsTypeSupported => IsRandomizationSupported(TRRandomizerType.ExtraPickups);
    public bool IsEnemyTypeSupported => IsRandomizationSupported(TRRandomizerType.Enemy);
    public bool IsTextureTypeSupported => IsRandomizationSupported(TRRandomizerType.Texture);
    public bool IsWireframeTypeSupported => IsRandomizationSupported(TRRandomizerType.Wireframe);
    public bool IsTextureSwapTypeSupported => IsRandomizationSupported(TRRandomizerType.TextureSwap);
    public bool IsStartPositionTypeSupported => IsRandomizationSupported(TRRandomizerType.StartPosition);
    public bool IsAudioTypeSupported => IsRandomizationSupported(TRRandomizerType.Audio);
    public bool IsAmbientTracksTypeSupported => IsRandomizationSupported(TRRandomizerType.AmbientTracks);
    public bool IsBlankTracksTypeSupported => IsRandomizationSupported(TRRandomizerType.BlankTracks);
    public bool IsSecretAudioSupported => IsRandomizationSupported(TRRandomizerType.SecretAudio);
    public bool IsSFXTypeSupported => IsRandomizationSupported(TRRandomizerType.SFX);
    public bool IsVFXTypeSupported => IsRandomizationSupported(TRRandomizerType.VFX);
    public bool IsOutfitTypeSupported => IsRandomizationSupported(TRRandomizerType.Outfit);
    public bool IsGymOutfitTypeSupported => IsRandomizationSupported(TRRandomizerType.GymOutfit);
    public bool IsBraidTypeSupported => IsRandomizationSupported(TRRandomizerType.Braid);
    public bool IsOutfitDaggerSupported => IsRandomizationSupported(TRRandomizerType.OutfitDagger);
    public bool IsDynamicTexturesTypeSupported => IsRandomizationSupported(TRRandomizerType.DynamicTextures);
    public bool IsDynamicEnemyTexturesTypeSupported => IsRandomizationSupported(TRRandomizerType.DynamicEnemyTextures);
    public bool IsMeshSwapsTypeSupported => IsRandomizationSupported(TRRandomizerType.MeshSwaps);
    public bool IsTextTypeSupported => IsRandomizationSupported(TRRandomizerType.Text);
    public bool IsEnvironmentTypeSupported => IsRandomizationSupported(TRRandomizerType.Environment);
    public bool IsHardEnvironmentTypeSupported => IsRandomizationSupported(TRRandomizerType.HardEnvironment);
    public bool IsLaddersTypeSupported => IsRandomizationSupported(TRRandomizerType.Ladders);
    public bool IsTrapsTypeSupported => IsRandomizationSupported(TRRandomizerType.Traps);
    public bool IsChallengeRoomsTypeSupported => IsRandomizationSupported(TRRandomizerType.ChallengeRooms);
    public bool IsWeatherTypeSupported => IsRandomizationSupported(TRRandomizerType.Weather);
    public bool IsBirdMonsterBehaviourTypeSupported => IsRandomizationSupported(TRRandomizerType.BirdMonsterBehaviour);
    public bool IsDocileBirdMonsterTypeSupported => IsRandomizationSupported(TRRandomizerType.DocileBirdMonster);
    public bool IsDragonSpawnTypeSupported => IsRandomizationSupported(TRRandomizerType.DragonSpawn);
    public bool IsSecretTexturesTypeSupported => IsRandomizationSupported(TRRandomizerType.SecretTextures);
    public bool IsKeyItemTexturesTypeSupported => IsRandomizationSupported(TRRandomizerType.KeyItemTextures);
    public bool IsWaterColourTypeSupported => IsRandomizationSupported(TRRandomizerType.WaterColour);
    public bool IsAtlanteanEggBehaviourTypeSupported => IsRandomizationSupported(TRRandomizerType.AtlanteanEggBehaviour);
    public bool IsHideDeadTrexesTypeSupported => IsRandomizationSupported(TRRandomizerType.HideDeadTrexes);
    public bool IsHiddenEnemiesTypeSupported => IsRandomizationSupported(TRRandomizerType.HiddenEnemies);
    public bool IsReplaceRequiredEnemyTypeSupported => IsRandomizationSupported(TRRandomizerType.ReplaceRequiredEnemies);
    public bool IsClonedEnemiesTypeSupported => IsRandomizationSupported(TRRandomizerType.ClonedEnemies);
    public bool IsDisableDemosTypeSupported => IsRandomizationSupported(TRRandomizerType.DisableDemos);
    public bool IsItemSpriteTypeSupported => IsRandomizationSupported(TRRandomizerType.ItemSprite);
    public bool IsReturnPathsTypeSupported => IsRandomizationSupported(TRRandomizerType.ReturnPaths);
    public bool IsGeneralBugFixesTypeSupported => IsRandomizationSupported(TRRandomizerType.GeneralBugFixes);

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
        FirePropertyChanged(nameof(IsTR3Main));

        _replaceRequiredEnemies.Description = IsTR1
            ? "Replaces the normally required Larson in Tomb of Qualopec and Torso in Great Pyramid with randomized enemy types."
            : "Replaces the normally required bird monster in Ice Palace and dragon in Dragon's Lair with randomized enemy types.";

        if (IsTR2)
        {
            _randomSecretsControl.Description = "Randomize secret locations. You should expect to find Stone, then Jade, then Gold.";
        }
        else if (IsTR1Main || IsTR3Main)
        {
            _randomSecretsControl.Description = "Randomize secret locations. Artefacts will be added as pickups and either reward rooms created for collecting all secrets, or rewards will be stacked with the secrets.";
        }
        else
        {
            _randomSecretsControl.Description = "Randomize secret locations. Artefacts will be added as pickups and rewards will be stacked with them.";
        }

        FirePropertyChanged(nameof(RandomizeSecretsText));
        AdjustAvailableOptions();
    }

    public void SetAllRandomizationsEnabled(bool enabled)
    {
        if (IsLevelSequenceTypeSupported)
        {
            RandomizeGameMode = enabled;
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
        if (IsWeatherTypeSupported)
        {
            RandomizeWeather = enabled;
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
            result &= RandomizeGameMode;
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
