using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl.Utils;

public class TR4MassExporter : TRMassExporter<TR4Level, TR4Type, TR4SFX, TR4Blob>
{
    public override Dictionary<string, List<TR4Type>> Data => _data;

    protected override TRDataExporter<TR4Level, TR4Type, TR4SFX, TR4Blob> CreateExporter()
        => new TR4DataExporter();

    protected override TR4Level ReadLevel(string path)
        => new TR4LevelControl().Read(path);

    private static readonly Dictionary<string, List<TR4Type>> _data = new()
    {
        
    };
}
