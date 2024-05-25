using TRDataControl.Environment;
using TRGE.Core;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class EnvironmentPicker
{
    private readonly Random _generator;
    private readonly RandomizerSettings _settings;
    private readonly TREdition _gfEdition;

    public EMOptions Options { get; set; }

    public EnvironmentPicker(Random generator, RandomizerSettings settings, TREdition gfEdition)
    {
        Options = new()
        {
            EnableHardMode = settings.HardEnvironmentMode,
            ExcludedTags = new()
        };

        _generator = generator;
        _settings = settings;
        _gfEdition = gfEdition;

        ResetTags();

        if (!_settings.AddReturnPaths)
        {
            Options.ExcludedTags.Add(EMTag.ReturnPath);
        }
        if (!_settings.FixOGBugs)
        {
            Options.ExcludedTags.Add(EMTag.GeneralBugFix);
        }
        if (!_settings.BlockShortcuts)
        {
            Options.ExcludedTags.Add(EMTag.ShortcutFix);
        }
        if (!_settings.RandomizeLadders)
        {
            Options.ExcludedTags.Add(EMTag.LadderChange);
        }
        if (!_settings.RandomizeWaterLevels)
        {
            Options.ExcludedTags.Add(EMTag.WaterChange);
        }
        if (!_settings.RandomizeSlotPositions)
        {
            Options.ExcludedTags.Add(EMTag.SlotChange);
        }
        if (!_settings.RandomizeTraps)
        {
            Options.ExcludedTags.Add(EMTag.TrapChange);
        }
        if (!_settings.RandomizeChallengeRooms)
        {
            Options.ExcludedTags.Add(EMTag.PuzzleRoom);
        }
        if (!_settings.RandomizeItems || !_settings.IncludeKeyItems)
        {
            Options.ExcludedTags.Add(EMTag.KeyItemFix);
        }
    }

    public void ResetTags()
    {
        // If we're using a community patch, exclude mods that only apply to non-community patch and vice-versa.
        // Same idea for classic/remastered only.
        Options.ExcludedTags = new()
        {
            _gfEdition.IsCommunityPatch
                ? EMTag.NonCommunityPatchOnly
                : EMTag.CommunityPatchOnly,
            _gfEdition.Remastered
                ? EMTag.ClassicOnly
                : EMTag.RemasteredOnly
        };
    }

    public List<EMEditorSet> GetRandomAny(EMEditorMapping mapping)
    {
        List<EMEditorSet> sets = new();        
        List<EMEditorSet> pool = Options.EnableHardMode 
            ? mapping.Any 
            : mapping.Any.FindAll(e => !e.IsHard);

        if (pool.Count > 0)
        {
            // Pick a random number of packs to apply, but at least 1
            sets = pool.RandomSelection(_generator, _generator.Next(1, pool.Count + 1));
        }

        return sets;
    }

    public EMEditorSet GetModToRun(List<EMEditorSet> modList)
    {
        if (Options.EnableHardMode)
        {
            // Anything goes.
            return modList[_generator.Next(0, modList.Count)];
        }

        if (modList.Any(e => !e.IsHard))
        {
            // Pick one that isn't classed as hard.
            EMEditorSet set;
            do
            {
                set = modList[_generator.Next(0, modList.Count)];
            }
            while (set.IsHard);

            return set;
        }

        // Everything in this set is hard but the user doesn't want that,
        // so nothing will be applied.
        return null;
    }
}
