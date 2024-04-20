using TRLevelControl.Model;
using TRModelTransporter.Model.Animations;

namespace TRModelTransporter.Model.Definitions;

public class TR2ModelDefinition : AbstractTRModelDefinition<TR2Type>
{
    public override TR2Type Entity => (TR2Type)Model.ID;
    public Dictionary<int, TR2PackedAnimation> Animations { get; set; }
    public ushort[] AnimationFrames { get; set; }
    public TRCinematicFrame[] CinematicFrames { get; set; }
    public Dictionary<int, TRColour4> Colours { get; set; }
    public List<TRMesh> Meshes { get; set; }
    public TRMeshTreeNode[] MeshTrees { get; set; }
    public TRModel Model { get; set; }
    public SortedDictionary<TR2SFX, TR2SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR2ModelDefinition definition && Entity == definition.Entity;
    }

    public override int GetHashCode()
    {
        return 1875520522 + Entity.GetHashCode();
    }
}
