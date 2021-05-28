using System;
using System.Collections.Generic;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Randomizers;
using TRGE.Coord;
using TRGE.Core;

namespace TR2RandomizerCore
{
    public class TR2RandomizerCoord
    {
        private static TR2RandomizerCoord _instance;

        public static TR2RandomizerCoord Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TR2RandomizerCoord();
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

        private TR2RandomizerCoord() { }

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
            TRLevelEditorFactory.RegisterEditor(TRVersion.TR2, typeof(TR2LevelRandomizer));
            TRLevelEditorFactory.RegisterEditor(TRVersion.TR2G, typeof(TR2LevelRandomizer));

            // #125 Invoke TRCoord.Instance after defining TRInterop.ExecutingVersionName otherwise
            // TRGE will not know the config file name to look for.
            TRCoord.Instance.HistoryChanged += TRCoord_HistoryChanged;
            TRCoord.Instance.HistoryAdded += TRCoord_HistoryChanged;

            TRCoord.Instance.BackupProgressChanged += TRCoord_BackupProgressChanged;
        }

        public TR2RandomizerController Open(string directoryPath)
        {
            _openEventArgs = new TROpenRestoreEventArgs();
            return new TR2RandomizerController(directoryPath);
        }

        public void ClearHistory()
        {
            TRCoord.Instance.ClearHistory();
        }
    }
}