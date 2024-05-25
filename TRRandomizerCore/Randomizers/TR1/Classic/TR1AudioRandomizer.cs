using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR1AudioRandomizer : BaseTR1Randomizer
{
    private static readonly List<int> _speechTracks = Enumerable.Range(51, 6).ToList();
    private static readonly TR1SFX _sfxFirstSpeechID = TR1SFX.BaldySpeech;
    private static readonly TR1SFX _sfxUziID = TR1SFX.LaraUziFire;

    private const double _psUziChance = 0.4;

    private AudioRandomizer _audioRandomizer;

    private List<TR1SFXDefinition> _soundEffects;
    private TR1SFXDefinition _psUziDefinition;
    private List<TRSFXGeneralCategory> _sfxCategories, _persistentCategories;

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        LoadAudioData();

        foreach (TR1ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            RandomizeMusicTriggers(_levelInstance);
            RandomizeSoundEffects(_levelInstance);
            ImportSpeechSFX(_levelInstance);
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
        _audioRandomizer = new(ScriptEditor.AudioProvider.GetCategorisedTracks())
        {
            Generator = _generator,
            Settings = Settings,
        };
        _audioRandomizer.ChooseUncontrolledLevels(new(Levels.Select(l => l.LevelFileBaseName)), TR1LevelNames.ASSAULT);

        // Decide which sound effect categories we want to randomize.
        _sfxCategories = AudioRandomizer.GetSFXCategories(Settings);

        // SFX in these categories can potentially remain as they are
        _persistentCategories = new()
        {
            TRSFXGeneralCategory.StandardWeaponFiring,
            TRSFXGeneralCategory.Ricochet,
            TRSFXGeneralCategory.Flying,
            TRSFXGeneralCategory.Explosion
        };

        _soundEffects = JsonConvert.DeserializeObject<List<TR1SFXDefinition>>(ReadResource(@"TR1\Audio\sfx.json"));

        // We don't want to store all SFX WAV data in JSON, so instead we reference the source level
        // and extract the details from there using the same format for model transport.
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

        // PS uzis need some manual setup. Make a copy of the standard uzi definition
        // then replace the sound data from the external wav file.
        TR1Level caves = levels[TR1LevelNames.CAVES];
        _psUziDefinition = new TR1SFXDefinition
        {
            SoundEffect = caves.SoundEffects[_sfxUziID]
        };
        _psUziDefinition.SoundEffect.Samples = new() { File.ReadAllBytes(GetResourcePath(@"TR1\Audio\ps_uzis.wav")) };
    }

    private void RandomizeMusicTriggers(TR1CombinedLevel level)
    {
        if (Settings.ChangeTriggerTracks)
        {
            _audioRandomizer.ResetFloorMap();
            foreach (TR1Room room in level.Data.Rooms.Where(r => !r.Flags.HasFlag(TRRoomFlag.Unused2)))
            {
                _audioRandomizer.RandomizeFloorTracks(room, level.Data.FloorData);
            }
        }
    }

    private void RandomizeSoundEffects(TR1CombinedLevel level)
    {
        if (_sfxCategories.Count == 0)
        {
            return;
        }

        if (_audioRandomizer.IsUncontrolledLevel(level.Name))
        {
            HashSet<string> usedSamples = new();

            // Replace each sample but be sure to avoid duplicates
            foreach (var (_, effect) in level.Data.SoundEffects)
            {
                for (int i = 0; i < effect.Samples.Count; i++)
                {
                    byte[] sample;
                    string id;
                    do
                    {
                        TR1SFXDefinition definition = _soundEffects[_generator.Next(0, _soundEffects.Count)];
                        int sampleIndex = _generator.Next(0, definition.SoundEffect.Samples.Count);
                        sample = definition.SoundEffect.Samples[sampleIndex];

                        id = definition.InternalIndex + "_" + sampleIndex;
                    }
                    while (!usedSamples.Add(id));

                    effect.Samples[i] = sample;
                }
            }
        }
        else
        {
            // Run through the SoundMap for this level and get the SFX definition for each one.
            // Choose a new sound effect provided the definition is in a category we want to change.
            // Lara's SFX are not changed by default.
            foreach (TR1SFX internalIndex in Enum.GetValues<TR1SFX>())
            {
                TR1SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (!level.Data.SoundEffects.ContainsKey(internalIndex) || definition == null 
                    || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
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
                if (internalIndex == _sfxUziID && _generator.NextDouble() < _psUziChance)
                {
                    // 2/5 chance of PS uzis replacing original uzis, but they won't be used for anything else
                    otherDefinitions = new() { _psUziDefinition };
                }
                else
                {
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
                        level.Data.SoundEffects[internalIndex] = nextDefinition.SoundEffect.Clone();
                        if (definition.PrimaryCategory == TRSFXGeneralCategory.StandardWeaponFiring
                            || definition.PrimaryCategory == TRSFXGeneralCategory.FastWeaponFiring)
                        {
                            level.Data.SoundEffects[internalIndex].Mode = TR1SFXMode.Restart;
                        }
                    }
                }
            }
        }
    }

    private void ImportSpeechSFX(TR1CombinedLevel level)
    {
        if (!(ScriptEditor as TR1ScriptEditor).FixSpeechesKillingMusic)
        {
            return;
        }

        // TR1X can play enemy speeches as SFX to avoid killing the current
        // track, so ensure that the required data is in the level if any
        // of these are used on the floor.
        List<ushort> usedSpeechTracks = level.Data.FloorData.GetActionItems(FDTrigAction.PlaySoundtrack)
            .Select(action => (ushort)action.Parameter)
            .Distinct()
            .Where(trackID => _speechTracks.Contains(trackID))
            .ToList();

        if (usedSpeechTracks.Count == 0)
        {
            return;
        }

        foreach (ushort trackID in usedSpeechTracks)
        {
            TR1SFX sfxID = (TR1SFX)((int)_sfxFirstSpeechID + trackID - _speechTracks.First());
            TR1SFXDefinition definition;
            if (level.Data.SoundEffects.ContainsKey(sfxID)
                || (definition = _soundEffects.Find(sfx => sfx.InternalIndex == sfxID)) == null)
            {
                continue;
            }

            level.Data.SoundEffects[sfxID] = definition.SoundEffect;
        }
    }

    private void RandomizeWibble(TR1CombinedLevel level)
    {
        if (Settings.RandomizeWibble)
        {
            // The engine does the actual randomization, we just tell it that every
            // sound effect should be included.
            foreach (var (_, effect) in level.Data.SoundEffects)
            {
                effect.RandomizePitch = true;
            }

            (ScriptEditor as TR1ScriptEditor).EnablePitchedSounds = true;
            ScriptEditor.SaveScript();
        }
    }
}
