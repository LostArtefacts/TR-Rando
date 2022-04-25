using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TRRandomizerView.Model;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for EnemyWindow.xaml
    /// </summary>
    public partial class EnemyWindow : Window
    {
        #region Dependency Properties
        public static readonly DependencyProperty CanExcludeProperty = DependencyProperty.Register
        (
            nameof(CanExclude), typeof(bool), typeof(EnemyWindow), new PropertyMetadata(false)
        );

        public static readonly DependencyProperty CanIncludeProperty = DependencyProperty.Register
        (
            nameof(CanInclude), typeof(bool), typeof(EnemyWindow), new PropertyMetadata(false)
        );

        public static readonly DependencyProperty CanExcludeAllProperty = DependencyProperty.Register
        (
            nameof(CanExcludeAll), typeof(bool), typeof(EnemyWindow), new PropertyMetadata(false)
        );

        public static readonly DependencyProperty CanIncludeAllProperty = DependencyProperty.Register
        (
            nameof(CanIncludeAll), typeof(bool), typeof(EnemyWindow), new PropertyMetadata(false)
        );

        public static readonly DependencyProperty SelectionCanMoveUpProperty = DependencyProperty.Register
        (
            nameof(CanMoveUp), typeof(bool), typeof(EnemyWindow), new PropertyMetadata(false)
        );

        public static readonly DependencyProperty SelectionCanMoveDownProperty = DependencyProperty.Register
        (
            nameof(CanMoveDown), typeof(bool), typeof(EnemyWindow), new PropertyMetadata(false)
        );

        public static readonly DependencyProperty ShowExclusionWarningsProperty = DependencyProperty.Register
        (
            nameof(ShowExclusionWarnings), typeof(bool), typeof(EnemyWindow), new PropertyMetadata(false)
        );

        public bool CanExclude
        {
            get => (bool)GetValue(CanExcludeProperty);
            set => SetValue(CanExcludeProperty, value);
        }

        public bool CanInclude
        {
            get => (bool)GetValue(CanIncludeProperty);
            set => SetValue(CanIncludeProperty, value);
        }

        public bool CanExcludeAll
        {
            get => (bool)GetValue(CanExcludeAllProperty);
            set => SetValue(CanExcludeAllProperty, value);
        }

        public bool CanIncludeAll
        {
            get => (bool)GetValue(CanIncludeAllProperty);
            set => SetValue(CanIncludeAllProperty, value);
        }

        public bool CanMoveUp
        {
            get => (bool)GetValue(SelectionCanMoveUpProperty);
            set => SetValue(SelectionCanMoveUpProperty, value);
        }

        public bool CanMoveDown
        {
            get => (bool)GetValue(SelectionCanMoveDownProperty);
            set => SetValue(SelectionCanMoveDownProperty, value);
        }

        public bool ShowExclusionWarnings
        {
            get => (bool)GetValue(ShowExclusionWarningsProperty);
            set => SetValue(ShowExclusionWarningsProperty, value);
        }
        #endregion

        public List<BoolItemIDControlClass> Controls { get; private set; }
        private readonly ObservableCollection<BoolItemIDControlClass> _availableControls, _excludedControls;

        public EnemyWindow(ControllerOptions controller)
        {
            InitializeComponent();
            Owner = WindowUtils.GetActiveWindow(this);
            DataContext = this;

            ShowExclusionWarnings = controller.ShowExclusionWarnings;
            _availableControlList.ItemsSource = _availableControls = CloneControls(controller.SelectableEnemyControls.Where(c => !c.Value));
            _excludedControlList.ItemsSource= _excludedControls = CloneControls(controller.SelectableEnemyControls.Where(c => c.Value));

            SortAvailableControls();
            UpdateShiftButtons();

            MinHeight = Height;
            MinWidth = Width;
        }

        private ObservableCollection<BoolItemIDControlClass> CloneControls(IEnumerable<BoolItemIDControlClass> controls)
        {
            List<BoolItemIDControlClass> clones = new List<BoolItemIDControlClass>();
            foreach (BoolItemIDControlClass item in controls)
            {
                clones.Add(item.Clone());
            }

            return new ObservableCollection<BoolItemIDControlClass>(clones);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.EnableMinimiseButton(this, false);
            WindowUtils.TidyMenu(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Controls = new List<BoolItemIDControlClass>();
            Controls.AddRange(_excludedControls);
            Controls.AddRange(_availableControls);
            DialogResult = true;
        }

        private void SortAvailableControls()
        {
            List<BoolItemIDControlClass> controls = _availableControls.ToList();
            controls.Sort(delegate (BoolItemIDControlClass c1, BoolItemIDControlClass c2)
            {
                return c1.Title.CompareTo(c2.Title);
            });

            _availableControls.Clear();
            controls.ForEach(c => _availableControls.Add(c));
        }

        private void ControlList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CanExclude = _availableControlList.SelectedItems.Count > 0;
            CanInclude = _excludedControlList.SelectedItems.Count > 0;
        }

        private void ExcludeButton_Click(object sender, RoutedEventArgs e)
        {
            ShiftSelection(_availableControlList, _excludedControlList, true);
        }

        private void ExcludeAllButton_Click(object sender, RoutedEventArgs e)
        {
            _availableControlList.SelectAll();
            ExcludeButton_Click(null, null);
        }

        private void IncludeButton_Click(object sender, RoutedEventArgs e)
        {
            ShiftSelection(_excludedControlList, _availableControlList, false);
            SortAvailableControls();
        }

        private void IncludeAllButton_Click(object sender, RoutedEventArgs e)
        {
            _excludedControlList.SelectAll();
            IncludeButton_Click(null, null);
        }

        private void ShiftSelection(ListBox fromBox, ListBox toBox, bool controlValue)
        {
            // Wpf listboxes dont have a SelectedIndices property so a little fudging is needed, otherwise
            // the order in the target list box is based on which item was selected first.

            ObservableCollection<BoolItemIDControlClass> fromControls = fromBox.ItemsSource as ObservableCollection<BoolItemIDControlClass>;
            ObservableCollection<BoolItemIDControlClass> toControls = toBox.ItemsSource as ObservableCollection<BoolItemIDControlClass>;

            List<int> indices = new List<int>();
            for (int i = 0; i < fromBox.SelectedItems.Count; i++)
            {
                BoolItemIDControlClass control = (BoolItemIDControlClass)fromBox.SelectedItems[i];
                indices.Add(fromControls.IndexOf(control));
            }

            indices.Sort();

            foreach (int index in indices)
            {
                BoolItemIDControlClass control = fromControls[index];
                control.Value = controlValue;
                toControls.Add(control);
            }

            for (int i = indices.Count - 1; i >= 0; i--)
            {
                fromControls.RemoveAt(indices[i]);
            }

            fromBox.SelectedIndex = -1;
            UpdateShiftButtons();
        }

        private void UpdateShiftButtons()
        {
            CanIncludeAll = _excludedControls.Count > 0;
            CanExcludeAll = _availableControls.Count > 0;

            _excludedCount.Text = _excludedControls.Count.ToString();
            _availableCount.Text = _availableControls.Count.ToString();
        }

        private void ExcludedControlList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selection = _excludedControlList.SelectedItems.Count;
            if (selection == 0)
            {
                CanInclude = CanMoveUp = CanMoveDown = false;
            }
            else
            {
                if (selection > 0)
                {
                    CanInclude = true;
                }
                if (selection == 1)
                {
                    int selectedIndex = _excludedControlList.SelectedIndex;
                    CanMoveUp = selectedIndex > 0;
                    CanMoveDown = selectedIndex >= 0 && selectedIndex < _excludedControls.Count - 1;
                }
                else
                {
                    CanMoveUp = CanMoveDown = false;
                }
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            int i = _excludedControlList.SelectedIndex;
            SwapItems(i, i - 1);
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            int i = _excludedControlList.SelectedIndex;
            SwapItems(i, i + 1);
        }

        private void SwapItems(int i, int j)
        {
            BoolItemIDControlClass entry1 = _excludedControls[i];
            BoolItemIDControlClass entry2 = _excludedControls[j];

            _excludedControls[i] = entry2;
            _excludedControls[j] = entry1;

            _excludedControlList.SelectedIndex = j;
            _excludedControlList.Focus();
            _excludedControlList.ScrollIntoView(_excludedControlList.SelectedItem);
        }
    }
}