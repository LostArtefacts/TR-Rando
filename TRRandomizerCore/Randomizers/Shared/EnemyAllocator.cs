using TRRandomizerCore.Editors;

namespace TRRandomizerCore.Randomizers;

public abstract class EnemyAllocator<T>
    where T : Enum
{
    protected Dictionary<T, List<string>> _gameEnemyTracker;
    protected List<T> _excludedEnemies;
    protected HashSet<T> _resultantEnemies;

    public RandomizerSettings Settings { get; set; }
    public Random Generator { get; set; }
    public IEnumerable<string> GameLevels { get; set; }

    public void Initialise()
    {
        // Track enemies whose counts across the game are restricted
        _gameEnemyTracker = GetGameTracker();

        // #272 Selective enemy pool - convert the shorts in the settings to actual entity types
        _excludedEnemies = Settings.UseEnemyExclusions
            ? Settings.ExcludedEnemies.Select(s => (T)(object)(uint)s).ToList()
            : new();
        _resultantEnemies = new();
    }

    public string GetExclusionStatusMessage()
    {
        if (!Settings.ShowExclusionWarnings)
        {
            return null;
        }

        IEnumerable<T> failedExclusions = _resultantEnemies.Where(_excludedEnemies.Contains);
        if (failedExclusions.Any())
        {
            List<string> failureNames = failedExclusions.Select(f => Settings.ExcludableEnemies[(short)(uint)(object)f]).ToList();
            failureNames.Sort();
            return string.Format("The following enemies could not be excluded entirely from the randomization pool.{0}{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, failureNames));
        }

        return null;
    }

    protected abstract Dictionary<T, List<string>> GetGameTracker();
}
