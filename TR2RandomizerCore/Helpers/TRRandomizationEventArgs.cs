using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRGE.Core;

namespace TR2RandomizerCore.Helpers
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
                case TRSaveCategory.LevelFile:
                    return TRRandomizationCategory.Script;
                case TRSaveCategory.Custom:
                    return TRRandomizationCategory.Randomize;
                case TRSaveCategory.Cancel:
                    return TRRandomizationCategory.Cancel;
                case TRSaveCategory.Commit:
                    return TRRandomizationCategory.Commit;
                default:
                    return TRRandomizationCategory.None;
            }
        }
    }
}