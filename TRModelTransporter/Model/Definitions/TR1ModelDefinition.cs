﻿using TRLevelControl.Model;
using TRModelTransporter.Model.Animations;
using TRModelTransporter.Model.Sound;

namespace TRModelTransporter.Model.Definitions;

public class TR1ModelDefinition : AbstractTRModelDefinition<TR1Type>
{
    public override TR1Type Entity => (TR1Type)Model.ID;
    public Dictionary<int, TR1PackedAnimation> Animations { get; set; }
    public ushort[] AnimationFrames { get; set; }
    public TRCinematicFrame[] CinematicFrames { get; set; }
    public Dictionary<int, TRColour> Colours { get; set; }
    public TR1PackedSound HardcodedSound { get; set; }
    public TRMesh[] Meshes { get; set; }
    public TRMeshTreeNode[] MeshTrees { get; set; }
    public TRModel Model { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR1ModelDefinition definition && Entity == definition.Entity;
    }

    public override int GetHashCode()
    {
        return 1674515507 + Entity.GetHashCode();
    }
}
