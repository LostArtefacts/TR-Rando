using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TRRandomizerView.Controls
{
    /// <summary>
    /// Interaction logic for DecimalUpDown.xaml
    /// </summary>
    public partial class DecimalUpDown : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register
        (
            "Value", typeof(double), typeof(DecimalUpDown), new PropertyMetadata((double)1)
        );

        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register
        (
            "DecimalPlaces", typeof(int), typeof(DecimalUpDown), new PropertyMetadata(2)
        );

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register
        (
            "MinValue", typeof(double), typeof(DecimalUpDown), new PropertyMetadata((double)0)
        );

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register
        (
            "MaxValue", typeof(double), typeof(DecimalUpDown), new PropertyMetadata(double.MaxValue)
        );

        public event EventHandler ValueChanged;

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, AdjustValue(value));
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, Math.Max(1, value));
        }

        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set
            {
                SetValue(MinValueProperty, value);
                Value = Value;
            }
        }

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set
            {
                SetValue(MaxValueProperty, value);
                Value = Value;
            }
        }
        #endregion

        public DecimalUpDown()
        {
            InitializeComponent();
            _textBox.DataContext = this;
        }

        private void RepeatUpButton_Click(object sender, RoutedEventArgs e)
        {
            Value += Math.Pow(10, DecimalPlaces * -1);
        }

        private void RepeatDownButton_Click(object sender, RoutedEventArgs e)
        {
            Value -= Math.Pow(10, DecimalPlaces * -1);
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            object data = e.DataObject.GetData(DataFormats.UnicodeText);
            if (!ValidateInput(data.ToString()))
            {
                e.CancelCommand();
            }
        }

        private void TextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidateInput(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            double mod = Math.Pow(10, DecimalPlaces * -1);
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                mod *= 5;
            }
            switch (e.Key)
            {
                case Key.Up:
                    Value += mod;
                    break;
                case Key.Down:
                    Value -= mod;
                    break;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double v = Value;
            if (ValidateInput(_textBox.Text))
            {
                v = double.Parse(_textBox.Text);
            }
            else
            {
                e.Handled = true;
            }
            Value = v;
            _textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:0." + new string('0', DecimalPlaces) + "}", v);
        }

        private bool ValidateInput(string text)
        {
            return double.TryParse(text, out double _);
        }

        private double AdjustValue(double value)
        {
            return Math.Min(MaxValue, Math.Max(MinValue, value));
        }
    }
}