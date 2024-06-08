using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using TRLevelControl.Model;
using TRRandomizerCore.Textures;

namespace TextureExport.Types;

public static class TRRExporter
{
    public static void Export(string ddsFolder, TRGameVersion version)
    {
        string dir = $@"{version}\DDS\PNG";
        Directory.CreateDirectory(dir);

        foreach (string ddsFile in Directory.GetFiles(ddsFolder))
        {
            using Pfim.IImage image = Pfim.Pfimage.FromFile(ddsFile);
            GCHandle handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            try
            {
                IntPtr data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                Bitmap bitmap = new(image.Width, image.Height, image.Stride, PixelFormat.Format32bppArgb, data);
                bitmap.Save(Path.Combine(dir, Path.ChangeExtension(Path.GetFileName(ddsFile), ".png")), ImageFormat.Png);
            }
            finally
            {
                handle.Free();
            }
        }
    }

    public static void GenerateCategories(TRGameVersion version)
    {
        Dictionary<TRTexCategory, SortedSet<ushort>> categories = new();
        Dictionary<TRTexCategory, ushort> defaults = new();

        foreach (TRTexCategory category in Enum.GetValues<TRTexCategory>())
        {
            string dir = $@"{version}\DDS\Categories\{category}";
            if (!Directory.Exists(dir))
            {
                continue;
            }

            categories[category] = new();
            foreach (string img in Directory.GetFiles(dir))
            {
                if (ushort.TryParse(Path.GetFileNameWithoutExtension(img), out ushort id))
                {
                    categories[category].Add(id);
                }
            }
        }

        void WriteInfo<T>(List<Dictionary<string, Dictionary<T, TRItemFlags>>> itemFlags)
            where T : Enum
        {
            TRTexInfo<T> info = new()
            {
                Categories = categories,
                Defaults = defaults,
                ItemFlags = itemFlags
            };

            File.WriteAllText($@"{version}\DDS\texinfo.json", JsonConvert.SerializeObject(info));
        }

        switch (version)
        {
            case TRGameVersion.TR1:
                defaults[TRTexCategory.Transparent] = 477;
                defaults[TRTexCategory.Lever] = 34;
                WriteInfo(_tr1ItemFlags);
                break;

            case TRGameVersion.TR2:
                defaults[TRTexCategory.Transparent] = 159;
                defaults[TRTexCategory.Lever] = 1202;
                defaults[TRTexCategory.Ladder] = 1172;
                WriteInfo(_tr2ItemFlags);
                break;

            case TRGameVersion.TR3:
                defaults[TRTexCategory.Transparent] = 794;
                defaults[TRTexCategory.Lever] = 1002;
                defaults[TRTexCategory.Ladder] = 600;
                WriteInfo(_tr3ItemFlags);
                break;
        }
    }

    private static readonly List<Dictionary<string, Dictionary<TR1Type, TRItemFlags>>> _tr1ItemFlags = new()
    {
        
    };

    private static readonly List<Dictionary<string, Dictionary<TR2Type, TRItemFlags>>> _tr2ItemFlags = new()
    {

    };

    private static readonly List<Dictionary<string, Dictionary<TR3Type, TRItemFlags>>> _tr3ItemFlags = new()
    {

    };
}
