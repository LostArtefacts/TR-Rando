using System;

namespace TRRandomizerView.Events;

public class EditorEventArgs : EventArgs
{
    public bool IsLoaded { get; set; }
    public bool IsDirty { get; set; }
    public bool CanExport { get; set; }
    public bool ReloadRequested { get; set; }
}
