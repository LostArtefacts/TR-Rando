using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TextureExport.Types;

public static class HtmlExporter
{
    public static void Export(TR1Level level, string lvlName)
    {
        TR1TexturePacker packer = new(level);
        StringBuilder tiles = new();
        BuildTiles(tiles, packer.Tiles, level.Palette);

        StringBuilder levelSel = new();
        BuildLevelSelect(levelSel, lvlName, TR1LevelNames.AsOrderedList);

        StringBuilder skyboxInfo = new();
        Dictionary<int, TRColour4> skyColours = new();
        BuildSkyBox(skyboxInfo, skyColours);

        StringBuilder palette = new();
        IEnumerable<Color> colors = level.Palette
            .Select(c => c.ToTR1Color())
            .Distinct();
        palette.Append(colors.Count()).Append(" unique colours");

        Write("TR1", lvlName, tiles, levelSel, skyboxInfo, palette);
    }

    public static void Export(TR2Level level, string lvlName)
    {
        TR2TexturePacker packer = new(level);
        StringBuilder tiles = new();
        BuildTiles(tiles, packer.Tiles);

        StringBuilder levelSel = new();
        BuildLevelSelect(levelSel, lvlName, TR2LevelNames.AsOrderedList);

        StringBuilder skyboxInfo = new();
        Dictionary<int, TRColour4> skyColours = GetSkyBoxColours(level.Models[TR2Type.Skybox_H]?.Meshes, level.Palette16);
        BuildSkyBox(skyboxInfo, skyColours);

        Write("TR2", lvlName, tiles, levelSel, skyboxInfo);
    }

    public static void Export(TR3Level level, string lvlName)
    {
        TR3TexturePacker packer = new(level);
        StringBuilder tiles = new();
        BuildTiles(tiles, packer.Tiles);

        StringBuilder levelSel = new();
        BuildLevelSelect(levelSel, lvlName, TR3LevelNames.AsOrderedList);

        StringBuilder skyboxInfo = new();
        Dictionary<int, TRColour4> skyColours = GetSkyBoxColours(level.Models[TR3Type.Skybox_H]?.Meshes, level.Palette16);
        BuildSkyBox(skyboxInfo, skyColours);

        Write("TR3", lvlName, tiles, levelSel, skyboxInfo);
    }

    private static void BuildTiles(StringBuilder html, IReadOnlyList<TRTextile> tiles, List<TRColour> palette = null)
    {
        foreach (TRTextile tile in tiles)
        {
            html.Append(string.Format("<div class=\"tile\" id=\"tile_{0}\">", tile.Index));

            foreach (TRTextileRegion region in tile.Rectangles)
            {
                using MemoryStream ms = new();
                region.Image.Save(ms, ImageFormat.Png);

                List<int> objectTextures = GetObjectTextureList(region);
                List<int> spriteTextures = GetSpriteTextureList(region);

                html.Append(string.Format("<img src=\"data:image/png;base64, {0}\" ", Convert.ToBase64String(ms.ToArray())));
                html.Append(string.Format("style=\"top:{0}px;left:{1}px;width:{2}px;height:{3}px\" ", region.Bounds.Y, region.Bounds.X, region.Bounds.Width, region.Bounds.Height));
                html.Append(string.Format("data-tile=\"{0}\" ", tile.Index));
                html.Append(string.Format("data-rect=\"{0}\" ", RectangleToString(region.Bounds)));

                if (palette != null)
                {
                    // Assume 8-bit so we want to see the palette indices for this segment
                    ISet<int> paletteIndices = new SortedSet<int>();
                    for (int y = 0; y < region.Image.Height; y++)
                    {
                        for (int x = 0; x < region.Image.Width; x++)
                        {
                            Color c = region.Image.GetPixel(x, y);
                            if (c.A != 0)
                            {
                                TRColour col = c.ToTRColour();
                                int index = palette.FindIndex(p => p.Red == col.Red && p.Green == col.Green && p.Blue == col.Blue);
                                if (index != -1)
                                {
                                    paletteIndices.Add(index);
                                }
                            }
                        }
                    }

                    html.Append(string.Format("data-palette=\"{0}\" ", string.Join(",", paletteIndices)));
                }

                List<string> objectData = new();
                List<string> spriteData = new();

                foreach (TRTextileSegment segment in region.Segments)
                {
                    if (segment.Texture is TRObjectTexture objTexture)
                    {
                        objectData.Add(segment.Index + ":" + RectangleToString(region.Bounds) + ":" + (objTexture.IsTriangle ? "T" : "Q"));
                    }
                    else
                    {
                        spriteData.Add(segment.Index + ":" + RectangleToString(segment.Bounds));
                    }
                }

                if (objectData.Count > 0)
                {
                    html.Append(string.Format("data-objects=\"{0}\" ", string.Join(";", objectData)));
                }
                if (spriteData.Count > 0)
                {
                    html.Append(string.Format("data-sprites=\"{0}\" ", string.Join(";", spriteData)));
                }

                html.Append("/>");
            }

            html.Append("</div>");
        }
    }

