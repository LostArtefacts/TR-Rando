using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TRRandomizerCore;
using TRRandomizerView.Events;
using TRRandomizerView.Model;
using TRRandomizerView.Utilities;
using TRRandomizerView.Windows;

namespace TRRandomizerView.Controls;

public partial class EditorControl : UserControl
{
    #region Dependency Properties
    public static readonly DependencyProperty TREditionProperty = DependencyProperty.Register
    (
        "Edition", typeof(string), typeof(EditorControl)
    );

    public static readonly DependencyProperty DataFolderProperty = DependencyProperty.Register
    (
        "DataFolder", typeof(string), typeof(EditorControl), new PropertyMetadata(string.Empty)
    );

    public string Edition
    {
        get => (string)GetValue(TREditionProperty);
        private set => SetValue(TREditionProperty, value);
    }

    public string DataFolder
    {
        get => (string)GetValue(DataFolderProperty);
        private set => SetValue(DataFolderProperty, value);
    }
    #endregion

    private const string _configExtension = "trr";
    private const string _configFilter = $"TR Rando Config Files|*.{_configExtension}";

    private readonly ControllerOptions _options;
    private bool _dirty, _reloadRequested, _hideRandomOptionsPrompt;
    private volatile bool _showExternalModPrompt;

    private static int _lastGlobalSeed = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

    public event EventHandler<EditorEventArgs> EditorStateChanged;

    public TRRandomizerController Controller;

    private readonly DispatcherTimer _popupTimer;

    public bool DevelopmentMode
    {
        get => _options.DevelopmentMode;
        set => _options.DevelopmentMode = value;
    }

