using System;
using System.Collections.Generic;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public abstract class AbstractTextureMonitorBroker<E> : IDisposable
        where E : Enum
    {
        private readonly Dictionary<string, TextureMonitor<E>> _monitors;
        private readonly TextureDatabase<E> _textureDatabase;
        private readonly object _lock;

        protected abstract Dictionary<E, E> ExpandedMonitorMap { get; }

        public AbstractTextureMonitorBroker()
        {
            _monitors = new Dictionary<string, TextureMonitor<E>>();
            _textureDatabase = CreateDatabase();
            _lock = new object();
        }

        protected abstract TextureDatabase<E> CreateDatabase();
        protected abstract E TranslateAlias(string lvlName, E entity);

        public TextureMonitor<E> CreateMonitor(string lvlName, List<E> entities = null)
        {
            lock (_lock)
            {
                if (entities == null)
                {
                    entities = new List<E>();
                }
                List<StaticTextureSource<E>> sources = GetSourcesToMonitor(entities);
                TextureMonitor<E> monitor = GetMonitor(lvlName);
                if (monitor == null)
                {
                    monitor = new TextureMonitor<E>(sources);
                    _monitors[lvlName] = monitor;
                }
                else
                {
                    monitor.AppendSources(sources);
                }

                return monitor;
            }
        }

        public void ClearMonitor(string lvlName, List<E> entities)
        {
            lock (_lock)
            {
                TextureMonitor<E> monitor = GetMonitor(lvlName);
                if (monitor == null)
                {
                    return;
                }

                List<StaticTextureSource<E>> sources = GetSourcesToMonitor(entities);
                monitor.RemoveSources(sources);
            }
        }

        private List<StaticTextureSource<E>> GetSourcesToMonitor(List<E> entities)
        {
            List<E> expandedEntities = new List<E>(entities);

            // We need to capture things like flames being imported into Boat, Opera, Skidoo and the fact that
            // the red Skidoo is available when importing MercSnomobDriver.
            if (ExpandedMonitorMap != null)
            {
                foreach (E entity in ExpandedMonitorMap.Keys)
                {
                    if (expandedEntities.Contains(entity) && !expandedEntities.Contains(ExpandedMonitorMap[entity]))
                    {
                        expandedEntities.Add(ExpandedMonitorMap[entity]);
                    }
                }
            }

            List<StaticTextureSource<E>> sources = new List<StaticTextureSource<E>>();
            foreach (E entity in expandedEntities)
            {
                foreach (StaticTextureSource<E> source in _textureDatabase.GetStaticSource(entity))
                {
                    // Does the source have any defined object texture indices we are interested in monitoring?
                    if (source.EntityTextureMap != null && !sources.Contains(source))
                    {
                        sources.Add(source);
                    }
                }
            }

            return sources;
        }

        public TextureMonitor<E> GetMonitor(string lvlName)
        {
            return _monitors.ContainsKey(lvlName) ? _monitors[lvlName] : null;
        }

        public bool RemoveMonitor(string lvlName)
        {
            lock (_lock)
            {
                return _monitors.Remove(lvlName);
            }
        }

        public Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> GetLevelMapping(string lvlName)
        {
            TextureMonitor<E> monitor = GetMonitor(lvlName);
            return monitor?.PreparedLevelMapping;
        }

        public List<E> GetIgnoredEntities(string lvlName)
        {
            TextureMonitor<E> monitor = GetMonitor(lvlName);
            if (monitor != null && monitor.RemovedTextures != null)
            {
                List<E> entities = new List<E>();
                foreach (E entity in monitor.RemovedTextures)
                {
                    entities.Add(TranslateAlias(lvlName, entity));
                }
                return entities;
            }
            return null;
        }

        public Dictionary<E, E> GetEntityMap(string lvlName)
        {
            TextureMonitor<E> monitor = GetMonitor(lvlName);
            return monitor?.EntityMap;
        }

        public void Dispose()
        {
            _textureDatabase.Dispose();
        }
    }
}