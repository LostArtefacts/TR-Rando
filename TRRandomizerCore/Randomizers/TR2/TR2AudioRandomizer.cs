using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Handlers;

namespace TRRandomizerCore.Randomizers
{
    public class TR2AudioRandomizer : BaseTR2Randomizer
    {
        private AudioRandomizer _audioRandomizer;

        private List<TRSFXDefinition<TRSoundDetails>> _soundEffects;
        private List<TRSFXGeneralCategory> _sfxCategories;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            LoadAudioData();

            foreach (TR2ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                RandomizeMusicTriggers(_levelInstance);

                RandomizeSoundEffects(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RandomizeMusicTriggers(TR2CombinedLevel level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            if (Settings.ChangeTriggerTracks)
            {
                RandomizeFloorTracks(level.Data, floorData);
            }

            if (Settings.SeparateSecretTracks)
            {
                RandomizeSecretTracks(level.Data, floorData);
            }

            floorData.WriteToLevel(level.Data);
        }

        private void RandomizeFloorTracks(TR2Level level, FDControl floorData)
        {
            foreach (TR2Room room in level.Rooms)
            {
                _audioRandomizer.RandomizeFloorTracks(room.SectorList, floorData, _generator);
            }
        }

        private void RandomizeSecretTracks(TR2Level level, FDControl floorData)
        {
            // Generate new triggers for secrets to allow different sounds for each one
            List<TRAudioTrack> secretTracks = _audioRandomizer.GetTracks(TRAudioCategory.Secret);
            Dictionary<int, TR2Entity> secrets = GetSecretItems(level);
            foreach (int entityIndex in secrets.Keys)
            {
                TR2Entity secret = secrets[entityIndex];
                TRRoomSector sector = FDUtilities.GetRoomSector(secret.X, secret.Y, secret.Z, secret.Room, level, floorData);
                if (sector.FDIndex == 0)
                {
                    // The secret is positioned on a tile that currently has no FD, so create it
                    floorData.CreateFloorData(sector);
                }

                List<FDEntry> entries = floorData.Entries[sector.FDIndex];
                FDTriggerEntry existingTriggerEntry = entries.Find(e => e is FDTriggerEntry) as FDTriggerEntry;
                bool existingEntityPickup = false;
                if (existingTriggerEntry != null)
                {
                    if (existingTriggerEntry.TrigType == FDTrigType.Pickup && existingTriggerEntry.TrigActionList[0].Parameter == entityIndex)
                    {
                        // GW gold secret (default location) already has a pickup trigger to spawn the
                        // TRex (or whatever enemy) so we'll just append to that item list here
                        existingEntityPickup = true;
                    }
                    else
                    {
                        // There is already a non-pickup trigger for this sector so while nothing is wrong with
                        // adding a pickup trigger, the game ignores it. So in this instance, the sound that
                        // plays will just be whatever is set in the script.
                        continue;
                    }
                }

                // Generate a new music action
                FDActionListItem musicAction = new FDActionListItem
                {
                    TrigAction = FDTrigAction.PlaySoundtrack,
                    Parameter = secretTracks[_generator.Next(0, secretTracks.Count)].ID
                };

                // For GW default gold, just append it
                if (existingEntityPickup)
                {
                    existingTriggerEntry.TrigActionList.Add(musicAction);
                }
                else
                {
                    entries.Add(new FDTriggerEntry
                    {
                        // The values here are replicated from Trigger#112 (in trview) in GW.
                        // The first action list must be the entity being picked up and so
                        // remaining action list items are actioned on pick up.
                        Setup = new FDSetup { Value = 1028 },
                        TrigSetup = new FDTrigSetup { Value = 15872 },
                        TrigActionList = new List<FDActionListItem>
                        {
                            new FDActionListItem
                            {
                                TrigAction = FDTrigAction.Object,
                                Parameter = (ushort)entityIndex
                            },
                            musicAction
                        }
                    });
                }
            }
        }

        private Dictionary<int, TR2Entity> GetSecretItems(TR2Level level)
        {
            Dictionary<int, TR2Entity> entities = new Dictionary<int, TR2Entity>();
            for (int i = 0; i < level.NumEntities; i++)
            {
                if (TR2EntityUtilities.IsSecretType((TR2Entities)level.Entities[i].TypeID))
                {
                    entities[i] = level.Entities[i];
                }
            }

            return entities;
        }

        private void LoadAudioData()
        {
            // Get the track data from audio_tracks.json. Loaded from TRGE as it sets the ambient tracks initially.
            _audioRandomizer = new AudioRandomizer(ScriptEditor.AudioProvider.GetCategorisedTracks());

            // Decide which sound effect categories we want to randomize.
            _sfxCategories = _audioRandomizer.GetSFXCategories(Settings);

            // Only load the SFX if we are changing at least one category
            if (_sfxCategories.Count > 0)
            {
                _soundEffects = JsonConvert.DeserializeObject<List<TRSFXDefinition<TRSoundDetails>>>(ReadResource(@"TR2\Audio\sfx.json"));
            }
        }

        private void RandomizeSoundEffects(TR2CombinedLevel level)
        {
            if (_sfxCategories.Count == 0)
            {
                // We haven't selected any SFX categories to change.
                return;
            }

            // Run through the SoundMap for this level and get the SFX definition for each one.
            // Choose a new sound effect provided the definition is in a category we want to change.
            // Lara's SFX are not changed by default.
            for (int internalIndex = 0; internalIndex < level.Data.SoundMap.Length; internalIndex++)
            {
                TRSFXDefinition<TRSoundDetails> definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (level.Data.SoundMap[internalIndex] == -1 || definition == null || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
                {
                    continue;
                }

                // The following allows choosing to keep humans making human noises, and animals animal noises.
                // Other humans can use Lara's SFX.
                Predicate<TRSFXDefinition<TRSoundDetails>> pred;
                if (Settings.LinkCreatureSFX && definition.Creature > TRSFXCreatureCategory.Lara)
                {
                    pred = sfx =>
                    {
                        return sfx.Categories.Contains(definition.PrimaryCategory) &&
                        sfx != definition &&
                        (
                            sfx.Creature == definition.Creature ||
                            (sfx.Creature == TRSFXCreatureCategory.Lara && definition.Creature == TRSFXCreatureCategory.Human)
                        );
                    };
                }
                else
                {
                    pred = sfx => sfx.Categories.Contains(definition.PrimaryCategory) && sfx != definition;
                }

                // Try to find definitions that match
                List<TRSFXDefinition<TRSoundDetails>> otherDefinitions = _soundEffects.FindAll(pred);
                if (otherDefinitions.Count > 0)
                {
                    // Pick a new definition and try to import it into the level. This should only fail if
                    // the JSON is misconfigured e.g. missing sample indices. In that case, we just leave 
                    // the current sound effect as-is.
                    TRSFXDefinition<TRSoundDetails> nextDefinition = otherDefinitions[_generator.Next(0, otherDefinitions.Count)];
                    short soundDetailsIndex = ImportSoundEffect(level.Data, nextDefinition);
                    if (soundDetailsIndex != -1)
                    {
                        // Only change it if the import succeeded
                        level.Data.SoundMap[internalIndex] = soundDetailsIndex;
                    }
                }
            }

            // Sample indices have to be in ascending order. Sort the level data only once.
            SoundUtilities.ResortSoundIndices(level.Data);
        }

        private short ImportSoundEffect(TR2Level level, TRSFXDefinition<TRSoundDetails> definition)
        {
            if (definition.SampleIndices.Count == 0)
            {
                return -1;
            }

            List<uint> levelSamples = level.SampleIndices.ToList();
            List<TRSoundDetails> levelSoundDetails = level.SoundDetails.ToList();

            uint minSample = definition.SampleIndices.Min();
            if (levelSamples.Contains(minSample))
            {
                // This index is already defined, so locate the TRSoundDetails that references it
                // and return its index.
                return (short)levelSoundDetails.FindIndex(d => levelSamples[d.Sample] == minSample);
            }

            // Otherwise, we need to import the samples, create a sound details object to 
            // point to the first sample, and then return the new index. Make sure the 
            // samples are sorted first.
            ushort newSampleIndex = (ushort)levelSamples.Count;
            List<uint> sortedSamples = new List<uint>(definition.SampleIndices);
            sortedSamples.Sort();
            levelSamples.AddRange(sortedSamples);

            level.SampleIndices = levelSamples.ToArray();
            level.NumSampleIndices = (uint)levelSamples.Count;

            levelSoundDetails.Add(new TRSoundDetails
            {
                Chance = definition.Details.Chance,
                Characteristics = definition.Details.Characteristics,
                Sample = newSampleIndex,
                Volume = definition.Details.Volume
            });

            level.SoundDetails = levelSoundDetails.ToArray();
            level.NumSoundDetails++;

            return (short)(level.NumSoundDetails - 1);
        }
    }
}