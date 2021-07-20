using System.Linq;
using TRTexture16Importer.Textures.Grouping;

namespace TRTexture16Importer.Textures.Source
{
    public abstract class AbstractTextureSource
    {
        public TextureCategory[] Categories { get; set; }
        public abstract string[] Variants { get; }

        public bool IsInCategory(TextureCategory category)
        {
            return Categories != null && Categories.ToList().Contains(category);
        }
    }
}