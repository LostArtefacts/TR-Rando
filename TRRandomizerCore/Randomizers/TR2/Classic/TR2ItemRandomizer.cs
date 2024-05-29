using TRDataControl;
using TRGE.Core;
using TRGE.Core.Item.Enums;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2ItemRandomizer : BaseTR2Randomizer
{
    private static readonly Location _barkhangSeraphLocation = new()
    {
        X = 35328,
        Y = 768,
        Z = 17920,
        Room = 43
    };

    private TR2ItemAllocator _allocator;

    internal TR2TextureMonitorBroker TextureMonitor { get; set; }
    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _allocator = new()
        {
            Generator = _generator,
            Settings = Settings,
            ItemFactory = ItemFactory,
        };

        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script);
            RandomizeVehicles(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void FinalizeRandomization()
    {
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            if (Settings.ItemMode == ItemMode.Shuffled || Settings.IncludeKeyItems)
            {
                LoadLevelInstance(lvl);

                if (Settings.ItemMode == ItemMode.Shuffled)
                {
                    _allocator.ApplyItemSwaps(_levelInstance.Name, _levelInstance.Data.Entities);
                }
                else
                {
                    AdjustSeraphContinuity(_levelInstance);
                    _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.OriginalSequence);
                }

                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void RandomizeSprites()
    {
        // This remains separate as it must be performed following all other texture work due to
        // TR2 texture deduplication.
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            _allocator.RandomizeSprites(_levelInstance.Data.Sprites, TR2TypeUtilities.GetKeyItemTypes(), TR2TypeUtilities.GetSecretTypes());

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void AdjustSeraphContinuity(TR2CombinedLevel level)
    {
        if (!Settings.MaintainKeyContinuity)
        {
            return;
        }

        if (level.Is(TR2LevelNames.MONASTERY))
        {
            TR2ScriptedLevel theDeck = Levels.Find(l => l.Is(TR2LevelNames.DECK));
            if ((theDeck == null || theDeck.Sequence > level.Sequence)
                && ItemFactory.CanCreateItem(level.Name, level.Data.Entities))
            {
                // Place The Seraph inside Barkhang. This location determines the key item ID to
                // therefore allow it to be randomized.
                TR2Entity seraph = ItemFactory.CreateItem(level.Name, level.Data.Entities, _barkhangSeraphLocation);
                seraph.TypeID = TR2Type.Puzzle4_S_P;
                level.Script.RemoveStartInventoryItem(TR2Items.Puzzle4);
            }
        }
        else if (level.Is(TR2LevelNames.TIBET))
        {
            TR2ScriptedLevel deck = Levels.Find(l => l.Is(TR2LevelNames.DECK));
            TR2ScriptedLevel monastery = Levels.Find(l => l.Is(TR2LevelNames.MONASTERY));

            // Deck not present => Barkhang pickup only
            // Deck present but Barkhang absent => Deck pickup, carried to Tibet if sequence is after
            // Deck and Barkhang present => remove Seraph from Tibet unless it comes between these levels
            if (deck == null ||
               (monastery == null && level.Script.Sequence < deck.Sequence) ||
               (monastery != null && (level.Script.Sequence < deck.Sequence || level.Script.Sequence > monastery.Sequence)))
            {
                level.Script.RemoveStartInventoryItem(TR2Items.Puzzle4);
            }
        }
        else if (level.Is(TR2LevelNames.DECK))
        {
            TR2ScriptedLevel monastery = Levels.Find(l => l.Is(TR2LevelNames.MONASTERY));
            if (monastery != null && monastery.Sequence < level.Sequence)
            {
                // Anticlimactic regular item pickup to end the level
                TR2Entity seraph = level.Data.Entities.Find(e => e.TypeID == TR2Type.Puzzle4_S_P);
                if (seraph != null)
                {
                    List<TR2Type> stdItemTypes = TR2TypeUtilities.GetStandardPickupTypes();
                    seraph.TypeID = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                }
            }
        }
    }

    private void RandomizeVehicles(TR2CombinedLevel level)
    {
        Dictionary<TR2Type, Queue<Location>> vehicleLocations = new();
        if (VehicleUtilities.HasLocations(level.Name, TR2Type.Boat))
        {
            vehicleLocations[TR2Type.Boat] = new();
            int boatCount = Math.Max(1, level.Data.Entities.Count(e => e.TypeID == TR2Type.Boat));
            for (int i = 0; i < boatCount; i++)
            {
                vehicleLocations[TR2Type.Boat].Enqueue(VehicleUtilities.GetRandomLocation(level.Name, level.Data, TR2Type.Boat, _generator));
            }
        }

        if (level.IsAssault)
        {
            // Regular skidoo rando comes with enemy rando currently
            vehicleLocations[TR2Type.RedSnowmobile] = new();
            vehicleLocations[TR2Type.RedSnowmobile].Enqueue(VehicleUtilities.GetRandomLocation(level.Name, level.Data, TR2Type.RedSnowmobile, _generator));
        }

        if (vehicleLocations.Count == 0)
        {
            return;
        }

        try
        {
            TR2DataImporter importer = new()
            {
                Level = level.Data,
                LevelName = level.Name,
                TypesToImport = new(vehicleLocations.Keys),
                DataFolder = GetResourcePath(@"TR2\Objects"),
                TextureMonitor = TextureMonitor.CreateMonitor(level.Name, new(vehicleLocations.Keys))
            };
            importer.Import();
        }
        catch (PackingException)
        {
            // Silently ignore failed imports for now as these are nice-to-have only
            return;
        }

        foreach (var (type, locations) in vehicleLocations)
        {
            List<TR2Entity> existingVehicles = level.Data.Entities.FindAll(e => e.TypeID == type);
            while (existingVehicles.Count < locations.Count)
            {
                if (!ItemFactory.CanCreateItem(level.Name, level.Data.Entities))
                {
                    break;
                }

                TR2Entity vehicle = ItemFactory.CreateItem(level.Name, level.Data.Entities);
                vehicle.TypeID = type;
                existingVehicles.Add(vehicle);
            }

            foreach (TR2Entity vehicle in existingVehicles)
            {
                Location location = locations.Dequeue();
                if (type == TR2Type.Boat)
                {
                    location = RoomWaterUtilities.MoveToTheSurface(location, level.Data);
                }
                vehicle.SetLocation(location);
            }
        }
    }
}
