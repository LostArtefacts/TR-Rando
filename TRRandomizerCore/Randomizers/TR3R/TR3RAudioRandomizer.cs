using Newtonsoft.Json;
using System.Numerics;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR3RAudioRandomizer : BaseTR3RRandomizer
{
    private const int _defaultSecretTrack = 122;

    private AudioRandomizer _audioRandomizer;

    private List<TR3SFXDefinition> _soundEffects;
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
        if (Settings.ChangeTriggerTracks)
        {
            RandomizeFloorTracks(level.Data);
        }

        if (Settings.SeparateSecretTracks)
        {
            RandomizeSecretTracks(level);
        }
    }

    private void RandomizeFloorTracks(TR3Level level)
    {
        _audioRandomizer.ResetFloorMap();
        foreach (TR3Room room in level.Rooms)
        {
            _audioRandomizer.RandomizeFloorTracks(room.Sectors, level.FloorData, _generator, sectorIndex =>
            {
                return new Vector2
                (
                    TRConsts.Step2 + room.Info.X + sectorIndex / room.NumZSectors * TRConsts.Step4,
                    TRConsts.Step2 + room.Info.Z + sectorIndex % room.NumZSectors * TRConsts.Step4
                );
            });
        }
    }

    private void RandomizeSecretTracks(TR3RCombinedLevel level)
    {
        List<TRAudioTrack> secretTracks = _audioRandomizer.GetTracks(TRAudioCategory.Secret);

        for (int i = 0; i < level.Script.NumSecrets; i++)
        {
            TRAudioTrack secretTrack = secretTracks[_generator.Next(0, secretTracks.Count)];
            if (secretTrack.ID == _defaultSecretTrack)
            {
                continue;
            }

            FDActionItem musicAction = new()
            {
                Action = FDTrigAction.PlaySoundtrack,
                Parameter = (short)secretTrack.ID
            };

            List<FDTriggerEntry> triggers = level.Data.FloorData.GetSecretTriggers(i);
            foreach (FDTriggerEntry trigger in triggers)
            {
                FDActionItem currentMusicAction = trigger.Actions.Find(a => a.Action == FDTrigAction.PlaySoundtrack);
                if (currentMusicAction == null)
                {
                    trigger.Actions.Add(musicAction);
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
            _soundEffects = JsonConvert.DeserializeObject<List<TR3SFXDefinition>>(ReadResource(@"TR3\Audio\sfx.json"));

            Dictionary<string, TR3Level> levels = new();
            TR3LevelControl reader = new();
            foreach (TR3SFXDefinition definition in _soundEffects)
            {
                if (!levels.ContainsKey(definition.SourceLevel))
                {
                    levels[definition.SourceLevel] = reader.Read(Path.Combine(BackupPath, definition.SourceLevel));
                }

                TR3Level level = levels[definition.SourceLevel];
                definition.SoundEffect = level.SoundEffects[definition.InternalIndex];
            }
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
            int maxSample = Enum.GetValues<TR3SFX>().Length;
            HashSet<uint> indices = new();
            foreach (var (_, effect) in level.Data.SoundEffects)
            {
                for (int i = 0; i < effect.Samples.Count; i++)
                {
                    uint sample;
                    do
                    {
                        sample = (uint)_generator.Next(0, maxSample + 1);
                    }
                    while (!indices.Add(sample));
                    effect.Samples[i] = sample;
                }
            }
        }
        else
        {
            foreach (TR3SFX internalIndex in Enum.GetValues<TR3SFX>())
            {
                TR3SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (!level.Data.SoundEffects.ContainsKey(internalIndex) || definition == null
                    || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
                {
                    continue;
                }

                Predicate<TR3SFXDefinition> pred;
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

                List<TR3SFXDefinition> otherDefinitions = _soundEffects.FindAll(pred);
                if (otherDefinitions.Count > 0)
                {
                    TR3SFXDefinition nextDefinition = otherDefinitions[_generator.Next(0, otherDefinitions.Count)];
                    if (nextDefinition != definition)
                    {
                        level.Data.SoundEffects[internalIndex] = nextDefinition.SoundEffect;
                    }
                }
            }
        }
    }

    private void RandomizeWibble(TR3RCombinedLevel level)
    {
        if (Settings.RandomizeWibble)
        {
            foreach (var (_, effect) in level.Data.SoundEffects)
            {
                effect.RandomizePitch = true;
            }
        }
    }
}
