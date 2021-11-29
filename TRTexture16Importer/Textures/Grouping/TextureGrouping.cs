using System;
using System.Collections.Generic;

namespace TRTexture16Importer.Textures
{
    public class TextureGrouping<E>
        where E : Enum
    {
        public StaticTextureSource<E> Leader { get; set; }
        public List<StaticTextureSource<E>> Masters { get; set; }
        public List<StaticTextureSource<E>> Followers { get; set; }
        public Dictionary<string, Dictionary<StaticTextureSource<E>, string>> ThemeAlternatives { get; set; }

        public TextureGrouping()
        {
            Masters = new List<StaticTextureSource<E>>();
            Followers = new List<StaticTextureSource<E>>();
            ThemeAlternatives = new Dictionary<string, Dictionary<StaticTextureSource<E>, string>>();
        }
    }
}