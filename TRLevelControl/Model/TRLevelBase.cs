namespace TRLevelControl.Model;

public abstract class TRLevelBase
{
    public TRVersion Version { get; set; }
    public abstract IEnumerable<TRMesh> DistinctMeshes { get; }
}
