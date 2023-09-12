using System.Drawing;
using TRLevelControl.Model;

namespace TRTexture16Importer;

public class PaletteTracker
{
    private readonly List<TRColour4> _replacedColours;

    public PaletteTracker()
    {
        _replacedColours = new();
    }

    public int Import(TR2Level lvl, Color c)
    {
        return Import(lvl, FromRGB(c));
    }

    public int Import(TR3Level lvl, Color c)
    {
        return Import(lvl, FromRGB(c));
    }

    private static TRColour4 FromRGB(Color c)
    {
        return new()
        {
            Red = c.R,
            Green = c.G,
            Blue = c.B
        };
    }

    public int Import(TR2Level lvl, TRColour4 c)
    {
        return Import(c, lvl.Meshes, lvl.Palette16);
    }

    public int Import(TR3Level lvl, TRColour4 c)
    {
        return Import(c, lvl.Meshes, lvl.Palette16);
    }

    public int Import(TRColour4 colour, TRMesh[] meshes, List<TRColour4> palette)
    {
        int existingIndex = palette.FindIndex(c
            => c.Red == colour.Red && c.Green == colour.Green && c.Blue == colour.Blue);
        if (existingIndex != -1)
        {
            return existingIndex;
        }

        int highestIndex = -1;
        foreach (TRMesh mesh in meshes)
        {
            foreach (TRFace4 t in mesh.ColouredRectangles)
            {
                highestIndex = Math.Max(highestIndex, BitConverter.GetBytes(t.Texture)[1]);
            }
            foreach (TRFace3 t in mesh.ColouredTriangles)
            {
                highestIndex = Math.Max(highestIndex, BitConverter.GetBytes(t.Texture)[1]);
            }
        }

        while (highestIndex < palette.Count - 1)
        {
            TRColour4 oldColour = palette[++highestIndex];
            if (!_replacedColours.Contains(oldColour))
            {
                _replacedColours.Add(oldColour);
                palette[highestIndex] = colour;
                return highestIndex;
            }
        }

        return FindClosestColour(colour, palette);
    }

    public static int FindClosestColour(TRColour4 colour, List<TRColour4> palette)
    {
        int colIndex = 0;
        double bestMatch = double.MaxValue;

        for (int i = 1; i < palette.Count; i++)
        {
            double match = Math.Sqrt
            (
                Math.Pow(colour.Red - palette[i].Red, 2) +
                Math.Pow(colour.Green - palette[i].Green, 2) +
                Math.Pow(colour.Blue - palette[i].Blue, 2)
            );

            if (match < bestMatch)
            {
                colIndex = i;
                bestMatch = match;
            }
        }

        return colIndex;
    }
}
