using System.Drawing;
using TRImageControl;
using TRImageControl.Packing;

namespace TRModelTransporter.Utilities;

public class TRTextureDeduplicator<E> where E : Enum
{
    public Dictionary<TexturedTile, List<TexturedTileSegment>> SegmentMap { get; set; }
    public List<TextureRemap> PrecompiledRemapping { get; set; }
    public bool UpdateGraphics { get; set; }

    private Dictionary<TexturedTile, List<int>> _segmentRemovalPositions;
    private List<MappedSegment> _segments;

    public EventHandler<TRTextureRemapEventArgs> SegmentRemapped;

    public TRTextureDeduplicator()
    {
        UpdateGraphics = false;
    }

    // This allows us to "fix" some of Core Design's approach in duplicating graphics. For example, In DragonFront_H, there
    // is a lot of clips taken from the largest piece of the dragon's spikes, and these are stored as separate textures. This 
    // attempts to merge these into one segment by comparing each bitmap with every other larger or equal in size to itself.
    public void Deduplicate()
    {
        InitialiseSegments();
        DeduplicateSegments();
        RemoveStaleSegments();
    }

    private void InitialiseSegments()
    {
        _segmentRemovalPositions = new Dictionary<TexturedTile, List<int>>();
        _segments = new List<MappedSegment>();

        foreach (TexturedTile tile in SegmentMap.Keys)
        {
            for (int i = 0; i < SegmentMap[tile].Count; i++)
            {
                _segments.Add(new MappedSegment
                {
                    Tile = tile,
                    Segment = SegmentMap[tile][i],
                    SegmentPosition = i
                });
            }
        }

        _segments.Sort(delegate (MappedSegment ms1, MappedSegment ms2)
        {
            return ms1.Segment.Area.CompareTo(ms2.Segment.Area);
        });
    }

    private MappedSegment FindSegmentFromTilePosition(int tileIndex, Rectangle bounds, bool exactMatch = true)
    {
        MappedSegment segment;
        for (int i = 0; i < _segments.Count; i++)
        {
            segment = _segments[i];
            if (segment.Tile.Index == tileIndex && ((exactMatch && segment.Segment.Bounds == bounds) || segment.Segment.Bounds.Contains(bounds)))
            {
                return segment;
            }
        }

        return null;
    }

    private void DeduplicateSegments()
    {
        if (PrecompiledRemapping == null)
        {
            // Exhaustively check each segment against every other. If it
            // is successfully moved, remove it from the list.
            for (int i = _segments.Count - 1; i >= 0; i--)
            {
                MappedSegment mappedSegment = _segments[i];
                if (MoveSegment(mappedSegment, i))
                {
                    _segments.RemoveAt(i);
                }
            }
        }
        else
        {
            // We can skip the exhaustive bitmap checking and jump straight
            // to moving the segments if we have a precompiled list ready.
            foreach (TextureRemap remap in PrecompiledRemapping)
            {
                ProcessRemap(remap);
            }
        }
    }

    private void StoreSegmentRemoval(MappedSegment mappedSegment)
    {
        if (!_segmentRemovalPositions.ContainsKey(mappedSegment.Tile))
        {
            _segmentRemovalPositions[mappedSegment.Tile] = new List<int>();
        }
        _segmentRemovalPositions[mappedSegment.Tile].Add(mappedSegment.SegmentPosition);
    }

    private bool MoveSegment(MappedSegment mappedSegment, int index)
    {
        for (int i = 0; i < _segments.Count; i++)
        {
            if (i == index)
            {
                continue;
            }

            MappedSegment candidate = _segments[i];

            // Check to see if the segment is contained within the other.
            // The returned point will be relative to the containing segment.
            Point? p = LocateSubSegment(mappedSegment, candidate);
            if (!p.HasValue)
            {
                continue;
            }

            // Make the point relative to the tile
            Point adjustmentPoint = p.Value;
            adjustmentPoint.X += candidate.Segment.MappedX;
            adjustmentPoint.Y += candidate.Segment.MappedY;

            ProcessRemap(mappedSegment, candidate, adjustmentPoint);

            return true;
        }

        return false;
    }

