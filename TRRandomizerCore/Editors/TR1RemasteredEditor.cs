using TRGE.Core;

namespace TRRandomizerCore.Editors;

public class TR1RemasteredEditor : TR1ClassicEditor
{
    public TR1RemasteredEditor(TRDirectoryIOArgs io, TREdition edition)
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
