using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TRTexture16Importer.Helpers;
using TRTexture16Importer.Textures.Source;

namespace TRTexture16Importer.Textures
{
    public class TextureDatabase : IDisposable
    {
        private readonly Dictionary<string, DynamicTextureSource> _dynamicSources;
        private readonly Dictionary<string, StaticTextureSource> _staticSources;

        public TextureDatabase()
        {
            _dynamicSources = new Dictionary<string, DynamicTextureSource>();
            _staticSources = new Dictionary<string, StaticTextureSource>();
        }

        public void Dispose()
        {
            foreach (StaticTextureSource source in _staticSources.Values)
            {
                source.Dispose();
            }
        }

        public DynamicTextureSource GetDynamicSource(string name)
        {
            name = name.ToUpper();
            if (!_dynamicSources.ContainsKey(name))
            {
                _dynamicSources[name] = LoadDynamicSource(name);
            }

            return _dynamicSources[name];
        }

        public StaticTextureSource GetStaticSource(string name)
        {
            name = name.ToUpper();
            if (!_staticSources.ContainsKey(name))
            {
                _staticSources[name] = LoadStaticSource(name);
            }

            return _staticSources[name];
        }

        private DynamicTextureSource LoadDynamicSource(string name)
        {
            string source = @"Resources\Textures\Source\Dynamic\" + name.Replace(".", @"\") + ".json";
            if (!File.Exists(source))
            {
                throw new IOException(string.Format("Missing texture pack source JSON ({0})", source));
            }

            return new DynamicTextureSource
            {
                OperationMap = JsonConvert.DeserializeObject<Dictionary<string, HSBOperation>>(File.ReadAllText(source))
            };
        }

        private StaticTextureSource LoadStaticSource(string name)
        {
            string dir = Path.Combine(@"Resources\Textures\Source\Static", name.Replace(".", @"\"));
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

            return new StaticTextureSource
            {
                PNGPath = png,
                ChangeSkyBox = png.ToUpper().Contains(@"\SKY\"), // TODO: make explicit in JSON
                VariantMap = JsonConvert.DeserializeObject<Dictionary<string, List<Rectangle>>>(File.ReadAllText(mapping))
            };
        }
    }
}