    private void ProcessRemap(TextureRemap remap)
    {
        // Find the original MappedSegment
        MappedSegment originalSegment = FindSegmentFromTilePosition(remap.OriginalTile, remap.OriginalBounds);
        // Find the candiate MappedSegment
        MappedSegment candidateSegment = FindSegmentFromTilePosition(remap.NewTile, remap.NewBounds/*, false*/);
        // Move it!
        if (originalSegment != null && candidateSegment != null)
        {
            // Move it!
            ProcessRemap(originalSegment, candidateSegment, remap.AdjustmentPoint);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Could not remap " + remap.OriginalTile + " : " + remap.OriginalIndex);
        }
    }

    private void ProcessRemap(MappedSegment originalSegment, MappedSegment candidateSegment, Point adjustmentPoint)
    {
        Rectangle oldBounds = originalSegment.Segment.Bounds;
        // We pull all of the sub textures from the original segment into the candidate
        int oldFirstTextureIndex = originalSegment.Segment.FirstTextureIndex;
        candidateSegment.Segment.InheritTextures(originalSegment.Segment, adjustmentPoint, candidateSegment.Tile.Index);
        // Store the removal for later processing in RemoveStaleSegments
        StoreSegmentRemoval(originalSegment);

        SegmentRemapped?.Invoke(this, new TRTextureRemapEventArgs
        {
            OldFirstTextureIndex = oldFirstTextureIndex,
            OldArea = originalSegment.Segment.Area,
            NewSegment = candidateSegment.Segment,
            OldTile = originalSegment.Tile,
            NewTile = candidateSegment.Tile,
            OldBounds = oldBounds,
            NewBounds = candidateSegment.Segment.Bounds,
            AdjustmentPoint = adjustmentPoint
        });
    }

    private static Point? LocateSubSegment(MappedSegment segmentToLocate, MappedSegment containerSegment)
    {
        int xEnd = containerSegment.Segment.Bounds.Width - segmentToLocate.Segment.Bounds.Width;
        int yEnd = containerSegment.Segment.Bounds.Height - segmentToLocate.Segment.Bounds.Height;
        Rectangle rect = new(0, 0, segmentToLocate.Segment.Bounds.Width, segmentToLocate.Segment.Bounds.Height);

        for (int x = 0; x <= xEnd; x++)
        {
            rect.X = x;
            for (int y = 0; y <= yEnd; y++)
            {
                rect.Y = y;
                TRImage bmp = containerSegment.Segment.Image.Export(rect);
                if (segmentToLocate.Segment.Image.Equals(bmp))
                {
                    return new Point(x, y);
                }
            }
        }
        return null;
    }

    private void RemoveStaleSegments()
    {
        foreach (TexturedTile tile in _segmentRemovalPositions.Keys)
        {
            List<int> removals = _segmentRemovalPositions[tile];
            removals.Sort();
            for (int i = removals.Count - 1; i >= 0; i--)
            {
                if (UpdateGraphics)
                {
                    tile.Remove(SegmentMap[tile][removals[i]]);
                }
                SegmentMap[tile].RemoveAt(removals[i]);
            }

            if (SegmentMap[tile].Count == 0)
            {
                SegmentMap.Remove(tile);
            }
        }
    }

    public bool ShouldIgnoreSegment(IEnumerable<int> ignoredIndices, TexturedTileSegment segment)
    {
        if (ignoredIndices != null)
        {
            if (!ignoredIndices.Any())
            {
                return true;
            }

            foreach (int textureIndex in ignoredIndices)
            {
                if (segment.IsObjectTextureFor(textureIndex))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private class MappedSegment
    {
        public TexturedTile Tile { get; set; }
        public TexturedTileSegment Segment { get; set; }
        public int SegmentPosition { get; set; }
    }
}
