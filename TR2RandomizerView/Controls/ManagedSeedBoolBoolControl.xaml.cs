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

namespace TR2RandomizerView.Controls
{
    /// <summary>
    /// Interaction logic for ManagedSeedBoolBoolControl.xaml
    /// </summary>
    public partial class ManagedSeedBoolBoolControl : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
        (
            "Title", typeof(string), typeof(ManagedSeedBoolBoolControl)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register
        (
            "Text", typeof(string), typeof(ManagedSeedBoolBoolControl)
        );

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register
        (
            "IsActive", typeof(bool), typeof(ManagedSeedBoolBoolControl)
        );

        public static readonly DependencyProperty SeedValueProperty = DependencyProperty.Register
        (
            "SeedValue", typeof(int), typeof(ManagedSeedBoolBoolControl)
        );

        public static readonly DependencyProperty SeedMinValueProperty = DependencyProperty.Register
        (
            "SeedMinValue", typeof(int), typeof(ManagedSeedBoolBoolControl), new PropertyMetadata(1)
        );

        public static readonly DependencyProperty SeedMaxValueProperty = DependencyProperty.Register
        (
            "SeedMaxValue", typeof(int), typeof(ManagedSeedBoolBoolControl), new PropertyMetadata(int.MaxValue)
        );

        public static readonly DependencyProperty CustomBoolProperty1 = DependencyProperty.Register
        (
            "CustomBool1", typeof(bool), typeof(ManagedSeedBoolBoolControl)
        );

        public static readonly DependencyProperty CustomBoolTitleProperty1 = DependencyProperty.Register
        (
            "CustomBoolTitle1", typeof(string), typeof(ManagedSeedBoolBoolControl)
        );

        public static readonly DependencyProperty CustomBoolProperty2 = DependencyProperty.Register
        (
            "CustomBool2", typeof(bool), typeof(ManagedSeedBoolBoolControl)
        );

        public static readonly DependencyProperty CustomBoolTitleProperty2 = DependencyProperty.Register
        (
            "CustomBoolTitle2", typeof(string), typeof(ManagedSeedBoolBoolControl)
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

        public bool CustomBool1
        {
            get => (bool)GetValue(CustomBoolProperty1);
            set => SetValue(CustomBoolProperty1, value);
        }

        public string CustomBoolTitle1
        {
            get => (string)GetValue(CustomBoolTitleProperty1);
            set => SetValue(CustomBoolTitleProperty1, value);
        }

        public bool CustomBool2
        {
            get => (bool)GetValue(CustomBoolProperty2);
            set => SetValue(CustomBoolProperty1, value);
        }

        public string CustomBoolTitle2
        {
            get => (string)GetValue(CustomBoolTitleProperty2);
            set => SetValue(CustomBoolTitleProperty2, value);
        }
        #endregion

        public ManagedSeedBoolBoolControl()
        {
            InitializeComponent();
            _content.DataContext = this;
        }
    }
}