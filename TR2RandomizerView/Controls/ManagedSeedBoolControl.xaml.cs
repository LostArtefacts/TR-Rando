using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace TR2RandomizerView.Controls
{
    /// <summary>
    /// Interaction logic for ManagedSeedBoolControl.xaml
    /// </summary>
    public partial class ManagedSeedBoolControl : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
        (
            "Title", typeof(string), typeof(ManagedSeedBoolControl)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register
        (
            "Text", typeof(string), typeof(ManagedSeedBoolControl)
        );

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register
        (
            "IsActive", typeof(bool), typeof(ManagedSeedBoolControl)
        );

        public static readonly DependencyProperty SeedValueProperty = DependencyProperty.Register
        (
            "SeedValue", typeof(int), typeof(ManagedSeedBoolControl)
        );

        public static readonly DependencyProperty SeedMinValueProperty = DependencyProperty.Register
        (
            "SeedMinValue", typeof(int), typeof(ManagedSeedBoolControl), new PropertyMetadata(1)
        );

        public static readonly DependencyProperty SeedMaxValueProperty = DependencyProperty.Register
        (
            "SeedMaxValue", typeof(int), typeof(ManagedSeedBoolControl), new PropertyMetadata(int.MaxValue)
        );

        public static readonly DependencyProperty CustomBoolProperty = DependencyProperty.Register
        (
            "CustomBool", typeof(bool), typeof(ManagedSeedBoolControl)
        );

        public static readonly DependencyProperty CustomBoolTitleProperty = DependencyProperty.Register
        (
            "CustomBoolTitle", typeof(string), typeof(ManagedSeedBoolControl)
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

        public bool CustomBool
        {
            get => (bool)GetValue(CustomBoolProperty);
            set => SetValue(CustomBoolProperty, value);
        }

        public string CustomBoolTitle
        {
            get => (string)GetValue(CustomBoolTitleProperty);
            set => SetValue(CustomBoolTitleProperty, value);
        }
        #endregion

        public ManagedSeedBoolControl()
        {
            InitializeComponent();
            _content.DataContext = this;
        }
    }
}