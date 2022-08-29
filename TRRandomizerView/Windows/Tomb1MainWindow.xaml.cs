using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TRGE.Core;
using TRRandomizerView.Model;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for Tomb1MainWindow.xaml
    /// </summary>
    public partial class Tomb1MainWindow : Window
    {
        public static readonly DependencyProperty ControllerProperty = DependencyProperty.Register
        (
            nameof(ControllerProxy), typeof(ControllerOptions), typeof(Tomb1MainWindow)
        );

        public ControllerOptions ControllerProxy
        {
            get => (ControllerOptions)GetValue(ControllerProperty);
            set => SetValue(ControllerProperty, value);
        }

        public Tomb1MainWindow(ControllerOptions proxy)
        {
            InitializeComponent();
            InitializeComboBoxes();

            _optionGroupListBox.SelectedIndex = 0;

            ControllerProxy = proxy;
            _content.DataContext = this;

            WaterColor_ValueChanged(null, EventArgs.Empty);

            Owner = WindowUtils.GetActiveWindow(this);
        }

        private void InitializeComboBoxes()
        {
            IEnumerable<TRUIColour> colours = Enum.GetValues(typeof(TRUIColour)).Cast<TRUIColour>();
            IEnumerable<TRUILocation> locations = Enum.GetValues(typeof(TRUILocation)).Cast<TRUILocation>();

            _screenshotCombo.ItemsSource = Enum.GetValues(typeof(TRScreenshotFormat)).Cast<TRScreenshotFormat>();
            _airbarColorCombo.ItemsSource = colours;
            _airbarLocationCombo.ItemsSource = locations;
            _airbarShowingModeCombo.ItemsSource = Enum.GetValues(typeof(TRAirbarMode)).Cast<TRAirbarMode>();

            _enemyHealthbarColorCombo.ItemsSource = colours;
            _enemyHealthbarLocationCombo.ItemsSource = locations;

            _healthbarColorCombo.ItemsSource = colours;
            _healthbarLocationCombo.ItemsSource = locations;
            _healthbarShowingModeCombo.ItemsSource = Enum.GetValues(typeof(TRHealthbarMode)).Cast<TRHealthbarMode>();

            _menuStyleCombo.ItemsSource = Enum.GetValues(typeof(TRMenuStyle)).Cast<TRMenuStyle>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.TidyMenu(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OptionGroupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_optionGroupListBox.SelectedIndex == -1)
            {
                _optionGroupListBox.SelectedIndex = 0;
                return;
            }

            ShowOptionGroup(_optionGroupListBox.SelectedIndex);
        }

        private void ShowOptionGroup(int index)
        {
            for (int i = 0; i < _optionContainer.Children.Count; i++)
            {
                _optionContainer.Children[i].Visibility = i == index ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void WaterColor_ValueChanged(object sender, EventArgs e)
        {
            _waterPreview.Background = new SolidColorBrush(Color.FromArgb(255, (byte)(ControllerProxy.WaterColorR * 255), (byte)(ControllerProxy.WaterColorG * 255), (byte)(ControllerProxy.WaterColorB * 255)));
        }
    }
}