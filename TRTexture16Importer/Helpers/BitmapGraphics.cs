using System;
using System.Drawing;
using TRTexture16Importer.Textures.Source;
using TRTexture16Importer.Textures.Target;

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

        public void AdjustHSB(Rectangle rect, HSBOperation operation)
        {
            int yEnd = rect.Y + rect.Height;
            int xEnd = rect.X + rect.Width;
            for (int y = rect.Y; y < yEnd; y++)
            {
                for (int x = rect.X; x < xEnd; x++)
                {
                    Bitmap.SetPixel(x, y, ApplyHSBOperation(Bitmap.GetPixel(x, y), operation));
                }
            }
        }

        private Color ApplyHSBOperation(Color c, HSBOperation operation)
        {
            HSB hsb = c.ToHSB();
            hsb.H = operation.ModifyHue(hsb.H);
            hsb.S = operation.ModifySaturation(hsb.S);
            hsb.B = operation.ModifyBrightness(hsb.B);

            return hsb.ToColour();
        }

        public void ImportSegment(StaticTextureSource source, StaticTextureTarget target, Rectangle sourceSegment)
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