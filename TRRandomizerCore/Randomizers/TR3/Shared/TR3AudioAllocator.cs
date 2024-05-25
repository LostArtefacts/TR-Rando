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

    public TR3AudioAllocator(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks)
        : base(tracks) { }

    protected override string GetAssaultName()
        => TR3LevelNames.ASSAULT;

    protected override void LoadData(string backupPath)
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

    public void RandomizeMusicTriggers(TR3Level level)
    {
        RandomizeFloorTracks(level.Rooms, level.FloorData);
        if (!Settings.RandomizeSecrets)
        {
            RandomizeSecretTracks(level.FloorData, _defaultSecretTrack);
        }
    }

    public void RandomizeSoundEffects(string levelName, TR3Level level)
    {
        if (Categories.Count == 0)
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
                    || definition.Creature == TRSFXCreatureCategory.Lara || !Categories.Contains(definition.PrimaryCategory))
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
}
