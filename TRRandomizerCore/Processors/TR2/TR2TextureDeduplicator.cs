using System.Collections.Generic;
using System.IO;
using TRRandomizerCore.Levels;
using TRGE.Core;
using TRModelTransporter.Utilities;
using TRLevelReader.Model.Enums;

namespace TRRandomizerCore.Processors
{
    internal class TR2TextureDeduplicator : TR2LevelProcessor
    {
        public void Deduplicate()
        {
            List<DeduplicationProcessor> processors = new List<DeduplicationProcessor> { new DeduplicationProcessor(this) };
            int levelSplit = (int)(Levels.Count / _maxThreads);

            bool beginProcessing = true;
            foreach (TR2ScriptedLevel lvl in Levels)
            {
                if (processors[processors.Count - 1].LevelCount == levelSplit)
                {
                    // Kick start the last one
                    processors[processors.Count - 1].Start();
                    processors.Add(new DeduplicationProcessor(this));
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
                foreach (DeduplicationProcessor processor in processors)
                {
                    processor.Start();
                }

                foreach (DeduplicationProcessor processor in processors)
                {
                    processor.Join();
                }
            }

            if (_processingException != null)
            {
                _processingException.Throw();
            }
        }

        internal class DeduplicationProcessor : AbstractProcessorThread<TR2TextureDeduplicator>
        {
            private readonly List<TR2CombinedLevel> _levels;
            private readonly TR2LevelTextureDeduplicator _deduplicator;

            internal override int LevelCount => _levels.Count;

            internal DeduplicationProcessor(TR2TextureDeduplicator outer)
                :base(outer)
            {
                _levels = new List<TR2CombinedLevel>();
                _deduplicator = new TR2LevelTextureDeduplicator();
            }

            internal void AddLevel(TR2CombinedLevel level)
            {
                _levels.Add(level);
            }

            protected override void ProcessImpl()
            {
                foreach (TR2CombinedLevel level in _levels)
                {
                    string dedupPath = _outer.GetResourcePath(@"TR2\Textures\Deduplication\" + level.JsonID + "-TextureRemap.json");
                    if (File.Exists(dedupPath))
                    {
                        _deduplicator.Level = level.Data;
                        _deduplicator.Deduplicate(dedupPath);

                        _outer.SaveLevel(level);
                    }

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }
        }
    }
}