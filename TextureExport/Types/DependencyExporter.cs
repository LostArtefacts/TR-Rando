using Newtonsoft.Json;
using System.IO;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;

namespace TextureExport.Types
{
    public static class DependencyExporter
    {
        public static void Export(TR2Level level, string lvl)
        {
            TR2TextureRemapGroup remapGroup = new TR2TextureRemapGroup();
            foreach (TRModel model in level.Models)
            {
                remapGroup.CalculateDependencies(level, (TR2Entities)model.ID);
            }

            string dir = @"TR2\Deduplication";
            Directory.CreateDirectory(dir);
            File.WriteAllText(string.Format(@"{0}\{1}-TextureRemap.json", dir, lvl.ToUpper()), JsonConvert.SerializeObject(remapGroup, Formatting.Indented));
        }

        public static void Export(TR3Level level, string lvl)
        {
            TR3TextureRemapGroup remapGroup = new TR3TextureRemapGroup();
            foreach (TRModel model in level.Models)
            {
                remapGroup.CalculateDependencies(level, (TR3Entities)model.ID);
            }

            remapGroup.Dependencies.Sort(delegate (TextureDependency<TR3Entities> d1, TextureDependency<TR3Entities> d2)
            {
                if (d1.TileIndex == d2.TileIndex)
                {
                    if (d1.Bounds.X == d2.Bounds.X)
                    {
                        return d1.Bounds.Y.CompareTo(d2.Bounds.Y);
                    }
                    return d1.Bounds.X.CompareTo(d2.Bounds.X);
                }
                return d1.TileIndex.CompareTo(d2.TileIndex);
            });

            string dir = @"TR3\Deduplication";
            Directory.CreateDirectory(dir);
            File.WriteAllText(string.Format(@"{0}\{1}-TextureRemap.json", dir, lvl.ToUpper()), JsonConvert.SerializeObject(remapGroup, Formatting.Indented));
        }
    }
}