using System;
using System.IO;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Globalisation;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Randomizers;
using TRGE.Coord;
using TRGE.Core;
using System.Collections.Generic;
using System.Drawing;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore
{
    public class TRRandomizerController
    {
        private readonly TREditor _editor;
        private readonly TRVersionSupport _support;

        internal AbstractTRScriptEditor ScriptEditor => _editor.ScriptEditor;
        internal RandomizerSettings LevelRandomizer => (_editor.LevelEditor as ISettingsProvider).Settings;

        internal TRRandomizerController(string directoryPath)
        {
            // If there is a checksum mismatch, we will just ignore the previous backup and open the folder afresh
            _editor = TRCoord.Instance.Open(directoryPath, TRScriptOpenOption.DiscardBackup);
            _editor.SaveProgressChanged += Editor_SaveProgressChanged;
            _editor.RestoreProgressChanged += Editor_RestoreProgressChanged;
            StoreExternalOrganisations();

            _support = new TRVersionSupport();
            if (!IsRandomizationSupported())
            {
                throw new NotSupportedException(string.Format("Randomization of {0} is not currently supported.", EditionTitle));
            }
        }

        #region Version Support
        public bool IsRandomizationSupported()
        {
            return _support.IsRandomizationSupported(_editor.Edition);
        }

        public bool IsRandomizationSupported(TRRandomizerType randomizerType)
        {
            return _support.IsRandomizationSupported(_editor.Edition, randomizerType);
        }

        public List<string> GetExecutables()
        {
            return _support.GetExecutables(_editor.Edition);
        }

        public bool IsTR1 => _editor.Edition.Version == TRVersion.TR1;
        public bool IsTR2 => _editor.Edition.Version == TRVersion.TR2;
        public bool IsTR3 => _editor.Edition.Version == TRVersion.TR3;
        #endregion

        #region ScriptEditor Passthrough
        private Organisation _extLevelOrganisation, _extPlayableOrganisation, _extUnarmedOrganisation, _extAmmolessOrganisation, _extMedilessOrganisation, _extSecretRewardsOrganisation, _extSunsetOrganisation, _extAudioOrganisation;

        /// <summary>
        /// We need to store any organisation values that aren't random so if randomization is turned off for the 
        /// corresponding functions, they will be reverted to what was previously set externally. If any haven't
        /// been set externally, turning them off will result in standard organisation.
        /// </summary>
        private void StoreExternalOrganisations()
        {
            _extLevelOrganisation = RandomizeLevelSequencing ? Organisation.Default : ScriptEditor.LevelSequencingOrganisation;
            _extPlayableOrganisation = RandomizePlayableLevels ? Organisation.Default : ScriptEditor.EnabledLevelOrganisation;
            _extSunsetOrganisation = RandomizeSunsets ? Organisation.Default : ScriptEditor.LevelSunsetOrganisation;
            _extAudioOrganisation = RandomizeAudioTracks ? Organisation.Default : ScriptEditor.GameTrackOrganisation;

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

        public int LevelCount => ScriptEditor.ScriptedLevels.Count;
        public int DefaultUnarmedLevelCount => ScriptEditor.Edition.UnarmedLevelCount;
        public int DefaultAmmolessLevelCount => ScriptEditor.Edition.AmmolessLevelCount;
        public int DefaultSunsetCount => ScriptEditor.Edition.SunsetLevelCount;

        public bool RandomizeLevelSequencing
        {
            get => ScriptEditor.LevelSequencingOrganisation == Organisation.Random;
            set
            {
                ScriptEditor.LevelSequencingOrganisation = value ? Organisation.Random : _extLevelOrganisation;
                LevelRandomizer.RandomizeSequencing = value;
            }
        }

        public int LevelSequencingSeed
        {
            get => ScriptEditor.LevelSequencingRNG.Value;
            set => ScriptEditor.LevelSequencingRNG = new RandomGenerator(value);
        }

        public bool RandomizePlayableLevels
        {
            get => ScriptEditor.EnabledLevelOrganisation == Organisation.Random;
            set => ScriptEditor.EnabledLevelOrganisation = value ? Organisation.Random : _extPlayableOrganisation;
        }

        public int PlayableLevelsSeed
        {
            get => ScriptEditor.EnabledLevelRNG.Value;
            set => ScriptEditor.EnabledLevelRNG = new RandomGenerator(value);
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

        public bool RandomizeAudioTracks
        {
            get => ScriptEditor.GameTrackOrganisation == Organisation.Random;
            set
            {
                ScriptEditor.GameTrackOrganisation = value ? Organisation.Random : _extAudioOrganisation;
                LevelRandomizer.RandomizeAudio = value;
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

        public bool UseRewardRoomCameras
        {
            get => LevelRandomizer.UseRewardRoomCameras;
            set => LevelRandomizer.UseRewardRoomCameras = value;
        }

        public bool IncludeKeyItems
        {
            get => LevelRandomizer.IncludeKeyItems;
            set => LevelRandomizer.IncludeKeyItems = value;
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

        public bool MaximiseDragonAppearance
        {
            get => LevelRandomizer.MaximiseDragonAppearance;
            set => LevelRandomizer.MaximiseDragonAppearance = value;
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
            get => G11N.Instance.Languages;
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

        public uint NightModeDarknessRange => TR2NightModeRandomizer.DarknessRange;

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

        public bool PuristMode
        {
            get => LevelRandomizer.PuristMode;
            set => LevelRandomizer.PuristMode = value;
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

        public Color[] VfxAvailableColorChoices
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
            // TREditor will first save the script file, process any initial level file modifications 
            // (e.g. adding pistols to unarmed levels) and then will pass control to TR2LevelRandomizer
            // -> SaveImpl to begin the main randomization.
            _randoEventArgs = new TRRandomizationEventArgs();
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

        public void Unload()
        {
            _editor.Unload();
        }
        #endregion
    }
}