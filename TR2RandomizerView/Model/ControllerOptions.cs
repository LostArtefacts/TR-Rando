using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using TR2RandomizerCore;
using TR2RandomizerCore.Globalisation;
using TR2RandomizerCore.Helpers;

namespace TR2RandomizerView.Model
{
    public class ControllerOptions : INotifyPropertyChanged
    {
        public int MaxSeedValue => 1000000000;

        private readonly ManagedSeed _secretRewardsControl;
        private readonly ManagedSeedNumeric _levelSequencingControl, _unarmedLevelsControl, _ammolessLevelsControl, _sunsetLevelsControl, _nightLevelsControl;
        private readonly ManagedSeedBool _audioTrackControl;

        private readonly ManagedSeedBool _randomSecretsControl, _randomItemsControl, _randomEnemiesControl, _randomTexturesControl, _randomOutfitsControl, _randomTextControl, _randomStartControl, _randomEnvironmentControl;

        private bool _disableDemos, _autoLaunchGame;

        private BoolItemControlClass _isHardSecrets, _allowGlitched;
        private BoolItemControlClass _includeKeyItems;
        private BoolItemControlClass _crossLevelEnemies, _protectMonks, _docileBirdMonsters;
        private BoolItemControlClass _persistTextures, _retainKeySpriteTextures, _retainSecretSpriteTextures;
        private BoolItemControlClass _includeBlankTracks, _changeTriggerTracks;
        private BoolItemControlClass _persistOutfits, _removeRobeDagger;
        private BoolItemControlClass _retainKeyItemNames, _retainLevelNames;
        private BoolItemControlClass _rotateStartPosition;
        private BoolItemControlClass _randomizeWaterLevels, _randomizeSlotPositions, _randomizeLadders;
        private uint _mirroredLevelCount;
        private bool _mirrorAssaultCourse;
        private uint _haircutLevelCount;
        private bool _assaultCourseHaircut;
        private uint _invisibleLevelCount;
        private bool _assaultCourseInvisible;
        private uint _nightModeDarkness;
        private uint _nightModeDarknessMaximum;
        private bool _nightModeAssaultCourse;

        private List<BoolItemControlClass> _secretBoolItemControls, _itemBoolItemControls, _enemyBoolItemControls, _textureBoolItemControls, _audioBoolItemControls, _outfitBoolItemControls, _textBoolItemControls, _startBoolItemControls, _environmentBoolItemControls;

        private RandoDifficulty _randoEnemyDifficulty;
        private ItemDifficulty _randoItemDifficulty;

        private Language[] _availableLanguages;
        private Language _gameStringLanguage;

        private int _levelCount, _maximumLevelCount;

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
        }

