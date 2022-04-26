using System;
using System.Collections.Generic;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public class TextureMonitor<E> : ITexturePositionMonitor<E>
        where E : Enum
    {
        private readonly List<StaticTextureSource<E>> _entitySources;

        public Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> PreparedLevelMapping { get; private set; }
        public List<E> RemovedTextures { get; private set; }

        public bool UseMirroring { get; set; }
        public bool UseNightTextures { get; set; }

        // Allow entities such as Artefacts to be defined in texture sources, but mapped to different types here
        public Dictionary<E, E> EntityMap { get; set; }

        public TextureMonitor(List<StaticTextureSource<E>> sources)
        {
            _entitySources = sources;
            EntityMap = new Dictionary<E, E>();
        }

        public void AppendSources(IEnumerable<StaticTextureSource<E>> sources)
        {
            foreach (StaticTextureSource<E> source in sources)
            {
                if (!_entitySources.Contains(source))
                {
                    _entitySources.Add(source);
                }
            }
        }

        public void RemoveSources(IEnumerable<StaticTextureSource<E>> sources)
        {
            foreach (StaticTextureSource<E> source in sources)
            {
                _entitySources.Remove(source);
                if (PreparedLevelMapping != null && PreparedLevelMapping.ContainsKey(source))
                {
                    PreparedLevelMapping.Remove(source);
                }
            }
        }

        public Dictionary<E, List<int>> GetMonitoredTextureIndices()
        {
            // The keys defined in the source ObjectTextureMap are TRObjectTexture index references
            // from the original level they were extracted from. We want to track what happens to
            // these textures.
            Dictionary<E, List<int>> entityIndices = new Dictionary<E, List<int>>();
            foreach (StaticTextureSource<E> source in _entitySources)
            {
                foreach (E entity in source.EntityTextureMap.Keys)
                {
                    if (!entityIndices.ContainsKey(entity))
                    {
                        entityIndices[entity] = new List<int>();
                    }
                    entityIndices[entity].AddRange(source.EntityTextureMap[entity].Keys); // The keys hold the texture indices, the values are the segment positions
                }
            }
            return entityIndices;
        }

        public void MonitoredTexturesPositioned(Dictionary<E, List<PositionedTexture>> texturePositions)
        {
            if (PreparedLevelMapping == null)
            {
                PreparedLevelMapping = new Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>>();
            }

            foreach (E entity in texturePositions.Keys)
            {
                StaticTextureSource<E>[] sources = GetSources(entity);
                List<StaticTextureTarget> targets = new List<StaticTextureTarget>();
                foreach (PositionedTexture texture in texturePositions[entity])
                {
                    foreach (StaticTextureSource<E> source in sources)
                    {
                        if (source.EntityTextureMap[entity].ContainsKey(texture.OriginalIndex))
                        {
                            // We'll make a new texture target for the level this monitor is associated with
                            targets.Add(new StaticTextureTarget
                            {
                                Segment = source.EntityTextureMap[entity][texture.OriginalIndex], // this points to the associated rectangle in the source's Bitmap
                                Tile = texture.TileIndex,
                                X = texture.Position.X,
                                Y = texture.Position.Y
                            });

                            if (!PreparedLevelMapping.ContainsKey(source))
                            {
                                PreparedLevelMapping[source] = new List<StaticTextureTarget>();
                            }
                            PreparedLevelMapping[source].Add(targets[targets.Count - 1]);
                        }
                    }
                }
            }
        }

        private StaticTextureSource<E>[] GetSources(E entity)
        {
            List<StaticTextureSource<E>> sources = new List<StaticTextureSource<E>>();
            foreach (StaticTextureSource<E> source in _entitySources)
            {
                if (source.EntityTextureMap.ContainsKey(entity))
                {
                    sources.Add(source);
                }
            }
            return sources.ToArray();
        }

        // Keep a note of removed textures so that anything defined statically in the texture source
        // files does not get imported (e.g. if Barney is removed from GW, we don't want the randomized
        // textures to be imported).
        public void EntityTexturesRemoved(List<E> entities)
        {
            if (RemovedTextures == null)
            {
                RemovedTextures = entities;
            }
            else
            {
                RemovedTextures.AddRange(entities);
            }
        }
    }
}