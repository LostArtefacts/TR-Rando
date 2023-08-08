using System;
using TRRandomizerCore;

namespace TRRandomizerView.Events;

public class DataFolderEventArgs : EventArgs
{
    public string DataFolder { get; private set; }
    public TRRandomizerController Controller { get; private set; }

    public DataFolderEventArgs(string dataFolder, TRRandomizerController controller)
    {
        DataFolder = dataFolder;
        Controller = controller;
    }
}