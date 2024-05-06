using System.Drawing;

namespace TRImageControl.Packing;

public class TRImageDeduplicator
{
    private Dictionary<TRTextile, List<TRTextileRegion>> _tiles;

    public void Deduplicate(Dictionary<TRTextile, List<TRTextileRegion>> tiles)
    {
        _tiles = tiles;
        foreach (TRTextile tile in _tiles.Keys)
        {
            List<TRTextileRegion> regions = _tiles[tile];
            for (int i = regions.Count - 1; i >= 0; i--)
            {
                if (Move(tile, regions[i], i))
                {
                    regions.RemoveAt(i);
                }
            }
        }
    }

    private bool Move(TRTextile sourceTile, TRTextileRegion region, int index)
    {
        foreach (TRTextile tile in _tiles.Keys)
        {
            List<TRTextileRegion> regions = _tiles[tile];
            for (int i = 0; i < regions.Count; i++)
            {
                if (i == index && tile == sourceTile)
                    continue;

                TRTextileRegion candidate = regions[i];
                Point? p = LocateSubRegion(region, candidate);
                if (p.HasValue)
                {
                    candidate.InheritTextures(region, p.Value, tile.Index);
                    return true;
                }
            }
        }

        return false;
    }

    private static Point? LocateSubRegion(TRTextileRegion region, TRTextileRegion candidate)
    {
        int xEnd = candidate.Bounds.Width - region.Bounds.Width;
        int yEnd = candidate.Bounds.Height - region.Bounds.Height;
        Rectangle rect = new(0, 0, region.Bounds.Width, region.Bounds.Height);

        for (int x = 0; x <= xEnd; x++)
        {
            rect.X = x;
            for (int y = 0; y <= yEnd; y++)
            {
                rect.Y = y;
                TRImage clip = candidate.Image.Export(rect);
                if (clip.Equals(region.Image))
                {
                    return new(candidate.Bounds.X + x, candidate.Bounds.Y + y);
                }
            }
        }
        return null;
    }
}
