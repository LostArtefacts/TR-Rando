using System;
using System.Collections.Generic;
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

        public string ConfigDirectory => TRCoord.Instance.ConfigDirectory;
        public string ConfigFilePath => TRCoord.Instance.ConfigFilePath;

        private TR2RandomizerCoord()
        {
            TRCoord.Instance.HistoryChanged += TRCoord_HistoryChanged;
            TRCoord.Instance.HistoryAdded += TRCoord_HistoryChanged;
        }

        private void TRCoord_HistoryChanged(object sender, EventArgs e)
        {
            HistoryChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Initialises the interop values between this application and TRGE.
        /// </summary>
        /// <param name="applicationID">The ID for this application, used in the application config file name.</param>
        /// <param name="modificationStamp">The text to inject into the passport and inventory titles to show that this application has modified the game.</param>
        /// <param name="version">The current version of the executing assembly e.g. 1.0.0</param>
        /// <param name="taggedVersion">The tagged version of the current release e.g. 1.0.0-beta</param>
        public void Initialise(string applicationID, string modificationStamp, string version, string taggedVersion)
        {
            TRInterop.ExecutingVersionName = applicationID;
            TRInterop.ScriptModificationStamp = modificationStamp;
            TRInterop.ExecutingVersion = version;
            TRInterop.TaggedVersion = taggedVersion;
            TRInterop.RandomisationSupported = true;
            TRLevelEditorFactory.RegisterEditor(TRVersion.TR2, typeof(TR2LevelRandomizer));
            TRLevelEditorFactory.RegisterEditor(TRVersion.TR2G, typeof(TR2LevelRandomizer));
        }

        public TR2RandomizerController Open(string directoryPath)
        {
            return new TR2RandomizerController(directoryPath);
        }

        public void ClearHistory()
        {
            TRCoord.Instance.ClearHistory();
        }
    }
}