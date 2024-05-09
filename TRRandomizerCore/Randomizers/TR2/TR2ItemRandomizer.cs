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

    // Called post enemy randomization if used to allow accurate enemy scoring
    public void RandomizeAmmo()
    {
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            FindUnarmedPistolsLocation();

            if (lvl.RemovesWeapons)
            {
                RandomizeUnarmedLevelWeapon();
            }

            if (lvl.Is(TR2LevelNames.HOME))
            {
                PopulateHSHCloset();
            }

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

    private readonly Dictionary<TR2Type, uint> _startingAmmoToGive = new()
    {
        {TR2Type.Shotgun_S_P, 8},
        {TR2Type.Automags_S_P, 4},
        {TR2Type.Uzi_S_P, 4},
        {TR2Type.Harpoon_S_P, 4}, // #149 Agreed that a low number of harpoons will be given for unarmed levels, but pistols will also be included
        {TR2Type.M16_S_P, 2},
        {TR2Type.GrenadeLauncher_S_P, 4},
    };

    private void RandomizeUnarmedLevelWeapon()
    {
        if (!Settings.GiveUnarmedItems)
        {
            return;
        }

        //Is there something in the unarmed level pistol location?
        if (_unarmedLevelPistolIndex != -1)
        {
            List<TR2Type> replacementWeapons = TR2TypeUtilities.GetGunTypes();
            replacementWeapons.Add(TR2Type.Pistols_S_P);
            TR2Type weaponType = replacementWeapons[_generator.Next(0, replacementWeapons.Count)];

            // force pistols for OneLimit and then we're done
            if (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
            {
                return;
            }

            if (_levelInstance.Is(TR2LevelNames.CHICKEN))
            {
                // Grenade Launcher and Harpoon cannot trigger the bells in Ice Palace
                while (weaponType.Equals(TR2Type.GrenadeLauncher_S_P) || weaponType.Equals(TR2Type.Harpoon_S_P))
                {
                    weaponType = replacementWeapons[_generator.Next(0, replacementWeapons.Count)];
                }
            }

            uint ammoToGive = 0;
            bool addPistols = false;
            uint smallMediToGive = 0;
            uint largeMediToGive = 0;

            if (_startingAmmoToGive.ContainsKey(weaponType))
            {
                ammoToGive = _startingAmmoToGive[weaponType];
                if (Settings.RandomizeEnemies && Settings.CrossLevelEnemies)
                {
                    // Create a score based on each type of enemy in this level and increase the ammo count based on this
                    EnemyDifficulty difficulty = TR2EnemyUtilities.GetEnemyDifficulty(_levelInstance.GetEnemyEntities());
                    ammoToGive *= (uint)difficulty;

                    // Depending on how difficult the enemy combination is, allocate some extra helpers.
                    addPistols = difficulty > EnemyDifficulty.Easy;

                    if (difficulty == EnemyDifficulty.Medium || difficulty == EnemyDifficulty.Hard)
                    {
                        smallMediToGive++;
                    }
                    if (difficulty > EnemyDifficulty.Medium)
                    {
                        largeMediToGive++;
                    }
                    if (difficulty == EnemyDifficulty.VeryHard)
                    {
                        largeMediToGive++;
                    }
                }
                else if (_levelInstance.Is(TR2LevelNames.LAIR))
                {
                    ammoToGive *= 6;
                }
            }

            TR2Entity unarmedLevelWeapons = _levelInstance.Data.Entities[_unarmedLevelPistolIndex];
            unarmedLevelWeapons.TypeID = weaponType;

            if (weaponType != TR2Type.Pistols_S_P)
            {
                //#68 - Provide some additional ammo for a weapon if not pistols
                AddUnarmedLevelAmmo(GetWeaponAmmo(weaponType), ammoToGive);

                // If we haven't decided to add the pistols (i.e. for enemy difficulty)
                // add a 1/3 chance of getting them anyway. #149 If the harpoon is being
                // given, the pistols will be included.
                if (addPistols || weaponType == TR2Type.Harpoon_S_P || _generator.Next(0, 3) == 0)
                {
                    CopyEntity(unarmedLevelWeapons, TR2Type.Pistols_S_P);
                }
            }

            for (int i = 0; i < smallMediToGive; i++)
            {
                CopyEntity(unarmedLevelWeapons, TR2Type.SmallMed_S_P);
            }
            for (int i = 0; i < largeMediToGive; i++)
            {
                CopyEntity(unarmedLevelWeapons, TR2Type.LargeMed_S_P);
            }
        }
    }

    private void CopyEntity(TR2Entity entity, TR2Type newType)
    {
        if (_levelInstance.Data.Entities.Count < _levelInstance.GetMaximumEntityLimit())
        {
            TR2Entity copy = (TR2Entity)entity.Clone();
            copy.TypeID = newType;
            _levelInstance.Data.Entities.Add(copy);
        }
    }

    private static TR2Type GetWeaponAmmo(TR2Type weapon)
    {
        return weapon switch
        {
            TR2Type.Shotgun_S_P => TR2Type.ShotgunAmmo_S_P,
            TR2Type.Automags_S_P => TR2Type.AutoAmmo_S_P,
            TR2Type.Uzi_S_P => TR2Type.UziAmmo_S_P,
            TR2Type.Harpoon_S_P => TR2Type.HarpoonAmmo_S_P,
            TR2Type.M16_S_P => TR2Type.M16Ammo_S_P,
            TR2Type.GrenadeLauncher_S_P => TR2Type.Grenades_S_P,
            _ => TR2Type.PistolAmmo_S_P,
        };
    }

    private void AddUnarmedLevelAmmo(TR2Type ammoType, uint count)
    {
        // #216 - Avoid bloating the entity list by creating additional pickups
        // and instead add the extra ammo directly to the inventory.
        _levelInstance.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(ammoType), count);
    }

    private void PopulateHSHCloset()
    {
        // Special handling for HSH to keep everything in the closet, but only if Lara loses guns or ammo.
        if (!_levelInstance.Script.RemovesAmmo && !_levelInstance.Script.RemovesWeapons)
        {
            return;
        }

        List<TR2Type> replacementWeapons = TR2TypeUtilities.GetGunTypes();
        if (_levelInstance.Script.RemovesWeapons)
        {
            replacementWeapons.Add(TR2Type.Pistols_S_P);
        }

        // Pick a new weapon, but exclude the grenade launcher because it affects the kill count
        TR2Type replacementWeapon;
        do
        {
            replacementWeapon = replacementWeapons[_generator.Next(0, replacementWeapons.Count)];
        }
        while (replacementWeapon == TR2Type.GrenadeLauncher_S_P);

        TR2Type replacementAmmo = GetWeaponAmmo(replacementWeapon);

        TR2Entity harpoonWeapon = null;
        List<TR2Type> oneOfEachType = new();
        foreach (TR2Entity entity in _levelInstance.Data.Entities)
        {
            if (!TR2TypeUtilities.IsAnyPickupType(entity.TypeID))
            {
                continue;
            }

            TR2Type entityType = entity.TypeID;
            if (TR2TypeUtilities.IsGunType(entityType))
            {
                entity.TypeID = replacementWeapon;

                if (replacementWeapon == TR2Type.Harpoon_S_P || (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit && replacementWeapon != TR2Type.Pistols_S_P))
                {
                    harpoonWeapon = entity;
                }
            }
            else if (TR2TypeUtilities.IsAmmoType(entityType) && replacementWeapon != TR2Type.Pistols_S_P)
            {
                entity.TypeID = replacementAmmo;
            }

            if (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
            {
                // look for extra utility/ammo items and hide them
                TR2Type eType = entity.TypeID;
                if (TR2TypeUtilities.IsUtilityType(eType) ||
                    TR2TypeUtilities.IsGunType(eType))
                {
                    if (oneOfEachType.Contains(eType))
                    {
                        ItemUtilities.HideEntity(entity);
                    }
                    else
                        oneOfEachType.Add(entity.TypeID);
                }
            }
        }

        // if weapon is harpoon OR difficulty is OneLimit, spawn pistols as well (see #149)
        if (harpoonWeapon != null)
            CopyEntity(harpoonWeapon, TR2Type.Pistols_S_P);
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
            //TexturePositionMonitor = TextureMonitor.CreateMonitor(_levelInstance.Name, vehicles.Keys.ToList())
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
                                location2ndBoat = VehicleUtilities.GetRandomLocation(_levelInstance, TR2Type.Boat, _generator, false);
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
        Location location = VehicleUtilities.GetRandomLocation(_levelInstance, entity, _generator);
        if (location != null)
        {
            locationMap[entity] = location;
        }
    }
}
