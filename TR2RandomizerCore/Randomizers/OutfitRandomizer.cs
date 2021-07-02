using System;
using System.Collections.Generic;
using System.IO;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Processors;
using TR2RandomizerCore.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;

namespace TR2RandomizerCore.Randomizers
{
    public class OutfitRandomizer : RandomizerBase
    {
        internal bool PersistOutfits { get; set; }
        internal TexturePositionMonitorBroker TextureMonitor { get; set; }

        private TR2Entities _persistentOutfit;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            SetPersistentOutfit();

            List<OutfitProcessor> processors = new List<OutfitProcessor> { new OutfitProcessor(this) };
            int levelSplit = (int)(Levels.Count / _maxThreads);

            bool beginProcessing = true;
            foreach (TR23ScriptedLevel lvl in Levels)
            {
                if (processors[processors.Count - 1].LevelCount == levelSplit)
                {
                    // Kick start the last one
                    processors[processors.Count - 1].Start();
                    processors.Add(new OutfitProcessor(this));
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
                foreach (OutfitProcessor processor in processors)
                {
                    processor.Start();
                }

                foreach (OutfitProcessor processor in processors)
                {
                    processor.Join();
                }
            }

            if (_processingException != null)
            {
                throw _processingException;
            }
        }

        private void SetPersistentOutfit()
        {
            if (PersistOutfits)
            {
                List<TR2Entities> allLaras = TR2EntityUtilities.GetLaraTypes();
                _persistentOutfit = allLaras[_generator.Next(0, allLaras.Count)];
            }
        }

        internal class OutfitProcessor : AbstractProcessorThread<OutfitRandomizer>
        {
            // Each of these needs to be removed and replaced with the corresponding animation
            // models for the associated outfit.
            private static readonly List<TR2Entities> _laraRemovals = new List<TR2Entities>
            {
                TR2Entities.LaraPistolAnim_H,
                TR2Entities.LaraAutoAnim_H,
                TR2Entities.LaraUziAnim_H,
                TR2Entities.Lara
            };

            private readonly Dictionary<TR2CombinedLevel, List<TR2Entities>> _outfitAllocations;

            internal override int LevelCount => _outfitAllocations.Count;

            internal OutfitProcessor(OutfitRandomizer outer)
                : base(outer)
            {
                _outfitAllocations = new Dictionary<TR2CombinedLevel, List<TR2Entities>>();
            }

            internal void AddLevel(TR2CombinedLevel level)
            {
                _outfitAllocations.Add(level, new List<TR2Entities>());
            }

            protected override void StartImpl()
            {
                // Make the outfit selection outwith the processing thread to ensure consistent RNG.
                // We select all potential Laras including the default for the level as there are
                // only 4 to choose from (there is also Assault Course Lara, but when holstering pistols
                // her trousers disappear, so she is excluded for the time being...).
                List<TR2Entities> allLaras = TR2EntityUtilities.GetLaraTypes();
                List<TR2CombinedLevel> levels = new List<TR2CombinedLevel>(_outfitAllocations.Keys);

                foreach (TR2CombinedLevel level in levels)
                {
                    // Add the persistent outfit first, but we will populate the candidate
                    // list regardless in case a level cannot support this choice.
                    if (_outer.PersistOutfits)
                    {
                        _outfitAllocations[level].Add(_outer._persistentOutfit);
                    }

                    while (_outfitAllocations[level].Count < allLaras.Count)
                    {
                        TR2Entities nextLara = allLaras[_outer._generator.Next(0, allLaras.Count)];
                        if (!_outfitAllocations[level].Contains(nextLara))
                        {
                            _outfitAllocations[level].Add(nextLara);
                        }
                    }
                }
            }

            protected override void ProcessImpl()
            {
                foreach (TR2CombinedLevel level in _outfitAllocations.Keys)
                {
                    foreach (TR2Entities lara in _outfitAllocations[level])
                    {
                        if (Import(level, lara))
                        {
                            _outer.SaveLevel(level.Data, level.Name);
                            break;
                        }
                    }

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }

            private bool Import(TR2CombinedLevel level, TR2Entities lara)
            {
                List<TR2Entities> laraImport = new List<TR2Entities> { lara };
                TRModelImporter importer = new TRModelImporter
                {
                    Level = level.Data,
                    LevelName = level.Name,
                    ClearUnusedSprites = false,
                    EntitiesToImport = laraImport,
                    EntitiesToRemove = _laraRemovals,
                    TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, laraImport)
                };

                string remapPath = @"Resources\Textures\Deduplication\" + level.JsonID + "-TextureRemap.json";
                if (File.Exists(remapPath))
                {
                    importer.TextureRemapPath = remapPath;
                }

                try
                {
                    // Try to import the selected models into the level.
                    importer.Import();
                    return true;
                }
                catch (PackingException)
                {
                    // We need to reload the level to undo anything that may have changed.
                    _outer.ReloadLevelData(level);
                    // Tell the monitor to no longer track what we tried to import
                    _outer.TextureMonitor.ClearMonitor(level.Name, laraImport);
                    return false;
                }
            }
        }
    }
}