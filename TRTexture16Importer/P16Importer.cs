using System.Drawing;
using TRLevelReader.Model;

namespace TRTexture16Importer
{
    public static class P16Importer
    {
        public static int Import(TR2Level lvl, Color c)
        {
            return Import(lvl, new TRColour4
            {
                Red = c.R,
                Green = c.G,
                Blue = c.B
            });
        }

        public static int Import(TR2Level lvl, TRColour4 c)
        {
            for (int i = lvl.Palette16.Length - 1; i >= 0; i--)
            {
                TRColour4 palette = lvl.Palette16[i];
                if (palette.Unused == 0)
                {
                    lvl.Palette16[i] = c;
                    c.Unused = 1; // Avoid anything else trying to use this index
                    return i;
                }
            }
            return -1;
        }
    }
}