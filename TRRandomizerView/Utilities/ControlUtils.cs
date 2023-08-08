using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TRRandomizerView.Utilities;

public static class ControlUtils
{
    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    public static ListViewItem GetItemAt(this ListView listView, System.Windows.Point clientRelativePosition)
    {
        HitTestResult hitTestResult = VisualTreeHelper.HitTest(listView, clientRelativePosition);
        DependencyObject selectedItem = null;
        if (hitTestResult != null)
        {
            selectedItem = hitTestResult.VisualHit;
            while (selectedItem != null)
            {
                if (selectedItem is ListViewItem)
                {
                    break;
                }
                selectedItem = VisualTreeHelper.GetParent(selectedItem);
            }
        }
        return selectedItem == null ? null : selectedItem as ListViewItem;
    }

    public static ImageSource ToImageSource(this Icon icon)
    {
        Bitmap bitmap = icon.ToBitmap();
        IntPtr hBitmap = bitmap.GetHbitmap();

        ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap
        (
            hBitmap,
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions()
        );

        if (!DeleteObject(hBitmap))
        {
            throw new Win32Exception();
        }

        return wpfBitmap;
    }

    public static class DefaultIcons
    {
        private static Icon _largeFolderIcon, _smallFolderIcon;

        public static Icon FolderLarge => _largeFolderIcon ?? (_largeFolderIcon = GetStockIcon(SHSIID_FOLDER, SHGSI_LARGEICON));
        public static Icon FolderSmall => _smallFolderIcon ?? (_smallFolderIcon = GetStockIcon(SHSIID_FOLDER, SHGSI_SMALLICON));

        [DllImport("shell32.dll")]
        private static extern int SHGetStockIconInfo(uint siid, uint uFlags, ref SHSTOCKICONINFO psii);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysIconIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        private const uint SHSIID_FOLDER = 0x3;
        private const uint SHGSI_ICON = 0x100;
        private const uint SHGSI_LARGEICON = 0x0;
        private const uint SHGSI_SMALLICON = 0x1;

        private static Icon GetStockIcon(uint type, uint size)
        {
            SHSTOCKICONINFO info = new SHSTOCKICONINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);

            SHGetStockIconInfo(type, SHGSI_ICON | size, ref info);

            Icon icon = (Icon)Icon.FromHandle(info.hIcon).Clone();
            DestroyIcon(info.hIcon);

            return icon;
        }
    }
}