using System;
using System.Collections.Generic;
using TRGE.Core;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Secrets;

namespace TRRandomizerView.Model
{
    public class OptionGenerator
    {
        private const double _boolChance = 0.5;

        private readonly ControllerOptions _options;
        private readonly Random _generator;

        public OptionGenerator(ControllerOptions options)
        {
            _options = options;
            _generator = new Random();
        }

        public void RandomizeActiveSeeds()
        {
            SetSeeds(-1);
        }

        public void SetSeeds(int seed)
        {
            if (_options.RandomizeLevelSequencing)
            {
                _options.LevelSequencingSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeUnarmedLevels)
            {
                _options.UnarmedLevelsSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeAmmolessLevels)
            {
                _options.AmmolessLevelsSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeHealth)
            {
                _options.HealthSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeSecretRewards)
            {
                _options.SecretRewardSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeSunsets)
            {
                _options.SunsetsSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeNightMode)
            {
                _options.NightModeSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeAudioTracks)
            {
                _options.AudioTracksSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeItems)
            {
                _options.ItemSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeEnemies)
            {
                _options.EnemySeed = InterpretSeed(seed);
            }
            if (_options.RandomizeSecrets)
            {
                _options.SecretSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeTextures)
            {
                _options.TextureSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeOutfits)
            {
                _options.OutfitSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeText)
            {
                _options.TextSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeStartPosition)
            {
                _options.StartPositionSeed = InterpretSeed(seed);
            }
            if (_options.RandomizeEnvironment)
            {
                _options.EnvironmentSeed = InterpretSeed(seed);
            }
        }

        public void RandomizeActiveOptions()
        {
            if (_options.RandomizeLevelSequencing)
            {
                RandomizeLevelSequencing_options();
            }
            if (_options.RandomizeUnarmedLevels)
            {
                RandomizeUnarmedOptions();
            }
            if (_options.RandomizeAmmolessLevels)
            {
                RandomizeAmmolessOptions();
            }
            if (_options.RandomizeHealth)
            {
                RandomizeHealthOptions();
            }
            if (_options.RandomizeSecretRewards)
            {
                RandomizeSecretRewardOptions();
            }
            if (_options.RandomizeSunsets)
            {
                RandomizeSunsetOptions();
            }
            if (_options.RandomizeNightMode)
            {
                RandomizeNightModeOptions();
            }
            if (_options.RandomizeAudioTracks)
            {
                RandomizeAudioOptions();
            }
            if (_options.RandomizeItems)
            {
                RandomizeItemOptions();
            }
            if (_options.RandomizeEnemies)
            {
                RandomizeEnemyOptions();
            }
            if (_options.RandomizeSecrets)
            {
                RandomizeSecretOptions();
            }
            if (_options.RandomizeTextures)
            {
                RandomizeTextureOptions();
            }
            if (_options.RandomizeOutfits)
            {
                RandomizeOutfitOptions();
            }
            if (_options.RandomizeText)
            {
                RandomizeTextOptions();
            }
            if (_options.RandomizeStartPosition)
            {
                RandomizeStartPositionOptions();
            }
            if (_options.RandomizeEnvironment)
            {
                RandomizeEnvironmentOptions();
            }
            if (_options.IsTR1Main)
            {
                RandomizeT1MOptions();
            }
        }

        private void RandomizeLevelSequencing_options()
        {
            _options.PlayableLevelCount = GetRandomUInt(1, _options.TotalLevelCount);
            if (_options.IsGlobeDisplayTypeSupported)
            {
                _options.GlobeDisplay = GetRandomEnumValue<GlobeDisplayOption>(typeof(GlobeDisplayOption));
            }
        }

        private void RandomizeUnarmedOptions()
        {
            _options.UnarmedLevelCount = GetRandomUInt(0, _options.MaximumLevelCount);
        }

        private void RandomizeAmmolessOptions()
        {
            _options.AmmolessLevelCount = GetRandomUInt(0, _options.MaximumLevelCount);
        }

        private void RandomizeHealthOptions()
        {
            RandomizeBoolItems(_options.HealthBoolItemControls);
            _options.MedilessLevelCount = GetRandomUInt(0, _options.MaximumLevelCount);
            _options.MinStartingHealth = GetRandomUInt(1, 1000);
            _options.MaxStartingHealth = GetRandomUInt((int)_options.MinStartingHealth, 1000);
        }

        private void RandomizeSecretRewardOptions()
        {
            // None currently
        }

