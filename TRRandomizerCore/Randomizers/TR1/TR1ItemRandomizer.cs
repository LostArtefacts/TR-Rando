using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers
{
    public class TR1ItemRandomizer : BaseTR1Randomizer
    {
        // The number of extra pickups to add per level
        private static readonly Dictionary<string, int> _extraItemCounts = new Dictionary<string, int>
        {
            [TRLevelNames.CAVES]
                = 10, // Default = 4
            [TRLevelNames.VILCABAMBA]
                = 9,  // Default = 7
            [TRLevelNames.VALLEY]
                = 15, // Default = 2
            [TRLevelNames.QUALOPEC]
                = 6,  // Default = 5
            [TRLevelNames.FOLLY]
                = 8,  // Default = 8
            [TRLevelNames.COLOSSEUM]
                = 11, // Default = 7
            [TRLevelNames.MIDAS]
                = 4,  // Default = 12
            [TRLevelNames.CISTERN]
                = 0,  // Default = 16
            [TRLevelNames.TIHOCAN]
                = 0,  // Default = 16
            [TRLevelNames.KHAMOON]
                = 0,  // Default = 18
            [TRLevelNames.OBELISK]
                = 0,  // Default = 26
            [TRLevelNames.SANCTUARY]
                = 0,  // Default = 22
            [TRLevelNames.MINES]
                = 0,  // Default = 16
            [TRLevelNames.ATLANTIS]
                = 0,  // Default = 44
            [TRLevelNames.PYRAMID]
                = 0,  // Default = 21
        };

        private Dictionary<string, List<Location>> _keyItemLocations;
        private Dictionary<string, List<Location>> _excludedLocations;
        private Dictionary<string, List<Location>> _pistolLocations;

        private static readonly int _ROTATION = -8192;

        // Track the pistols so they remain a weapon type and aren't moved
        private TREntity _unarmedLevelPistols;

        // Secret reward items handled in separate class, so track the reward entities
        private TRSecretMapping<TREntity> _secretMapping;

        private List<Location> _locations;
        private ItemSpriteRandomizer<TREntities> _spriteRandomizer;

        public ItemFactory ItemFactory { get; set; }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            _keyItemLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\item_locations.json"));
            _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\invalid_item_locations.json"));
            _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\unarmed_locations.json"));

            foreach (TR1ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                FindUnarmedLevelPistols(_levelInstance);

                _locations = GetItemLocationPool(_levelInstance);
                _secretMapping = TRSecretMapping<TREntity>.Get(GetResourcePath(@"TR1\SecretMapping\" + _levelInstance.Name + "-SecretMapping.json"));

                if (Settings.IncludeExtraPickups)
                    AddExtraPickups(_levelInstance);

                if (Settings.RandomizeItemTypes)
                    RandomizeItemTypes(_levelInstance);

                // Do key items before standard items because we exclude
                // key item tiles from the valid pickup location pool
                if (Settings.IncludeKeyItems)
                    RandomizeKeyItems(_levelInstance);

                if (Settings.RandomizeItemPositions)
                    RandomizeItemLocations(_levelInstance);

                if (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
                    EnforceOneLimit(_levelInstance);

                if (Settings.RandomizeItemSprites)
                    RandomizeSprites();

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

        private void FindUnarmedLevelPistols(TR1CombinedLevel level)
        {
            if (level.Script.RemovesWeapons)
            {
                List<TREntity> pistolEntities = level.Data.Entities.ToList().FindAll(e => TR1EntityUtilities.IsWeaponPickup((TREntities)e.TypeID));
                foreach (TREntity pistols in pistolEntities)
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

            List<TREntities> stdItemTypes = TR1EntityUtilities.GetStandardPickupTypes();
            stdItemTypes.Remove(TREntities.Pistols_S_P);
            stdItemTypes.Remove(TREntities.PistolAmmo_S_P);

            // Add what we can to the level. The locations and types may be further randomized depending on the selected options.
            List<TREntity> entities = level.Data.Entities.ToList();
            for (int i = 0; i < _extraItemCounts[level.Name]; i++)
            {
                if (!ItemFactory.CanCreateItem(level.Name, entities))
                {
                    break;
                }

                TREntity newItem = ItemFactory.CreateItem(level.Name, entities, _locations[_generator.Next(0, _locations.Count)]);
                newItem.TypeID = (short)stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
            }

            level.Data.Entities = entities.ToArray();
            level.Data.NumEntities = (uint)entities.Count;
        }

        public void RandomizeItemTypes(TR1CombinedLevel level)
        {
            if (level.IsAssault)
            {
                return;
            }

            List<TREntities> stdItemTypes = TR1EntityUtilities.GetStandardPickupTypes();
            stdItemTypes.Remove(TREntities.PistolAmmo_S_P); // Sprite/model not available

            bool hasPistols = Array.Find(level.Data.Entities, e => e.TypeID == (short)TREntities.Pistols_S_P) != null;

            for (int i = 0; i < level.Data.NumEntities; i++)
            {
                if (_secretMapping.RewardEntities.Contains(i))
                {
                    // Leave default secret rewards as they are
                    continue;
                }

                TREntity entity = level.Data.Entities[i];
                TREntities entityType = (TREntities)entity.TypeID;
                
                if (entity == _unarmedLevelPistols)
                {
                    // Enemy rando may have changed this already to something else and allocated
                    // ammo to the inventory, so only change pistols.
                    if (entityType == TREntities.Pistols_S_P)
                    {
                        do
                        {
                            entityType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                        }
                        while (!TR1EntityUtilities.IsWeaponPickup(entityType));
                        entity.TypeID = (short)entityType;
                    }
                }
                else if (TR1EntityUtilities.IsStandardPickupType(entityType))
                {
                    TREntities newType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    if (newType == TREntities.Pistols_S_P && (hasPistols || !level.Script.RemovesWeapons))
                    {
                        // Only one pistol pickup per level, and only if it's unarmed
                        do
                        {
                            newType = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                        }
                        while (!TR1EntityUtilities.IsWeaponPickup(newType) || newType == TREntities.Pistols_S_P);
                    }
                    entity.TypeID = (short)newType;
                }

                hasPistols = Array.Find(level.Data.Entities, e => e.TypeID == (short)TREntities.Pistols_S_P) != null;
            }
        }

        public void EnforceOneLimit(TR1CombinedLevel level)
        {
            if (level.IsAssault)
            {
                return;
            }

            ISet<TREntities> oneOfEachType = new HashSet<TREntities>();
            if (_unarmedLevelPistols != null)
            {
                // These will be excluded, but track their type before looking at other items.
                oneOfEachType.Add((TREntities)_unarmedLevelPistols.TypeID);
            }

            // Look for extra utility/ammo items and hide them
            for (int i = 0; i < level.Data.NumEntities; i++)
            {
                TREntity ent = level.Data.Entities[i];
                if (_secretMapping.RewardEntities.Contains(i) || ent == _unarmedLevelPistols)
                {
                    // Rewards and unarmed level weapons excluded
                    continue;
                }
                
                TREntities eType = (TREntities)ent.TypeID;
                if (TR1EntityUtilities.IsStandardPickupType(eType) || TR1EntityUtilities.IsCrystalPickup(eType))
                {
                    if (!oneOfEachType.Add(eType))
                    {
                        ItemUtilities.HideEntity(ent);
                        ItemFactory.FreeItem(level.Name, i);
                    }
                }
            }
        }

        public void RandomizeItemLocations(TR1CombinedLevel level)
        {
            if (level.IsAssault)
            {
                return;
            }

            for (int i = 0; i < level.Data.NumEntities; i++)
            {
                if (_secretMapping.RewardEntities.Contains(i))
                {
                    // These will either be in their default spot or in their dedicated reward room, so leave them be
                    continue;
                }

                TREntity entity = level.Data.Entities[i];
                // Move standard items only, excluding any unarmed level pistols, and reward items
                if (TR1EntityUtilities.IsStandardPickupType((TREntities)entity.TypeID) && entity != _unarmedLevelPistols)
                {
                    Location location = _locations[_generator.Next(0, _locations.Count)];
                    entity.X = location.X;
                    entity.Y = location.Y;
                    entity.Z = location.Z;
                    entity.Room = (short)location.Room;
                    entity.Angle = location.Angle;
                    entity.Intensity = 0;

                    // Anything other than -1 means a sloped sector and so the location generator
                    // will have picked a suitable angle for it. For flat sectors, spin the entities
                    // around randomly for variety.
                    if (entity.Angle == -1)
                    {
                        entity.Angle = (short)(_generator.Next(0, 8) * _ROTATION);
                    }
                }
            }

            if (ScriptEditor.Edition.IsCommunityPatch)
            {
                // T1M allows us to keep the end-level stats accurate. All generated locations
                // should be reachable.
                level.Script.UnobtainablePickups = null;
            }
        }

        private List<Location> GetItemLocationPool(TR1CombinedLevel level)
        {
            List<Location> exclusions = new List<Location>();
            if (_excludedLocations.ContainsKey(level.Name))
            {
                exclusions.AddRange(_excludedLocations[level.Name]);
            }

            foreach (TREntity entity in level.Data.Entities)
            {
                if (!TR1EntityUtilities.CanSharePickupSpace((TREntities)entity.TypeID))
                {
                    exclusions.Add(new Location
                    {
                        X = entity.X,
                        Y = entity.Y,
                        Z = entity.Z,
                        Room = entity.Room
                    });
                }
            }

            if (Settings.RandomizeSecrets)
            {
                //Make sure to exclude the reward room
                exclusions.Add(new Location
                {
                    Room = RoomWaterUtilities.DefaultRoomCountDictionary[level.Name],
                    InvalidatesRoom = true
                });
            }

            TR1LocationGenerator generator = new TR1LocationGenerator();
            return generator.Generate(level.Data, exclusions);
        }

        private void RandomizeKeyItems(TR1CombinedLevel level)
        {
            List<Location> locations;
            if (!_keyItemLocations.ContainsKey(level.Name) || (locations = _keyItemLocations[level.Name]).Count == 0)
            {
                return;
            }

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            for (int i = 0; i < level.Data.NumEntities; i++)
            {
                TREntity entity = level.Data.Entities[i];
                TREntities type = (TREntities)entity.TypeID;
                if (!TR1EntityUtilities.IsKeyItemType(type) || IsSecretItem(entity, i, level.Data, floorData))
                {
                    continue;
                }

                // Only move a key item if there is at least one location defined for it. Any triggers below the
                // item will be handled by default environment mods, so don't place an item in the same sector as a secret.
                // The only one we don't currently move is MinesFuseNearConveyor - potential FlipMap complications.
                int itemID = 10000 + ((level.Script.OriginalSequence - 1) * 1000) + entity.TypeID + entity.Room;
                List<Location> pool = locations.FindAll(l => l.KeyItemGroupID == itemID);
                if (pool.Count > 0)
                {
                    Location location;
                    do
                    {
                        location = pool[_generator.Next(0, pool.Count)];
                    }
                    while (location.ContainsSecret(level.Data, floorData));

                    entity.X = location.X;
                    entity.Y = location.Y;
                    entity.Z = location.Z;
                    entity.Room = (short)location.Room;
                }
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
                _spriteRandomizer = new ItemSpriteRandomizer<TREntities>
                {
                    StandardItemTypes = TR1EntityUtilities.GetStandardPickupTypes(),
                    RandomizeKeyItemSprites = Settings.RandomizeKeyItemSprites,
                    RandomizeSecretSprites = Settings.RandomizeSecretSprites,
                    Mode = Settings.SpriteRandoMode
                };

                // Pistol ammo sprite is not available
                _spriteRandomizer.StandardItemTypes.Remove(TREntities.PistolAmmo_S_P);
#if DEBUG
                _spriteRandomizer.TextureChanged += (object sender, SpriteEventArgs<TREntities> e) =>
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1} => {2}", _levelInstance.Name, e.OldSprite, e.NewSprite));
                };
#endif
            }

            // For key items, some may be used as secrets so look for entity instances of each to determine what's what
            _spriteRandomizer.SecretItemTypes = new List<TREntities>();
            _spriteRandomizer.KeyItemTypes = new List<TREntities>();
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(_levelInstance.Data);
            foreach (TREntities type in TR1EntityUtilities.GetListOfKeyItemTypes())
            {
                int typeInstanceIndex = Array.FindIndex(_levelInstance.Data.Entities, e => e.TypeID == (short)type);
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

        private bool IsSecretItem(TREntity entity, int entityIndex, TRLevel level, FDControl floorData)
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
}