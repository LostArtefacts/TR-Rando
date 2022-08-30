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
        private static readonly Dictionary<string, List<Location>> _allVehicleLocations;
        static VehicleUtilities()
        {
            _allVehicleLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR2\Locations\vehicle_locations.json"));
        }

        public static Location GetRandomLocation(string levelName, TR2Entities vehicle, Random random)
        {
            if (_allVehicleLocations.ContainsKey(levelName))
            {
                short vehicleID = (short)vehicle;
                List<Location> vehicleLocations = _allVehicleLocations[levelName].FindAll(l => l.TargetType == vehicleID);
                if (vehicleLocations.Count > 0)
                {
                    return vehicleLocations[random.Next(0, vehicleLocations.Count)];
                }
            }

            return null;
        }
    }
}