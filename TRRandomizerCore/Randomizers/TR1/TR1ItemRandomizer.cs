using Newtonsoft.Json;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1ItemRandomizer : BaseTR1Randomizer
{
    // The number of extra pickups to add per level
    private static readonly Dictionary<string, int> _extraItemCounts = new()
    {
        [TR1LevelNames.CAVES]
            = 10, // Default = 4
        [TR1LevelNames.VILCABAMBA]
            = 9,  // Default = 7
        [TR1LevelNames.VALLEY]
            = 15, // Default = 2
        [TR1LevelNames.QUALOPEC]
            = 6,  // Default = 5
        [TR1LevelNames.FOLLY]
            = 8,  // Default = 8
        [TR1LevelNames.COLOSSEUM]
            = 11, // Default = 7
        [TR1LevelNames.MIDAS]
            = 4,  // Default = 12
        [TR1LevelNames.CISTERN]
            = 0,  // Default = 16
        [TR1LevelNames.TIHOCAN]
            = 0,  // Default = 16
        [TR1LevelNames.KHAMOON]
            = 0,  // Default = 18
        [TR1LevelNames.OBELISK]
            = 0,  // Default = 26
        [TR1LevelNames.SANCTUARY]
            = 0,  // Default = 22
        [TR1LevelNames.MINES]
            = 0,  // Default = 16
        [TR1LevelNames.ATLANTIS]
            = 0,  // Default = 44
        [TR1LevelNames.PYRAMID]
            = 0,  // Default = 21
    };

    private readonly Dictionary<string, List<Location>> _excludedLocations;
    private readonly Dictionary<string, List<Location>> _pistolLocations;

    // Track the pistols so they remain a weapon type and aren't moved
    private TR1Entity _unarmedLevelPistols;

    // Secret reward items handled in separate class, so track the reward entities
    private TRSecretMapping<TR1Entity> _secretMapping;

    private readonly LocationPicker _picker;
    private ItemSpriteRandomizer<TR1Type> _spriteRandomizer;

    public ItemFactory ItemFactory { get; set; }

    public TR1ItemRandomizer()
    {
        _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\invalid_item_locations.json"));
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\unarmed_locations.json"));
        _picker = new(GetResourcePath(@"TR1\Locations\routes.json"));
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);

        foreach (TR1ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            FindUnarmedLevelPistols(_levelInstance);

            _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, false), Settings, _generator);
            _secretMapping = TRSecretMapping<TR1Entity>.Get(GetResourcePath($@"TR1\SecretMapping\{_levelInstance.Name}-SecretMapping.json"));

            if (Settings.IncludeExtraPickups)
            {
                AddExtraPickups(_levelInstance);
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

            if (Settings.RandomizeItemSprites)
            {
                RandomizeSprites();
            }

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }

        if (ScriptEditor.Edition.IsCommunityPatch && Settings.UseRecommendedCommunitySettings)
        {
            (ScriptEditor.Script as TR1Script).Enable3dPickups = false;
            ScriptEditor.SaveScript();
        }
    }

    public void RandomizeKeyItems()
    {
        foreach (TR1ScriptedLevel lvl in Levels)
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

    private void FindUnarmedLevelPistols(TR1CombinedLevel level)
    {
        if (level.Script.RemovesWeapons)
        {
            List<TR1Entity> pistolEntities = level.Data.Entities.FindAll(e => TR1TypeUtilities.IsWeaponPickup(e.TypeID));
            foreach (TR1Entity pistols in pistolEntities)
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

    private void AddExtraPickups(TR1CombinedLevel level)
    {
        if (!_extraItemCounts.ContainsKey(level.Name))
        {
            return;
        }

        List<TR1Type> stdItemTypes = TR1TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR1Type.Pistols_S_P);
        stdItemTypes.Remove(TR1Type.PistolAmmo_S_P);

        // Add what we can to the level. The locations and types may be further randomized depending on the selected options.
        for (int i = 0; i < _extraItemCounts[level.Name]; i++)
        {
            if (!ItemFactory.CanCreateItem(level.Name, level.Data.Entities))
            {
                break;
            }

            TR1Entity newItem = ItemFactory.CreateItem(level.Name, level.Data.Entities, _picker.GetRandomLocation());
            newItem.TypeID = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
        }
    }

    public void RandomizeItemTypes(TR1CombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        List<TR1Type> stdItemTypes = TR1TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR1Type.PistolAmmo_S_P); // Sprite/model not available

        bool hasPistols = level.Data.Entities.Any(e => e.TypeID == TR1Type.Pistols_S_P);

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            if (_secretMapping.RewardEntities.Contains(i))
            {
                // Leave default secret rewards as they are
                continue;
            }

            TR1Entity entity = level.Data.Entities[i];
            TR1Type entityType = entity.TypeID;
            
            if (entity == _unarmedLevelPistols)
            {
                // Enemy rando may have changed this already to something else and allocated
                // ammo to the inventory, so only change pistols.
                if (entityType == TR1Type.Pistols_S_P && Settings.GiveUnarmedItems)
                {
                    do
                    {
                        entityType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!TR1TypeUtilities.IsWeaponPickup(entityType));
                    entity.TypeID = entityType;
                }
            }
            else if (TR1TypeUtilities.IsStandardPickupType(entityType))
            {
                TR1Type newType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                if (newType == TR1Type.Pistols_S_P && (hasPistols || !level.Script.RemovesWeapons))
                {
                    // Only one pistol pickup per level, and only if it's unarmed
                    do
                    {
                        newType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!TR1TypeUtilities.IsWeaponPickup(newType) || newType == TR1Type.Pistols_S_P);
                }
                entity.TypeID = newType;
            }

            hasPistols = level.Data.Entities.Any(e => e.TypeID == TR1Type.Pistols_S_P);
        }
    }

    public void EnforceOneLimit(TR1CombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        HashSet<TR1Type> uniqueTypes = new();
        if (_unarmedLevelPistols != null)
        {
            // These will be excluded, but track their type before looking at other items.
            uniqueTypes.Add(_unarmedLevelPistols.TypeID);
        }

        // Look for extra utility/ammo items and hide them
        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR1Entity entity = level.Data.Entities[i];
            if (_secretMapping.RewardEntities.Contains(i) || entity == _unarmedLevelPistols)
            {
                // Rewards and unarmed level weapons excluded
                continue;
            }
            
            if ((TR1TypeUtilities.IsStandardPickupType(entity.TypeID) || TR1TypeUtilities.IsCrystalPickup(entity.TypeID))
                && !uniqueTypes.Add(entity.TypeID))
            {
                ItemUtilities.HideEntity(entity);
                ItemFactory.FreeItem(level.Name, i);
            }
        }
    }

    public void RandomizeItemLocations(TR1CombinedLevel level)
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

            TR1Entity entity = level.Data.Entities[i];
            // Move standard items only, excluding any unarmed level pistols, and reward items
            if (TR1TypeUtilities.IsStandardPickupType(entity.TypeID) && entity != _unarmedLevelPistols)
            {
                _picker.RandomizePickupLocation(entity);
                entity.Intensity = 0;
            }
        }

        if (ScriptEditor.Edition.IsCommunityPatch)
        {
            // T1M allows us to keep the end-level stats accurate. All generated locations
            // should be reachable.
            level.Script.UnobtainablePickups = null;
        }
    }

    private List<Location> GetItemLocationPool(TR1CombinedLevel level, bool keyItemMode)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(level.Name))
        {
            exclusions.AddRange(_excludedLocations[level.Name]);
        }

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        foreach (TR1Entity entity in level.Data.Entities)
        {
            if (!TR1TypeUtilities.CanSharePickupSpace(entity.TypeID))
            {
                exclusions.Add(entity.GetFloorLocation(loc =>
                    FDUtilities.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room, level.Data, floorData)));
            }
        }

        if (Settings.RandomizeSecrets)
        {
            //Make sure to exclude the reward room
            exclusions.Add(new()
            {
                Room = RoomWaterUtilities.DefaultRoomCountDictionary[level.Name],
                InvalidatesRoom = true
            });
        }

        TR1LocationGenerator generator = new();
        return generator.Generate(level.Data, exclusions, keyItemMode);
    }

    private void RandomizeKeyItems(TR1CombinedLevel level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        _picker.TriggerTestAction = location => LocationUtilities.HasAnyTrigger(location, level.Data, floorData);
        _picker.RoomInfos = level.Data.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();

        _picker.Initialise(_levelInstance.Name, GetItemLocationPool(_levelInstance, true), Settings, _generator);

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR1Entity entity = level.Data.Entities[i];
            if (!TR1TypeUtilities.IsKeyItemType(entity.TypeID)
                || ItemFactory.IsItemLocked(level.Name, i))
            {
                continue;
            }

            _picker.RandomizeKeyItemLocation(
                entity, LocationUtilities.HasPickupTriger(entity, i, level.Data, floorData),
                level.Script.OriginalSequence, level.Data.Rooms[entity.Room].Info);
        }
    }

    private void RandomizeSprites()
    {
        if (ScriptEditor.Edition.IsCommunityPatch
            && !Settings.UseRecommendedCommunitySettings
            && (ScriptEditor.Script as TR1Script).Enable3dPickups)
        {
            // With 3D pickups enabled, sprite randomization is meaningless
            return;
        }

        if (_spriteRandomizer == null)
        {
            _spriteRandomizer = new ItemSpriteRandomizer<TR1Type>
            {
                StandardItemTypes = TR1TypeUtilities.GetStandardPickupTypes(),
                RandomizeKeyItemSprites = Settings.RandomizeKeyItemSprites,
                RandomizeSecretSprites = Settings.RandomizeSecretSprites,
                Mode = Settings.SpriteRandoMode
            };

            // Pistol ammo sprite is not available
            _spriteRandomizer.StandardItemTypes.Remove(TR1Type.PistolAmmo_S_P);
#if DEBUG
            _spriteRandomizer.TextureChanged += (object sender, SpriteEventArgs<TR1Type> e) =>
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1} => {2}", _levelInstance.Name, e.OldSprite, e.NewSprite));
            };
