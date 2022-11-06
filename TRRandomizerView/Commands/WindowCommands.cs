using System.Windows.Input;

namespace TRRandomizerView.Commands
{
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
        public static readonly RoutedUICommand Open = new RoutedUICommand();
        public static readonly RoutedUICommand Randomize = new RoutedUICommand();
        public static readonly RoutedUICommand Close = new RoutedUICommand();
        public static readonly RoutedUICommand EmptyRecent = new RoutedUICommand();
        public static readonly RoutedUICommand Exit = new RoutedUICommand();

        // Edit
        public static readonly RoutedUICommand SelectAll = new RoutedUICommand();
        public static readonly RoutedUICommand DeSelectAll = new RoutedUICommand();
        public static readonly RoutedUICommand RandomizeSeeds = new RoutedUICommand();
        public static readonly RoutedUICommand RandomizeOptions = new RoutedUICommand();
        public static readonly RoutedUICommand CreateGlobalSeed = new RoutedUICommand();
        public static readonly RoutedUICommand EditCommunitySettings = new RoutedUICommand();

        // Tools
        public static readonly RoutedUICommand ShowBackup = new RoutedUICommand();
        public static readonly RoutedUICommand ShowErrors = new RoutedUICommand();
        public static readonly RoutedUICommand Restore = new RoutedUICommand();
        public static readonly RoutedUICommand DeleteBackup = new RoutedUICommand();
        public static readonly RoutedUICommand ImportSettings = new RoutedUICommand();
        public static readonly RoutedUICommand ExportSettings = new RoutedUICommand();
        public static readonly RoutedUICommand DevelopmentMode = new RoutedUICommand();

        // Help
        public static readonly RoutedUICommand GitHub = new RoutedUICommand();
        public static readonly RoutedUICommand Discord = new RoutedUICommand();
        public static readonly RoutedUICommand CheckForUpdate = new RoutedUICommand();
        public static readonly RoutedUICommand About = new RoutedUICommand();

        // Other
        public static readonly RoutedUICommand OpenAdvancedWindowCommand = new RoutedUICommand();
    }
}