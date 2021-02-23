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
    /// Interaction logic for ManagedSeedIntControl.xaml
    /// </summary>
    public partial class ManagedSeedIntControl : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
        (
            "Title", typeof(string), typeof(ManagedSeedIntControl)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register
        (
            "Text", typeof(string), typeof(ManagedSeedIntControl)
        );

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register
        (
            "IsActive", typeof(bool), typeof(ManagedSeedIntControl)
        );

        public static readonly DependencyProperty SeedValueProperty = DependencyProperty.Register
        (
            "SeedValue", typeof(int), typeof(ManagedSeedIntControl)
        );

        public static readonly DependencyProperty SeedMinValueProperty = DependencyProperty.Register
        (
            "SeedMinValue", typeof(int), typeof(ManagedSeedIntControl), new PropertyMetadata(1)
        );

        public static readonly DependencyProperty SeedMaxValueProperty = DependencyProperty.Register
        (
            "SeedMaxValue", typeof(int), typeof(ManagedSeedIntControl), new PropertyMetadata(int.MaxValue)
        );

        public static readonly DependencyProperty CustomIntProperty = DependencyProperty.Register
        (
            "CustomInt", typeof(int), typeof(ManagedSeedIntControl)
        );

        public static readonly DependencyProperty CustomIntTitleProperty = DependencyProperty.Register
        (
            "CustomIntTitle", typeof(string), typeof(ManagedSeedIntControl)
        );

        public static readonly DependencyProperty CustomIntMinValueProperty = DependencyProperty.Register
        (
            "CustomIntMinValue", typeof(int), typeof(ManagedSeedIntControl), new PropertyMetadata(1)
        );

        public static readonly DependencyProperty CustomIntMaxValueProperty = DependencyProperty.Register
        (
            "CustomIntMaxValue", typeof(int), typeof(ManagedSeedIntControl), new PropertyMetadata(int.MaxValue)
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

        public int CustomInt
        {
            get => (int)GetValue(CustomIntProperty);
            set => SetValue(CustomIntProperty, value);
        }

        public string CustomIntTitle
        {
            get => (string)GetValue(CustomIntTitleProperty);
            set => SetValue(CustomIntTitleProperty, value);
        }

        public int CustomIntMinValue
        {
            get => (int)GetValue(CustomIntMinValueProperty);
            set => SetValue(CustomIntMinValueProperty, value);
        }

        public int CustomIntMaxValue
        {
            get => (int)GetValue(CustomIntMaxValueProperty);
            set => SetValue(CustomIntMaxValueProperty, value);
        }
        #endregion

        public ManagedSeedIntControl()
        {
            InitializeComponent();
            _content.DataContext = this;
        }
    }
}
