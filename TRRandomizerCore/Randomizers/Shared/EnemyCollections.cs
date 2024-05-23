namespace TRRandomizerCore.Randomizers;

public class EnemyTransportCollection<T>
    where T : Enum
{
    public List<T> TypesToImport { get; set; } = new();
    public List<T> TypesToRemove { get; set; } = new();
    public T BirdMonsterGuiser { get; set; }
    public bool ImportResult { get; set; }
}

public class EnemyRandomizationCollection<T>
    where T : Enum
{
    public List<T> Available { get; set; } = new();
    public List<T> Droppable { get; set; } = new();
    public List<T> Water { get; set; } = new();
    public List<T> All { get; set; } = new();
    public T BirdMonsterGuiser { get; set; }
}
