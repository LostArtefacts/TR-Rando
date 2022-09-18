using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRTexture16Importer.Helpers
{
    public class TR1PaletteManager : IDisposable
    {
        private const int _paletteLimit = byte.MaxValue;
        private const double _weightR = 1;
        private const double _weightG = 1;
        private const double _weightB = 1;

        private readonly Rectangle _defaultBounds = new Rectangle(0, 0, 256, 256);

        public TRLevel Level { get; set; }
        public Dictionary<int, Bitmap> ChangedTiles { get; set; }
        public List<TREntities> ObsoleteModels { get; set; }

        private List<Color> _palette, _predefinedPalette;

        public TR1PaletteManager()
        {
            ChangedTiles = new Dictionary<int, Bitmap>();
            ObsoleteModels = new List<TREntities>();
        }

        public Bitmap GetOriginalTile(int tileIndex)
        {
            return Level.Images8[tileIndex].ToBitmap(Level.Palette);
        }

        public void MergeTiles()
        {
            _palette = new List<Color>
            {
                Color.FromArgb(0, 0, 0, 0) // Placeholder for transparency
            };

            // Scan over replacement and original images, the idea being they will have been
            // updated as necessary with removals and additions. Store each unique colour in the
            // palette or replace with a suitable match.
            for (int i = 0; i < Level.Images8.Length; i++)
            {
                Bitmap bmp = ChangedTiles.ContainsKey(i) ? ChangedTiles[i] : GetOriginalTile(i);
                BitmapGraphics bg = new BitmapGraphics(bmp);
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
                    Level.Images8[i].Pixels[y * 256 + x] = (byte)colIndex;

                    return c;
                });
            }

            // Grab meshes we aren't interested in - but don't remove Lara's hips e.g. Atlantean spawns
            List<TRMesh> ignoredMeshes = new List<TRMesh>();
            TRMesh[] laraMeshes = TRMeshUtilities.GetModelMeshes(Level, TREntities.Lara);
            foreach (TREntities entity in ObsoleteModels)
            {
                TRMesh[] meshes = TRMeshUtilities.GetModelMeshes(Level, entity);
                if (meshes != null)
                {
                    foreach (TRMesh mesh in meshes)
                    {
                        if (laraMeshes == null || !laraMeshes.Contains(mesh))
                        {
                            ignoredMeshes.AddRange(meshes);
                        }
                    }
                }
            }

            // Update all colours used in all meshes
            foreach (TRMesh mesh in Level.Meshes)
            {
                if (ignoredMeshes.Contains(mesh))
                {
                    continue;
                }

                foreach (TRFace4 face in mesh.ColouredRectangles)
                {
                    face.Texture = GetMeshFaceColour(face.Texture);
                }
                foreach (TRFace3 face in mesh.ColouredTriangles)
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
                else
                {
                    return 0;
                }
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

            Level.Palette = new TRColour[_paletteLimit + 1];
            for (int i = 0; i < Level.Palette.Length; i++)
            {
                Color c = _palette[i];
                Level.Palette[i] = new TRColour
                {
                    Red = (byte)(c.R / 4),
                    Green = (byte)(c.G / 4),
                    Blue = (byte)(c.B / 4)
                };
            }
        }

        public int AddPredefinedColour(Color c)
        {
            if (_predefinedPalette == null)
            {
                _predefinedPalette = new List<Color>();
            }

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

            foreach (TRMesh mesh in Level.Meshes)
            {
                foreach (TRFace4 face in mesh.ColouredRectangles)
                {
                    face.Texture = GetMeshFaceColour(face.Texture);
                }
                foreach (TRFace3 face in mesh.ColouredTriangles)
                {
                    face.Texture = GetMeshFaceColour(face.Texture);
                }
            }

            WritePalletteToLevel();
        }

        public int GetOrAddPaletteIndex(TRColour c)
        {
            return GetOrAddPaletteIndex(Color.FromArgb(c.Red * 4, c.Green * 4, c.Blue * 4));
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
            // Compare the colour with each in the palette by finding a greyscale value:
            // best match wins. We don't actually care what the match value is, hence
            // no Sqrt so to boost performance. We start at 1 because we don't want to
            // match black to transparency.

            int colIndex = 0;
            int bestMatch = int.MaxValue;

            for (int i = 1; i < _palette.Count; i++)
            {
                double match =
                    Math.Pow((colour.R - _palette[i].R) * _weightR, 2) +
                    Math.Pow((colour.G - _palette[i].G) * _weightG, 2) +
                    Math.Pow((colour.B - _palette[i].B) * _weightB, 2);

                if (match < bestMatch)
                {
                    colIndex = i;
                    bestMatch = (int)match;
                }
            }

            return colIndex;
        }

        public void Dispose()
        {
            foreach (Bitmap bmp in ChangedTiles.Values)
            {
                bmp.Dispose();
            }
            ChangedTiles.Clear();
        }
    }
}