using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TextureAllocator
{
    private readonly Dictionary<TRGameVersion, TRTexInfo> _texInfo;
    private Dictionary<TRRScriptedLevel, TRGData> _levelData;

    public Random Generator { get; set; }
    public RandomizerSettings Settings { get; set; }

    public TextureAllocator()
    {
        _texInfo = JsonConvert.DeserializeObject<Dictionary<TRGameVersion, TRTexInfo>>(File.ReadAllText(@"Resources\Shared\Graphics\TRRTex.json"));
    }

    public void LoadData(List<TRRScriptedLevel> levels, Func<TRRScriptedLevel, TRGData> loadData)
    {
        _levelData = new();
        foreach (TRRScriptedLevel level in levels)
        {
            if ((_levelData[level] = loadData(level)) == null)
            {
                // Operation cancelled
                return;
            }
        }
    }

    public void Allocate(TRGameVersion version, Func<TRRScriptedLevel, TRGData, bool> saveData)
    {
        Dictionary<TRRScriptedLevel, List<ushort>> textureCache = _levelData.ToDictionary(l => l.Key, l => l.Value.Textures.ToList());
        TRTexInfo texInfo = _texInfo[version];

        // Temporary settings
        int mode = 1;
        bool retainMode = true;

        List<TRRScriptedLevel> levels = new(_levelData.Keys);
        List<TRRScriptedLevel> levelSwaps = new();
        if (mode == 1)
        {
            levelSwaps.AddRange(levels);
            do
            {
                levelSwaps.Shuffle(Generator);
            }
            while (levelSwaps.Any(l => levels.IndexOf(l) == levelSwaps.IndexOf(l)));
        }

        List<ushort> allTextures = new(textureCache.Values.SelectMany(t => t).Distinct());

        foreach (var (level, data) in _levelData)
        {
            List<ushort> baseTextures = new();
            List<ushort> newTextures = new();
            if (mode == 0)
            {
                baseTextures.AddRange(data.Textures);
                newTextures.AddRange(data.Textures);
            }
            else if (mode == 1)
            {
                TRRScriptedLevel nextLevel = levelSwaps[levels.IndexOf(level)];
                baseTextures.AddRange(textureCache[nextLevel]);
                newTextures = textureCache[nextLevel];

                while (newTextures.Count < data.Textures.Count)
                {
                    newTextures.Add(newTextures.RandomItem(Generator));
                }
            }
            else
            {
                baseTextures.AddRange(data.Textures);
                newTextures.AddRange(allTextures.RandomSelection(Generator, data.Textures.Count));
            }

            newTextures.Shuffle(Generator);

            if (retainMode)
            {
                for (int i = 0; i < data.Textures.Count; i++)
                {
                    ushort originalTexture = data.Textures[i];
                    ushort newTexture = newTextures[i];

                    // Try to match levers, ladders and windows/gates etc with similar from the import set.
                    // By default, ensure everything else is opaque.
                    if (!texInfo.Categories.Keys.Any(c => SelectTexture(originalTexture, ref newTexture, texInfo, c, baseTextures)))
                    {
                        while (!texInfo.Categories[TRTexCategory.Opaque].Contains(newTexture))
                        {
                            newTexture = newTextures.RandomItem(Generator);
                        }
                    }

                    newTextures[i] = newTexture;
                }
            }

            data.Textures.Clear();
            data.Textures.AddRange(newTextures);
            if (!saveData(level, data))
            {
                break;
            }
        }
    }

    private bool SelectTexture(ushort originalTexture, ref ushort targetTexture,
        TRTexInfo texInfo, TRTexCategory category, List<ushort> baseTextures)
    {
        if (category == TRTexCategory.Opaque)
        {
            return false;
        }

        SortedSet<ushort> controlSet = texInfo.Categories[category];
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
            targetTexture = texInfo.Defaults[category];
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
}
