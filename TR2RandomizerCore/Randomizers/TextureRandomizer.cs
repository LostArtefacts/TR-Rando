using System;
using System.Collections.Generic;
using System.Threading;
using TR2RandomizerCore.Helpers;
using TRGE.Core;
using TRLevelReader.Model;
using TRTexture16Importer.Textures;
using TRTexture16Importer.Textures.Source;

namespace TR2RandomizerCore.Randomizers
{
    public class TextureRandomizer : RandomizerBase
    {
        private const uint _maxThreads = 2;

        private readonly Dictionary<AbstractTextureSource, string> _persistentVariants;
        private readonly object _monitorLock, _drawLock, _writeLock;
        private TextureDatabase _textureDatabase;

        public bool PersistVariants { get; set; }

        public TextureRandomizer()
        {
            _persistentVariants = new Dictionary<AbstractTextureSource, string>();
            _monitorLock = new object();
            _drawLock = new object();
            _writeLock = new object();
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            List<TextureProcessor> processors = new List<TextureProcessor>();
            int levelSplit = (int)(Levels.Count / _maxThreads);
            foreach (TR23ScriptedLevel lvl in Levels)
            {
                if (SaveMonitor.IsCancelled) return;

                if (processors.Count == 0 || processors[processors.Count - 1].LevelCount == levelSplit)
                {
                    processors.Add(new TextureProcessor(this));
                }

                processors[processors.Count - 1].AddLevel(new TR2CombinedLevel
                {
                    LevelData = LoadLevel(lvl.LevelFileBaseName),
                    LevelScript = lvl
                });

                TriggerProgress();
            }

            using (_textureDatabase = new TextureDatabase())
            {
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

                if (PersistVariants)
                {
                    _persistentVariants[source] = variant;
                }
                return variant;
            }
        }

        private TextureLevelMapping GetMapping(TR2CombinedLevel level)
        {
            lock (_drawLock)
            {
                return TextureLevelMapping.Get(level.LevelData, level.LevelScript.LevelFileBaseName, _textureDatabase);
            }
        }

        private bool TriggerProgress()
        {
            lock (_monitorLock)
            {
                SaveMonitor.FireSaveStateChanged(1);
                return !SaveMonitor.IsCancelled;
            }
        }

        public new void SaveLevel(TR2Level level, string name)
        {
            lock (_writeLock)
            {
                base.SaveLevel(level, name);
            }
        }

        internal class TextureProcessor
        {
            private readonly Dictionary<TR2CombinedLevel, TextureHolder> _holders;
            private readonly TextureRandomizer _outer;
            private readonly Thread _thread;

            internal int LevelCount => _holders.Count;

            internal TextureProcessor(TextureRandomizer outer)
            {
                _holders = new Dictionary<TR2CombinedLevel, TextureHolder>();
                _outer = outer;
                _thread = new Thread(Process);
            }

            internal void AddLevel(TR2CombinedLevel level)
            {
                _holders.Add(level, null);
            }

            internal void Start()
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

                _thread.Start();
            }

            internal void Join()
            {
                _thread.Join();
            }

            private void Process()
            {
                foreach (TR2CombinedLevel level in _holders.Keys)
                {
                    using (TextureHolder holder = _holders[level])
                    {
                        foreach (AbstractTextureSource source in holder.Variants.Keys)
                        {
                            lock (_outer._drawLock)
                            {
                                holder.Mapping.RedrawTargets(source, holder.Variants[source]);
                            }
                        }
                    }

                    _outer.SaveLevel(level.LevelData, level.LevelScript.LevelFileBaseName);
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
                foreach (DynamicTextureSource source in Mapping.DynamicMapping.Keys)
                {
                    Variants.Add(source, outer.GetSourceVariant(source));
                }

                foreach (StaticTextureSource source in Mapping.StaticMapping.Keys)
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