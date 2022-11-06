using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using TRRandomizerCore;
using TRRandomizerView.Events;
using TRRandomizerView.Model;
using TRRandomizerView.Updates;

namespace TRRandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IRecentFolderOpener
    {
        #region Dependency Properties
        public static readonly DependencyProperty AppTitleProperty = DependencyProperty.Register
        (
            "AppTitle", typeof(string), typeof(MainWindow)
        );

        public static readonly DependencyProperty AboutTitleProperty = DependencyProperty.Register
        (
            "AboutTitle", typeof(string), typeof(MainWindow)
        );

        public static readonly DependencyProperty IsEditorActiveProperty = DependencyProperty.Register
        (
            "IsEditorActive", typeof(bool), typeof(MainWindow)
        );

        public static readonly DependencyProperty IsEditorDirtyProperty = DependencyProperty.Register
        (
            "IsEditorDirty", typeof(bool), typeof(MainWindow)
        );

        public static readonly DependencyProperty EditorCanExportProperty = DependencyProperty.Register
        (
            "EditorCanExport", typeof(bool), typeof(MainWindow)
        );

        public static readonly DependencyProperty CanEmptyRecentFoldersProperty = DependencyProperty.Register
        (
            "CanEmptyRecentFolders", typeof(bool), typeof(MainWindow)
        );

        public static readonly DependencyProperty FolderControlVisibilityProperty = DependencyProperty.Register
        (
            "FolderControlVisibility", typeof(Visibility), typeof(MainWindow)
        );

        public static readonly DependencyProperty EditorControlVisibilityProperty = DependencyProperty.Register
        (
            "EditorControlVisibility", typeof(Visibility), typeof(MainWindow)
        );

        public static readonly DependencyProperty EditorStatusVisibilityProperty = DependencyProperty.Register
        (
            "EditorStatusVisibility", typeof(Visibility), typeof(MainWindow)
        );

        public static readonly DependencyProperty EditorSavedStatusVisibilityProperty = DependencyProperty.Register
        (
            "EditorSavedStatusVisibility", typeof(Visibility), typeof(MainWindow)
        );

        public static readonly DependencyProperty EditorUnsavedStatusVisibilityProperty = DependencyProperty.Register
        (
            "EditorUnsavedStatusVisibility", typeof(Visibility), typeof(MainWindow)
        );

        public static readonly DependencyProperty RecentFoldersProperty = DependencyProperty.Register
        (
            "RecentFolders", typeof(RecentFolderList), typeof(MainWindow)
        );

        public static readonly DependencyProperty HasRecentFoldersProperty = DependencyProperty.Register
        (
            "HasRecentFolders", typeof(bool), typeof(MainWindow)
        );

        public string AppTitle
        {
            get => (string)GetValue(AppTitleProperty);
            private set => SetValue(AppTitleProperty, value);
        }

        public string AboutTitle
        {
            get => (string)GetValue(AboutTitleProperty);
            private set => SetValue(AboutTitleProperty, value);
        }

        public bool IsEditorActive
        {
            get => (bool)GetValue(IsEditorActiveProperty);
            set
            {
                SetValue(IsEditorActiveProperty, value);
                FolderControlVisibility = value ? Visibility.Collapsed : Visibility.Visible;
                EditorControlVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                EditorStatusVisibility = value ? Visibility.Visible : Visibility.Hidden;
                CanEmptyRecentFolders = !value;
            }
        }

        public bool IsEditorDirty
        {
            get => (bool)GetValue(IsEditorDirtyProperty);
            set
            {
                SetValue(IsEditorDirtyProperty, value);
                EditorSavedStatusVisibility = !value ? Visibility.Visible : Visibility.Collapsed;
                EditorUnsavedStatusVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool EditorCanExport
        {
            get => (bool)GetValue(EditorCanExportProperty);
            set => SetValue(EditorCanExportProperty, value);
        }

        public Visibility EditorControlVisibility
        {
            get => (Visibility)GetValue(EditorControlVisibilityProperty);
            set => SetValue(EditorControlVisibilityProperty, value);
        }

        public Visibility EditorStatusVisibility
        {
            get => (Visibility)GetValue(EditorStatusVisibilityProperty);
            set => SetValue(EditorStatusVisibilityProperty, value);
        }

        public Visibility EditorSavedStatusVisibility
        {
            get => (Visibility)GetValue(EditorSavedStatusVisibilityProperty);
            set => SetValue(EditorSavedStatusVisibilityProperty, value);
        }

        public Visibility EditorUnsavedStatusVisibility
        {
            get => (Visibility)GetValue(EditorUnsavedStatusVisibilityProperty);
            set => SetValue(EditorUnsavedStatusVisibilityProperty, value);
        }

        public bool CanEmptyRecentFolders
        {
            get => (bool)GetValue(CanEmptyRecentFoldersProperty);
            set => SetValue(CanEmptyRecentFoldersProperty, HasRecentFolders && value);
        }

        public Visibility FolderControlVisibility
        {
            get => (Visibility)GetValue(FolderControlVisibilityProperty);
            set => SetValue(FolderControlVisibilityProperty, value);
        }

        public RecentFolderList RecentFolders
        {
            get => (RecentFolderList)GetValue(RecentFoldersProperty);
            set
            {
                SetValue(RecentFoldersProperty, value);
                HasRecentFolders = !RecentFolders.IsEmpty;
                _folderControl.RecentFolders = RecentFolders;
            }
        }

        public bool HasRecentFolders
        {
            get => (bool)GetValue(HasRecentFoldersProperty);
            set => SetValue(HasRecentFoldersProperty, value);
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            if (Application.Current is App app) //gets rid of VS designer issue
            {
                AppTitle = app.Title;
                AboutTitle = "About " + app.Title;
            }

            TRRandomizerCoord.Instance.HistoryChanged += Coord_HistoryChanged;

            _folderControl.DataFolderOpened += FolderControl_DataFolderOpened;
            _editorControl.EditorStateChanged += EditorControl_EditorStateChanged;
            RefreshHistoryMenu();

            _editionStatusText.DataContext = _folderStatusText.DataContext = _editorControl;
            IsEditorActive = false;

            UpdateChecker.Instance.UpdateAvailable += UpdateChecker_UpdateAvailable; ;

            MinWidth = Width;
            MinHeight = Height;
        }

        #region Open/Save(Randomize)/Close
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ConfirmEditorSaveState())
            {
                _folderControl.OpenDataFolder();
            }
        }

        private void RandomizeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive;
        }

        private void RandomizeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.Randomize();
        }

        private void CloseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive;
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ConfirmEditorSaveState())
            {
                _editorControl.Unload();
                IsEditorActive = false;
            }
        }

        private void FolderControl_DataFolderOpened(object sender, DataFolderEventArgs e)
        {
            _editorControl.Load(e);
            IsEditorActive = true;
        }

        private void EditorControl_EditorStateChanged(object sender, EditorEventArgs e)
        {
            IsEditorDirty = e.IsDirty;
            EditorCanExport = e.CanExport;
            _developmentModeMenuItem.IsChecked = _editorControl.DevelopmentMode;
            if (e.ReloadRequested)
            {
                _editorControl.Unload();
                IsEditorActive = false;
                _folderControl.OpenDataFolder(_editorControl.DataFolder);
            }
        }
        #endregion

        #region History
        private void Coord_HistoryChanged(object sender, EventArgs e)
        {
            RefreshHistoryMenu();
        }

        private void RefreshHistoryMenu()
        {
            if (Dispatcher.CheckAccess())
            {
                RecentFolders = new RecentFolderList(this);
            }
            else
            {
                Dispatcher.Invoke(RefreshHistoryMenu);
            }
        }

        public void OpenDataFolder(RecentFolder folder)
        {
            if (ConfirmEditorSaveState())
            {
                _folderControl.OpenDataFolder(folder);
            }
        }

        private void EmptyRecentCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanEmptyRecentFolders;
        }

        private void EmptyRecentCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _folderControl.EmptyRecentFolders();
        }
        #endregion

        #region Edit Commands
        private void SelectAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive && _editorControl.CanSelectAll();
        }

        private void SelectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.SelectAll();
        }

        private void DeSelectAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive && _editorControl.CanDeSelectAll();
        }

        private void DeSelectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.DeSelectAll();
        }

        private void RandomizeSeedsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive && _editorControl.CanRandomize();
        }

        private void RandomizeSeedsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.RandomizeAllSeeds();
        }

        private void RandomizeOptionsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive && _editorControl.CanRandomize();
        }

        private void RandomizeOptionsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.RandomizeAllOptions();
        }

        private void GlobalSeedCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive && _editorControl.CanRandomize();
        }

        private void GlobalSeedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.ConfigureGlobalSeed();
        }

        private void EditorActiveCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive;
        }

        private void EditCommunitySettingsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive && _editorControl.CanEditCommunitySettings();
        }

        private void EditCommunitySettingsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.EditCommunitySettings();
        }
        #endregion

        #region Tools
        private void ShowBackupCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.OpenBackupFolder();
        }

        private void ShowErrorsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsEditorActive && _editorControl.CanOpenErrorFolder();
        }

        private void ShowErrorsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.OpenErrorFolder();
        }

        private void RestoreCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.RestoreDefaults();
        }

        private void DeleteBackupCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_editorControl.DeleteBackup())
            {
                _editorControl.Unload();
                IsEditorActive = false;
            }
        }

        private void ImportSettingsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _editorControl.ImportSettings();
        }

        private void ExportSettingsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = EditorCanExport;
        }

        private void ExportCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ConfirmEditorSaveState())
            {
                _editorControl.ExportSettings();
            }
        }

        private void DevelopmentModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _developmentModeMenuItem.IsChecked = _editorControl.DevelopmentMode = !_editorControl.DevelopmentMode;
        }

        private void EditorFolder_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
        #endregion

        #region Help
        private void GitHubCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start("https://github.com/DanzaG/TR2-Rando");
        }

        private void DiscordCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start("https://discord.gg/f4bUqwgcCN");
        }

        private void CheckForUpdateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (UpdateChecker.Instance.CheckForUpdates())
                {
                    ShowUpdateWindow();
                }
                else
                {
                    MessageWindow.ShowMessage(string.Format("The current version of {0} is up to date.", AppTitle));
                }
            }
            catch (Exception ex)
            {
                MessageWindow.ShowException(ex);
            }
        }

        private void UpdateChecker_UpdateAvailable(object sender, UpdateEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateChecker_UpdateAvailable(sender, e));
            }
            else
            {
                _updateAvailableMenu.Visibility = Visibility.Visible;
            }
        }

        private void UpdateAvailableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowUpdateWindow();
        }

        private void ShowUpdateWindow()
        {
            new UpdateAvailableWindow().ShowDialog();
        }

        private void AboutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }
        #endregion

        #region Exiting
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ConfirmEditorSaveState())
            {
                Application.Current.Shutdown();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!ConfirmEditorSaveState())
            {
                e.Cancel = true;
            }
        }

        private bool ConfirmEditorSaveState()
        {
            if (IsEditorDirty)
            {
                switch (MessageWindow.ShowConfirmCancel("Do you want to save the changes you have made?"))
                {
                    case MessageBoxResult.Yes:
                        return _editorControl.Randomize();
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }
        #endregion
    }
}