using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRTexture16Importer.Helpers;
using TRTexture16Importer.Textures.Source;
using TRTexture16Importer.Textures.Target;

namespace TRTexture16Importer.Textures
{
    public class TextureLevelMapping : IDisposable
    {
        private static readonly Color _defaultSkyBox = Color.FromArgb(88, 152, 184);
        private static readonly int _tileWidth = 256;
        private static readonly int _tileHeight = 256;

        public Dictionary<DynamicTextureSource, DynamicTextureTarget> DynamicMapping { get; set; }
        public Dictionary<StaticTextureSource, List<StaticTextureTarget>> StaticMapping { get; set; }
        public Color DefaultSkyBox { get; set; }

        private readonly Dictionary<int, BitmapGraphics> _tileMap;
        private readonly TR2Level _level;

        private TextureLevelMapping(TR2Level level)
        {
            _level = level;
            _tileMap = new Dictionary<int, BitmapGraphics>();
        }

        public static TextureLevelMapping Get(TR2Level level, string lvlFile, TextureDatabase database)
        {
            string mapFile = Path.Combine(@"Resources\Textures\Mapping\", lvlFile + "-Textures.json");
            if (!File.Exists(mapFile))
            {
                return null;
            }

            Dictionary<DynamicTextureSource, DynamicTextureTarget> dynamicMapping = new Dictionary<DynamicTextureSource, DynamicTextureTarget>();
            Dictionary<StaticTextureSource, List<StaticTextureTarget>> staticMapping = new Dictionary<StaticTextureSource, List<StaticTextureTarget>>();
            Color skyBoxColour = _defaultSkyBox;

            Dictionary<string, object> rootMapping = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(mapFile));
            if (rootMapping.ContainsKey("Dynamic"))
            {
                SortedDictionary<string, DynamicTextureTarget> mapping = JsonConvert.DeserializeObject<SortedDictionary<string, DynamicTextureTarget>>(rootMapping["Dynamic"].ToString());
                foreach (string sourceName in mapping.Keys)
                {
                    DynamicTextureSource source = database.GetDynamicSource(sourceName);
                    dynamicMapping[source] = mapping[sourceName];
                }
            }

            if (rootMapping.ContainsKey("Static"))
            {
                SortedDictionary<string, object> mapping = JsonConvert.DeserializeObject<SortedDictionary<string, object>>(rootMapping["Static"].ToString());
                foreach (string sourceName in mapping.Keys)
                {
                    if (sourceName.ToUpper().Equals("SKYBOX"))
                    {
                        skyBoxColour = ColorTranslator.FromHtml(mapping[sourceName].ToString());
                    }
                    else
                    {
                        staticMapping[database.GetStaticSource(sourceName)] = JsonConvert.DeserializeObject<List<StaticTextureTarget>>(mapping[sourceName].ToString());
                    }
                }
            }

            return new TextureLevelMapping(level)
            {
                DynamicMapping = dynamicMapping,
                StaticMapping = staticMapping,
                DefaultSkyBox = skyBoxColour
            };
        }

        public void RedrawDynamicTargets(DynamicTextureSource source, string variant)
        {
            HSBOperation op = source.OperationMap[variant];
            DynamicTextureTarget target = DynamicMapping[source];
            
            foreach (int objectIndex in target.ObjectTextureIndices)
            {
                TRObjectTexture texture = _level.ObjectTextures[objectIndex];
                List<Point> points = new List<Point>();
                foreach (TRObjectTextureVert vert in texture.Vertices)
                {
                    points.Add(VertToPoint(vert));
                }

                Rectangle rect = GetRect(points, texture);
                GetBitmapGraphics(texture.AtlasAndFlag).AdjustHSB(rect, op);
            }

            foreach (int spriteIndex in target.SpriteTextureIndices)
            {
                TRSpriteTexture texture = _level.SpriteTextures[spriteIndex];
                Rectangle rect = new Rectangle(texture.X, texture.Y, (texture.Width + 1) / 256, (texture.Height + 1) / 256);
                GetBitmapGraphics(texture.Atlas).AdjustHSB(rect, op);
            }
        }

