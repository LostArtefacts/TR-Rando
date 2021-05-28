using System.Collections.Generic;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;
using TRTexture16Importer.Textures.Source;
using TRTexture16Importer.Textures.Target;

namespace TR2RandomizerCore.Utilities
{
    public class TexturePositionMonitor : ITexturePositionMonitor
    {
        private readonly List<StaticTextureSource> _entitySources;

        internal Dictionary<StaticTextureSource, List<StaticTextureTarget>> PreparedLevelMapping { get; private set; }
        internal List<TR2Entities> RemovedTextures { get; private set; }

        internal TexturePositionMonitor(List<StaticTextureSource> sources)
        {
            _entitySources = sources;
        }

        public Dictionary<TR2Entities, List<int>> GetMonitoredTextureIndices()
        {
            // The keys defined in the source ObjectTextureMap are TRObjectTexture index references
            // from the original level they were extracted from. We want to track what happens to
            // these textures.
            Dictionary<TR2Entities, List<int>> entityIndices = new Dictionary<TR2Entities, List<int>>();
            foreach (StaticTextureSource source in _entitySources)
            {
                foreach (TR2Entities entity in source.EntityTextureMap.Keys)
                {
                    entityIndices[entity] = new List<int>(source.EntityTextureMap[entity].Keys); // The keys hold the texture indices, the values are the segment positions
                }
            }
            return entityIndices;
        }

        public void MonitoredTexturesPositioned(Dictionary<TR2Entities, List<PositionedTexture>> texturePositions)
        {
            PreparedLevelMapping = new Dictionary<StaticTextureSource, List<StaticTextureTarget>>();
            foreach (TR2Entities entity in texturePositions.Keys)
            {
                StaticTextureSource source = GetSource(entity);
                List<StaticTextureTarget> targets = new List<StaticTextureTarget>();
                foreach (PositionedTexture texture in texturePositions[entity])
                {
                    // We'll make a new texture target for the level this monitor is associated with
                    targets.Add(new StaticTextureTarget
                    {
                        Segment = source.EntityTextureMap[entity][texture.OriginalIndex], // this points to the associated rectangle in the source's Bitmap
                        Tile = texture.TileIndex,
                        X = texture.Position.X,
                        Y = texture.Position.Y
                    });
                }

                if (targets.Count > 0)
                {
                    if (!PreparedLevelMapping.ContainsKey(source))
                    {
                        PreparedLevelMapping[source] = new List<StaticTextureTarget>();
                    }
                    PreparedLevelMapping[source].AddRange(targets);
                }
            }
        }

        private StaticTextureSource GetSource(TR2Entities entity)
        {
            foreach (StaticTextureSource source in _entitySources)
            {
                if (source.EntityTextureMap.ContainsKey(entity))
                {
                    return source;
                }
            }
            return null;
        }

        // Keep a note of removed textures so that anything defined statically in the texture source
        // files does not get imported (e.g. if Barney is removed from GW, we don't want the randomized
        // textures to be imported).
        public void EntityTexturesRemoved(List<TR2Entities> entities)
        {
            RemovedTextures = entities;
        }
    }
}