using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR1AudioAllocator : AudioRandomizer
{
    private const int _defaultSecretTrack = 13;

    private List<TR1SFXDefinition> _soundEffects;
    private TR1SFXDefinition _psUziDefinition;
    private List<TRSFXGeneralCategory> _sfxCategories, _persistentCategories;

    public TR1AudioAllocator(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks)
        : base(tracks) { }

    public List<TR1SFXDefinition> GetDefinitions()
        => _soundEffects;

    public List<TRSFXGeneralCategory> GetCategories()
        => _sfxCategories;

    public bool IsPersistent(TRSFXGeneralCategory category)
        => _persistentCategories.Contains(category);

    public TR1SFXDefinition GetPSUziDefinition()
        => _psUziDefinition;

    public void Initialise(IEnumerable<string> levelNames, string backupPath)
    {
        ChooseUncontrolledLevels(new(levelNames), TR1LevelNames.ASSAULT);

        _sfxCategories = GetSFXCategories(Settings);

        // SFX in these categories can potentially remain as they are
        _persistentCategories = new()
        {
            TRSFXGeneralCategory.StandardWeaponFiring,
            TRSFXGeneralCategory.Ricochet,
            TRSFXGeneralCategory.Flying,
            TRSFXGeneralCategory.Explosion,
        };

        _soundEffects = JsonConvert.DeserializeObject<List<TR1SFXDefinition>>(File.ReadAllText(@"Resources\TR1\Audio\sfx.json"));

        // We don't want to store all SFX WAV data in JSON, so instead we reference the source level
        // and extract the details from there using the same format for model transport.
        Dictionary<string, TR1Level> levels = new();
        TR1LevelControl reader = new();
        foreach (TR1SFXDefinition definition in _soundEffects)
        {
            if (!levels.ContainsKey(definition.SourceLevel))
            {
                levels[definition.SourceLevel] = reader.Read(Path.Combine(backupPath, definition.SourceLevel));
            }

            TR1Level level = levels[definition.SourceLevel];
            definition.SoundEffect = level.SoundEffects[definition.InternalIndex];
        }

        // PS uzis need some manual setup. Make a copy of the standard uzi definition
        // then replace the sound data from the external wav file.
        TR1Level caves = levels[TR1LevelNames.CAVES];
        _psUziDefinition = new TR1SFXDefinition
        {
            SoundEffect = caves.SoundEffects[TR1SFX.LaraUziFire]
        };
        _psUziDefinition.SoundEffect.Samples = new() { File.ReadAllBytes(@"Resources\TR1\Audio\ps_uzis.wav") };
    }

    public void RandomizeMusicTriggers(TR1Level level)
    {
        if (Settings.ChangeTriggerTracks)
        {
            RandomizeFloorTracks(level);
        }

        if (Settings.SeparateSecretTracks && !Settings.RandomizeSecrets)
        {
            RandomizeSecretTracks(level);
        }
    }

    public void RandomizeFloorTracks(TR1Level level)
    {
        ResetFloorMap();
        foreach (TR1Room room in level.Rooms.Where(r => !r.Flags.HasFlag(TRRoomFlag.Unused2)))
        {
            RandomizeFloorTracks(room, level.FloorData);
        }
    }

    private void RandomizeSecretTracks(TR1Level level)
    {
        int numSecrets = level.FloorData.GetActionItems(FDTrigAction.SecretFound)
            .Select(a => a.Parameter)
            .Distinct().Count();
        if (numSecrets == 0)
        {
            return;
        }

        List<TRAudioTrack> secretTracks = GetTracks(TRAudioCategory.Secret);

        for (int i = 0; i < numSecrets; i++)
        {
            TRAudioTrack secretTrack = secretTracks[Generator.Next(0, secretTracks.Count)];
            if (secretTrack.ID == _defaultSecretTrack)
            {
                continue;
            }

            FDActionItem musicAction = new()
            {
                Action = FDTrigAction.PlaySoundtrack,
                Parameter = (short)secretTrack.ID
            };

            List<FDTriggerEntry> triggers = level.FloorData.GetSecretTriggers(i);
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

    public void RandomizeWibble(TR1Level level)
    {
        if (Settings.RandomizeWibble)
        {
            // The engine does the actual randomization, we just tell it that every
            // sound effect should be included.
            foreach (var (_, effect) in level.SoundEffects)
            {
                effect.RandomizePitch = true;
            }
        }
    }
}
