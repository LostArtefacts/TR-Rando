using System;
using System.Collections.Generic;
using TRGE.Core;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Randomizers
{
    public class TR3TextureRandomizer : BaseTR3Randomizer, ITextureVariantHandler
    {
        private readonly Dictionary<AbstractTextureSource, string> _persistentVariants;
        private readonly object _drawLock;
        private TR3TextureDatabase _textureDatabase;
        private Dictionary<TextureCategory, bool> _textureOptions;

        internal bool NightModeOnly => !Settings.RandomizeTextures;
        internal TR3TextureMonitorBroker TextureMonitor { get; set; }

        public TR3TextureRandomizer()
        {
            _persistentVariants = new Dictionary<AbstractTextureSource, string>();
            _drawLock = new object();
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            using (_textureDatabase = new TR3TextureDatabase())
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

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                TextureMonitor<TR3Entities> monitor = TextureMonitor.GetMonitor(_levelInstance.Name);
                if (monitor != null && monitor.UseNightTextures)
                {
                    TR3TextureMapping mapping = GetMapping(_levelInstance);
                    using (TextureHolder<TR3Entities, TR3Level> holder = new TextureHolder<TR3Entities, TR3Level>(mapping, this))
                    {
                        foreach (AbstractTextureSource source in holder.Variants.Keys)
                        {
                            if (source.IsInCategory(TextureCategory.NightMode))
                            {
                                RedrawTargets(holder.Mapping, source, holder.Variants[source], _textureOptions);
                            }
                        }

                        DrawReplacements(holder.Mapping);
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
            foreach (TR3ScriptedLevel lvl in Levels)
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

        public string GetSourceVariant(AbstractTextureSource source)
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

        public void StoreVariant(AbstractTextureSource source, string variant)
        {
            if (Settings.PersistTextureVariants)
            {
                _persistentVariants[source] = variant;
            }
        }

        private TR3TextureMapping GetMapping(TR3CombinedLevel level)
        {
            lock (_drawLock)
            {
                return TR3TextureMapping.Get
                (
                    level.Data,
                    level.Name,
                    _textureDatabase,
                    TextureMonitor.GetLevelMapping(level.Name),
                    TextureMonitor.GetIgnoredEntities(level.Name),
                    TextureMonitor.GetEntityMap(level.Name)
                );
            }
        }

        private void RedrawTargets(AbstractTextureMapping<TR3Entities, TR3Level> mapping, AbstractTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
        {
            lock (_drawLock)
            {
                mapping.RedrawTargets(source, variant, options);
            }
        }

        private void DrawReplacements(AbstractTextureMapping<TR3Entities, TR3Level> mapping)
        {
            lock (_drawLock)
            {
                mapping.DrawReplacements();
            }
        }

        internal class TextureProcessor : AbstractProcessorThread<TR3TextureRandomizer>
        {
            private readonly Dictionary<TR3CombinedLevel, TextureHolder<TR3Entities, TR3Level>> _holders;
            private readonly TR3LandmarkImporter _landmarkImporter;

            internal override int LevelCount => _holders.Count;

            internal TextureProcessor(TR3TextureRandomizer outer)
                : base(outer)
            {
                _holders = new Dictionary<TR3CombinedLevel, TextureHolder<TR3Entities, TR3Level>>();
                _landmarkImporter = new TR3LandmarkImporter();
            }

            internal void AddLevel(TR3CombinedLevel level)
            {
                _holders.Add(level, null);
                if (level.HasCutScene)
                {
                    _holders.Add(level.CutSceneLevel, null);
                }
            }

            protected override void StartImpl()
            {
                List<TR3CombinedLevel> levels = new List<TR3CombinedLevel>(_holders.Keys);
                levels.Sort(delegate (TR3CombinedLevel lvl1, TR3CombinedLevel lvl2)
                {
                    return lvl1.IsCutScene && lvl1.ParentLevel == lvl2 ? 1 : 0;
                });

                foreach (TR3CombinedLevel level in levels)
                {
                    TR3TextureMapping mapping = _outer.GetMapping(level);
                    if (mapping != null)
                    {
                        TextureHolder<TR3Entities, TR3Level> parentHolder = null;
                        if (level.IsCutScene)
                        {
                            parentHolder = _holders[level.ParentLevel];
                        }
                        _holders[level] = new TextureHolder<TR3Entities, TR3Level>(mapping, _outer, parentHolder);
                    }
                    else
                    {
                        _holders.Remove(level);
                        _outer.TriggerProgress(2); // Skip processing
                    }
                }
            }

            protected override void ProcessImpl()
            {
                Dictionary<TextureCategory, bool> options = new Dictionary<TextureCategory, bool>(_outer._textureOptions);

                foreach (TR3CombinedLevel level in _holders.Keys)
                {
                    ProcessLevel(level, options);

                    int progress = level.IsCutScene ? 0 : 1;
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

            private void ProcessLevel(TR3CombinedLevel level, Dictionary<TextureCategory, bool> options)
            {
                TextureMonitor<TR3Entities> monitor = _outer.TextureMonitor.GetMonitor(level.Name);

                options[TextureCategory.NightMode] = monitor != null && monitor.UseNightTextures;
                options[TextureCategory.DayMode] = !options[TextureCategory.NightMode];

                using (TextureHolder<TR3Entities, TR3Level> holder = _holders[level])
                {
                    foreach (AbstractTextureSource source in holder.Variants.Keys)
                    {
                        _outer.RedrawTargets(holder.Mapping, source, holder.Variants[source], options);
                    }

                    // Add landmarks, but only if there is room available for them
                    if (holder.Mapping.LandmarkMapping.Count > 0)
                    {
                        _landmarkImporter.Import(level.Data, holder.Mapping, monitor != null && monitor.UseMirroring);
                    }

                    _outer.DrawReplacements(holder.Mapping);
                }
            }
        }
    }
}