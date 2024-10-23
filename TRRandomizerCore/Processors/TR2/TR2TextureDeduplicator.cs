using Newtonsoft.Json;
using TRDataControl;
using TRGE.Core;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Utilities;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors;

internal class TR2TextureDeduplicator : TR2LevelProcessor
{
    public void Deduplicate()
    {
        List<DeduplicationProcessor> processors = new() { new(this) };
        int levelSplit = (int)(Levels.Count / _maxThreads);

        bool beginProcessing = true;
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            if (processors[^1].LevelCount == levelSplit)
            {
                // Kick start the last one
                processors[^1].Start();
                processors.Add(new(this));
            }

            processors[^1].AddLevel(LoadCombinedLevel(lvl));

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

        _processingException?.Throw();
    }

    internal class DeduplicationProcessor : AbstractProcessorThread<TR2TextureDeduplicator>
    {
        private readonly List<TR2CombinedLevel> _levels;

        internal override int LevelCount => _levels.Count;

        internal DeduplicationProcessor(TR2TextureDeduplicator outer)
            :base(outer)
        {
            _levels = new();
        }

        internal void AddLevel(TR2CombinedLevel level)
        {
            _levels.Add(level);
        }

        protected override void ProcessImpl()
        {
            foreach (TR2CombinedLevel level in _levels)
            {
                if (level.Is(TR2LevelNames.FLOATER) && level.Data.AnimatedTextures[0].Textures[0] == 1702)
                {
                    level.IsUKBox = true;
                }

                string dedupPath = _outer.GetResourcePath($"TR2/Textures/Deduplication/{level.JsonID}-TextureRemap.json");
                if (File.Exists(dedupPath))
                {
                    TR2TexturePacker levelPacker = new(level.Data);
                    Dictionary<TRTextile, List<TRTextileRegion>> allTextures = new();
                    foreach (TRTextile tile in levelPacker.Tiles)
                    {
                        allTextures[tile] = new List<TRTextileRegion>(tile.Rectangles);
                    }

                    TR2TextureRemapGroup remapGroup = JsonConvert.DeserializeObject<TR2TextureRemapGroup>(File.ReadAllText(dedupPath));
                    TRTextureDeduplicator<TR2Type> deduplicator = new()
                    {
                        UpdateGraphics = true,
                        SegmentMap = allTextures,
                        PrecompiledRemapping = remapGroup.Remapping,
                    };

                    deduplicator.Deduplicate();

                    levelPacker.AllowEmptyPacking = true;
                    levelPacker.Pack(true);

                    TR2TextureRemapper remapper = new(level.Data);
                    remapper.ResetUnusedTextures();

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
