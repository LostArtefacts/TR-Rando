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
using TR2Randomizer.Randomizers;
using TRViewInterop.Routes;
using Newtonsoft.Json;
using System.IO;
using TRLevelReader.Helpers;

namespace TR2Randomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SecretReplacer _replacer;
        private ItemRandomizer _itemrandomizer;
        private EnemyRandomizer _enemyrandomizer;

        public MainWindow()
        {
            InitializeComponent();

            _replacer = new SecretReplacer();
            _itemrandomizer = new ItemRandomizer();
            _enemyrandomizer = new EnemyRandomizer();

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

                Thread ReplaceThread = new Thread(() => _replacer.Randomize(1));
                ReplaceThread.Start();
            }
            else
            {
                _replacer.PlaceAll = false;

                int seed = Convert.ToInt32(SecretsSeedEntry.Text);

                Thread ReplaceThread = new Thread(() => _replacer.Randomize(seed));
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
            if (EnemiesSeedEntry.Text != string.Empty)
            {
                int seed = Convert.ToInt32(EnemiesSeedEntry.Text);

                Thread EnemyRandomizeThread = new Thread(() => _enemyrandomizer.Randomize(seed));
                EnemyRandomizeThread.Start();
            }
        }

        private void TrackLaraEPC_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("getcoords.exe");
        }

        private void TrackLaraUK_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("getcoords_uk.exe");
        }

        private void ImportLocations_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "route"; // Default file name
            dlg.DefaultExt = ".tvr"; // Default file extension
            dlg.Filter = "TRView Route (.tvr)|*.tvr"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;

                //Convert to a list of int locations that are scaled to TR world
                List<TRViewLocation> routeLocations = RouteToLocationsConverter.Convert(filename);

                //What level are we importing for?
                string level = LevelNames.AsList[ImportLevel.SelectedIndex];

                //What locations do we want to import for? secrets or items
                Dictionary<string, List<Location>> Locations;
                if (LocationType.SelectedIndex == 0)
                {
                    Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText("locations.json"));

                    foreach (TRViewLocation loc in routeLocations)
                    {
                        Location NewLocation = new Location
                        {
                            X = loc.X,
                            Y = loc.Y,
                            Z = loc.Z,
                            Room = loc.Room,
                            IsInRoomSpace = false,
                            Difficulty = Difficulty.Easy,
                            IsItem = false,
                            RequiresGlitch = false
                        };

                        if (!Locations[level].Any(n => (n.X == NewLocation.X) && (n.Y == NewLocation.Y) && (n.Z == NewLocation.Z)))
                        {
                            Locations[level].Add(NewLocation);
                        }
                    }

                    File.WriteAllText("locations.json", JsonConvert.SerializeObject(Locations, Formatting.Indented));
                }
                else
                {
                    Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText("item_locations.json"));

                    foreach (TRViewLocation loc in routeLocations)
                    {
                        Location NewLocation = new Location
                        {
                            X = loc.X,
                            Y = loc.Y,
                            Z = loc.Z,
                            Room = loc.Room,
                            IsInRoomSpace = false,
                            Difficulty = Difficulty.Easy,
                            IsItem = true,
                            RequiresGlitch = false
                        };

                        if (!Locations[level].Any( n => (n.X == NewLocation.X) && (n.Y == NewLocation.Y) && (n.Z == NewLocation.Z)))
                        {
                            Locations[level].Add(NewLocation);
                        }
                    }

                    File.WriteAllText("item_locations.json", JsonConvert.SerializeObject(Locations, Formatting.Indented));
                }
            }
        }
    }
}
