using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Randomizers
{
    public class TR2TextureRandomizer : BaseTR2Randomizer, ITextureVariantHandler
    {
        private static readonly Color[] _wireframeColours = ColorUtilities.GetWireframeColours();

        private readonly Dictionary<AbstractTextureSource, string> _persistentVariants;
        private readonly Dictionary<string, WireframeData> _wireframeData;
        private readonly object _drawLock;
        private TR2TextureDatabase _textureDatabase;
        private Dictionary<TextureCategory, bool> _textureOptions;
        private List<TR2ScriptedLevel> _wireframeLevels;
        private List<TR2ScriptedLevel> _solidLaraLevels;
        private Color _persistentWireColour;

        internal bool NightModeOnly => !Settings.RandomizeTextures;
        internal TR2TextureMonitorBroker TextureMonitor { get; set; }

        public TR2TextureRandomizer()
        {
            _persistentVariants = new Dictionary<AbstractTextureSource, string>();
            _wireframeData = JsonConvert.DeserializeObject<Dictionary<string, WireframeData>>(ReadResource(@"TR2\Textures\wireframing.json"));
            _drawLock = new object();
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            using (_textureDatabase = new TR2TextureDatabase())
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

            foreach (TR2ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                TextureMonitor<TR2Entities> monitor = TextureMonitor.GetMonitor(_levelInstance.Name);
                if (monitor != null && monitor.UseNightTextures)
                {
                    TR2TextureMapping mapping = GetMapping(_levelInstance);
                    using (TextureHolder<TR2Entities, TR2Level> holder = new TextureHolder<TR2Entities, TR2Level>(mapping, this))
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
                [TextureCategory.Secret] = !Settings.RetainSecretSpriteTextures,
                [TextureCategory.LevelColours] = !Settings.RetainMainLevelTextures
            };

            SetMessage("Randomizing textures - loading levels");

            ChooseWireframeLevels();

            List<TextureProcessor> processors = new List<TextureProcessor> { new TextureProcessor(this) };
            int levelSplit = (int)(Levels.Count / _maxThreads);

            bool beginProcessing = true;
            foreach (TR2ScriptedLevel lvl in Levels)
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

        private void ChooseWireframeLevels()
        {
            TR2ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR2LevelNames.ASSAULT));
            ISet<TR2ScriptedLevel> exlusions = new HashSet<TR2ScriptedLevel> { assaultCourse };

            _wireframeLevels = Levels.RandomSelection(_generator, (int)Settings.WireframeLevelCount, exclusions: exlusions);
            if (Settings.AssaultCourseWireframe)
            {
                _wireframeLevels.Add(assaultCourse);
            }

            if (Settings.UseSolidLaraWireframing)
            {
                _solidLaraLevels = new List<TR2ScriptedLevel>(_wireframeLevels);
            }
            else
            {
                _solidLaraLevels = _wireframeLevels.RandomSelection(_generator, _generator.Next(Math.Min(1, _wireframeLevels.Count), _wireframeLevels.Count));
            }

            if (Settings.PersistTextureVariants)
            {
                _persistentWireColour = _wireframeColours[_generator.Next(0, _wireframeColours.Length)];
            }

            _wireframeData.Values.ToList().ForEach(d => d.HighlightLadders = Settings.UseWireframeLadders);
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

        private TR2TextureMapping GetMapping(TR2CombinedLevel level)
        {
            lock (_drawLock)
            {
                return TR2TextureMapping.Get
                (
                    level.Data,
                    level.JsonID,
                    _textureDatabase,
                    TextureMonitor.GetLevelMapping(level.Name),
                    TextureMonitor.GetIgnoredEntities(level.Name)
                );
            }
        }

        private void RedrawTargets(AbstractTextureMapping<TR2Entities, TR2Level> mapping, AbstractTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
        {
            lock (_drawLock)
            {
                mapping.RedrawTargets(source, variant, options);
            }
        }

        private void DrawReplacements(AbstractTextureMapping<TR2Entities, TR2Level> mapping)
        {
            lock (_drawLock)
            {
                mapping.DrawReplacements();
            }
        }

        private bool IsWireframeLevel(TR2CombinedLevel lvl)
        {
            return !NightModeOnly &&
                _wireframeData.ContainsKey(lvl.JsonID) &&
                (_wireframeLevels.Contains(lvl.Script) || (lvl.IsCutScene && _wireframeLevels.Contains(lvl.ParentLevel.Script)));
        }

        private bool IsSolidLaraLevel(TR2CombinedLevel lvl)
        {
            return IsWireframeLevel(lvl) &&
                (_solidLaraLevels.Contains(lvl.Script) || (lvl.IsCutScene && _solidLaraLevels.Contains(lvl.ParentLevel.Script)));
        }

        private WireframeData GetWireframeData(TR2CombinedLevel lvl)
        {
            return IsWireframeLevel(lvl) ? _wireframeData[lvl.JsonID] : null;
        }

        private Color GetWireframeVariant()
        {
            return Settings.PersistTextureVariants ?
                _persistentWireColour :
                _wireframeColours[_generator.Next(0, _wireframeColours.Length)];
        }

        internal class TextureProcessor : AbstractProcessorThread<TR2TextureRandomizer>
        {
            private readonly Dictionary<TR2CombinedLevel, TextureHolder<TR2Entities, TR2Level>> _holders;
            private readonly TR2LandmarkImporter _landmarkImporter;
            private readonly TR2Wireframer _wireframer;

            internal override int LevelCount => _holders.Count;

            internal TextureProcessor(TR2TextureRandomizer outer)
                :base(outer)
            {
                _holders = new Dictionary<TR2CombinedLevel, TextureHolder<TR2Entities, TR2Level>>();
                _landmarkImporter = new TR2LandmarkImporter();
                _wireframer = new TR2Wireframer();
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
                    TR2TextureMapping mapping = _outer.GetMapping(level);
                    if (mapping != null)
                    {
                        TextureHolder<TR2Entities, TR2Level> parentHolder = null;
                        if (level.IsCutScene)
                        {
                            parentHolder = _holders[level.ParentLevel];
                        }
                        _holders[level] = new TextureHolder<TR2Entities, TR2Level>(mapping, _outer, parentHolder);

                        if (_outer.IsWireframeLevel(level))
                        {
                            WireframeData data = _outer.GetWireframeData(level);
                            data.SolidEnemies = _outer.Settings.UseSolidEnemyWireframing;
                            if (level.IsCutScene)
                            {
                                WireframeData parentData = _outer.GetWireframeData(level.ParentLevel);
                                data.HighlightColour = parentData.HighlightColour;
                                data.SolidLara = parentData.SolidLara;
                            }
                            else
                            {
                                data.HighlightColour = _outer.GetWireframeVariant();
                                data.SolidLara = _outer.IsSolidLaraLevel(level);
                            }

                            if (_outer.Settings.UseDifferentWireframeColours)
                            {
                                foreach (TRModel model in level.Data.Models)
                                {
                                    data.ModelColours[model.ID] = _outer.GetWireframeVariant();
                                }

                                // Make sure the front and back of the dragon match
                                if (data.ModelColours.ContainsKey((uint)TR2Entities.DragonFront_H))
                                {
                                    data.ModelColours[(uint)TR2Entities.DragonBack_H] = data.ModelColours[(uint)TR2Entities.DragonFront_H];
                                }
                            }
                        }
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
                TextureMonitor<TR2Entities> monitor = _outer.TextureMonitor.GetMonitor(level.Name);
                bool isWireframe = _outer.IsWireframeLevel(level);

                options[TextureCategory.NightMode] = monitor != null && monitor.UseNightTextures;
                options[TextureCategory.DayMode] = !options[TextureCategory.NightMode];

                using (TextureHolder<TR2Entities, TR2Level> holder = _holders[level])
                {
                    foreach (AbstractTextureSource source in holder.Variants.Keys)
                    {
                        _outer.RedrawTargets(holder.Mapping, source, holder.Variants[source], options);
                    }

                    if (!isWireframe)
                    {
                        // Add landmarks, but only if there is room available for them
                        if (holder.Mapping.LandmarkMapping.Count > 0)
                        {
                            _landmarkImporter.Import(level.Data, holder.Mapping, monitor != null && monitor.UseMirroring);
                        }

                        _outer.DrawReplacements(holder.Mapping);
                    }
                }

                if (isWireframe)
                {
                    _wireframer.Apply(level.Data, _outer.GetWireframeData(level));
                }
            }
        }
    }
}