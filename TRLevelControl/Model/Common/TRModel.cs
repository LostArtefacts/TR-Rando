namespace TRLevelControl.Model;

public class TRModel : ICloneable
{
    public List<TRAnimation> Animations { get; set; } = new();
    public List<TRMeshTreeNode> MeshTrees { get; set; } = new();
    public List<TRMesh> Meshes { get; set; } = new();

    public int TotalCommandCount => Animations.Sum(a => a.Commands.Count);
    public int TotalChangeCount => Animations.Sum(a => a.Changes.Count);
    public int TotalDispatchCount => Animations.Sum(a => a.TotalDispatchCount);
    public int TotalFrameCount => Animations.Sum(a => a.Frames.Count);

    public TRModel Clone()
    {
        return new()
        {
            Animations = new(Animations.Select(a => a.Clone())),
            MeshTrees = new(MeshTrees.Select(m => m.Clone())),
            Meshes = new(Meshes.Select(m => m.Clone())),
        };
    }

    object ICloneable.Clone()
        => Clone();
}
