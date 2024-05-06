using RectanglePacker;
using RectanglePacker.Events;
using RectanglePacker.Organisation;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl.Packing;

public abstract class TRTexturePacker : AbstractPacker<TRTextile, TRTextileRegion>
{
    private readonly List<TRTextileSegment> _allSegments;
    public abstract int NumLevelImages { get; }

    public TRTexturePacker(int maximumTiles)
    {
        TileWidth = TRConsts.TPageWidth;
        TileHeight = TRConsts.TPageHeight;
        MaximumTiles = maximumTiles;
        AllowEmptyPacking = true;

        Options = new()
        {
            FillMode = PackingFillMode.Vertical,
            OrderMode = PackingOrderMode.Height,
            Order = PackingOrder.Descending,
            GroupMode = PackingGroupMode.None,
            StartMethod = PackingStartMethod.EndTile
        };

        _allSegments = new();
    }

    protected void LoadLevel()
    {
        _allSegments.AddRange(LoadObjectSegments());
        _allSegments.AddRange(LoadSpriteSegments());
        _allSegments.Sort((s1, s2) => s2.Area.CompareTo(s1.Area));

        for (int i = 0; i < NumLevelImages; i++)
        {
            TRTextile tile = AddTile();
            tile.Image = GetImage(i);
            tile.AllowOverlapping = true; // Allow initially for the likes of Opera House - see tile 3 [128, 128]
        }

        foreach (TRTextileSegment texture in _allSegments)
        {
            _tiles[texture.Atlas].AddSegment(texture);
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

    public Dictionary<TRTextile, List<TRTextileRegion>> GetMeshRegions(IEnumerable<TRMesh> meshes, TRMesh dummyMesh = null)
    {
        IEnumerable<int> textures = meshes
            .Where(m => m != dummyMesh)
            .SelectMany(m => m.TexturedFaces.Select(t => (int)t.Texture))
            .Distinct();

        return GetObjectRegions(textures);
    }

    public Dictionary<TRTextile, List<TRTextileRegion>> GetObjectRegions(int index)
        => GetObjectRegions(new List<int> { index });

    public Dictionary<TRTextile, List<TRTextileRegion>> GetObjectRegions(IEnumerable<int> textureIndices)
    {
        Dictionary<TRTextile, List<TRTextileRegion>> regionMap = new();
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> regions = tile.GetObjectRegions(textureIndices);
            if (regions.Count > 0)
            {
                regionMap[tile] = regions;
            }
        }

        return regionMap;
    }

    public Dictionary<TRTextile, List<TRTextileRegion>> GetSpriteRegions(TRSpriteSequence sequence)
        => GetSpriteRegions(new List<TRSpriteSequence> { sequence });

    public Dictionary<TRTextile, List<TRTextileRegion>> GetSpriteRegions(IEnumerable<TRSpriteSequence> sequences)
    {
        Dictionary<TRTextile, List<TRTextileRegion>> regionMap = new();
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> regions = tile.Rectangles
                .Where(r => r.Segments.Any(s => s.Texture is TRSpriteTexture spr && sequences.Any(q => q.Textures.Contains(spr))))
                .ToList();
            if (regions.Count > 0)
            {
                regionMap[tile] = regions;
            }
        }

        return regionMap;
    }

    public void RemoveObjectRegions(IEnumerable<int> indices)
    {
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> regions = tile.GetObjectRegions(indices);
            for (int i = 0; i < regions.Count; i++)
            {
                tile.Remove(regions[i]);
            }
        }
    }

    public void RemoveSpriteRegions(IEnumerable<TRSpriteSequence> sequences)
    {
        foreach (TRTextile tile in _tiles)
        {
            List<TRTextileRegion> regions = tile.Rectangles
                .Where(r => r.Segments.Any(s => s.Texture is TRSpriteTexture spr && sequences.Any(q => q.Textures.Contains(spr))))
                .ToList();
            for (int i = 0; i < regions.Count; i++)
            {
                tile.Remove(regions[i]);
            }
        }
    }

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
            SetImage(i, tile.Image);
        }
    }

    protected abstract void CreateImageSpace(int count);
    protected virtual void PostCommit() { }

    public abstract TRImage GetImage(int tileIndex);
    public abstract void SetImage(int tileIndex, TRImage image);
    protected abstract List<TRTextileSegment> LoadObjectSegments();
    protected abstract List<TRTextileSegment> LoadSpriteSegments();
}
