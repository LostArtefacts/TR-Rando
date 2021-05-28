using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TRLevelReader.Model.Enums;
using TRTexture16Importer.Textures.Source;

namespace TRTexture16Importer.Textures
{
    public class TextureDatabase : IDisposable
    {
        private readonly Dictionary<string, DynamicTextureSource> _dynamicSources;
        private readonly Dictionary<string, StaticTextureSource> _staticSources;
        private readonly Dictionary<TR2Entities, string> _entityMap;

        public TextureDatabase()
        {
            _dynamicSources = new Dictionary<string, DynamicTextureSource>();
            _staticSources = new Dictionary<string, StaticTextureSource>();
            _entityMap = JsonConvert.DeserializeObject<Dictionary<TR2Entities, string>>(File.ReadAllText(@"Resources\Textures\Source\Static\entity_lookup.json"));
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

        public StaticTextureSource GetStaticSource(TR2Entities entity)
        {
            if (_entityMap.ContainsKey(entity))
            {
                return GetStaticSource(_entityMap[entity]);
            }
            return null;
        }

        private DynamicTextureSource LoadDynamicSource(string name)
        {
            string source = @"Resources\Textures\Source\Dynamic\" + name.Replace(".", @"\") + ".json";
            if (!File.Exists(source))
            {
                throw new IOException(string.Format("Missing texture pack source JSON ({0})", source));
            }

            return JsonConvert.DeserializeObject<DynamicTextureSource>(File.ReadAllText(source));
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

            string mapping = Path.Combine(dir, "Data.json");
            if (!File.Exists(mapping))
            {
                throw new IOException(string.Format("Missing texture pack source JSON ({0})", mapping));
            }

            StaticTextureSource source = JsonConvert.DeserializeObject<StaticTextureSource>(File.ReadAllText(mapping));
            source.PNGPath = png;
            return source;
        }
    }
}