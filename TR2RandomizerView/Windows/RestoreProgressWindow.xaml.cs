using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using TR2RandomizerCore;
using TR2RandomizerCore.Helpers;
using TR2RandomizerView.Utilities;

namespace TR2RandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for RestoreProgressWindow.xaml
    /// </summary>
    public partial class RestoreProgressWindow : Window
    {
        #region Dependency Properties
        public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register
        (
            "ProgressValue", typeof(int), typeof(RestoreProgressWindow), new PropertyMetadata(0)
        );

        public static readonly DependencyProperty ProgressTargetProperty = DependencyProperty.Register
        (
            "ProgressTarget", typeof(int), typeof(RestoreProgressWindow), new PropertyMetadata(100)
        );

        public static readonly DependencyProperty ProgressDescriptionProperty = DependencyProperty.Register
        (
            "ProgressDescription", typeof(string), typeof(RestoreProgressWindow), new PropertyMetadata("Restoring original data files")
        );

        public int ProgressValue
        {
            get => (int)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        public int ProgressTarget
        {
            get => (int)GetValue(ProgressTargetProperty);
            set => SetValue(ProgressTargetProperty, value);
        }

        public string ProgressDescription
        {
            get => (string)GetValue(ProgressDescriptionProperty);
            set => SetValue(ProgressDescriptionProperty, value);
        }
        #endregion

        private volatile bool _complete;
        private readonly TR2RandomizerController _controller;

        public Exception RestoreException { get; private set; }

        public RestoreProgressWindow(TR2RandomizerController controller)
        {
            InitializeComponent();
            Owner = WindowUtils.GetActiveWindow(this);
            DataContext = this;
            _complete = false;
            _controller = controller;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.EnableCloseButton(this, false);
            WindowUtils.TidyMenu(this);
            new Thread(Restore).Start();
        }

        private void Restore()
        {
            _controller.RestoreProgressChanged += Controller_RestoreProgressChanged;

            try
            {
                _controller.Restore();
            }
            catch (Exception e)
            {
                RestoreException = e;
            }
            finally
            {
                _controller.RestoreProgressChanged -= Controller_RestoreProgressChanged;

                Dispatcher.Invoke(delegate
                {
                    Dispatcher.Invoke(delegate
                    {
                        _complete = true;
                        WindowUtils.EnableCloseButton(this, true);
                        DialogResult = RestoreException == null;
                    });
                });
            }
        }

        private void Controller_RestoreProgressChanged(object sender, TROpenRestoreEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                ProgressTarget = e.ProgressTarget;
                ProgressValue = e.ProgressValue;
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_complete)
            {
                e.Cancel = true;
            }
        }
    }
}