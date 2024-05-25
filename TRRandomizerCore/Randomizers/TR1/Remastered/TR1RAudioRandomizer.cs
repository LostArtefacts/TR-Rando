using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR1RAudioRandomizer : BaseTR1RRandomizer
{
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

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            _allocator.RandomizeMusicTriggers(_levelInstance.Data);
            RandomizeSoundEffects(_levelInstance);
            _allocator.RandomizeWibble(_levelInstance.Data);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void RandomizeSoundEffects(TR1RCombinedLevel level)
    {
        List<TRSFXGeneralCategory> categories = _allocator.GetCategories();
        if (categories.Count == 0)
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
        List<TR1SFXDefinition> soundEffects = _allocator.GetDefinitions();
        List<TRSFXGeneralCategory> categories = _allocator.GetCategories();

        if (_allocator.IsUncontrolledLevel(level.Name))
        {
            return soundEffects[_generator.Next(0, soundEffects.Count)];
        }

        TR1SFXDefinition definition = soundEffects.Find(sfx => sfx.InternalIndex == currentSFX);
        if (definition == null
            || definition.Creature == TRSFXCreatureCategory.Lara || !categories.Contains(definition.PrimaryCategory))
        {
            return null;
        }

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

        List<TR1SFXDefinition> otherDefinitions = soundEffects.FindAll(pred);
        return otherDefinitions.Any()
            ? otherDefinitions[_generator.Next(0, otherDefinitions.Count)]
            : null;
    }
}