        private void RandomizeSunsetOptions()
        {
            _options.SunsetCount = GetRandomUInt(0, _options.MaximumLevelCount);
        }

        private void RandomizeNightModeOptions()
        {
            _options.NightModeCount = GetRandomUInt(0, _options.MaximumLevelCount);
            _options.NightModeAssaultCourse = GetRandomBool();
            _options.NightModeDarkness = GetRandomUInt(0, (int)_options.NightModeDarknessMaximum);
            if (_options.IsSunsetTypeSupported)
            {
                _options.OverrideSunsets = GetRandomBool();
            }

            if (_options.IsVFXTypeSupported)
            {
                _options.VfxRandomize = GetRandomBool();
                _options.VfxFilterColor = GetRandomArrayMember(_options.VfxAvailColors);
                _options.VfxVivid = GetRandomBool();
                _options.VfxLevel = GetRandomBool();
                _options.VfxRoom = GetRandomBool();
                _options.VfxCaustics = GetRandomBool();
                _options.VfxWave = GetRandomBool();
            }
        }

        private void RandomizeAudioOptions()
        {
            RandomizeBoolItems(_options.AudioBoolItemControls);
            _options.UncontrolledSFXCount = GetRandomUInt(0, _options.MaximumLevelCount);
            _options.UncontrolledSFXAssaultCourse = GetRandomBool();
        }

        private void RandomizeItemOptions()
        {
            RandomizeBoolItems(_options.ItemBoolItemControls);
            _options.RandoItemDifficulty = GetRandomEnumValue<ItemDifficulty>(typeof(ItemDifficulty));
            if (_options.IsItemSpriteTypeSupported)
            {
                if (_options.RandomizeItemSprites = GetRandomBool())
                {
                    _options.RandomizeKeyItemSprites = GetRandomBool();
                    _options.RandomizeSecretSprites = GetRandomBool();
                    _options.SpriteRandoMode = GetRandomEnumValue<SpriteRandoMode>(typeof(SpriteRandoMode));
                }
            }
        }

        private void RandomizeEnemyOptions()
        {
            RandomizeBoolItems(_options.EnemyBoolItemControls);
            do
            {
                _options.RandoEnemyDifficulty = GetRandomEnumValue<RandoDifficulty>(typeof(RandoDifficulty));
            }
            while (_options.RandoEnemyDifficulty == RandoDifficulty.DefaultOrNoRestrictions); // Used internally only and is not a UI option
            if (_options.IsBirdMonsterBehaviourTypeSupported)
            {
                _options.BirdMonsterBehaviour = GetRandomEnumValue<BirdMonsterBehaviour>(typeof(BirdMonsterBehaviour));
            }
            if (_options.IsDragonSpawnTypeSupported)
            {
                _options.DragonSpawnType = GetRandomEnumValue<DragonSpawnType>(typeof(DragonSpawnType));
            }
            if (_options.UseEnemyExclusions = GetRandomBool())
            {
                int inclusionCount = GetRandomInt(1, _options.SelectableEnemyControls.Count - 1);
                List<BoolItemIDControlClass> includedEnemies = _options.SelectableEnemyControls.RandomSelection(_generator, inclusionCount);
                _options.SelectableEnemyControls.ForEach(c => c.Value = includedEnemies.Contains(c));
            }
        }

        private void RandomizeSecretOptions()
        {
            RandomizeBoolItems(_options.SecretBoolItemControls);
            if (_options.IsSecretCountTypeSupported)
            {
                _options.SecretCountMode = GetRandomEnumValue<TRSecretCountMode>(typeof(TRSecretCountMode));
                if (_options.SecretCountMode == TRSecretCountMode.Customized)
                {
                    _options.MinSecretCount = GetRandomUInt(1, (int)_options.MaxSecretCount);
                    _options.MaxSecretCount = GetRandomUInt((int)_options.MinSecretCount, (int)_options.MaxSecretCount);
                }
            }
        }

        private void RandomizeTextureOptions()
        {
            RandomizeBoolItems(_options.TextureBoolItemControls);
            _options.WireframeLevelCount = GetRandomUInt(0, _options.MaximumLevelCount);
            _options.AssaultCourseWireframe = GetRandomBool();
            _options.UseSolidLaraWireframing = GetRandomBool();
            _options.UseSolidEnemyWireframing = GetRandomBool();
            _options.UseDifferentWireframeColours = GetRandomBool();
            if (_options.IsLaddersTypeSupported)
            {
                _options.UseWireframeLadders = GetRandomBool();
            }
        }

