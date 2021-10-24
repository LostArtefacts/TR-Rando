using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Utilities
{
    public abstract class AbstractMassTRTextureDeduplicator<E, L>
        where E : Enum
        where L : class
    {
        public abstract List<string> LevelNames { get; }

        public string LevelFileDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public bool OutputTileImages { get; set; }

        private readonly TRTextureDeduplicator<E> _deduplicator;
        private readonly Dictionary<string, AbstractTextureRemapGroup<E, L>> _levelRemap;
        private string _currentLevel;
        private int _currentSave;

        public AbstractMassTRTextureDeduplicator()
        {
            _deduplicator = new TRTextureDeduplicator<E>
            {
                UpdateGraphics = true
            };

            _levelRemap = new Dictionary<string, AbstractTextureRemapGroup<E, L>>();
        }


        public void Export()
        {
            Directory.CreateDirectory(OutputDirectory);
            _deduplicator.SegmentRemapped += SegmentRepositioned;

            try
            {
                foreach (string lvlName in LevelNames)
                {
                    _currentLevel = lvlName;
                    _currentSave = 0;
                    string lvlPath = Path.Combine(LevelFileDirectory, lvlName);
                    if (File.Exists(lvlPath))
                    {
                        ExportDuplicateLevelTextures(lvlPath);
                    }
                }

                if (_levelRemap.Count > 0)
                {
                    File.WriteAllText(Path.Combine(OutputDirectory, "Full-TextureRemap.json"), JsonConvert.SerializeObject(_levelRemap, Formatting.Indented));
                }
            }
            finally
            {
                _deduplicator.SegmentRemapped -= SegmentRepositioned;
            }
        }

        private void ExportDuplicateLevelTextures(string lvlPath)
        {
            using (AbstractTexturePacker<E, L> levelPacker = CreatePacker(ReadLevel(lvlPath)))
            {
                Dictionary<TexturedTile, List<TexturedTileSegment>> allTextures = new Dictionary<TexturedTile, List<TexturedTileSegment>>();
                foreach (TexturedTile tile in levelPacker.Tiles)
                {
                    allTextures[tile] = new List<TexturedTileSegment>(tile.Rectangles);
                }

                _deduplicator.SegmentMap = allTextures;
                _deduplicator.Deduplicate();

                if (_levelRemap.ContainsKey(_currentLevel))
                {
                    if (OutputTileImages)
                    {
                        string dir = Path.Combine(OutputDirectory, _currentLevel);
                        Directory.CreateDirectory(dir);
                        foreach (TexturedTile tile in allTextures.Keys)
                        {
                            tile.BitmapGraphics.Bitmap.Save(Path.Combine(dir, tile.Index.ToString() + ".png"), ImageFormat.Png);
                        }
                    }

                    File.WriteAllText(Path.Combine(OutputDirectory, _currentLevel + "-TextureRemap.json"), JsonConvert.SerializeObject(_levelRemap[_currentLevel], Formatting.Indented));
                }
            }
        }

        public void Import()
        {
            foreach (string lvlName in LevelNames)
            {
                string lvlPath = Path.Combine(LevelFileDirectory, lvlName);
                string mapPath = Path.Combine(OutputDirectory, lvlName + "-TextureRemap.json");
                if (File.Exists(lvlPath) && File.Exists(mapPath))
                {
                    _currentLevel = lvlName;
                    ImportDeduplication(lvlPath, mapPath);
                }
            }
        }

        private void ImportDeduplication(string lvlPath, string mapPath)
        {
            L level = ReadLevel(lvlPath);
            AbstractTRLevelTextureDeduplicator<E, L> deduplicator = CreateDeduplicator();
            deduplicator.Level = level;
            deduplicator.Deduplicate(mapPath);

            WriteLevel(level, Path.Combine(OutputDirectory, _currentLevel, _currentLevel));
        }

        private void SegmentRepositioned(object sender, TRTextureRemapEventArgs e)
        {
            if (!_levelRemap.ContainsKey(_currentLevel))
            {
                _levelRemap[_currentLevel] = CreateRemapGroup();
            }

            _levelRemap[_currentLevel].Remapping.Add(new TextureRemap
            {
                OriginalTile = e.OldTile.Index,
                OriginalIndex = e.OldFirstTextureIndex,
                OriginalBounds = e.OldBounds,
                NewBounds = e.NewBounds,
                NewTile = e.NewTile.Index,
                NewIndex = e.NewSegment.FirstTextureIndex,
                AdjustmentPoint = e.AdjustmentPoint
            });

            _currentSave += e.OldArea;

            Debug.WriteLine(_currentLevel + ": " + _currentSave);
        }

        protected abstract AbstractTextureRemapGroup<E, L> CreateRemapGroup();
        protected abstract AbstractTexturePacker<E, L> CreatePacker(L level);
        protected abstract AbstractTRLevelTextureDeduplicator<E, L> CreateDeduplicator();
        protected abstract L ReadLevel(string path);
        protected abstract void WriteLevel(L level, string path);
    }
}
