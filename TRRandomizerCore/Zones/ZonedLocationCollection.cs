using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Zones
{
    class ZonedLocationCollection
    {
        private readonly Dictionary<int, List<Location>> _zoneAppliedLocations;
        private int _numGeneralZones;

        public List<Location> StoneZone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.StoneSecretZone);
            }
        }

        public List<Location> JadeZone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.JadeSecretZone);
            }
        }

        public List<Location> GoldZone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.GoldSecretZone);
            }
        }

        public List<Location> Key1Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Key1Zone);
            }
        }

        public List<Location> Key2Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Key2Zone);
            }
        }

        public List<Location> Key3Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Key3Zone);
            }
        }

        public List<Location> Key4Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Key4Zone);
            }
        }

        public List<Location> Puzzle1Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Puzzle1Zone);
            }
        }

        public List<Location> Puzzle2Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Puzzle2Zone);
            }
        }

        public List<Location> Puzzle3Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Puzzle3Zone);
            }
        }

        public List<Location> Puzzle4Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Puzzle4Zone);
            }
        }

        public List<Location> Quest1Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Quest1Zone);
            }
        }

        public List<Location> Quest2Zone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.Quest2Zone);
            }
        }

        public List<Location> GeneralZone
        {
            get
            {
                return GetZoneLocations((int)LevelZones.ItemZones);
            }
        }

        public int NumGeneralZones
        {
            get
            {
                return _numGeneralZones;
            }
        }

        public List<Location> GetZoneLocations(int zone)
        {
            if (_zoneAppliedLocations.ContainsKey(zone))
            {
                return _zoneAppliedLocations[zone];
            }

            return null;
        }

        /// <summary>
        /// Populate <see cref="_zoneAppliedLocations"/>  to get for each secret or key item index the list of locations they are set to appear into in both json for Zone and location in this specific level
        /// </summary>
        /// <param name="zoneFilePath">path of the Zone file for the level @"TR2\Zones\" + _levelInstance.Name + "-Zones.json"</param>
        /// <param name="locations">locations.json serialized</param>
        /// <param name="popMethod">SecretsOnly;KeyPuzzleQuestOnly; are the only ones used today</param>
        public void PopulateZones(string zoneFilePath, List<Location> locations, ZonePopulationMethod popMethod)
        {
            Dictionary<int, List<int>> ZoneMap = JsonConvert.DeserializeObject<Dictionary<int, List<int>>>(File.ReadAllText(zoneFilePath));

            if (popMethod == ZonePopulationMethod.SecretsOnly || popMethod == ZonePopulationMethod.Full)
            {
                _zoneAppliedLocations.Add((int)LevelZones.StoneSecretZone, (from loc in locations
                                                                            where ZoneMap[(int)LevelZones.StoneSecretZone].Contains(loc.Room)
                                                                            select loc).ToList());
                // HSH is not zoned... file is empty... so each secret can go everywhere
                if (!DoesZoneHaveLocations((int)LevelZones.StoneSecretZone))
                    _zoneAppliedLocations[(int)LevelZones.StoneSecretZone] = locations;

                _zoneAppliedLocations.Add((int)LevelZones.JadeSecretZone, (from loc in locations
                                                                           where ZoneMap[(int)LevelZones.JadeSecretZone].Contains(loc.Room)
                                                                           select loc).ToList());

                if (!DoesZoneHaveLocations((int)LevelZones.JadeSecretZone))
                    _zoneAppliedLocations[(int)LevelZones.JadeSecretZone] = locations;

                _zoneAppliedLocations.Add((int)LevelZones.GoldSecretZone, (from loc in locations
                                                                           where ZoneMap[(int)LevelZones.GoldSecretZone].Contains(loc.Room)
                                                                           select loc).ToList());

                if (!DoesZoneHaveLocations((int)LevelZones.GoldSecretZone))
                    _zoneAppliedLocations[(int)LevelZones.GoldSecretZone] = locations;
            }

            if (popMethod == ZonePopulationMethod.KeyPuzzleQuestOnly || popMethod == ZonePopulationMethod.Full)
            {
                //We don't fall back to populating with all locations for these as if there is no set defined we don't want to open
                //up the full pool otherwise softlock city - Danza.

                _zoneAppliedLocations.Add((int)LevelZones.Key1Zone, (from loc in locations
                                                                     where ZoneMap[(int)LevelZones.Key1Zone].Contains(loc.Room)
                                                                     select loc).ToList());


                _zoneAppliedLocations.Add((int)LevelZones.Key2Zone, (from loc in locations
                                                                     where ZoneMap[(int)LevelZones.Key2Zone].Contains(loc.Room)
                                                                     select loc).ToList());


                _zoneAppliedLocations.Add((int)LevelZones.Key3Zone, (from loc in locations
                                                                     where ZoneMap[(int)LevelZones.Key3Zone].Contains(loc.Room)
                                                                     select loc).ToList());


                _zoneAppliedLocations.Add((int)LevelZones.Key4Zone, (from loc in locations
                                                                     where ZoneMap[(int)LevelZones.Key4Zone].Contains(loc.Room)
                                                                     select loc).ToList());


                _zoneAppliedLocations.Add((int)LevelZones.Puzzle1Zone, (from loc in locations
                                                                        where ZoneMap[(int)LevelZones.Puzzle1Zone].Contains(loc.Room)
                                                                        select loc).ToList());


                _zoneAppliedLocations.Add((int)LevelZones.Puzzle2Zone, (from loc in locations
                                                                        where ZoneMap[(int)LevelZones.Puzzle2Zone].Contains(loc.Room)
                                                                        select loc).ToList());

                _zoneAppliedLocations.Add((int)LevelZones.Puzzle3Zone, (from loc in locations
                                                                        where ZoneMap[(int)LevelZones.Puzzle3Zone].Contains(loc.Room)
                                                                        select loc).ToList());


                _zoneAppliedLocations.Add((int)LevelZones.Puzzle4Zone, (from loc in locations
                                                                        where ZoneMap[(int)LevelZones.Puzzle4Zone].Contains(loc.Room)
                                                                        select loc).ToList());


                _zoneAppliedLocations.Add((int)LevelZones.Quest1Zone, (from loc in locations
                                                                       where ZoneMap[(int)LevelZones.Quest1Zone].Contains(loc.Room)
                                                                       select loc).ToList());


                _zoneAppliedLocations.Add((int)LevelZones.Quest2Zone, (from loc in locations
                                                                       where ZoneMap[(int)LevelZones.Quest2Zone].Contains(loc.Room)
                                                                       select loc).ToList());

            }

            if (popMethod == ZonePopulationMethod.GeneralOnly || popMethod == ZonePopulationMethod.Full)
            {
                _zoneAppliedLocations.Add((int)LevelZones.ItemZones, (from loc in locations
                                                                      where ZoneMap[(int)LevelZones.ItemZones].Contains(loc.Room)
                                                                      select loc).ToList());

                //Only bother looking for additional zones if the first general zone contains any
                if (DoesZoneHaveLocations((int)LevelZones.ItemZones))
                {
                    bool clearLast = false;

                    for (int i = ((int)LevelZones.ItemZones + 1); i < ZoneMap.Count; i++)
                    {
                        _zoneAppliedLocations.Add(i, (from loc in locations
                                                      where ZoneMap[i].Contains(loc.Room)
                                                      select loc).ToList());

                        if (!DoesZoneHaveLocations(i))
                        {
                            //If latest zone doesnt have any locations break and stop.
                            //Then remove the zone from the dictionary.
                            clearLast = true;

                            break;
                        }

                        //If we have a new general zone with actual locations increase the count
                        _numGeneralZones++;
                    }

                    if (clearLast)
                        _zoneAppliedLocations.Remove(ZoneMap.Count - 1);
                }
                else
                {
                    // General item zone was empty so open up the full pool
                    _zoneAppliedLocations[(int)LevelZones.ItemZones] = locations;
                }
            }
        }

        public ZonedLocationCollection()
        {
            _zoneAppliedLocations = new Dictionary<int, List<Location>>();
            _numGeneralZones = 1;
        }

        private bool DoesZoneHaveLocations(int zone)
        {
            if (_zoneAppliedLocations.ContainsKey(zone))
            {
                if (_zoneAppliedLocations[zone].Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}