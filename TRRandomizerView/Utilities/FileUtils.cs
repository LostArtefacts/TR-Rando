using Microsoft.Win32;
using SWF = System.Windows.Forms;

namespace TRRandomizerView.Utilities;

public class FileUtils
{
    public static string GetOpenPath(string title = null, string filter = null)
    {
        OpenFileDialog dlg = new()
        {
            Filter = filter ?? string.Empty,
            Title = title ?? "Open File"
        };

        if (dlg.ShowDialog(WindowUtils.GetActiveWindow()) ?? false)
        {
            return dlg.FileName;
        }
        return null;
    }

    public static string GetSavePath(string title = null, string filter = null, string initialName = null)
    {
        SaveFileDialog dlg = new()
        {
            Filter = filter ?? string.Empty,
            Title = title ?? "Save File",
            FileName = initialName ?? string.Empty
        };

        if (dlg.ShowDialog(WindowUtils.GetActiveWindow()) ?? false)
        {
            return dlg.FileName;
        }
        return null;
    }

    public static string GetFolderPath(string title = null)
    {
        using SWF.FolderBrowserDialog dlg = new()
        {
            Description = title ?? "Select Folder",
            UseDescriptionForTitle = true,
        };

        if (dlg.ShowDialog() == SWF.DialogResult.OK)
        {
            return dlg.SelectedPath;
        }
        return null;
    }
}