        private Point VertToPoint(TRObjectTextureVert vert)
        {
            int x = vert.XCoordinate.Fraction;
            if (vert.XCoordinate.Whole == 255)
            {
                x++;
            }

            int y = vert.YCoordinate.Fraction;
            if (vert.YCoordinate.Whole == 255)
            {
                y++;
            }
            return new Point(x, y);
        }

        private Rectangle GetRect(List<Point> points, TRObjectTexture texture)
        {
            TRObjectTextureVert lastVert = texture.Vertices[texture.Vertices.Length - 1];
            if (lastVert.XCoordinate.Fraction == 0 && lastVert.XCoordinate.Whole == 0 && lastVert.YCoordinate.Fraction == 0 && lastVert.YCoordinate.Whole == 0)
            {
                // The texture is used as a triangle, so remove the final point so that we extend to the full rectangle
                points.RemoveAt(points.Count - 1);
            }

            int minX = points.Min(p => p.X);
            int minY = points.Min(p => p.Y);
            int maxX = points.Max(p => p.X);
            int maxY = points.Max(p => p.Y);

            return new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
        }

        public void RedrawStaticTargets(StaticTextureSource source, string variant)
        {
            List<Rectangle> segments = source.VariantMap[variant];
            foreach (StaticTextureTarget target in StaticMapping[source])
            {
                if (target.Segment < 0 || target.Segment >= segments.Count)
                {
                    throw new IndexOutOfRangeException(string.Format("Segment {0} is invalid for texture source {1}.", target.Segment, source.PNGPath));
                }

                GetBitmapGraphics(target.Tile).ImportSegment(source, target, segments[target.Segment]);
            }

            if (source.ChangeSkyBox)
            {
                TRMesh skybox = TR2LevelUtilities.GetModelFirstMesh(_level, TR2Entities.Skybox_H);
                if (skybox != null)
                {
                    int skyColourIndex = _level.Palette16.ToList().FindIndex
                    (
                        e => e.Red == DefaultSkyBox.R && e.Green == DefaultSkyBox.G && e.Blue == DefaultSkyBox.B
                    );

                    // Let's use the final palette index
                    int newColourIndex = _level.Palette16.Length - 1;
                    Color c = source.Bitmap.GetPixel(segments[0].X, segments[0].Y);
                    _level.Palette16[newColourIndex] = new TRColour4
                    {
                        Red = c.R,
                        Green = c.G,
                        Blue = c.B,
                        Unused = 0
                    };

                    foreach (TRFace3 t in skybox.ColouredTriangles)
                    {
                        byte[] arr = BitConverter.GetBytes(t.Texture);
                        int highByte = Convert.ToInt32(arr[1]);
                        if (highByte == skyColourIndex)
                        {
                            arr[1] = (byte)newColourIndex;
                            t.Texture = BitConverter.ToUInt16(arr, 0);
                        }
                    }
                }
            }
        }

        private BitmapGraphics GetBitmapGraphics(int tile)
        {
            if (!_tileMap.ContainsKey(tile))
            {
                TRTexImage16 tex = _level.Images16[tile];

                Bitmap bmp = new Bitmap(_tileWidth, _tileHeight, PixelFormat.Format32bppArgb);
                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                List<byte> pixelCollection = new List<byte>();

                foreach (Textile16Pixel px in tex.To32BPPFormat())
                {
                    pixelCollection.AddRange(px.RGB32);
                }

                Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
                bmp.UnlockBits(bitmapData);

                _tileMap.Add(tile, new BitmapGraphics(bmp));
            }

            return _tileMap[tile];
        }

        public void Dispose()
        {
            foreach (int tile in _tileMap.Keys)
            {
                using (BitmapGraphics bmp = _tileMap[tile])
                {
                    _level.Images16[tile].Pixels = T16Importer.ImportFromBitmap(bmp.Bitmap);
                }
            }
        }
    }
}