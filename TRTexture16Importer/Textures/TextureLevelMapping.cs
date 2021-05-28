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
        public List<TextureGrouping> StaticGrouping { get; set; }
        public Color DefaultSkyBox { get; set; }

        private readonly Dictionary<int, BitmapGraphics> _tileMap;
        private readonly TR2Level _level;

        private TextureLevelMapping(TR2Level level)
        {
            _level = level;
            _tileMap = new Dictionary<int, BitmapGraphics>();
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
            List<TextureGrouping> staticGrouping = new List<TextureGrouping>();
            Color skyBoxColour = _defaultSkyBox;

            Dictionary<string, object> rootMapping = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(mapFile));

            // Read the dynamic mapping - this holds object and sprite texture indices for the level to which we will apply an HSB operation
            if (rootMapping.ContainsKey("Dynamic"))
            {
                SortedDictionary<string, Dictionary<int, List<Rectangle>>> mapping = JsonConvert.DeserializeObject<SortedDictionary<string, Dictionary<int, List<Rectangle>>>>(rootMapping["Dynamic"].ToString());
                foreach (string sourceName in mapping.Keys)
                {
                    DynamicTextureSource source = database.GetDynamicSource(sourceName);
                    dynamicMapping[source] = new DynamicTextureTarget
                    {
                        TileTargets = mapping[sourceName]
                    };
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

            // We can group specific static sources together to guarantee they get themed
            if (rootMapping.ContainsKey("Grouping"))
            {                
                List<Dictionary<string, object>> groupListData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(rootMapping["Grouping"].ToString());
                foreach (IDictionary<string, object> groupData in groupListData)
                {
                    TextureGrouping grouping = new TextureGrouping
                    {
                        Leader = database.GetStaticSource(groupData["Leader"].ToString())
                    };

                    SortedSet<string> followers = JsonConvert.DeserializeObject<SortedSet<string>>(groupData["Followers"].ToString());
                    foreach (string sourceName in followers)
                    {
                        grouping.Followers.Add(database.GetStaticSource(sourceName));
                    }

                    if (groupData.ContainsKey("ThemeAlternatives"))
                    {
                        Dictionary<string, Dictionary<string, string>> alternatives = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(groupData["ThemeAlternatives"].ToString());
                        foreach (string theme in alternatives.Keys)
                        {
                            Dictionary<StaticTextureSource, string> map = new Dictionary<StaticTextureSource, string>();
                            foreach (string sourceName in alternatives[theme].Keys)
                            {
                                map.Add(database.GetStaticSource(sourceName), alternatives[theme][sourceName]);
                            }
                            grouping.ThemeAlternatives.Add(theme, map);
                        }
                    }

                    staticGrouping.Add(grouping);
                }
            }

            // Allows for dynamic mapping to be targeted at levels e.g. when importing non-native
            // models that are otherwise undefined in the default level JSON data.
            if (predefinedMapping != null)
            {
                foreach (StaticTextureSource source in predefinedMapping.Keys)
                {
                    staticMapping[source] = predefinedMapping[source];
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

            return new TextureLevelMapping(level)
            {
                DynamicMapping = dynamicMapping,
                StaticMapping = staticMapping,
                StaticGrouping = staticGrouping,
                DefaultSkyBox = skyBoxColour
            };
        }

        public void RedrawTargets(AbstractTextureSource source, string variant)
        {
            if (source is DynamicTextureSource dynamicSource)
            {
                RedrawDynamicTargets(dynamicSource, variant);
            }
            else if (source is StaticTextureSource staticSource)
            {
                RedrawStaticTargets(staticSource, variant);
            }
        }

        public void RedrawDynamicTargets(DynamicTextureSource source, string variant)
        {
            HSBOperation op = source.OperationMap[variant];
            DynamicTextureTarget target = DynamicMapping[source];

            foreach (int tileIndex in target.TileTargets.Keys)
            {
                BitmapGraphics bg = GetBitmapGraphics(tileIndex);
                foreach (Rectangle rect in target.TileTargets[tileIndex])
                {
                    bg.AdjustHSB(rect, op);
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