        public bool RandomizationPossible
        {
            get => RandomizeLevelSequencing || RandomizeUnarmedLevels || RandomizeAmmolessLevels || RandomizeSecretRewards || RandomizeSunsets ||
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

        public BoolItemControlClass DocileBirdMonsters
        {
            get => _docileBirdMonsters;
            set
            {
                _docileBirdMonsters = value;
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

        public bool AutoLaunchGame
        {
            get => _autoLaunchGame;
            set
            {
                _autoLaunchGame = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private TR2RandomizerController _controller;

        public ControllerOptions()
        {
            _levelSequencingControl = new ManagedSeedNumeric();
            _unarmedLevelsControl = new ManagedSeedNumeric();
            _ammolessLevelsControl = new ManagedSeedNumeric();
            _secretRewardsControl = new ManagedSeed();
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
                Description = "Some hard secrets may require glitches."
            };
            BindingOperations.SetBinding(IsHardSecrets, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);
            IsGlitchedSecrets = new BoolItemControlClass()
            {
                Title = "Enable glitched secrets",
                Description = null
            };
            BindingOperations.SetBinding(IsGlitchedSecrets, BoolItemControlClass.IsActiveProperty, randomizeSecretsBinding);

            // Items
            Binding randomizeItemsBinding = new Binding(nameof(RandomizeItems)) { Source = this };
            IncludeKeyItems = new BoolItemControlClass()
            {
                Title = "Enable key items",
                Description = "Most key items will be randomized. Keys will spawn before their respective locks."
            };
            BindingOperations.SetBinding(IncludeKeyItems, BoolItemControlClass.IsActiveProperty, randomizeItemsBinding);

            // Enemies
            Binding randomizeEnemiesBinding = new Binding(nameof(RandomizeEnemies)) { Source = this };
            CrossLevelEnemies = new BoolItemControlClass()
            {
                Title = "Enable cross-level enemies",
                Description = "Allow enemy types to appear in any level."
            };
            BindingOperations.SetBinding(CrossLevelEnemies, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
            DocileBirdMonsters = new BoolItemControlClass()
            {
                Title = "Enable docile bird monsters",
                Description = "Randomized bird monsters will not initiate on Lara and will not end the level upon death."
            };
            BindingOperations.SetBinding(DocileBirdMonsters, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);
            ProtectMonks = new BoolItemControlClass()
            {
                Title = "Avoid having to kill monks",
                Description = "Monks will not be given pickups and will not appear at the end of Diving Area."
            };
            BindingOperations.SetBinding(ProtectMonks, BoolItemControlClass.IsActiveProperty, randomizeEnemiesBinding);

            // Textures
            Binding randomizeTexturesBinding = new Binding(nameof(RandomizeTextures)) { Source = this };
            PersistTextures = new BoolItemControlClass()
            {
                Title = "Use persistent textures",
                Description = "Each unique texture will only be randomized once, rather than once per level."
            };
            BindingOperations.SetBinding(PersistTextures, BoolItemControlClass.IsActiveProperty, randomizeTexturesBinding);
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
            IncludeBlankTracks = new BoolItemControlClass()
            {
                Title = "Include blank tracks",
                Description = "Applies only to the title screen and level ambience tracks"
            };
            BindingOperations.SetBinding(IncludeBlankTracks, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);
            ChangeTriggerTracks = new BoolItemControlClass()
            {
                Title = "Change trigger tracks",
                Description = "Change the tracks in the game that play when crossing triggers, such as the violins in Venice, danger sounds etc."
            };
            BindingOperations.SetBinding(ChangeTriggerTracks, BoolItemControlClass.IsActiveProperty, randomizeAudioBinding);

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

            // all item controls
            SecretBoolItemControls = new List<BoolItemControlClass>()
            {
                _isHardSecrets, _allowGlitched,
            };
            ItemBoolItemControls = new List<BoolItemControlClass>()
            {
                _includeKeyItems,
            };
            EnemyBoolItemControls = new List<BoolItemControlClass>()
            {
                _crossLevelEnemies, _docileBirdMonsters, _protectMonks,
            };
            TextureBoolItemControls = new List<BoolItemControlClass>()
            {
                _persistTextures, _retainKeySpriteTextures, _retainSecretSpriteTextures
            };
            AudioBoolItemControls = new List<BoolItemControlClass>()
            {
                _includeBlankTracks, _changeTriggerTracks
            };
            OutfitBoolItemControls = new List<BoolItemControlClass>()
            {
                _persistOutfits, _removeRobeDagger
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
        }

        public void Load(TR2RandomizerController controller)
        {
            _controller = controller;

            TotalLevelCount = _controller.LevelCount;

            RandomizeLevelSequencing = _controller.RandomizeLevelSequencing;
            LevelSequencingSeed = _controller.LevelSequencingSeed;
            PlayableLevelCount = _controller.PlayableLevelCount;

            RandomizeUnarmedLevels = _controller.RandomizeUnarmedLevels;
            UnarmedLevelsSeed = _controller.UnarmedLevelsSeed;
            UnarmedLevelCount = _controller.UnarmedLevelCount;

            RandomizeAmmolessLevels = _controller.RandomizeAmmolessLevels;
            AmmolessLevelsSeed = _controller.AmmolessLevelsSeed;
            AmmolessLevelCount = _controller.AmmolessLevelCount;

            RandomizeSecretRewards = _controller.RandomizeSecretRewards;
            SecretRewardSeed = _controller.SecretRewardSeed;

            RandomizeSunsets = _controller.RandomizeSunsets;
            SunsetsSeed = _controller.SunsetsSeed;
            SunsetCount = _controller.SunsetCount;

            RandomizeNightMode = _controller.RandomizeNightMode;
            NightModeSeed = _controller.NightModeSeed;
            NightModeCount = _controller.NightModeCount;
            NightModeAssaultCourse = _controller.NightModeAssaultCourse;
            NightModeDarkness = _controller.NightModeDarkness;
            NightModeDarknessMaximum = _controller.NightModeDarknessRange;

            RandomizeAudioTracks = _controller.RandomizeAudioTracks;
            AudioTracksSeed = _controller.AudioTracksSeed;
            IncludeBlankTracks.Value = _controller.RandomGameTracksIncludeBlank;
            ChangeTriggerTracks.Value = _controller.ChangeTriggerTracks;

            RandomizeItems = _controller.RandomizeItems;
            ItemSeed = _controller.ItemSeed;
            IncludeKeyItems.Value = _controller.IncludeKeyItems;
            RandoItemDifficulty = _controller.RandoItemDifficulty;

            RandomizeEnemies = _controller.RandomizeEnemies;
            EnemySeed = _controller.EnemySeed;
            CrossLevelEnemies.Value = _controller.CrossLevelEnemies;
            ProtectMonks.Value = _controller.ProtectMonks;
            DocileBirdMonsters.Value = _controller.DocileBirdMonsters;
            RandoEnemyDifficulty = _controller.RandoEnemyDifficulty;

            RandomizeSecrets = _controller.RandomizeSecrets;
            SecretSeed = _controller.SecretSeed;
            IsHardSecrets.Value = _controller.HardSecrets;
            IsGlitchedSecrets.Value = _controller.GlitchedSecrets;

            RandomizeTextures = _controller.RandomizeTextures;
            TextureSeed = _controller.TextureSeed;
            PersistTextures.Value = _controller.PersistTextures;
            RetainKeySpriteTextures.Value = _controller.RetainKeySpriteTextures;
            RetainSecretSpriteTextures.Value = _controller.RetainSecretSpriteTextures;

            RandomizeOutfits = _controller.RandomizeOutfits;
            OutfitSeed = _controller.OutfitSeed;
            PersistOutfits.Value = _controller.PersistOutfits;
            RemoveRobeDagger.Value = _controller.RemoveRobeDagger;
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
        }

        public void RandomizeActiveSeeds()
        {
            Random rng = new Random();
            if (RandomizeLevelSequencing)
            {
                LevelSequencingSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeUnarmedLevels)
            {
                UnarmedLevelsSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeAmmolessLevels)
            {
                AmmolessLevelsSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeSecretRewards)
            {
                SecretRewardSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeSunsets)
            {
                SunsetsSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeNightMode)
            {
                NightModeSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeAudioTracks)
            {
                AudioTracksSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeItems)
            {
                ItemSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeEnemies)
            {
                EnemySeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeSecrets)
            {
                SecretSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeTextures)
            {
                TextureSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeOutfits)
            {
                OutfitSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeText)
            {
                TextSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeStartPosition)
            {
                StartPositionSeed = rng.Next(1, MaxSeedValue);
            }
            if (RandomizeEnvironment)
            {
                EnvironmentSeed = rng.Next(1, MaxSeedValue);
            }
        }

        public void SetGlobalSeed(int seed)
        {
            if (RandomizeLevelSequencing)
            {
                LevelSequencingSeed = seed;
            }
            if (RandomizeUnarmedLevels)
            {
                UnarmedLevelsSeed = seed;
            }
            if (RandomizeAmmolessLevels)
            {
                AmmolessLevelsSeed = seed;
            }
            if (RandomizeSecretRewards)
            {
                SecretRewardSeed = seed;
            }
            if (RandomizeSunsets)
            {
                SunsetsSeed = seed;
            }
            if (RandomizeNightMode)
            {
                NightModeSeed = seed;
            }
            if (RandomizeAudioTracks)
            {
                AudioTracksSeed = seed;
            }
            if (RandomizeItems)
            {
                ItemSeed = seed;
            }
            if (RandomizeEnemies)
            {
                EnemySeed = seed;
            }
            if (RandomizeSecrets)
            {
                SecretSeed = seed;
            }
            if (RandomizeTextures)
            {
                TextureSeed = seed;
            }
            if (RandomizeOutfits)
            {
                OutfitSeed = seed;
            }
            if (RandomizeText)
            {
                TextSeed = seed;
            }
            if (RandomizeStartPosition)
            {
                StartPositionSeed = seed;
            }
            if (RandomizeEnvironment)
            {
                EnvironmentSeed = seed;
            }
        }

        public void SetAllRandomizationsEnabled(bool enabled)
        {
            RandomizeLevelSequencing = RandomizeUnarmedLevels = RandomizeAmmolessLevels =
                RandomizeSecretRewards = RandomizeSunsets = RandomizeNightMode = RandomizeAudioTracks =
                RandomizeItems = RandomizeSecrets = RandomizeEnemies = RandomizeTextures = RandomizeOutfits = 
                RandomizeText = RandomizeStartPosition = RandomizeEnvironment = enabled;
        }

        public bool AllRandomizationsEnabled()
        {
            return RandomizeLevelSequencing && RandomizeUnarmedLevels && RandomizeAmmolessLevels &&
                RandomizeSecretRewards && RandomizeSunsets && RandomizeNightMode && RandomizeAudioTracks &&
                RandomizeItems && RandomizeSecrets && RandomizeEnemies && RandomizeTextures && RandomizeOutfits && 
                RandomizeText && RandomizeStartPosition && RandomizeEnvironment;
        }

        public void Save()
        {
            _controller.RandomizeLevelSequencing = RandomizeLevelSequencing;
            _controller.LevelSequencingSeed = LevelSequencingSeed;

            // While this can be separated into its own option, for now it's combined with sequencing
            _controller.RandomizePlayableLevels = RandomizeLevelSequencing;
            _controller.PlayableLevelsSeed = LevelSequencingSeed;
            _controller.PlayableLevelCount = PlayableLevelCount;

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

            _controller.RandomizeSunsets = RandomizeSunsets;
            _controller.SunsetsSeed = SunsetsSeed;
            _controller.SunsetCount = SunsetCount;

            _controller.RandomizeNightMode = RandomizeNightMode;
            _controller.NightModeSeed = NightModeSeed;
            _controller.NightModeCount = NightModeCount;
            _controller.NightModeAssaultCourse = NightModeAssaultCourse;
            _controller.NightModeDarkness = NightModeDarkness;

            _controller.RandomizeAudioTracks = RandomizeAudioTracks;
            _controller.AudioTracksSeed = AudioTracksSeed;
            _controller.RandomGameTracksIncludeBlank = IncludeBlankTracks.Value;
            _controller.ChangeTriggerTracks = ChangeTriggerTracks.Value;

            _controller.RandomizeItems = RandomizeItems;
            _controller.ItemSeed = ItemSeed;
            _controller.IncludeKeyItems = IncludeKeyItems.Value;
            _controller.RandoItemDifficulty = RandoItemDifficulty;

            _controller.RandomizeEnemies = RandomizeEnemies;
            _controller.EnemySeed = EnemySeed;
            _controller.CrossLevelEnemies = CrossLevelEnemies.Value;
            _controller.ProtectMonks = ProtectMonks.Value;
            _controller.DocileBirdMonsters = DocileBirdMonsters.Value;
            _controller.RandoEnemyDifficulty = RandoEnemyDifficulty;

            _controller.RandomizeSecrets = RandomizeSecrets;
            _controller.SecretSeed = SecretSeed;
            _controller.HardSecrets = IsHardSecrets.Value;
            _controller.GlitchedSecrets = IsGlitchedSecrets.Value;

            _controller.RandomizeTextures = RandomizeTextures;
            _controller.TextureSeed = TextureSeed;
            _controller.PersistTextures = PersistTextures.Value;
            _controller.RetainKeySpriteTextures = RetainKeySpriteTextures.Value;
            _controller.RetainSecretSpriteTextures = RetainSecretSpriteTextures.Value;

            _controller.RandomizeOutfits = RandomizeOutfits;
            _controller.OutfitSeed = OutfitSeed;
            _controller.PersistOutfits = PersistOutfits.Value;
            _controller.RemoveRobeDagger = RemoveRobeDagger.Value;
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
        }

        public void Unload()
        {
            _controller = null;
        }
    }
}
