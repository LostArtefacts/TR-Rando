using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2SecretRandomizer : BaseTR2Randomizer
{
    public IMirrorControl Mirrorer { get; set; }
    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        TR2SecretAllocator allocator = new()
        {
            Generator = _generator,
            Settings = Settings,
            Mirrorer = Mirrorer,
            ItemFactory = ItemFactory,
        };

        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            if (Settings.DevelopmentMode)
            {
                allocator.PlaceAllSecrets(_levelInstance.Name, _levelInstance.Data);
            }
            else
            {
                List<Location> pickedLocations = allocator.RandomizeSecrets(_levelInstance.Name, _levelInstance.Data);
                AddDamageControl(_levelInstance, pickedLocations);
            }

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private static void AddDamageControl(TR2CombinedLevel level, List<Location> locations)
    {
        IEnumerable<Location> damagingLocations = locations.Where(l => l.RequiresDamage);
        if (!damagingLocations.Any())
        {
            return;
        }

        uint easyDamageCount = 0;
        uint hardDamageCount = 0;

        foreach (Location location in damagingLocations)
        {
            if (location.Difficulty == Difficulty.Hard)
            {
                hardDamageCount++;
            }
            else
            {
                easyDamageCount++;
            }
        }

        if (level.Script.RemovesWeapons)
        {
            hardDamageCount++;
        }

        if (level.Sequence < 2)
        {
            easyDamageCount++;
        }

        level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR2Type.LargeMed_S_P), hardDamageCount);
        level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR2Type.SmallMed_S_P), easyDamageCount);
    }
}
