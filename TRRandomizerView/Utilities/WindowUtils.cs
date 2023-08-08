using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TRRandomizerView.Utilities;

public static class WindowUtils
{
    #region PInvoke
    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWind, bool bRevert);

    [DllImport("user32.dll")]
    private static extern IntPtr EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    [DllImport("user32.dll")]
    private static extern IntPtr RemoveMenu(IntPtr hWind, uint uPostition, uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr InsertMenu(IntPtr hWind, int iPosition, uint uFlags, int iIDNewItem, string lpNewItem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public const uint WM_SYSCOMMAND = 0x112;
    public const uint MF_BYCOMMAND = 0x0;
    public const uint MF_BYPOSITION = 0x400;
    public const uint MF_REMOVE = 0x1000;
    public const uint MF_ENABLED = 0x0;
    public const uint MF_DISABLED = 0x2;
    public const uint MF_SEPARATOR = 0x800;
    public const uint SC_CLOSE = 0xF060;
    public const int GWL_STYLE = -16;
    public const int WS_MINIMIZE = 0x20000;
    #endregion

    #region Menu Alignment
    private static FieldInfo _menuDropAlignmentField;

    public static void SetMenuAlignment()
    {
        _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
        EnsureCulturePopupAlignment();
        SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
    }

    private static void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        EnsureCulturePopupAlignment();
    }

    private static void EnsureCulturePopupAlignment()
    {
        if (SystemParameters.MenuDropAlignment && _menuDropAlignmentField != null && !CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
        {
            _menuDropAlignmentField.SetValue(null, false);
        }
    }
    #endregion

    #region Handles
    public static IntPtr GetWindowHandle(Window window)
    {
        return new WindowInteropHelper(window).Handle;
    }

    public static IntPtr GetActiveWindowHandle()
    {
        Window w = GetActiveWindow();
        return w == null ? IntPtr.Zero : GetWindowHandle(w);
    }

    /// <summary>
    /// If the application has an active (focussed) window, it will be returned. Otherwise,
    /// the most recently opened Application window will be returned. Additional checks are performed
    /// for VS Adorner Windows to exclude these.
    /// </summary>
    public static Window GetActiveWindow(Window currentWindow = null)
    {
        Window w = Application.Current.Windows.OfType<Window>().SingleOrDefault(e => e.IsActive);
        if (w == null)
        {
            WindowCollection wc = Application.Current.Windows;
            for (int i = wc.Count - 1; i >= 0; i--)
            {
                w = wc[i];
                if (w != currentWindow && !w.GetType().FullName.ToLower().Contains("adornerwindow"))
                {
                    break;
                }
            }
        }
        return w;
    }
    #endregion

    #region SystemMenu
    /**
     * Based on the folliwng system menu sequencing in Windows. Removes unneccasry entries
     * based on the Window's properties.
     * 
     * 0 Restore
     * 1 Move
     * 2 Size
     * 3 Minimise
     * 4 Maximise
     * 5 --------
     * 6 Close
     */
    public static void TidyMenu(Window w)
    {
        IntPtr h = GetSystemMenu(GetWindowHandle(w), false);
        if (w.ResizeMode == ResizeMode.NoResize)
        {
            RemoveMenu(h, 5, MF_BYPOSITION | MF_REMOVE); //Separator above Close
            RemoveMenu(h, 4, MF_BYPOSITION | MF_REMOVE); //Maximise
            RemoveMenu(h, 3, MF_BYPOSITION | MF_REMOVE); //Minimise
            RemoveMenu(h, 2, MF_BYPOSITION | MF_REMOVE); //Size
            RemoveMenu(h, 0, MF_BYPOSITION | MF_REMOVE); //Restore
        }
        else if (w.ResizeMode == ResizeMode.CanMinimize)
        {
            RemoveMenu(h, 4, MF_BYPOSITION | MF_REMOVE); //Maximise
            RemoveMenu(h, 2, MF_BYPOSITION | MF_REMOVE); //Size
            RemoveMenu(h, 0, MF_BYPOSITION | MF_REMOVE); //Restore
        }
        else
        {
            long value = GetWindowLong(h, GWL_STYLE);
            if (((int)value & ~WS_MINIMIZE) == 0)
            {
                RemoveMenu(h, 3, MF_BYPOSITION | MF_REMOVE); //Minimise
            }
        }
    }

    public static void EnableCloseButton(Window w, bool enabled)
    {
        IntPtr h = GetSystemMenu(GetWindowHandle(w), false);
        uint cmd = MF_BYCOMMAND | (enabled ? MF_ENABLED : MF_DISABLED);
        EnableMenuItem(h, SC_CLOSE, cmd);

        if (enabled)
        {
            w.Closing -= W_Closing;
        }
        else
        {
            w.Closing += W_Closing;
        }
    }

    private static void W_Closing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    public static void EnableMinimiseButton(Window w, bool enabled)
    {
        IntPtr h = GetWindowHandle(w);
        long value = GetWindowLong(h, GWL_STYLE);
        int newVal = (int)(enabled ? (value | WS_MINIMIZE) : (value & ~WS_MINIMIZE));
        SetWindowLong(h, GWL_STYLE, newVal);
    }
    #endregion
}