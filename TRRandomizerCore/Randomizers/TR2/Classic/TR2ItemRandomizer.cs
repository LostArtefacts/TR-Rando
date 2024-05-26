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
            RandomizeVehicles();

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void RandomizeKeyItems()
    {
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            AdjustSeraphContinuity(_levelInstance);
            _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data, _levelInstance.Script.OriginalSequence);

            SaveLevelInstance();
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

    private void RandomizeVehicles()
    {
        // For now, we only add the boat if it has a location defined for a level. The skidoo is added
        // to levels that have MercSnowMobDriver present (see EnemyRandomizer) but we could alter this
        // to include it potentially in any level.
        // This perhaps needs better tracking, for example if every level has a vehicle location defined
        // we might not necessarily want to include it in every level.
        Dictionary<TR2Type, Location> vehicles = new();
        PopulateVehicleLocation(TR2Type.Boat, vehicles);
        if (_levelInstance.IsAssault)
        {
            // The assault course doesn't have enemies i.e. MercSnowMobDriver, so just add the skidoo too
            PopulateVehicleLocation(TR2Type.RedSnowmobile, vehicles);
        }

        int entityLimit = _levelInstance.GetMaximumEntityLimit();

        List<TR2Entity> boatToMove = _levelInstance.Data.Entities.FindAll(e => e.TypeID == TR2Type.Boat);

        if (vehicles.Count == 0 || vehicles.Count - boatToMove.Count + _levelInstance.Data.Entities.Count > entityLimit)
        {
            return;
        }

        TR2DataImporter importer = new()
        {
            Level = _levelInstance.Data,
            LevelName = _levelInstance.Name,
            ClearUnusedSprites = false,
            TypesToImport = new(vehicles.Keys),
            DataFolder = GetResourcePath(@"TR2\Objects"),
            TextureMonitor = TextureMonitor.CreateMonitor(_levelInstance.Name, vehicles.Keys.ToList())
        };


        try
        {
            importer.Import();

            // looping on boats and or skidoo
            foreach (TR2Type entity in vehicles.Keys)
            {
                if (_levelInstance.Data.Entities.Count == entityLimit)
                {
                    break;
                }

                Location location = vehicles[entity];

                if (entity == TR2Type.Boat)
                {
                    location = RoomWaterUtilities.MoveToTheSurface(location, _levelInstance.Data);
                }

                if (boatToMove.Count == 0)
                {
                    //Creation new entity
                    _levelInstance.Data.Entities.Add(new()
                    {
                        TypeID = entity,
                        Room = location.Room,
                        X = location.X,
                        Y = location.Y,
                        Z = location.Z,
                        Angle = location.Angle,
                        Flags = 0,
                        Intensity1 = -1,
                        Intensity2 = -1
                    });
                }
                else
                {
                    //I am in a level with 1 or 2 boat(s) to move
                    for (int i = 0; i < boatToMove.Count; i++)
                    {
                        if (i == 0) // for the first one i take the vehicle value
                        {
                            TR2Entity boat = boatToMove[i];

                            boat.Room = location.Room;
                            boat.X = location.X;
                            boat.Y = location.Y;
                            boat.Z = location.Z;
                            boat.Angle = location.Angle;
                            boat.Flags = 0;
                            boat.Intensity1 = -1;
                            boat.Intensity2 = -1;

                        }
                        else // I have to find another location that is different
                        {
                            Location location2ndBoat = vehicles[entity];
                            int checkCount = 0;
                            while (location2ndBoat.IsEquivalent(vehicles[entity]) && checkCount < 5)//compare locations in bottom of water ( authorize 5 round max in case there is only 1 valid location)
                            {
                                location2ndBoat = VehicleUtilities.GetRandomLocation(_levelInstance.Name, _levelInstance.Data, TR2Type.Boat, _generator, false);
                                checkCount++;
                            }

                            if (checkCount < 5)// If i actually found a different location I proceed (if not vanilla location it is) 
                            {
                                location2ndBoat = RoomWaterUtilities.MoveToTheSurface(location2ndBoat, _levelInstance.Data);

                                TR2Entity boat2 = boatToMove[i];

                                boat2.Room = location2ndBoat.Room;
                                boat2.X = location2ndBoat.X;
                                boat2.Y = location2ndBoat.Y;
                                boat2.Z = location2ndBoat.Z;
                                boat2.Angle = location2ndBoat.Angle;
                                boat2.Flags = 0;
                                boat2.Intensity1 = -1;
                                boat2.Intensity2 = -1;
                            }

                        }

                    }
                }
            }
        }
        catch (PackingException)
        {
            // Silently ignore failed imports for now as these are nice-to-have only
        }
    }

    /// <summary>
    /// Populate (or add in) the locationMap with a random location designed for the specific entity type in parameter
    /// </summary>
    /// <param name="entity">Type of the entity <see cref="TR2Type"/></param>
    /// <param name="locationMap">Dictionnary EntityType/location </param>
    private void PopulateVehicleLocation(TR2Type entity, Dictionary<TR2Type, Location> locationMap)
    {
        Location location = VehicleUtilities.GetRandomLocation(_levelInstance.Name, _levelInstance.Data, entity, _generator);
        if (location != null)
        {
            locationMap[entity] = location;
        }
    }
}
