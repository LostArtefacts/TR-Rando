using System.Drawing;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl.Helpers;

public class TRPalette8Control : IDisposable
{
    private const int _paletteLimit = TRConsts.PaletteSize - 1;

    private readonly Rectangle _defaultBounds = new(0, 0, TRConsts.TPageWidth, TRConsts.TPageHeight);

    public TR1Level Level { get; set; }
    public Dictionary<int, Bitmap> ChangedTiles { get; set; }
    public List<TR1Type> ObsoleteModels { get; set; }

    private List<Color> _palette, _predefinedPalette;

    public TRPalette8Control()
    {
        ChangedTiles = new();
        ObsoleteModels = new();
    }

    public Bitmap GetOriginalTile(int tileIndex)
    {
        return Level.Images8[tileIndex].ToBitmap(Level.Palette);
    }

    public void MergeTiles()
    {
        _palette = new()
        {
            Color.FromArgb(0, 0, 0, 0) // Placeholder for transparency
        };

        // Scan over replacement and original images, the idea being they will have been
        // updated as necessary with removals and additions. Store each unique colour in the
        // palette or replace with a suitable match.
        for (int i = 0; i < Level.Images8.Count; i++)
        {
            Bitmap bmp = ChangedTiles.ContainsKey(i) ? ChangedTiles[i] : GetOriginalTile(i);
            TRImage bg = new(bmp);
            bg.Scan(_defaultBounds, (c, x, y) =>
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

                return c;
            });
        }

        // Grab meshes we aren't interested in - but don't remove Lara's hips e.g. Atlantean spawns
        List<TRMesh> ignoredMeshes = new();
        Level.Models.TryGetValue(TR1Type.Lara, out TRModel lara);
        foreach (TR1Type entity in ObsoleteModels)
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
        if (colourRef > _paletteLimit)
        {
            if (_predefinedPalette != null)
            {
                // This is a predefined colour we're tracking during import, so it's in our palette and not the level's
                colourRef -= (_paletteLimit + 1);
                return (ushort)GetOrAddPaletteIndex(_predefinedPalette[colourRef]);
            }

            return 0;
        }

        return (ushort)GetOrAddPaletteIndex(Level.Palette[colourRef]);
    }

    public void WritePalletteToLevel()
    {
        // Fill up the remainder with black
        while (_palette.Count <= _paletteLimit)
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

        return colIndex + _paletteLimit + 1;
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
        if (colIndex == -1 || colIndex > _paletteLimit)
        {
            if (_palette.Count <= _paletteLimit)
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

    public void Dispose()
    {
        foreach (Bitmap bmp in ChangedTiles.Values)
        {
            bmp.Dispose();
        }
        ChangedTiles.Clear();
        GC.SuppressFinalize(this);
    }
}
