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
            nameof(Value), typeof(decimal), typeof(DecimalUpDown), new PropertyMetadata((decimal)1)
        );

        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register
        (
            nameof(DecimalPlaces), typeof(int), typeof(DecimalUpDown), new PropertyMetadata(2)
        );

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register
        (
            nameof(MinValue), typeof(decimal), typeof(DecimalUpDown), new PropertyMetadata((decimal)0)
        );

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register
        (
            nameof(MaxValue), typeof(decimal), typeof(DecimalUpDown), new PropertyMetadata(decimal.MaxValue)
        );

        public event EventHandler ValueChanged;

        public decimal Value
        {
            get => (decimal)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, Clamp(value));
                SetDisplayText();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, Math.Max(1, value));
        }

        public decimal MinValue
        {
            get => (decimal)GetValue(MinValueProperty);
            set
            {
                SetValue(MinValueProperty, value);
                Value = Value;
            }
        }

        public decimal MaxValue
        {
            get => (decimal)GetValue(MaxValueProperty);
            set
            {
                SetValue(MaxValueProperty, value);
                Value = Value;
            }
        }

        #endregion

        private bool _editing;

        public DecimalUpDown()
        {
            InitializeComponent();
            _textBox.DataContext = this;
        }

        private void RepeatUpButton_Click(object sender, RoutedEventArgs e)
        {
            Value += (decimal)Math.Pow(10, DecimalPlaces * -1);
        }

        private void RepeatDownButton_Click(object sender, RoutedEventArgs e)
        {
            Value -= (decimal)Math.Pow(10, DecimalPlaces * -1);
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            object data = e.DataObject.GetData(DataFormats.UnicodeText);
            if (!decimal.TryParse(data.ToString(), out decimal _))
            {
                e.CancelCommand();
            }
        }

        private void TextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out decimal _);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            decimal mod = (decimal)Math.Pow(10, DecimalPlaces * -1);
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
            if (_editing)
            {
                return;
            }

            if (decimal.TryParse(_textBox.Text, out decimal val))
            {
                Value = val;
            }
        }

        private void SetDisplayText()
        {
            _editing = true;
            _textBox.Text = Value.ToString("F" + DecimalPlaces.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);
            _editing = false;
        }

        private decimal Clamp(decimal value)
        {
            return Math.Min(MaxValue, Math.Max(MinValue, value));
        }
    }
}