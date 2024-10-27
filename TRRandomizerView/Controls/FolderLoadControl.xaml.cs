using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TRGE.Core;
using TRRandomizerCore;
using TRRandomizerView.Events;
using TRRandomizerView.Model;
using TRRandomizerView.Utilities;
using TRRandomizerView.Windows;

namespace TRRandomizerView.Controls;

public partial class FolderLoadControl : UserControl
{
    #region Dependency Properties
    public static readonly DependencyProperty AppTitleProperty = DependencyProperty.Register
    (
        "AppTitle", typeof(string), typeof(FolderLoadControl)
    );

    public static readonly DependencyProperty RecentFoldersProperty = DependencyProperty.Register
    (
        "RecentFolders", typeof(RecentFolderList), typeof(FolderLoadControl)
    );

    public static readonly DependencyProperty RecentFoldersVisibilityProperty = DependencyProperty.Register
    (
        "RecentFoldersVisibility", typeof(Visibility), typeof(FolderLoadControl)
    );

    public string AppTitle
    {
        get => (string)GetValue(AppTitleProperty);
        private set => SetValue(AppTitleProperty, value);
    }

    public RecentFolderList RecentFolders
    {
        get => (RecentFolderList)GetValue(RecentFoldersProperty);
        set
        {
            SetValue(RecentFoldersProperty, value);
            RecentFoldersVisibility = RecentFolders.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public Visibility RecentFoldersVisibility
    {
        get => (Visibility)GetValue(RecentFoldersVisibilityProperty);
        set => SetValue(RecentFoldersVisibilityProperty, value);
    }
    #endregion

    public event EventHandler<DataFolderEventArgs> DataFolderOpened;

    public FolderLoadControl()
    {
        InitializeComponent();
        if (Application.Current is App app) //gets rid of VS designer issue
        {
            AppTitle = app.Title;
        }
        _content.DataContext = this;
    }

    private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_historyListView.SelectedItem != null)
        {
            OpenDataFolder(_historyListView.SelectedItem as RecentFolder);
            _historyListView.UnselectAll();
        }
    }

    private void HistoryListView_MouseMove(object sender, MouseEventArgs e)
    {
        ListViewItem item = _historyListView.GetItemAt(e.GetPosition(_historyListView));
        _historyListView.Cursor = item == null ? Cursors.Arrow : Cursors.Hand;
    }

    public void OpenDataFolder()
    {
        if (FileUtils.GetFolderPath("Select Data Folder") is string path)
        {
            OpenDataFolder(path);
        }
    }

    public void OpenDataFolder(RecentFolder folder)
    {
        OpenDataFolder(folder.FolderPath);
    }

    public void OpenDataFolder(string folderPath)
    {
        OpenProgressWindow opw = new(folderPath);
        try
        {
            if (opw.ShowDialog() ?? false)
            {
                DataFolderOpened?.Invoke(this, new DataFolderEventArgs(folderPath, opw.OpenedController));
            }
            else if (opw.OpenException != null)
            {
                throw opw.OpenException;
            }
        }
        catch (ChecksumMismatchException)
        {
            MessageWindow.ShowError(folderPath + "\n\nGame data integrity check failed. Randomization cannot be performed as the game data files in the chosen directory are not original." +
                "\n\nPlease uninstall the game and remove any external mods you may have applied.\n\nOnce complete, reinstall the game afresh, and open the data folder again in the randomizer to proceed.",
                "https://github.com/LostArtefacts/TR-Rando/blob/master/USING.md#troubleshooting");
        }
        catch (Exception e)
        {
            MessageWindow.ShowException(e);
        }
    }

    public static void EmptyRecentFolders()
    {
        string msg = string.Format
        (
            "All backup files and edit configuration files will be removed from the directory below. Make sure to backup any files you want to keep.\n\n{0}\n\nDo you wish to proceed?",
            TRRandomizerCoord.ConfigDirectory
        );

        if (MessageWindow.ShowConfirm(msg))
        {
            try
            {
                TRRandomizerCoord.ClearHistory();
            }
            catch (Exception e)
            {
                MessageWindow.ShowException(e);
            }
        }
    }
}
