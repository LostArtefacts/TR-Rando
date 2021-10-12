using TRGE.Coord;

namespace TRRandomizerCore.Helpers
{
    // This is a wrapper class to avoid anything that uses core having a direct dependency on TRGE
    public class TROpenRestoreEventArgs
    {
        public int ProgressValue { get; internal set; }
        public int ProgressTarget { get; internal set; }

        public bool IsComplete => ProgressValue >= ProgressTarget;

        internal void Copy(TRBackupRestoreEventArgs e)
        {
            ProgressValue = e.ProgressValue;
            ProgressTarget = e.ProgressTarget;
        }
    }
}