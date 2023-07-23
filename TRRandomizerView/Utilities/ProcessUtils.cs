using System.Diagnostics;
using System.IO;

namespace TRRandomizerView.Utilities;

public class ProcessUtils
{
    public static void OpenFile(string fileName, string arguments = null)
    {
        fileName = Path.GetFullPath(fileName);
        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(fileName)
        });
    }

    public static void OpenFolder(string folder)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = Path.GetFullPath(folder),
            UseShellExecute = true
        });
    }

    public static void OpenURL(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
