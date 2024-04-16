using Newtonsoft.Json;
using System.Numerics;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Handlers;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR3AudioRandomizer : BaseTR3Randomizer
{
    private const int _maxSample = 413;
    private const int _defaultSecretTrack = 122;

    private AudioRandomizer _audioRandomizer;
    private TRAudioTrack _fixedSecretTrack;

    private List<TRSFXDefinition<TR3SoundDetails>> _soundEffects;
    private List<TRSFXGeneralCategory> _sfxCategories;
    private List<TR3ScriptedLevel> _uncontrolledLevels;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        LoadAudioData();
        ChooseUncontrolledLevels();

        foreach (TR3ScriptedLevel lvl in Levels)
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

    private void ChooseUncontrolledLevels()
    {
        TR3ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR3LevelNames.ASSAULT));
        ISet<TR3ScriptedLevel> exlusions = new HashSet<TR3ScriptedLevel> { assaultCourse };

        _uncontrolledLevels = Levels.RandomSelection(_generator, (int)Settings.UncontrolledSFXCount, exclusions: exlusions);
        if (Settings.AssaultCourseWireframe)
        {
            _uncontrolledLevels.Add(assaultCourse);
        }
    }

    public bool IsUncontrolledLevel(TR3ScriptedLevel level)
    {
        return _uncontrolledLevels.Contains(level);
    }

    private void RandomizeMusicTriggers(TR3CombinedLevel level)
    {
        FDControl floorData = new();
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
        _audioRandomizer.ResetFloorMap();
        foreach (TR3Room room in level.Rooms)
        {
            _audioRandomizer.RandomizeFloorTracks(room.Sectors, floorData, _generator, sectorIndex =>
            {
                // Get the midpoint of the tile in world coordinates
                return new Vector2
                (
                    TRConsts.Step2 + room.Info.X + sectorIndex / room.NumZSectors * TRConsts.Step4,
                    TRConsts.Step2 + room.Info.Z + sectorIndex % room.NumZSectors * TRConsts.Step4
                );
            });
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

            FDActionListItem musicAction = new()
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

        // Decide which sound effect categories we want to randomize.
        _sfxCategories = AudioRandomizer.GetSFXCategories(Settings);

        // Only load the SFX if we are changing at least one category
        if (_sfxCategories.Count > 0)
        {
            _soundEffects = JsonConvert.DeserializeObject<List<TRSFXDefinition<TR3SoundDetails>>>(ReadResource(@"TR3\Audio\sfx.json"));
        }
    }

    private void RandomizeSoundEffects(TR3CombinedLevel level)
    {
        if (_sfxCategories.Count == 0)
        {
            // We haven't selected any SFX categories to change.
            return;
        }

        if (IsUncontrolledLevel(level.Script))
        {
            // Choose a random sample for each current entry and replace the entire index list.
            HashSet<uint> indices = new();
            while (indices.Count < level.Data.SampleIndices.Count)
            {
                indices.Add((uint)_generator.Next(0, _maxSample + 1));
            }
            level.Data.SampleIndices.Clear();
            level.Data.SampleIndices.AddRange(indices);
        }
        else
        {
            // Run through the SoundMap for this level and get the SFX definition for each one.
            // Choose a new sound effect provided the definition is in a category we want to change.
            // Lara's SFX are not changed by default.
            for (int internalIndex = 0; internalIndex < level.Data.SoundMap.Length; internalIndex++)
            {
                TRSFXDefinition<TR3SoundDetails> definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (level.Data.SoundMap[internalIndex] == -1 || definition == null || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
                {
                    continue;
                }

                // The following allows choosing to keep humans making human noises, and animals animal noises.
                // Other humans can use Lara's SFX.
                Predicate<TRSFXDefinition<TR3SoundDetails>> pred;
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
                List<TRSFXDefinition<TR3SoundDetails>> otherDefinitions = _soundEffects.FindAll(pred);
                if (otherDefinitions.Count > 0)
                {
                    // Pick a new definition and try to import it into the level. This should only fail if
                    // the JSON is misconfigured e.g. missing sample indices. In that case, we just leave 
                    // the current sound effect as-is.
                    TRSFXDefinition<TR3SoundDetails> nextDefinition = otherDefinitions[_generator.Next(0, otherDefinitions.Count)];
                    short soundDetailsIndex = ImportSoundEffect(level.Data, definition, nextDefinition);
                    if (soundDetailsIndex != -1)
                    {
                        // Only change it if the import succeeded
                        level.Data.SoundMap[internalIndex] = soundDetailsIndex;
                    }
                }
            }
        }

        // Sample indices have to be in ascending order. Sort the level data only once.
        SoundUtilities.ResortSoundIndices(level.Data);
    }

    private static short ImportSoundEffect(TR3Level level, TRSFXDefinition<TR3SoundDetails> currentDefinition, TRSFXDefinition<TR3SoundDetails> newDefinition)
    {
        if (newDefinition.SampleIndices.Count == 0)
        {
            return -1;
        }

        List<TR3SoundDetails> levelSoundDetails = level.SoundDetails.ToList();

        uint minSample = newDefinition.SampleIndices.Min();
        if (level.SampleIndices.Contains(minSample))
        {
            return (short)levelSoundDetails.FindIndex(d => level.SampleIndices[d.Sample] == minSample);
        }

        ushort newSampleIndex = (ushort)level.SampleIndices.Count;
        level.SampleIndices.AddRange(newDefinition.SampleIndices);

        level.SoundDetails.Add(new TR3SoundDetails
        {
            Chance = currentDefinition.Details.Chance,
            Characteristics = newDefinition.Details.Characteristics,
            Pitch = newDefinition.Details.Pitch,
            Range = newDefinition.Details.Range,
            Sample = newSampleIndex,
            Volume = newDefinition.Details.Volume
        });

        return (short)(level.SoundDetails.Count - 1);
    }

    private void RandomizeWibble(TR3CombinedLevel level)
    {
        if (Settings.RandomizeWibble)
        {
            foreach (TR3SoundDetails details in level.Data.SoundDetails)
            {
                details.Wibble = true;
            }
        }
    }
}
