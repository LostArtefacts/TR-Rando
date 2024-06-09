using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TextureAllocator<T, R>
    where T : Enum
    where R : Enum
{
    private readonly TRTexInfo<T> _texInfo;
    private readonly Dictionary<TRRScriptedLevel, TRGData> _trgData;
    private readonly Dictionary<TRRScriptedLevel, Dictionary<T, R>> _mapData;

    public Random Generator { get; set; }
    public RandomizerSettings Settings { get; set; }

    public TextureAllocator(TRGameVersion version)
    {
        _texInfo = JsonConvert.DeserializeObject<TRTexInfo<T>>(File.ReadAllText($@"Resources\{version}\Textures\texinfo.json"));
        _trgData = new();
        _mapData = new();
    }

    public void LoadData(TRRScriptedLevel level, TRGData trgData, Dictionary<T, R> mapData)
    {
        _trgData[level] = trgData;
        _mapData[level] = mapData;
    }

    public void Allocate(Func<TRRScriptedLevel, TRGData, Dictionary<T, R>, bool> saveData)
    {
        Dictionary<TRRScriptedLevel, List<ushort>> textureCache = _trgData.ToDictionary(l => l.Key, l => l.Value.Textures.ToList());
        List<ushort> allTextures = new(textureCache.Values.SelectMany(t => t).Distinct());
        List<TRRScriptedLevel> levels = new(_trgData.Keys);        
        List<TRRScriptedLevel> levelSwaps = new();

        if (Settings.TextureMode == TextureMode.Game)
        {
            levelSwaps.AddRange(levels);
            do
            {
                levelSwaps.Shuffle(Generator);
            }
            while (levelSwaps.Any(l => levels.IndexOf(l) == levelSwaps.IndexOf(l)));
        }

        foreach (var (level, trgData) in _trgData)
        {
            List<ushort> baseTextures = new();
            List<ushort> newTextures = new();
            if (Settings.TextureMode == TextureMode.Level)
            {
                baseTextures.AddRange(trgData.Textures);
                newTextures.AddRange(trgData.Textures);
            }
            else if (Settings.TextureMode == TextureMode.Game)
            {
                TRRScriptedLevel nextLevel = levelSwaps[levels.IndexOf(level)];
                baseTextures.AddRange(textureCache[nextLevel]);
                newTextures = textureCache[nextLevel];

                while (newTextures.Count < trgData.Textures.Count)
                {
                    newTextures.Add(newTextures.RandomItem(Generator));
                }
            }
            else
            {
                baseTextures.AddRange(trgData.Textures);
                newTextures.AddRange(allTextures.RandomSelection(Generator, trgData.Textures.Count));
            }

            newTextures.Shuffle(Generator);

            if (Settings.MatchTextureTypes)
            {
                for (int i = 0; i < trgData.Textures.Count; i++)
                {
                    ushort originalTexture = trgData.Textures[i];
                    ushort newTexture = newTextures[i];

                    // Try to match levers, ladders and windows/gates etc with similar from the import set.
                    // By default, ensure everything else is opaque.
                    if (!_texInfo.Categories.Keys.Any(c => SelectTexture(originalTexture, ref newTexture, c, baseTextures)))
                    {
                        while (!_texInfo.Categories[TRTexCategory.Opaque].Contains(newTexture))
                        {
                            newTexture = newTextures.RandomItem(Generator);
                        }
                    }

                    newTextures[i] = newTexture;
                }
            }

            Dictionary<T, R> itemRemp = Settings.TextureMode == TextureMode.Game && Settings.MatchTextureItems
                ? RemapItems(level, levelSwaps[levels.IndexOf(level)])
                : null;

            trgData.Textures.Clear();
            trgData.Textures.AddRange(newTextures);
            if (!saveData(level, trgData, itemRemp))
            {
                break;
            }
        }
    }

    private bool SelectTexture(ushort originalTexture, ref ushort targetTexture,
        TRTexCategory category, List<ushort> baseTextures)
    {
        if (category == TRTexCategory.Opaque)
        {
            return false;
        }

        SortedSet<ushort> controlSet = _texInfo.Categories[category];
        if (!controlSet.Contains(originalTexture))
        {
            return false;
        }

        if (category == TRTexCategory.Fixed)
        {
            targetTexture = originalTexture;
        }
        else if (!baseTextures.Any(controlSet.Contains))
        {
            targetTexture = _texInfo.Defaults[category];
        }
        else
        {
            while (!controlSet.Contains(targetTexture))
            {
                targetTexture = baseTextures.RandomItem(Generator);
            }
        }

        return true;
    }

    private Dictionary<T, R> RemapItems(TRRScriptedLevel level, TRRScriptedLevel otherLevel)
    {
        string levelFile = level.LevelFileBaseName.ToUpper();
        string otherLevelFile = otherLevel.LevelFileBaseName.ToUpper();

        Dictionary<string, Dictionary<T, TRItemFlags>> currentAreaMap = _texInfo.ItemFlags.FirstOrDefault(a => a.ContainsKey(levelFile));
        Dictionary<string, Dictionary<T, TRItemFlags>> otherAreaMap = _texInfo.ItemFlags.FirstOrDefault(a => a.ContainsKey(otherLevelFile));
        if (currentAreaMap == null || otherAreaMap == null)
        {
            return null;
        }

        Dictionary<T, TRItemFlags> currentLevelMap = currentAreaMap[levelFile];
        Dictionary<T, TRItemFlags> otherLevelMap = otherAreaMap[otherLevelFile];

        Dictionary<T, R> newMapping = new(_mapData[level]);
        Dictionary<T, R> otherMapping = _mapData[otherLevel];

        List<TRItemFlags> pairFlags = new()
        {
            TRItemFlags.PairA,
            TRItemFlags.PairB,
            TRItemFlags.PairC,
        };

        bool IsPaired(TRItemFlags flags)
            => pairFlags.Any(f => flags.HasFlag(f));

        bool IsDoor(TRItemFlags flags)
            => flags.HasFlag(TRItemFlags.LeftDoor) || flags.HasFlag(TRItemFlags.RightDoor);

        bool TryMatch(T type, TRItemFlags flags, Dictionary<T, TRItemFlags> otherLevelMap)
        {
            List<T> otherTypes = new();
            foreach (var (otherType, otherFlags) in otherLevelMap)
            {
                if (flags == otherFlags)
                {
                    otherTypes.Add(otherType);
                }
            }

            if (otherTypes.Count > 0)
            {
                T otherType = otherTypes.RandomItem(Generator);
                if (otherMapping.ContainsKey(otherType))
                {
                    newMapping[type] = otherMapping[otherType];
                }
                else
                {
                    // The "first" models don't have mappings e.g. Vilcabamba pushblock, so we mimic that approach.
                    newMapping.Remove(type);
                }
                return true;
            }
            else if (IsDoor(flags) && !IsPaired(flags))
            {
                // Applicable where normally paired doors are used in single instances, like Caves room 17.
                return pairFlags.Any(pair => TryMatch(type, flags | pair, otherLevelMap));
            }

            return false;
        }

        bool DoorMatch(T type, TRItemFlags flags, Dictionary<T, TRItemFlags> otherLevelMap)
        {
            if (!IsDoor(flags) || !IsPaired(flags) || flags.HasFlag(TRItemFlags.FourClick))
            {
                return false;
            }

            pairFlags.ForEach(pair => flags &= ~pair);
            return pairFlags.Any(pair => TryMatch(type, flags | pair, otherLevelMap));
        }

        foreach (var (type, flags) in currentLevelMap)
        {
            otherMapping = _mapData[otherLevel];
            if (!TryMatch(type, flags, otherLevelMap))
            {
                if (DoorMatch(type, flags, otherLevelMap))
                {
                    continue;
                }

                // The used level doesn't have an equivalent item, so check other area levels
                // e.g. if using Folly, search Colly, Midas etc too.
                foreach (var (siblingLevel, siblingMap) in otherAreaMap)
                {
                    if (siblingMap == otherLevelMap)
                    {
                        continue;
                    }

                    otherMapping = _mapData[_mapData.Keys.First(l => string.Equals(l.LevelFileBaseName, 
                        siblingLevel, StringComparison.InvariantCultureIgnoreCase))];
                    if (TryMatch(type, flags, siblingMap)
                        || DoorMatch(type, flags, siblingMap))
                    {
                        break;
                    }
                }
            }
        }

        return newMapping;
    }
}
