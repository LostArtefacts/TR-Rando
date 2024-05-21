namespace TRDataControl;

public class ImportResult<T>
    where T : Enum
{
    public List<T> ImportedTypes { get; set; } = new();
    public List<T> RemovedTypes { get; set; } = new();
}
