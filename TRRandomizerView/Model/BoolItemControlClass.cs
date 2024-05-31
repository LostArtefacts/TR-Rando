using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TRRandomizerView.Model;

public class BoolItemControlClass : DependencyObject, INotifyPropertyChanged
{
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register
    (
        nameof(IsActive), typeof(bool), typeof(BoolItemControlClass)
    );

    private bool _value;
    public bool Value
    {
        get => _value;
        set
        {
            _value = value;
            FirePropertyChanged();
        }
    }

    private bool _isAailable;
    public bool IsAvailable
    {
        get => _isAailable;
        set
        {
            _isAailable = value;
            FirePropertyChanged();
        }
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public string HelpURL { get; set; }
    public bool HasHelpURL => HelpURL != null;

    public event PropertyChangedEventHandler PropertyChanged;

    public BoolItemControlClass()
    {
        IsAvailable = true;
    }

    protected void FirePropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
