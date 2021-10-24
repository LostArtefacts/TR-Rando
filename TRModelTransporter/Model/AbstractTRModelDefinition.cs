using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using TRLevelReader.Model;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Model
{
    public abstract class AbstractTRModelDefinition<E> : IDisposable where E : Enum
    {
        [JsonIgnore]
        public abstract E Entity { get; }
        [JsonIgnore]
        public E Alias { get; set; }
        [JsonIgnore]
        public Bitmap Bitmap { get; set; }
        [JsonIgnore]
        public bool HasGraphics => ObjectTextures.Count > 0;
        [JsonIgnore]
        public bool IsDependencyOnly { get; set; }

        public E[] Dependencies { get; set; }        
        public Dictionary<int, List<IndexedTRObjectTexture>> ObjectTextures { get; set; }
        public Dictionary<E, Dictionary<int, List<IndexedTRSpriteTexture>>> SpriteTextures { get; set; }
        public Dictionary<E, TRSpriteSequence> SpriteSequences { get; set; }
        public Rectangle[] TextureSegments { get; set; }

        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
            }
        }
    }
}