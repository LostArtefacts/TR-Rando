using RectanglePacker;
using RectanglePacker.Events;
using RectanglePacker.Organisation;
using System.Collections.Immutable;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl.Packing;

public abstract class TRTexturePacker<E, L> : AbstractPacker<TRTextile, TRTextileRegion>
    where E : Enum
    where L : class
{
    public L Level { get; private set; }
    public abstract int NumLevelImages { get; }

    protected readonly string _levelClassifier;

    public IReadOnlyList<TRTextileSegment> AllTextures => _allTextures;

    private readonly List<TRTextileSegment> _allTextures;

    public TRTexturePacker()
        : this(default) { }

    public TRTexturePacker(L level, int maximumTiles = 16, ITextureClassifier classifier = null)
    {
        TileWidth = TRConsts.TPageWidth;
        TileHeight = TRConsts.TPageHeight;
        MaximumTiles = maximumTiles;
        AllowEmptyPacking = true;

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

        _allTextures = new List<TRTextileSegment>();

        if (Level != null)
        {
            _allTextures.AddRange(LoadObjectTextures());
            _allTextures.AddRange(LoadSpriteTextures());
            _allTextures.Sort((s1, s2) => s2.Area.CompareTo(s1.Area));

            for (int i = 0; i < NumLevelImages; i++)
            {
                TRTextile tile = AddTile();
                tile.Image = GetTile(i);
                tile.AllowOverlapping = true; // Allow initially for the likes of Opera House - see tile 3 [128, 128]
            }

            foreach (TRTextileSegment texture in _allTextures)
            {
                _tiles[texture.Atlas].AddTexture(texture);
            }
        }
    }

    public PackingResult<TRTextile, TRTextileRegion> Pack(bool commitToLevel)
    {
        try
        {
            PackingResult<TRTextile, TRTextileRegion> result = Pack();

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

    protected abstract List<TRTextileSegment> LoadObjectTextures();
    protected abstract List<TRTextileSegment> LoadSpriteTextures();

    public Dictionary<TRTextile, List<TRTextileRegion>> GetModelSegments(E modelEntity)
    {
        Dictionary<TRTextile, List<TRTextileRegion>> segmentMap = new();
        List<TRMesh> meshes = GetModelMeshes(modelEntity);
        if (meshes != null)
        {
            ISet<int> indices = meshes.SelectMany(m => m.TexturedFaces.Select(f => (int)f.Texture)).ToImmutableSortedSet();
            foreach (TRTextile tile in _tiles)
            {
                List<TRTextileRegion> segments = tile.GetObjectTextureIndexSegments(indices);
                if (segments.Count > 0)
                {
                    segmentMap[tile] = segments;
                }
            }
        }

        return segmentMap;
    }

    public Dictionary<TRTextile, List<TRTextileRegion>> GetObjectTextureSegments(IEnumerable<int> indices)
    {
        Dictionary<TRTextile, List<TRTextileRegion>> segmentMap = new();
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> segments = tile.GetObjectTextureIndexSegments(indices);
            if (segments.Count > 0)
            {
                segmentMap[tile] = segments;
            }
        }

        return segmentMap;
    }

    protected abstract List<TRMesh> GetModelMeshes(E modelEntity);

    public Dictionary<TRTextile, List<TRTextileRegion>> GetSpriteSegments(E entity)
    {
        TRSpriteSequence sequence = GetSpriteSequence(entity);
        Dictionary<TRTextile, List<TRTextileRegion>> regionMap = new();
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> regions = tile.Rectangles
                .Where(r => r.Textures.Any(s => s is IndexedTRSpriteTexture spr && sequence.Textures.Contains(spr.Texture)))
                .ToList();
            if (regions.Count > 0)
            {
                regionMap[tile] = regions;
            }
        }

        return regionMap;
    }

    public Dictionary<TRTextile, List<TRTextileRegion>> GetSpriteTextureSegments(IEnumerable<int> indices)
    {
        Dictionary<TRTextile, List<TRTextileRegion>> segmentMap = new();
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> segments = tile.GetSpriteTextureIndexSegments(indices);
            if (segments.Count > 0)
            {
                segmentMap[tile] = segments;
            }
        }

        return segmentMap;
    }

    protected abstract TRSpriteSequence GetSpriteSequence(E entity);

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
            Dictionary<TRTextile, List<TRTextileRegion>> modelSegments = GetModelSegments(modelEntity);
            foreach (TRTextile tile in modelSegments.Keys)
            {
                List<TRTextileRegion> segments = modelSegments[tile];
                for (int i = 0; i < segments.Count; i++)
                {
                    TRTextileRegion segment = segments[i];
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
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> segments = tile.GetObjectTextureIndexSegments(indices);
            for (int i = 0; i < segments.Count; i++)
            {
                tile.Remove(segments[i]);
            }
        }
    }

    public void RemoveSpriteTextureSegments(IEnumerable<int> indices)
    {
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> segments = tile.GetSpriteTextureIndexSegments(indices);
            for (int i = 0; i < segments.Count; i++)
            {
                tile.Remove(segments[i]);
            }
        }
    }

    public void RemoveModelSegmentsChecked(IEnumerable<E> modelEntitiesToRemove)
    {
        if (!modelEntitiesToRemove.Any())
        {
            return;
        }

        // Perform an exhaustive check against every other model in the level to find shared textures.

        Dictionary<E, Dictionary<TRTextile, List<TRTextileRegion>>> candidateSegments = new();

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

            Dictionary<TRTextile, List<TRTextileRegion>> modelSegments = GetModelSegments(otherEntity);
            foreach (E entityToRemove in modelEntitiesToRemove)
            {
                Dictionary<TRTextile, List<TRTextileRegion>> entityMap = candidateSegments[entityToRemove];
                foreach (TRTextile tile in entityMap.Keys)
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
        foreach (Dictionary<TRTextile, List<TRTextileRegion>> segmentMap in candidateSegments.Values)
        {
            foreach (TRTextile tile in segmentMap.Keys)
            {
                foreach (TRTextileRegion segment in segmentMap[tile])
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
        Dictionary<E, Dictionary<TRTextile, List<TRTextileRegion>>> candidateSegments = new();

        // First cache each segment for the models we wish to remove
        foreach (E entity in entitiesToRemove)
        {
            candidateSegments[entity] = GetSpriteSegments(entity);
        }

        // Tell each tile to remove the candidate segments
        foreach (Dictionary<TRTextile, List<TRTextileRegion>> segmentMap in candidateSegments.Values)
        {
            foreach (TRTextile tile in segmentMap.Keys)
            {
                foreach (TRTextileRegion segment in segmentMap[tile])
                {
                    tile.Remove(segment);
                }
            }
        }
    }

    public abstract TRImage GetTile(int tileIndex);

    protected abstract void CreateImageSpace(int count);

    public abstract void SetTile(int tileIndex, TRImage image);

    private void Commit()
    {
        if (_tiles.Count > NumLevelImages)
        {
            CreateImageSpace(_tiles.Count - NumLevelImages);
        }

        for (int i = 0; i < _tiles.Count; i++)
        {
            TRTextile tile = _tiles[i];
            if (!tile.ImageChanged)
            {
                continue;
            }

            tile.Commit();
            SetTile(i, tile.Image);
        }
    }

    protected virtual void PostCommit() { }

    protected override TRTextile CreateTile()
    {
        return new TRTextile();
    }
}
