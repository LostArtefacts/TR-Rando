using Newtonsoft.Json;
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

    internal TR2TextureMonitorBroker TextureMonitor { get; set; }
    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    // This replaces plane cargo index as TRGE may have randomized the weaponless level(s), but will also have injected pistols
    // into predefined locations. See FindUnarmedPistolsLocation below.
    private int _unarmedLevelPistolIndex;
    private readonly Dictionary<string, List<Location>> _excludedLocations;
    private readonly Dictionary<string, List<Location>> _pistolLocations;

    private readonly LocationPicker _picker;
    private ItemSpriteRandomizer<TR2Type> _spriteRandomizer;

    public TR2ItemRandomizer()
    {
        _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\invalid_item_locations.json"));
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\unarmed_locations.json"));
        _picker = new(GetResourcePath(@"TR2\Locations\routes.json"));
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);

        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            FindUnarmedPistolsLocation();

            _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, false), Settings, _generator);

            if (Settings.RandomizeItemPositions)
            {
                RandomizeItemLocations(_levelInstance);
            }

            if (Settings.RandomizeItemTypes)
            {
                RandomizeItemTypes(_levelInstance);
            }

            if (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
            {
                EnforceOneLimit(_levelInstance);
            }

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
            RandomizeKeyItems(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void RandomizeLevelsSprites()
    {
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            RandomizeSprites();

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private List<Location> GetItemLocationPool(TR2CombinedLevel level, bool keyItemMode)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(level.Name))
        {
            exclusions.AddRange(_excludedLocations[level.Name]);
        }

        foreach (TR2Entity entity in level.Data.Entities)
        {
            if (!TR2TypeUtilities.CanSharePickupSpace(entity.TypeID))
            {
                exclusions.Add(entity.GetFloorLocation(loc => level.Data.GetRoomSector(loc)));
            }
        }

        TR2LocationGenerator generator = new();
        return generator.Generate(level.Data, exclusions, keyItemMode);
    }

    private void RandomizeSprites()
    {
        // If the _spriteRandomizer doesn't exists it gets fed all the settings of the rando and Lists of the game once. 
        if (_spriteRandomizer == null)
        {
            _spriteRandomizer = new ItemSpriteRandomizer<TR2Type>
            {
                StandardItemTypes = TR2TypeUtilities.GetGunTypes().Concat(TR2TypeUtilities.GetAmmoTypes()).ToList(),
                KeyItemTypes = TR2TypeUtilities.GetKeyItemTypes(),
                SecretItemTypes = TR2TypeUtilities.GetSecretTypes(),
                RandomizeKeyItemSprites = Settings.RandomizeKeyItemSprites,
                RandomizeSecretSprites = Settings.RandomizeSecretSprites,
                Mode = Settings.SpriteRandoMode
            };
#if DEBUG
            _spriteRandomizer.TextureChanged += (object sender, SpriteEventArgs<TR2Type> e) =>
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1} => {2}", _levelInstance.Name, e.OldSprite, e.NewSprite));
            };
#endif
        }

        _spriteRandomizer.Sequences = _levelInstance.Data.Sprites;
        _spriteRandomizer.Randomize(_generator);
    }

    public void RandomizeItemLocations(TR2CombinedLevel level)
    {
        if (level.Is(TR2LevelNames.HOME) && (level.Script.RemovesWeapons || level.Script.RemovesAmmo))
        {
            return;
        }

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR2Entity entity = level.Data.Entities[i];
            if (!TR2TypeUtilities.IsStandardPickupType(entity.TypeID)
                || ItemFactory.IsItemLocked(level.Name, i)
                || i == _unarmedLevelPistolIndex)
            {
                continue;
            }

            _picker.RandomizePickupLocation(entity);
            entity.Intensity1 = entity.Intensity2 = -1;
        }
    }

    private void RandomizeKeyItems(TR2CombinedLevel level)
    {
        _picker.TriggerTestAction = location => LocationUtilities.HasAnyTrigger(location, level.Data);
        _picker.KeyItemTestAction = (location, hasPickupTrigger) => TestKeyItemLocation(location, hasPickupTrigger, level);
        _picker.RoomInfos = level.Data.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();

        _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, true), Settings, _generator);

        // The Seraph may be removed from The Deck and added to Barkhang. Do that first to allow
        // its location to be changed.
        AdjustSeraphContinuity(level);

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR2Entity entity = level.Data.Entities[i];
            if (!TR2TypeUtilities.IsKeyItemType(entity.TypeID)
                || ItemFactory.IsItemLocked(level.Name, i))
            {
                continue;
            }

            _picker.RandomizeKeyItemLocation(
                entity, LocationUtilities.HasPickupTriger(entity, i, level.Data),
                level.Script.OriginalSequence, level.Data.Rooms[entity.Room].Info);
            entity.Intensity1 = entity.Intensity2 = -1;
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

    private bool TestKeyItemLocation(Location location, bool hasPickupTrigger, TR2CombinedLevel level)
    {
        // Make sure if we're placing on the same tile as an enemy, that the
        // enemy can drop the item.
        TR2Entity enemy = level.Data.Entities
            .FindAll(e => TR2TypeUtilities.IsEnemyType(e.TypeID))
            .Find(e => e.GetLocation().IsEquivalent(location));

        return enemy == null || (Settings.AllowEnemyKeyDrops && !hasPickupTrigger && TR2TypeUtilities.CanDropPickups
        (
            TR2TypeUtilities.GetAliasForLevel(level.Name, enemy.TypeID), 
            Settings.RandomizeEnemies && !Settings.ProtectMonks,
            Settings.RandomizeEnemies && Settings.UnconditionalChickens
        ));
    }

    private void RandomizeItemTypes(TR2CombinedLevel level)
    {
        if (level.IsAssault
            || (level.Is(TR2LevelNames.HOME) && (level.Script.RemovesWeapons || level.Script.RemovesAmmo)))
        {
            return;
        }

        List<TR2Type> stdItemTypes = TR2TypeUtilities.GetStandardPickupTypes();

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR2Entity entity = level.Data.Entities[i];
            if (i == _unarmedLevelPistolIndex)
            {
                // Handled separately in RandomizeAmmo
                continue;
            }
            else if (stdItemTypes.Contains(entity.TypeID))
            {
                entity.TypeID = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
            }
        }
    }

    private void EnforceOneLimit(TR2CombinedLevel level)
    {
        HashSet<TR2Type> uniqueTypes = new();

        // Look for extra utility/ammo items and hide them
        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR2Entity entity = level.Data.Entities[i];
            if (TR2TypeUtilities.IsStandardPickupType(entity.TypeID)
                && !uniqueTypes.Add(entity.TypeID))
            {
                ItemUtilities.HideEntity(entity);
                ItemFactory.FreeItem(level.Name, i);
            }
        }
    }

    private void FindUnarmedPistolsLocation()
    {
        // #66 - checks were previously performed to clean locations from previous
        // randomization sessions to avoid item pollution. This is no longer required
        // as randomization is now always performed on the original level files.

        // #124 Default pistol locations are no longer limited to one per level.

        _unarmedLevelPistolIndex = -1;

        if (_levelInstance.Script.RemovesWeapons && _pistolLocations.ContainsKey(_levelInstance.Name))
        {
            int pistolIndex = _levelInstance.Data.Entities.FindIndex(e => e.TypeID == TR2Type.Pistols_S_P);
            if (pistolIndex != -1)
            {
                // Sanity check that the location is one that we expect
                TR2Entity pistols = _levelInstance.Data.Entities[pistolIndex];
                Location pistolLocation = new()
                {
                    X = pistols.X,
                    Y = pistols.Y,
                    Z = pistols.Z,
                    Room = pistols.Room
                };

                int match = _pistolLocations[_levelInstance.Name].FindIndex
                (
                    location =>
                        location.X == pistolLocation.X &&
                        location.Y == pistolLocation.Y &&
                        location.Z == pistolLocation.Z &&
                        location.Room == pistolLocation.Room
                );

                if (match != -1)
                {
                    _unarmedLevelPistolIndex = pistolIndex;
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
