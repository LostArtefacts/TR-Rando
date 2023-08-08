using System.Collections.Generic;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Model.Animations;
using TRModelTransporter.Model.Sound;

namespace TRModelTransporter.Model.Definitions;

public class TR3ModelDefinition : AbstractTRModelDefinition<TR3Entities>
{
    public override TR3Entities Entity => (TR3Entities)Model.ID;
    public Dictionary<int, TR3PackedAnimation> Animations { get; set; }
    public ushort[] AnimationFrames { get; set; }
    public TRCinematicFrame[] CinematicFrames { get; set; }
    public Dictionary<int, TRColour4> Colours { get; set; }
    public TR3PackedSound HardcodedSound { get; set; }
    public TRMesh[] Meshes { get; set; }
    public TRMeshTreeNode[] MeshTrees { get; set; }
    public TRModel Model { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR3ModelDefinition definition && Entity == definition.Entity;
    }

    public override int GetHashCode()
    {
        return 1075520522 + Entity.GetHashCode();
    }
}
