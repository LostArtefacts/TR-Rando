using System.Drawing;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl;

public class TRPalette8Control
{
    public TR1Level Level { get; set; }
    public Dictionary<int, TRImage> ChangedTiles { get; set; }
    public List<TR1Type> ObsoleteTypes { get; set; }

    private List<Color> _palette, _predefinedPalette;

    public TRPalette8Control()
    {
        ChangedTiles = new();
        ObsoleteTypes = new();
    }

    public TRImage GetOriginalTile(int tileIndex)
    {
        return new(Level.Images8[tileIndex].Pixels, Level.Palette);
    }

    public void MergeTiles()
    {
        // Reuse the original transparency colour even though the RGB is redundant
        Color transparency = Level.Palette[0].ToTR1Color();
        _palette = new()
        {
            Color.FromArgb(0, transparency.R, transparency.G, transparency.B)
        };

        // Scan over replacement and original images, the idea being they will have been
        // updated as necessary with removals and additions. Store each unique colour in the
        // palette or replace with a suitable match.
        for (int i = 0; i < Level.Images8.Count; i++)
        {
            TRImage image = ChangedTiles.ContainsKey(i) ? ChangedTiles[i] : GetOriginalTile(i);
            image.Read((c, x, y) =>
            {
                int colIndex;
                if (c.A == 0)
                {
                    colIndex = 0;
                }
                else
                {
                    colIndex = GetOrAddPaletteIndex(c);
                    c = _palette[colIndex];
                }

                // Store the pointer in the level tiles
                Level.Images8[i].Pixels[y * TRConsts.TPageWidth + x] = (byte)colIndex;
            });
        }

        // Grab meshes we aren't interested in - but don't remove Lara's hips e.g. Atlantean spawns
        List<TRMesh> ignoredMeshes = new();
        Level.Models.TryGetValue(TR1Type.Lara, out TRModel lara);
        foreach (TR1Type entity in ObsoleteTypes)
        {
            Level.Models.TryGetValue(entity, out TRModel model);
            if (model == null)
            {
                continue;
            }

            foreach (TRMesh mesh in model.Meshes)
            {
                if (lara == null || !lara.Meshes.Contains(mesh))
                {
                    ignoredMeshes.AddRange(model.Meshes);
                }
            }
        }

        // Update all colours used in all meshes
        foreach (TRMesh mesh in Level.DistinctMeshes)
        {
            if (ignoredMeshes.Contains(mesh))
            {
                continue;
            }

            foreach (TRMeshFace face in mesh.ColouredFaces)
            {
                face.Texture = GetMeshFaceColour(face.Texture);
            }
        }

        WritePalletteToLevel();
    }

    private ushort GetMeshFaceColour(ushort colourRef)
    {
        if (colourRef >= TRConsts.PaletteSize)
        {
            if (_predefinedPalette != null)
            {
                // This is a predefined colour we're tracking during import, so it's in our palette and not the level's
                colourRef -= TRConsts.PaletteSize;
                return (ushort)GetOrAddPaletteIndex(_predefinedPalette[colourRef]);
            }

            return 0;
        }

        return (ushort)GetOrAddPaletteIndex(Level.Palette[colourRef]);
    }

    public void WritePalletteToLevel()
    {
        // Fill up the remainder with black
        while (_palette.Count < TRConsts.PaletteSize)
        {
            _palette.Add(Color.Black);
        }

        Level.Palette.Clear();
        Level.Palette.AddRange(_palette.Select(c => c.ToTRColour()));
    }

    public int AddPredefinedColour(Color c)
    {
        _predefinedPalette ??= new();

        int colIndex = _predefinedPalette.IndexOf(c);
        if (colIndex == -1)
        {
            colIndex = _predefinedPalette.Count;
            _predefinedPalette.Add(c);
        }

        return colIndex + TRConsts.PaletteSize;
    }

    public void MergePredefinedColours()
    {
        if (_predefinedPalette == null || _predefinedPalette.Count == 0)
        {
            return;
        }

        for (int i = _palette.Count - 1; i > 0; i--)
        {
            if (_palette[i] == Color.Black)
            {
                _palette.RemoveAt(i);
            }
            else
            {
                break;
            }
        }

        foreach (TRMesh mesh in Level.DistinctMeshes)
        {
            foreach (TRMeshFace face in mesh.ColouredFaces)
            {
                face.Texture = GetMeshFaceColour(face.Texture);
            }
        }

        WritePalletteToLevel();
    }

    public int GetOrAddPaletteIndex(TRColour c)
    {
        return GetOrAddPaletteIndex(c.ToTR1Color());
    }

    public int GetOrAddPaletteIndex(Color c)
    {
        int colIndex = _palette.IndexOf(c);
        if (colIndex == -1 || colIndex >= TRConsts.PaletteSize)
        {
            if (_palette.Count < TRConsts.PaletteSize)
            {
                // We have room to store this colour
                colIndex = _palette.Count;
                _palette.Add(c);
            }
            else
            {
                // There is no room for this colour, so find the nearest match in the palette
                colIndex = FindClosestColour(c);
            }
        }

        return colIndex;
    }

    private int FindClosestColour(Color colour)
    {
        return FindClosestColour(colour, _palette);
    }

    public static int FindClosestColour(Color colour, IEnumerable<Color> palette)
    {
        // Start at 1 to avoid matching black to transparency.
        return palette.FindClosest(colour, 1);
    }
}
