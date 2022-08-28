using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using TRTexture16Importer.Textures;

namespace TRTexture16Importer.Helpers
{
    public class BitmapGraphics : IDisposable
    {
        private Bitmap _bitmap;
        private int _width, _height;

        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _bitmap = value;
                _width = _bitmap.Width;
                _height = _bitmap.Height;

                Graphics = Graphics.FromImage(_bitmap);
            }
        }

        public Graphics Graphics { get; private set; }

        public BitmapGraphics(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public void AdjustHSB(Rectangle rect, HSBOperation operation)
        {
            Scan(rect, delegate (Color c, int x, int y)
            {
                return ApplyHSBOperation(c, operation);
            });
        }

        private Color ApplyHSBOperation(Color c, HSBOperation operation)
        {
            HSB hsb = c.ToHSB();
            hsb.H = operation.ModifyHue(hsb.H);
            hsb.S = operation.ModifySaturation(hsb.S);
            hsb.B = operation.ModifyBrightness(hsb.B);

            return hsb.ToColour();
        }

        public void Replace(Color search, Color replace, Rectangle rect)
        {
            Scan(rect, delegate (Color c, int x, int y)
            {
                return c == search ? replace : c;
            });
        }

        public void Scan(Rectangle rect, Func<Color, int, int, Color> action)
        {
            // This is about 25% faster than using GetPixel/SetPixel

            BitmapData bd = Bitmap.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, Bitmap.PixelFormat);
            int bytesPerPixel = Image.GetPixelFormatSize(Bitmap.PixelFormat) / 8;
            int byteCount = bd.Stride * _height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bd.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, byteCount);

            int startY = rect.Y;
            int endY = rect.Y + rect.Height;
            int startX = rect.X * bytesPerPixel;
            int endX = startX + rect.Width * bytesPerPixel;

            for (int y = startY; y < endY; y++)
            {
                int currentLine = y * bd.Stride;
                for (int x = startX; x < endX; x += bytesPerPixel)
                {
                    int b = pixels[currentLine + x];
                    int g = pixels[currentLine + x + 1];
                    int r = pixels[currentLine + x + 2];
                    int a = pixels[currentLine + x + 3];

                    Color c = Color.FromArgb(a, r, g, b);
                    c = action.Invoke(c, x / bytesPerPixel, y);

                    pixels[currentLine + x] = c.B;
                    pixels[currentLine + x + 1] = c.G;
                    pixels[currentLine + x + 2] = c.R;
                    pixels[currentLine + x + 3] = c.A;
                }
            }

            Marshal.Copy(pixels, 0, ptrFirstPixel, byteCount);
            Bitmap.UnlockBits(bd);
        }

        public void ImportSegment(Bitmap source, StaticTextureTarget target, Rectangle sourceSegment)
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

            Graphics.DrawImage(source, targetRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        public void Delete(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(rect);
            Graphics.SetClip(path);
            Graphics.Clear(Color.Transparent);
            Graphics.ResetClip();
        }

        public void Fill(Rectangle rect, Color c)
        {
            Graphics.FillRectangle(new SolidBrush(c), rect);
        }

        public void Import(Bitmap bitmap, Rectangle rect)
        {
            Delete(rect);
            Graphics.DrawImage(bitmap, rect);
        }

        public void Overlay(Bitmap bitmap)
        {
            Graphics.DrawImage(bitmap, new Rectangle(0, 0, Bitmap.Width, Bitmap.Height));
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