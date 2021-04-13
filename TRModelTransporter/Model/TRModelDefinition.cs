using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Textures;

namespace TRModelTransporter.Model
{
    public class TRModelDefinition : IDisposable
    {
        [JsonIgnore]
        public TR2Entities Entity => (TR2Entities)Model.ID;
        [JsonIgnore]
        public Bitmap Bitmap { get; set; }

        public Dictionary<int, PackedAnimation> Animations { get; set; }
        public Dictionary<int, TRColour4> Colours { get; set; }
        public TR2Entities[] Dependencies { get; set; }
        public ushort[] Frames { get; set; }
        public TRMesh[] Meshes { get; set; }
        public TRMeshTreeNode[] MeshTrees { get; set; }
        public TRModel Model { get; set; }
        public Dictionary<int, List<IndexedTRObjectTexture>> ObjectTextures { get; set; }
        public Rectangle[] ObjectTextureSegments { get; set; }
        public int[] RandomTextureIndices { get; set; }

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