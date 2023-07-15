using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TRLevelControl.Model;
using TRTexture16Importer;

namespace TextureExport.Types
{
    public static class PngExporter
    {
        public static void Export(TRLevel level, string lvl)
        {
            Export(@"TR1\PNG", lvl, level.Images8, level.Palette);
        }

        public static void Export(TR2Level level, string lvl)
        {
            Export(@"TR2\PNG", lvl, level.Images16);
        }

        public static void Export(TR3Level level, string lvl)
        {
            Export(@"TR3\PNG", lvl, level.Images16);
        }

        public static void Export(string topDir, string lvl, IEnumerable<TRTexImage8> images, TRColour[] palette)
        {
            string dir = MakeDir(topDir, lvl);
            int index = 0;
            foreach (TRTexImage8 tex in images)
            {
                using (Bitmap bmp = tex.ToBitmap(palette))
                {
                    bmp.Save(Path.Combine(dir, index.ToString().PadLeft(2, '0') + ".png"));
                    index++;
                }
            }
        }

        public static void Export(string topDir, string lvl, IEnumerable<TRTexImage16> images)
        {
            string dir = MakeDir(topDir, lvl);
            int index = 0;
            foreach (TRTexImage16 tex in images)
            {
                using (Bitmap bmp = tex.ToBitmap())
                {
                    bmp.Save(Path.Combine(dir, index.ToString().PadLeft(2, '0') + ".png"));
                    index++;
                }
            }
        }

        private static string MakeDir(string topDir, string lvl)
        {
            string dir = Path.Combine(topDir, Path.GetFileNameWithoutExtension(lvl));
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);

            return dir;
        }
    }
}