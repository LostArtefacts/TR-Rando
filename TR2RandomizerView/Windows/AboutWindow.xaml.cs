using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using TR2RandomizerView.Utilities;

namespace TR2RandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        #region Dependency Properties
        public static readonly DependencyProperty AboutTitleProperty = DependencyProperty.Register
        (
            "AboutTitle", typeof(string), typeof(AboutWindow)
        );

        public static readonly DependencyProperty AppTitleProperty = DependencyProperty.Register
        (
            "AppTitle", typeof(string), typeof(AboutWindow)
        );

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register
        (
            "Version", typeof(string), typeof(AboutWindow)
        );

        public static readonly DependencyProperty CopyrightProperty = DependencyProperty.Register
        (
            "Copyright", typeof(string), typeof(AboutWindow)
        );

        public string AboutTitle
        {
            get => (string)GetValue(AboutTitleProperty);
            private set => SetValue(AboutTitleProperty, value);
        }

        public string AppTitle
        {
            get => (string)GetValue(AppTitleProperty);
            private set => SetValue(AppTitleProperty, value);
        }

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            private set => SetValue(VersionProperty, value);
        }

        public string Copyright
        {
            get => (string)GetValue(CopyrightProperty);
            private set => SetValue(CopyrightProperty, value);
        }
        #endregion

        public AboutWindow()
        {
            InitializeComponent();
            Owner = WindowUtils.GetActiveWindow(this);
            DataContext = this;

            App app = (App)Application.Current;
            AboutTitle = "About " + app.Title;
            AppTitle = app.Description;
            Version = app.TaggedVersion;
            Copyright = app.Copyright;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.TidyMenu(this);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}