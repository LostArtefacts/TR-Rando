using System.Drawing;
using System.Text.RegularExpressions;
using TRGE.Core;
using TRImageControl;

namespace TRRandomizerCore.Editors;

public class TRRTitleEditor
{
    private static readonly Regex _titleRegex = new(@"TITLE_[A-Z]{2}\.DDS", RegexOptions.IgnoreCase);

    private static readonly Dictionary<TRVersion, Point> _badgePositions = new()
    {
        [TRVersion.TR1] = new(645, 588),
        [TRVersion.TR2] = new(625, 665),
        [TRVersion.TR3] = new(262, 266),
    };

    public static void Stamp(TRRScript script, TRDirectoryIOArgs io)
    {
        IEnumerable<string> titleFiles = script.GetAdditionalBackupFiles()
            .Select(f => Path.GetFileName(f))
            .Where(f => _titleRegex.IsMatch(f));
        if (!titleFiles.Any())
        {
            return;
        }

        string sharedFolder = Path.GetFullPath(Path.Combine(io.BackupDirectory.FullName, "../../TRR"));
        Directory.CreateDirectory(sharedFolder);

        TRImage badge = null;
        foreach (string titleFile in titleFiles)
        {
            string cachedFile = Path.Combine(sharedFolder, 
                $"{Path.GetFileNameWithoutExtension(titleFile)}_{script.Edition.Version}{Path.GetExtension(titleFile)}");
            if (!File.Exists(cachedFile))
            {
                // This is slow with debugger attached and intensive in release, so we only want to do it once per game.
                TRImage titleImage = new(Path.Combine(io.BackupDirectory.FullName, titleFile));
                badge ??= new($"Resources/Shared/Graphics/{script.Edition.Version}badge-small.png");
                titleImage.Import(badge, _badgePositions[script.Edition.Version], true);
                titleImage.Save(cachedFile);
            }

            File.Copy(cachedFile, Path.Combine(io.WIPOutputDirectory.FullName, titleFile), true);
        }
    }
}
