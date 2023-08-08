using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TRRandomizerView.Controls;

public partial class NumericUpDown : UserControl
{
    #region Dependency Properties
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register
    (
        "Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(1)
    );

    public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register
    (
        "MinValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0)
    );

    public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register
    (
        "MaxValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(int.MaxValue)
    );

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, AdjustValue(value));
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

    public NumericUpDown()
    {
        InitializeComponent();
        _textBox.DataContext = this;
    }

    private void RepeatUpButton_Click(object sender, RoutedEventArgs e)
    {
        ++Value;
    }

    private void RepeatDownButton_Click(object sender, RoutedEventArgs e)
    {
        --Value;
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
        int mod = 1;
        if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            mod = 10;
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
        int v = Value;
        if (ValidateInput(_textBox.Text))
        {
            v = int.Parse(_textBox.Text);
        }
        else
        {
            e.Handled = true;
            _textBox.Text = v.ToString();
        }
        Value = v;
    }

    private static bool ValidateInput(string text)
    {
        return int.TryParse(text, out int _);
    }

    private int AdjustValue(int value)
    {
        return Math.Min(MaxValue, Math.Max(MinValue, value));
    }
}
