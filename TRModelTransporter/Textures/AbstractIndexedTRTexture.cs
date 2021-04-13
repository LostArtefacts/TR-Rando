using Newtonsoft.Json;
using System;
using System.Drawing;

namespace TRModelTransporter.Textures
{
    public abstract class AbstractIndexedTRTexture
    {
        protected Rectangle _bounds;
        private int _boundsXDiff, _boundsYDiff;

        public int Index { get; set; }
        [JsonIgnore]
        public abstract int Atlas { get; set; }
        [JsonIgnore]
        public int Area => _bounds.Width * _bounds.Height;
        [JsonIgnore]
        public Rectangle Bounds => _bounds;

        public AbstractIndexedTRTexture()
        {
            _boundsXDiff = 0;
            _boundsYDiff = 0;
        }

        public void SetContainingBounds(Rectangle rectangle)
        {
            if (rectangle == _bounds)
            {
                return;
            }

            int x1 = rectangle.X;
            int y1 = rectangle.Y;
            int x2 = _bounds.X;
            int y2 = _bounds.Y;

            int xDiff = Math.Abs(x2 - x1);
            int yDiff = Math.Abs(y2 - y1);

            if (x1 < x2)
            {
                xDiff *= -1;
            }
            if (y1 < y2)
            {
                yDiff *= -1;
            }

            _bounds.X += xDiff;
            _bounds.Y += yDiff;

            _boundsXDiff = xDiff;
            _boundsYDiff = yDiff;
        }

        public void Commit(int tileIndex)
        {
            Atlas = tileIndex;

            if (_boundsXDiff != 0 || _boundsYDiff != 0)
            {
                ApplyBoundDiffToTexture(_boundsXDiff, _boundsYDiff);
            }
        }

        protected abstract void GetBoundsFromTexture();
        protected abstract void ApplyBoundDiffToTexture(int xDiff, int yDiff);
    }
}