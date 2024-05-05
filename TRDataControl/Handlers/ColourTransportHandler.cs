using TRImageControl;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class ColourTransportHandler
{
    public static void Export(TR1Level level, TR1Blob definition)
    {
        definition.Colours = GetUsedMeshColours(definition.Meshes, level.Palette);
    }

    public static void Export(TR2Level level, TR2Blob definition)
    {
        definition.Colours = GetUsedMeshColours(definition.Meshes, level.Palette16);
    }

    public static void Export(TR3Level level, TR3Blob definition)
    {
        definition.Colours = GetUsedMeshColours(definition.Meshes, level.Palette16);
    }

    private static Dictionary<int, TRColour> GetUsedMeshColours(List<TRMesh> meshes, List<TRColour> colours)
    {
        ISet<int> colourIndices = GetAllColourIndices(meshes, false);

        Dictionary<int, TRColour> usedColours = new();
        foreach (int i in colourIndices)
        {
            usedColours[i] = colours[i];
        }

        return usedColours;
    }

    private static Dictionary<int, TRColour4> GetUsedMeshColours(List<TRMesh> meshes, List<TRColour4> colours)
    {
        ISet<int> colourIndices = GetAllColourIndices(meshes, true);

        Dictionary<int, TRColour4> usedColours = new();
        foreach (int i in colourIndices)
        {
            usedColours[i] = colours[i];
        }

        return usedColours;
    }

    private static ISet<int> GetAllColourIndices(List<TRMesh> meshes, bool has16Bit)
    {
        ISet<int> colourIndices = new SortedSet<int>();
        foreach (TRMesh mesh in meshes)
        {
            foreach (TRMeshFace face in mesh.ColouredFaces)
            {
                colourIndices.Add(has16Bit ? face.Texture >> 8 : face.Texture);
            }
        }

        return colourIndices;
    }

    public static void Import(TR1Blob definition, TRPalette8Control paletteManager)
    {
        Dictionary<int, int> indexMap = new();

        foreach (int paletteIndex in definition.Colours.Keys)
        {
            TRColour newColour = definition.Colours[paletteIndex];
            indexMap[paletteIndex] = paletteManager.GetOrAddPaletteIndex(newColour);
        }

        paletteManager.WritePalletteToLevel();
        ReindexMeshTextures(definition.Meshes, indexMap, false);
    }

    public static void Import(TR2Level level, TR2Blob definition)
    {
        Dictionary<int, int> indexMap = new();
        TRPalette16Control tracker = new(level);
        
        foreach (int paletteIndex in definition.Colours.Keys)
        {
            TRColour4 newColour = definition.Colours[paletteIndex];
            indexMap[paletteIndex] = tracker.Import(newColour);
        }

        ReindexMeshTextures(definition.Meshes, indexMap, true);
    }

    public static void Import(TR3Level level, TR3Blob definition)
    {
        Dictionary<int, int> indexMap = new();
        TRPalette16Control tracker = new(level);

        foreach (int paletteIndex in definition.Colours.Keys)
        {
            TRColour4 newColour = definition.Colours[paletteIndex];
            indexMap[paletteIndex] = tracker.Import(newColour);
        }

        ReindexMeshTextures(definition.Meshes, indexMap, true);
    }

    private static void ReindexMeshTextures(List<TRMesh> meshes, Dictionary<int, int> indexMap, bool has16Bit)
    {
        foreach (TRMesh mesh in meshes)
        {
            foreach (TRMeshFace face in mesh.ColouredFaces)
            {
                face.Texture = ReindexTexture(face.Texture, indexMap, has16Bit);
            }
        }
    }

    private static ushort ReindexTexture(ushort value, Dictionary<int, int> indexMap, bool has16Bit)
    {
        int p16 = value;
        if (has16Bit)
        {
            p16 >>= 8;
        }
        if (indexMap.ContainsKey(p16))
        {
            return (ushort)(has16Bit ? (indexMap[p16] << 8 | (value & 0xFF)) : indexMap[p16]);
        }
        return value;
    }
}
