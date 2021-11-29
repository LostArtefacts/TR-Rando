using System.Linq;

namespace TRTexture16Importer.Textures
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