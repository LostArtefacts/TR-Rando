using System;
using System.Collections.Generic;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Processors;
using TR2RandomizerCore.Utilities;
using TRGE.Core;
using TRTexture16Importer.Textures;
using TRTexture16Importer.Textures.Source;

namespace TR2RandomizerCore.Randomizers
{
    public class TextureRandomizer : RandomizerBase
    {
        private readonly Dictionary<AbstractTextureSource, string> _persistentVariants;
        private readonly object _drawLock;
        private TextureDatabase _textureDatabase;

        internal bool PersistVariants { get; set; }
        internal TexturePositionMonitorBroker TextureMonitor { get; set; }

        public TextureRandomizer()
        {
            _persistentVariants = new Dictionary<AbstractTextureSource, string>();
            _drawLock = new object();
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            SetMessage("Randomizing textures - loading levels");
            
            using (_textureDatabase = new TextureDatabase())
            {
                List<TextureProcessor> processors = new List<TextureProcessor> { new TextureProcessor(this) };
                int levelSplit = (int)(Levels.Count / _maxThreads);
                
                bool beginProcessing = true;
                foreach (TR23ScriptedLevel lvl in Levels)
                {
                    if (processors[processors.Count - 1].LevelCount == levelSplit)
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
            }

            if (_processingException != null)
            {
                throw _processingException;
            }
        }

        private string GetSourceVariant(AbstractTextureSource source)
        {
            lock (_drawLock)
            {
                if (PersistVariants && _persistentVariants.ContainsKey(source))
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
            if (PersistVariants)
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

        private void RedrawTargets(TextureLevelMapping mapping, AbstractTextureSource source, string variant)
        {
            lock (_drawLock)
            {
                mapping.RedrawTargets(source, variant);
            }
        }

        internal class TextureProcessor : AbstractProcessorThread<TextureRandomizer>
        {
            private readonly Dictionary<TR2CombinedLevel, TextureHolder> _holders;

            internal override int LevelCount => _holders.Count;

            internal TextureProcessor(TextureRandomizer outer)
                :base(outer)
            {
                _holders = new Dictionary<TR2CombinedLevel, TextureHolder>();
            }

            internal void AddLevel(TR2CombinedLevel level)
            {
                _holders.Add(level, null);
            }

            protected override void StartImpl()
            {
                // Load the level mapping and variants outwith the processor thread
                // to ensure the RNG selected for each level/texture remains consistent
                // between randomization sessions.
                List<TR2CombinedLevel> levels = new List<TR2CombinedLevel>(_holders.Keys);
                foreach (TR2CombinedLevel level in levels)
                {
                    TextureLevelMapping mapping = _outer.GetMapping(level);
                    if (mapping != null)
                    {
                        _holders[level] = new TextureHolder(mapping, _outer);
                    }
                    else
                    {
                        _holders.Remove(level);
                    }
                }
            }

            protected override void ProcessImpl()
            {
                foreach (TR2CombinedLevel level in _holders.Keys)
                {
                    using (TextureHolder holder = _holders[level])
                    {
                        foreach (AbstractTextureSource source in holder.Variants.Keys)
                        {
                            _outer.RedrawTargets(holder.Mapping, source, holder.Variants[source]);
                        }
                    }

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }

                    _outer.SaveLevel(level);
                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }
        }

        internal class TextureHolder : IDisposable
        {
            internal TextureLevelMapping Mapping { get; private set; }
            internal Dictionary<AbstractTextureSource, string> Variants { get; private set; }

            internal TextureHolder(TextureLevelMapping mapping, TextureRandomizer outer)
            {
                Mapping = mapping;
                Variants = new Dictionary<AbstractTextureSource, string>();

                // Check first for any grouped sources
                List<TextureGrouping> groupingList = mapping.StaticGrouping;
                List<StaticTextureSource> handledSources = new List<StaticTextureSource>();
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

                foreach (StaticTextureSource source in Mapping.StaticMapping.Keys)
                {
                    // Only randomize sources that aren't already grouped and that actually have variants
                    if (source.HasVariants && !handledSources.Contains(source))
                    {
                        Variants.Add(source, outer.GetSourceVariant(source));
                    }
                }

                // Dynamic changes should be made after static (e.g. for overlays)
                foreach (DynamicTextureSource source in Mapping.DynamicMapping.Keys)
                {
                    Variants.Add(source, outer.GetSourceVariant(source));
                }
            }

            public void Dispose()
            {
                Mapping.Dispose();
            }
        }
    }
}