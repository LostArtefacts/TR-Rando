using System.Collections.Generic;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Animations;
using TRModelTransporter.Model.Sound;

namespace TRModelTransporter.Model.Definitions
{
    public class TR2ModelDefinition : AbstractTRModelDefinition<TR2Entities>
    {
        public override TR2Entities Entity => (TR2Entities)Model.ID;
        public Dictionary<int, TR2PackedAnimation> Animations { get; set; }
        public ushort[] AnimationFrames { get; set; }
        public TRCinematicFrame[] CinematicFrames { get; set; }
        public Dictionary<int, TRColour4> Colours { get; set; }
        public TR2PackedSound HardcodedSound { get; set; }
        public TRMesh[] Meshes { get; set; }
        public TRMeshTreeNode[] MeshTrees { get; set; }
        public TRModel Model { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TR2ModelDefinition definition && Entity == definition.Entity;
        }

        public override int GetHashCode()
        {
            return 1875520522 + Entity.GetHashCode();
        }
    }
}