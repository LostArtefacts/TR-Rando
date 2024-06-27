using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TRRandomizerView.Commands;
using TRRandomizerView.Utilities;

namespace TRRandomizerView.Windows;

public partial class MessageWindow : Window
{
    #region Dependency Properties
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register
    (
        nameof(Message), typeof(string), typeof(MessageWindow)
    );

    public static readonly DependencyProperty DetailsProperty = DependencyProperty.Register
    (
        nameof(Details), typeof(string), typeof(MessageWindow)
    );

    public static readonly DependencyProperty HelpURLProperty = DependencyProperty.Register
    (
        nameof(HelpURL), typeof(string), typeof(MessageWindow)
    );

    public static readonly DependencyProperty ImageIconProperty = DependencyProperty.Register
    (
        nameof(ImageIcon), typeof(BitmapSource), typeof(MessageWindow)
    );

    public static readonly DependencyProperty OkButtonVisibilityProperty = DependencyProperty.Register
    (
        nameof(OkButtonVisibility), typeof(Visibility), typeof(MessageWindow)
    );

    public static readonly DependencyProperty DetailsButtonVisibilityProperty = DependencyProperty.Register
    (
        nameof(DetailsButtonVisibility), typeof(Visibility), typeof(MessageWindow)
    );

    public static readonly DependencyProperty ErrorLinkVisibilityProperty = DependencyProperty.Register
    (
        nameof(ErrorLinkVisibility), typeof(Visibility), typeof(MessageWindow)
    );

    public static readonly DependencyProperty HelpLinkVisibilityProperty = DependencyProperty.Register
    (
        nameof(HelpLinkVisibility), typeof(Visibility), typeof(MessageWindow)
    );

    public static readonly DependencyProperty YesNoButtonVisibilityProperty = DependencyProperty.Register
    (
        nameof(YesNoButtonVisibility), typeof(Visibility), typeof(MessageWindow)
    );

    public static readonly DependencyProperty CancelButtonVisibilityProperty = DependencyProperty.Register
    (
        nameof(CancelButtonVisibility), typeof(Visibility), typeof(MessageWindow)
    );

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string Details
    {
        get => (string)GetValue(DetailsProperty);
        set => SetValue(DetailsProperty, value);
    }

    public string HelpURL
    {
        get => (string)GetValue(HelpURLProperty);
        set => SetValue(HelpURLProperty, value);
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

    public Visibility DetailsButtonVisibility
    {
        get => (Visibility)GetValue(DetailsButtonVisibilityProperty);
        set => SetValue(DetailsButtonVisibilityProperty, value);
    }

    public Visibility ErrorLinkVisibility
    {
        get => (Visibility)GetValue(ErrorLinkVisibilityProperty);
        set => SetValue(ErrorLinkVisibilityProperty, value);
    }

    public Visibility HelpLinkVisibility
    {
        get => (Visibility)GetValue(HelpLinkVisibilityProperty);
        set => SetValue(HelpLinkVisibilityProperty, value);
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

    private MessageWindow(string message, Icon icon, MessageBoxButton buttons, string details = null, string helpURL = null)
    {
        InitializeComponent();
        Owner = WindowUtils.GetActiveWindow(this);
        DataContext = this;
        Title = ((App)Application.Current).Title;

        Message = message;
        Details = details;
        HelpURL = helpURL ?? string.Empty;
        ImageIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        OkButtonVisibility = (buttons == MessageBoxButton.OK || buttons == MessageBoxButton.OKCancel) ? Visibility.Visible : Visibility.Collapsed;
        DetailsButtonVisibility = details == null ? Visibility.Collapsed : Visibility.Visible;
        ErrorLinkVisibility = details != null && WindowCommands.ShowErrors.CanExecute(null, Application.Current.MainWindow) ? Visibility.Visible : Visibility.Collapsed;
        HelpLinkVisibility = helpURL == null ? Visibility.Collapsed : Visibility.Visible;
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

    public static void ShowMessage(string message, string helpURL = null)
    {
        Show(message, SystemIcons.Information, MessageBoxButton.OK, helpURL: helpURL);
    }

    public static bool ShowMessageWithCancel(string message, string helpURL = null)
    {
        return Show(message, SystemIcons.Information, MessageBoxButton.OKCancel, helpURL: helpURL) == MessageBoxResult.OK;
    }

    public static void ShowWarning(string message, string helpURL = null)
    {
        Show(message, SystemIcons.Warning, MessageBoxButton.OK, helpURL: helpURL);
    }

    public static bool ShowWarningWithCancel(string message, string helpURL = null)
    {
        return Show(message, SystemIcons.Warning, MessageBoxButton.OKCancel, helpURL: helpURL) == MessageBoxResult.OK;
    }

    public static void ShowError(string message, string helpURL = null)
    {
        Show(message, SystemIcons.Error, MessageBoxButton.OK, helpURL: helpURL);
    }

    public static void ShowException(Exception e)
    {
        Show(e.Message, SystemIcons.Error, MessageBoxButton.OK, e.ToString());
    }

    public static bool ShowConfirm(string message, string helpURL = null)
    {
        return Show(message, SystemIcons.Question, MessageBoxButton.YesNo, helpURL: helpURL) == MessageBoxResult.Yes;
    }

    public static MessageBoxResult ShowConfirmCancel(string message, string helpURL = null)
    {
        return Show(message, SystemIcons.Question, MessageBoxButton.YesNoCancel, helpURL: helpURL);
    }

    private static MessageBoxResult Show(string message, Icon icon, MessageBoxButton buttons, string details = null, string helpURL = null)
    {
        MessageWindow mw = new(message, icon, buttons, details, helpURL);
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

    private void DetailsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowError(Details);
    }

    private void ErrorButton_Click(object sender, RoutedEventArgs e)
    {
        WindowCommands.ShowErrors.Execute(null, Application.Current.MainWindow);
    }

    private void HelpLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessUtils.OpenURL(HelpURL);
    }
}
