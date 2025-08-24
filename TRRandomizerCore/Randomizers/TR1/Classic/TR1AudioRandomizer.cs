using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR1AudioRandomizer : BaseTR1Randomizer
{
    private static readonly List<int> _speechTracks = Enumerable.Range(51, 6).ToList();
    private static readonly TR1SFX _sfxFirstSpeechID = TR1SFX.BaldySpeech;
    
    private const double _psUziChance = 0.4;

    private TR1AudioAllocator _allocator;

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _allocator = new(ScriptEditor.AudioProvider.GetCategorisedTracks())
        {
            Generator = _generator,
            Settings = Settings,
        };
        _allocator.Initialise(Levels.Select(l => l.LevelFileBaseName), BackupPath);

        foreach (TR1ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            _allocator.RandomizeMusicTriggers(_levelInstance.Data);
            RandomizeSoundEffects(_levelInstance);
            ImportSpeechSFX(_levelInstance.Data);
            _allocator.RandomizePitch(_levelInstance.Data.SoundEffects.Values);
            
            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }

        var script = ScriptEditor.Script as TR1Script;
        if (Settings.RandomizeWibble)
        {
            script.EnforceConfig("enable_pitched_sounds", true);
        }

        if (Settings.SeparateSecretTracks && Settings.SecretRewardMode == Secrets.TRSecretRewardMode.Stack)
        {
            script.EnforceConfig("fix_secrets_killing_music", false);
            script.EnforceConfig("fix_speeches_killing_music", false);
        }

        ScriptEditor.SaveScript();
    }

    private void RandomizeSoundEffects(TR1CombinedLevel level)
    {
        List<TR1SFXDefinition> soundEffects = _allocator.GetDefinitions();
        if (_allocator.Categories.Count == 0)
        {
            return;
        }

        if (_allocator.IsUncontrolledLevel(level.Name))
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
                        TR1SFXDefinition definition = soundEffects[_generator.Next(0, soundEffects.Count)];
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
                TR1SFXDefinition definition = soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (!level.Data.SoundEffects.ContainsKey(internalIndex) || definition == null 
                    || definition.Creature == TRSFXCreatureCategory.Lara || !_allocator.Categories.Contains(definition.PrimaryCategory))
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
                        (sfx != definition || _allocator.IsPersistent(definition.PrimaryCategory)) &&
                        (
                            sfx.Creature == definition.Creature ||
                            (sfx.Creature == TRSFXCreatureCategory.Lara && definition.Creature == TRSFXCreatureCategory.Human)
                        );
                    };
                }
                else
                {
                    pred = sfx => sfx.Categories.Contains(definition.PrimaryCategory) && (sfx != definition || _allocator.IsPersistent(definition.PrimaryCategory));
                }

                List<TR1SFXDefinition> otherDefinitions;
                if (internalIndex == TR1SFX.LaraUziFire && _generator.NextDouble() < _psUziChance)
                {
                    // 2/5 chance of PS uzis replacing original uzis, but they won't be used for anything else
                    otherDefinitions = new() { _allocator.GetPSUziDefinition() };
                }
                else
                {
                    otherDefinitions = soundEffects.FindAll(pred);
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

    private void ImportSpeechSFX(TR1Level level)
    {
        // TR1X can play enemy speeches as SFX to avoid killing the current track, so ensure that
        // the required data is in the level if any of these are used on the floor.
        List<ushort> usedSpeechTracks = [.. level.FloorData.GetActionItems(FDTrigAction.PlaySoundtrack)
            .Select(action => (ushort)action.Parameter)
            .Distinct()
            .Where(trackID => _speechTracks.Contains(trackID))];

        if (usedSpeechTracks.Count == 0)
        {
            return;
        }

        List<TR1SFXDefinition> soundEffects = _allocator.GetDefinitions();
        foreach (ushort trackID in usedSpeechTracks)
        {
            TR1SFX sfxID = (TR1SFX)((int)_sfxFirstSpeechID + trackID - _speechTracks.First());
            TR1SFXDefinition definition;
            if (level.SoundEffects.ContainsKey(sfxID)
                || (definition = soundEffects.Find(sfx => sfx.InternalIndex == sfxID)) == null)
            {
                continue;
            }

            level.SoundEffects[sfxID] = definition.SoundEffect;
        }
    }
}
