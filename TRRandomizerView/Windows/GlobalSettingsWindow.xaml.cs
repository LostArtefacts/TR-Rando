using System.Windows;
using TRRandomizerView.Model;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows;

public partial class GlobalSettingsWindow : Window
{
    public GlobalSettingsWindow(ControllerOptions options)
    {
        InitializeComponent();
        _content.DataContext = options;

        Owner = WindowUtils.GetActiveWindow(this);
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
