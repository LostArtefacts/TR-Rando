using Newtonsoft.Json;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public abstract class EnemyAllocator<T>
    where T : Enum
{
    protected Dictionary<T, List<string>> _gameEnemyTracker;
    protected Dictionary<string, List<Location>> _relocations;
    protected List<T> _excludedEnemies;
    protected HashSet<T> _resultantEnemies;

    public RandomizerSettings Settings { get; set; }
    public Random Generator { get; set; }
    public IEnumerable<string> GameLevels { get; set; }

    public EnemyAllocator(TRGameVersion version)
    {
        string relocFile = $"Resources/{version}/Locations/enemy_relocations.json";
        _relocations = File.Exists(relocFile)
            ? JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(relocFile))
            : new();
    }

    public void Initialise()
    {
        _resultantEnemies = new();
        _gameEnemyTracker = GetGameTracker();
        _excludedEnemies = Settings.UseEnemyExclusions
            ? new(Settings.ExcludedEnemies.Select(s => (T)(object)(uint)s))
            : new();
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

    protected T SelectRequiredEnemy(List<T> pool, string levelName, RandoDifficulty difficulty)
    {
        pool.RemoveAll(t => !IsEnemySupported(levelName, t, difficulty));

        if (pool.All(_excludedEnemies.Contains))
        {
            // Select the last excluded enemy (lowest priority)
            return _excludedEnemies.Last(pool.Contains);
        }
        
        T type;
        do
        {
            type = pool[Generator.Next(0, pool.Count)];
        }
        while (_excludedEnemies.Contains(type));

        return type;
    }

    protected RandoDifficulty GetImpliedDifficulty()
    {
        if (_excludedEnemies.Count > 0 && Settings.RandoEnemyDifficulty == RandoDifficulty.Default)
        {
            // If every enemy in the pool has room restrictions for any level, we have to imply NoRestrictions difficulty mode
            List<T> includedEnemies = Settings.ExcludableEnemies.Keys.Except(Settings.ExcludedEnemies).Select(s => (T)(object)(uint)s).ToList();
            foreach (string level in GameLevels)
            {
                IEnumerable<T> restrictedRoomEnemies = GetRestrictedRooms(level.ToUpper(), RandoDifficulty.Default).Keys;
                if (includedEnemies.All(e => restrictedRoomEnemies.Contains(e) || _gameEnemyTracker.ContainsKey(e)))
                {
                    return RandoDifficulty.NoRestrictions;
                }
            }
        }
        return Settings.RandoEnemyDifficulty;
    }

    protected void SetOneShot<E>(E entity, int index, FDControl floorData)
        where E : TREntity<T>
    {
        if (!IsOneShotType(entity.TypeID))
        {
            return;
        }

        floorData.GetEntityTriggers(index)
            .ForEach(t => t.OneShot = true);
    }

    protected void RelocateEnemies<E>(string levelName, List<E> entities)
        where E : TREntity<T>
    {
        if (!Settings.RelocateAwkwardEnemies || !_relocations.ContainsKey(levelName))
        {
            return;
        }

        foreach (Location location in _relocations[levelName])
        {
            E enemy = entities[location.EntityIndex];
            if (EqualityComparer<T>.Default.Equals(enemy.TypeID, (T)(object)(uint)location.TargetType))
            {
                enemy.SetLocation(location);
                enemy.X++; // Avoid shifted enemies picking anything up
            }
        }
    }

    protected abstract Dictionary<T, List<string>> GetGameTracker();
    protected abstract bool IsEnemySupported(string levelName, T type, RandoDifficulty difficulty);
    protected abstract Dictionary<T, List<int>> GetRestrictedRooms(string levelName, RandoDifficulty difficulty);
    protected abstract bool IsOneShotType(T type);
}
