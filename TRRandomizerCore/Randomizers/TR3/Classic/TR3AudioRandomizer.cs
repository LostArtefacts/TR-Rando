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

public class TR3AudioRandomizer : BaseTR3Randomizer
{
    private const int _defaultSecretTrack = 122;
    private const int _numSamples = 414;

    private AudioRandomizer _audioRandomizer;
    private TRAudioTrack _fixedSecretTrack;

    private List<TR3SFXDefinition> _soundEffects;
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
        if (Settings.UncontrolledSFXAssaultCourse)
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
        if (Settings.ChangeTriggerTracks)
        {
            RandomizeFloorTracks(level.Data);
        }

        // The secret sound is hardcoded in TR3 to track 122. The workaround for this is to 
        // always set the secret sound on the corresponding triggers regardless of whether
        // or not secret rando is enabled.
        RandomizeSecretTracks(level);
    }

    private void RandomizeFloorTracks(TR3Level level)
    {
        _audioRandomizer.ResetFloorMap();
        foreach (TR3Room room in level.Rooms)
        {
            _audioRandomizer.RandomizeFloorTracks(room.Sectors, level.FloorData, _generator, sectorIndex =>
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

    private void RandomizeSecretTracks(TR3CombinedLevel level)
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

            FDActionItem musicAction = new()
            {
                Action = FDTrigAction.PlaySoundtrack,
                Parameter = (short)secretTrack.ID
            };

            // Add a music action for each trigger defined for this secret.
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
        // Get the track data from audio_tracks.json. Loaded from TRGE as it sets the ambient tracks initially.
        _audioRandomizer = new AudioRandomizer(ScriptEditor.AudioProvider.GetCategorisedTracks());

        // Decide which sound effect categories we want to randomize.
        _sfxCategories = AudioRandomizer.GetSFXCategories(Settings);

        // Only load the SFX if we are changing at least one category
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

    private void RandomizeSoundEffects(TR3CombinedLevel level)
    {
        if (_sfxCategories.Count == 0)
        {
            // We haven't selected any SFX categories to change.
            return;
        }

        if (IsUncontrolledLevel(level.Script))
        {
            // Choose a random but unique pointer into MAIN.SFX for each sample.
            HashSet<uint> indices = new();
            foreach (TR3SoundEffect effect in level.Data.SoundEffects.Values)
            {
                do
                {
                    effect.SampleID = (uint)_generator.Next(0, _numSamples + 1 - Math.Max(effect.SampleCount, 1));
                }
                while (!indices.Add(effect.SampleID));
            }
        }
        else
        {
            // Run through the SoundMap for this level and get the SFX definition for each one.
            // Choose a new sound effect provided the definition is in a category we want to change.
            // Lara's SFX are not changed by default.
            foreach (TR3SFX internalIndex in Enum.GetValues<TR3SFX>())
            {
                TR3SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (!level.Data.SoundEffects.ContainsKey(internalIndex) || definition == null
                    || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
                {
                    continue;
                }

                // The following allows choosing to keep humans making human noises, and animals animal noises.
                // Other humans can use Lara's SFX.
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

                // Try to find definitions that match
                List<TR3SFXDefinition> otherDefinitions = _soundEffects.FindAll(pred);
                if (otherDefinitions.Count > 0)
                {
                    // Pick a new definition and try to import it into the level. This should only fail if
                    // the JSON is misconfigured e.g. missing sample indices. In that case, we just leave 
                    // the current sound effect as-is.
                    TR3SFXDefinition nextDefinition = otherDefinitions[_generator.Next(0, otherDefinitions.Count)];
                    if (nextDefinition != definition)
                    {
                        level.Data.SoundEffects[internalIndex] = nextDefinition.SoundEffect;
                    }
                }
            }
        }
    }

    private void RandomizeWibble(TR3CombinedLevel level)
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
