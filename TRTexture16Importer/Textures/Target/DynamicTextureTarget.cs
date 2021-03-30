using System.Collections.Generic;

namespace TRTexture16Importer.Textures.Target
{
    public class DynamicTextureTarget
    {
        public List<int> ObjectTextureIndices { get; set; }
        public List<int> SpriteTextureIndices { get; set; }

        public DynamicTextureTarget()
        {
            ObjectTextureIndices = new List<int>();
            SpriteTextureIndices = new List<int>();
        }
    }
}