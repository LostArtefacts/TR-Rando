using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRGE.Coord;
using TRGE.Core;

namespace TRRandomizerCore;

public class TRRandomizerCoord
{
    private static TRRandomizerCoord _instance;

    public static TRRandomizerCoord Instance
    {
        get => _instance ??= new();
    }

    public static IReadOnlyList<string> History => TRCoord.Instance.History;
    public event EventHandler HistoryChanged;

    public event EventHandler<TROpenRestoreEventArgs> OpenProgressChanged;
    private TROpenRestoreEventArgs _openEventArgs;

    public static string ConfigDirectory => TRCoord.Instance.ConfigDirectory;
    public static string ConfigFilePath => TRCoord.Instance.ConfigFilePath;

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

        TRLevelEditorFactory.RegisterEditor(TREdition.TR1PC, typeof(TR1ClassicEditor));
        TRLevelEditorFactory.RegisterEditor(TREdition.TR2PC, typeof(TR2ClassicEditor));
        TRLevelEditorFactory.RegisterEditor(TREdition.TR3PC, typeof(TR3ClassicEditor));
        TRLevelEditorFactory.RegisterEditor(TREdition.TR1RM, typeof(TR1RemasteredEditor));
        TRLevelEditorFactory.RegisterEditor(TREdition.TR2RM, typeof(TR2RemasteredEditor));
        TRLevelEditorFactory.RegisterEditor(TREdition.TR3RM, typeof(TR3RemasteredEditor));

        // #125 Invoke TRCoord.Instance after defining TRInterop.ExecutingVersionName otherwise
        // TRGE will not know the config file name to look for.
        TRCoord.Instance.HistoryChanged += TRCoord_HistoryChanged;
        TRCoord.Instance.HistoryAdded += TRCoord_HistoryChanged;

        TRCoord.Instance.BackupProgressChanged += TRCoord_BackupProgressChanged;
    }

    public TRRandomizerController Open(string directoryPath, bool performChecksumTest)
    {
        _openEventArgs = new();
        return new(directoryPath, performChecksumTest);
    }

    public static void ClearHistory()
    {
        TRCoord.Instance.ClearHistory();
    }

    public static void ClearCurrentBackup()
    {
        TRCoord.Instance.ClearCurrentBackup();
    }
}
