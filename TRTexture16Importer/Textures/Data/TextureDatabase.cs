using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace TRTexture16Importer.Textures
{
    public class TextureDatabase<E> : IDisposable
        where E : Enum
    {
        private static readonly string _entityLookupPath = @"Static\entity_lookup.json";
        private static readonly string _dynamicDataPath = @"Dynamic\{0}.json";
        private static readonly string _staticDataPath = @"Static\{0}";
        private static readonly string _staticPngFile = "Segments.png";
        private static readonly string _staticDataFile = "Data.json";

        private readonly Dictionary<string, DynamicTextureSource> _dynamicSources;
        private readonly Dictionary<string, StaticTextureSource<E>> _staticSources;
        private readonly Dictionary<E, string[]> _entityMap;

        public GlobalGrouping<E> GlobalGrouping { get; private set; }

        public string DataPath { get; protected set; }

        public TextureDatabase(string dataPath)
        {
            DataPath = dataPath;
            _dynamicSources = new Dictionary<string, DynamicTextureSource>();
            _staticSources = new Dictionary<string, StaticTextureSource<E>>();
            _entityMap = JsonConvert.DeserializeObject<Dictionary<E, string[]>>(File.ReadAllText(Path.Combine(DataPath, _entityLookupPath)));
            GlobalGrouping = new GlobalGrouping<E>(this);
        }

        public void Dispose()
        {
            foreach (StaticTextureSource<E> source in _staticSources.Values)
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

        public StaticTextureSource<E> GetStaticSource(string name)
        {
            name = name.ToUpper();
            if (!_staticSources.ContainsKey(name))
            {
                _staticSources[name] = LoadStaticSource(name);
            }

            return _staticSources[name];
        }

        public StaticTextureSource<E>[] GetStaticSource(E entity)
        {
            List<StaticTextureSource<E>> sources = new List<StaticTextureSource<E>>();
            if (_entityMap.ContainsKey(entity))
            {
                foreach (string src in _entityMap[entity])
                {
                    sources.Add(GetStaticSource(src));
                }
            }
            return sources.ToArray();
        }

        private DynamicTextureSource LoadDynamicSource(string name)
        {
            string source = Path.Combine(DataPath, string.Format(_dynamicDataPath, name.Replace(".", @"\")));
            if (!File.Exists(source))
            {
                throw new IOException(string.Format("Missing texture pack source JSON ({0})", source));
            }

            return JsonConvert.DeserializeObject<DynamicTextureSource>(File.ReadAllText(source));
        }

        private StaticTextureSource<E> LoadStaticSource(string name)
        {
            string dir = Path.Combine(DataPath, string.Format(_staticDataPath, name.Replace(".", @"\")));
            if (!Directory.Exists(dir))
            {
                throw new IOException(string.Format("Missing texture pack source folder ({0})", name));
            }

            string mapping = Path.Combine(dir, _staticDataFile);
            if (!File.Exists(mapping))
            {
                throw new IOException(string.Format("Missing texture pack source JSON ({0})", mapping));
            }

            StaticTextureSource<E> source = JsonConvert.DeserializeObject<StaticTextureSource<E>>(File.ReadAllText(mapping));

            // PNG paths may be set within the data, but if not look for the file in the same folder.
            if (source.PNGPath == null)
            {
                source.PNGPath = Path.Combine(dir, _staticPngFile);
            }

            if (!File.Exists(source.PNGPath))
            {
                throw new IOException(string.Format("Missing texture pack source PNG ({0})", source.PNGPath));
            }

            return source;
        }
    }
}