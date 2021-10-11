using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using TRRandomizerCore.Levels;
using TRLevelReader.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;
using TRTexture16Importer.Textures;
using TRTexture16Importer.Textures.Source;
using TRTexture16Importer.Textures.Target;

namespace TRRandomizerCore.Helpers
{
    // This class is in RandoCore rathern than TRTexture16Importer as otherwise
    // there is a cyclic dependency between it and ModelTransporter
    public class LandmarkImporter
    {
        private const int _maxTextures = 2048;

        public bool Import(TR2CombinedLevel level, TextureLevelMapping mapping, bool isLevelMirrored)
        {
            // Ensure any changes already made are committed to the level
            mapping.CommitGraphics();

            using (TexturePacker packer = new TexturePacker(level.Data))
            {
                List<TRObjectTexture> textures = level.Data.ObjectTextures.ToList();
                foreach (StaticTextureSource source in mapping.LandmarkMapping.Keys)
                {
                    if (textures.Count == _maxTextures)
                    {
                        break;
                    }

                    if (!source.HasVariants)
                    {
                        continue;
                    }

                    List<Rectangle> segments = source.VariantMap[source.Variants[0]];
                    foreach (int segmentIndex in mapping.LandmarkMapping[source].Keys)
                    {
                        foreach (LandmarkTextureTarget target in mapping.LandmarkMapping[source][segmentIndex])
                        {
                            IndexedTRObjectTexture texture = CreateTexture(segments[segmentIndex], isLevelMirrored);
                            target.MappedTextureIndex = textures.Count;
                            textures.Add(texture.Texture);

                            Bitmap image;
                            if (target.BackgroundIndex != -1)
                            {
                                IndexedTRObjectTexture indexedTexture = new IndexedTRObjectTexture
                                {
                                    Index = target.BackgroundIndex,
                                    Texture = level.Data.ObjectTextures[target.BackgroundIndex]
                                };
                                BitmapGraphics tile = GetTile(level.Data, indexedTexture.Atlas);

                                BitmapGraphics clip = new BitmapGraphics(tile.Extract(indexedTexture.Bounds));
                                clip.Overlay(source.Bitmap);
                                image = clip.Bitmap;
                            }
                            else
                            {
                                image = source.ClonedBitmap;
                            }

                            packer.AddRectangle(new TexturedTileSegment(texture, image));
                        }
                    }
                }

                try
                {
                    packer.Pack(true);

                    // Perform the room data remapping
                    foreach (StaticTextureSource source in mapping.LandmarkMapping.Keys)
                    {
                        if (!source.HasVariants)
                        {
                            continue;
                        }

                        foreach (int segmentIndex in mapping.LandmarkMapping[source].Keys)
                        {
                            foreach (LandmarkTextureTarget target in mapping.LandmarkMapping[source][segmentIndex])
                            {
                                if (target.MappedTextureIndex == -1)
                                {
                                    // There wasn't enough space for this
                                    continue;
                                }

                                TR2Room room = level.Data.Rooms[target.RoomNumber];
                                foreach (int rectIndex in target.RectangleIndices)
                                {
                                    room.RoomData.Rectangles[rectIndex].Texture = (ushort)target.MappedTextureIndex;
                                }
                            }
                        }
                    }

                    level.Data.ObjectTextures = textures.ToArray();
                    level.Data.NumObjectTextures = (uint)textures.Count;

                    return true;
                }
                catch (PackingException)
                {
                    return false;
                }
            }
        }

        private IndexedTRObjectTexture CreateTexture(Rectangle rectangle, bool mirrored)
        {
            // Configure the points and reverse them if the level is mirrored
            List<TRObjectTextureVert> vertices = new List<TRObjectTextureVert>
            {
                CreatePoint(0, 0),
                CreatePoint(rectangle.Width, 0),
                CreatePoint(rectangle.Width, rectangle.Height),
                CreatePoint(0, rectangle.Height)
            };

            if (mirrored)
            {
                vertices.Reverse();
            }

            // Make a dummy texture object with the given bounds
            TRObjectTexture texture = new TRObjectTexture
            {
                AtlasAndFlag = 0,
                Attribute = 0,
                Vertices = vertices.ToArray()
            };

            return new IndexedTRObjectTexture
            {
                Index = 0,
                Texture = texture
            };
        }

        private TRObjectTextureVert CreatePoint(int x, int y)
        {
            return new TRObjectTextureVert
            {
                XCoordinate = new FixedFloat16
                {
                    Whole = (byte)(x == 0 ? 1 : 255),
                    Fraction = (byte)(x == 0 ? 0 : x - 1)
                },
                YCoordinate = new FixedFloat16
                {
                    Whole = (byte)(y == 0 ? 1 : 255),
                    Fraction = (byte)(y == 0 ? 0 : y - 1)
                }
            };
        }

        private BitmapGraphics GetTile(TR2Level level, int index)
        {
            TRTexImage16 tex = level.Images16[index];

            Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            List<byte> pixelCollection = new List<byte>();

            foreach (Textile16Pixel px in tex.To32BPPFormat())
            {
                pixelCollection.AddRange(px.RGB32);
            }

            Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
            bmp.UnlockBits(bitmapData);

            return new BitmapGraphics(bmp);
        }
    }
}