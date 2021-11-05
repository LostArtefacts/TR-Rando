using System;
using System.Collections.Generic;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers
{
    public class TR3AudioRandomizer : BaseTR3Randomizer
    {
        private const int _defaultSecretTrack = 122;

        private AudioRandomizer _audioRandomizer;
        private TRAudioTrack _fixedSecretTrack;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            LoadAudioData();

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                RandomizeMusicTriggers(_levelInstance);

                //RandomizeSoundEffects(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RandomizeMusicTriggers(TR3CombinedLevel level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            if (Settings.ChangeTriggerTracks)
            {
                RandomizeFloorTracks(level.Data, floorData);
            }

            // The secret sound is hardcoded in TR3 to track 122. The workaround for this is to 
            // always set the secret sound on the corresponding triggers regardless of whether
            // or not secret rando is enabled.
            RandomizeSecretTracks(level, floorData);

            floorData.WriteToLevel(level.Data);
        }

        private void RandomizeFloorTracks(TR3Level level, FDControl floorData)
        {
            foreach (TR3Room room in level.Rooms)
            {
                _audioRandomizer.RandomizeFloorTracks(room.Sectors, floorData, _generator);
            }
        }

        private void RandomizeSecretTracks(TR3CombinedLevel level, FDControl floorData)
        {
            if (level.Script.NumSecrets == 0)
            {
                return;
            }

            List<TRAudioTrack> secretTracks = _audioRandomizer.GetTracks(TRAudioCategory.Secret);

            // If we want the same secret sound throughout the game, select it now.
            if (!Settings.SeparateSecretTracks && _fixedSecretTrack == null)
            {
                _fixedSecretTrack = secretTracks[_generator.Next(0, secretTracks.Count)];
            }

            for (int i = 0; i < level.Script.NumSecrets; i++)
            {
                // Pick a track for this secret and prepare an action item
                TRAudioTrack secretTrack = _fixedSecretTrack ?? secretTracks[_generator.Next(0, secretTracks.Count)];
                if (secretTrack.ID == _defaultSecretTrack)
                {
                    // The game hardcodes this track, so there is no point in amending the triggers.
                    continue;
                }

                FDActionListItem musicAction = new FDActionListItem
                {
                    TrigAction = FDTrigAction.PlaySoundtrack,
                    Parameter = secretTrack.ID
                };

                // Add a music action for each trigger defined for this secret.
                List<FDTriggerEntry> triggers = FDUtilities.GetSecretTriggers(floorData, i);
                foreach (FDTriggerEntry trigger in triggers)
                {
                    FDActionListItem currentMusicAction = trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.PlaySoundtrack);
                    if (currentMusicAction == null)
                    {
                        trigger.TrigActionList.Add(musicAction);
                    }
                }
            }
        }

        private void LoadAudioData()
        {
            // Get the track data from audio_tracks.json. Loaded from TRGE as it sets the ambient tracks initially.
            _audioRandomizer = new AudioRandomizer(ScriptEditor.AudioProvider.GetCategorisedTracks());
        }
    }
}