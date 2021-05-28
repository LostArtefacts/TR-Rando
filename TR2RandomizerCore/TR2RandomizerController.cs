using System;
using System.IO;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Randomizers;
using TRGE.Coord;
using TRGE.Core;
using TRLevelReader.Helpers;

namespace TR2RandomizerCore
{
    public class TR2RandomizerController
    {
        private readonly TREditor _editor;

        internal TR23ScriptEditor ScriptEditor => _editor.ScriptEditor as TR23ScriptEditor;
        internal TR2LevelRandomizer LevelRandomizer => _editor.LevelEditor as TR2LevelRandomizer;

        internal TR2RandomizerController(string directoryPath)
        {
            // If there is a checksum mismatch, we will just ignore the previous backup and open the folder afresh
            _editor = TRCoord.Instance.Open(directoryPath, TRScriptOpenOption.DiscardBackup);
            _editor.SaveProgressChanged += Editor_SaveProgressChanged;
            _editor.RestoreProgressChanged += Editor_RestoreProgressChanged;
            StoreExternalOrganisations();
        }

        #region ScriptEditor Passthrough
        private Organisation _extLevelOrganisation, _extUnarmedOrganisation, _extAmmolessOrganisation, _extSecretRewardsOrganisation, _extSunsetOrganisation, _extAudioOrganisation;

        /// <summary>
        /// We need to store any organsation values that aren't random so if randomization is turned off for the 
        /// corresponding functions, they will be reverted to what was previously set externally. If any haven't
        /// been set externally, turning them off will result in standard organisation.
        /// </summary>
        private void StoreExternalOrganisations()
        {
            _extLevelOrganisation = RandomizeLevelSequencing ? Organisation.Default : ScriptEditor.LevelSequencingOrganisation;
            _extUnarmedOrganisation = RandomizeUnarmedLevels ? Organisation.Default : ScriptEditor.UnarmedLevelOrganisation;
            _extAmmolessOrganisation = RandomizeAmmolessLevels ? Organisation.Default : ScriptEditor.AmmolessLevelOrganisation;
            _extSecretRewardsOrganisation = RandomizeSecretRewards ? Organisation.Default : ScriptEditor.SecretBonusOrganisation;
            _extSunsetOrganisation = RandomizeSunsets ? Organisation.Default : ScriptEditor.LevelSunsetOrganisation;
            _extAudioOrganisation = RandomizeAudioTracks ? Organisation.Default : ScriptEditor.GameTrackOrganisation;
        }

        public int LevelCount => ScriptEditor.ScriptedLevels.Count;

        public bool RandomizeLevelSequencing
        {
            get => ScriptEditor.LevelSequencingOrganisation == Organisation.Random;
            set => ScriptEditor.LevelSequencingOrganisation = value ? Organisation.Random : _extLevelOrganisation;
        }

        public int LevelSequencingSeed
        {
            get => ScriptEditor.LevelSequencingRNG.Value;
            set => ScriptEditor.LevelSequencingRNG = new RandomGenerator(value);
        }

        public bool RandomizeUnarmedLevels
        {
            get => ScriptEditor.UnarmedLevelOrganisation == Organisation.Random;
            set => ScriptEditor.UnarmedLevelOrganisation = value ? Organisation.Random : _extUnarmedOrganisation;
        }

        public int UnarmedLevelsSeed
        {
            get => ScriptEditor.UnarmedLevelRNG.Value;
            set => ScriptEditor.UnarmedLevelRNG = new RandomGenerator(value);
        }

        public uint UnarmedLevelCount
        {
            get => ScriptEditor.RandomUnarmedLevelCount;
            set => ScriptEditor.RandomUnarmedLevelCount = value;
        }

        public bool RandomizeAmmolessLevels
        {
            get => ScriptEditor.AmmolessLevelOrganisation == Organisation.Random;
            set => ScriptEditor.AmmolessLevelOrganisation = value ? Organisation.Random : _extAmmolessOrganisation;
        }

        public int AmmolessLevelsSeed
        {
            get => ScriptEditor.AmmolessLevelRNG.Value;
            set => ScriptEditor.AmmolessLevelRNG = new RandomGenerator(value);
        }

        public uint AmmolessLevelCount
        {
            get => ScriptEditor.RandomAmmolessLevelCount;
            set => ScriptEditor.RandomAmmolessLevelCount = value;
        }

        public bool RandomizeSecretRewards
        {
            get => ScriptEditor.SecretBonusOrganisation == Organisation.Random;
            set => ScriptEditor.SecretBonusOrganisation = value ? Organisation.Random : _extSecretRewardsOrganisation;
        }

        public int SecretRewardSeed
        {
            get => ScriptEditor.SecretBonusRNG.Value;
            set => ScriptEditor.SecretBonusRNG = new RandomGenerator(value);
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
            set => ScriptEditor.GameTrackOrganisation = value ? Organisation.Random : _extAudioOrganisation;
        }

        public int AudioTracksSeed
        {
            get => ScriptEditor.GameTrackRNG.Value;
            set => ScriptEditor.GameTrackRNG = new RandomGenerator(value);
        }

        public bool RandomGameTracksIncludeBlank
        {
            get => ScriptEditor.RandomGameTracksIncludeBlank;
            set => ScriptEditor.RandomGameTracksIncludeBlank = value;
        }

        public bool DisableDemos
        {
            get => !ScriptEditor.DemosEnabled;
            set => ScriptEditor.DemosEnabled = !value;
        }
        #endregion

        #region LevelRandomizer Passthrough
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

        public bool HardSecrets
        {
            get => LevelRandomizer.HardSecrets;
            set => LevelRandomizer.HardSecrets = value;
        }

        public bool IncludeKeyItems
        {
            get => LevelRandomizer.IncludeKeyItems;
            set => LevelRandomizer.IncludeKeyItems = value;
        }

        public bool DevelopmentMode
        {
            get => LevelRandomizer.DevelopmentMode;
            set => LevelRandomizer.DevelopmentMode = value;
        }

        public bool CrossLevelEnemies
        {
            get => LevelRandomizer.CrossLevelEnemies;
            set => LevelRandomizer.CrossLevelEnemies = value;
        }
        #endregion

        #region TREditor Passthrough
        public string EditionTitle => _editor.Edition.Title;
        public bool IsExportPossible => _editor.IsExportPossible;
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