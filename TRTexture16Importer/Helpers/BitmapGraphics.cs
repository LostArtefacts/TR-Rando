using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

            if (target.Clear)
            {
                // For the likes of Golden Mask secrets, which have different
                // dimensions as compared with the dragons, we need to clear
                // the rectangle first.
                Delete(targetRectangle);
            }

            Graphics.DrawImage(source.Bitmap, targetRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        public void Delete(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(rect);
            Graphics.SetClip(path);
            Graphics.Clear(Color.Transparent);
            Graphics.ResetClip();
        }

        public void Import(Bitmap bitmap, Rectangle rect)
        {
            Graphics.DrawImage(bitmap, rect);//, sourceRectangle, GraphicsUnit.Pixel);
        }

        public Bitmap Extract(Rectangle rect, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            return Bitmap.Clone(rect, format);
        }

        public void Dispose()
        {
            Graphics.Dispose();
            Bitmap.Dispose();
        }
    }
}