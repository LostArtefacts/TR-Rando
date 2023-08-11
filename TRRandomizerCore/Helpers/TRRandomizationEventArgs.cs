using TRGE.Core;

namespace TRRandomizerCore.Helpers;

// This is a wrapper class to avoid anything that uses core having a direct dependency on TRGE
public class TRRandomizationEventArgs
{
    public int ProgressValue { get; private set; }
    public int ProgressTarget { get; private set; }
    public string CustomDescription { get; private set; }
    public bool IsCancelled { get; set; }
    public TRRandomizationCategory Category { get; private set; }

    internal TRRandomizationEventArgs()
    {
        IsCancelled = false;
    }

    internal void Copy(TRSaveEventArgs e)
    {
        ProgressValue = e.ProgressValue;
        ProgressTarget = e.ProgressTarget;
        CustomDescription = e.CustomDescription;
        Category = ConvertCategory(e.Category);

        if (IsCancelled != e.IsCancelled)
        {
            e.IsCancelled = IsCancelled;
        }
    }

    private static TRRandomizationCategory ConvertCategory(TRSaveCategory category)
    {
        return category switch
        {
            TRSaveCategory.Scripting => TRRandomizationCategory.Script,       // TRGE is saving script data
            TRSaveCategory.LevelFile => TRRandomizationCategory.PreRandomize, // TRGE is applying any pre-randomization work to level files
            TRSaveCategory.Custom => TRRandomizationCategory.Randomize,       // TRXRandoEditor is running
            TRSaveCategory.Cancel => TRRandomizationCategory.Cancel,          // The operation has been cancelled externally
            TRSaveCategory.Commit => TRRandomizationCategory.Commit,          // TRGE is commiting the changes to the original data directory
            TRSaveCategory.Warning => TRRandomizationCategory.Warning,        // A processor wants to send a warning message
            _ => TRRandomizationCategory.None,
        };
    }
}
