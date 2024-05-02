using Newtonsoft.Json;
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
    private readonly Dictionary<string, List<Location>> _excludedLocations;
    private readonly Dictionary<string, List<Location>> _pistolLocations;

    // Track the pistols so they remain a weapon type and aren't moved
    private TR3Entity _unarmedLevelPistols;

    // Secret reward items handled in separate class, so track the reward entities
    private TR3SecretMapping _secretMapping;

    private readonly LocationPicker _picker;

    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public TR3ItemRandomizer()
    {
        _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\invalid_item_locations.json"));
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\unarmed_locations.json"));
        _picker = new(GetResourcePath(@"TR3\Locations\routes.json"));
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        
        foreach (TR3ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            FindUnarmedLevelPistols(_levelInstance);

            _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, false), Settings, _generator);
            _secretMapping = TR3SecretMapping.Get(_levelInstance);

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
                    level.Data.FloorData.RemoveEntityTriggers(i);
                }
            }
        }
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

            TR3Entity entity = level.Data.Entities[i];
            // Move standard items only, excluding any unarmed level pistols, and reward items
            if (TR3TypeUtilities.IsStandardPickupType(entity.TypeID) && entity != _unarmedLevelPistols)
            {
                _picker.RandomizePickupLocation(entity);
            }
        }
    }

    private List<Location> GetItemLocationPool(TR3CombinedLevel level, bool keyItemMode)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(level.Name))
        {
            exclusions.AddRange(_excludedLocations[level.Name]);
        }

        foreach (TR3Entity entity in level.Data.Entities)
        {
            if (!TR3TypeUtilities.CanSharePickupSpace(entity.TypeID))
            {
                exclusions.Add(entity.GetFloorLocation(loc => level.Data.GetRoomSector(loc)));
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
            for (short i = 0; i < level.Data.Rooms.Count; i++)
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
        return generator.Generate(level.Data, exclusions, keyItemMode);
    }

    public void RandomizeKeyItems(TR3CombinedLevel level)
    {
        _picker.TriggerTestAction = location => LocationUtilities.HasAnyTrigger(location, level.Data);
        _picker.KeyItemTestAction = (location, hasPickupTrigger) => TestKeyItemLocation(location, hasPickupTrigger, level);
        _picker.RoomInfos = level.Data.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();

        _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, true), Settings, _generator);

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR3Entity entity = level.Data.Entities[i];
            if (!TR3TypeUtilities.IsKeyItemType(entity.TypeID)
                || ItemFactory.IsItemLocked(level.Name, i))
            {
                continue;
            }

            _picker.RandomizeKeyItemLocation(
                entity, LocationUtilities.HasPickupTriger(entity, i, level.Data),
                level.Script.OriginalSequence, level.Data.Rooms[entity.Room].Info);
        }
    }

    private bool TestKeyItemLocation(Location location, bool hasPickupTrigger, TR3CombinedLevel level)
    {
        // Make sure if we're placing on the same tile as an enemy, that the
        // enemy can drop the item.
        TR3Entity enemy = level.Data.Entities
            .FindAll(e => TR3TypeUtilities.IsEnemyType(e.TypeID))
            .Find(e => e.GetLocation().IsEquivalent(location));

        return enemy == null || (Settings.AllowEnemyKeyDrops && !hasPickupTrigger && TR3TypeUtilities.CanDropPickups
        (
            TR3TypeUtilities.GetAliasForLevel(level.Name, enemy.TypeID),
            !Settings.RandomizeEnemies || Settings.ProtectMonks
        ));
    }
}
