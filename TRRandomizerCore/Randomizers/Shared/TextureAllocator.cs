using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class TextureAllocator
{
    private readonly Dictionary<TRGameVersion, TRRTexInfo> _texInfo;
    private Dictionary<TRRScriptedLevel, TRGData> _levelData;

    public Random Generator { get; set; }
    public RandomizerSettings Settings { get; set; }

    public TextureAllocator()
    {
        _texInfo = JsonConvert.DeserializeObject<Dictionary<TRGameVersion, TRRTexInfo>>(File.ReadAllText(@"Resources\Shared\Graphics\TRRTex.json"));
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
        TRRTexInfo texInfo = _texInfo[version];

        // Temporary settings
        int mode = 1;
        bool retainMode = true;

        List<TRRScriptedLevel> levels = new(_levelData.Keys);
        List<ushort> allTextures = new(textureCache.Values.SelectMany(t => t).Distinct());

        foreach (var (level, data) in _levelData)
        {
            List<ushort> newTextures;
            if (mode == 0)
            {
                newTextures = new(data.Textures);
            }
            else if (mode == 1)
            {
                TRRScriptedLevel nextLevel;
                do
                {
                    nextLevel = levels.RandomItem(Generator);
                }
                while (level == nextLevel);
                levels.Remove(nextLevel);

                newTextures = textureCache[nextLevel];
                while (newTextures.Count < data.Textures.Count)
                {
                    newTextures.Add(newTextures.RandomItem(Generator));
                }
            }
            else
            {
                newTextures = allTextures.RandomSelection(Generator, data.Textures.Count);
            }

            newTextures.Shuffle(Generator);

            if (retainMode)
            {
                // Restore any that need to remain, like mist and skyboxes, and ensure everything
                // else is a usable texture.
                for (int i = 0; i < data.Textures.Count; i++)
                {
                    if (texInfo.FixedIDs.Contains(data.Textures[i]))
                    {
                        newTextures[i] = data.Textures[i];
                    }
                    else
                    {
                        while (!texInfo.UsableIDs.Contains(newTextures[i]))
                        {
                            newTextures[i] = newTextures.RandomItem(Generator);
                        }
                    }
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

    class TRRTexInfo
    {
        public SortedSet<ushort> UsableIDs { get; set; }
        public SortedSet<ushort> FixedIDs { get; set; }
    }
}
