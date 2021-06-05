using System.Windows;
using System.Windows.Controls;

namespace TR2RandomizerView.Controls
{
    /// <summary>
    /// Interaction logic for EnemyControl.xaml
    /// </summary>
    public partial class EnemyControl : UserControl
    {
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register
        (
            "IsActive", typeof(bool), typeof(EnemyControl)
        );

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public EnemyControl()
        {
            InitializeComponent();
            _enemyControlCheck.DataContext = this;
        }
    }
}