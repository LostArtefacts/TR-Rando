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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace TR2Randomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SecretReplacer _replacer;
        private ItemRandomizer _itemrandomizer;

        public MainWindow()
        {
            InitializeComponent();

            _replacer = new SecretReplacer();
            _itemrandomizer = new ItemRandomizer();

            ReplacementStatusManager.CanRandomize = true;
            ReplacementStatusManager.LevelProgress = 0;
            ReplacementStatusManager.AllowHard = false;
        }

        private void SecretsSeedEntry_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ItemsSeedEntry_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void EnemiesSeedEntry_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void RandomizeSecrets_Click(object sender, RoutedEventArgs e)
        {
            if (SecretsSeedEntry.Text == string.Empty)
            {
                _replacer.PlaceAll = true;

                Thread ReplaceThread = new Thread(() => _replacer.Replace(1));
                ReplaceThread.Start();
            }
            else
            {
                _replacer.PlaceAll = false;

                int seed = Convert.ToInt32(SecretsSeedEntry.Text);

                Thread ReplaceThread = new Thread(() => _replacer.Replace(seed));
                ReplaceThread.Start();
            }
        }

        private void RandomizeItems_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsSeedEntry.Text != string.Empty)
            {
                int seed = Convert.ToInt32(ItemsSeedEntry.Text);

                Thread ItemRandomizeThread = new Thread(() => _itemrandomizer.Randomize(seed));
                ItemRandomizeThread.Start();
            }
        }

        private void RandomizeEnemies_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TrackLaraEPC_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("getcoords.exe");
        }

        private void TrackLaraUK_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("getcoords_uk.exe");
        }
    }
}
