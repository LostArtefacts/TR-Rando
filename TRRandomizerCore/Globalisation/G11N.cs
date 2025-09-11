using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using TRLevelControl.Model;

namespace TRRandomizerCore.Globalisation;

public class G11N
{
    private static readonly List<Language> _definedLanguages;
    private static readonly Dictionary<TRGameVersion, JObject> _defaultRawStrings = [];

    static G11N()
    {
        _definedLanguages = JsonConvert.DeserializeObject<List<Language>>(File.ReadAllText(
            "Resources/Shared/G11N/languages.json"));
    }

    private readonly TRGameVersion _game;
    private readonly SortedDictionary<Language, TRGameStrings> _languageMap;
    private readonly SortedSet<Language> _realLanguages;

    public List<Language> RealLanguages => [.. _realLanguages];

    public G11N(TRGameVersion game)
    {
        _game = game;
        _languageMap = [];
        _realLanguages = [];

        foreach (Language language in _definedLanguages)
        {
            TRGameStrings strings;
            if (!language.IsHybrid && (strings = LoadLanguage(language, _game)) != null)
            {
                _languageMap[language] = strings;
                _realLanguages.Add(language);
            }
        }
    }

    public static List<Language> GetSupportedLanguages(TRGameVersion game)
        => _definedLanguages.FindAll(l => l.IsHybrid || LoadLanguage(l, game) != null);

    public static Language GetLanguage(string tag)
        => _definedLanguages.Find(l => string.Equals(l.Tag, tag, StringComparison.InvariantCultureIgnoreCase));

    public TRGameStrings GetDefaultGameStrings()
        => GetGameStrings(Language.DefaultTag);

    public TRGameStrings GetGameStrings(string tag)
        => GetGameStrings(GetLanguage(tag));

    public TRGameStrings GetGameStrings(Language language)
        => _languageMap.TryGetValue(language, out var strings)
        ? strings
        : throw new KeyNotFoundException($"There is no language defined for {language.Name} ({language.Tag}).");

    private static TRGameStrings LoadLanguage(Language language, TRGameVersion game)
    {
        var langStrings = ReadLanguage(language, game);
        if (langStrings == null)
        {
            return null;
        }

        if (!language.IsDefault)
        {
            var mergedStrings = GetDefaultRawStrings(game);
            var defaultStrings = GetDefaultRawStrings(game);

            mergedStrings.Merge(langStrings, new()
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Ignore,
            });            
            ApplyMergeFallbacks(mergedStrings, defaultStrings);

            langStrings = mergedStrings;
        }

        return langStrings.ToObject<TRGameStrings>();
    }

    private static JObject ReadLanguage(Language language, TRGameVersion game)
    {
        var path = $"Resources/{game}/Strings/G11N/gamestrings_{language.Tag}.json";
        return File.Exists(path) ? JObject.Parse(File.ReadAllText(path, Encoding.UTF8)) : null;
    }

    private static JObject GetDefaultRawStrings(TRGameVersion game)
    {
        if (!_defaultRawStrings.TryGetValue(game, out var rawStrings))
        {
            rawStrings = _defaultRawStrings[game] 
                = ReadLanguage(GetLanguage(Language.DefaultTag), game);
        }
        return (JObject)rawStrings.DeepClone();
    }

    private static void ApplyMergeFallbacks(JObject merged, JObject defaults)
    {
        foreach (var prop in defaults.Properties())
        {
            var mergedValue = merged[prop.Name];
            var defaultValue = prop.Value;

            switch (mergedValue)
            {
                case null:
                    merged[prop.Name] = defaultValue.DeepClone();
                    break;

                case JArray arr when !arr.HasValues:
                    merged[prop.Name] = defaultValue.DeepClone();
                    break;

                case JObject obj when defaultValue is JObject defObj:
                    ApplyMergeFallbacks(obj, defObj);
                    break;
            }
        }
    }
}
