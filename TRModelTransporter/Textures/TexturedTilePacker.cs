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
using TRTexture16Importer;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Textures
{
    public class TexturedTilePacker : AbstractPacker<TexturedTile, TileSegment>, IDisposable
    {
        public TR2Level Level { get; private set; }

        public IReadOnlyList<AbstractIndexedTRTexture> AllTextures => _allTextures;

        private readonly List<AbstractIndexedTRTexture> _allTextures;

        public TexturedTilePacker()
            : this(null) { }

        public TexturedTilePacker(TR2Level level)
        {
            TileWidth = 256;
            TileHeight = 256;
            MaximumTiles = 16;

            FillMode = PackingFillMode.Vertical;
            OrderMode = PackingOrderMode.Height;
            Order = PackingOrder.Descending;
            GroupMode = PackingGroupMode.None;

            Level = level;

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
                }

                foreach (AbstractIndexedTRTexture texture in _allTextures)
                {
                    _tiles[texture.Atlas].AddTexture(texture);
                }
            }
        }

        public PackingResult<TexturedTile, TileSegment> Pack(bool commitToLevel)
        {
            PackingResult<TexturedTile, TileSegment> result = Pack();

            if (result.OrphanCount == 0 && commitToLevel)
            {
                Commit();
            }

            return result;
        }

        public Dictionary<TexturedTile, List<TileSegment>> GetModelSegments(TR2Entities modelEntity)
        {
            TRMesh[] meshes = TR2LevelUtilities.GetModelMeshes(Level, modelEntity);
            IEnumerable<int> indices = GetMeshTextureIndices(meshes);
            Dictionary<TexturedTile, List<TileSegment>> segmentMap = new Dictionary<TexturedTile, List<TileSegment>>();
            foreach (TexturedTile tile in _tiles)
            {
                List<TileSegment> segments = tile.GetTextureIndexSegments(indices);
                if (segments.Count > 0)
                {
                    segmentMap[tile] = segments;
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

        public void RemoveModelSegments(TR2Entities modelEntity)
        {
            RemoveModelSegments(new TR2Entities[] { modelEntity });
        }

        public void RemoveModelSegments(IEnumerable<TR2Entities> modelEntities)
        {
            foreach (TR2Entities modelEntity in modelEntities)
            {
                Dictionary<TexturedTile, List<TileSegment>> segmentMap = GetModelSegments(modelEntity);
                foreach (TexturedTile tile in segmentMap.Keys)
                {
                    foreach (TileSegment segment in segmentMap[tile])
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
                textures.Add(new IndexedTRObjectTexture
                {
                    Index = i,
                    Texture = Level.ObjectTextures[i]
                });
            }
            return textures;
        }

        private List<IndexedTRSpriteTexture> LoadSpriteTextures()
        {
            List<IndexedTRSpriteTexture> textures = new List<IndexedTRSpriteTexture>((int)Level.NumSpriteTextures);
            for (int i = 0; i < Level.NumSpriteTextures; i++)
            {
                textures.Add(new IndexedTRSpriteTexture
                {
                    Index = i,
                    Texture = Level.SpriteTextures[i]
                });
            }
            return textures;
        }

        private void Commit()
        {
            for (int i = 0; i < _tiles.Count; i++)
            {
                TexturedTile tile = _tiles[i];
                tile.Commit();

                if (i >= Level.NumImages)
                {
                    List<TRTexImage16> imgs16 = Level.Images16.ToList();
                    List<TRTexImage8> imgs8 = Level.Images8.ToList();
                    imgs16.Add(new TRTexImage16());
                    imgs8.Add(new TRTexImage8
                    {
                        Pixels = new byte[256 * 256]
                    });
                    Level.Images16 = imgs16.ToArray();
                    Level.Images8 = imgs8.ToArray();
                    Level.NumImages++;
                }

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