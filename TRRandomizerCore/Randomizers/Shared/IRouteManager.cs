using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public interface IRouteManager
{
    int LevelSize { get; }
    int GetProximity(Location locationA, Location locationB);
}
