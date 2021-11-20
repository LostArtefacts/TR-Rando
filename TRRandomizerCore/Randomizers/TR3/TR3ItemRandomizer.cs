using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly int _ANY_ROOM_ALLOWED = 2048;  //Max rooms is 1024 - so this should never be possible.

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            _keyItemZones = JsonConvert.DeserializeObject<Dictionary<string, LevelPickupZoneDescriptor>>(ReadResource(@"TR3\Locations\Zones.json"));
            _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\item_locations.json"));

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                //Replace with UI option in future
                Settings.RandomizeItemTypes = true;
                Settings.RandomizeItemPositions = true;

                LoadLevelInstance(lvl);

                if (Settings.RandomizeItemTypes)
                    RandomizeItemTypes(_levelInstance);

                if (Settings.RandomizeItemPositions)
                    RandomizeItemLocations(_levelInstance);

                if (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
                    EnforceOneLimit(_levelInstance);

                if (Settings.IncludeKeyItems)
                    RandomizeKeyItems(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        public void RandomizeItemTypes(TR3CombinedLevel level)
        {
            List<TR3Entities> stdItemTypes = TR3EntityUtilities.GetStandardPickupTypes();

            foreach (TR2Entity ent in level.Data.Entities)
            {
                if (TR3EntityUtilities.IsStandardPickupType((TR3Entities)ent.TypeID) && 
                    (ent.Room < RoomWaterUtilities.DefaultRoomCountDictionary[level.Name] || Settings.RandomizeSecretRewardsPhysical))
                {
                    ent.TypeID = (short)stdItemTypes[_generator.Next(0, (stdItemTypes.Count - 1))];
                }
            }
        }

        public void EnforceOneLimit(TR3CombinedLevel level)
        {
            List<TR3Entities> oneOfEachType = new List<TR3Entities>();
            List<TR2Entity> allEntities = _levelInstance.Data.Entities.ToList();

            // look for extra utility/ammo items and hide them
            foreach (TR2Entity ent in allEntities)
            {
                TR3Entities eType = (TR3Entities)ent.TypeID;

                if (TR3EntityUtilities.IsStandardPickupType(eType) ||
                    TR3EntityUtilities.IsCrystalPickup(eType))
                {
                    if (oneOfEachType.Contains(eType))
                    {
                        ItemUtilities.HideEntity(ent);
                    }
                    else
                    {
                        oneOfEachType.Add((TR3Entities)ent.TypeID);
                    }     
                }
            }
        }

        public void RandomizeItemLocations(TR3CombinedLevel level)
        {
            //Future - still thinking of an automated approach for this rather than defining locations.
        }

        public void RandomizeKeyItems(TR3CombinedLevel level)
        {
            List<Location> levelLocations = _locations[level.Name];

            foreach (TR2Entity ent in level.Data.Entities)
            {
                //Calculate its alias
                TR3Entities AliasedKeyItemID = (TR3Entities)(ent.TypeID + ent.Room + GetLevelKeyItemBaseAlias(level.Name));

                if (AliasedKeyItemID < TR3Entities.JungleKeyItemBase)
                    throw new NotSupportedException("Level does not have key item alias group defined");

                //Is entity one we are allowed/expected to move?
                if (_keyItemZones[level.Name].ExpectedKeyItems.Contains(AliasedKeyItemID))
                {
                    do
                    {
                        Location loc = levelLocations[_generator.Next(0, levelLocations.Count - 1)];

                        ent.X = loc.X;
                        ent.Y = loc.Y;
                        ent.Z = loc.Z;
                        ent.Room = (short)loc.Room;

                    } while (!_keyItemZones[level.Name].AllowedRooms[AliasedKeyItemID].Contains(ent.Room) ||
                            (!_keyItemZones[level.Name].AllowedRooms[AliasedKeyItemID].Contains(_ANY_ROOM_ALLOWED)));
                    //Try generating locations until it is in the zone - if list contains 2048 then any room is allowed.
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
