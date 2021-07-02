using System.Collections.Generic;
using TRTexture16Importer.Textures.Source;

namespace TRTexture16Importer.Textures.Grouping
{
    public class TextureGrouping
    {
        public StaticTextureSource Leader { get; set; }
        public List<StaticTextureSource> Masters { get; set; }
        public List<StaticTextureSource> Followers { get; set; }
        public Dictionary<string, Dictionary<StaticTextureSource, string>> ThemeAlternatives { get; set; }

        public TextureGrouping()
        {
            Masters = new List<StaticTextureSource>();
            Followers = new List<StaticTextureSource>();
            ThemeAlternatives = new Dictionary<string, Dictionary<StaticTextureSource, string>>();
        }
    }
}