using RectanglePacker.Defaults;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Textures
{
    public class TexturedTile : DefaultTile<TileSegment>, IDisposable
    {
        public BitmapGraphics BitmapGraphics { get; set; }

        public void AddTexture(AbstractIndexedTRTexture texture)
        {
            // Check if the texture bounds is contained in any of the existing segment bounds
            foreach (TileSegment segment in _rectangles)
            {
                if (segment.Bounds.Contains(texture.Bounds))
                {
                    // If so, just map the texture to the same segment
                    segment.AddTexture(texture);
                    return;
                }
            }

            // Otherwise, make a new segment
            TileSegment newSegment = new TileSegment(texture, BitmapGraphics.Extract(texture.Bounds));
            base.Add(newSegment, texture.Bounds.X, texture.Bounds.Y);
        }

        public List<TileSegment> GetTextureIndexSegments(IEnumerable<int> indices)
        {
            List<TileSegment> segments = new List<TileSegment>();
            foreach (int index in indices)
            {
                foreach (TileSegment segment in Rectangles)
                {
                    if (segment.IsFor(index) && !segments.Contains(segment))
                    {
                        segments.Add(segment);
                    }
                }
            }
            return segments;
        }

        public void Commit()
        {
            foreach (TileSegment segment in Rectangles)
            {
                segment.Commit(Index);
            }
        }

        protected override bool Add(TileSegment segment, int x, int y)
        {
            bool added = base.Add(segment, x, y);
            if (added)
            {
                CheckBitmapStatus();
                BitmapGraphics.Import(segment.Bitmap, segment.MappedBounds);

                segment.Bind();
            }
            return added;
        }

        public override bool Remove(TileSegment segment)
        {
            bool removed = base.Remove(segment);
            if (removed)
            {
                CheckBitmapStatus();
                BitmapGraphics.Delete(segment.Bounds);
            }
            return removed;
        }

        private void CheckBitmapStatus()
        {
            if (BitmapGraphics == null)
            {
                BitmapGraphics = new BitmapGraphics(new Bitmap(Width, Height, PixelFormat.Format32bppArgb));
            }
        }

        public void Dispose()
        {
            if (BitmapGraphics != null)
            {
                BitmapGraphics.Dispose();
            }

            foreach (TileSegment segment in _rectangles)
            {
                segment.Dispose();
            }
        }
    }
}