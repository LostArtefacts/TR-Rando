﻿using System.Windows;
using System.Windows.Controls;

namespace TRRandomizerView.Controls;

public partial class ManagedSeedControl : UserControl
{
    #region Dependency Properties
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
    (
        "Title", typeof(string), typeof(ManagedSeedControl)
    );

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register
    (
        "Text", typeof(string), typeof(ManagedSeedControl)
    );

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register
    (
        "IsActive", typeof(bool), typeof(ManagedSeedControl)
    );

    public static readonly DependencyProperty SeedValueProperty = DependencyProperty.Register
    (
        "SeedValue", typeof(int), typeof(ManagedSeedControl)
    );

    public static readonly DependencyProperty SeedMinValueProperty = DependencyProperty.Register
    (
        "SeedMinValue", typeof(int), typeof(ManagedSeedControl), new PropertyMetadata(1)
    );

    public static readonly DependencyProperty SeedMaxValueProperty = DependencyProperty.Register
    (
        "SeedMaxValue", typeof(int), typeof(ManagedSeedControl), new PropertyMetadata(int.MaxValue)
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
    #endregion

    public ManagedSeedControl()
    {
        InitializeComponent();
        _content.DataContext = this;
    }
}
