using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRTexture16Importer.Helpers;
using TRTexture16Importer.Textures.Grouping;
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
        public Dictionary<StaticTextureSource, Dictionary<int, List<LandmarkTextureTarget>>> LandmarkMapping { get; set; }
        public List<TextureGrouping> StaticGrouping { get; set; }
        public Color DefaultSkyBox { get; set; }

        private readonly Dictionary<int, BitmapGraphics> _tileMap;
        private readonly TR2Level _level;
        private bool _committed;

        private TextureLevelMapping(TR2Level level)
        {
            _level = level;
            _tileMap = new Dictionary<int, BitmapGraphics>();
            _committed = false;
        }

        public static TextureLevelMapping Get(TR2Level level, string mappingFilePrefix, TextureDatabase database, Dictionary<StaticTextureSource, List<StaticTextureTarget>> predefinedMapping = null, List<TR2Entities> entitiesToIgnore = null)
        {
            string mapFile = Path.Combine(@"Resources\Textures\Mapping\", mappingFilePrefix + "-Textures.json");
            if (!File.Exists(mapFile))
            {
                return null;
            }

            Dictionary<DynamicTextureSource, DynamicTextureTarget> dynamicMapping = new Dictionary<DynamicTextureSource, DynamicTextureTarget>();
            Dictionary<StaticTextureSource, List<StaticTextureTarget>> staticMapping = new Dictionary<StaticTextureSource, List<StaticTextureTarget>>();
            Dictionary<StaticTextureSource, Dictionary<int, List<LandmarkTextureTarget>>> landmarkMapping = new Dictionary<StaticTextureSource, Dictionary<int, List<LandmarkTextureTarget>>>();
            Color skyBoxColour = _defaultSkyBox;

            Dictionary<string, object> rootMapping = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(mapFile));

            // Read the dynamic mapping - this holds object and sprite texture indices for the level to which we will apply an HSB operation
            if (rootMapping.ContainsKey("Dynamic"))
            {
                SortedDictionary<string, Dictionary<string, Dictionary<int, List<Rectangle>>>> mapping = JsonConvert.DeserializeObject<SortedDictionary<string, Dictionary<string, Dictionary<int, List<Rectangle>>>>>(rootMapping["Dynamic"].ToString());
                foreach (string sourceName in mapping.Keys)
                {
                    DynamicTextureSource source = database.GetDynamicSource(sourceName);
                    DynamicTextureTarget target = new DynamicTextureTarget
                    {
                        DefaultTileTargets = mapping[sourceName]["Default"]
                    };

                    if (mapping[sourceName].ContainsKey("Optional"))
                    {
                        target.OptionalTileTargets = mapping[sourceName]["Optional"];
                    }

                    dynamicMapping[source] = target;
                }
            }

            // The static mapping contains basic texture segment source to tile target locations
            if (rootMapping.ContainsKey("Static"))
            {
                SortedDictionary<string, object> mapping = JsonConvert.DeserializeObject<SortedDictionary<string, object>>(rootMapping["Static"].ToString());
                foreach (string sourceName in mapping.Keys)
                {
                    staticMapping[database.GetStaticSource(sourceName)] = JsonConvert.DeserializeObject<List<StaticTextureTarget>>(mapping[sourceName].ToString());
                }
            }

            // Landmark mapping links static sources to room number -> rectangle/triangle indices
            if (rootMapping.ContainsKey("Landmarks"))
            {
                Dictionary<string, Dictionary<int, List<LandmarkTextureTarget>>> mapping = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, List<LandmarkTextureTarget>>>>(rootMapping["Landmarks"].ToString());
                foreach (string sourceName in mapping.Keys)
                {
                    landmarkMapping[database.GetStaticSource(sourceName)] = mapping[sourceName];
                }
            }

            // If a level has had textures removed externally, but the JSON file has static
            // imports ready for it, we need to make sure they are ignored.
            if (entitiesToIgnore != null)
            {
                List<StaticTextureSource> sources = new List<StaticTextureSource>(staticMapping.Keys);
                for (int i = 0; i < sources.Count; i++)
                {
                    StaticTextureSource source = sources[i];
                    if (source.TextureEntities != null)
                    {
                        foreach (TR2Entities entity in source.TextureEntities)
                        {
                            if (entitiesToIgnore.Contains(entity))
                            {
                                staticMapping.Remove(source);
                                break;
                            }
                        }
                    }
                }
            }

            // Allows for dynamic mapping to be targeted at levels e.g. when importing non-native
            // models that are otherwise undefined in the default level JSON data.
            // This should be done after removing ignored entity textures, for the likes of when
            // Lara is being replaced.
            if (predefinedMapping != null)
            {
                foreach (StaticTextureSource source in predefinedMapping.Keys)
                {
                    staticMapping[source] = predefinedMapping[source];
                }
            }

            // Apply grouping to what has been selected as source elements
            List<TextureGrouping> staticGrouping = database.GlobalGrouping.GetGrouping(staticMapping.Keys);

            return new TextureLevelMapping(level)
            {
                DynamicMapping = dynamicMapping,
                StaticMapping = staticMapping,
                StaticGrouping = staticGrouping,
                LandmarkMapping = landmarkMapping,
                DefaultSkyBox = skyBoxColour
            };
        }

        public void RedrawTargets(AbstractTextureSource source, string variant, bool includeOptionalTargets)
        {
            if (source is DynamicTextureSource dynamicSource)
            {
                RedrawDynamicTargets(dynamicSource, variant, includeOptionalTargets);
            }
            else if (source is StaticTextureSource staticSource)
            {
                RedrawStaticTargets(staticSource, variant);
            }
        }

        public void RedrawDynamicTargets(DynamicTextureSource source, string variant, bool includeOptionalTargets)
        {
            HSBOperation op = source.OperationMap[variant];
            DynamicTextureTarget target = DynamicMapping[source];

            RedrawDynamicTargets(target.DefaultTileTargets, op);

            if (includeOptionalTargets)
            {
                RedrawDynamicTargets(target.OptionalTileTargets, op);
            }
        }

        private void RedrawDynamicTargets(Dictionary<int, List<Rectangle>> targets, HSBOperation operation)
        {
            foreach (int tileIndex in targets.Keys)
            {
                BitmapGraphics bg = GetBitmapGraphics(tileIndex);
                foreach (Rectangle rect in targets[tileIndex])
                {
                    bg.AdjustHSB(rect, operation);
                }
            }
        }

        public void RedrawStaticTargets(StaticTextureSource source, string variant)
        {
            // This can happen if we have a source grouped for this level,
            // but the source is actually only in place on certain conditions
            // - an example is the flame in Venice, which is only added if
            // the Flamethrower has been imported.
            if (!StaticMapping.ContainsKey(source))
            {
                return;
            }

            List<Rectangle> segments = source.VariantMap[variant];
            foreach (StaticTextureTarget target in StaticMapping[source])
            {
                if (target.Segment < 0 || target.Segment >= segments.Count)
                {
                    throw new IndexOutOfRangeException(string.Format("Segment {0} is invalid for texture source {1}.", target.Segment, source.PNGPath));
                }

                GetBitmapGraphics(target.Tile).ImportSegment(source, target, segments[target.Segment]);
            }

            if (source.EntityColourMap != null)
            {
                foreach (TR2Entities entity in source.EntityColourMap.Keys)
                {
                    TRMesh[] meshes = TR2LevelUtilities.GetModelMeshes(_level, entity);
                    ISet<int> colourIndices = new HashSet<int>();
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

                    Dictionary<int, int> remapIndices = new Dictionary<int, int>();
                    foreach (Color targetColour in source.EntityColourMap[entity].Keys)
                    {
                        int matchedIndex = -1;
                        foreach (int currentIndex in colourIndices)
                        {
                            TRColour4 currentColour = _level.Palette16[currentIndex];
                            if (currentColour.Red == targetColour.R && currentColour.Green == targetColour.G && currentColour.Blue == targetColour.B)
                            {
                                matchedIndex = currentIndex;
                            }
                        }

                        if (matchedIndex == -1)
                        {
                            continue;
                        }

                        // Extract the colour from the top-left of the rectangle specified in the source, and import that into the level
                        int sourceRectangle = source.EntityColourMap[entity][targetColour];
                        int newColourIndex = P16Importer.Import(_level, source.Bitmap.GetPixel(segments[sourceRectangle].X, segments[sourceRectangle].Y));
                        remapIndices.Add(matchedIndex, newColourIndex);
                    }

                    // Remap the affected mesh textures to the newly inserted colours
                    foreach (TRMesh mesh in meshes)
                    {
                        foreach (TRFace4 t in mesh.ColouredRectangles)
                        {
                            t.Texture = ConvertMeshTexture(t.Texture, remapIndices);
                        }
                        foreach (TRFace3 t in mesh.ColouredTriangles)
                        {
                            t.Texture = ConvertMeshTexture(t.Texture, remapIndices);
                        }
                    }
                }

                // Reset the palette tracking 
                P16Importer.ResetPaletteTracking(_level);
            }
        }

        private ushort ConvertMeshTexture(ushort texture, Dictionary<int, int> remapIndices)
        {
            byte[] arr = BitConverter.GetBytes(texture);
            int highByte = Convert.ToInt32(arr[1]);
            if (remapIndices.ContainsKey(highByte))
            {
                arr[1] = (byte)remapIndices[highByte];
                return BitConverter.ToUInt16(arr, 0);
            }
            return texture;
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
            CommitGraphics();
        }

        public void CommitGraphics()
        {
            if (!_committed)
            {
                foreach (int tile in _tileMap.Keys)
                {
                    using (BitmapGraphics bmp = _tileMap[tile])
                    {
                        _level.Images16[tile].Pixels = T16Importer.ImportFromBitmap(bmp.Bitmap);
                    }
                }
                _committed = true;
            }
        }
    }
}