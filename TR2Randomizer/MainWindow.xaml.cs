﻿using System;
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
using TRLevelReader.Model;
using TRLevelReader;
using TRTexture16Importer;
using System.Windows.Forms;

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
        private TextureRandomizer _texrandomizer;
        private string _baseDataPath;
        private Random _rng = new Random();

        private string BaseDataPath
        {
            get => _baseDataPath;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _baseDataPath = value;
                    _replacer.BasePath = value;
                    _itemrandomizer.BasePath = value;
                    _enemyrandomizer.BasePath = value;
                    _texrandomizer.BasePath = value;
                    MainStatusBarText.Text = $"Data folder: {value}";
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _replacer = new SecretReplacer();
            _itemrandomizer = new ItemRandomizer();
            _enemyrandomizer = new EnemyRandomizer();
            _texrandomizer = new TextureRandomizer();

            ReplacementStatusManager.CanRandomize = true;
            ReplacementStatusManager.LevelProgress = 0;
            ReplacementStatusManager.AllowHard = false;

            RandomizeAllSeeds();
        }

        private void MainWindow_OnLoad(object sender, RoutedEventArgs e)
        {
            BaseDataPath = Directory.GetCurrentDirectory();
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

        private int GenerateSeed()
        {
            const int max = 1000000000;
            return _rng.Next(0, max);
        }

        private void RandomizeEnemiesSeed()
        {
            EnemiesSeedEntry.Text = GenerateSeed().ToString();
        }

        private void RandomizeItemsSeed()
        {
            ItemsSeedEntry.Text = GenerateSeed().ToString();
        }

        private void RandomizeSecretsSeed()
        {
            SecretsSeedEntry.Text = GenerateSeed().ToString();
        }

        private void RandomizeTexturesSeed()
        {
            TextureSeedEntry.Text = GenerateSeed().ToString();
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
                string level = TR2LevelNames.AsList[ImportLevel.SelectedIndex];

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

                        if (!Locations[level].Any(n => (n.X == NewLocation.X) && (n.Y == NewLocation.Y) && (n.Z == NewLocation.Z) && (n.Room == NewLocation.Room)))
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

                        if (!Locations[level].Any( n => (n.X == NewLocation.X) && (n.Y == NewLocation.Y) && (n.Z == NewLocation.Z) && (n.Room == NewLocation.Room)))
                        {
                            Locations[level].Add(NewLocation);
                        }
                    }

                    File.WriteAllText("item_locations.json", JsonConvert.SerializeObject(Locations, Formatting.Indented));
                }
            }
        }

        private void TextureInject_Click(object sender, RoutedEventArgs e)
        {
            TR2Level instance = new TR2Level();
            TR2LevelReader reader = new TR2LevelReader();
            TR2LevelWriter writer = new TR2LevelWriter();

            string CurrentDir = Directory.GetCurrentDirectory();

            string LvlName = TR2LevelNames.AsList[ImportLevel.SelectedIndex];

            instance = reader.ReadLevel(LvlName);

            int ExtensionIndex = LvlName.IndexOf('.');

            Directory.SetCurrentDirectory(CurrentDir + "\\TexturePacks\\" + LvlName.Remove(ExtensionIndex)+ "\\" + PackDirectory.Text);

            for (int i = 0; i < instance.NumImages; i++)
            {
                instance.Images16[i].Pixels = T16Importer.ImportFrom32PNG(LvlName + i + ".png");
            }

            writer.WriteLevelToFile(instance, LvlName);

            Directory.SetCurrentDirectory(CurrentDir);
        }

        private void TextureSeedEntry_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void RandomizeTextures_Click(object sender, RoutedEventArgs e)
        {
            if (TextureSeedEntry.Text != string.Empty)
            {
                int seed = Convert.ToInt32(TextureSeedEntry.Text);

                Thread TextureRandomizeThread = new Thread(() => _texrandomizer.Randomize(seed));
                TextureRandomizeThread.Start();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.SelectedPath))
                BaseDataPath = dialog.SelectedPath;
        }

        private void RandomizeAllSeeds()
        {
            RandomizeSecretsSeed();
            RandomizeItemsSeed();
            RandomizeEnemiesSeed();
            RandomizeTexturesSeed();
        }

        private void RandomizeAllSeeds_Click(object sender, RoutedEventArgs e)
        {
            RandomizeAllSeeds();
        }
    }
}
