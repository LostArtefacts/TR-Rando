using System;
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
            int nextAvailableIndex = GetNextPaletteIndex(lvl);
            if (nextAvailableIndex == -1)
            {
                return -1;
            }

            lvl.Palette16[nextAvailableIndex] = c;
            c.Unused = 1; // Avoid anything else trying to use this index
            return nextAvailableIndex;
        }

        // #159 Using high indices as we were doing previously appears not to work in TR2Main,
        // so instead we will check for the next available palette slot and use that. Each level
        // seems to only have around 30-40 used colours. We still use the Unused field to track
        // what we are making use of during imports.
        public static int GetNextPaletteIndex(TR2Level lvl)
        {
            int highestUsedPalette = -1;
            foreach (TRMesh mesh in lvl.Meshes)
            {
                foreach (TRFace4 t in mesh.ColouredRectangles)
                {
                    highestUsedPalette = Math.Max(highestUsedPalette, BitConverter.GetBytes(t.Texture)[1]);
                }
                foreach (TRFace3 t in mesh.ColouredTriangles)
                {
                    highestUsedPalette = Math.Max(highestUsedPalette, BitConverter.GetBytes(t.Texture)[1]);
                }
            }

            while (highestUsedPalette < lvl.Palette16.Length - 1)
            {
                TRColour4 palette = lvl.Palette16[++highestUsedPalette];
                if (palette.Unused == 0)
                {
                    return highestUsedPalette;
                }
            }

            return -1;
        }
    }
}