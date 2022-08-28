using RectanglePacker.Organisation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Model;
using TRModelTransporter.Events;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRModelTransporter.Utilities;

namespace TRModelTransporter.Handlers
{
    public abstract class AbstractTextureExportHandler<E, L, D>
        where E : Enum
        where L : class
        where D : AbstractTRModelDefinition<E>
    {
        protected const int _exportBitmapWidth = 320;
        protected const int _exportBitmapHeight = 640;

        protected L _level;
        protected D _definition;
        protected ITextureClassifier _classifier;
        protected IEnumerable<E> _spriteDependencies;
        protected IEnumerable<int> _ignoreableTextureIndices;

        protected AbstractTexturePacker<E, L> _packer;

        protected List<TexturedTileSegment> _allSegments;
        public event EventHandler<SegmentEventArgs> SegmentExported;
        public event EventHandler<TRTextureRemapEventArgs> SegmentRemapped;

        public void Export(L level, D definition, ITextureClassifier classifier, IEnumerable<E> spriteDependencies, IEnumerable<int> ignoreableTextureIndices)
        {
            _level = level;
            _definition = definition;
            _classifier = classifier;
            _spriteDependencies = spriteDependencies;
            _ignoreableTextureIndices = ignoreableTextureIndices;

            _allSegments = new List<TexturedTileSegment>();

            using (_packer = CreatePacker())
            {
                CollateSegments();
                ExportSegments();
            }
        }

        protected abstract AbstractTexturePacker<E, L> CreatePacker();

        protected abstract TRSpriteSequence GetSprite(E entity);

        protected virtual void CollateSegments()
        {
            Dictionary<TexturedTile, List<TexturedTileSegment>> textureSegments = _packer.GetModelSegments(_definition.Entity);

            TRTextureDeduplicator<E> deduplicator = new TRTextureDeduplicator<E>
            {
                SegmentMap = textureSegments,
                UpdateGraphics = false,
                SegmentRemapped = SegmentRemapped
            };
            deduplicator.Deduplicate();

            _definition.ObjectTextures = new Dictionary<int, List<IndexedTRObjectTexture>>();
            _definition.SpriteSequences = new Dictionary<E, TRSpriteSequence>();
            _definition.SpriteTextures = new Dictionary<E, Dictionary<int, List<IndexedTRSpriteTexture>>>();

            int bitmapIndex = 0;
            foreach (List<TexturedTileSegment> segments in textureSegments.Values)
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    TexturedTileSegment segment = segments[i];
                    if (!deduplicator.ShouldIgnoreSegment(_ignoreableTextureIndices, segment))
                    {
                        _allSegments.Add(segment);
                        _definition.ObjectTextures[bitmapIndex++] = new List<IndexedTRObjectTexture>(segment.Textures.Cast<IndexedTRObjectTexture>().ToArray());
                    }
                }
            }

            foreach (E spriteEntity in _spriteDependencies)
            {
                TRSpriteSequence sequence = GetSprite(spriteEntity);
                if (sequence != null)
                {
                    _definition.SpriteSequences[spriteEntity] = sequence;
                }

                Dictionary<TexturedTile, List<TexturedTileSegment>> spriteSegments = _packer.GetSpriteSegments(spriteEntity);
                _definition.SpriteTextures[spriteEntity] = new Dictionary<int, List<IndexedTRSpriteTexture>>();
                foreach (List<TexturedTileSegment> segments in spriteSegments.Values)
                {
                    for (int i = 0; i < segments.Count; i++)
                    {
                        TexturedTileSegment segment = segments[i];
                        _allSegments.Add(segment);
                        _definition.SpriteTextures[spriteEntity][bitmapIndex++] = new List<IndexedTRSpriteTexture>(segment.Textures.Cast<IndexedTRSpriteTexture>().ToArray());
                    }
                }
            }
        }

        protected virtual void ExportSegments()
        {
            if (_allSegments.Count == 0)
            {
                return;
            }

            using (DefaultTexturePacker segmentPacker = new DefaultTexturePacker())
            {
                segmentPacker.AddRectangles(_allSegments);

                segmentPacker.Options = new PackingOptions
                {
                    FillMode = PackingFillMode.Horizontal,
                    OrderMode = PackingOrderMode.Area,
                    Order = PackingOrder.Descending,
                    GroupMode = PackingGroupMode.Squares
                };
                segmentPacker.TileWidth = _exportBitmapWidth;
                segmentPacker.TileHeight = _exportBitmapHeight;
                segmentPacker.MaximumTiles = 1;

                segmentPacker.Pack();

                if (segmentPacker.OrphanedRectangles.Count > 0)
                {
                    throw new PackingException(string.Format("Failed to export textures for {0}.", _definition.Entity));
                }

                TexturedTile tile = segmentPacker.Tiles[0];
                List<Rectangle> rects = new List<Rectangle>();
                foreach (TexturedTileSegment segment in _allSegments)
                {
                    rects.Add(segment.MappedBounds);
                }

                _definition.TextureSegments = rects.ToArray();

                Rectangle region = tile.GetOccupiedRegion();
                _definition.Bitmap = tile.BitmapGraphics.Extract(region);

                foreach (TexturedTileSegment segment in _allSegments)
                {
                    SegmentExported?.Invoke(this, new SegmentEventArgs
                    {
                        SegmentIndex = segment.FirstTextureIndex,
                        Bitmap = segment.Bitmap
                    });
                }
            }
        }
    }
}