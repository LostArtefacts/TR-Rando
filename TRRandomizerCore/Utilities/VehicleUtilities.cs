using Newtonsoft.Json;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRLevelControl.Model;
using TRLevelControl.Helpers;

namespace TRRandomizerCore.Utilities;

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
    /// Get a random locaiton for the specific vehicle while checking if a working vehicle is actually required by secrets
    /// </summary>
    /// <param name="level">The level <see cref="TR2CombinedLevel"/></param>
    /// <param name="vehicle">The vehicle type <see cref="TR2Type"/></param>
    /// <param name="random">The random generator</param>
    /// <param name="testSecrets">True by default to check if a vehicle is required for a secret; false indicates a second "spare" vehicle as in BOAT.TR2</param>
    /// <returns></returns>
    public static Location GetRandomLocation(TR2CombinedLevel level, TR2Type vehicle, Random random, bool testSecrets = true)
    {
        if (_allVehicleLocations.ContainsKey(level.Name))
        {
            short vehicleID = (short)vehicle;

            bool vehicleRequired = testSecrets && IsVehicleRequired(level);

            List<Location> vehicleLocations = _allVehicleLocations[level.Name]
                .FindAll(l => l.TargetType == vehicleID && (!vehicleRequired || l.Validated));

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
        List<TR2Type> secretTypes = TR2EntityUtilities.GetListOfSecretTypes();

        foreach (TR2Entity entity in level.Data.Entities)
        {
            if (secretTypes.Contains((TR2Type)entity.TypeID))
            {
                Location usedlocation = levelLocations
                    .Find(l => l.X == entity.X && l.Y == entity.Y && l.Z == entity.Z && l.Room == entity.Room && l.VehicleRequired);
                if (usedlocation != null)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
