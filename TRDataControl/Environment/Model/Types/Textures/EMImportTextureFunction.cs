using System.Drawing;
using TRLevelControl;
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
        using BitmapGraphics bg = new(new Bitmap(Bitmap));
        List<Color> palette = level.Palette.Select(c => c.ToTR1Color()).ToList();
        Rectangle size = new(0, 0, bg.Bitmap.Width, bg.Bitmap.Height);
        bg.Scan(size, (c, x, y) =>
        {
            int colIndex;
            if (c.A == 0)
            {
                colIndex = 0;
            }
            else
            {
                colIndex = TRPalette8Control.FindClosestColour(c, palette);
                c = palette[colIndex];
            }

            level.Images8[Tile].Pixels[(y + Y) * TRConsts.TPageWidth + x + X] = (byte)colIndex;
            return c;
        });
    }

    public override void ApplyToLevel(TR2Level level)
    {
        using BitmapGraphics bg = new(level.Images16[Tile].ToBitmap());
        using Bitmap bmp = new(Bitmap);
        bg.Import(bmp, new Rectangle(X, Y, bmp.Width, bmp.Height));
        level.Images16[Tile].Pixels = TextureUtilities.ImportFromBitmap(bg.Bitmap);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        using BitmapGraphics bg = new(level.Images16[Tile].ToBitmap());
        using Bitmap bmp = new(Bitmap);
        bg.Import(bmp, new Rectangle(X, Y, bmp.Width, bmp.Height));
        level.Images16[Tile].Pixels = TextureUtilities.ImportFromBitmap(bg.Bitmap);
    }
}
