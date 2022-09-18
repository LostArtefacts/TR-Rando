using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TRRandomizerCore.Helpers;
using TRLevelReader.Model.Enums;
using System.Linq;
using TRRandomizerCore.Levels;
using TRLevelReader.Model;

namespace TRRandomizerCore.Utilities
{
    public static class VehicleUtilities
    {
        private static readonly Dictionary<string, List<Location>> _allVehicleLocations;
        private static readonly Dictionary<string, List<Location>> _allLocations;

        static VehicleUtilities()
        {
            _allVehicleLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR2\Locations\vehicle_locations.json"));
            _allLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR2\Locations\locations.json"));
        }

        /// <summary>
        /// Get a random locaiton for the specific vehicule while checking if a working vehicule is actually required by secrets
        /// </summary>
        /// <param name="level">The level <see cref="TR2CombinedLevel"/></param>
        /// <param name="vehicle">The vehicule type <see cref="TR2Entities"/></param>
        /// <param name="random">The random generator</param>
        /// <param name="VehiculeRestriction">True by default to check for vehicule required, if false...we don't care (used for 2nd boat in BOAT.TR2)</param>
        /// <returns></returns>
        public static Location GetRandomLocation(TR2CombinedLevel level, TR2Entities vehicle, Random random, bool VehiculeRestriction = true)
        {
            if (_allVehicleLocations.ContainsKey(level.Name))
            {
                short vehicleID = (short)vehicle;

                bool vehicleRequired = VehiculeRestriction ? IsVehicleRequired(level) : false;

                List<Location> vehicleLocations = _allVehicleLocations[level.Name].FindAll(l => l.TargetType == vehicleID && (!vehicleRequired || (vehicleRequired && l.Validated == true)));
                //List<Location> vehicleLocations = _allVehicleLocations[level.Name].FindAll(l => l.TargetType == vehicleID && l.VehicleRequired == true);
                if (vehicleLocations.Count > 0)
                {
                    return vehicleLocations[random.Next(0, vehicleLocations.Count)];
                }
            }

            return null;
        }

        /// <summary>
        /// Function to check if a specific level has at least one secret requiring vehicle
        /// </summary>
        /// <param name="levelName"></param>
        /// <returns>True if vehicle is required</returns>
        public static bool IsVehicleRequired(TR2CombinedLevel level)
        {

            if (!_allLocations.ContainsKey(level.Name))
            {
                return false;
            }

            List<Location> levelLocations = _allLocations[level.Name];
            List<TR2Entities> secretTypes = new List<TR2Entities> { TR2Entities.StoneSecret_S_P, TR2Entities.JadeSecret_S_P, TR2Entities.GoldSecret_S_P };

            foreach (TR2Entity entity in level.Data.Entities)
            {
                if (secretTypes.Contains((TR2Entities)entity.TypeID))
                {
                    Location usedlocation = levelLocations.Find(l => l.X == entity.X && l.Y == entity.Y && l.Z == entity.Z && l.Room == entity.Room && l.VehicleRequired == true);
                    if (usedlocation != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}