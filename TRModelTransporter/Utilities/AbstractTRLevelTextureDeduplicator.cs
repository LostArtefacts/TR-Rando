using System;
using System.Collections.Generic;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

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
        using (AbstractTexturePacker<E, L> levelPacker = CreatePacker(Level))
        {
            Dictionary<TexturedTile, List<TexturedTileSegment>> allTextures = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
            foreach (TexturedTile tile in levelPacker.Tiles)
            {
                allTextures[tile] = new List<TexturedTileSegment>(tile.Rectangles);
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
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            foreach (TexturedTile tile in allTextures.Keys)
            {
                foreach (TexturedTileSegment segment in allTextures[tile])
                {
                    TidySegment(segment, indexMap);
                }
            }

            ReindexTextures(indexMap);
        }
    }

    protected abstract AbstractTexturePacker<E, L> CreatePacker(L level);
    protected abstract AbstractTextureRemapGroup<E, L> GetRemapGroup(string path);
    protected abstract void ReindexTextures(Dictionary<int, int> indexMap);

    private void TidySegment(TexturedTileSegment segment, Dictionary<int, int> reindexMap)
    {
        for (int i = segment.Textures.Count - 1; i > 0; i--) //ignore the first = the largest
        {
            AbstractIndexedTRTexture texture = segment.Textures[i];
            AbstractIndexedTRTexture candidateTexture = null;
            for (int j = 0; j < segment.Textures.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                AbstractIndexedTRTexture texture2 = segment.Textures[j];
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
