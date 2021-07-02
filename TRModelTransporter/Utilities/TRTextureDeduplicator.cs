using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Utilities
{
    public class TRTextureDeduplicator
    {
        // This prevents Lara's hips being duplicated - this seems to be part of some animation model exports, so just ignore them
        // in the bitmap export and they won't be imported into the new level. The only issue with this is when viewing a level
        // in trview in that the textures will be mapped to tile 0 - e.g. the hips that show in place of DragonExplosionEmitter_N.
        // The alternative is trying to remap to Lara's hips in the target level - TODO - but this is minor.
        public static readonly IReadOnlyDictionary<TR2Entities, List<int>> IgnoreEntityTextures = new Dictionary<TR2Entities, List<int>>
        {
            [TR2Entities.LaraMiscAnim_H] = new List<int>(), // empty list indicates to ignore everything
            [TR2Entities.LaraSnowmobAnim_H] = new List<int> { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 20, 21, 23 },
            [TR2Entities.SnowmobileBelt] = new List<int> { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 20, 21, 23 },
            [TR2Entities.DragonExplosionEmitter_N] = new List<int> { 0, 1, 2, 3, 4, 5, 6, 8, 13, 14, 16, 17, 19 }
        };

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

        // This allows us to "fix" some of Core Design's lazy approach in duplicating graphics. For example, In DragonFront_H, there
        // is a lot of clips taken from the largest piece of the dragon's spikes, and these are stored as separate textures. This 
        // attempts to merge these into one segment by comparing each bitmap with every other larger or equal in size to itself.
        public void Deduplicate()
        {
            InitialiseSegments();
            DeduplicateSegments();
            RemoveStaleSegments();
        }

        //private int GetUsedArea()
        //{
        //    int area = 0;
        //    foreach (MappedSegment seg in _segments)
        //    {
        //        area += seg.Segment.Area;
        //    }
        //    return area;
        //}

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

        private Point? LocateSubSegment(MappedSegment segmentToLocate, MappedSegment containerSegment)
        {
            int xEnd = containerSegment.Segment.Bounds.Width - segmentToLocate.Segment.Bounds.Width;
            int yEnd = containerSegment.Segment.Bounds.Height - segmentToLocate.Segment.Bounds.Height;
            Rectangle rect = new Rectangle(0, 0, segmentToLocate.Segment.Bounds.Width, segmentToLocate.Segment.Bounds.Height);

            for (int x = 0; x <= xEnd; x++)
            {
                rect.X = x;
                for (int y = 0; y <= yEnd; y++)
                {
                    rect.Y = y;
                    using (Bitmap bmp = containerSegment.Segment.Bitmap.Clone(rect, PixelFormat.Format32bppArgb))
                    {
                        if (CompareBitmaps(segmentToLocate.Segment.Bitmap, bmp))
                        {
                            return new Point(x, y);
                        }
                    }
                }
            }
            return null;
        }

        private bool CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            for (int x = 0; x < bmp1.Width; x++)
            {
                for (int y = 0; y < bmp1.Height; y++)
                {
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
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

        public bool ShouldIgnoreSegment(TR2Entities entity, TexturedTileSegment segment)
        {
            if (IgnoreEntityTextures.ContainsKey(entity))
            {
                if (IgnoreEntityTextures[entity].Count == 0)
                {
                    return true;
                }

                foreach (int textureIndex in IgnoreEntityTextures[entity])
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
}