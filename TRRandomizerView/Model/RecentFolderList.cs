using System.Collections.Generic;
using TRRandomizerCore;

namespace TRRandomizerView.Model;

public class RecentFolderList : List<RecentFolder>
{
    public RecentFolderList(IRecentFolderOpener folderOpener)
    {
        foreach (string folder in TRRandomizerCoord.History)
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
