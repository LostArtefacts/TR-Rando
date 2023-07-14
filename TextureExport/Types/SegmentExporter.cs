using System.Collections.Generic;
using System.IO;
using TRLevelControl.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TextureExport.Types
{
    public static class SegmentExporter
    {
        public static void Export(TR2Level level, string lvl)
        {
            string folder = PrepareDirectory("TR2", lvl);
            using (TR2TexturePacker packer = new TR2TexturePacker(level))
            {
                Export(packer.Tiles, folder);
            }
        }

        public static void Export(TR3Level level, string lvl)
        {
            string folder = PrepareDirectory("TR3", lvl);
            using (TR3TexturePacker packer = new TR3TexturePacker(level))
            {
                Export(packer.Tiles, folder);
            }
        }

        private static string PrepareDirectory(string dir, string lvl)
        {
            string levelFolder = Path.Combine(dir + @"\Segments", Path.GetFileNameWithoutExtension(lvl));
            if (Directory.Exists(levelFolder))
            {
                Directory.Delete(levelFolder, true);
            }
            Directory.CreateDirectory(levelFolder);
            
            return levelFolder;
        }

        private static void Export(IReadOnlyList<TexturedTile> tiles, string folder)
        {
            foreach (TexturedTile tile in tiles)
            {
                foreach (TexturedTileSegment texture in tile.Rectangles)
                {
                    bool isSprite = texture.FirstTexture is IndexedTRSpriteTexture;
                    texture.Bitmap.Save(Path.Combine(folder, (isSprite ? "Sprite_" : "Object_") + texture.FirstTextureIndex + ".png"));
                }
            }
        }
    }
}