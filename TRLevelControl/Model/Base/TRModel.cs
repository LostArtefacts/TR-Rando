namespace TRLevelControl.Model;

public class TRModel : ICloneable
{
    public List<TRAnimation> Animations { get; set; } = new();
    public List<TRMeshTreeNode> MeshTrees { get; set; } = new();
    public uint ID { get; set; }

    public ushort NumMeshes { get; set; }

    public ushort StartingMesh { get; set; }

    public TRModel Clone()
    {
        return new()
        {
            Animations = new(Animations.Select(a => a.Clone())),
            MeshTrees = new(MeshTrees.Select(m => m.Clone())),
            ID = ID,
            NumMeshes = NumMeshes,
            StartingMesh = StartingMesh,
        };
    }

    object ICloneable.Clone()
        => Clone();
}