#endif
        }

        // For key items, some may be used as secrets so look for entity instances of each to determine what's what
        _spriteRandomizer.SecretItemTypes = new List<TR1Type>();
        _spriteRandomizer.KeyItemTypes = new List<TR1Type>();
        FDControl floorData = new();
        floorData.ParseFromLevel(_levelInstance.Data);
        foreach (TR1Type type in TR1TypeUtilities.GetKeyItemTypes())
        {
            int typeInstanceIndex = _levelInstance.Data.Entities.FindIndex(e => e.TypeID == type);
            if (typeInstanceIndex != -1)
            {
                if (IsSecretItem(_levelInstance.Data.Entities[typeInstanceIndex], typeInstanceIndex, _levelInstance.Data, floorData))
                {
                    _spriteRandomizer.SecretItemTypes.Add(type);
                }
                else
                {
                    _spriteRandomizer.KeyItemTypes.Add(type);
                }
            }
        }

        _spriteRandomizer.Sequences = _levelInstance.Data.SpriteSequences.ToList();
        _spriteRandomizer.Textures = _levelInstance.Data.SpriteTextures.ToList();

        _spriteRandomizer.Randomize(_generator);

        _levelInstance.Data.SpriteTextures = _spriteRandomizer.Textures.ToArray();
    }

    private static bool IsSecretItem(TR1Entity entity, int entityIndex, TR1Level level, FDControl floorData)
    {
        TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
        if (sector.FDIndex != 0)
        {
            return floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                && trigger.TrigType == FDTrigType.Pickup
                && trigger.TrigActionList[0].Parameter == entityIndex
                && trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.SecretFound) != null;
        }

        return false;
    }
}
