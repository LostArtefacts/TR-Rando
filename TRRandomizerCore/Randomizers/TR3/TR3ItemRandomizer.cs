using Newtonsoft.Json;
using TRFDControl;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3ItemRandomizer : BaseTR3Randomizer
{
    private readonly Dictionary<string, LevelPickupZoneDescriptor> _keyItemZones;
    private readonly Dictionary<string, List<Location>> _keyItemLocations;
    private readonly Dictionary<string, List<Location>> _excludedLocations;
    private readonly Dictionary<string, List<Location>> _pistolLocations;

    private static readonly int _ANY_ROOM_ALLOWED = 2048;  //Max rooms is 1024 - so this should never be possible.

    // Track the pistols so they remain a weapon type and aren't moved
    private TR3Entity _unarmedLevelPistols;

    // Secret reward items handled in separate class, so track the reward entities
    private TRSecretMapping<TR2Entity> _secretMapping;

    private readonly LocationPicker _picker;

    public ItemFactory ItemFactory { get; set; }

    public TR3ItemRandomizer()
    {
        _keyItemZones = JsonConvert.DeserializeObject<Dictionary<string, LevelPickupZoneDescriptor>>(ReadResource(@"TR3\Locations\Zones.json"));
        _keyItemLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\item_locations.json"));
        _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\invalid_item_locations.json"));
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\unarmed_locations.json"));
        _picker = new();
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        
        foreach (TR3ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            FindUnarmedLevelPistols(_levelInstance);

            _picker.Initialise(GetItemLocationPool(_levelInstance), _generator);
            _secretMapping = TRSecretMapping<TR2Entity>.Get(GetResourcePath($@"TR3\SecretMapping\{_levelInstance.Name}-SecretMapping.json"));

            // #312 If this is the assault course, import required models. On failure, don't perform any item rando.
            if (_levelInstance.IsAssault && !ImportAssaultModels(_levelInstance))
            {
                continue;
            }

            if (Settings.RandomizeItemTypes)
            {
                RandomizeItemTypes(_levelInstance);
            }

            if (Settings.RandomizeItemPositions)
            {
                RandomizeItemLocations(_levelInstance);
            }

            if (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
            {
                EnforceOneLimit(_levelInstance);
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
        foreach (TR3ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            _picker.Initialise(GetItemLocationPool(_levelInstance), _generator);
            RandomizeKeyItems(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private bool ImportAssaultModels(TR3CombinedLevel level)
    {
        // #312 We need all item models plus Lara's associated weapon animations for the
        // assault course. The DEagle and Uzi anims will match Lara's default home outfit
        // - outfit rando will take care of replacing these if it's enabled.
        TR3ModelImporter importer = new()
        {
            Level = level.Data,
            LevelName = level.Name,
            ClearUnusedSprites = false,
            EntitiesToImport = new List<TR3Type>
            {
                TR3Type.LaraShotgunAnimation_H,
                TR3Type.LaraDeagleAnimation_H_Home,
                TR3Type.LaraUziAnimation_H_Home,
                TR3Type.LaraMP5Animation_H,
                TR3Type.LaraRocketAnimation_H,
                TR3Type.LaraGrenadeAnimation_H,
                TR3Type.LaraHarpoonAnimation_H,
                TR3Type.PistolAmmo_P,
                TR3Type.Shotgun_P, TR3Type.Shotgun_M_H,
                TR3Type.ShotgunAmmo_P, TR3Type.ShotgunAmmo_M_H,
                TR3Type.Deagle_P, TR3Type.Deagle_M_H,
                TR3Type.DeagleAmmo_P, TR3Type.DeagleAmmo_M_H,
                TR3Type.Uzis_P, TR3Type.Uzis_M_H,
                TR3Type.UziAmmo_P, TR3Type.UziAmmo_M_H,
                TR3Type.MP5_P, TR3Type.MP5_M_H,
                TR3Type.MP5Ammo_P, TR3Type.MP5Ammo_M_H,
                TR3Type.RocketLauncher_M_H, TR3Type.RocketLauncher_P,
                TR3Type.Rockets_M_H, TR3Type.Rockets_P,
                TR3Type.GrenadeLauncher_M_H, TR3Type.GrenadeLauncher_P,
                TR3Type.Grenades_M_H, TR3Type.Grenades_P,
                TR3Type.Harpoon_M_H, TR3Type.Harpoon_P,
                TR3Type.Harpoons_M_H, TR3Type.Harpoons_P,
                TR3Type.SmallMed_P, TR3Type.SmallMed_M_H,
                TR3Type.LargeMed_P, TR3Type.LargeMed_M_H
            },
            DataFolder = GetResourcePath(@"TR3\Models")
        };

        string remapPath = @"TR3\Textures\Deduplication\" + level.Name + "-TextureRemap.json";
        if (ResourceExists(remapPath))
        {
            importer.TextureRemapPath = GetResourcePath(remapPath);
        }

        try
        {
            // Try to import the selected models into the level.
            importer.Import();
            return true;
        }
        catch (PackingException)
        {
            return false;
        }
    }

    private void FindUnarmedLevelPistols(TR3CombinedLevel level)
    {
        if (level.Script.RemovesWeapons)
        {
            List<TR3Entity> pistolEntities = level.Data.Entities.FindAll(e => e.TypeID == TR3Type.Pistols_P);
            foreach (TR3Entity pistols in pistolEntities)
            {
                int match = _pistolLocations[level.Name].FindIndex
                (
                    location =>
                        location.X == pistols.X &&
                        location.Y == pistols.Y &&
                        location.Z == pistols.Z &&
                        location.Room == pistols.Room
                );
                if (match != -1)
                {
                    _unarmedLevelPistols = pistols;
                    break;
                }
            }
        }
        else
        {
            _unarmedLevelPistols = null;
        }
    }

    public void RandomizeItemTypes(TR3CombinedLevel level)
    {
        List<TR3Type> stdItemTypes = TR3TypeUtilities.GetStandardPickupTypes();

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            if (_secretMapping != null && _secretMapping.RewardEntities.Contains(i))
            {
                // Leave default secret rewards as they are - handled separately
                continue;
            }

            TR3Entity ent = level.Data.Entities[i];
            TR3Type currentType = ent.TypeID;
            // If this is an unarmed level's pistols, make sure they're replaced with another weapon.
            // Similar case for the assault course, so that Lara can still shoot Winnie.
            if ((ent == _unarmedLevelPistols && Settings.GiveUnarmedItems)
                || (level.IsAssault && TR3TypeUtilities.IsWeaponPickup(currentType)))
            {
                do
                {
                    ent.TypeID = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                }
                while (!TR3TypeUtilities.IsWeaponPickup(ent.TypeID));

                if (level.IsAssault)
                {
                    // Add some extra ammo too
                    level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR3TypeUtilities.GetWeaponAmmo(ent.TypeID)), 20);
                }
            }
            else if (TR3TypeUtilities.IsStandardPickupType(currentType))
            {
                ent.TypeID = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
            }
        }
    }

    public void EnforceOneLimit(TR3CombinedLevel level)
    {
        HashSet<TR3Type> uniqueTypes = new();
        if (_unarmedLevelPistols != null)
        {
            // These will be excluded, but track their type before looking at other items.
            uniqueTypes.Add(_unarmedLevelPistols.TypeID);
        }

        // FD for removing crystal triggers if applicable.
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        // Look for extra utility/ammo items and hide them
        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR3Entity entity = level.Data.Entities[i];
            if ((_secretMapping != null && _secretMapping.RewardEntities.Contains(i)) || entity == _unarmedLevelPistols)
            {
                // Rewards and unarmed level weapons excluded
                continue;
            }

            if ((TR3TypeUtilities.IsStandardPickupType(entity.TypeID) || TR3TypeUtilities.IsCrystalPickup(entity.TypeID))
                && !uniqueTypes.Add(entity.TypeID))
            {
                ItemUtilities.HideEntity(entity);
                ItemFactory.FreeItem(level.Name, i);
                if (TR3TypeUtilities.IsCrystalPickup(entity.TypeID))
                {
                    FDUtilities.RemoveEntityTriggers(level.Data, i, floorData);
                }
            }
        }

        floorData.WriteToLevel(level.Data);
    }

    public void RandomizeItemLocations(TR3CombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            if (_secretMapping.RewardEntities.Contains(i)
                || ItemFactory.IsItemLocked(_levelInstance.Name, i))
            {
                continue;
            }

            TR3Entity ent = level.Data.Entities[i];
            // Move standard items only, excluding any unarmed level pistols, and reward items
            if (TR3TypeUtilities.IsStandardPickupType(ent.TypeID) && ent != _unarmedLevelPistols)
            {
                Location location = _picker.GetPickupLocation();
                _picker.SetLocation(ent, location);
            }
        }
    }

    private List<Location> GetItemLocationPool(TR3CombinedLevel level)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(level.Name))
        {
            exclusions.AddRange(_excludedLocations[level.Name]);
        }

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        foreach (TR3Entity entity in level.Data.Entities)
        {
            if (!TR3TypeUtilities.CanSharePickupSpace(entity.TypeID))
            {
                exclusions.Add(LocationPicker.CreateExcludedLocation(entity, loc =>
                    FDUtilities.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room, level.Data, floorData)));
            }
        }

        if (Settings.RandomizeSecrets && level.HasSecrets)
        {
            // Make sure to exclude the reward room
            exclusions.Add(new()
            {
                Room = RoomWaterUtilities.DefaultRoomCountDictionary[level.Name],
                InvalidatesRoom = true
            });
        }

        if (level.HasExposureMeter)
        {
            // Don't put items underwater if it's too cold
            for (int i = 0; i < level.Data.NumRooms; i++)
            {
                if (level.Data.Rooms[i].ContainsWater)
                {
                    exclusions.Add(new()
                    {
                        Room = i,
                        InvalidatesRoom = true
                    });
                }
            }
        }

        TR3LocationGenerator generator = new();
        return generator.Generate(level.Data, exclusions);
    }

    public void RandomizeKeyItems(TR3CombinedLevel level)
    {
        if (!_keyItemLocations.ContainsKey(level.Name))
        {
            return;
        }

        //Get all locations that have a KeyItemGroupID - e.g. intended for key items
        List<Location> levelLocations = _keyItemLocations[level.Name].Where(i => i.KeyItemGroupID != 0).ToList();

        foreach (TR3Entity entity in level.Data.Entities)
        {
            //Calculate its alias
            TR3Type aliasedKeyItemID = entity.TypeID + entity.Room + GetLevelKeyItemBaseAlias(level.Name);

            //Any special handling for key item entities
            switch (aliasedKeyItemID)
            {
                case TR3Type.DetonatorKey:
                    entity.Invisible = false;
                    break;
                default:
                    break;
            }

            if (aliasedKeyItemID < TR3Type.JungleKeyItemBase)
                throw new NotSupportedException("Level does not have key item alias group defined");

            //Is entity one we are allowed/expected to move? (is the alias and base type correct?)
            if (_keyItemZones[level.Name].AliasedExpectedKeyItems.Contains(aliasedKeyItemID) &&
                _keyItemZones[level.Name].BaseExpectedKeyItems.Contains(entity.TypeID))
            {
                do
                {
                    //Only get locations that are to position the intended key item.
                    //We can probably get rid of the do while loop as any location in this list should be valid
                    List<Location> keyItemLocations = levelLocations.Where(i => i.KeyItemGroupID == (int)aliasedKeyItemID).ToList();
                            
                    Location location = keyItemLocations[_generator.Next(0, keyItemLocations.Count)];

                    _picker.SetLocation(entity, location);

                } while (!_keyItemZones[level.Name].AllowedRooms[aliasedKeyItemID].Contains(entity.Room) &&
                        (!_keyItemZones[level.Name].AllowedRooms[aliasedKeyItemID].Contains(_ANY_ROOM_ALLOWED)));
                //Try generating locations until it is in the zone - if list contains 2048 then any room is allowed.
            }
        }
    }

    private static int GetLevelKeyItemBaseAlias(string name)
    {
        TR3Type alias = name switch
        {
            TR3LevelNames.JUNGLE => TR3Type.JungleKeyItemBase,
            TR3LevelNames.RUINS => TR3Type.TempleKeyItemBase,
            TR3LevelNames.GANGES => TR3Type.GangesKeyItemBase,
            TR3LevelNames.CAVES => TR3Type.KaliyaKeyItemBase,
            TR3LevelNames.NEVADA => TR3Type.NevadaKeyItemBase,
            TR3LevelNames.HSC => TR3Type.HSCKeyItemBase,
            TR3LevelNames.AREA51 => TR3Type.Area51KeyItemBase,
            TR3LevelNames.COASTAL => TR3Type.CoastalKeyItemBase,
            TR3LevelNames.CRASH => TR3Type.CrashKeyItemBase,
            TR3LevelNames.MADUBU => TR3Type.MadubuKeyItemBase,
            TR3LevelNames.PUNA => TR3Type.PunaKeyItemBase,
            TR3LevelNames.THAMES => TR3Type.ThamesKeyItemBase,
            TR3LevelNames.ALDWYCH => TR3Type.AldwychKeyItemBase,
            TR3LevelNames.LUDS => TR3Type.LudsKeyItemBase,
            TR3LevelNames.CITY => TR3Type.CityKeyItemBase,
            TR3LevelNames.ANTARC => TR3Type.AntarcticaKeyItemBase,
            TR3LevelNames.RXTECH => TR3Type.RXKeyItemBase,
            TR3LevelNames.TINNOS => TR3Type.TinnosKeyItemBase,
            TR3LevelNames.WILLIE => TR3Type.CavernKeyItemBase,
            TR3LevelNames.HALLOWS => TR3Type.HallowsKeyItemBase,
            _ => default,
        };

        return (int)alias;
    }
}