        private void RandomizeOutfitOptions()
        {
            RandomizeBoolItems(_options.OutfitBoolItemControls);
            _options.HaircutLevelCount = GetRandomUInt(0, _options.MaximumLevelCount);
            _options.AssaultCourseHaircut = GetRandomBool();
            _options.InvisibleLevelCount = GetRandomUInt(0, _options.MaximumLevelCount);
            _options.AssaultCourseInvisible = GetRandomBool();
        }

        private void RandomizeTextOptions()
        {
            RandomizeBoolItems(_options.TextBoolItemControls);
            _options.GameStringLanguage = GetRandomArrayMember(_options.AvailableLanguages);
        }

        private void RandomizeStartPositionOptions()
        {
            RandomizeBoolItems(_options.StartBoolItemControls);
        }

        private void RandomizeEnvironmentOptions()
        {
            RandomizeBoolItems(_options.EnvironmentBoolItemControls);
            _options.MirroredLevelCount = GetRandomUInt(0, _options.MaximumLevelCount);
            _options.MirrorAssaultCourse = GetRandomBool();
        }

        private void RandomizeT1MOptions()
        {
            _options.AirbarColor = GetRandomEnumValue<TRUIColour>(typeof(TRUIColour));
            _options.AirbarLocation = GetRandomEnumValue<TRUILocation>(typeof(TRUILocation));
            _options.AirbarShowingMode = GetRandomEnumValue<TRAirbarMode>(typeof(TRAirbarMode));
            _options.EnableCine = GetRandomBool();
            _options.EnableFmv = GetRandomBool();
            _options.DisableMagnums = GetRandomBool();
            _options.DisableShotgun = GetRandomBool();
            _options.DisableUzis = GetRandomBool();
            _options.EnableEnemyHealthbar = GetRandomBool();
            _options.EnablePitchedSounds = GetRandomBool();
            _options.EnableSaveCrystals = GetRandomBool();
            _options.EnemyHealthbarColor = GetRandomEnumValue<TRUIColour>(typeof(TRUIColour));
            _options.EnemyHealthbarLocation = GetRandomEnumValue<TRUILocation>(typeof(TRUILocation));
            _options.HealthbarColor = GetRandomEnumValue<TRUIColour>(typeof(TRUIColour));
            _options.HealthbarLocation = GetRandomEnumValue<TRUILocation>(typeof(TRUILocation));
            _options.HealthbarShowingMode = GetRandomEnumValue<TRHealthbarMode>(typeof(TRHealthbarMode));
            _options.MaximumSaveSlots = GetRandomInt(1, 100);
            _options.MenuStyle = GetRandomEnumValue<TRMenuStyle>(typeof(TRMenuStyle));
            _options.RevertToPistols = GetRandomBool();
            _options.WalkToItems = GetRandomBool();
            _options.WaterColorB = GetRandomDouble(30, 100, 2);
            _options.WaterColorG = GetRandomDouble(30, 100, 2);
            _options.WaterColorR = GetRandomDouble(30, 100, 2);
        }

        private int InterpretSeed(int seed)
        {
            return seed < 0 ? GetRandomSeed() : seed;
        }

        private int GetRandomSeed()
        {
            return GetRandomInt(1, _options.MaxSeedValue);
        }

        private int GetRandomInt(int min, int max)
        {
            return _generator.Next(min, max + 1);
        }

        private uint GetRandomUInt(int min, int max)
        {
            return (uint)_generator.Next(min, max + 1);
        }

        private double GetRandomDouble(int min, int max, int decimalPlaces)
        {
            if (min < 0 || max > 100)
            {
                throw new ArgumentException();
            }
            return Math.Round(_generator.Next(min, max + 1) * Math.Pow(10, -2), decimalPlaces);
        }

        private bool GetRandomBool()
        {
            return _generator.NextDouble() < _boolChance;
        }

        private void RandomizeBoolItems(IEnumerable<BoolItemControlClass> boolItems)
        {
            foreach (BoolItemControlClass boolItem in boolItems)
            {
                if (boolItem.IsAvailable)
                {
                    boolItem.Value = GetRandomBool();
                }
            }
        }

        private T GetRandomEnumValue<T>(Type enumType)
        {
            Array values = Enum.GetValues(enumType);
            return (T)values.GetValue(_generator.Next(0, values.Length));
        }

        private T GetRandomArrayMember<T>(T[] array)
        {
            return array[_generator.Next(0, array.Length)];
        }
    }
}