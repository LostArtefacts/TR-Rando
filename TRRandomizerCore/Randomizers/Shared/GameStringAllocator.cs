using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Globalisation;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class GameStringAllocator
{
    private const int _maxLevelNameLength = 24;

    protected G11N _g11n;
    protected TRGameStrings _gameStrings, _defaultGameStrings;

    public Random Generator { get; set; }
    public RandomizerSettings Settings { get; set; }

    public Dictionary<TRStringKey, string> Allocate(TRGameVersion version, AbstractTRScriptEditor script)
    {
        if (!Settings.RandomizeGameStrings)
        {
            return new();
        }
        
        _g11n = new(version);

        if (!Settings.GameStringLanguage.IsHybrid)
        {
            _gameStrings = _g11n.GetGameStrings(Settings.GameStringLanguage);
        }
        _defaultGameStrings = _g11n.GetDefaultGameStrings();

        Dictionary<TRStringKey, string> globalStrings = GenerateGlobalStrings();
        ProcessLevelStrings(script.AssaultLevel);
        globalStrings[TRStringKey.INV_ITEM_LARAS_HOME] = script.AssaultLevel.Name;

        foreach (AbstractTRScriptedLevel level in script.EnabledScriptedLevels)
        {
            ProcessLevelStrings(level);
        }

        return globalStrings;
    }

    protected TRGameStrings GetGameStrings()
    {
        // This allows for a hybrid language to be used, so each call will randomly pick another language.
        return Settings.GameStringLanguage.IsHybrid
            ? _g11n.GetGameStrings(_g11n.RealLanguages.RandomItem(Generator))
            : _gameStrings;
    }

    protected List<string> GetGlobalStrings(TRStringKey key)
    {
        return GetGameStrings().GlobalStrings[key];
    }

    protected TRLevelStrings GetLevelStrings(string lvlName)
    {
        TRGameStrings strings = GetGameStrings();
        if (!strings.LevelStrings.ContainsKey(lvlName))
        {
            strings = _defaultGameStrings;
        }
        return strings.LevelStrings[lvlName];
    }

    private Dictionary<TRStringKey, string> GenerateGlobalStrings()
    {
        Dictionary<TRStringKey, List<string>> defaultGlobalStrings = _defaultGameStrings.GlobalStrings;
        Dictionary<TRStringKey, string> result = new();

        foreach (TRStringKey stringKey in defaultGlobalStrings.Keys)
        {
            List<string> options = GetGlobalStrings(stringKey);
            result[stringKey] = TRGameStrings.Encode(options.RandomItem(Generator));
        }

        return result;
    }

    private void ProcessLevelStrings(AbstractTRScriptedLevel level)
    {
        string levelID = level.LevelFileBaseName.ToUpper();
        if (!_defaultGameStrings.LevelStrings.ContainsKey(levelID))
        {
            return;
        }

        TRLevelStrings defaultLevelStrings = _defaultGameStrings.LevelStrings[levelID];

        if (!Settings.RetainLevelNames && defaultLevelStrings.Names != null && defaultLevelStrings.Names.Count > 0)
        {
            List<string> options = GetLevelStrings(levelID).Names;
            if (options.Any(o => o.Length <= _maxLevelNameLength))
            {
                string levelName;
                do
                {
                    levelName = options.RandomItem(Generator);
                }
                while (levelName.Length > _maxLevelNameLength);

                level.Name = TRGameStrings.Encode(levelName);
            }
        }

        if (Settings.RetainKeyItemNames)
        {
            return;
        }

        for (int i = 0; i < level.Keys.Count; i++)
        {
            if (GenerateKeyItemName(levelID, TRKeyItemKey.Key1 + i) is string newName)
            {
                level.Keys[i] = newName;
            }
        }

        for (int i = 0; i < level.Pickups.Count; i++)
        {
            if (GenerateKeyItemName(levelID, TRKeyItemKey.Pickup1 + i) is string newName)
            {
                level.Pickups[i] = newName;
            }
        }

        for (int i = 0; i < level.Puzzles.Count; i++)
        {
            if (GenerateKeyItemName(levelID, TRKeyItemKey.Puzzle1 + i) is string newName)
            {
                level.Puzzles[i] = newName;
            }
        }
    }

    private string GenerateKeyItemName(string levelID, TRKeyItemKey keyName)
    {
        Dictionary<TRKeyItemKey, List<string>> optionMap = GetLevelStrings(levelID).KeyItems;
        if (optionMap == null || !optionMap.ContainsKey(keyName))
        {
            return null;
        }

        List<string> options = optionMap[keyName];
        return TRGameStrings.Encode(options.RandomItem(Generator));
    }
}
