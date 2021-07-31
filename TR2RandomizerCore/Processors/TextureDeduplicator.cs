using System.Collections.Generic;
using System.IO;
using TR2RandomizerCore.Helpers;
using TRGE.Core;
using TRModelTransporter.Utilities;

namespace TR2RandomizerCore.Processors
{
    internal class TextureDeduplicator : LevelProcessor
    {
        public void Deduplicate()
        {
            List<DeduplicationProcessor> processors = new List<DeduplicationProcessor> { new DeduplicationProcessor(this) };
            int levelSplit = (int)(Levels.Count / _maxThreads);

            bool beginProcessing = true;
            foreach (TR23ScriptedLevel lvl in Levels)
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
                throw _processingException;
            }
        }

        internal class DeduplicationProcessor : AbstractProcessorThread<TextureDeduplicator>
        {
            private readonly List<TR2CombinedLevel> _levels;
            private readonly TRLevelTextureDeduplicator _deduplicator;

            internal override int LevelCount => _levels.Count;

            internal DeduplicationProcessor(TextureDeduplicator outer)
                :base(outer)
            {
                _levels = new List<TR2CombinedLevel>();
                _deduplicator = new TRLevelTextureDeduplicator();
            }

            internal void AddLevel(TR2CombinedLevel level)
            {
                _levels.Add(level);
            }

            protected override void ProcessImpl()
            {
                foreach (TR2CombinedLevel level in _levels)
                {
                    string dedupPath = @"Resources\Textures\Deduplication\" + level.JsonID + "-TextureRemap.json";
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