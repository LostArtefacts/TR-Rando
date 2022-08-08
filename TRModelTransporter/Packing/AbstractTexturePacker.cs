using RectanglePacker;
using RectanglePacker.Events;
using RectanglePacker.Organisation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Packing
{
    public abstract class AbstractTexturePacker<E, L> : AbstractPacker<TexturedTile, TexturedTileSegment>, IDisposable
        where E : Enum
        where L : class
    {
        public L Level { get; private set; }
        public abstract uint NumLevelImages { get; }

        protected readonly string _levelClassifier;

        public IReadOnlyList<AbstractIndexedTRTexture> AllTextures => _allTextures;

        private readonly List<AbstractIndexedTRTexture> _allTextures;

        public AbstractTexturePacker()
            : this(default) { }

        public AbstractTexturePacker(L level, int maximumTiles = 16, ITextureClassifier classifier = null)
        {
            TileWidth = 256;
            TileHeight = 256;
            MaximumTiles = maximumTiles;

            Options = new PackingOptions
            {
                FillMode = PackingFillMode.Vertical,
                OrderMode = PackingOrderMode.Height,
                Order = PackingOrder.Descending,
                GroupMode = PackingGroupMode.None,
                StartMethod = PackingStartMethod.EndTile
            };

            Level = level;
            _levelClassifier = classifier == null ? string.Empty : classifier.GetClassification();

            _allTextures = new List<AbstractIndexedTRTexture>();

            if (Level != null)
            {
                _allTextures.AddRange(LoadObjectTextures());
                _allTextures.AddRange(LoadSpriteTextures());
                _allTextures.Sort(new TRTextureReverseAreaComparer());

                for (int i = 0; i < NumLevelImages; i++)
                {
                    TexturedTile tile = AddTile();
                    tile.BitmapGraphics = new BitmapGraphics(GetTile(i));
                    tile.AllowOverlapping = true; // Allow initially for the likes of Opera House - see tile 3 [128, 128]
                }

                foreach (AbstractIndexedTRTexture texture in _allTextures)
                {
                    _tiles[texture.Atlas].AddTexture(texture);
                }
            }
        }

        public PackingResult<TexturedTile, TexturedTileSegment> Pack(bool commitToLevel)
        {
            try
            {
                PackingResult<TexturedTile, TexturedTileSegment> result = Pack();

                if (result.OrphanCount == 0 && commitToLevel)
                {
                    Commit();
                    PostCommit();
                }

                return result;
            }
            catch (InvalidOperationException e)
            {
                throw new PackingException(e.Message, e);
            }
        }

        protected abstract List<AbstractIndexedTRTexture> LoadObjectTextures();
        protected abstract List<AbstractIndexedTRTexture> LoadSpriteTextures();

        public Dictionary<TexturedTile, List<TexturedTileSegment>> GetModelSegments(E modelEntity)
        {
            Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
            TRMesh[] meshes = GetModelMeshes(modelEntity);
            if (meshes != null)
            {
                IEnumerable<int> indices = GetMeshTextureIndices(meshes);
                foreach (TexturedTile tile in _tiles)
                {
                    List<TexturedTileSegment> segments = tile.GetObjectTextureIndexSegments(indices);
                    if (segments.Count > 0)
                    {
                        segmentMap[tile] = segments;
                    }
                }
            }

            return segmentMap;
        }

        public Dictionary<TexturedTile, List<TexturedTileSegment>> GetObjectTextureSegments(IEnumerable<int> indices)
        {
            Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
            foreach (TexturedTile tile in _tiles)
            {
                List<TexturedTileSegment> segments = tile.GetObjectTextureIndexSegments(indices);
                if (segments.Count > 0)
                {
                    segmentMap[tile] = segments;
                }
            }

            return segmentMap;
        }

        protected abstract TRMesh[] GetModelMeshes(E modelEntity);

        public Dictionary<TexturedTile, List<TexturedTileSegment>> GetSpriteSegments(E entity)
        {
            Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
            TRSpriteSequence sequence = GetSpriteSequence(entity);
            if (sequence != null)
            {
                List<int> indices = new List<int>();
                for (int j = 0; j < sequence.NegativeLength * -1; j++)
                {
                    indices.Add(sequence.Offset + j);
                }

                foreach (TexturedTile tile in _tiles)
                {
                    List<TexturedTileSegment> segments = tile.GetSpriteTextureIndexSegments(indices);
                    if (segments.Count > 0)
                    {
                        segmentMap[tile] = segments;
                    }
                }
            }

            return segmentMap;
        }

        public Dictionary<TexturedTile, List<TexturedTileSegment>> GetSpriteTextureSegments(IEnumerable<int> indices)
        {
            Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
            foreach (TexturedTile tile in _tiles)
            {
                List<TexturedTileSegment> segments = tile.GetSpriteTextureIndexSegments(indices);
                if (segments.Count > 0)
                {
                    segmentMap[tile] = segments;
                }
            }

            return segmentMap;
        }

        protected abstract TRSpriteSequence GetSpriteSequence(E entity);

        protected IEnumerable<int> GetMeshTextureIndices(TRMesh[] meshes)
        {
            ISet<int> textureIndices = new SortedSet<int>();
            foreach (TRMesh mesh in meshes)
            {
                foreach (TRFace4 rect in mesh.TexturedRectangles)
                {
                    textureIndices.Add(rect.Texture);
                }
                foreach (TRFace3 tri in mesh.TexturedTriangles)
                {
                    textureIndices.Add(tri.Texture);
                }
            }
            return textureIndices;
        }

        public void RemoveModelSegments(IEnumerable<E> modelEntitiesToRemove, AbstractTextureRemapGroup<E, L> remapGroup)
        {
            if (remapGroup == null)
            {
                RemoveModelSegmentsChecked(modelEntitiesToRemove);
                return;
            }

            // TextureRemapGroup will have been precompiled to determine shared textures between entities, so we know
            // in advance which we cannot remove.
            foreach (E modelEntity in modelEntitiesToRemove)
            {
                Dictionary<TexturedTile, List<TexturedTileSegment>> modelSegments = GetModelSegments(modelEntity);
                foreach (TexturedTile tile in modelSegments.Keys)
                {
                    List<TexturedTileSegment> segments = modelSegments[tile];
                    for (int i = 0; i < segments.Count; i++)
                    {
                        TexturedTileSegment segment = segments[i];
                        if (remapGroup.CanRemoveRectangle(tile.Index, segment.Bounds, modelEntitiesToRemove))
                        {
                            tile.Remove(segment);
                        }
                    }
                }
            }
        }

        public void RemoveObjectTextureSegments(IEnumerable<int> indices)
        {
            foreach (TexturedTile tile in _tiles)
            {
                List<TexturedTileSegment> segments = tile.GetObjectTextureIndexSegments(indices);
                for (int i = 0; i < segments.Count; i++)
                {
                    tile.Remove(segments[i]);
                }
            }
        }

        public void RemoveSpriteTextureSegments(IEnumerable<int> indices)
        {
            foreach (TexturedTile tile in _tiles)
            {
                List<TexturedTileSegment> segments = tile.GetSpriteTextureIndexSegments(indices);
                for (int i = 0; i < segments.Count; i++)
                {
                    tile.Remove(segments[i]);
                }
            }
        }

        public void RemoveModelSegmentsChecked(IEnumerable<E> modelEntitiesToRemove)
        {
            if (modelEntitiesToRemove.Count() == 0)
            {
                return;
            }

            // Perform an exhaustive check against every other model in the level to find shared textures.

            Dictionary<E, Dictionary<TexturedTile, List<TexturedTileSegment>>> candidateSegments = new Dictionary<E, Dictionary<TexturedTile, List<TexturedTileSegment>>>();

            // First cache each segment for the models we wish to remove
            foreach (E modelEntity in modelEntitiesToRemove)
            {
                candidateSegments[modelEntity] = GetModelSegments(modelEntity);
            }

            // Load the segments for every other model in the level. For each, scan the segments of
            // the removal candidates. If any is used by any other model, the segment is no longer
            // a candidate. If the other model that shares it is in the list to remove, it will
            // remain a candidate.
            foreach (E otherEntity in GetAllModelTypes())
            {
                if (modelEntitiesToRemove.Contains(otherEntity))
                {
                    continue;
                }

                Dictionary<TexturedTile, List<TexturedTileSegment>> modelSegments = GetModelSegments(otherEntity);
                foreach (E entityToRemove in modelEntitiesToRemove)
                {
                    Dictionary<TexturedTile, List<TexturedTileSegment>> entityMap = candidateSegments[entityToRemove];
                    foreach (TexturedTile tile in entityMap.Keys)
                    {
                        if (modelSegments.ContainsKey(tile))
                        {
                            int match = entityMap[tile].FindIndex(s1 => modelSegments[tile].Any(s2 => s1 == s2));
                            if (match != -1)
                            {
                                entityMap[tile].RemoveAt(match);
                            }
                        }
                    }
                }
            }

            // Tell each tile to remove the candidate segments
            foreach (Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap in candidateSegments.Values)
            {
                foreach (TexturedTile tile in segmentMap.Keys)
                {
                    foreach (TexturedTileSegment segment in segmentMap[tile])
                    {
                        tile.Remove(segment);
                    }
                }
            }
        }

        protected abstract IEnumerable<E> GetAllModelTypes();

        public void RemoveSpriteSegments(E entity)
        {
            RemoveSpriteSegments(new E[] { entity });
        }

        public void RemoveSpriteSegments(IEnumerable<E> entitiesToRemove)
        {
            Dictionary<E, Dictionary<TexturedTile, List<TexturedTileSegment>>> candidateSegments = new Dictionary<E, Dictionary<TexturedTile, List<TexturedTileSegment>>>();

            // First cache each segment for the models we wish to remove
            foreach (E entity in entitiesToRemove)
            {
                candidateSegments[entity] = GetSpriteSegments(entity);
            }

            // Tell each tile to remove the candidate segments
            foreach (Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap in candidateSegments.Values)
            {
                foreach (TexturedTile tile in segmentMap.Keys)
                {
                    foreach (TexturedTileSegment segment in segmentMap[tile])
                    {
                        tile.Remove(segment);
                    }
                }
            }
        }

        public abstract Bitmap GetTile(int tileIndex);

        protected abstract void CreateImageSpace(uint count);

        public abstract void SetTile(int tileIndex, Bitmap bitmap);

        private void Commit()
        {
            if (_tiles.Count > NumLevelImages)
            {
                CreateImageSpace((uint)_tiles.Count - NumLevelImages);
            }

            for (int i = 0; i < _tiles.Count; i++)
            {
                TexturedTile tile = _tiles[i];
                if (!tile.BitmapChanged)
                {
                    continue;
                }

                tile.Commit();
                SetTile(i, tile.BitmapGraphics.Bitmap);
            }
        }

        protected virtual void PostCommit() { }

        public void Dispose()
        {
            foreach (TexturedTile tile in _tiles)
            {
                tile.Dispose();
            }
        }

        protected override TexturedTile CreateTile()
        {
            return new TexturedTile();
        }
    }
}