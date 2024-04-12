using TRGE.Core;
using TRRandomizerCore.Randomizers;

namespace TRRandomizerCore.Editors;

public class TR3RemasteredEditor : TR3ClassicEditor
{
    public TR3RemasteredEditor(TRDirectoryIOArgs io, TREdition edition)
        : base(io, edition) { }

    protected override int GetSaveTarget(int numLevels)
    {
        int target = 0;

        return target;
    }

    protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
    {
        
    }
}
