using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TRFDControl;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers
{
    public class TR3ItemRandomizer : BaseTR3Randomizer
    {
        private Dictionary<string, LevelPickupZoneDescriptor> _keyItemZones;
        private Dictionary<string, List<Location>> _locations;
        private Dictionary<string, List<Location>> _excludedLocations;
        private Dictionary<string, List<Location>> _pistolLocations;

        private static readonly int _ROTATION = -8192;
        private static readonly int _ANY_ROOM_ALLOWED = 2048;  //Max rooms is 1024 - so this should never be possible.

        // Track the pistols so they remain a weapon type and aren't moved
        private TR2Entity _unarmedLevelPistols;

        public ItemFactory ItemFactory { get; set; }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            _keyItemZones = JsonConvert.DeserializeObject<Dictionary<string, LevelPickupZoneDescriptor>>(ReadResource(@"TR3\Locations\Zones.json"));
            _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\item_locations.json"));
            _excludedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\invalid_item_locations.json"));
            _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\unarmed_locations.json"));

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                //Replace with UI option in future
                Settings.RandomizeItemTypes = true;
                Settings.RandomizeItemPositions = true;

                LoadLevelInstance(lvl);

                FindUnarmedLevelPistols(_levelInstance);

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

        private void FindUnarmedLevelPistols(TR3CombinedLevel level)
        {
            if (level.Script.RemovesWeapons)
            {
                List<TR2Entity> pistolEntities = level.Data.Entities.ToList().FindAll(e => e.TypeID == (short)TR3Entities.Pistols_P);
                foreach (TR2Entity pistols in pistolEntities)
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
            List<TR3Entities> stdItemTypes = TR3EntityUtilities.GetStandardPickupTypes();

            foreach (TR2Entity ent in level.Data.Entities)
            {
                // If this is an unarmed level's pistols, make sure they're replaced with another weapon
                if (ent == _unarmedLevelPistols)
                {
                    do
                    {
                        ent.TypeID = (short)stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                    }
                    while (!TR3EntityUtilities.IsWeaponPickup((TR3Entities)ent.TypeID));
                }
                else if (TR3EntityUtilities.IsStandardPickupType((TR3Entities)ent.TypeID) && 
                    (ent.Room < RoomWaterUtilities.DefaultRoomCountDictionary[level.Name] || Settings.RandomizeSecretRewardsPhysical))
                {
                    ent.TypeID = (short)stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
                }
            }
        }

        public void EnforceOneLimit(TR3CombinedLevel level)
        {
            List<TR3Entities> oneOfEachType = new List<TR3Entities>();
            List<TR2Entity> allEntities = _levelInstance.Data.Entities.ToList();

            // FD for removing crystal triggers if applicable.
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            // look for extra utility/ammo items and hide them
            for (int i = 0; i < allEntities.Count; i++)
            {
                TR2Entity ent = allEntities[i];
                TR3Entities eType = (TR3Entities)ent.TypeID;

                if (TR3EntityUtilities.IsStandardPickupType(eType) ||
                    TR3EntityUtilities.IsCrystalPickup(eType))
                {
                    if (oneOfEachType.Contains(eType))
                    {
                        ItemUtilities.HideEntity(ent);
                        ItemFactory.FreeItem(level.Name, i);
                        if (TR3EntityUtilities.IsCrystalPickup(eType))
                        {
                            FDUtilities.RemoveEntityTriggers(level.Data, i, floorData);
                        }
                    }
                    else
                    {
                        oneOfEachType.Add((TR3Entities)ent.TypeID);
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

            List<Location> locations = GetItemLocationPool(level);

            foreach (TR2Entity ent in level.Data.Entities)
            {
                // Move standard items only, excluding any unarmed level pistols, and reward items
                if (TR3EntityUtilities.IsStandardPickupType((TR3Entities)ent.TypeID)
                    && ent != _unarmedLevelPistols
                    && ent.Room < RoomWaterUtilities.DefaultRoomCountDictionary[level.Name])
                {
                    Location location = locations[_generator.Next(0, locations.Count)];
                    ent.X = location.X;
                    ent.Y = location.Y;
                    ent.Z = location.Z;
                    ent.Room = (short)location.Room;
                    ent.Angle = location.Angle;

                    // Anything other than -1 means a sloped sector and so the location generator
                    // will have picked a suitable angle for it. For flat sectors, spin the entities
                    // around randomly for variety.
                    if (ent.Angle == -1)
                    {
                        ent.Angle = (short)(_generator.Next(0, 8) * _ROTATION);
                    }
                }
            }
        }

        private List<Location> GetItemLocationPool(TR3CombinedLevel level)
        {
            List<Location> exclusions = new List<Location>();
            if (_excludedLocations.ContainsKey(level.Name))
            {
                exclusions.AddRange(_excludedLocations[level.Name]);
            }

            foreach (TR2Entity entity in level.Data.Entities)
            {
                if (!TR3EntityUtilities.CanSharePickupSpace((TR3Entities)entity.TypeID))
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

            if (Settings.RandomizeSecrets && level.HasSecrets)
            {
                // Make sure to exclude the reward room
                exclusions.Add(new Location
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
                        exclusions.Add(new Location
                        {
                            Room = i,
                            InvalidatesRoom = true
                        });
                    }
                }
            }

            LocationGenerator generator = new LocationGenerator();
            return generator.Generate(level.Data, exclusions);
        }

        public void RandomizeKeyItems(TR3CombinedLevel level)
        {
            if (level.Name != TR3LevelNames.ASSAULT)
            {
                //Get all locations that have a KeyItemGroupID - e.g. intended for key items
                List<Location> levelLocations = _locations[level.Name].Where(i => i.KeyItemGroupID != 0).ToList();

                foreach (TR2Entity ent in level.Data.Entities)
                {
                    //Calculate its alias
                    TR3Entities AliasedKeyItemID = (TR3Entities)(ent.TypeID + ent.Room + GetLevelKeyItemBaseAlias(level.Name));

                    //Any special handling for key item entities
                    switch (AliasedKeyItemID)
                    {
                        case TR3Entities.DetonatorKey:
                            ent.Invisible = false;
                            break;
                        default:
                            break;
                    }

                    if (AliasedKeyItemID < TR3Entities.JungleKeyItemBase)
                        throw new NotSupportedException("Level does not have key item alias group defined");

                    //Is entity one we are allowed/expected to move? (is the alias and base type correct?)
                    if (_keyItemZones[level.Name].AliasedExpectedKeyItems.Contains(AliasedKeyItemID) &&
                        _keyItemZones[level.Name].BaseExpectedKeyItems.Contains((TR3Entities)ent.TypeID))
                    {
                        do
                        {
                            //Only get locations that are to position the intended key item.
                            //We can probably get rid of the do while loop as any location in this list should be valid
                            List<Location> KeyItemLocations = levelLocations.Where(i => i.KeyItemGroupID == (int)AliasedKeyItemID).ToList();
                                
                            Location loc = KeyItemLocations[_generator.Next(0, KeyItemLocations.Count)];

                            ent.X = loc.X;
                            ent.Y = loc.Y;
                            ent.Z = loc.Z;
                            ent.Room = (short)loc.Room;

                        } while (!_keyItemZones[level.Name].AllowedRooms[AliasedKeyItemID].Contains(ent.Room) &&
                                (!_keyItemZones[level.Name].AllowedRooms[AliasedKeyItemID].Contains(_ANY_ROOM_ALLOWED)));
                        //Try generating locations until it is in the zone - if list contains 2048 then any room is allowed.
                    }
                }
            }
        }

        private int GetLevelKeyItemBaseAlias(string name)
        {
            switch (name)
            {
                case TR3LevelNames.JUNGLE:
                    return (int)TR3Entities.JungleKeyItemBase;
                case TR3LevelNames.RUINS:
                    return (int)TR3Entities.TempleKeyItemBase;
                case TR3LevelNames.GANGES:
                    return (int)TR3Entities.GangesKeyItemBase;
                case TR3LevelNames.CAVES:
                    return (int)TR3Entities.KaliyaKeyItemBase;
                case TR3LevelNames.NEVADA:
                    return (int)TR3Entities.NevadaKeyItemBase;
                case TR3LevelNames.HSC:
                    return (int)TR3Entities.HSCKeyItemBase;
                case TR3LevelNames.AREA51:
                    return (int)TR3Entities.Area51KeyItemBase;
                case TR3LevelNames.COASTAL:
                    return (int)TR3Entities.CoastalKeyItemBase;
                case TR3LevelNames.CRASH:
                    return (int)TR3Entities.CrashKeyItemBase;
                case TR3LevelNames.MADUBU:
                    return (int)TR3Entities.MadubuKeyItemBase;
                case TR3LevelNames.PUNA:
                    return (int)TR3Entities.PunaKeyItemBase;
                case TR3LevelNames.THAMES:
                    return (int)TR3Entities.ThamesKeyItemBase;
                case TR3LevelNames.ALDWYCH:
                    return (int)TR3Entities.AldwychKeyItemBase;
                case TR3LevelNames.LUDS:
                    return (int)TR3Entities.LudsKeyItemBase;
                case TR3LevelNames.CITY:
                    return (int)TR3Entities.CityKeyItemBase;
                case TR3LevelNames.ANTARC:
                    return (int)TR3Entities.AntarcticaKeyItemBase;
                case TR3LevelNames.RXTECH:
                    return (int)TR3Entities.RXKeyItemBase;
                case TR3LevelNames.TINNOS:
                    return (int)TR3Entities.TinnosKeyItemBase;
                case TR3LevelNames.WILLIE:
                    return (int)TR3Entities.CavernKeyItemBase;
                case TR3LevelNames.HALLOWS:
                    return (int)TR3Entities.HallowsKeyItemBase;
            }

            return 0;
        }
    }
}
