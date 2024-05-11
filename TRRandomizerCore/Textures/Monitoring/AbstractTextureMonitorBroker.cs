using TRImageControl.Textures;

namespace TRRandomizerCore.Textures;

public abstract class AbstractTextureMonitorBroker<T>
    where T : Enum
{
    private readonly Dictionary<string, TextureMonitor<T>> _monitors;
    private readonly TextureDatabase<T> _textureDatabase;
    private readonly object _lock;

    protected abstract Dictionary<T, T> ExpandedMonitorMap { get; }

    public AbstractTextureMonitorBroker()
    {
        _monitors = new();
        _textureDatabase = CreateDatabase();
        _lock = new();
    }

    protected abstract TextureDatabase<T> CreateDatabase();
    protected abstract T TranslateAlias(string lvlName, T type);

    public TextureMonitor<T> CreateMonitor(string lvlName, List<T> types = null)
    {
        lock (_lock)
        {
            types ??= new();
            List<StaticTextureSource<T>> sources = GetSourcesToMonitor(types);
            TextureMonitor<T> monitor = GetMonitor(lvlName);
            if (monitor == null)
            {
                monitor = new TextureMonitor<T>(sources);
                _monitors[lvlName] = monitor;
            }
            else
            {
                monitor.AppendSources(sources);
            }

            return monitor;
        }
    }

    public void ClearMonitor(string lvlName, List<T> types)
    {
        lock (_lock)
        {
            TextureMonitor<T> monitor = GetMonitor(lvlName);
            if (monitor == null)
            {
                return;
            }

            List<StaticTextureSource<T>> sources = GetSourcesToMonitor(types);
            monitor.RemoveSources(sources);
        }
    }

    private List<StaticTextureSource<T>> GetSourcesToMonitor(List<T> types)
    {
        List<T> expandedTypes = new(types);

        // We need to capture things like flames being imported into Boat, Opera, Skidoo and the fact that
        // the red Skidoo is available when importing MercSnomobDriver.
        if (ExpandedMonitorMap != null)
        {
            foreach (T type in ExpandedMonitorMap.Keys)
            {
                if (expandedTypes.Contains(type) && !expandedTypes.Contains(ExpandedMonitorMap[type]))
                {
                    expandedTypes.Add(ExpandedMonitorMap[type]);
                }
            }
        }

        List<StaticTextureSource<T>> sources = new();
        foreach (T type in expandedTypes)
        {
            foreach (StaticTextureSource<T> source in _textureDatabase.GetStaticSource(type))
            {
                // Does the source have any defined object texture indices we are interested in monitoring?
                if (source.EntityTextureMap != null && !sources.Contains(source))
                {
                    sources.Add(source);
                }
            }
        }

        return sources;
    }

    public TextureMonitor<T> GetMonitor(string lvlName)
    {
        return _monitors.ContainsKey(lvlName) ? _monitors[lvlName] : null;
    }

    public bool RemoveMonitor(string lvlName)
    {
        lock (_lock)
        {
            return _monitors.Remove(lvlName);
        }
    }

    public Dictionary<StaticTextureSource<T>, List<StaticTextureTarget>> GetLevelMapping(string lvlName)
    {
        TextureMonitor<T> monitor = GetMonitor(lvlName);
        return monitor?.PreparedLevelMapping;
    }

    public List<T> GetIgnoredTypes(string lvlName)
    {
        TextureMonitor<T> monitor = GetMonitor(lvlName);
        if (monitor != null && monitor.RemovedTextures != null)
        {
            List<T> types = new();
            foreach (T type in monitor.RemovedTextures)
            {
                types.Add(TranslateAlias(lvlName, type));
            }
            return types;
        }
        return null;
    }

    public Dictionary<T, T> GetTypeMap(string lvlName)
    {
        TextureMonitor<T> monitor = GetMonitor(lvlName);
        return monitor?.TypeMap;
    }
}
