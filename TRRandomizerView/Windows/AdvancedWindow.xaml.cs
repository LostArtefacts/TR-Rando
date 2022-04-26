using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TRRandomizerCore.Helpers;
using TRRandomizerView.Model;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for AdvancedWindow.xaml
    /// </summary>
    public partial class AdvancedWindow : Window
    {
        private static readonly string _darknessPreviewPath = @"pack://application:,,,/TRRandomizer;component/Resources/Darkness/{0}.jpg";

        public static readonly DependencyProperty MainDescriptionProperty = DependencyProperty.Register
        (
            nameof(MainDescription), typeof(string), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty BoolItemsSourceProperty = DependencyProperty.Register
        (
            nameof(BoolItemsSource), typeof(List<BoolItemControlClass>), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasBoolItemsProperty = DependencyProperty.Register
        (
            nameof(HasBoolItems), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasItemDifficultyProperty = DependencyProperty.Register
        (
            nameof(HasItemDifficulty), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasDifficultyProperty = DependencyProperty.Register
        (
            nameof(HasDifficulty), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasLanguageProperty = DependencyProperty.Register
        (
            nameof(HasLanguage), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasMirroringProperty = DependencyProperty.Register
        (
            nameof(HasMirroring), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasHaircutsProperty = DependencyProperty.Register
        (
            nameof(HasHaircuts), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasInvisibilityProperty = DependencyProperty.Register
        (
            nameof(HasInvisibility), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasNightModeProperty = DependencyProperty.Register
        (
            nameof(HasNightMode), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasGlobeOptionsProperty = DependencyProperty.Register
        (
            nameof(HasGlobeOptions), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasTextureOptionsProperty = DependencyProperty.Register
        (
            nameof(HasTextureOptions), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasAudioOptionsProperty = DependencyProperty.Register
        (
            nameof(HasAudioOptions), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty HasBirdMonsterBehaviourProperty = DependencyProperty.Register
        (
            nameof(HasBirdMonsterBehaviour), typeof(bool), typeof(AdvancedWindow)
        );

        public static readonly DependencyProperty ControllerProperty = DependencyProperty.Register
        (
            nameof(ControllerProxy), typeof(ControllerOptions), typeof(AdvancedWindow)
        );

        public string MainDescription
        {
            get => (string)GetValue(MainDescriptionProperty);
            set => SetValue(MainDescriptionProperty, value);
        }

        public List<BoolItemControlClass> BoolItemsSource
        {
            get => (List<BoolItemControlClass>)GetValue(BoolItemsSourceProperty);
            set => SetValue(BoolItemsSourceProperty, value);
        }

        public bool HasBoolItems
        {
            get => (bool)GetValue(HasBoolItemsProperty);
            set => SetValue(HasBoolItemsProperty, value);
        }

        public bool HasItemDifficulty
        {
            get => (bool)GetValue(HasItemDifficultyProperty);
            set => SetValue(HasItemDifficultyProperty, value);
        }

        public bool HasDifficulty
        {
            get => (bool)GetValue(HasDifficultyProperty);
            set => SetValue(HasDifficultyProperty, value);
        }

        public bool HasLanguage
        {
            get => (bool)GetValue(HasDifficultyProperty);
            set => SetValue(HasDifficultyProperty, value);
        }

        public bool HasMirroring
        {
            get => (bool)GetValue(HasMirroringProperty);
            set => SetValue(HasMirroringProperty, value);
        }

        public bool HasHaircuts
        {
            get => (bool)GetValue(HasHaircutsProperty);
            set => SetValue(HasHaircutsProperty, value);
        }

        public bool HasInvisibility
        {
            get => (bool)GetValue(HasInvisibilityProperty);
            set => SetValue(HasInvisibilityProperty, value);
        }

        public bool HasNightMode
        {
            get => (bool)GetValue(HasNightModeProperty);
            set => SetValue(HasNightModeProperty, value);
        }

        public bool HasGlobeOptions
        {
            get => (bool)GetValue(HasGlobeOptionsProperty);
            set => SetValue(HasGlobeOptionsProperty, value);
        }

        public bool HasTextureOptions
        {
            get => (bool)GetValue(HasTextureOptionsProperty);
            set => SetValue(HasTextureOptionsProperty, value);
        }

        public bool HasAudioOptions
        {
            get => (bool)GetValue(HasAudioOptionsProperty);
            set => SetValue(HasAudioOptionsProperty, value);
        }

        public bool HasBirdMonsterBehaviour
        {
            get => (bool)GetValue(HasBirdMonsterBehaviourProperty);
            set => SetValue(HasBirdMonsterBehaviourProperty, value);
        }

        public ControllerOptions ControllerProxy
        {
            get => (ControllerOptions)GetValue(ControllerProperty);
            set => SetValue(ControllerProperty, value);
        }

        public AdvancedWindow()
        {
            InitializeComponent();
            _content.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.TidyMenu(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // This should not be here, but in some cases both radio buttons can remain unchecked on load
            // TODO: This causes opening an AdvancedWindow to automatically become "unsaved"
            if (HasDifficulty)
            {
                _unrestrictedButton.IsChecked = !(_defaultDifficultyButton.IsChecked = ControllerProxy.RandoEnemyDifficulty == RandoDifficulty.Default);
            }
            if (HasItemDifficulty)
            {
                _itemOneLimitButton.IsChecked = !(_defaultItemDifficultyButton.IsChecked = ControllerProxy.RandoItemDifficulty == ItemDifficulty.Default);
            }
            if (HasGlobeOptions)
            {
                switch (ControllerProxy.GlobeDisplay)
                {
                    case GlobeDisplayOption.Default:
                        _globeDefaultButton.IsChecked = true;
                        break;
                    case GlobeDisplayOption.Area:
                        _globeAreaButton.IsChecked = true;
                        break;
                    case GlobeDisplayOption.Level:
                        _globeLevelButton.IsChecked = true;
                        break;
                }
            }
            if (HasBirdMonsterBehaviour)
            {
                switch (ControllerProxy.BirdMonsterBehaviour)
                {
                    case BirdMonsterBehaviour.Default:
                        _defaultBirdBehaviourButton.IsChecked = true;
                        break;
                    case BirdMonsterBehaviour.Unconditional:
                        _unconditionalBirdBehaviourButton.IsChecked = true;
                        break;
                    case BirdMonsterBehaviour.Docile:
                        _docileBirdBehaviourButton.IsChecked = true;
                        break;
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _darknessPreview.Source = new BitmapImage(new Uri(string.Format(_darknessPreviewPath, ControllerProxy.NightModeDarkness)));
        }

        private void ExclusionsButton_Click(object sender, RoutedEventArgs e)
        {
            EnemyWindow ew = new EnemyWindow(ControllerProxy);
            if (ew.ShowDialog() ?? false)
            {
                ControllerProxy.SelectableEnemyControls = ew.Controls;
                ControllerProxy.ShowExclusionWarnings = ew.ShowExclusionWarnings;
            }
        }
    }
}