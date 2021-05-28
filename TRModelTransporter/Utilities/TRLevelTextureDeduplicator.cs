using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TRLevelReader.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Utilities
{
    public class TRLevelTextureDeduplicator
    {
        public TR2Level Level { get; set; }

        private readonly TRTextureDeduplicator _deduplicator;

        public TRLevelTextureDeduplicator()
        {
            _deduplicator = new TRTextureDeduplicator
            {
                UpdateGraphics = true
            };
        }

        public void Deduplicate(string remappingPath)
        {
            using (TexturePacker levelPacker = new TexturePacker(Level))
            {
                Dictionary<TexturedTile, List<TexturedTileSegment>> allTextures = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
                foreach (TexturedTile tile in levelPacker.Tiles)
                {
                    allTextures[tile] = new List<TexturedTileSegment>(tile.Rectangles);
                }

                TextureRemapGroup remapGroup = JsonConvert.DeserializeObject<TextureRemapGroup>(File.ReadAllText(remappingPath));

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

                Level.ReindexTextures(indexMap);
                Level.ResetUnusedTextures();
            }
        }

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
}