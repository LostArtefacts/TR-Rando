using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using TR2RandomizerView.Model;
using System.Collections.Generic;
using System.Windows.Input;
using TR2RandomizerView.Windows;
using TR2RandomizerView.Utilities;

namespace TR2RandomizerView.Controls
{
    /// <summary>
    /// Interaction logic for ManagedSeedAdvancedControl.xaml
    /// </summary>
    public partial class ManagedSeedAdvancedControl : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
        (
            nameof(Title), typeof(string), typeof(ManagedSeedAdvancedControl)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register
        (
            nameof(Text), typeof(string), typeof(ManagedSeedAdvancedControl)
        );

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register
        (
            nameof(IsActive), typeof(bool), typeof(ManagedSeedAdvancedControl)
        );

        public static readonly DependencyProperty SeedValueProperty = DependencyProperty.Register
        (
            nameof(SeedValue), typeof(int), typeof(ManagedSeedAdvancedControl)
        );

        public static readonly DependencyProperty SeedMinValueProperty = DependencyProperty.Register
        (
            nameof(SeedMinValue), typeof(int), typeof(ManagedSeedAdvancedControl), new PropertyMetadata(1)
        );

        public static readonly DependencyProperty SeedMaxValueProperty = DependencyProperty.Register
        (
            nameof(SeedMaxValue), typeof(int), typeof(ManagedSeedAdvancedControl), new PropertyMetadata(int.MaxValue)
        );

        public static readonly DependencyProperty AdvancedWindowToOpenProperty = DependencyProperty.Register
        (
            nameof(AdvancedWindowToOpen), typeof(AdvancedWindow), typeof(ManagedSeedAdvancedControl)
        );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public int SeedValue
        {
            get => (int)GetValue(SeedValueProperty);
            set => SetValue(SeedValueProperty, value);
        }

        public int SeedMinValue
        {
            get => (int)GetValue(SeedMinValueProperty);
            set => SetValue(SeedMinValueProperty, value);
        }

        public int SeedMaxValue
        {
            get => (int)GetValue(SeedMaxValueProperty);
            set => SetValue(SeedMaxValueProperty, value);
        }

        public AdvancedWindow AdvancedWindowToOpen
        {
            get => (AdvancedWindow)GetValue(AdvancedWindowToOpenProperty);
            set => SetValue(AdvancedWindowToOpenProperty, value);
        }
        #endregion

        public ManagedSeedAdvancedControl()
        {
            InitializeComponent();
            _content.DataContext = this;
        }

        private void AdvancedWindowCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;// IsEditorActive && _editorControl.CanRandomize();
        }

        private void AdvancedWindowCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (AdvancedWindowToOpen.Owner == null)
                AdvancedWindowToOpen.Owner = WindowUtils.GetActiveWindow(AdvancedWindowToOpen);
            if (AdvancedWindowToOpen.ShowDialog() ?? false)
            {

            }
        }
    }
}