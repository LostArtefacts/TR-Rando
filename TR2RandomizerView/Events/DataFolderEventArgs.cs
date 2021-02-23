using System;
using TR2RandomizerCore;

namespace TR2RandomizerView.Events
{
    public class DataFolderEventArgs : EventArgs
    {
        public string DataFolder { get; private set; }
        public TR2RandomizerController Controller { get; private set; }

        public DataFolderEventArgs(string dataFolder, TR2RandomizerController controller)
        {
            DataFolder = dataFolder;
            Controller = controller;
        }
    }
}