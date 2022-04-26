using TRGE.Core;

namespace TRRandomizerCore.Helpers
{
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
            switch (category)
            {
                case TRSaveCategory.Scripting:
                    return TRRandomizationCategory.Script;       // TRGE is saving script data
                case TRSaveCategory.LevelFile:
                    return TRRandomizationCategory.PreRandomize; // TRGE is applying any pre-randomization work to level files
                case TRSaveCategory.Custom:
                    return TRRandomizationCategory.Randomize;    // TR2LevelRandomizer is running
                case TRSaveCategory.Cancel:
                    return TRRandomizationCategory.Cancel;       // The operation has been cancelled externally
                case TRSaveCategory.Commit:
                    return TRRandomizationCategory.Commit;       // TRGE is commiting the changes to the original data directory
                case TRSaveCategory.Warning:
                    return TRRandomizationCategory.Warning;      // A processor wants to send a warning message
                default:
                    return TRRandomizationCategory.None;
            }
        }
    }
}