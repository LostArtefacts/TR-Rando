using TRLevelControl.Model;

namespace TRDataControl;

public abstract class TRMassExporter<L, T, S, B>
    where L : TRLevelBase
    where T : Enum
    where S : Enum
    where B : TRBlobBase<T>
{
    public abstract Dictionary<string, List<T>> Data { get; }

    private TRDataExporter<L, T, S, B> _exporter;
    private List<T> _processedTypes;

    public void Export(string levelFileDirectory, string exportDirectory)
    {
        _exporter = CreateExporter();
        _exporter.DataFolder = exportDirectory;
        _exporter.BaseLevelDirectory = levelFileDirectory;
        _processedTypes = new();

        foreach (string level in Data.Keys)
        {
            _exporter.LevelName = level;
            string levelPath = Path.Combine(levelFileDirectory, level);
            foreach (T type in Data[level])
            {
                Export(levelPath, type);
            }
        }
    }

    private void Export(string levelPath, T type)
    {
        if (_processedTypes.Contains(type))
            return;

        L level = ReadLevel(levelPath);
        B blob = _exporter.Export(level, type);
        _processedTypes.Add(type);

        foreach (T dependency in blob.Dependencies)
        {
            Export(levelPath, dependency);
        }
    }

    protected abstract TRDataExporter<L, T, S, B> CreateExporter();
    protected abstract L ReadLevel(string path);
}
