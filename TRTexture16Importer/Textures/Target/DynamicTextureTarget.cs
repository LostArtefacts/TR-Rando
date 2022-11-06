using System.Collections.Generic;
using System.Drawing;
using TRLevelReader.Model;

namespace TRTexture16Importer.Textures
{
    public class DynamicTextureTarget
    {
        public Dictionary<int, List<Rectangle>> DefaultTileTargets { get; set; }
        public Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>> OptionalTileTargets { get; set; }
        public List<TRMesh> ModelColourTargets { get; set; }

        public DynamicTextureTarget()
        {
            DefaultTileTargets = new Dictionary<int, List<Rectangle>>();
            OptionalTileTargets = new Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>>();
            ModelColourTargets = new List<TRMesh>();
        }
    }
}