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
        folder = Path.GetFullPath(folder);
        if (!Path.EndsInDirectorySeparator(folder))
        {
            folder += Path.DirectorySeparatorChar;
        }
        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true,
            Verb = "open",
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
