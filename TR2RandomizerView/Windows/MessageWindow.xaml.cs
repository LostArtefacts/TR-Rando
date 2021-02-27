using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using TR2RandomizerView.Utilities;

namespace TR2RandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        #region Dependency Properties
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register
        (
            "Message", typeof(string), typeof(MessageWindow)
        );

        public static readonly DependencyProperty ImageIconProperty = DependencyProperty.Register
        (
            "ImageIcon", typeof(BitmapSource), typeof(MessageWindow)
        );

        public static readonly DependencyProperty OkButtonVisibilityProperty = DependencyProperty.Register
        (
            "OkButtonVisibility", typeof(Visibility), typeof(MessageWindow)
        );

        public static readonly DependencyProperty YesNoButtonVisibilityProperty = DependencyProperty.Register
        (
            "YesNoButtonVisibility", typeof(Visibility), typeof(MessageWindow)
        );

        public static readonly DependencyProperty CancelButtonVisibilityProperty = DependencyProperty.Register
        (
            "CancelButtonVisibility", typeof(Visibility), typeof(MessageWindow)
        );

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public BitmapSource ImageIcon
        {
            get => (BitmapSource)GetValue(ImageIconProperty);
            set => SetValue(ImageIconProperty, value);
        }

        public Visibility OkButtonVisibility
        {
            get => (Visibility)GetValue(OkButtonVisibilityProperty);
            set => SetValue(OkButtonVisibilityProperty, value);
        }

        public Visibility YesNoButtonVisibility
        {
            get => (Visibility)GetValue(YesNoButtonVisibilityProperty);
            set => SetValue(YesNoButtonVisibilityProperty, value);
        }

        public Visibility CancelButtonVisibility
        {
            get => (Visibility)GetValue(CancelButtonVisibilityProperty);
            set => SetValue(CancelButtonVisibilityProperty, value);
        }
        #endregion

        private MessageBoxResult _result;

        private MessageWindow(string message, Icon icon, MessageBoxButton buttons)
        {
            InitializeComponent();
            Owner = WindowUtils.GetActiveWindow(this);
            DataContext = this;

            Message = message;
            ImageIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            OkButtonVisibility = (buttons == MessageBoxButton.OK || buttons == MessageBoxButton.OKCancel) ? Visibility.Visible : Visibility.Collapsed;
            YesNoButtonVisibility = (buttons == MessageBoxButton.YesNo || buttons == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;
            CancelButtonVisibility = (buttons == MessageBoxButton.OKCancel || buttons == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    _result = MessageBoxResult.OK;
                    break;
                case MessageBoxButton.OKCancel:
                case MessageBoxButton.YesNoCancel:
                    _result = MessageBoxResult.Cancel;
                    break;
                case MessageBoxButton.YesNo:
                    _result = MessageBoxResult.No;
                    break;
            }
        }

        public static void ShowMessage(string message)
        {
            Show(message, SystemIcons.Information, MessageBoxButton.OK);
        }

        public static bool ShowMessageWithCancel(string message)
        {
            return Show(message, SystemIcons.Information, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        public static void ShowWarning(string message)
        {
            Show(message, SystemIcons.Warning, MessageBoxButton.OK);
        }

        public static bool ShowWarningWithCancel(string message)
        {
            return Show(message, SystemIcons.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        public static void ShowError(string message)
        {
            Show(message, SystemIcons.Error, MessageBoxButton.OK);
        }

        public static bool ShowConfirm(string message)
        {
            return Show(message, SystemIcons.Question, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        public static MessageBoxResult ShowConfirmCancel(string message)
        {
            return Show(message, SystemIcons.Question, MessageBoxButton.YesNoCancel);
        }

        private static MessageBoxResult Show(string message, Icon icon, MessageBoxButton buttons)
        {
            MessageWindow mw = new MessageWindow(message, icon, buttons);
            mw.ShowDialog();
            return mw._result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.TidyMenu(this);
            if (YesNoButtonVisibility == Visibility.Visible && CancelButtonVisibility == Visibility.Collapsed)
            {
                WindowUtils.EnableCloseButton(this, false);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.OK;
            DialogResult = true;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.Yes;
            WindowUtils.EnableCloseButton(this, true);
            DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.No;
            WindowUtils.EnableCloseButton(this, true);
            DialogResult = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WindowUtils.EnableCloseButton(this, true);
            DialogResult = false;
        }
    }
}