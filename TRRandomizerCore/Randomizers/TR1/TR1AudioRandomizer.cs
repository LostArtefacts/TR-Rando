using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TRFDControl;
using TRGE.Core;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRModelTransporter.Handlers;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers
{
    public class TR1AudioRandomizer : BaseTR1Randomizer
    {
        private static readonly short _sfxUziID = 43;

        private AudioRandomizer _audioRandomizer;

        private List<TR1SFXDefinition> _soundEffects;
        private TR1SFXDefinition _psUziDefinition;
        private List<TRSFXGeneralCategory> _sfxCategories, _persistentCategories;
        private List<TR1ScriptedLevel> _uncontrolledLevels;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            LoadAudioData();
            ChooseUncontrolledLevels();

            foreach (TR1ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                RandomizeMusicTriggers(_levelInstance);

                RandomizeSoundEffects(_levelInstance);

                RandomizeWibble(_levelInstance);
                
                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void LoadAudioData()
        {
            // Get the track data from audio_tracks.json. Loaded from TRGE as it sets the ambient tracks initially.
            _audioRandomizer = new AudioRandomizer(ScriptEditor.AudioProvider.GetCategorisedTracks());

            // Decide which sound effect categories we want to randomize.
            _sfxCategories = _audioRandomizer.GetSFXCategories(Settings);

            // SFX in these categories can potentially remain as they are
            _persistentCategories = new List<TRSFXGeneralCategory>
            {
                TRSFXGeneralCategory.StandardWeaponFiring,
                TRSFXGeneralCategory.Ricochet,
                TRSFXGeneralCategory.Flying
            };

            // Only load the SFX if we are changing at least one category
            if (_sfxCategories.Count > 0)
            {
                _soundEffects = JsonConvert.DeserializeObject<List<TR1SFXDefinition>>(ReadResource(@"TR1\Audio\sfx.json"));

                // We don't want to store all SFX WAV data in JSON, so instead we reference the source level
                // and extract the details from there using the same format for model transport.
                Dictionary<string, TRLevel> levels = new Dictionary<string, TRLevel>();
                TR1LevelReader reader = new TR1LevelReader();
                foreach (TR1SFXDefinition definition in _soundEffects)
                {
                    if (!levels.ContainsKey(definition.SourceLevel))
                    {
                        levels[definition.SourceLevel] = reader.ReadLevel(Path.Combine(BackupPath, definition.SourceLevel));
                    }

                    TRLevel level = levels[definition.SourceLevel];
                    definition.SoundData = SoundUtilities.BuildPackedSound(level.SoundMap, level.SoundDetails, level.SampleIndices, level.Samples, new short[] { definition.InternalIndex });
                }

                // PS uzis need some manual setup. Make a copy of the standard uzi definition
                // then replace the sound data from the external wav file.
                TRLevel caves = levels[TRLevelNames.CAVES];
                _psUziDefinition = new TR1SFXDefinition
                {
                    InternalIndex = -1,
                    SoundData = SoundUtilities.BuildPackedSound(caves.SoundMap, caves.SoundDetails, caves.SampleIndices, caves.Samples, new short[] { _sfxUziID })
                };
                uint sample = _psUziDefinition.SoundData.Samples.Keys.First();
                _psUziDefinition.SoundData.Samples[sample] = File.ReadAllBytes(GetResourcePath(@"TR1\Audio\ps_uzis.wav"));
            }
        }

        private void ChooseUncontrolledLevels()
        {
            TR1ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TRLevelNames.ASSAULT));
            ISet<TR1ScriptedLevel> exlusions = new HashSet<TR1ScriptedLevel> { assaultCourse };

            _uncontrolledLevels = Levels.RandomSelection(_generator, (int)Settings.UncontrolledSFXCount, exclusions: exlusions);
            if (Settings.AssaultCourseWireframe)
            {
                _uncontrolledLevels.Add(assaultCourse);
            }
        }

        public bool IsUncontrolledLevel(TR1ScriptedLevel level)
        {
            return _uncontrolledLevels.Contains(level);
        }

        private void RandomizeMusicTriggers(TR1CombinedLevel level)
        {
            if (Settings.ChangeTriggerTracks)
            {
                FDControl floorData = new FDControl();
                floorData.ParseFromLevel(level.Data);

                _audioRandomizer.ResetFloorMap();
                foreach (TRRoom room in level.Data.Rooms)
                {
                    _audioRandomizer.RandomizeFloorTracks(room.Sectors, floorData, _generator, sectorIndex =>
                    {
                        // Get the midpoint of the tile in world coordinates
                        return new Vector2
                        (
                            AudioRandomizer.HalfSectorSize + room.Info.X + sectorIndex / room.NumZSectors * AudioRandomizer.FullSectorSize,
                            AudioRandomizer.HalfSectorSize + room.Info.Z + sectorIndex % room.NumZSectors * AudioRandomizer.FullSectorSize
                        );
                    });
                }

                floorData.WriteToLevel(level.Data);
            }
        }

        private void RandomizeSoundEffects(TR1CombinedLevel level)
        {
            if (_sfxCategories.Count == 0)
            {
                // We haven't selected any SFX categories to change.
                return;
            }

            if (IsUncontrolledLevel(level.Script))
            {
                List<uint> newSampleIndices = new List<uint>();
                List<byte> newSamples = new List<byte>();
                ISet<string> usedSamples = new HashSet<string>();

                // Replace each sample but be sure to avoid duplicates
                for (int i = 0; i < level.Data.NumSampleIndices; i++)
                {
                    byte[] sample;
                    string id;
                    do
                    {
                        TR1SFXDefinition definition = _soundEffects[_generator.Next(0, _soundEffects.Count)];
                        List<byte[]> samples = definition.SoundData.Samples.Values.ToList();
                        int sampleIndex = _generator.Next(0, samples.Count);
                        sample = samples[sampleIndex];

                        id = definition.InternalIndex + "_" + sampleIndex;
                    }
                    while (!usedSamples.Add(id));

                    newSampleIndices.Add((uint)newSamples.Count);
                    newSamples.AddRange(sample);
                }

                level.Data.SampleIndices = newSampleIndices.ToArray();
                level.Data.Samples = newSamples.ToArray();
                level.Data.NumSamples = (uint)newSamples.Count;
            }
            else
            {
                for (int internalIndex = 0; internalIndex < level.Data.SoundMap.Length; internalIndex++)
                {
                    TR1SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                    if (level.Data.SoundMap[internalIndex] == -1 || definition == null || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
                    {
                        continue;
                    }

                    // The following allows choosing to keep humans making human noises, and animals animal noises.
                    // Other humans can use Lara's SFX.
                    Predicate<TR1SFXDefinition> pred;
                    if (Settings.LinkCreatureSFX && definition.Creature > TRSFXCreatureCategory.Lara)
                    {
                        pred = sfx =>
                        {
                            return sfx.Categories.Contains(definition.PrimaryCategory) &&
                            (sfx != definition || _persistentCategories.Contains(definition.PrimaryCategory)) &&
                            (
                                sfx.Creature == definition.Creature ||
                                (sfx.Creature == TRSFXCreatureCategory.Lara && definition.Creature == TRSFXCreatureCategory.Human)
                            );
                        };
                    }
                    else
                    {
                        pred = sfx => sfx.Categories.Contains(definition.PrimaryCategory) && (sfx != definition || _persistentCategories.Contains(definition.PrimaryCategory));
                    }

                    List<TR1SFXDefinition> otherDefinitions;
                    if (internalIndex == _sfxUziID && _generator.NextDouble() < 0.4)
                    {
                        // 2/5 chance of PS uzis replacing original uzis, but they won't be used for anything else
                        otherDefinitions = new List<TR1SFXDefinition> { _psUziDefinition };
                    }
                    else
                    {
                        // Try to find definitions that match
                        otherDefinitions = _soundEffects.FindAll(pred);
                    }

                    if (otherDefinitions.Count > 0)
                    {
                        // Pick a new definition and try to import it into the level. This should only fail if
                        // the JSON is misconfigured e.g. missing sample indices. In that case, we just leave 
                        // the current sound effect as-is.
                        TR1SFXDefinition nextDefinition = otherDefinitions[_generator.Next(0, otherDefinitions.Count)];
                        if (nextDefinition != definition)
                        {
                            short soundDetailsIndex = ImportSoundEffect(level.Data, nextDefinition);
                            if (soundDetailsIndex != -1)
                            {
                                // Only change it if the import succeeded
                                level.Data.SoundMap[internalIndex] = soundDetailsIndex;
                            }
                        }
                    }
                }
            }

            // Sample indices have to be in ascending order. Sort the level data only once.
            SoundUtilities.ResortSoundIndices(level.Data);
        }

        private short ImportSoundEffect(TRLevel level, TR1SFXDefinition definition)
        {
            if (definition.SoundData.SampleIndices.Count == 0)
            {
                return -1;
            }

            TRSoundDetails defDetails = definition.SoundData.SoundDetails.Values.First();
            TRSoundDetails newDetails;

            // This may result in duplicate WAV data if a sound definition is imported more than
            // once, but ResortSoundIndices will tidy the data such that only the required WAV
            // file is retained in the end.
            List<byte> levelSamples = level.Samples.ToList();
            List<uint> levelSampleIndices = level.SampleIndices.ToList();

            foreach (byte[] sample in definition.SoundData.Samples.Values)
            {
                levelSampleIndices.Add((uint)levelSamples.Count);
                levelSamples.AddRange(sample);
            }

            newDetails = new TRSoundDetails
            {
                Chance = defDetails.Chance,
                Characteristics = defDetails.Characteristics,
                Sample = (ushort)level.SampleIndices.Length,
                Volume = defDetails.Volume
            };

            level.Samples = levelSamples.ToArray();
            level.NumSamples = (uint)levelSamples.Count;

            level.SampleIndices = levelSampleIndices.ToArray();
            level.NumSampleIndices = (uint)levelSampleIndices.Count;

            List<TRSoundDetails> levelSoundDetails = level.SoundDetails.ToList();
            levelSoundDetails.Add(newDetails);
            level.SoundDetails = levelSoundDetails.ToArray();
            level.NumSoundDetails++;

            return (short)(level.NumSoundDetails - 1);
        }

        private void RandomizeWibble(TR1CombinedLevel level)
        {
            if (Settings.RandomizeWibble)
            {
                // The engine does the actual randomization, we just tell it that every
                // sound effect should be included.
                foreach (TRSoundDetails details in level.Data.SoundDetails)
                {
                    details.Wibble = true;
                }

                if (ScriptEditor.Edition.IsCommunityPatch)
                {
                    (ScriptEditor.Script as TR1Script).EnablePitchedSounds = true;
                    ScriptEditor.SaveScript();
                }
            }
        }
    }
}