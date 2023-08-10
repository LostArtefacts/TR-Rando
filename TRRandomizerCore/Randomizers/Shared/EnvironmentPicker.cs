using TREnvironmentEditor;
using TREnvironmentEditor.Helpers;
using TREnvironmentEditor.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class EnvironmentPicker
{
    public EMOptions Options { get; set; }
    public Random Generator { get; set; }

    public EnvironmentPicker(bool hardMode)
    {
        Options = new EMOptions
        {
            EnableHardMode = hardMode,
            ExcludedTags = new List<EMTag>()
        };
    }

    public void LoadTags(RandomizerSettings settings, bool isCommunityPatch)
    {
        List<EMTag> excludedTags = new List<EMTag>();
        if (!settings.RandomizeLadders)
        {
            excludedTags.Add(EMTag.LadderChange);
        }
        if (!settings.RandomizeWaterLevels)
        {
            excludedTags.Add(EMTag.WaterChange);
        }
        if (!settings.RandomizeSlotPositions)
        {
            excludedTags.Add(EMTag.SlotChange);
        }
        if (!settings.RandomizeTraps)
        {
            excludedTags.Add(EMTag.TrapChange);
        }
        if (!settings.RandomizeChallengeRooms)
        {
            excludedTags.Add(EMTag.PuzzleRoom);
        }

        // If we're using a community patch, exclude mods that
        // only apply to non-community patch and vice-versa.
        excludedTags.Add(isCommunityPatch 
            ? EMTag.NonCommunityPatchOnly 
            : EMTag.CommunityPatchOnly);

        Options.ExcludedTags = excludedTags;
    }

    public void ResetTags(bool isCommunityPatch)
    {
        Options.ExcludedTags = new List<EMTag>
        {
            isCommunityPatch
            ? EMTag.NonCommunityPatchOnly
            : EMTag.CommunityPatchOnly
        };
    }

    public List<EMEditorSet> GetRandomAny(EMEditorMapping mapping)
    {
        List<EMEditorSet> sets = new List<EMEditorSet>();
        
        List<EMEditorSet> pool = Options.EnableHardMode 
            ? mapping.Any 
            : mapping.Any.FindAll(e => !e.IsHard);

        if (pool.Count > 0)
        {
            // Pick a random number of packs to apply, but at least 1
            sets = pool.RandomSelection(Generator, Generator.Next(1, pool.Count + 1));
        }

        return sets;
    }

    public EMEditorSet GetModToRun(List<EMEditorSet> modList)
    {
        if (Options.EnableHardMode)
        {
            // Anything goes.
            return modList[Generator.Next(0, modList.Count)];
        }

        if (modList.Any(e => !e.IsHard))
        {
            // Pick one that isn't classed as hard.
            EMEditorSet set;
            do
            {
                set = modList[Generator.Next(0, modList.Count)];
            }
            while (set.IsHard);

            return set;
        }

        // Everything in this set is hard but the user doesn't want that,
        // so nothing will be applied.
        return null;
    }
}
