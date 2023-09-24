using Newtonsoft.Json;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRLevelControl.Model;
using TRLevelControl.Helpers;

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

    public static Location GetRandomLocation(TR2CombinedLevel level, TR2Type vehicle, Random random, bool testSecrets = true)
    {
        if (_vehicleLocations.ContainsKey(level.Name))
        {
            short vehicleID = (short)vehicle;
            if (testSecrets)
            {
                IEnumerable<Location> dependencies = GetDependentLocations(level);
                if (dependencies.Any(l => l.TargetType == vehicleID))
                {
                    // Vehicles that have secrets dependent on their OG positions will not be moved.
                    return null;
                }
            }

            List<Location> vehicleLocations = _vehicleLocations[level.Name]
                .FindAll(l => l.TargetType == vehicleID);
            if (vehicleLocations.Count > 0)
            {
                return vehicleLocations[random.Next(0, vehicleLocations.Count)];
            }
        }

        return null;
    }

    public static IEnumerable<Location> GetDependentLocations(TR2CombinedLevel level)
    {
        if (!_secretLocations.ContainsKey(level.Name))
        {
            return Array.Empty<Location>();
        }

        IEnumerable<Location> levelLocations = _secretLocations[level.Name].Where(l => l.VehicleRequired);
        IEnumerable<TR2Entity> secrets = level.Data.Entities.Where(e => TR2TypeUtilities.IsSecretType(e.TypeID));

        return levelLocations
            .Where(l => secrets.Any(s => l.X == s.X && l.Y == s.Y && l.Z == s.Z && l.Room == s.Room));
    }
}
