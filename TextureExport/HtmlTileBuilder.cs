using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TRLevelReader.Model;

namespace TextureExport
{
    public class HtmlTileBuilder : IDisposable
    {
        private readonly SortedDictionary<int, SortedSet<MappedRectangle>> _tileMap;
        private readonly Dictionary<int, Bitmap> _bmpMap;
        private readonly TR2Level _level;

        public HtmlTileBuilder(TR2Level level)
        {
            _tileMap = new SortedDictionary<int, SortedSet<MappedRectangle>>();
            _bmpMap = new Dictionary<int, Bitmap>();
            _level = level;
        }

        public void ExportAllTexturesToHtml(string filePath)
        {
            LoadObjectTextureData();
            LoadSpriteTextureData();
            Export(filePath);
        }

        private void LoadObjectTextureData()
        {
            for (int i = 0; i < _level.NumObjectTextures; i++)
            {
                TRObjectTexture texture = _level.ObjectTextures[i];
                int tile = texture.AtlasAndFlag;
                if (!_tileMap.ContainsKey(tile))
                {
                    _tileMap.Add(tile, new SortedSet<MappedRectangle>());
                }

                List<Point> points = new List<Point>();
                foreach (TRObjectTextureVert vert in texture.Vertices)
                {
                    points.Add(VertToPoint(vert));
                }

                Rectangle rect = GetRectangle(points, texture);
                AddRectangle(rect, tile, i);
            }
        }

        private void LoadSpriteTextureData()
        {
            for (int i = 0; i < _level.NumSpriteTextures; i++)
            {
                TRSpriteTexture texture = _level.SpriteTextures[i];
                int tile = texture.Atlas;
                if (!_tileMap.ContainsKey(tile))
                {
                    _tileMap.Add(tile, new SortedSet<MappedRectangle>());
                }

                Rectangle rect = new Rectangle(texture.X, texture.Y, (texture.Width + 1) / 256, (texture.Height + 1) / 256);
                AddRectangle(rect, tile, i, true);
            }
        }

        private static Point VertToPoint(TRObjectTextureVert vert)
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

        private Rectangle GetRectangle(List<Point> points, TRObjectTexture texture)
        {
            if (TextureIsTriangle(texture))
            {
                points.RemoveAt(3);
            }

            int minX = points.Min(p => p.X);
            int minY = points.Min(p => p.Y);
            int maxX = points.Max(p => p.X);
            int maxY = points.Max(p => p.Y);

            return new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
        }

        private bool TextureIsTriangle(TRObjectTexture texture)
        {
            TRObjectTextureVert[] verts = texture.Vertices;
            if (verts.Length != 4)
            {
                throw new Exception();
            }

            TRObjectTextureVert lastVert = verts[3];
            return lastVert.XCoordinate.Fraction == 0 && lastVert.XCoordinate.Whole == 0 && lastVert.YCoordinate.Fraction == 0 && lastVert.YCoordinate.Whole == 0;
        }

        private void AddRectangle(Rectangle rect, int tile, int objectIndex, bool isSprite = false)
        {
            SortedSet<MappedRectangle> mappedRectangles = _tileMap[tile];
            MappedRectangle mr = null;
            foreach (MappedRectangle mrect in mappedRectangles)
            {
                if (mrect.R.Equals(rect) || mrect.R.Contains(rect) || rect.Contains(mrect.R))
                {
                    mr = mrect;
                    if (rect.Contains(mr.R))
                    {
                        mrect.R = rect;
                        mrect.ObjectTextureIndices.Clear();
                    }
                    break;
                }
            }

            if (mr == null)
            {
                _tileMap[tile].Add(mr = new MappedRectangle
                {
                    R = rect
                });
            }

            if (isSprite)
            {
                mr.SpriteTextureIndices.Add(objectIndex);
            }
            else
            {
                mr.ObjectTextureIndices.Add(objectIndex);
            }
        }

