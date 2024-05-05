using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TextureExport.Types;

public static class SegmentExporter
{
    public static void Export(TR2Level level, string lvl)
    {
        string folder = PrepareDirectory("TR2", lvl);
        TR2TexturePacker packer = new(level);
        Export(packer.Tiles, folder);
    }

    public static void Export(TR3Level level, string lvl)
    {
        string folder = PrepareDirectory("TR3", lvl);
        TR3TexturePacker packer = new(level);
        Export(packer.Tiles, folder);
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

    private static void Export(IReadOnlyList<TRTextile> tiles, string folder)
    {
        foreach (TRTextile tile in tiles)
        {
            foreach (TRTextileRegion texture in tile.Rectangles)
            {
                bool isSprite = texture.FirstTexture is IndexedTRSpriteTexture;
                texture.Image.Save(Path.Combine(folder, (isSprite ? "Sprite_" : "Object_") + texture.FirstTextureIndex + ".png"));
            }
        }
    }
}
