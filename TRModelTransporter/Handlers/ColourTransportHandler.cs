using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRModelTransporter.Model.Definitions;
using TRTexture16Importer;

namespace TRModelTransporter.Handlers
{
    public class ColourTransportHandler
    {
        public void Export(TRLevel level, TR1ModelDefinition definition)
        {
            definition.Colours = GetUsedMeshColours(definition.Meshes, level.Palette);
        }

        public void Export(TR2Level level, TR2ModelDefinition definition)
        {
            definition.Colours = GetUsedMeshColours(definition.Meshes, level.Palette16);
        }

        public void Export(TR3Level level, TR3ModelDefinition definition)
        {
            definition.Colours = GetUsedMeshColours(definition.Meshes, level.Palette16);
        }

        private Dictionary<int, TRColour> GetUsedMeshColours(TRMesh[] meshes, TRColour[] colours)
        {
            ISet<int> colourIndices = GetAllColourIndices(meshes);

            Dictionary<int, TRColour> usedColours = new Dictionary<int, TRColour>();
            foreach (int i in colourIndices)
            {
                usedColours[i] = colours[i];
            }

            return usedColours;
        }

        private Dictionary<int, TRColour4> GetUsedMeshColours(TRMesh[] meshes, TRColour4[] colours)
        {
            ISet<int> colourIndices = GetAllColourIndices(meshes);

            Dictionary<int, TRColour4> usedColours = new Dictionary<int, TRColour4>();
            foreach (int i in colourIndices)
            {
                usedColours[i] = colours[i];
            }

            return usedColours;
        }

        private ISet<int> GetAllColourIndices(TRMesh[] meshes)
        {
            ISet<int> colourIndices = new SortedSet<int>();
            foreach (TRMesh mesh in meshes)
            {
                foreach (TRFace4 rect in mesh.ColouredRectangles)
                {
                    colourIndices.Add(rect.Texture >> 8);
                }
                foreach (TRFace3 tri in mesh.ColouredTriangles)
                {
                    colourIndices.Add(tri.Texture >> 8);
                }
            }

            return colourIndices;
        }

        public void Import(TRLevel level, TR1ModelDefinition definition)
        {
            // Limited to 256 colours so need to work out how to best handle this
            throw new NotImplementedException();
        }

        public void Import(TR2Level level, TR2ModelDefinition definition)
        {
            List<TRColour4> palette16 = level.Palette16.ToList();
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            
            foreach (int paletteIndex in definition.Colours.Keys)
            {
                TRColour4 newColour = definition.Colours[paletteIndex];
                int existingIndex = FindPaletteIndex(palette16, newColour);
                indexMap[paletteIndex] = existingIndex == -1 ? PaletteUtilities.Import(level, newColour) : existingIndex;
            }

            ReindexMeshTextures(definition.Meshes, indexMap);

            PaletteUtilities.ResetPaletteTracking(level.Palette16);
        }

        public void Import(TR3Level level, TR3ModelDefinition definition)
        {
            List<TRColour4> palette16 = level.Palette16.ToList();
            Dictionary<int, int> indexMap = new Dictionary<int, int>();

            foreach (int paletteIndex in definition.Colours.Keys)
            {
                TRColour4 newColour = definition.Colours[paletteIndex];
                int existingIndex = FindPaletteIndex(palette16, newColour);
                indexMap[paletteIndex] = existingIndex == -1 ? PaletteUtilities.Import(level, newColour) : existingIndex;
            }

            ReindexMeshTextures(definition.Meshes, indexMap);

            PaletteUtilities.ResetPaletteTracking(level.Palette16);
        }

        private int FindPaletteIndex(List<TRColour4> colours, TRColour4 colour)
        {
            return colours.FindIndex
            (
                e => e.Red == colour.Red && e.Green == colour.Green && e.Blue == colour.Blue
            );
        }

        private void ReindexMeshTextures(TRMesh[] meshes, Dictionary<int, int> indexMap)
        {
            foreach (TRMesh mesh in meshes)
            {
                foreach (TRFace4 rect in mesh.ColouredRectangles)
                {
                    rect.Texture = ReindexTexture(rect.Texture, indexMap);
                }
                foreach (TRFace3 tri in mesh.ColouredTriangles)
                {
                    tri.Texture = ReindexTexture(tri.Texture, indexMap);
                }
            }
        }

        private ushort ReindexTexture(ushort value, Dictionary<int, int> indexMap)
        {
            int p16 = value >> 8;
            if (indexMap.ContainsKey(p16))
            {
                return (ushort)(indexMap[p16] << 8 | (value & 0xFF));
            }
            return value;
        }
    }
}