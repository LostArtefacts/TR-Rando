using TRImageControl.Packing;
using TRImageControl.Textures;

namespace TRRandomizerCore.Textures;

public class TextureMonitor<T> : ITexturePositionMonitor<T>
    where T : Enum
{
    private readonly List<StaticTextureSource<T>> _typeSources;

    public Dictionary<StaticTextureSource<T>, List<StaticTextureTarget>> PreparedLevelMapping { get; private set; }
    public List<T> RemovedTextures { get; private set; }

    public bool UseMirroring { get; set; }
    public bool UseNightTextures { get; set; }
    public bool UseLaraOutfitTextures { get; set; }

    // Allow entities such as Artefacts to be defined in texture sources, but mapped to different types here
    public Dictionary<T, T> TypeMap { get; set; }

    public TextureMonitor(List<StaticTextureSource<T>> sources)
    {
        _typeSources = sources;
        TypeMap = new();
        UseLaraOutfitTextures = true;
    }

    public void AppendSources(IEnumerable<StaticTextureSource<T>> sources)
    {
        foreach (StaticTextureSource<T> source in sources)
        {
            if (!_typeSources.Contains(source))
            {
                _typeSources.Add(source);
            }
        }
    }

    public void RemoveSources(IEnumerable<StaticTextureSource<T>> sources)
    {
        foreach (StaticTextureSource<T> source in sources)
        {
            _typeSources.Remove(source);
            if (PreparedLevelMapping != null && PreparedLevelMapping.ContainsKey(source))
            {
                PreparedLevelMapping.Remove(source);
            }
        }
    }

    public Dictionary<T, List<int>> GetMonitoredIndices()
    {
        // The keys defined in the source ObjectTextureMap are TRObjectTexture index references
        // from the original level they were extracted from. We want to track what happens to
        // these textures.
        Dictionary<T, List<int>> indices = new();
        foreach (StaticTextureSource<T> source in _typeSources)
        {
            foreach (T type in source.EntityTextureMap.Keys)
            {
                if (!indices.ContainsKey(type))
                {
                    indices[type] = new List<int>();
                }
                indices[type].AddRange(source.EntityTextureMap[type].Keys); // The keys hold the texture indices, the values are the segment positions
            }
        }
        return indices;
    }

    public void OnTexturesPositioned(Dictionary<T, List<PositionedTexture>> texturePositions)
    {
        PreparedLevelMapping ??= new();

        foreach (var (type, positions) in texturePositions)
        {
            List<StaticTextureSource<T>> sources = GetSources(type);
            foreach (PositionedTexture texture in positions)
            {
                foreach (StaticTextureSource<T> source in sources)
                {
                    if (!source.EntityTextureMap[type].ContainsKey(texture.OriginalIndex))
                    {
                        continue;
                    }

                    if (!PreparedLevelMapping.ContainsKey(source))
                    {
                        PreparedLevelMapping[source] = new();
                    }
                    PreparedLevelMapping[source].Add(new()
                    {
                        Segment = source.EntityTextureMap[type][texture.OriginalIndex], // this points to the associated rectangle in the source's Bitmap
                        Tile = texture.TileIndex,
                        X = texture.Position.X,
                        Y = texture.Position.Y
                    });
                }
            }
        }
    }

    private List<StaticTextureSource<T>> GetSources(T type)
    {
        List<StaticTextureSource<T>> sources = new();
        foreach (StaticTextureSource<T> source in _typeSources)
        {
            if (source.EntityTextureMap.ContainsKey(type))
            {
                sources.Add(source);
            }
        }
        return sources;
    }

    // Keep a note of removed textures so that anything defined statically in the texture source
    // files does not get imported (e.g. if Barney is removed from GW, we don't want the randomized
    // textures to be imported).
    public void OnTexturesRemoved(List<T> entities)
    {
        if (RemovedTextures == null)
        {
            RemovedTextures = entities;
        }
        else
        {
            RemovedTextures.AddRange(entities);
        }
    }
}
