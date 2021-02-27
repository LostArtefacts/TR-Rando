using System.Collections.Generic;
using TR2RandomizerCore;

namespace TR2RandomizerView.Model
{
    public class RecentFolderList : List<RecentFolder>
    {
        public RecentFolderList(IRecentFolderOpener folderOpener)
        {
            foreach (string folder in TR2RandomizerCoord.Instance.History)
            {
                Add(new RecentFolder(folderOpener)
                {
                    Index = Count + 1,
                    FolderPath = folder
                });
            }
        }

        public bool IsEmpty => Count == 0;
    }
}