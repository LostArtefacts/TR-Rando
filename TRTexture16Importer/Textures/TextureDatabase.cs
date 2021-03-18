using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TRTexture16Importer.Textures
{
    public class TextureDatabase : IDisposable
    {
        private readonly Dictionary<string, TextureSource> _sources;

        public TextureDatabase()
        {
            _sources = new Dictionary<string, TextureSource>();
        }

        public void Dispose()
        {
            foreach (TextureSource source in _sources.Values)
            {
                source.Dispose();
            }
        }

        public TextureSource Get(string name)
        {
            name = name.ToUpper();
            if (!_sources.ContainsKey(name))
            {
                _sources[name] = LoadSource(name);
            }

            return _sources[name];
        }

        private TextureSource LoadSource(string name)
        {
            string dir = Path.Combine(@"Resources\Textures\Source", name.Replace(".", @"\"));
            if (!Directory.Exists(dir))
            {
                throw new IOException(string.Format("Missing texture pack source folder ({0})", name));
            }

            string png = Path.Combine(dir, "Segments.png");
            if (!File.Exists(png))
            {
                throw new IOException(string.Format("Missing texture pack source PNG ({0})", png));
            }

            string mapping = Path.Combine(dir, "Segments.json");
            if (!File.Exists(mapping))
            {
                throw new IOException(string.Format("Missing texture pack source JSON ({0})", mapping));
            }

            return new TextureSource
            {
                PNGPath = png,
                ChangeSkyBox = png.ToUpper().Contains(@"\SKY\"), // TODO: make explicit in JSON
                VariantMap = JsonConvert.DeserializeObject<Dictionary<string, List<Rectangle>>>(File.ReadAllText(mapping))
            };
        }
    }
}