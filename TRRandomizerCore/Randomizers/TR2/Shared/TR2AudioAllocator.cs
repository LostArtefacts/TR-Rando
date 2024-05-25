using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR2AudioAllocator : AudioRandomizer
{
    private readonly int _numSamples;

    private List<TR2SFXDefinition> _soundEffects;

    public TR2AudioAllocator(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks, int numSamples)
        : base(tracks)
    {
        _numSamples = numSamples;
    }

    protected override string GetAssaultName()
        => TR2LevelNames.ASSAULT;

    protected override void LoadData(string backupPath)
    {
        _soundEffects = JsonConvert.DeserializeObject<List<TR2SFXDefinition>>(File.ReadAllText(@"Resources\TR2\Audio\sfx.json"));

        Dictionary<string, TR2Level> levels = new();
        TR2LevelControl reader = new();
        foreach (TR2SFXDefinition definition in _soundEffects)
        {
            if (!levels.ContainsKey(definition.SourceLevel))
            {
                levels[definition.SourceLevel] = reader.Read(Path.Combine(backupPath, definition.SourceLevel));
            }

            TR2Level level = levels[definition.SourceLevel];
            definition.SoundEffect = level.SoundEffects[definition.InternalIndex];
        }
    }

    public void RandomizeMusicTriggers(TR2Level level)
    {
        RandomizeFloorTracks(level.Rooms, level.FloorData);
        RandomizeSecretTracks(level);
    }

    public void RandomizeSecretTracks(TR2Level level)
    {
        if (!Settings.SeparateSecretTracks)
        {
            return;
        }

        List<TRAudioTrack> secretTracks = GetTracks(TRAudioCategory.Secret);
        Dictionary<int, TR2Entity> secrets = GetSecretItems(level);
        foreach (int entityIndex in secrets.Keys)
        {
            TR2Entity secret = secrets[entityIndex];
            TRRoomSector sector = level.GetRoomSector(secret);
            if (sector.FDIndex == 0)
            {
                level.FloorData.CreateFloorData(sector);
            }

            List<FDEntry> entries = level.FloorData[sector.FDIndex];
            FDTriggerEntry existingTriggerEntry = entries.Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            bool existingEntityPickup = false;
            if (existingTriggerEntry != null)
            {
                if (existingTriggerEntry.TrigType == FDTrigType.Pickup && existingTriggerEntry.Actions[0].Parameter == entityIndex)
                {
                    // GW gold secret (default location) already has a pickup trigger to spawn the
                    // TRex (or whatever enemy) so we'll just append to that item list here
                    existingEntityPickup = true;
                }
                else
                {
                    // There is already a non-pickup trigger for this sector so while nothing is wrong with
                    // adding a pickup trigger, the game ignores it. So in this instance, the sound that
                    // plays will just be whatever is set in the script.
                    continue;
                }
            }

            FDActionItem musicAction = new()
            {
                Action = FDTrigAction.PlaySoundtrack,
                Parameter = (short)secretTracks[Generator.Next(0, secretTracks.Count)].ID
            };

            if (existingEntityPickup)
            {
                existingTriggerEntry.Actions.Add(musicAction);
            }
            else
            {
                entries.Add(new FDTriggerEntry
                {
                    TrigType = FDTrigType.Pickup,
                    Actions = new()
                    {
                        new() { Parameter = (short)entityIndex },
                        musicAction
                    }
                });
            }
        }
    }

    private static Dictionary<int, TR2Entity> GetSecretItems(TR2Level level)
    {
        Dictionary<int, TR2Entity> entities = new();
        for (int i = 0; i < level.Entities.Count; i++)
        {
            if (TR2TypeUtilities.IsSecretType(level.Entities[i].TypeID))
            {
                entities[i] = level.Entities[i];
            }
        }

        return entities;
    }

    public void RandomizeSoundEffects(string levelName, TR2Level level)
    {
        if (Categories.Count == 0)
        {
            return;
        }

        if (IsUncontrolledLevel(levelName))
        {
            // Choose a random but unique pointer into MAIN.SFX for each sample.
            HashSet<uint> indices = new();
            foreach (TR2SoundEffect effect in level.SoundEffects.Values)
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
            foreach (TR2SFX internalIndex in Enum.GetValues<TR2SFX>())
            {
                TR2SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (!level.SoundEffects.ContainsKey(internalIndex) || definition == null
                    || definition.Creature == TRSFXCreatureCategory.Lara || !Categories.Contains(definition.PrimaryCategory))
                {
                    continue;
                }

                Predicate<TR2SFXDefinition> pred;
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

                List<TR2SFXDefinition> otherDefinitions = _soundEffects.FindAll(pred);
                if (otherDefinitions.Count > 0)
                {
                    TR2SFXDefinition nextDefinition = otherDefinitions[Generator.Next(0, otherDefinitions.Count)];
                    if (nextDefinition != definition)
                    {
                        level.SoundEffects[internalIndex] = nextDefinition.SoundEffect;
                    }
                }
            }
        }
    }
}
