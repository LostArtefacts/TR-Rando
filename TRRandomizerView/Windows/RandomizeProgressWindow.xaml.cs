using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Shell;
using TRRandomizerCore;
using TRRandomizerCore.Helpers;
using TRRandomizerView.Model;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for RandomizeProgressWindow.xaml
    /// </summary>
    public partial class RandomizeProgressWindow : Window
    {
        #region Dependency Properties
        public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register
        (
            "ProgressValue", typeof(int), typeof(RandomizeProgressWindow), new PropertyMetadata(0)
        );

        public static readonly DependencyProperty ProgressTargetProperty = DependencyProperty.Register
        (
            "ProgressTarget", typeof(int), typeof(RandomizeProgressWindow), new PropertyMetadata(100)
        );

        public static readonly DependencyProperty ProgressDescriptionProperty = DependencyProperty.Register
        (
            "ProgressDescription", typeof(string), typeof(RandomizeProgressWindow), new PropertyMetadata("Preparing randomization")
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

        private readonly TRRandomizerController _controller;
        private readonly ControllerOptions _options;
        private bool _cancelPending, _cancelled;

        public RandomizeProgressWindow(TRRandomizerController controller, ControllerOptions options)
        {
            InitializeComponent();
            Owner = WindowUtils.GetActiveWindow(this);
            DataContext = this;
            _controller = controller;
            _options = options;
            _cancelPending = _cancelled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.TidyMenu(this);
            Owner.TaskbarItemInfo = new TaskbarItemInfo
            {
                ProgressState = TaskbarItemProgressState.Normal
            };
            new Thread(Randomize).Start();
        }

        private void Randomize()
        {
            _controller.RandomizationProgressChanged += Controller_RandomizationProgressChanged;
            Exception error = null;

            try
            {
                _options.Save();
                _controller.Randomize();
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                _controller.RandomizationProgressChanged -= Controller_RandomizationProgressChanged;

                Dispatcher.Invoke(delegate
                {
                    WindowUtils.EnableCloseButton(this, true);
                    if (error != null)
                    {
                        Owner.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                        MessageWindow.ShowException(error);
                        DialogResult = false;
                    }
                    else
                    {
                        DialogResult = !_cancelled;
                    }
                    Owner.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                });
            }
        }

        private void Controller_RandomizationProgressChanged(object sender, TRRandomizationEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                if (_cancelPending)
                {
                    e.IsCancelled = true;
                    _cancelPending = false;
                    _cancelled = true;
                }
                else if (e.Category == TRRandomizationCategory.Warning)
                {
                    Dispatcher.Invoke(delegate
                    {
                        Owner.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
                        if (!MessageWindow.ShowConfirm(FormatWarningMessage(e.CustomDescription)))
                        {
                            Cancel();
                        }
                        Owner.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    });
                }
                else
                {
                    ProgressTarget = e.ProgressTarget;
                    ProgressValue = e.ProgressValue;
                    Owner.TaskbarItemInfo.ProgressValue = ((double)ProgressValue) / ProgressTarget;

                    if (e.CustomDescription != null)
                    {
                        ProgressDescription = e.CustomDescription;
                    }
                    else
                    {
                        switch (e.Category)
                        {
                            case TRRandomizationCategory.Script:
                                ProgressDescription = "Randomizing script data";
                                break;
                            case TRRandomizationCategory.PreRandomize:
                                ProgressDescription = "Preparing level files";
                                break;
                            case TRRandomizationCategory.Randomize:
                                ProgressDescription = "Randomizing level files";
                                break;
                            case TRRandomizationCategory.Commit:
                                _cancelButton.IsEnabled = false;
                                WindowUtils.EnableCloseButton(this, false);
                                ProgressDescription = "Committing changes";
                                break;
                        }
                    }
                }
            });
        }

        private string FormatWarningMessage(string message)
        {
            return string.Format("{0}{1}{1}Do you wish to continue?", message, Environment.NewLine);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            _cancelPending = true;
            _cancelButton.IsEnabled = false;
            WindowUtils.EnableCloseButton(this, false);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_cancelPending && DialogResult == null)
            {
                Cancel();
                e.Cancel = true;
            }
        }
    }
}