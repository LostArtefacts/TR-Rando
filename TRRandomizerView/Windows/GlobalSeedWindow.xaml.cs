using System.Windows;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for GlobalSeedWindow.xaml
    /// </summary>
    public partial class GlobalSeedWindow : Window
    {
        public int Seed { get; private set; }

        public GlobalSeedWindow(int minSeed, int maxSeed, int seedValue)
        {
            InitializeComponent();
            Owner = WindowUtils.GetActiveWindow(this);

            _seedControl.MinValue = minSeed;
            _seedControl.MaxValue = maxSeed;
            _seedControl.Value = seedValue;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.TidyMenu(this);
            _seedControl.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Seed = _seedControl.Value;
            DialogResult = true;
        }
    }
}
