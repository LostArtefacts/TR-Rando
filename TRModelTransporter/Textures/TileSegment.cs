using RectanglePacker.Defaults;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace TRModelTransporter.Textures
{
    public class TileSegment : DefaultRectangle, IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public List<AbstractIndexedTRTexture> Textures { get; private set; }

        public TileSegment(AbstractIndexedTRTexture initialTexture, Bitmap bitmap)
            : base(initialTexture.Bounds)
        {
            Bitmap = bitmap;
            Textures = new List<AbstractIndexedTRTexture>();
            AddTexture(initialTexture);
        }

        public void AddTexture(AbstractIndexedTRTexture texture)
        {
            Textures.Add(texture);
        }

        public bool IsFor(int textureIndex)
        {
            return Textures.FindIndex(t => t.Index == textureIndex) != -1;
        }

        // Triggered when successfully mapped onto a tile, so inform
        // the indexed textures to update their vertices.
        public void Bind()
        {
            foreach (AbstractIndexedTRTexture texture in Textures)
            {
                texture.SetContainingBounds(MappedBounds);
            }
        }

        // Triggered when changes are to be saved back to the level. Index
        // is that of containing tile.
        public void Commit(int tileIndex)
        {
            foreach (AbstractIndexedTRTexture texture in Textures)
            {
                texture.Commit(tileIndex);
            }
        }

        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
            }
        }
    }
}