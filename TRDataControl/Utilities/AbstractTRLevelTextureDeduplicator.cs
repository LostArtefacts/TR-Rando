using TRImageControl.Packing;

namespace TRModelTransporter.Utilities;

public abstract class AbstractTRLevelTextureDeduplicator<E, L>
    where E : Enum
    where L : class
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
        TRTexturePacker<E, L> levelPacker = CreatePacker(Level);
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

    protected abstract TRTexturePacker<E, L> CreatePacker(L level);
    protected abstract AbstractTextureRemapGroup<E, L> GetRemapGroup(string path);
    protected abstract void ReindexTextures(Dictionary<int, int> indexMap);

    private static void TidySegment(TRTextileRegion segment, Dictionary<int, int> reindexMap)
    {
        for (int i = segment.Textures.Count - 1; i > 0; i--) //ignore the first = the largest
        {
            TRTextileSegment texture = segment.Textures[i];
            TRTextileSegment candidateTexture = null;
            for (int j = 0; j < segment.Textures.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                TRTextileSegment texture2 = segment.Textures[j];
                if (texture.Equals(texture2))
                {
                    candidateTexture = texture2;
                    break;
                }
            }

            if (candidateTexture != null)
            {
                reindexMap[texture.Index] = candidateTexture.Index;
                texture.Invalidate();
                segment.Textures.RemoveAt(i);
            }
        }
    }
}
