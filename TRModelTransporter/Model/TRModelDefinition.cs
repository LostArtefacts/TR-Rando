using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Model
{
    public class TRModelDefinition : IDisposable
    {
        [JsonIgnore]
        public TR2Entities Entity => (TR2Entities)Model.ID;
        [JsonIgnore]
        public TR2Entities Alias { get; set; }
        [JsonIgnore]
        public Bitmap Bitmap { get; set; }
        [JsonIgnore]
        public bool HasGraphics => ObjectTextures.Count > 0;
        [JsonIgnore]
        public bool IsDependencyOnly { get; set; }

        public Dictionary<int, PackedAnimation> Animations { get; set; }
        public ushort[] AnimationFrames { get; set; }
        public TRCinematicFrame[] CinematicFrames { get; set; }
        public Dictionary<int, TRColour4> Colours { get; set; }
        public TR2Entities[] Dependencies { get; set; }
        public PackedSound HardcodedSound { get; set; }
        public TRMesh[] Meshes { get; set; }
        public TRMeshTreeNode[] MeshTrees { get; set; }
        public TRModel Model { get; set; }
        public int ObjectTextureCost { get; set; }
        public Dictionary<int, List<IndexedTRObjectTexture>> ObjectTextures { get; set; }
        public Dictionary<TR2Entities, TRSpriteSequence> SpriteSequences { get; set; }
        public Dictionary<TR2Entities, Dictionary<int, List<IndexedTRSpriteTexture>>> SpriteTextures { get; set; }
        public Rectangle[] TextureSegments { get; set; }

        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
            }
        }

        public override bool Equals(object obj)
        {
            return obj is TRModelDefinition definition && Entity == definition.Entity;
        }

        public override int GetHashCode()
        {
            return 1875520522 + Entity.GetHashCode();
        }
    }
}