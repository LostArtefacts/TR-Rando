using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR3AudioAllocator : AudioRandomizer
{
    private const int _defaultSecretTrack = 122;
    private const int _numSamples = 414;

    private List<TR3SFXDefinition> _soundEffects;
    private List<TRSFXGeneralCategory> _sfxCategories;

    public TR3AudioAllocator(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks)
        : base(tracks) { }

    public void Initialise(IEnumerable<string> levelNames, string backupPath)
    {
        ChooseUncontrolledLevels(new(levelNames), TR3LevelNames.ASSAULT);

        _sfxCategories = GetSFXCategories(Settings);
        if (_sfxCategories.Count > 0)
        {
            _soundEffects = JsonConvert.DeserializeObject<List<TR3SFXDefinition>>(File.ReadAllText(@"Resources\TR3\Audio\sfx.json"));

            Dictionary<string, TR3Level> levels = new();
            TR3LevelControl reader = new();
            foreach (TR3SFXDefinition definition in _soundEffects)
            {
                if (!levels.ContainsKey(definition.SourceLevel))
                {
                    levels[definition.SourceLevel] = reader.Read(Path.Combine(backupPath, definition.SourceLevel));
                }

                TR3Level level = levels[definition.SourceLevel];
                definition.SoundEffect = level.SoundEffects[definition.InternalIndex];
            }
        }
    }

    public void RandomizeMusicTriggers(TR3Level level)
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

    private void RandomizeFloorTracks(TR3Level level)
    {
        ResetFloorMap();
        foreach (TR3Room room in level.Rooms)
        {
            RandomizeFloorTracks(room, level.FloorData);
        }
    }

    private void RandomizeSecretTracks(TR3Level level)
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

    public void RandomizeSoundEffects(string levelName, TR3Level level)
    {
        if (_sfxCategories.Count == 0)
        {
            return;
        }

        if (IsUncontrolledLevel(levelName))
        {
            HashSet<uint> indices = new();
            foreach (TR3SoundEffect effect in level.SoundEffects.Values)
            {
                do
                {
                    effect.SampleID = (uint)Generator.Next(0, _numSamples + 1 - Math.Max(effect.SampleCount, 1));
                }
                while (!indices.Add(effect.SampleID));
            }
        }
        else
        {
            foreach (TR3SFX internalIndex in Enum.GetValues<TR3SFX>())
            {
                TR3SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (!level.SoundEffects.ContainsKey(internalIndex) || definition == null
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
                    TR3SFXDefinition nextDefinition = otherDefinitions[Generator.Next(0, otherDefinitions.Count)];
                    if (nextDefinition != definition)
                    {
                        level.SoundEffects[internalIndex] = nextDefinition.SoundEffect;
                    }
                }
            }
        }
    }

    public void RandomizePitch(TR3Level level)
    {
        if (Settings.RandomizeWibble)
        {
            foreach (var (_, effect) in level.SoundEffects)
            {
                effect.RandomizePitch = true;
            }
        }
    }
}
