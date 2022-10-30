using System;
using System.Collections.Generic;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRGE.Coord;
using TRGE.Core;

namespace TRRandomizerCore
{
    public class TRRandomizerCoord
    {
        private static TRRandomizerCoord _instance;

        public static TRRandomizerCoord Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TRRandomizerCoord();
                }
                return _instance;
            }
        }

        public IReadOnlyList<string> History => TRCoord.Instance.History;
        public event EventHandler HistoryChanged;

        public event EventHandler<TROpenRestoreEventArgs> OpenProgressChanged;
        private TROpenRestoreEventArgs _openEventArgs;

        public string ConfigDirectory => TRCoord.Instance.ConfigDirectory;
        public string ConfigFilePath => TRCoord.Instance.ConfigFilePath;

        private TRRandomizerCoord() { }

        private void TRCoord_HistoryChanged(object sender, EventArgs e)
        {
            HistoryChanged?.Invoke(this, e);
        }

        private void TRCoord_BackupProgressChanged(object sender, TRBackupRestoreEventArgs e)
        {
            _openEventArgs.Copy(e);
            OpenProgressChanged.Invoke(this, _openEventArgs);
        }

        /// <summary>
        /// Initialises the interop values between this application and TRGE.
        /// </summary>
        /// <param name="applicationID">The ID for this application, used in the application config file name.</param>
        /// <param name="version">The current version of the executing assembly e.g. 1.0.0</param>
        /// <param name="taggedVersion">The tagged version of the current release e.g. 1.0.0-beta</param>
        /// <param name="modificationStamp">The text to inject into the passport and inventory titles to show that this application has modified the game.</param>
        public void Initialise(string applicationID, string version, string taggedVersion, ModificationStamp modificationStamp)
        {
            TRInterop.ExecutingVersionName = applicationID;
            modificationStamp.ApplyTo(TRInterop.ScriptModificationStamp);
            TRInterop.ExecutingVersion = version;
            TRInterop.TaggedVersion = taggedVersion;
            TRInterop.RandomisationSupported = true;
            TRInterop.SecretRewardsSupported = true;
            TRInterop.ChecksumTester = new ChecksumTester();

            TRLevelEditorFactory.RegisterEditor(TRVersion.TR1, typeof(TR1RandoEditor));
            TRLevelEditorFactory.RegisterEditor(TRVersion.TR2, typeof(TR2RandoEditor));
            TRLevelEditorFactory.RegisterEditor(TRVersion.TR3, typeof(TR3RandoEditor));

            // Not yet fully supported i.e. no locations, textures etc defined
            //TRLevelEditorFactory.RegisterEditor(TRVersion.TR2G, typeof(TR2RandoEditor));
            //TRLevelEditorFactory.RegisterEditor(TRVersion.TR3G, typeof(TR3RandoEditor));

            // #125 Invoke TRCoord.Instance after defining TRInterop.ExecutingVersionName otherwise
            // TRGE will not know the config file name to look for.
            TRCoord.Instance.HistoryChanged += TRCoord_HistoryChanged;
            TRCoord.Instance.HistoryAdded += TRCoord_HistoryChanged;

            TRCoord.Instance.BackupProgressChanged += TRCoord_BackupProgressChanged;
        }

        public TRRandomizerController Open(string directoryPath, bool performChecksumTest)
        {
            _openEventArgs = new TROpenRestoreEventArgs();
            return new TRRandomizerController(directoryPath, performChecksumTest);
        }

        public void ClearHistory()
        {
            TRCoord.Instance.ClearHistory();
        }

        public void ClearCurrentBackup()
        {
            TRCoord.Instance.ClearCurrentBackup();
        }
    }
}