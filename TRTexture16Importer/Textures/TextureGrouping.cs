using System.Collections.Generic;
using TRTexture16Importer.Textures.Source;

namespace TRTexture16Importer.Textures
{
    public class TextureGrouping
    {
        public StaticTextureSource Leader { get; set; }
        public List<StaticTextureSource> Followers { get; set; }
        public Dictionary<string, Dictionary<StaticTextureSource, string>> ThemeAlternatives { get; set; }

        public TextureGrouping()
        {
            Followers = new List<StaticTextureSource>();
            ThemeAlternatives = new Dictionary<string, Dictionary<StaticTextureSource, string>>();
        }
    }
}