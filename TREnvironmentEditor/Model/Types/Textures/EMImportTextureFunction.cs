using System.Drawing;
using TRLevelControl.Model;
using TRTexture16Importer;
using TRTexture16Importer.Helpers;

namespace TREnvironmentEditor.Model.Types;

public class EMImportTextureFunction : BaseEMFunction
{
    public string Bitmap { get; set; }
    public ushort Tile { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        using (BitmapGraphics bg = new BitmapGraphics(new Bitmap(Bitmap)))
        {
            List<Color> palette = level.Palette.Select(c => Color.FromArgb(c.Red * 4, c.Green * 4, c.Blue * 4)).ToList();
            Rectangle size = new Rectangle(0, 0, bg.Bitmap.Width, bg.Bitmap.Height);
            bg.Scan(size, (c, x, y) =>
            {
                int colIndex;
                if (c.A == 0)
                {
                    colIndex = 0;
                }
                else
                {
                    colIndex = TR1PaletteManager.FindClosestColour(c, palette);
                    c = palette[colIndex];
                }

                level.Images8[Tile].Pixels[(y + Y) * 256 + x + X] = (byte)colIndex;
                return c;
            });
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        using (BitmapGraphics bg = new BitmapGraphics(level.Images16[Tile].ToBitmap()))
        using (Bitmap bmp = new Bitmap(Bitmap))
        {
            bg.Import(bmp, new Rectangle(X, Y, bmp.Width, bmp.Height));
            level.Images16[Tile].Pixels = TextureUtilities.ImportFromBitmap(bg.Bitmap);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        using (BitmapGraphics bg = new BitmapGraphics(level.Images16[Tile].ToBitmap()))
        using (Bitmap bmp = new Bitmap(Bitmap))
        {
            bg.Import(bmp, new Rectangle(X, Y, bmp.Width, bmp.Height));
            level.Images16[Tile].Pixels = TextureUtilities.ImportFromBitmap(bg.Bitmap);
        }
    }
}
