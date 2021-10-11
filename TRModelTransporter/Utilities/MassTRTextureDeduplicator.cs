using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Utilities
{
    public class MassTRTextureDeduplicator
    {
        private readonly TR2LevelReader _reader;
        private readonly TRTextureDeduplicator _deduplicator;
        private readonly Dictionary<string, TextureRemapGroup> _levelRemap;
        private readonly string _levelFileDirectory, _outputDirectory;
        private readonly bool _outputTileImages;
        private string _currentLevel;
        private int _currentSave;

        public MassTRTextureDeduplicator(string levelFileDirectory, string outputDirectory, bool outputTileImages)
        {
            _reader = new TR2LevelReader();
            _deduplicator = new TRTextureDeduplicator
            {
                UpdateGraphics = true
            };

            _levelRemap = new Dictionary<string, TextureRemapGroup>();

            _levelFileDirectory = levelFileDirectory;
            _outputDirectory = outputDirectory;
            _outputTileImages = outputTileImages;
        }

        public void Export()
        {
            Directory.CreateDirectory(_outputDirectory);
            _deduplicator.SegmentRemapped += SegmentRepositioned;

            try
            {
                foreach (string lvlName in TR2LevelNames.AsList)
                {
                    _currentLevel = lvlName;
                    _currentSave = 0;
                    string lvlPath = Path.Combine(_levelFileDirectory, lvlName);
                    if (File.Exists(lvlPath))
                    {
                        ExportDuplicateLevelTextures(lvlPath);
                    }
                }

                if (_levelRemap.Count > 0)
                {
                    File.WriteAllText(Path.Combine(_outputDirectory, "Full-TextureRemap.json"), JsonConvert.SerializeObject(_levelRemap, Formatting.Indented));
                }
            }
            finally
            {
                _deduplicator.SegmentRemapped -= SegmentRepositioned;
            }
        }

        public void Import()
        {
            foreach (string lvlName in TR2LevelNames.AsList)
            {
                string lvlPath = Path.Combine(_levelFileDirectory, lvlName);
                string mapPath = Path.Combine(_outputDirectory, lvlName + "-TextureRemap.json");
                if (File.Exists(lvlPath) && File.Exists(mapPath))
                {
                    _currentLevel = lvlName;
                    ImportDeduplication(lvlPath, mapPath);
                }
            }
        }

        private void ExportDuplicateLevelTextures(string lvlPath)
        {
            TR2Level level = _reader.ReadLevel(lvlPath);
            using (TexturePacker levelPacker = new TexturePacker(level))
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
                    if (_outputTileImages)
                    {
                        string dir = Path.Combine(_outputDirectory, _currentLevel);
                        Directory.CreateDirectory(dir);
                        foreach (TexturedTile tile in allTextures.Keys)
                        {
                            tile.BitmapGraphics.Bitmap.Save(Path.Combine(dir, tile.Index.ToString() + ".png"), ImageFormat.Png);
                        }
                    }

                    File.WriteAllText(Path.Combine(_outputDirectory, _currentLevel + "-TextureRemap.json"), JsonConvert.SerializeObject(_levelRemap[_currentLevel], Formatting.Indented));
                }
            }
        }

        private void ImportDeduplication(string lvlPath, string mapPath)
        {
            TR2Level level = _reader.ReadLevel(lvlPath);
            new TRLevelTextureDeduplicator
            {
                Level = level
            }.Deduplicate(mapPath);

            new TR2LevelWriter().WriteLevelToFile(level, Path.Combine(_outputDirectory, _currentLevel, _currentLevel));
        }

        private void SegmentRepositioned(object sender, TRTextureRemapEventArgs e)
        {
            if (!_levelRemap.ContainsKey(_currentLevel))
            {
                _levelRemap[_currentLevel] = new TextureRemapGroup();
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

            Console.WriteLine(_currentLevel + ": " + _currentSave);
        }
    }
}