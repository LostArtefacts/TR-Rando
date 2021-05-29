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

namespace TextureExport
{
    public class HtmlTileBuilder
    {
        private readonly TR2Level _level;

        public HtmlTileBuilder(TR2Level level)
        {
            _level = level;
        }

        public void ExportAllTexturesToHtml(string filePath)
        {
            using (TexturePacker packer = new TexturePacker(_level))
            {
                StringBuilder tiles = new StringBuilder();
                foreach (TexturedTile tile in packer.Tiles)
                {
                    tiles.Append(string.Format("<div class=\"tile\" id=\"tile_{0}\">", tile.Index));

                    foreach (TexturedTileSegment segment in tile.Rectangles)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            segment.Bitmap.Save(ms, ImageFormat.Png);

                            List<int> objectTextures = GetObjectTextureList(segment);
                            List<int> spriteTextures = GetSpriteTextureList(segment);

                            tiles.Append(string.Format("<img src=\"data:image/png;base64, {0}\" ", Convert.ToBase64String(ms.ToArray())));
                            tiles.Append(string.Format("style=\"top:{0}px;left:{1}px;width:{2}px;height:{3}px\" ", segment.Bounds.Y, segment.Bounds.X, segment.Bounds.Width, segment.Bounds.Height));
                            tiles.Append(string.Format("data-tile=\"{0}\" ", tile.Index));
                            tiles.Append(string.Format("data-rect=\"{0}\" ", RectangleToString(segment.Bounds)));

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
                                tiles.Append(string.Format("data-objects=\"{0}\" ", string.Join(";", objectData)));
                            }
                            if (spriteData.Count > 0)
                            {
                                tiles.Append(string.Format("data-sprites=\"{0}\" ", string.Join(";", spriteData)));
                            }

                            tiles.Append("/>");
                        }
                    }

                    tiles.Append("</div>");
                }

                StringBuilder levelSel = new StringBuilder();
                foreach (string lvl in LevelNames.AsList)
                {
                    levelSel.Append("<option");
                    if (lvl.ToUpper() == filePath.ToUpper())
                    {
                        levelSel.Append(" selected=\"selected\"");
                    }
                    levelSel.Append(" value=\"").Append(lvl).Append(".html\">").Append(lvl).Append("</option>");
                }

                StringBuilder skyboxInfo = new StringBuilder();
                Dictionary<int, TRColour4> skyColours = GetSkyBoxColours();
                if (skyColours.Count > 0)
                {
                    skyboxInfo.Append("<div><span>Palette #</span><span>RGB</span><span>Swatch</span></div>");
                    string rgbTpl = "<span>[{0}, {1}, {2}]</span>";
                    string swatchTpl = "<span style=\"background:rgb({0},{1},{2})\">&nbsp;</span>"; ;
                    foreach (int index in skyColours.Keys)
                    {
                        TRColour4 c = skyColours[index];
                        skyboxInfo.Append("<div class=\"body-row\"><span>").Append(index).Append("</span>");
                        skyboxInfo.Append(string.Format(rgbTpl, c.Red, c.Green, c.Blue));
                        skyboxInfo.Append(string.Format(swatchTpl, c.Red, c.Green, c.Blue));
                        skyboxInfo.Append("</div>");
                    }
                }

                string tpl = File.ReadAllText(@"Resources\TileTemplate.html");
                tpl = tpl.Replace("{Title}", filePath);
                tpl = tpl.Replace("{Levels}", levelSel.ToString());
                tpl = tpl.Replace("{Tiles}", tiles.ToString());
                tpl = tpl.Replace("{SkyBox}", skyboxInfo.ToString());

                File.WriteAllText(filePath + ".html", tpl);
            }
        }

        private string RectangleToString(Rectangle r)
        {
            return string.Format("[{0}, {1}, {2}, {3}]", r.X, r.Y, r.Width, r.Height);
        }

        private List<int> GetObjectTextureList(TexturedTileSegment segment)
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

        private List<int> GetSpriteTextureList(TexturedTileSegment segment)
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

        private Dictionary<int, TRColour4> GetSkyBoxColours()
        {
            Dictionary<int, TRColour4> colours = new Dictionary<int, TRColour4>();
            TRMesh[] meshes = TR2LevelUtilities.GetModelMeshes(_level, TR2Entities.Skybox_H);
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
                    colours.Add(i, _level.Palette16[i]);
                }
            }
            return colours;
        }

        public void ExportAllTextureSegments(string filePath)
        {
            string levelFolder = Path.Combine("Segments", Path.GetFileNameWithoutExtension(filePath));
            if (Directory.Exists(levelFolder))
            {
                Directory.Delete(levelFolder, true);
            }
            Directory.CreateDirectory(levelFolder);

            using (TexturePacker packer = new TexturePacker(_level))
            {
                foreach (TexturedTile tile in packer.Tiles)
                {
                    foreach (TexturedTileSegment texture in tile.Rectangles)
                    {
                        bool isSprite = texture.FirstTexture is IndexedTRSpriteTexture;
                        texture.Bitmap.Save(Path.Combine(levelFolder, (isSprite ? "Sprite_" : "Object_") + texture.FirstTextureIndex + ".png"));
                    }
                }
            }
        }
    }
}