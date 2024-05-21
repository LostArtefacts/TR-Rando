using Newtonsoft.Json;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities;

public static class VehicleUtilities
{
    private static readonly Dictionary<string, List<Location>> _vehicleLocations;
    private static readonly Dictionary<string, List<Location>> _secretLocations;

    static VehicleUtilities()
    {
        _vehicleLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR2\Locations\vehicle_locations.json"));
        _secretLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR2\Locations\locations.json"));
    }

    public static Location GetRandomLocation(string levelName, TR2Level level, TR2Type vehicle, Random random, bool testSecrets = true)
    {
        if (_vehicleLocations.ContainsKey(levelName))
        {
            short vehicleID = (short)vehicle;
            if (testSecrets)
            {
                IEnumerable<Location> dependencies = GetDependentLocations(levelName, level);
                if (dependencies.Any(l => l.TargetType == vehicleID))
                {
                    // Vehicles that have secrets dependent on their OG positions will not be moved.
                    return null;
                }
            }

            List<Location> vehicleLocations = _vehicleLocations[levelName]
                .FindAll(l => l.TargetType == vehicleID);
            if (vehicleLocations.Count > 0)
            {
                return vehicleLocations[random.Next(0, vehicleLocations.Count)];
            }
        }

        return null;
    }

    public static IEnumerable<Location> GetDependentLocations(string levelName, TR2Level level)
    {
        if (!_secretLocations.ContainsKey(levelName))
        {
            return Array.Empty<Location>();
        }

        IEnumerable<Location> levelLocations = _secretLocations[levelName].Where(l => l.VehicleRequired);
        IEnumerable<TR2Entity> secrets = level.Entities.Where(e => TR2TypeUtilities.IsSecretType(e.TypeID));

        return levelLocations
            .Where(l => secrets.Any(s => l.X == s.X && l.Y == s.Y && l.Z == s.Z && l.Room == s.Room));
    }
}
