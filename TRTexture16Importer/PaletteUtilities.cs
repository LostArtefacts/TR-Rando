using System.Drawing;
using TRLevelControl.Model;

namespace TRTexture16Importer;

public static class PaletteUtilities
{
    public static int Import(TR2Level lvl, Color c)
    {
        return Import(lvl, FromRGB(c));
    }

    public static int Import(TR3Level lvl, Color c)
    {
        return Import(lvl, FromRGB(c));
    }

    private static TRColour4 FromRGB(Color c)
    {
        return new TRColour4
        {
            Red = c.R,
            Green = c.G,
            Blue = c.B
        };
    }

    public static int Import(TR2Level lvl, TRColour4 c)
    {
        int nextAvailableIndex = GetNextPaletteIndex(lvl.Meshes, lvl.Palette16);
        if (nextAvailableIndex != -1)
        {
            lvl.Palette16[nextAvailableIndex] = c;
            c.Unused = 255; // Avoid anything else trying to use this index
        }
        
        return nextAvailableIndex;
    }

    public static int Import(TR3Level lvl, TRColour4 c)
    {
        int nextAvailableIndex = GetNextPaletteIndex(lvl.Meshes, lvl.Palette16);
        if (nextAvailableIndex != -1)
        {
            lvl.Palette16[nextAvailableIndex] = c;
            c.Unused = 255; // Avoid anything else trying to use this index
        }

        return nextAvailableIndex;
    }

    // #159 Using high indices as we were doing previously appears not to work in TR2Main,
    // so instead we will check for the next available palette slot and use that. Each level
    // seems to only have around 30-40 used colours. We still use the Unused field to track
    // what we are making use of during imports.
    public static int GetNextPaletteIndex(TRMesh[] meshes, List<TRColour4> colours)
    {
        int highestUsedPalette = -1;
        foreach (TRMesh mesh in meshes)
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

        while (highestUsedPalette < colours.Count - 1)
        {
            TRColour4 palette = colours[++highestUsedPalette];
            if (palette.Unused == 0)
            {
                return highestUsedPalette;
            }
        }

        return -1;
    }

    public static void ResetPaletteTracking(IEnumerable<TRColour4> colours)
    {
        foreach (TRColour4 c in colours)
        {
            if (c.Unused == 255)
            {
                c.Unused = 0;
            }
        }
    }
}
