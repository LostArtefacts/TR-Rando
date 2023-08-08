using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TRRandomizerView.Model;

namespace TRRandomizerView.Controls;

public partial class ManagedSeedBoolControl : UserControl
{
    #region Dependency Properties
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
    (
        nameof(Title), typeof(string), typeof(ManagedSeedBoolControl)
    );

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register
    (
        nameof(Text), typeof(string), typeof(ManagedSeedBoolControl)
    );

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register
    (
        nameof(IsActive), typeof(bool), typeof(ManagedSeedBoolControl)
    );

    public static readonly DependencyProperty SeedValueProperty = DependencyProperty.Register
    (
        nameof(SeedValue), typeof(int), typeof(ManagedSeedBoolControl)
    );

    public static readonly DependencyProperty SeedMinValueProperty = DependencyProperty.Register
    (
        nameof(SeedMinValue), typeof(int), typeof(ManagedSeedBoolControl), new PropertyMetadata(1)
    );

    public static readonly DependencyProperty SeedMaxValueProperty = DependencyProperty.Register
    (
        nameof(SeedMaxValue), typeof(int), typeof(ManagedSeedBoolControl), new PropertyMetadata(int.MaxValue)
    );

    public static readonly DependencyProperty BoolItemsSourceProperty = DependencyProperty.Register
    (
        nameof(BoolItemsSource), typeof(List<BoolItemControlClass>), typeof(ManagedSeedBoolControl)
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

    public List<BoolItemControlClass> BoolItemsSource
    {
        get => (List<BoolItemControlClass>)GetValue(BoolItemsSourceProperty);
        set => SetValue(BoolItemsSourceProperty, value);
    }
    #endregion

    public ManagedSeedBoolControl()
    {
        InitializeComponent();
        _content.DataContext = this;
    }
}
