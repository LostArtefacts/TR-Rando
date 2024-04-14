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

public class TR3RAudioRandomizer : BaseTR3RRandomizer
{
    private const int _maxSample = 413;
    private const int _defaultSecretTrack = 122;

    private AudioRandomizer _audioRandomizer;

    private List<TRSFXDefinition<TR3SoundDetails>> _soundEffects;
    private List<TRSFXGeneralCategory> _sfxCategories;
    private List<TRRScriptedLevel> _uncontrolledLevels;

    public override void Randomize(int seed)
    {
        _generator = new(seed);

        LoadAudioData();
        ChooseUncontrolledLevels();

        foreach (TRRScriptedLevel lvl in Levels)
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
        TRRScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR3LevelNames.ASSAULT));
        HashSet<TRRScriptedLevel> exlusions = new () { assaultCourse };

        _uncontrolledLevels = Levels.RandomSelection(_generator, (int)Settings.UncontrolledSFXCount, exclusions: exlusions);
        if (Settings.AssaultCourseWireframe)
        {
            _uncontrolledLevels.Add(assaultCourse);
        }
    }

    public bool IsUncontrolledLevel(TRRScriptedLevel level)
    {
        return _uncontrolledLevels.Contains(level);
    }

    private void RandomizeMusicTriggers(TR3RCombinedLevel level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        if (Settings.ChangeTriggerTracks)
        {
            RandomizeFloorTracks(level.Data, floorData);
        }

        if (Settings.SeparateSecretTracks)
        {
            RandomizeSecretTracks(level, floorData);
        }

        floorData.WriteToLevel(level.Data);
    }

    private void RandomizeFloorTracks(TR3Level level, FDControl floorData)
    {
        _audioRandomizer.ResetFloorMap();
        foreach (TR3Room room in level.Rooms)
        {
            _audioRandomizer.RandomizeFloorTracks(room.Sectors, floorData, _generator, sectorIndex =>
            {
                return new Vector2
                (
                    TRConsts.Step2 + room.Info.X + sectorIndex / room.NumZSectors * TRConsts.Step4,
                    TRConsts.Step2 + room.Info.Z + sectorIndex % room.NumZSectors * TRConsts.Step4
                );
            });
        }
    }

    private void RandomizeSecretTracks(TR3RCombinedLevel level, FDControl floorData)
    {
        List<TRAudioTrack> secretTracks = _audioRandomizer.GetTracks(TRAudioCategory.Secret);

        for (int i = 0; i < level.Script.NumSecrets; i++)
        {
            TRAudioTrack secretTrack = secretTracks[_generator.Next(0, secretTracks.Count)];
            if (secretTrack.ID == _defaultSecretTrack)
            {
                continue;
            }

            FDActionListItem musicAction = new()
            {
                TrigAction = FDTrigAction.PlaySoundtrack,
                Parameter = secretTrack.ID
            };

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
        _audioRandomizer = new AudioRandomizer(ScriptEditor.AudioProvider.GetCategorisedTracks());
        _sfxCategories = AudioRandomizer.GetSFXCategories(Settings);
        if (_sfxCategories.Count > 0)
        {
            _soundEffects = JsonConvert.DeserializeObject<List<TRSFXDefinition<TR3SoundDetails>>>(ReadResource(@"TR3\Audio\sfx.json"));
        }
    }

    private void RandomizeSoundEffects(TR3RCombinedLevel level)
    {
        if (_sfxCategories.Count == 0)
        {
            return;
        }

        if (IsUncontrolledLevel(level.Script))
        {
            HashSet<uint> indices = new();
            while (indices.Count < level.Data.NumSampleIndices)
            {
                indices.Add((uint)_generator.Next(0, _maxSample + 1));
            }
            level.Data.SampleIndices = indices.ToArray();
        }
        else
        {
            for (int internalIndex = 0; internalIndex < level.Data.SoundMap.Length; internalIndex++)
            {
                TRSFXDefinition<TR3SoundDetails> definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (level.Data.SoundMap[internalIndex] == -1 || definition == null || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
                {
                    continue;
                }

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

                List<TRSFXDefinition<TR3SoundDetails>> otherDefinitions = _soundEffects.FindAll(pred);
                if (otherDefinitions.Count > 0)
                {
                    TRSFXDefinition<TR3SoundDetails> nextDefinition = otherDefinitions[_generator.Next(0, otherDefinitions.Count)];
                    short soundDetailsIndex = ImportSoundEffect(level.Data, definition, nextDefinition);
                    if (soundDetailsIndex != -1)
                    {
                        level.Data.SoundMap[internalIndex] = soundDetailsIndex;
                    }
                }
            }
        }

        SoundUtilities.ResortSoundIndices(level.Data);
    }

    private static short ImportSoundEffect(TR3Level level, TRSFXDefinition<TR3SoundDetails> currentDefinition, TRSFXDefinition<TR3SoundDetails> newDefinition)
    {
        if (newDefinition.SampleIndices.Count == 0)
        {
            return -1;
        }

        List<uint> levelSamples = level.SampleIndices.ToList();
        List<TR3SoundDetails> levelSoundDetails = level.SoundDetails.ToList();

        uint minSample = newDefinition.SampleIndices.Min();
        if (levelSamples.Contains(minSample))
        {
            return (short)levelSoundDetails.FindIndex(d => levelSamples[d.Sample] == minSample);
        }

        ushort newSampleIndex = (ushort)levelSamples.Count;
        List<uint> sortedSamples = new(newDefinition.SampleIndices);
        sortedSamples.Sort();
        levelSamples.AddRange(sortedSamples);

        level.SampleIndices = levelSamples.ToArray();
        level.NumSampleIndices = (uint)levelSamples.Count;

        levelSoundDetails.Add(new TR3SoundDetails
        {
            Chance = currentDefinition.Details.Chance,
            Characteristics = newDefinition.Details.Characteristics,
            Pitch = newDefinition.Details.Pitch,
            Range = newDefinition.Details.Range,
            Sample = newSampleIndex,
            Volume = newDefinition.Details.Volume
        });

        level.SoundDetails = levelSoundDetails.ToArray();
        level.NumSoundDetails++;

        return (short)(level.NumSoundDetails - 1);
    }

    private void RandomizeWibble(TR3RCombinedLevel level)
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
