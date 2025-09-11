using TRGE.Core;
using TRGE.Core.Item;
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
    protected bool _encodingRequired;

    public Random Generator { get; set; }
    public RandomizerSettings Settings { get; set; }

    public Dictionary<TRStringKey, string> Allocate(TRGameVersion version, AbstractTRScriptEditor script)
    {
        if (!Settings.RandomizeGameStrings)
        {
            return new();
        }
        
        _g11n = new(version);
        _encodingRequired = script is TR3ScriptEditor;

        if (!Settings.GameStringLanguage.IsHybrid)
        {
            _gameStrings = _g11n.GetGameStrings(Settings.GameStringLanguage);
        }
        _defaultGameStrings = _g11n.GetDefaultGameStrings();

        Dictionary<TRStringKey, string> globalStrings = GenerateGlobalStrings();
        ProcessObjectStrings(script.Script);
        ProcessLevelStrings(script.AssaultLevel);
        if (script.Script is not TRXScript)
        {
            globalStrings[TRStringKey.INV_ITEM_LARAS_HOME] = script.AssaultLevel.Name;
        }

        foreach (AbstractTRScriptedLevel level in script.EnabledScriptedLevels)
        {
            ProcessLevelStrings(level);
        }

        if (script.Script is TRXScript trxScript)
        {
            // We don't write to all possible TRX language files, so just hide the UI option.
            trxScript.EnforceConfig("language", "en", true);
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

    protected List<string> GetObjectStrings(string key)
    {
        return GetGameStrings().ObjectStrings[key];
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

    protected string Encode(string value)
        => _encodingRequired ? TRGameStrings.Encode(value) : value;

    private Dictionary<TRStringKey, string> GenerateGlobalStrings()
    {
        Dictionary<TRStringKey, List<string>> defaultGlobalStrings = _defaultGameStrings.GlobalStrings;
        Dictionary<TRStringKey, string> result = new();

        foreach (TRStringKey stringKey in defaultGlobalStrings.Keys)
        {
            List<string> options = GetGlobalStrings(stringKey);
            result[stringKey] = Encode(options.RandomItem(Generator));
        }

        return result;
    }

    private void ProcessObjectStrings(AbstractTRScript script)
    {
        if (script is not TRXScript trxScript)
        {
            return;
        }

        var defaultObjectStrings = _defaultGameStrings.ObjectStrings;
        foreach (var stringKey in defaultObjectStrings.Keys)
        {
            var options = GetObjectStrings(stringKey);
            var type = TRXNaming.GetType(stringKey, trxScript.Edition.Version);
            if (!trxScript.ObjectStrings.TryGetValue(type, out var objText))
            {
                trxScript.ObjectStrings[type] = objText = new();
            }
            objText.Name = Encode(options.RandomItem(Generator));
        }
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

                level.Name = Encode(levelName);
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
        return Encode(options.RandomItem(Generator));
    }

    public static void ApplyTRRGlobalStrings(TRRScript script, Dictionary<TRStringKey, string> generatedStrings, Dictionary<TRStringKey, string> gameMap)
    {
        foreach (var (key, value) in generatedStrings)
        {
            if (_trrCommonMap.ContainsKey(key))
            {
                script.CommonStrings[_trrCommonMap[key]] = value;
            }
            else if (gameMap.ContainsKey(key))
            {
                script.GameStrings[gameMap[key]] = value;
            }
        }
    }

    public void ApplyTRRLevelStrings(TRRScript script, Dictionary<string, Dictionary<TRKeyItemKey, string>> keyItemMap)
    {
        // TRR has single entries for shared key names, so to allow variety we'll shuffle the order
        // in which we apply the names to the script e.g. Vilcabamba silver key may overwrite Cistern
        // silver key. Shuffle key order too for cases like Tihocan with key2/3 being shared.
        List<TRRScriptedLevel> levels = new(script.Levels.Concat(script.GoldLevels).Cast<TRRScriptedLevel>())
        {
            script.AssaultLevel as TRRScriptedLevel
        };
        levels.Shuffle(Generator);

        foreach (TRRScriptedLevel level in levels)
        {
            string baseName = level.LevelFileBaseName.ToUpper();
            script.GameStrings["LVL_" + Path.GetFileNameWithoutExtension(baseName)] = level.Name;
            if (!keyItemMap.ContainsKey(baseName))
            {
                continue;
            }

            List<TRKeyItemKey> keys = new(keyItemMap[baseName].Keys);
            keys.Shuffle(Generator);
            foreach (TRKeyItemKey key in keys)
            {
                string value;
                if (key < TRKeyItemKey.Puzzle1)
                {
                    value = level.Keys[(int)key];
                }
                else if (key < TRKeyItemKey.Pickup1)
                {
                    value = level.Puzzles[key - TRKeyItemKey.Puzzle1];
                }
                else
                {
                    value = level.Pickups[key - TRKeyItemKey.Pickup1];
                }

                if (value != null)
                {
                    script.GameStrings[keyItemMap[baseName][key]] = value;
                }
            }
        }
    }

    private static readonly Dictionary<TRStringKey, string> _trrCommonMap = new()
    {
        [TRStringKey.HEADING_GAME_OVER] = "GAMEOVER",
        [TRStringKey.HEADING_INVENTORY] = "INVENTORY",
        [TRStringKey.HEADING_ITEMS] = "ITEMS",
        [TRStringKey.HEADING_OPTION] = "OPTIONS",
        [TRStringKey.INV_ITEM_BIG_MEDI] = "BIGMEDI",
        [TRStringKey.INV_ITEM_CONTROLS] = "CONTROL",
        [TRStringKey.INV_ITEM_DETAILS] = "DETAIL",
        [TRStringKey.INV_ITEM_GAME] = "GAME",
        [TRStringKey.INV_ITEM_MEDI] = "SMOLMEDI",
        [TRStringKey.INV_ITEM_SOUND] = "SOUND",
        [TRStringKey.MISC_EMPTY_SLOT_FMT] = "SLOT",
        [TRStringKey.INV_ITEM_LARAS_HOME] = "HOME",
    };
}
