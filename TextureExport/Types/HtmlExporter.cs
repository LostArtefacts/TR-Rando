using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TextureExport.Types
{
    public static class HtmlExporter
    {
        public static void Export(TRLevel level, string lvlName)
        {
            using (TR1TexturePacker packer = new TR1TexturePacker(level))
            {
                StringBuilder tiles = new StringBuilder();
                BuildTiles(tiles, packer.Tiles);

                StringBuilder levelSel = new StringBuilder();
                BuildLevelSelect(levelSel, lvlName, TRLevelNames.AsOrderedList);

                StringBuilder skyboxInfo = new StringBuilder();
                Dictionary<int, TRColour4> skyColours = new Dictionary<int, TRColour4>();
                BuildSkyBox(skyboxInfo, skyColours);

                Write("TR1", lvlName, tiles, levelSel, skyboxInfo);
            }
        }

        public static void Export(TR2Level level, string lvlName)
        {
            using (TR2TexturePacker packer = new TR2TexturePacker(level))
            {
                StringBuilder tiles = new StringBuilder();
                BuildTiles(tiles, packer.Tiles);

                StringBuilder levelSel = new StringBuilder();
                BuildLevelSelect(levelSel, lvlName, TR2LevelNames.AsOrderedList);

                StringBuilder skyboxInfo = new StringBuilder();
                Dictionary<int, TRColour4> skyColours = GetSkyBoxColours(TRMeshUtilities.GetModelMeshes(level, TR2Entities.Skybox_H), level.Palette16);
                BuildSkyBox(skyboxInfo, skyColours);

                Write("TR2", lvlName, tiles, levelSel, skyboxInfo);
            }
        }

        public static void Export(TR3Level level, string lvlName)
        {
            using (TR3TexturePacker packer = new TR3TexturePacker(level))
            {
                StringBuilder tiles = new StringBuilder();
                BuildTiles(tiles, packer.Tiles);

                StringBuilder levelSel = new StringBuilder();
                BuildLevelSelect(levelSel, lvlName, TR3LevelNames.AsOrderedList);

                StringBuilder skyboxInfo = new StringBuilder();
                Dictionary<int, TRColour4> skyColours = GetSkyBoxColours(TRMeshUtilities.GetModelMeshes(level, TR3Entities.Skybox_H), level.Palette16);
                BuildSkyBox(skyboxInfo, skyColours);

                Write("TR3", lvlName, tiles, levelSel, skyboxInfo);
            }
        }

        private static void BuildTiles(StringBuilder html, IReadOnlyList<TexturedTile> tiles)
        {
            foreach (TexturedTile tile in tiles)
            {
                html.Append(string.Format("<div class=\"tile\" id=\"tile_{0}\">", tile.Index));

                foreach (TexturedTileSegment segment in tile.Rectangles)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        segment.Bitmap.Save(ms, ImageFormat.Png);

                        List<int> objectTextures = GetObjectTextureList(segment);
                        List<int> spriteTextures = GetSpriteTextureList(segment);

                        html.Append(string.Format("<img src=\"data:image/png;base64, {0}\" ", Convert.ToBase64String(ms.ToArray())));
                        html.Append(string.Format("style=\"top:{0}px;left:{1}px;width:{2}px;height:{3}px\" ", segment.Bounds.Y, segment.Bounds.X, segment.Bounds.Width, segment.Bounds.Height));
                        html.Append(string.Format("data-tile=\"{0}\" ", tile.Index));
                        html.Append(string.Format("data-rect=\"{0}\" ", RectangleToString(segment.Bounds)));

                        List<string> objectData = new List<string>();
                        List<string> spriteData = new List<string>();

                        foreach (AbstractIndexedTRTexture texture in segment.Textures)
                        {
                            if (texture is IndexedTRObjectTexture)
                            {
                                objectData.Add(texture.Index + ": " + RectangleToString(texture.Bounds));
                            }
                            else
                            {
                                spriteData.Add(texture.Index + ": " + RectangleToString(texture.Bounds));
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
                }

                html.Append("</div>");
            }
        }

        private static string RectangleToString(Rectangle r)
        {
            return string.Format("[{0}, {1}, {2}, {3}]", r.X, r.Y, r.Width, r.Height);
        }

        private static List<int> GetObjectTextureList(TexturedTileSegment segment)
        {
            List<int> indices = new List<int>();
            foreach (AbstractIndexedTRTexture texture in segment.Textures)
            {
                if (texture is IndexedTRObjectTexture)
                {
                    indices.Add(texture.Index);
                }
            }
            return indices;
        }

        private static List<int> GetSpriteTextureList(TexturedTileSegment segment)
        {
            List<int> indices = new List<int>();
            foreach (AbstractIndexedTRTexture texture in segment.Textures)
            {
                if (texture is IndexedTRSpriteTexture)
                {
                    indices.Add(texture.Index);
                }
            }
            return indices;
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

        private static Dictionary<int, TRColour4> GetSkyBoxColours(TRMesh[] meshes, TRColour4[] palette16)
        {
            Dictionary<int, TRColour4> colours = new Dictionary<int, TRColour4>();
            if (meshes != null)
            {
                ISet<int> colourIndices = new SortedSet<int>();
                foreach (TRMesh mesh in meshes)
                {
                    foreach (TRFace4 t in mesh.ColouredRectangles)
                    {
                        colourIndices.Add(BitConverter.GetBytes(t.Texture)[1]);
                    }
                    foreach (TRFace3 t in mesh.ColouredTriangles)
                    {
                        colourIndices.Add(BitConverter.GetBytes(t.Texture)[1]);
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

        private static void Write(string dir, string lvlName, StringBuilder tiles, StringBuilder levelSelect, StringBuilder skyBox)
        {
            string tpl = File.ReadAllText(@"Resources\TileTemplate.html");
            tpl = tpl.Replace("{Title}", lvlName);
            tpl = tpl.Replace("{Levels}", levelSelect.ToString());
            tpl = tpl.Replace("{Tiles}", tiles.ToString());
            tpl = tpl.Replace("{SkyBox}", skyBox.ToString());

            dir += @"\HTML";
            Directory.CreateDirectory(dir);

            File.WriteAllText(string.Format(@"{0}\{1}.html", dir, lvlName), tpl);
        }
    }
}