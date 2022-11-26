using RectanglePacker.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public abstract class AbstractLandmarkImporter<E, L>
        where E : Enum
        where L : class
    {
        public bool IsCommunityPatch { get; set; }

        protected abstract int MaxTextures { get; }

        protected abstract AbstractTexturePacker<E, L> CreatePacker(L level);
        protected abstract TRObjectTexture[] GetObjectTextures(L level);
        protected abstract void SetObjectTextures(L level, IEnumerable<TRObjectTexture> textures);
        protected abstract void SetRoomTexture(L level, int roomIndex, int rectangleIndex, ushort textureIndex);

        public bool Import(L level, AbstractTextureMapping<E, L> mapping, bool isLevelMirrored)
        {
            // Ensure any changes already made are committed to the level
            mapping.CommitGraphics();

            // If we are already at the maximum number of textures, bail out.
            List<TRObjectTexture> textures = GetObjectTextures(level).ToList();
            if (textures.Count == MaxTextures)
            {
                return false;
            }

            using (AbstractTexturePacker<E, L> packer = CreatePacker(level))
            {
                Dictionary<LandmarkTextureTarget, TexturedTileSegment> targetSegmentMap = new Dictionary<LandmarkTextureTarget, TexturedTileSegment>();

                foreach (StaticTextureSource<E> source in mapping.LandmarkMapping.Keys)
                {
                    if (textures.Count == MaxTextures)
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
                        Dictionary<int, LandmarkTextureTarget> backgroundCache = new Dictionary<int, LandmarkTextureTarget>();

                        foreach (LandmarkTextureTarget target in mapping.LandmarkMapping[source][segmentIndex])
                        {
                            if (target.BackgroundIndex != -1 && backgroundCache.ContainsKey(target.BackgroundIndex))
                            {
                                // The same graphic has already been added, so just copy the mapping.
                                // This is most likely for flipped rooms.
                                target.MappedTextureIndex = backgroundCache[target.BackgroundIndex].MappedTextureIndex;
                                targetSegmentMap[target] = targetSegmentMap[backgroundCache[target.BackgroundIndex]];
                                continue;
                            }

                            IndexedTRObjectTexture texture = CreateTexture(segments[segmentIndex], isLevelMirrored);
                            target.MappedTextureIndex = textures.Count;
                            textures.Add(texture.Texture);

                            Bitmap image;
                            if (target.BackgroundIndex != -1)
                            {
                                IndexedTRObjectTexture indexedTexture = new IndexedTRObjectTexture
                                {
                                    Index = target.BackgroundIndex,
                                    Texture = textures[target.BackgroundIndex]
                                };
                                BitmapGraphics tile = packer.Tiles[indexedTexture.Atlas].BitmapGraphics;
                                BitmapGraphics clip = new BitmapGraphics(tile.Extract(indexedTexture.Bounds));
                                clip.Overlay(source.Bitmap);
                                image = clip.Bitmap;

                                backgroundCache[target.BackgroundIndex] = target;
                            }
                            else
                            {
                                image = source.ClonedBitmap;
                            }

                            TexturedTileSegment segment = new TexturedTileSegment(texture, image);
                            packer.AddRectangle(segment);
                            targetSegmentMap[target] = segment;
                        }
                    }
                }

                try
                {
                    PackingResult<TexturedTile, TexturedTileSegment> result = packer.Pack(true);

                    // Perform the room data remapping
                    foreach (StaticTextureSource<E> source in mapping.LandmarkMapping.Keys)
                    {
                        if (!source.HasVariants)
                        {
                            continue;
                        }

                        foreach (int segmentIndex in mapping.LandmarkMapping[source].Keys)
                        {
                            foreach (LandmarkTextureTarget target in mapping.LandmarkMapping[source][segmentIndex])
                            {
                                if (target.MappedTextureIndex == -1 || result.Packer.OrphanedRectangles.Contains(targetSegmentMap[target]))
                                {
                                    // There wasn't enough space for this
                                    continue;
                                }

                                foreach (int rectIndex in target.RectangleIndices)
                                {
                                    SetRoomTexture(level, target.RoomNumber, rectIndex, (ushort)target.MappedTextureIndex);
                                }
                            }
                        }
                    }

                    SetObjectTextures(level, textures);

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
    }
}