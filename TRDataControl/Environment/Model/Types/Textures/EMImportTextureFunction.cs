using System.Drawing;
using TRImageControl;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMImportTextureFunction : BaseEMFunction
{
    public string Bitmap { get; set; }
    public ushort Tile { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TRImage textile = new(Bitmap);
        List<Color> palette = level.Palette.Select(c => c.ToTR1Color()).ToList();
        textile.Read((c, x, y) =>
        {
            int colIndex;
            if (c.A == 0)
            {
                colIndex = 0;
            }
            else
            {
                colIndex = TRPalette8Control.FindClosestColour(c, palette);
            }

            level.Images8[Tile].Pixels[(y + Y) * TRConsts.TPageWidth + x + X] = (byte)colIndex;
        });
    }

    public override void ApplyToLevel(TR2Level level)
    {
        Import(level.Images16);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        Import(level.Images16);
    }

    private void Import(List<TRTexImage16> images)
    {
        TRImage textile = new(images[Tile].Pixels);
        textile.Import(new(Bitmap), new(X, Y));
        images[Tile].Pixels = textile.ToRGB555();
    }
}
