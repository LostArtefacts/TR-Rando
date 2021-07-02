using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TR2RandomizerCore;

namespace TR2RandomizerView.Model
{
    public class ControllerOptions : INotifyPropertyChanged
    {
        public int MaxSeedValue => 1000000000;

        private readonly ManagedSeed _secretRewardsControl;
        private readonly ManagedSeedNumeric _levelSequencingControl, _unarmedLevelsControl, _ammolessLevelsControl, _sunsetLevelsControl;
        private readonly ManagedSeedBool _audioTrackControl;

        private readonly ManagedSeedBool _randomSecretsControl, _randomItemsControl, _randomEnemiesControl, _randomTexturesControl, _randomOutfitsControl;

        private bool _disableDemos, _protectMonks, _allowGlitched, _docileBirdMonsters, _retainKeySpriteTextures, _autoLaunchGame;

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
        }

        public bool RandomizationPossible
        {
            get => RandomizeLevelSequencing || RandomizeUnarmedLevels || RandomizeAmmolessLevels || RandomizeSecretRewards || RandomizeSunsets ||
                   RandomizeAudioTracks || RandomizeItems || RandomizeEnemies || RandomizeSecrets || RandomizeTextures || RandomizeOutfits;
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

        public bool RandomAudioTracksIncludeBlank
        {
            get => _audioTrackControl.CustomBool;
            set
            {
                _audioTrackControl.CustomBool = value;
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

        public bool HardSecrets
        {
            get => _randomSecretsControl.CustomBool;
            set
            {
                _randomSecretsControl.CustomBool = value;
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

        public bool IncludeKeyItems
        {
            get => _randomItemsControl.CustomBool;
            set
            {
                _randomItemsControl.CustomBool = value;
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

        public bool CrossLevelEnemies
        {
            get => _randomEnemiesControl.CustomBool;
            set
            {
                _randomEnemiesControl.CustomBool = value;
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

        public bool PersistTextures
        {
            get => _randomTexturesControl.CustomBool;
            set
            {
                _randomTexturesControl.CustomBool = value;
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

        public bool PersistOutfits
        {
            get => _randomOutfitsControl.CustomBool;
            set
            {
                _randomOutfitsControl.CustomBool = value;
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

        public bool ProtectMonks
        {
            get => _protectMonks;
            set
            {
                _protectMonks = value;
                FirePropertyChanged();
            }
        }

        public bool GlitchedSecrets
        {
            get => _allowGlitched;
            set
            {
                _allowGlitched = value;
                FirePropertyChanged();
            }
        }

        public bool DocileBirdMonsters
        {
            get => _docileBirdMonsters;
            set
            {
                _docileBirdMonsters = value;
                FirePropertyChanged();
            }
        }

        public bool RetainKeySpriteTextures
        {
            get => _retainKeySpriteTextures;
            set
            {
                _retainKeySpriteTextures = value;
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
            _audioTrackControl = new ManagedSeedBool();

            _randomItemsControl = new ManagedSeedBool();
            _randomEnemiesControl = new ManagedSeedBool();
            _randomSecretsControl = new ManagedSeedBool();
            _randomTexturesControl = new ManagedSeedBool();
            _randomOutfitsControl = new ManagedSeedBool();
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

            RandomizeAudioTracks = _controller.RandomizeAudioTracks;
            AudioTracksSeed = _controller.AudioTracksSeed;
            RandomAudioTracksIncludeBlank = _controller.RandomGameTracksIncludeBlank;

            RandomizeItems = _controller.RandomizeItems;
            ItemSeed = _controller.ItemSeed;
            IncludeKeyItems = _controller.IncludeKeyItems;

            RandomizeEnemies = _controller.RandomizeEnemies;
            EnemySeed = _controller.EnemySeed;
            CrossLevelEnemies = _controller.CrossLevelEnemies;
            ProtectMonks = _controller.ProtectMonks;
            DocileBirdMonsters = _controller.DocileBirdMonsters;

            RandomizeSecrets = _controller.RandomizeSecrets;
            SecretSeed = _controller.SecretSeed;
            HardSecrets = _controller.HardSecrets;
            GlitchedSecrets = _controller.GlitchedSecrets;

            RandomizeTextures = _controller.RandomizeTextures;
            TextureSeed = _controller.TextureSeed;
            PersistTextures = _controller.PersistTextures;
            RetainKeySpriteTextures = _controller.RetainKeySpriteTextures;

            RandomizeOutfits = _controller.RandomizeOutfits;
            OutfitSeed = _controller.OutfitSeed;
            PersistOutfits = _controller.PersistOutfits;

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
        }

        public void SetAllRandomizationsEnabled(bool enabled)
        {
            RandomizeLevelSequencing = RandomizeUnarmedLevels = RandomizeAmmolessLevels =
                RandomizeSecretRewards = RandomizeSunsets = RandomizeAudioTracks =
                RandomizeItems = RandomizeSecrets = RandomizeEnemies = RandomizeTextures = RandomizeOutfits = enabled;
        }

        public bool AllRandomizationsEnabled()
        {
            return RandomizeLevelSequencing && RandomizeUnarmedLevels && RandomizeAmmolessLevels &&
                RandomizeSecretRewards && RandomizeSunsets && RandomizeAudioTracks &&
                RandomizeItems && RandomizeSecrets && RandomizeEnemies && RandomizeTextures && RandomizeOutfits;
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

            _controller.RandomizeAudioTracks = RandomizeAudioTracks;
            _controller.AudioTracksSeed = AudioTracksSeed;
            _controller.RandomGameTracksIncludeBlank = RandomAudioTracksIncludeBlank;

            _controller.RandomizeItems = RandomizeItems;
            _controller.ItemSeed = ItemSeed;
            _controller.IncludeKeyItems = IncludeKeyItems;

            _controller.RandomizeEnemies = RandomizeEnemies;
            _controller.EnemySeed = EnemySeed;
            _controller.CrossLevelEnemies = CrossLevelEnemies;
            _controller.ProtectMonks = ProtectMonks;
            _controller.DocileBirdMonsters = DocileBirdMonsters;

            _controller.RandomizeSecrets = RandomizeSecrets;
            _controller.SecretSeed = SecretSeed;
            _controller.HardSecrets = HardSecrets;
            _controller.GlitchedSecrets = GlitchedSecrets;

            _controller.RandomizeTextures = RandomizeTextures;
            _controller.TextureSeed = TextureSeed;
            _controller.PersistTextures = PersistTextures;
            _controller.RetainKeySpriteTextures = RetainKeySpriteTextures;

            _controller.RandomizeOutfits = RandomizeOutfits;
            _controller.OutfitSeed = OutfitSeed;
            _controller.PersistOutfits = PersistOutfits;

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