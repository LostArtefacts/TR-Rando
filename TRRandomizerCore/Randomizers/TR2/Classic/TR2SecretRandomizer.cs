using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2SecretRandomizer : BaseTR2Randomizer
{
    private static readonly List<string> _textureFixLevels = new()
    {
        TR2LevelNames.FLOATER,
        TR2LevelNames.LAIR,
        TR2LevelNames.HOME
    };

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
            DefaultItemShade = -1,
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

            FixSecretTextures(_levelInstance.Name, _levelInstance.Data);

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

    private static void FixSecretTextures(string levelName, TR2Level level)
    {
        if (!_textureFixLevels.Contains(levelName))
        {
            return;
        }

        // Swap Stone and Jade textures - OG has them the wrong way around.
        (level.Sprites[TR2Type.JadeSecret_S_P].Textures, level.Sprites[TR2Type.StoneSecret_S_P].Textures)
            = (level.Sprites[TR2Type.StoneSecret_S_P].Textures, level.Sprites[TR2Type.JadeSecret_S_P].Textures);
    }
}