    private static string RectangleToString(Rectangle r)
    {
        return string.Format("[{0}, {1}, {2}, {3}]", r.X, r.Y, r.Width, r.Height);
    }

    private static List<int> GetObjectTextureList(TRTextileRegion region)
    {
        return new(region.Segments.Where(s => s.Texture is TRObjectTexture)
            .Select(s => s.Index));
    }

    private static List<int> GetSpriteTextureList(TRTextileRegion region)
    {
        return new(region.Segments.Where(s => s.Texture is TRSpriteTexture)
            .Select(s => s.Index));
    }

    private static void BuildLevelSelect(StringBuilder html, string currentLevel, IEnumerable<string> levelNames)
    {
        foreach (string lvl in levelNames)
        {
            html.Append("<option");
            if (lvl.ToUpper() == currentLevel.ToUpper())
            {
                html.Append(" selected=\"selected\"");
            }
            html.Append(" value=\"").Append(lvl).Append(".html\">").Append(lvl).Append("</option>");
        }
    }

    private static Dictionary<int, TRColour4> GetSkyBoxColours(List<TRMesh> meshes, List<TRColour4> palette16)
    {
        Dictionary<int, TRColour4> colours = new();
        if (meshes != null)
        {
            ISet<int> colourIndices = new SortedSet<int>();
            foreach (TRMesh mesh in meshes)
            {
                foreach (TRMeshFace face in mesh.ColouredFaces)
                {
                    colourIndices.Add(BitConverter.GetBytes(face.Texture)[1]);
                }
            }

            foreach (int i in colourIndices)
            {
                colours.Add(i, palette16[i]);
            }
        }
        return colours;
    }

    private static void BuildSkyBox(StringBuilder html, Dictionary<int, TRColour4> skyColours)
    {
        if (skyColours.Count > 0)
        {
            html.Append("<div><span>Palette #</span><span>RGB</span><span>Swatch</span></div>");
            string rgbTpl = "<span>[{0}, {1}, {2}]</span>";
            string swatchTpl = "<span style=\"background:rgb({0},{1},{2})\">&nbsp;</span>"; ;
            foreach (int index in skyColours.Keys)
            {
                TRColour4 c = skyColours[index];
                html.Append("<div class=\"body-row\"><span>").Append(index).Append("</span>");
                html.Append(string.Format(rgbTpl, c.Red, c.Green, c.Blue));
                html.Append(string.Format(swatchTpl, c.Red, c.Green, c.Blue));
                html.Append("</div>");
            }
        }
    }

    private static void Write(string dir, string lvlName, StringBuilder tiles, StringBuilder levelSelect, StringBuilder skyBox, StringBuilder palette = null)
    {
        string tpl = File.ReadAllText(@"Resources\TileTemplate.html");
        tpl = tpl.Replace("{Title}", lvlName);
        tpl = tpl.Replace("{Levels}", levelSelect.ToString());
        tpl = tpl.Replace("{Tiles}", tiles.ToString());
        tpl = tpl.Replace("{SkyBox}", skyBox.ToString());
        tpl = tpl.Replace("{Palette}", palette == null ? string.Empty : palette.ToString());

        dir += @"\HTML";
        Directory.CreateDirectory(dir);

        File.WriteAllText(string.Format(@"{0}\{1}.html", dir, lvlName), tpl);
    }
}
