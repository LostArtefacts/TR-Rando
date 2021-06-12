using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRTexture16Importer;

namespace TRModelTransporter.Handlers
{
    public class ColourTransportHandler : AbstractTransportHandler
    {
        public override void Export()
        {
            ISet<int> colourIndices = new SortedSet<int>();
            foreach (TRMesh mesh in Definition.Meshes)
            {
                foreach (TRFace4 rect in mesh.ColouredRectangles)
                {
                    colourIndices.Add(BitConverter.GetBytes(rect.Texture)[1]);
                }
                foreach (TRFace3 tri in mesh.ColouredTriangles)
                {
                    colourIndices.Add(BitConverter.GetBytes(tri.Texture)[1]);
                }
            }

            Definition.Colours = new Dictionary<int, TRColour4>(colourIndices.Count);
            foreach (int i in colourIndices)
            {
                Definition.Colours[i] = Level.Palette16[i];
            }
        }

        public override void Import()
        {
            List<TRColour4> palette16 = Level.Palette16.ToList();
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            foreach (int paletteIndex in Definition.Colours.Keys)
            {
                TRColour4 newColour = Definition.Colours[paletteIndex];
                int existingIndex = palette16.FindIndex
                (
                    e => e.Red == newColour.Red && e.Green == newColour.Green && e.Blue == newColour.Blue// && e.Unused == newColour.Unused
                );

                if (existingIndex != -1)
                {
                    indexMap[paletteIndex] = existingIndex;
                }
                else
                {
                    indexMap[paletteIndex] = P16Importer.Import(Level, newColour);
                }
            }

            foreach (TRMesh mesh in Definition.Meshes)
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
            byte[] arr = BitConverter.GetBytes(value);
            int highByte = Convert.ToInt32(arr[1]);
            if (indexMap.ContainsKey(highByte))
            {
                arr[1] = (byte)indexMap[highByte];
                return BitConverter.ToUInt16(arr, 0);
            }
            return value;
        }
    }
}