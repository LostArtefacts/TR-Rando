using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TRRandomizerView.Utilities;

public static class ControlUtils
{
    public static ListViewItem GetItemAt(this ListView listView, Point clientRelativePosition)
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
}
