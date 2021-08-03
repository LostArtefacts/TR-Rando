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
        public List<FaceConversionTextureTarget> FaceConversions { get; set; }
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
            List<FaceConversionTextureTarget> faceConversions = new List<FaceConversionTextureTarget>();
            Color skyBoxColour = _defaultSkyBox;

            Dictionary<string, object> rootMapping = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(mapFile));

            // Read the dynamic mapping - this holds object and sprite texture indices for the level to which we will apply an HSB operation
            if (rootMapping.ContainsKey("Dynamic"))
            {
                SortedDictionary<string, Dictionary<string, object>> mapping = JsonConvert.DeserializeObject<SortedDictionary<string, Dictionary<string, object>>>(rootMapping["Dynamic"].ToString());
                foreach (string sourceName in mapping.Keys)
                {
                    DynamicTextureSource source = database.GetDynamicSource(sourceName);
                    DynamicTextureTarget target = new DynamicTextureTarget
                    {
                        DefaultTileTargets = JsonConvert.DeserializeObject<Dictionary<int, List<Rectangle>>>(mapping[sourceName]["Default"].ToString())
                    };

                    if (mapping[sourceName].ContainsKey("Optional"))
                    {
                        target.OptionalTileTargets = JsonConvert.DeserializeObject<Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>>>(mapping[sourceName]["Optional"].ToString());
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

            // Allows vertices to be amended in existing textures to fix original game issues, or even to rotate/flip textures
            if (rootMapping.ContainsKey("FaceConversions"))
            {
                faceConversions = JsonConvert.DeserializeObject<List<FaceConversionTextureTarget>>(rootMapping["FaceConversions"].ToString());
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

            // Add global sources, unless they are already defined. These tend to be sprite sequences
            // so they will be mapped per GenerateSpriteSequenceTargets, but there is also scope to
            // define global targets if relevant.
            foreach (StaticTextureSource source in database.GlobalGrouping.Sources.Keys)
            {
                if (!staticMapping.ContainsKey(source))
                {
                    staticMapping[source] = new List<StaticTextureTarget>(database.GlobalGrouping.Sources[source]);
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
                FaceConversions = faceConversions,
                DefaultSkyBox = skyBoxColour
            };
        }

        public void RedrawTargets(AbstractTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
        {
            if (source is DynamicTextureSource dynamicSource)
            {
                RedrawDynamicTargets(dynamicSource, variant, options);
            }
            else if (source is StaticTextureSource staticSource)
            {
                RedrawStaticTargets(staticSource, variant, options);
            }
        }

        public void RedrawDynamicTargets(DynamicTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
        {
            HSBOperation op = source.OperationMap[variant];
            DynamicTextureTarget target = DynamicMapping[source];

            RedrawDynamicTargets(target.DefaultTileTargets, op);

            foreach (TextureCategory category in target.OptionalTileTargets.Keys)
            {
                if (options.ContainsKey(category) && options[category])
                {
                    RedrawDynamicTargets(target.OptionalTileTargets[category], op);
                }
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

        public void RedrawStaticTargets(StaticTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
        {
            if (source.Categories != null)
            {
                // Exclude it if any of its categories are in the options and switched off
                foreach (TextureCategory category in source.Categories)
                {
                    if (options.ContainsKey(category) && !options[category])
                    {
                        return;
                    }
                }
            }

            // For sprite sequence sources, the targets are mapped dynamically.
            if (source.IsSpriteSequence && (!StaticMapping.ContainsKey(source) || StaticMapping[source].Count == 0))
            {
                GenerateSpriteSequenceTargets(source);
            }

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

        private void GenerateSpriteSequenceTargets(StaticTextureSource source)
        {
            if (!source.HasVariants)
            {
                throw new ArgumentException(string.Format("SpriteSequence {0} cannot be dynamically mapped without at least one source rectangle.", source.SpriteSequence));
            }

            int i = _level.SpriteSequences.ToList().FindIndex(s => s.SpriteID == (int)source.SpriteSequence);
            if (i == -1)
            {
                return;
            }

            TRSpriteSequence sequence = _level.SpriteSequences[i];
            StaticMapping[source] = new List<StaticTextureTarget>();

            // An assumption is made here that each variant in the source will have the same number
            // of rectangles. We only want to define targets for the number of source rectangles, rather
            // than the total number of sprites.
            int numTargets = source.VariantMap[source.Variants[0]].Count;
            for (int j = 0; j < numTargets; j++)
            {
                TRSpriteTexture sprite = _level.SpriteTextures[sequence.Offset + j];
                StaticMapping[source].Add(new StaticTextureTarget
                {
                    Segment = j,
                    Tile = sprite.Atlas,
                    X = sprite.X,
                    Y = sprite.Y
                });
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

        public void ProcessFaceConversions()
        {
            foreach (FaceConversionTextureTarget conv in FaceConversions)
            {
                TR2RoomData roomData = _level.Rooms[conv.RoomNumber].RoomData;
                foreach (int rectIndex in conv.RectangleIndices)
                {
                    TRFace4 face = roomData.Rectangles[rectIndex];
                    ushort[] remappedVertices = new ushort[face.Vertices.Length];
                    for (int i = 0; i < face.Vertices.Length; i++)
                    {
                        if (conv.Conversion.ContainsKey(i))
                        {
                            remappedVertices[i] = face.Vertices[conv.Conversion[i]];
                        }
                        else
                        {
                            remappedVertices[i] = face.Vertices[i];
                        }
                    }
                    face.Vertices = remappedVertices;
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