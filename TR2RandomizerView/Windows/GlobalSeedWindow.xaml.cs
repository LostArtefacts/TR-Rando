using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TR2RandomizerView.Utilities;

namespace TR2RandomizerView.Windows
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
            Owner = WindowUtils.GetActiveWindow();

            _seedControl.MinValue = minSeed;
            _seedControl.MaxValue = maxSeed;
            _seedControl.Value = seedValue;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.TidyMenu(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Seed = _seedControl.Value;
            DialogResult = true;
        }
    }
}
