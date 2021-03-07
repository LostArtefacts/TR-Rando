using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TRLevelReader.Model;
using TRTexture16Importer.Helpers;

namespace TRTexture16Importer.Textures
{
    public class TextureLevelMapping : IDisposable
    {
        public Dictionary<TextureSource, List<TextureTarget>> Mapping { get; set; }

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

            Dictionary<string, List<TextureTarget>> rawMapping = JsonConvert.DeserializeObject<Dictionary<string, List<TextureTarget>>>(File.ReadAllText(mapFile));
            Dictionary<TextureSource, List<TextureTarget>> mapping = new Dictionary<TextureSource, List<TextureTarget>>();
            foreach (string sourceName in rawMapping.Keys)
            {
                TextureSource source = database.Get(sourceName);
                mapping[source] = rawMapping[sourceName];
            }

            return new TextureLevelMapping(level)
            {
                Mapping = mapping
            };
        }

        public void RedrawTargets(TextureSource source, string colour)
        {
            List<Rectangle> segments = source.TextureMap[colour];
            foreach (TextureTarget target in Mapping[source])
            {
                GetBitmapGraphics(target.Tile).Draw(source, target, segments[target.Segment]);
            }
        }

        private BitmapGraphics GetBitmapGraphics(int tile)
        {
            if (!_tileMap.ContainsKey(tile))
            {
                const int width = 256;
                const int height = 256;

                TRTexImage16 tex = _level.Images16[tile];

                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
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