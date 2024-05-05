using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRModelTransporter.Utilities;

public abstract class AbstractTRLevelTextureDeduplicator<E, L>
    where E : Enum
    where L : TRLevelBase
{
    public L Level { get; set; }

    private readonly TRTextureDeduplicator<E> _deduplicator;

    public AbstractTRLevelTextureDeduplicator()
    {
        _deduplicator = new TRTextureDeduplicator<E>
        {
            UpdateGraphics = true
        };
    }

    public void Deduplicate(string remappingPath)
    {
        TRTexturePacker levelPacker = CreatePacker(Level);
        Dictionary<TRTextile, List<TRTextileRegion>> allTextures = new();
        foreach (TRTextile tile in levelPacker.Tiles)
        {
            allTextures[tile] = new List<TRTextileRegion>(tile.Rectangles);
        }

        AbstractTextureRemapGroup<E, L> remapGroup = GetRemapGroup(remappingPath);

        _deduplicator.SegmentMap = allTextures;
        _deduplicator.PrecompiledRemapping = remapGroup.Remapping;
        _deduplicator.Deduplicate();

        levelPacker.AllowEmptyPacking = true;
        levelPacker.Pack(true);

        // Now we want to go through every IndexedTexture and see if it's
        // pointing to the same thing - so tile, position, and point direction
        // have to be equal. See IndexedTRObjectTexture
        Dictionary<int, int> indexMap = new();
        foreach (TRTextile tile in allTextures.Keys)
        {
            foreach (TRTextileRegion segment in allTextures[tile])
            {
                TidySegment(segment, indexMap);
            }
        }

        ReindexTextures(indexMap);
    }

    protected abstract TRTexturePacker CreatePacker(L level);
    protected abstract AbstractTextureRemapGroup<E, L> GetRemapGroup(string path);
    protected abstract void ReindexTextures(Dictionary<int, int> indexMap);

    private static void TidySegment(TRTextileRegion region, Dictionary<int, int> reindexMap)
    {
        for (int i = region.Segments.Count - 1; i > 0; i--) //ignore the first = the largest
        {
            TRTextileSegment segment = region.Segments[i];
            TRTextileSegment candidateTexture = null;
            for (int j = 0; j < region.Segments.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                TRTextileSegment texture2 = region.Segments[j];
                if (segment.Equals(texture2))
                {
                    candidateTexture = texture2;
                    break;
                }
            }

            if (candidateTexture != null)
            {
                reindexMap[segment.Index] = candidateTexture.Index;
                segment.Invalidate();
                region.Segments.RemoveAt(i);
            }
        }
    }
}
