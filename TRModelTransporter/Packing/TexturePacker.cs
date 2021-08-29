using RectanglePacker;
using RectanglePacker.Events;
using RectanglePacker.Organisation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRTexture16Importer;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Packing
{
    public class TexturePacker : AbstractPacker<TexturedTile, TexturedTileSegment>, IDisposable
    {
        public TR2Level Level { get; private set; }
        private readonly string _levelClassifier;

        public IReadOnlyList<AbstractIndexedTRTexture> AllTextures => _allTextures;

        private readonly List<AbstractIndexedTRTexture> _allTextures;

        public TexturePacker()
            : this(null) { }

        public TexturePacker(TR2Level level, ITextureClassifier classifier = null)
        {
            TileWidth = 256;
            TileHeight = 256;
            MaximumTiles = 16;

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

            _allTextures = new List<AbstractIndexedTRTexture>();

            if (Level != null)
            {
                _allTextures.AddRange(LoadObjectTextures());
                _allTextures.AddRange(LoadSpriteTextures());
                _allTextures.Sort(new TRTextureReverseAreaComparer());

                for (int i = 0; i < Level.NumImages; i++)
                {
                    TexturedTile tile = AddTile();
                    tile.BitmapGraphics = GetTileBitmap(i);
                    tile.AllowOverlapping = true; // Allow initially for the likes of Opera House - see tile 3 [128, 128]
                }

                foreach (AbstractIndexedTRTexture texture in _allTextures)
                {
                    _tiles[texture.Atlas].AddTexture(texture);
                }
            }
        }

        public PackingResult<TexturedTile, TexturedTileSegment> Pack(bool commitToLevel)
        {
            try
            {
                PackingResult<TexturedTile, TexturedTileSegment> result = Pack();

                if (result.OrphanCount == 0 && commitToLevel)
                {
                    Commit();
                }

                return result;
            }
            catch (InvalidOperationException e)
            {
                throw new PackingException(e.Message, e);
            }
        }

        public Dictionary<TexturedTile, List<TexturedTileSegment>> GetModelSegments(TR2Entities modelEntity)
        {
            Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
            TRMesh[] meshes = TR2LevelUtilities.GetModelMeshes(Level, modelEntity);
            if (meshes != null)
            {
                IEnumerable<int> indices = GetMeshTextureIndices(meshes);
                foreach (TexturedTile tile in _tiles)
                {
                    List<TexturedTileSegment> segments = tile.GetObjectTextureIndexSegments(indices);
                    if (segments.Count > 0)
                    {
                        segmentMap[tile] = segments;
                    }
                }
            }

            return segmentMap;
        }

        public Dictionary<TexturedTile, List<TexturedTileSegment>> GetSpriteSegments(TR2Entities entity)
        {
            Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
            int i = Level.SpriteSequences.ToList().FindIndex(s => s.SpriteID == (int)entity);
            if (i != -1)
            {
                TRSpriteSequence sequence = Level.SpriteSequences[i];
                List<int> indices = new List<int>();
                for (int j = 0; j < sequence.NegativeLength * -1; j++)
                {
                    indices.Add(sequence.Offset + j);
                }

                foreach (TexturedTile tile in _tiles)
                {
                    List<TexturedTileSegment> segments = tile.GetSpriteTextureIndexSegments(indices);
                    if (segments.Count > 0)
                    {
                        segmentMap[tile] = segments;
                    }
                }
            }

            return segmentMap;
        }

        private IEnumerable<int> GetMeshTextureIndices(TRMesh[] meshes)
        {
            ISet<int> textureIndices = new SortedSet<int>();
            foreach (TRMesh mesh in meshes)
            {
                foreach (TRFace4 rect in mesh.TexturedRectangles)
                {
                    textureIndices.Add(rect.Texture);
                }
                foreach (TRFace3 tri in mesh.TexturedTriangles)
                {
                    textureIndices.Add(tri.Texture);
                }
            }
            return textureIndices;
        }


        public void RemoveModelSegments(IEnumerable<TR2Entities> modelEntitiesToRemove, TextureRemapGroup remapGroup)
        {
            if (remapGroup == null)
            {
                RemoveModelSegmentsChecked(modelEntitiesToRemove);
                return;
            }

            // TextureRemapGroup will have been precompiled to determine shared textures between entities, so we know
            // in advance which we cannot remove.
            foreach (TR2Entities modelEntity in modelEntitiesToRemove)
            {
                Dictionary<TexturedTile, List<TexturedTileSegment>> modelSegments = GetModelSegments(modelEntity);
                foreach (TexturedTile tile in modelSegments.Keys)
                {
                    List<TexturedTileSegment> segments = modelSegments[tile];
                    for (int i = 0; i < segments.Count; i++)
                    {
                        TexturedTileSegment segment = segments[i];
                        if (remapGroup.CanRemoveRectangle(tile.Index, segment.Bounds, modelEntitiesToRemove))
                        {
                            tile.Remove(segment);
                        }
                    }
                }
            }
        }

        public void RemoveModelSegmentsChecked(IEnumerable<TR2Entities> modelEntitiesToRemove)
        {
            // Perform an exhaustive check against every other model in the level to find shared textures.

            Dictionary<TR2Entities, Dictionary<TexturedTile, List<TexturedTileSegment>>> candidateSegments = new Dictionary<TR2Entities, Dictionary<TexturedTile, List<TexturedTileSegment>>>();

            // First cache each segment for the models we wish to remove
            foreach (TR2Entities modelEntity in modelEntitiesToRemove)
            {
                candidateSegments[modelEntity] = GetModelSegments(modelEntity);
            }

            // Load the segments for every other model in the level. For each, scan the segments of
            // the removal candidates. If any is used by any other model, the segment is no longer
            // a candidate. If the other model that shares it is in the list to remove, it will
            // remain a candidate.
            foreach (TRModel model in Level.Models)
            {
                TR2Entities otherEntity = (TR2Entities)model.ID;
                if (modelEntitiesToRemove.Contains(otherEntity))
                {
                    continue;
                }

                Dictionary<TexturedTile, List<TexturedTileSegment>> modelSegments = GetModelSegments(otherEntity);
                foreach (TR2Entities entityToRemove in modelEntitiesToRemove)
                {
                    Dictionary<TexturedTile, List<TexturedTileSegment>> entityMap = candidateSegments[entityToRemove];
                    foreach (TexturedTile tile in entityMap.Keys)
                    {
                        if (modelSegments.ContainsKey(tile))
                        {
                            int match = entityMap[tile].FindIndex(s1 => modelSegments[tile].Any(s2 => s1 == s2));
                            if (match != -1)
                            {
                                //System.Diagnostics.Debug.WriteLine("\t\t" + otherEntity + ", " + tile.Index + ": " + entityMap[tile][match].Bounds);
                                entityMap[tile].RemoveAt(match);
                            }
                        }
                    }
                }
            }

            // Tell each tile to remove the candidate segments
            foreach (Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap in candidateSegments.Values)
            {
                foreach (TexturedTile tile in segmentMap.Keys)
                {
                    foreach (TexturedTileSegment segment in segmentMap[tile])
                    {
                        tile.Remove(segment);
                    }
                }
            }
        }

        public void RemoveSpriteSegments(TR2Entities entity)
        {
            RemoveSpriteSegments(new TR2Entities[] { entity });
        }

        public void RemoveSpriteSegments(IEnumerable<TR2Entities> entitiesToRemove)
        {
            Dictionary<TR2Entities, Dictionary<TexturedTile, List<TexturedTileSegment>>> candidateSegments = new Dictionary<TR2Entities, Dictionary<TexturedTile, List<TexturedTileSegment>>>();

            // First cache each segment for the models we wish to remove
            foreach (TR2Entities entity in entitiesToRemove)
            {
                candidateSegments[entity] = GetSpriteSegments(entity);
            }

            // Tell each tile to remove the candidate segments
            foreach (Dictionary<TexturedTile, List<TexturedTileSegment>> segmentMap in candidateSegments.Values)
            {
                foreach (TexturedTile tile in segmentMap.Keys)
                {
                    foreach (TexturedTileSegment segment in segmentMap[tile])
                    {
                        tile.Remove(segment);
                    }
                }
            }
        }

        private List<IndexedTRObjectTexture> LoadObjectTextures()
        {
            List<IndexedTRObjectTexture> textures = new List<IndexedTRObjectTexture>((int)Level.NumObjectTextures);
            for (int i = 0; i < Level.NumObjectTextures; i++)
            {
                TRObjectTexture texture = Level.ObjectTextures[i];
                if (texture.IsValid())
                {
                    textures.Add(new IndexedTRObjectTexture
                    {
                        Index = i,
                        Classification = _levelClassifier,
                        Texture = texture
                    });
                }
            }
            return textures;
        }

        private List<IndexedTRSpriteTexture> LoadSpriteTextures()
        {
            List<IndexedTRSpriteTexture> textures = new List<IndexedTRSpriteTexture>((int)Level.NumSpriteTextures);
            for (int i = 0; i < Level.NumSpriteTextures; i++)
            {
                TRSpriteTexture texture = Level.SpriteTextures[i];
                if (texture.IsValid())
                {
                    textures.Add(new IndexedTRSpriteTexture
                    {
                        Index = i,
                        Classification = _levelClassifier,
                        Texture = texture
                    });
                }
            }
            return textures;
        }

        private void Commit()
        {
            if (_tiles.Count > Level.NumImages)
            {
                List<TRTexImage16> imgs16 = Level.Images16.ToList();
                List<TRTexImage8> imgs8 = Level.Images8.ToList();

                uint diff = (uint)_tiles.Count - Level.NumImages;
                for (int i = 0; i < diff; i++)
                {
                    imgs16.Add(new TRTexImage16());
                    imgs8.Add(new TRTexImage8 { Pixels = new byte[256 * 256] });
                }

                Level.Images16 = imgs16.ToArray();
                Level.Images8 = imgs8.ToArray();
                Level.NumImages += diff;
            }

            for (int i = 0; i < _tiles.Count; i++)
            {
                TexturedTile tile = _tiles[i];
                if (!tile.BitmapChanged)
                {
                    continue;
                }

                tile.Commit();
                Level.Images16[i].Pixels = T16Importer.ImportFromBitmap(tile.BitmapGraphics.Bitmap);
            }
        }

        private BitmapGraphics GetTileBitmap(int tileIndex)
        {
            TRTexImage16 tex = Level.Images16[tileIndex];

            Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            List<byte> pixelCollection = new List<byte>();

            foreach (Textile16Pixel px in tex.To32BPPFormat())
            {
                pixelCollection.AddRange(px.RGB32);
            }

            Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
            bmp.UnlockBits(bitmapData);

            return new BitmapGraphics(bmp);
        }

        public void Dispose()
        {
            foreach (TexturedTile tile in _tiles)
            {
                tile.Dispose();
            }
        }

        protected override TexturedTile CreateTile()
        {
            return new TexturedTile();
        }
    }
}