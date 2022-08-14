using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<string, List<Location>> _keyItemLocations;
        private Dictionary<string, List<Location>> _excludedLocations;
        private Dictionary<string, List<Location>> _pistolLocations;

        private static readonly int _ROTATION = -8192;

        // Track the pistols so they remain a weapon type and aren't moved
        private TREntity _unarmedLevelPistols;

        // Secret reward items handled in separate class, so track the reward entities
        private TRSecretMapping<TREntity> _secretMapping;

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

                _secretMapping = TRSecretMapping<TREntity>.Get(GetResourcePath(@"TR1\SecretMapping\" + _levelInstance.Name + "-SecretMapping.json"));

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
                List<TREntity> pistolEntities = level.Data.Entities.ToList().FindAll(e => e.TypeID == (short)TREntities.Pistols_S_P);
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

        public void RandomizeItemTypes(TR1CombinedLevel level)
        {
            if (level.IsAssault)
            {
                return;
            }

            List<TREntities> stdItemTypes = TR1EntityUtilities.GetStandardPickupTypes();
            stdItemTypes.Remove(TREntities.PistolAmmo_S_P); // Sprite/model not available

            for (int i = 0; i < level.Data.NumEntities; i++)
            {
                if (_secretMapping.RewardEntities.Contains(i))
                {
                    // Leave default secret rewards as they are
                    continue;
                }

                TREntity entity = level.Data.Entities[i];
                TREntities currentType = (TREntities)entity.TypeID;
                
                if (entity == _unarmedLevelPistols)
                {
                    do
                    {
                        entity.TypeID = (short)stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!TR1EntityUtilities.IsWeaponPickup(currentType));
                }
                else if (TR1EntityUtilities.IsStandardPickupType(currentType))
                {
                    entity.TypeID = (short)stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                }
            }
        }

        public void EnforceOneLimit(TR1CombinedLevel level)
        {
            if (level.IsAssault)
            {
                return;
            }

            List<TREntities> oneOfEachType = new List<TREntities>();
            List<TREntity> allEntities = _levelInstance.Data.Entities.ToList();

            // look for extra utility/ammo items and hide them
            for (int i = 0; i < allEntities.Count; i++)
            {
                if (_secretMapping.RewardEntities.Contains(i))
                {
                    // Rewards excluded
                    continue;
                }

                TREntity ent = allEntities[i];
                TREntities eType = (TREntities)ent.TypeID;

                if (TR1EntityUtilities.IsStandardPickupType(eType) ||
                    TR1EntityUtilities.IsCrystalPickup(eType))
                {
                    if (oneOfEachType.Contains(eType))
                    {
                        ItemUtilities.HideEntity(ent);
                    }
                    else
                    {
                        oneOfEachType.Add((TREntities)ent.TypeID);
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

            List<Location> locations = GetItemLocationPool(level);

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
                    Location location = locations[_generator.Next(0, locations.Count)];
                    entity.X = location.X;
                    entity.Y = location.Y;
                    entity.Z = location.Z;
                    entity.Room = (short)location.Room;
                    entity.Angle = location.Angle;

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
                // Make sure to exclude the reward room
                //exclusions.Add(new Location
                //{
                //    Room = RoomWaterUtilities.DefaultRoomCountDictionary[level.Name],
                //    InvalidatesRoom = true
                //});
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

            foreach (TREntity entity in level.Data.Entities)
            {
                TREntities type = (TREntities)entity.TypeID;
                if (!TR1EntityUtilities.IsKeyItemType(type))
                {
                    continue;
                }

                // Only move a key item if there is at least one location defined for it. Any triggers below the
                // item will be handled by default environment mods.
                int itemID = 10000 + ((level.Script.OriginalSequence - 1) * 1000) + entity.TypeID + entity.Room;
                List<Location> pool = locations.FindAll(l => l.KeyItemGroupID == itemID);
                if (pool.Count > 0)
                {
                    Location location = pool[_generator.Next(0, pool.Count)];
                    entity.X = location.X;
                    entity.Y = location.Y;
                    entity.Z = location.Z;
                    entity.Room = (short)location.Room;
                }
            }
        }
    }
}