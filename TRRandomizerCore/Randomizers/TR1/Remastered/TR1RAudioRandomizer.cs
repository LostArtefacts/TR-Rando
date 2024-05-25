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

public class TR1RAudioRandomizer : BaseTR1RRandomizer
{
    private const int _defaultSecretTrack = 13;

    private AudioRandomizer _audioRandomizer;

    private List<TR1SFXDefinition> _soundEffects;
    private List<TRSFXGeneralCategory> _sfxCategories, _persistentCategories;
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

    private void LoadAudioData()
    {
        _audioRandomizer = new(ScriptEditor.AudioProvider.GetCategorisedTracks());
        _sfxCategories = AudioRandomizer.GetSFXCategories(Settings);
        _persistentCategories = new()
        {
            TRSFXGeneralCategory.StandardWeaponFiring,
            TRSFXGeneralCategory.Ricochet,
            TRSFXGeneralCategory.Flying,
            TRSFXGeneralCategory.Explosion
        };

        _soundEffects = JsonConvert.DeserializeObject<List<TR1SFXDefinition>>(ReadResource(@"TR1\Audio\sfx.json"));

        Dictionary<string, TR1Level> levels = new();
        TR1LevelControl reader = new();
        foreach (TR1SFXDefinition definition in _soundEffects)
        {
            if (!levels.ContainsKey(definition.SourceLevel))
            {
                levels[definition.SourceLevel] = reader.Read(Path.Combine(BackupPath, definition.SourceLevel));
            }

            TR1Level level = levels[definition.SourceLevel];
            definition.SoundEffect = level.SoundEffects[definition.InternalIndex];
        }
    }

    private void ChooseUncontrolledLevels()
    {
        TRRScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR1LevelNames.ASSAULT));
        HashSet<TRRScriptedLevel> exlusions = new() { assaultCourse };

        _uncontrolledLevels = Levels.RandomSelection(_generator, (int)Settings.UncontrolledSFXCount, exclusions: exlusions);
        if (Settings.UncontrolledSFXAssaultCourse)
        {
            _uncontrolledLevels.Add(assaultCourse);
        }
    }

    public bool IsUncontrolledLevel(TRRScriptedLevel level)
    {
        return _uncontrolledLevels.Contains(level);
    }

    private void RandomizeMusicTriggers(TR1RCombinedLevel level)
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

    private void RandomizeFloorTracks(TR1Level level)
    {
        _audioRandomizer.ResetFloorMap();
        foreach (TR1Room room in level.Rooms.Where(r => !r.Flags.HasFlag(TRRoomFlag.Unused2)))
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

    private void RandomizeSecretTracks(TR1RCombinedLevel level)
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

    private void RandomizeSoundEffects(TR1RCombinedLevel level)
    {
        if (_sfxCategories.Count == 0)
        {
            return;
        }

        // TR1R has hardcoded sample indices (into MAIN.SFX) so we use a different approach compared to OG, by
        // instead changing any animation commands and sound sources to point to different IDs. This does
        // however mean hardcoded SFX like Lara's guns remain unchanged.
        void ChangeCommands(IEnumerable<TRModel> models)
        {
            IEnumerable<TRSFXCommand> commands = models
                .SelectMany(m => m.Animations.SelectMany(a => a.Commands.Where(c => c is TRSFXCommand)))
                .Cast<TRSFXCommand>();
            foreach (TRSFXCommand command in commands)
            {
                TR1SFXDefinition definition = SelectSFXReplacement(level, (TR1SFX)command.SoundID);
                if (definition != null)
                {
                    command.SoundID = (short)definition.InternalIndex;
                    level.Data.SoundEffects[definition.InternalIndex] = definition.SoundEffect.Clone();
                }
            }
        }

        ChangeCommands(level.Data.Models.Values);
        ChangeCommands(level.PDPData.Values);

        foreach (TRSoundSource<TR1SFX> soundSource in level.Data.SoundSources)
        {
            TR1SFXDefinition definition = SelectSFXReplacement(level, soundSource.ID);
            if (definition != null)
            {
                soundSource.ID = definition.InternalIndex;
                level.Data.SoundEffects[definition.InternalIndex] = definition.SoundEffect.Clone();
            }
        }
    }

    private TR1SFXDefinition SelectSFXReplacement(TR1RCombinedLevel level, TR1SFX currentSFX)
    {
        if (IsUncontrolledLevel(level.Script))
        {
            return _soundEffects[_generator.Next(0, _soundEffects.Count)];
        }

        TR1SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == currentSFX);
        if (definition == null
            || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
        {
            return null;
        }

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

        List<TR1SFXDefinition> otherDefinitions = _soundEffects.FindAll(pred);
        return otherDefinitions.Any()
            ? otherDefinitions[_generator.Next(0, otherDefinitions.Count)]
            : null;
    }

    private void RandomizeWibble(TR1RCombinedLevel level)
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
