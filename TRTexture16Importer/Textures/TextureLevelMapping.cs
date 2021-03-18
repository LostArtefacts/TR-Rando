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

namespace TRTexture16Importer.Textures
{
    public class TextureLevelMapping : IDisposable
    {
        private static readonly Color _defaultSkyBox = Color.FromArgb(88, 152, 184);
        private static readonly int _tileWidth = 256;
        private static readonly int _tileHeight = 256;

        public Dictionary<TextureSource, List<TextureTarget>> Mapping { get; set; }
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

            SortedDictionary<string, object> rawMapping = JsonConvert.DeserializeObject<SortedDictionary<string, object>>(File.ReadAllText(mapFile));
            Dictionary<TextureSource, List<TextureTarget>> mapping = new Dictionary<TextureSource, List<TextureTarget>>();
            Color skyBoxColour = _defaultSkyBox;

            foreach (string sourceName in rawMapping.Keys)
            {
                if (sourceName.ToUpper().Equals("SKYBOX"))
                {
                    skyBoxColour = ColorTranslator.FromHtml(rawMapping[sourceName].ToString());
                }
                else
                {
                    mapping[database.Get(sourceName)] = JsonConvert.DeserializeObject<List<TextureTarget>>(rawMapping[sourceName].ToString());
                }
            }

            return new TextureLevelMapping(level)
            {
                Mapping = mapping,
                DefaultSkyBox = skyBoxColour
            };
        }

        public void RedrawTargets(TextureSource source, string variant)
        {
            List<Rectangle> segments = source.VariantMap[variant];
            foreach (TextureTarget target in Mapping[source])
            {
                if (target.Segment < 0 || target.Segment >= segments.Count)
                {
                    throw new IndexOutOfRangeException(string.Format("Segment {0} is invalid for texture source {1}.", target.Segment, source.PNGPath));
                }

                GetBitmapGraphics(target.Tile).Draw(source, target, segments[target.Segment]);
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