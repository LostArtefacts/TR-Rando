using System;
using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Utilities;
using TRGE.Core;
using TRTexture16Importer.Textures;
using TRTexture16Importer.Textures.Grouping;
using TRTexture16Importer.Textures.Source;

namespace TRRandomizerCore.Randomizers
{
    public class TR2TextureRandomizer : BaseTR2Randomizer
    {
        private readonly Dictionary<AbstractTextureSource, string> _persistentVariants;
        private readonly object _drawLock;
        private TextureDatabase _textureDatabase;
        private Dictionary<TextureCategory, bool> _textureOptions;

        internal bool NightModeOnly => !Settings.RandomizeTextures;
        internal TexturePositionMonitorBroker TextureMonitor { get; set; }

        public TR2TextureRandomizer()
        {
            _persistentVariants = new Dictionary<AbstractTextureSource, string>();
            _drawLock = new object();
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            using (_textureDatabase = new TextureDatabase())
            {
                if (NightModeOnly)
                {
                    RandomizeNightModeTextures();
                }
                else
                {
                    RandomizeAllTextures();
                }
            }
        }

        private void RandomizeNightModeTextures()
        {
            // This is called if global texture randomization is disabled, but night-mode randomization is selected.
            // The main idea is to replace the SkyBox in levels that are now set at night, but this is treated as a
            // texture category so potentially any other textures could also be targeted.
            _textureOptions = new Dictionary<TextureCategory, bool>();

            foreach (TR23ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                TexturePositionMonitor monitor = TextureMonitor.GetMonitor(_levelInstance.Name);
                if (monitor != null && monitor.UseNightTextures)
                {
                    TextureLevelMapping mapping = GetMapping(_levelInstance);
                    using (TextureHolder holder = new TextureHolder(mapping, this))
                    {
                        foreach (AbstractTextureSource source in holder.Variants.Keys)
                        {
                            if (source.IsInCategory(TextureCategory.NightMode))
                            {
                                RedrawTargets(holder.Mapping, source, holder.Variants[source], _textureOptions);
                            }
                        }
                    }

                    SaveLevelInstance();
                }

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RandomizeAllTextures()
        {
            // These options are used to switch on/off specific textures
            _textureOptions = new Dictionary<TextureCategory, bool>
            {
                [TextureCategory.KeyItem] = !Settings.RetainKeySpriteTextures,
                [TextureCategory.Secret] = !Settings.RetainSecretSpriteTextures
            };

            SetMessage("Randomizing textures - loading levels");

            List<TextureProcessor> processors = new List<TextureProcessor> { new TextureProcessor(this) };
            int levelSplit = (int)(Levels.Count / _maxThreads);

            bool beginProcessing = true;
            foreach (TR23ScriptedLevel lvl in Levels)
            {
                if (processors[processors.Count - 1].LevelCount >= levelSplit)
                {
                    // Kick start the last one
                    processors[processors.Count - 1].Start();
                    processors.Add(new TextureProcessor(this));
                }

                processors[processors.Count - 1].AddLevel(LoadCombinedLevel(lvl));

                if (!TriggerProgress())
                {
                    beginProcessing = false;
                    break;
                }
            }

            if (beginProcessing)
            {
                SetMessage("Randomizing textures - applying texture packs");
                foreach (TextureProcessor processor in processors)
                {
                    processor.Start();
                }

                foreach (TextureProcessor processor in processors)
                {
                    processor.Join();
                }
            }

            if (_processingException != null)
            {
                _processingException.Throw();
            }
        }

        private string GetSourceVariant(AbstractTextureSource source)
        {
            lock (_drawLock)
            {
                if (Settings.PersistTextureVariants && _persistentVariants.ContainsKey(source))
                {
                    return _persistentVariants[source];
                }

                string[] variants = source.Variants;
                string variant = variants[_generator.Next(0, variants.Length)];

                StoreVariant(source, variant);
                return variant;
            }
        }

        private void StoreVariant(AbstractTextureSource source, string variant)
        {
            if (Settings.PersistTextureVariants)
            {
                _persistentVariants[source] = variant;
            }
        }

        private TextureLevelMapping GetMapping(TR2CombinedLevel level)
        {
            lock (_drawLock)
            {
                return TextureLevelMapping.Get
                (
                    level.Data,
                    level.JsonID,
                    _textureDatabase,
                    TextureMonitor.GetLevelMapping(level.Name),
                    TextureMonitor.GetIgnoredEntities(level.Name)
                );
            }
        }

        private void RedrawTargets(TextureLevelMapping mapping, AbstractTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
        {
            lock (_drawLock)
            {
                mapping.RedrawTargets(source, variant, options);
            }
        }

        internal class TextureProcessor : AbstractProcessorThread<TR2TextureRandomizer>
        {
            private readonly Dictionary<TR2CombinedLevel, TextureHolder> _holders;
            private readonly LandmarkImporter _landmarkImporter;

            internal override int LevelCount => _holders.Count;

            internal TextureProcessor(TR2TextureRandomizer outer)
                :base(outer)
            {
                _holders = new Dictionary<TR2CombinedLevel, TextureHolder>();
                _landmarkImporter = new LandmarkImporter();
            }

            internal void AddLevel(TR2CombinedLevel level)
            {
                _holders.Add(level, null);
                if (level.HasCutScene)
                {
                    _holders.Add(level.CutSceneLevel, null);
                }
            }

            protected override void StartImpl()
            {
                // Load the level mapping and variants outwith the processor thread
                // to ensure the RNG selected for each level/texture remains consistent
                // between randomization sessions. Levels are sorted to guarantee cutscene
                // levels are processed after their parent levels, because these will inherit
                // the variants allocated there. We don't yet do forward lookup, for example
                // the Floater stone at the end of Xian might be purple, but Floater itself
                // Red.
                List<TR2CombinedLevel> levels = new List<TR2CombinedLevel>(_holders.Keys);
                levels.Sort(delegate (TR2CombinedLevel lvl1, TR2CombinedLevel lvl2)
                {
                    return lvl1.IsCutScene && lvl1.ParentLevel == lvl2 ? 1 : 0;
                });

                foreach (TR2CombinedLevel level in levels)
                {
                    TextureLevelMapping mapping = _outer.GetMapping(level);
                    if (mapping != null)
                    {
                        TextureHolder parentHolder = null;
                        if (level.IsCutScene)
                        {
                            parentHolder = _holders[level.ParentLevel];
                        }
                        _holders[level] = new TextureHolder(mapping, _outer, parentHolder);
                    }
                    else
                    {
                        _holders.Remove(level);
                    }
                }
            }

            protected override void ProcessImpl()
            {
                Dictionary<TextureCategory, bool> options = new Dictionary<TextureCategory, bool>(_outer._textureOptions);

                foreach (TR2CombinedLevel level in _holders.Keys)
                {
                    ProcessLevel(level, options);

                    int progress = level.IsCutScene ? 0 : 1; // This is a bit of a hack for the time being as the overall progress target isn't aware of cutscene levels
                    if (!_outer.TriggerProgress(progress))
                    {
                        break;
                    }

                    _outer.SaveLevel(level);
                    if (!_outer.TriggerProgress(progress))
                    {
                        break;
                    }
                }
            }

            private void ProcessLevel(TR2CombinedLevel level, Dictionary<TextureCategory, bool> options)
            {
                TexturePositionMonitor monitor = _outer.TextureMonitor.GetMonitor(level.Name);

                options[TextureCategory.NightMode] = monitor != null && monitor.UseNightTextures;
                options[TextureCategory.DayMode] = !options[TextureCategory.NightMode];

                using (TextureHolder holder = _holders[level])
                {
                    foreach (AbstractTextureSource source in holder.Variants.Keys)
                    {
                        _outer.RedrawTargets(holder.Mapping, source, holder.Variants[source], options);
                    }

                    // Add landmarks, but only if there is room available for them
                    if (holder.Mapping.LandmarkMapping.Count > 0)
                    {
                        _landmarkImporter.Import(level, holder.Mapping, monitor != null && monitor.UseMirroring);
                    }
                }
            }
        }

        internal class TextureHolder : IDisposable
        {
            internal TextureLevelMapping Mapping { get; private set; }
            internal Dictionary<AbstractTextureSource, string> Variants { get; private set; }

            internal TextureHolder(TextureLevelMapping mapping, TR2TextureRandomizer outer, TextureHolder parentHolder = null)
            {
                Mapping = mapping;
                Variants = new Dictionary<AbstractTextureSource, string>();
                
                // Check first for any grouped sources, but only if the parent holder is null
                // as regrouping is not currently possible.
                List<StaticTextureSource> handledSources = new List<StaticTextureSource>();
                if (parentHolder == null)
                {
                    List<TextureGrouping> groupingList = mapping.StaticGrouping;
                    foreach (TextureGrouping staticGrouping in groupingList)
                    {
                        // Choose a variant for the leader, then assign this to the followers if they support it
                        string variant = outer.GetSourceVariant(staticGrouping.Leader);
                        Variants.Add(staticGrouping.Leader, variant);
                        handledSources.Add(staticGrouping.Leader);

                        foreach (StaticTextureSource source in staticGrouping.Followers)
                        {
                            if (source.HasVariants)
                            {
                                // Are we enforcing a specific colour for this theme?
                                if (staticGrouping.ThemeAlternatives.ContainsKey(variant) && staticGrouping.ThemeAlternatives[variant].ContainsKey(source))
                                {
                                    Variants.Add(source, staticGrouping.ThemeAlternatives[variant][source]);
                                }
                                // Otherwise, does the grouped source have the same variant available?
                                else if (source.Variants.Contains(variant))
                                {
                                    Variants.Add(source, variant);
                                    // If persistent textures are being used, have outer store what has been assigned to this source.
                                    outer.StoreVariant(source, variant);
                                }
                                // Otherwise, just add another random value for now (we ignore single variant sources such as FL/DL Spooky theme)
                                else if (source.Variants.Length > 1)
                                {
                                    Variants.Add(source, outer.GetSourceVariant(source));
                                }

                                handledSources.Add(source);
                            }
                        }
                    }
                }
                else
                {
                    foreach (AbstractTextureSource source in parentHolder.Variants.Keys)
                    {
                        Variants[source] = parentHolder.Variants[source];
                    }
                }

                foreach (StaticTextureSource source in Mapping.StaticMapping.Keys)
                {
                    // Only randomize sources that aren't already grouped and that actually have variants, or if we have a master
                    // parent holder, only add it the source if it's not already defined.
                    if (source.HasVariants && ((parentHolder == null && !handledSources.Contains(source)) || (parentHolder != null && !Variants.ContainsKey(source))))
                    {
                        Variants.Add(source, outer.GetSourceVariant(source));
                    }
                }

                // Dynamic changes should be made after static (e.g. for overlays)
                foreach (DynamicTextureSource source in Mapping.DynamicMapping.Keys)
                {
                    if (!Variants.ContainsKey(source))
                    {
                        Variants.Add(source, outer.GetSourceVariant(source));
                    }
                }
            }

            public void Dispose()
            {
                Mapping.Dispose();
            }
        }
    }
}