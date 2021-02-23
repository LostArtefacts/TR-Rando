using System;
using System.Windows;
using System.Windows.Controls;

namespace TR2RandomizerView.Controls
{
    /// <summary>
    /// Interaction logic for SeedControl.xaml
    /// </summary>
    public partial class SeedControl : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register
        (
            "Value", typeof(int), typeof(SeedControl), new PropertyMetadata(1)
        );

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register
        (
            "MinValue", typeof(int), typeof(SeedControl), new PropertyMetadata(1)
        );

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register
        (
            "MaxValue", typeof(int), typeof(SeedControl), new PropertyMetadata(int.MaxValue)
        );

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set
            {
                SetValue(MinValueProperty, value);
                Value = Value;
            }
        }

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set
            {
                SetValue(MaxValueProperty, value);
                Value = Value;
            }
        }
        #endregion

        public SeedControl()
        {
            InitializeComponent();
            _seedSpinner.DataContext = this;
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e)
        {
            Value = new Random().Next(MinValue, MaxValue);
        }
    }
}
