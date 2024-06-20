using System.Windows;
using System.Windows.Navigation;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        Owner = WindowUtils.GetActiveWindow(this);
        DataContext = Application.Current;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        WindowUtils.TidyMenu(this);
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessUtils.OpenURL(e.Uri.AbsoluteUri);
        e.Handled = true;
    }
}
