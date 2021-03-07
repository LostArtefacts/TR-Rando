using System;
using System.Drawing;
using TRTexture16Importer.Textures;

namespace TRTexture16Importer.Helpers
{
    public class BitmapGraphics : IDisposable
    {
        public Bitmap Bitmap { get; set; }
        public Graphics Graphics { get; set; }

        public BitmapGraphics(Bitmap bitmap)
        {
            Bitmap = bitmap;
            Graphics = Graphics.FromImage(Bitmap);
        }

        public void Draw(TextureSource source, TextureTarget target, Rectangle sourceSegment)
        {
            Rectangle sourceRectangle = sourceSegment;
            if (target.ClipRequired)
            {
                sourceRectangle.X += target.Clip.X;
                sourceRectangle.Y += target.Clip.Y;
                sourceRectangle.Width = target.Clip.Width;
                sourceRectangle.Height = target.Clip.Height;
            }

            Rectangle targetRectangle = new Rectangle(target.X, target.Y, sourceRectangle.Width, sourceRectangle.Height);

            Graphics.DrawImage(source.Bitmap, targetRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        public void Dispose()
        {
            Graphics.Dispose();
            Bitmap.Dispose();
        }
    }
}