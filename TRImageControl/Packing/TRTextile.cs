using RectanglePacker.Defaults;

namespace TRImageControl.Packing;

public class TRTextile : DefaultTile<TRTextileRegion>
{
    private TRImage _image;
    public TRImage Image
    {
        get => _image;
        set
        {
            _image = value;
            _image.DataChanged += (s, e) =>
            {
                _graphicChanged = true;
            };
        }
    }

    // Some room textures in Opera House (at least) overlap, so we need to 
    // allow this when initialising the tiles.
    public bool AllowOverlapping
    {
        get => _allowOverlapping;
        set => _allowOverlapping = value;
    }

    public bool ImageChanged => _segmentAdded || _segmentRemoved || _graphicChanged;
    private bool _segmentAdded, _segmentRemoved, _graphicChanged;

    public TRTextile()
    {
        _segmentAdded = _segmentRemoved = _graphicChanged = false;
    }

    public void AddSegment(TRTextileSegment segment)
    {
        foreach (TRTextileRegion region in Rectangles)
        {
            if (region.Bounds.Contains(segment.Bounds))
            {
                // If so, just map the texture to the same segment
                region.AddTexture(segment);
                return;
            }
        }

        // Otherwise, make a new segment
        TRTextileRegion newRegion = new(segment, Image.Export(segment.Bounds));
        Add(newRegion, segment.Position);
    }

    public List<TRTextileRegion> GetObjectRegions(IEnumerable<int> indices)
    {
        List<TRTextileRegion> regions = new();
        foreach (int index in indices)
        {
            foreach (TRTextileRegion region in Rectangles)
            {
                if (region.IsObjectTextureFor(index) && !regions.Contains(region))
                {
                    regions.Add(region);
                }
            }
        }
        return regions;
    }

    public void Commit()
    {
        foreach (TRTextileRegion region in Rectangles)
        {
            region.Commit(Index);
        }
    }

    protected override bool Add(TRTextileRegion region, int x, int y)
    {
        bool added = base.Add(region, x, y);
        if (added)
        {
            Image ??= new(Width, Height);
            Image.Import(region.Image, region.MappedBounds.Location);
            region.Bind();
            _segmentAdded = true;
        }
        return added;
    }

    public override bool Remove(TRTextileRegion region)
    {
        bool removed = base.Remove(region);
        if (removed)
        {
            Image ??= new(Width, Height);
            Image.Delete(region.Bounds);
            region.Unbind();
            _segmentRemoved = true;
        }
        return removed;
    }

    public override void PackingStarted()
    {
        // No overlapping during actual packing
        AllowOverlapping = false;

        // Only if a segment has been removed prior to packing having
        // started do we keep a hold of that fact. This prevents the
        // bitmap being saved back to the level if it's unchanged.
        if (!_segmentRemoved)
        {
            _segmentAdded = _segmentRemoved = false;
        }
    }
}
