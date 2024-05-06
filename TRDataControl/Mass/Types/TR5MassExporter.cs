using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl.Utils;

public class TR5MassExporter : TRMassExporter<TR5Level, TR5Type, TR5SFX, TR5Blob>
{
    public override Dictionary<string, List<TR5Type>> Data => _data;

    protected override TRDataExporter<TR5Level, TR5Type, TR5SFX, TR5Blob> CreateExporter()
        => new TR5DataExporter();

    protected override TR5Level ReadLevel(string path)
        => new TR5LevelControl().Read(path);

    private static readonly Dictionary<string, List<TR5Type>> _data = new()
    {

    };
}
