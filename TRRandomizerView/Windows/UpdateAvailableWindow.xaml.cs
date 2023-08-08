using System.ComponentModel;
using System.Windows;
using TRRandomizerView.Updates;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows;

public partial class UpdateAvailableWindow : Window
{
    #region Dependency Properties
    public static readonly DependencyProperty CurrentVersionProperty = DependencyProperty.Register
    (
        "CurrentVersion", typeof(string), typeof(UpdateAvailableWindow)
    );

    public static readonly DependencyProperty NewVersionProperty = DependencyProperty.Register
    (
        "NewVersion", typeof(string), typeof(UpdateAvailableWindow)
    );

    public static readonly DependencyProperty PublishDateProperty = DependencyProperty.Register
    (
        "PublishDate", typeof(string), typeof(UpdateAvailableWindow)
    );

    public static readonly DependencyProperty UpdateBodyProperty = DependencyProperty.Register
    (
        "UpdateBody", typeof(string), typeof(UpdateAvailableWindow)
    );

    public static readonly DependencyProperty UpdateURLProperty = DependencyProperty.Register
    (
        "UpdateURL", typeof(string), typeof(UpdateAvailableWindow)
    );

    public string CurrentVersion
    {
        get => (string)GetValue(CurrentVersionProperty);
        set => SetValue(CurrentVersionProperty, value);
    }

    public string NewVersion
    {
        get => (string)GetValue(NewVersionProperty);
        set => SetValue(NewVersionProperty, value);
    }

    public string PublishDate
    {
        get => (string)GetValue(PublishDateProperty);
        set => SetValue(PublishDateProperty, value);
    }

    public string UpdateBody
    {
        get => (string)GetValue(UpdateBodyProperty);
        set => SetValue(UpdateBodyProperty, value);
    }

    public string UpdateURL
    {
        get => (string)GetValue(UpdateURLProperty);
        set => SetValue(UpdateURLProperty, value);
    }
    #endregion

    public UpdateAvailableWindow()
    {
        InitializeComponent();
        Owner = WindowUtils.GetActiveWindow(this);
        DataContext = this;

        Update update = UpdateChecker.Instance.LatestUpdate;
        UpdateProperties(update);

        UpdateChecker.Instance.UpdateAvailable += UpdateChecker_UpdateAvailable;

        MinWidth = Width;
        MinHeight = Height;
    }

    private void UpdateChecker_UpdateAvailable(object sender, UpdateEventArgs e)
    {
        UpdateProperties(e.Update);
    }

    private void UpdateProperties(Update update)
    {
        CurrentVersion = update.CurrentVersion;
        NewVersion = update.NewVersion;
        PublishDate = update.ReleaseDate.ToString("yyyy-MM-dd HH:mm:ss");
        UpdateBody = update.UpdateBody;
        UpdateURL = update.UpdateURL;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        WindowUtils.EnableMinimiseButton(this, false);
        WindowUtils.TidyMenu(this);
    }

    private void GitHubButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessUtils.OpenURL(UpdateURL);
        DialogResult = true;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        UpdateChecker.Instance.UpdateAvailable -= UpdateChecker_UpdateAvailable;
    }
}
