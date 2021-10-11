using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TRRandomizerCore.Helpers;
using TRLevelReader.Model.Enums;

namespace TRRandomizerCore.Utilities
{
    public static class VehicleUtilities
    {
        private static readonly Dictionary<string, Dictionary<TR2Entities, List<Location>>> _allVehicleLocations;
        static VehicleUtilities()
        {
            _allVehicleLocations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<TR2Entities, List<Location>>>>(File.ReadAllText(@"Resources\TR2\Locations\vehicle_locations.json"));
        }

        public static Location GetRandomLocation(string levelName, TR2Entities vehicle, Random random)
        {
            if (_allVehicleLocations.ContainsKey(levelName) && _allVehicleLocations[levelName].ContainsKey(vehicle))
            {
                // we will only spawn one skidoo, so only need one random location
                List<Location> levelLocations = _allVehicleLocations[levelName][vehicle];
                return levelLocations[random.Next(0, levelLocations.Count)];
            }

            return null;
        }
    }
}