        private Bitmap GetBitmap(int tile)
        {
            if (!_bmpMap.ContainsKey(tile))
            {
                TRTexImage16 tex = _level.Images16[tile];

                Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                List<byte> pixelCollection = new List<byte>();

                foreach (Textile16Pixel px in tex.To32BPPFormat())
                {
                    pixelCollection.AddRange(px.RGB32);
                }

                Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
                bmp.UnlockBits(bitmapData);

                _bmpMap.Add(tile, bmp);
            }
            return _bmpMap[tile];
        }

        private void Export(string levelName)
        {
            StringBuilder tiles = new StringBuilder();
            foreach (int tile in _tileMap.Keys)
            {
                tiles.Append(string.Format("<div class=\"tile\" id=\"tile_{0}\">", tile));

                Bitmap tileBmp = GetBitmap(tile);
                foreach (MappedRectangle mr in _tileMap[tile])
                {
                    using (Bitmap bmp = new Bitmap(mr.R.Width, mr.R.Height, PixelFormat.Format32bppArgb))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Graphics g = Graphics.FromImage(bmp);
                        g.DrawImage(tileBmp, new Rectangle(0, 0, mr.R.Width, mr.R.Height), mr.R, GraphicsUnit.Pixel);
                        bmp.Save(ms, ImageFormat.Png);

                        tiles.Append(string.Format("<img src=\"data:image/png;base64, {0}\" ", Convert.ToBase64String(ms.ToArray())));
                        tiles.Append(string.Format("style=\"top:{0}px;left:{1}px;width:{2}px;height:{3}px\" ", mr.R.Y, mr.R.X, mr.R.Width, mr.R.Height));
                        tiles.Append(string.Format("data-tile=\"{0}\" ", tile));

                        StringBuilder tooltip = new StringBuilder();
                        tooltip.Append("Tile: " + tile);
                        tooltip.Append(string.Format(";Rectangle: [{0}, {1}, {2}, {3}]", mr.R.Y, mr.R.X, mr.R.Width, mr.R.Height));

                        if (mr.ObjectTextureIndices.Count > 0)
                        {
                            string inds = string.Join(", ", mr.ObjectTextureIndices);
                            tiles.Append(string.Format("data-objects=\"{0}\" ", inds));
                            tooltip.Append(";Object Texture Indices: " + inds);
                        }
                        if (mr.SpriteTextureIndices.Count > 0)
                        {
                            string inds = string.Join(", ", mr.SpriteTextureIndices);
                            tiles.Append(string.Format("data-sprites=\"{0}\" ", inds));
                            tooltip.Append(";Sprite Texture Indices: " + inds);
                        }

                        tiles.Append(string.Format("title=\"{0}\" />", tooltip));
                    }
                }

                tiles.Append("</div>");
            }

            string tpl = File.ReadAllText(@"Resources\TileTemplate.html");
            tpl = tpl.Replace("{Title}", levelName);
            tpl = tpl.Replace("{Tiles}", tiles.ToString());

            File.WriteAllText(levelName + ".html", tpl);
        }

        public void Dispose()
        {
            foreach (Bitmap bmp in _bmpMap.Values)
            {
                bmp.Dispose();
            }
        }
    }

    class MappedRectangle : IComparable<MappedRectangle>
    {
        public Rectangle R { get; set; }
        public SortedSet<int> ObjectTextureIndices { get; set; }
        public SortedSet<int> SpriteTextureIndices { get; set; }

        public MappedRectangle()
        {
            ObjectTextureIndices = new SortedSet<int>();
            SpriteTextureIndices = new SortedSet<int>();
        }

        public int CompareTo(MappedRectangle other)
        {
            Point p1 = R.Location;
            Point p2 = other.R.Location;
            if (p1.X == p2.X)
            {
                return p1.Y.CompareTo(p2.Y);
            }
            if (p1.Y == p2.Y)
            {
                return p1.X.CompareTo(p2.X);
            }
            return p1.Y.CompareTo(p2.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is MappedRectangle rectangle && EqualityComparer<Rectangle>.Default.Equals(R, rectangle.R);
        }

        public override int GetHashCode()
        {
            return -51877379 + R.GetHashCode();
        }

        public override string ToString()
        {
            return R.ToString() + ", [Objects=" + string.Join(", ", ObjectTextureIndices) + "], [Sprites=" + string.Join(", ", SpriteTextureIndices) + "]";
        }
    }
}