using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TRLevelReader.Model;
using TRTexture16Importer.Helpers;

namespace TRTexture16Importer.Textures
{
    public abstract class AbstractTextureMapping<E, L> : IDisposable
        where E : Enum
        where L : class
    {
        private static readonly Color _defaultSkyBox = Color.FromArgb(88, 152, 184);

        public Dictionary<DynamicTextureSource, DynamicTextureTarget> DynamicMapping { get; set; }
        public Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> StaticMapping { get; set; }
        public List<ReplacementTextureTarget> ReplacementMapping { get; set; }
        public Dictionary<StaticTextureSource<E>, Dictionary<int, List<LandmarkTextureTarget>>> LandmarkMapping { get; set; }
        public List<TextureGrouping<E>> StaticGrouping { get; set; }
        public Color DefaultSkyBox { get; set; }
        public Dictionary<E, E> EntityMap { get; set; }

        protected readonly Dictionary<int, BitmapGraphics> _tileMap;
        protected readonly L _level;
        protected bool _committed;

        protected AbstractTextureMapping(L level)
        {
            _level = level;
            _tileMap = new Dictionary<int, BitmapGraphics>();
            _committed = false;
        }

        protected abstract TRMesh[] GetModelMeshes(E entity);
        protected abstract TRColour[] GetPalette8();
        protected abstract TRColour4[] GetPalette16();
        protected abstract int ImportColour(Color colour);
        protected abstract TRSpriteSequence[] GetSpriteSequences();
        protected abstract TRSpriteTexture[] GetSpriteTextures();
        protected abstract Bitmap GetTile(int tileIndex);
        protected abstract void SetTile(int tileIndex, Bitmap bitmap);

        protected static void LoadMapping(AbstractTextureMapping<E, L> levelMapping, string mapFile, TextureDatabase<E> database, Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> predefinedMapping = null, List<E> entitiesToIgnore = null)
        {
            Dictionary<DynamicTextureSource, DynamicTextureTarget> dynamicMapping = new Dictionary<DynamicTextureSource, DynamicTextureTarget>();
            Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> staticMapping = new Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>>();
            List<ReplacementTextureTarget> replacementMapping = new List<ReplacementTextureTarget>();
            Dictionary<StaticTextureSource<E>, Dictionary<int, List<LandmarkTextureTarget>>> landmarkMapping = new Dictionary<StaticTextureSource<E>, Dictionary<int, List<LandmarkTextureTarget>>>();
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

            // This allows replacing colours in specific areas of tiles with another and will be
            // performed post static and dynamic redrawing. Best example is fixing TR3 fake sky
            // textures in the likes of Jungle after replacing the main skybox.
            if (rootMapping.ContainsKey("ColourReplacements"))
            {
                replacementMapping = JsonConvert.DeserializeObject<List<ReplacementTextureTarget>>(rootMapping["ColourReplacements"].ToString());
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
                List<StaticTextureSource<E>> sources = new List<StaticTextureSource<E>>(staticMapping.Keys);
                for (int i = 0; i < sources.Count; i++)
                {
                    StaticTextureSource<E> source = sources[i];
                    if (source.TextureEntities != null)
                    {
                        foreach (E entity in source.TextureEntities)
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
                foreach (StaticTextureSource<E> source in predefinedMapping.Keys)
                {
                    staticMapping[source] = predefinedMapping[source];
                }
            }

            // Add global sources, unless they are already defined. These tend to be sprite sequences
            // so they will be mapped per GenerateSpriteSequenceTargets, but there is also scope to
            // define global targets if relevant.
            foreach (StaticTextureSource<E> source in database.GlobalGrouping.Sources.Keys)
            {
                if (!staticMapping.ContainsKey(source))
                {
                    staticMapping[source] = new List<StaticTextureTarget>(database.GlobalGrouping.Sources[source]);
                }
            }

            // Apply grouping to what has been selected as source elements
            List<TextureGrouping<E>> staticGrouping = database.GlobalGrouping.GetGrouping(staticMapping.Keys);

            levelMapping.DynamicMapping = dynamicMapping;
            levelMapping.StaticMapping = staticMapping;
            levelMapping.ReplacementMapping = replacementMapping;
            levelMapping.StaticGrouping = staticGrouping;
            levelMapping.LandmarkMapping = landmarkMapping;
            levelMapping.DefaultSkyBox = skyBoxColour;
        }

        public void RedrawTargets(AbstractTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
        {
            if (source is DynamicTextureSource dynamicSource)
            {
                RedrawDynamicTargets(dynamicSource, variant, options);
            }
            else if (source is StaticTextureSource<E> staticSource)
            {
                RedrawStaticTargets(staticSource, variant, options);
            }
        }

        public void RedrawDynamicTargets(DynamicTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
        {
            HSBOperation op = source.OperationMap[variant];
            DynamicTextureTarget target = DynamicMapping[source];

            if (options.ContainsKey(TextureCategory.LevelColours) && options[TextureCategory.LevelColours])
            {
                RedrawDynamicTargets(target.DefaultTileTargets, op);
            }

            foreach (TextureCategory category in target.OptionalTileTargets.Keys)
            {
                if (options.ContainsKey(category) && options[category])
                {
                    RedrawDynamicTargets(target.OptionalTileTargets[category], op);
                }
            }

            RecolourDynamicTargets(target.ModelColourTargets, op);
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

        private void RecolourDynamicTargets(List<TRMesh> meshes, HSBOperation operation)
        {
            TRColour[] palette = GetPalette8();
            ISet<ushort> colourIndices = new HashSet<ushort>();
            Dictionary<int, int> remapIndices = new Dictionary<int, int>();

            foreach (TRMesh mesh in meshes)
            {
                foreach (TRFace4 f in mesh.ColouredRectangles)
                {
                    colourIndices.Add(f.Texture);
                }
                foreach (TRFace3 f in mesh.ColouredTriangles)
                {
                    colourIndices.Add(f.Texture);
                }
            }

            foreach (ushort colourIndex in colourIndices)
            {
                if (colourIndex == 0)
                {
                    continue;
                }
                TRColour col = palette[colourIndex];
                Color c = Color.FromArgb(col.Red * 4, col.Green * 4, col.Blue * 4);
                HSB hsb = c.ToHSB();
                hsb.H = operation.ModifyHue(hsb.H);
                hsb.S = operation.ModifySaturation(hsb.S);
                hsb.B = operation.ModifyBrightness(hsb.B);

                int newColourIndex = ImportColour(hsb.ToColour());
                remapIndices.Add(colourIndex, newColourIndex);
            }

            foreach (TRMesh mesh in meshes)
            {
                foreach (TRFace4 f in mesh.ColouredRectangles)
                {
                    if (remapIndices.ContainsKey(f.Texture))
                    {
                        f.Texture = (ushort)remapIndices[f.Texture];
                    }
                }
                foreach (TRFace3 f in mesh.ColouredTriangles)
                {
                    if (remapIndices.ContainsKey(f.Texture))
                    {
                        f.Texture = (ushort)remapIndices[f.Texture];
                    }
                }
            }
        }

        public void RedrawStaticTargets(StaticTextureSource<E> source, string variant, Dictionary<TextureCategory, bool> options)
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

            // This can happen if we have a source grouped for this level, but the source is actually only
            // in place on certain conditions - an example is the flame in Venice, which is only added if
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

                GetBitmapGraphics(target.Tile).ImportSegment(source.Bitmap, target, segments[target.Segment]);
            }

            if (source.EntityColourMap != null)
            {
                TRColour4[] palette = GetPalette16();

                foreach (E entity in source.EntityColourMap.Keys)
                {
                    E translatedEntity = entity;
                    if (EntityMap != null && EntityMap.ContainsKey(entity))
                    {
                        translatedEntity = EntityMap[entity];
                    }
                    TRMesh[] meshes = GetModelMeshes(translatedEntity);
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
                            TRColour4 currentColour = palette[currentIndex];
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
                        int newColourIndex = ImportColour(source.Bitmap.GetPixel(segments[sourceRectangle].X, segments[sourceRectangle].Y));
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
                PaletteUtilities.ResetPaletteTracking(palette);
            }

            if (source.EntityColourMap8 != null)
            {
                foreach (E entity in source.EntityColourMap8.Keys)
                {
                    E translatedEntity = entity;
                    if (EntityMap != null && EntityMap.ContainsKey(entity))
                    {
                        translatedEntity = EntityMap[entity];
                    }
                    TRMesh[] meshes = GetModelMeshes(translatedEntity);
                    if (meshes == null || meshes.Length == 0)
                    {
                        continue;
                    }

                    Dictionary<int, int> remapIndices = new Dictionary<int, int>();

                    TRColour[] palette = GetPalette8();
                    foreach (Color targetColour in source.EntityColourMap8[entity].Keys)
                    {
                        TRColour col = new TRColour
                        {
                            Red = (byte)(targetColour.R / 4),
                            Green = (byte)(targetColour.G / 4),
                            Blue = (byte)(targetColour.B / 4)
                        };
                        int matchedIndex = Array.FindIndex(palette, c => c.Red == col.Red && c.Green == col.Green && c.Blue == col.Blue);
                        if (matchedIndex == -1)
                        {
                            continue;
                        }

                        int sourceRectangle = source.EntityColourMap8[entity][targetColour];
                        int newColourIndex = ImportColour(source.Bitmap.GetPixel(segments[sourceRectangle].X, segments[sourceRectangle].Y));
                        remapIndices.Add(matchedIndex, newColourIndex);
                    }

                    foreach (TRMesh mesh in meshes)
                    {
                        foreach (TRFace4 f in mesh.ColouredRectangles)
                        {
                            if (remapIndices.ContainsKey(f.Texture))
                            {
                                f.Texture = (ushort)remapIndices[f.Texture];
                            }
                        }
                        foreach (TRFace3 f in mesh.ColouredTriangles)
                        {
                            if (remapIndices.ContainsKey(f.Texture))
                            {
                                f.Texture = (ushort)remapIndices[f.Texture];
                            }
                        }
                    }
                }
            }
        }

        public void DrawReplacements()
        {
            foreach (ReplacementTextureTarget replacement in ReplacementMapping)
            {
                Color search = GetBitmapGraphics(replacement.Search.Tile).Bitmap.GetPixel(replacement.Search.X, replacement.Search.Y);
                Color replace = GetBitmapGraphics(replacement.Replace.Tile).Bitmap.GetPixel(replacement.Replace.X, replacement.Replace.Y);
                // Scan each tile and replace colour Search with above
                foreach (int tile in replacement.ReplacementMap.Keys)
                {
                    BitmapGraphics graphics = GetBitmapGraphics(tile);
                    foreach (Rectangle rect in replacement.ReplacementMap[tile])
                    {
                        graphics.Replace(search, replace, rect);
                    }
                }
            }
        }

        private void GenerateSpriteSequenceTargets(StaticTextureSource<E> source)
        {
            if (!source.HasVariants)
            {
                throw new ArgumentException(string.Format("SpriteSequence {0} cannot be dynamically mapped without at least one source rectangle.", source.SpriteSequence));
            }

            List<TRSpriteSequence> spriteSequences = GetSpriteSequences().ToList();
            TRSpriteTexture[] spriteTextures = GetSpriteTextures();

            int spriteID = Convert.ToInt32(source.SpriteSequence);
            TRSpriteSequence sequence = spriteSequences.Find(s => s.SpriteID == spriteID);
            if (sequence == null)
            {
                return;
            }

            StaticMapping[source] = new List<StaticTextureTarget>();

            // An assumption is made here that each variant in the source will have the same number
            // of rectangles. We only want to define targets for the number of source rectangles, rather
            // than the total number of sprites.
            int numTargets = source.VariantMap[source.Variants[0]].Count;
            for (int j = 0; j < numTargets; j++)
            {
                TRSpriteTexture sprite = spriteTextures[sequence.Offset + j];
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
            int p16 = texture >> 8;
            if (remapIndices.ContainsKey(p16))
            {
                return (ushort)(remapIndices[p16] << 8 | (texture & 0xFF));
            }
            return texture;
        }

        private BitmapGraphics GetBitmapGraphics(int tile)
        {
            if (!_tileMap.ContainsKey(tile))
            {
                _tileMap.Add(tile, new BitmapGraphics(GetTile(tile)));
            }

            return _tileMap[tile];
        }

        public void Dispose()
        {
            CommitGraphics();
        }

        public virtual void CommitGraphics()
        {
            if (!_committed)
            {
                foreach (int tile in _tileMap.Keys)
                {
                    using (BitmapGraphics bmp = _tileMap[tile])
                    {
                        SetTile(tile, bmp.Bitmap);
                    }
                }
                _committed = true;
            }
        }
    }
}