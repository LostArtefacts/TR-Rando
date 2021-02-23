using System;

namespace TR2RandomizerView.Events
{
    public class EditorEventArgs : EventArgs
    {
        public bool IsDirty { get; set; }
        public bool CanExport { get; set; }
        public bool ReloadRequested { get; set; }
    }
}