    public EditorControl()
    {
        InitializeComponent();
        _editorGrid.DataContext = _options = new ControllerOptions();
        _options.PropertyChanged += Controller_PropertyChanged;
        _dirty = false;
        _showExternalModPrompt = true;
        _reloadRequested = false;

        _popupTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 1, 500)
        };
        _popupTimer.Tick += PopupTimer_Tick;
    }

    private void Controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _dirty = true;
        FireEditorStateChanged();
    }

    private void FireEditorStateChanged()
    {
        EditorStateChanged?.Invoke(this, new EditorEventArgs
        {
            IsDirty = _dirty,
            CanExport = Controller != null && Controller.IsExportPossible,
            ReloadRequested = _reloadRequested
        });
    }

    public void Load(DataFolderEventArgs e)
    {
        Controller = e.Controller;
        Edition = Controller.EditionTitle;
        DataFolder = e.DataFolder;

        Controller.ConfigExternallyChanged += Controller_ConfigExternallyChanged;

        Reload();
    }

    private void Controller_ConfigExternallyChanged(object sender, FileSystemEventArgs e)
    {
        // Prevent several message boxes appearing i.e. in case the initial box is displayed and
        // several external edits are carried out.
        if (_showExternalModPrompt)
        {
            _showExternalModPrompt = false;
            new Thread(() => Dispatcher.Invoke(() => HandleConfigExternallyChanged(e))).Start();
        }
    }

    private void HandleConfigExternallyChanged(FileSystemEventArgs _)
    {
        string message = _dirty ?
            "The configuration file has been modified by an external program and you have unsaved changes.\n\nDo you want to reload the configuration and lose your changes?" :
            "The configuration file has been modified by an external program. Do you want to reload the configuration?";

        try
        {
            if (MessageWindow.ShowConfirm(message))
            {
                _dirty = false;
                _reloadRequested = true;
                Controller.ConfigExternallyChanged -= Controller_ConfigExternallyChanged;
                FireEditorStateChanged();
            }
            else
            {
                _dirty = true;
                FireEditorStateChanged();
            }
        }
        finally
        {
            _showExternalModPrompt = true;
        }
    }

    private void Reload()
    {
        _options.Load(Controller);
        _dirty = false;
        FireEditorStateChanged();
    }

    public bool Randomize()
    {
        if (!_options.RandomizationPossible)
        {
            ShowInvalidSelectionMessage();
            return false;
        }

        if (_options.DevelopmentMode)
        {
            if (!MessageWindow.ShowConfirm("Development mode is switched on and so the generated level files will not be playable.\n\nDo you wish to continue?"))
            {
                return false;
            }
        }

        RandomizeProgressWindow spw = new(Controller, _options);
        if (spw.ShowDialog() ?? false)
        {
            _dirty = false;
            FireEditorStateChanged();

            if (Controller.AutoLaunchGame)
            {
                LaunchGame();
            }

            return true;
        }
        return false;
    }

    public void LaunchGame()
    {
        try
        {
            string exePath = null;
            foreach (string exe in Controller.GetExecutables())
            {
                string path = Path.GetFullPath(Path.Combine(DataFolder, @"..\" + exe));
                if (File.Exists(path))
                {
                    exePath = path;
                    break;
                }
            }
            
            if (exePath == null)
            {
                throw new IOException("The game could not be launched automatically as no suitable executable was found.");
            }

            ProcessUtils.OpenFile(exePath);
        }
        catch (Exception e)
        {
            MessageWindow.ShowWarning(e.Message);
        }
    }

    public void Unload()
    {
        if (Controller != null)
        {
            Controller.ConfigExternallyChanged -= Controller_ConfigExternallyChanged;
            Controller.Unload();
            Controller = null;
        }

        _options.Unload();

        _dirty = false;
        _reloadRequested = false;
        _hideRandomOptionsPrompt = false;
        FireEditorStateChanged();
    }

    public void SelectAll()
    {
        _options.SetAllRandomizationsEnabled(true);
    }

    public void DeSelectAll()
    {
        _options.SetAllRandomizationsEnabled(false);
    }

    public bool CanSelectAll()
    {
        return !_options.AllRandomizationsEnabled();
    }

    public bool CanDeSelectAll()
    {
        return _options.RandomizationPossible;
    }

    public bool CanRandomize()
    {
        return _options.RandomizationPossible;
    }

    public bool CanEditCommunitySettings()
    {
        return _options.IsTR1X || _options.IsTR3Main;
    }

    public void OpenBackupFolder()
    {
        ProcessUtils.OpenFolder(Controller.BackupDirectory);
    }

    public bool CanOpenErrorFolder()
    {
        return Directory.Exists(Controller.ErrorDirectory);
    }

    public void OpenErrorFolder()
    {
        ProcessUtils.OpenFolder(Controller.ErrorDirectory);
    }

    public void RestoreDefaults()
    {
        if (MessageWindow.ShowConfirm("The files that were backed up when this folder was first opened will be copied back to the original directory.\n\nDo you wish to proceed?"))
        {
            RestoreProgressWindow rpw = new(Controller);
            try
            {
                if (rpw.ShowDialog() ?? false)
                {
                    Reload();
                    MessageWindow.ShowMessage("The restore completed successfully.");
                }
                else if (rpw.RestoreException != null)
                {
                    throw rpw.RestoreException;
                }
            }
            catch (Exception e)
            {
                MessageWindow.ShowException(e);
            }
        }
    }

    public bool DeleteBackup()
    {
        if (MessageWindow.ShowConfirm("The files that were backed up when this folder was first opened will be deleted and the editor will be closed.\n\nDo you wish to proceed?"))
        {
            try
            {
                _showExternalModPrompt = false;
                TRRandomizerCoord.ClearCurrentBackup();
                _dirty = false;
                return true;
            }
            catch (Exception e)
            {
                MessageWindow.ShowException(e);
            }
            finally
            {
                _showExternalModPrompt = true;
            }
        }

        return false;
    }

    private void EditorControl_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                ImportSettings(files[0]);
            }
        }
    }

    public void ImportSettings()
    {
        if (FileUtils.GetOpenPath("Import Settings", _configFilter) is string path)
        {
            ImportSettings(path);
        }
    }

    private void ImportSettings(string filePath)
    {
        try
        {
            Controller.ImportSettings(filePath);
            _options.Load(Controller);
        }
        catch (Exception e)
        {
            MessageWindow.ShowException(e);
        }
    }

    public void ExportSettings()
    {
        string initialName = GetSafeFileName(Edition, _configExtension);
        if (FileUtils.GetSavePath("Export Settings", _configFilter, initialName) is string path)
        {
            try
            {
                Controller.ExportSettings(path);
            }
            catch (Exception e)
            {
                MessageWindow.ShowException(e);
            }
        }
    }

    public void ResetSettings()
    {
        try
        {
            Controller.ResetSettings();
            _options.Load(Controller);
        }
        catch (Exception e)
        {
            MessageWindow.ShowException(e);
        }
    }

    public void RandomizeAllSeeds()
    {
        if (_options.RandomizationPossible)
        {
            _popupTimer.Stop();
            _options.RandomizeActiveSeeds();
            ShowPopupMessage("Seeds Randomized!");
        }
        else
        {
            ShowInvalidSelectionMessage();
        }
    }

    public void RandomizeAllOptions()
    {
        if (_options.RandomizationPossible)
        {
            if (_hideRandomOptionsPrompt || MessageWindow.ShowConfirm("This option can produce very challenging outcomes.\nAre you sure you wish to continue?"))
            {
                _popupTimer.Stop();
                _options.RandomizeActiveOptions();
                ShowPopupMessage("Options Randomized!");
                _hideRandomOptionsPrompt = true;
            }
        }
        else
        {
            ShowInvalidSelectionMessage();
        }
    }

    private void ShowPopupMessage(string text)
    {
        _popupTextBlock.Text = text;
        _feedbackPopup.IsOpen = true;
        _popupTimer.Start();
    }

    private void PopupTimer_Tick(object sender, EventArgs e)
    {
        _feedbackPopup.IsOpen = false;
    }

    public void ConfigureGlobalSeed()
    {
        GlobalSeedWindow gsw = new(1, ControllerOptions.MaxSeedValue, _lastGlobalSeed);
        if (gsw.ShowDialog() ?? false)
        {
            _options.SetGlobalSeed(gsw.Seed);
            _lastGlobalSeed = gsw.Seed;
        }
    }

    public void EditGlobalSettings()
    {
        GlobalSettingsWindow gsw = new(_options);
        gsw.ShowDialog();
    }

    public void EditCommunitySettings()
    {
        if (_options.IsTR1X)
        {
            LaunchTR1XSettings();
        }
        else if (_options.IsTR3Main)
        {
            LaunchTR3MainSettings();
        }
    }

    private void LaunchTR1XSettings()
    {
        TR1XWindow window = new(_options);
        window.ShowDialog();
    }

    private void LaunchTR3MainSettings()
    {
        try
        {
            string exePath = Path.GetFullPath(Path.Combine(DataFolder, @"..\tomb3_ConfigTool.exe"));
            ProcessUtils.OpenFile(exePath);
        }
        catch (Exception e)
        {
            MessageWindow.ShowWarning(e.Message);
        }
    }

    private static void ShowInvalidSelectionMessage()
    {
        MessageWindow.ShowMessage("Please choose at least one element to include in the randomization.");
    }

    private static string GetSafeFileName(string str, string ext)
    {
        return new Regex("[^a-zA-Z0-9_-]").Replace(str, string.Empty) + "." + ext;
    }
}
