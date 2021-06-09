using System;
using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;
using TRTexture16Importer.Textures;
using TRTexture16Importer.Textures.Source;
using TRTexture16Importer.Textures.Target;

namespace TR2RandomizerCore.Utilities
{
    internal class TexturePositionMonitorBroker : IDisposable
    {
        private static readonly Dictionary<TR2Entities, TR2Entities> _expandedMonitorMap = new Dictionary<TR2Entities, TR2Entities>
        {
            [TR2Entities.MercSnowmobDriver] = TR2Entities.RedSnowmobile,
            [TR2Entities.FlamethrowerGoon] = TR2Entities.Flame_S_H,
            [TR2Entities.MarcoBartoli] = TR2Entities.Flame_S_H
        };

        private readonly Dictionary<string, TexturePositionMonitor> _monitors;
        private readonly TextureDatabase _textureDatabase;
        private readonly object _lock;

        internal TexturePositionMonitorBroker()
        {
            _monitors = new Dictionary<string, TexturePositionMonitor>();
            _textureDatabase = new TextureDatabase();
            _lock = new object();
        }

        internal TexturePositionMonitor CreateMonitor(string lvlName, List<TR2Entities> entities)
        {
            lock (_lock)
            {
                List<TR2Entities> expandedEntities = new List<TR2Entities>(entities);

                // This needs improving - it shouldn't be defined here but we need to capture 
                // things like flames being imported into Boat, Opera, Skidoo and the fact that
                // the red Skidoo is available when importing MercSnomobDriver.
                foreach (TR2Entities entity in _expandedMonitorMap.Keys)
                {
                    if (expandedEntities.Contains(entity) && !expandedEntities.Contains(_expandedMonitorMap[entity]))
                    {
                        expandedEntities.Add(_expandedMonitorMap[entity]);
                    }
                }

                List<StaticTextureSource> sources = new List<StaticTextureSource>();
                foreach (TR2Entities entity in expandedEntities)
                {
                    StaticTextureSource source = _textureDatabase.GetStaticSource(entity);
                    // Does the source have any defined object texture indices we are interested in monitoring?
                    if (source != null && source.EntityTextureMap != null && !sources.Contains(source))
                    {
                        sources.Add(source);
                    }
                }

                TexturePositionMonitor monitor = new TexturePositionMonitor(sources);
                _monitors[lvlName] = monitor;
            
                return monitor;
            }
        }

        internal TexturePositionMonitor GetMonitor(string lvlName)
        {
            return _monitors.ContainsKey(lvlName) ? _monitors[lvlName] : null;
        }

        internal bool RemoveMonitor(string lvlName)
        {
            lock (_lock)
            {
                return _monitors.Remove(lvlName);
            }
        }

        internal Dictionary<StaticTextureSource, List<StaticTextureTarget>> GetLevelMapping(string lvlName)
        {
            TexturePositionMonitor monitor = GetMonitor(lvlName);
            return monitor?.PreparedLevelMapping;
        }

        internal List<TR2Entities> GetIgnoredEntities(string lvlName)
        {
            TexturePositionMonitor monitor = GetMonitor(lvlName);
            if (monitor != null)
            {
                List<TR2Entities> entities = new List<TR2Entities>();
                foreach (TR2Entities entity in monitor.RemovedTextures)
                {
                    entities.Add(TR2EntityUtilities.GetAliasForLevel(lvlName, entity));
                }
                return entities;
            }
            return null;
        }

        public void Dispose()
        {
            _textureDatabase.Dispose();
        }
    }
}