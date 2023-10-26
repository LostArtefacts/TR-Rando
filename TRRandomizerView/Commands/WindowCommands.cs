using System.Windows.Input;

namespace TRRandomizerView.Commands;

public static class WindowCommands
{
    static WindowCommands()
    {
        Open.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
        Randomize.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
        Close.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
        Exit.InputGestures.Add(new KeyGesture(Key.F4, ModifierKeys.Alt));

        SelectAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
        DeSelectAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift));

        RandomizeSeeds.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
        RandomizeOptions.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift));
        CreateGlobalSeed.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Control));

        ImportSettings.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
        ExportSettings.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
        DevelopmentMode.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

        GitHub.InputGestures.Add(new KeyGesture(Key.F1));
    }

    // File
    public static readonly RoutedUICommand Open = new();
    public static readonly RoutedUICommand Randomize = new();
    public static readonly RoutedUICommand Close = new();
    public static readonly RoutedUICommand EmptyRecent = new();
    public static readonly RoutedUICommand Exit = new();

    // Edit
    public static readonly RoutedUICommand SelectAll = new();
    public static readonly RoutedUICommand DeSelectAll = new();
    public static readonly RoutedUICommand RandomizeSeeds = new();
    public static readonly RoutedUICommand RandomizeOptions = new();
    public static readonly RoutedUICommand CreateGlobalSeed = new();
    public static readonly RoutedUICommand EditCommunitySettings = new();

    // Tools
    public static readonly RoutedUICommand ShowBackup = new();
    public static readonly RoutedUICommand ShowErrors = new();
    public static readonly RoutedUICommand Restore = new();
    public static readonly RoutedUICommand DeleteBackup = new();
    public static readonly RoutedUICommand ImportSettings = new();
    public static readonly RoutedUICommand ExportSettings = new();
    public static readonly RoutedUICommand ResetSettings = new();
    public static readonly RoutedUICommand DevelopmentMode = new();

    // Help
    public static readonly RoutedUICommand GitHub = new();
    public static readonly RoutedUICommand Discord = new();
    public static readonly RoutedUICommand CheckForUpdate = new();
    public static readonly RoutedUICommand About = new();

    // Other
    public static readonly RoutedUICommand OpenAdvancedWindowCommand = new();
    public static readonly RoutedUICommand OpenGlobalSettingsCommand = new();
    public static readonly RoutedUICommand LaunchGameCommand = new();
}
