using Newtonsoft.Json;
using System.Text;
using TRLevelControl.Model;

namespace TRRandomizerCore.Globalisation;

public class G11N
{
    private static readonly List<Language> _definedLanguages;

    static G11N()
    {
        _definedLanguages = JsonConvert.DeserializeObject<List<Language>>(File.ReadAllText(
            "Resources/Shared/G11N/languages.json"));
    }

    private readonly TRGameVersion _game;
    private readonly SortedDictionary<Language, TRGameStrings> _languageMap;
    private readonly SortedSet<Language> _realLanguages;

    public List<Language> RealLanguages => _realLanguages.ToList();

    public G11N(TRGameVersion game)
    {
        _game = game;
        _languageMap = new();
        _realLanguages = new();

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
        => _definedLanguages.Where(l => l.IsHybrid || LoadLanguage(l, game) != null).ToList();

    public static Language GetLanguage(string tag)
        => _definedLanguages.Find(l => string.Equals(l.Tag, tag, StringComparison.InvariantCultureIgnoreCase));

    public TRGameStrings GetDefaultGameStrings()
        => GetGameStrings(Language.DefaultTag);

    public TRGameStrings GetGameStrings(string tag)
        => GetGameStrings(GetLanguage(tag));

    public TRGameStrings GetGameStrings(Language language)
        => _languageMap.ContainsKey(language)
        ? _languageMap[language]
        : throw new KeyNotFoundException($"There is no language defined for {language.Name} ({language.Tag}).");

    private static TRGameStrings LoadLanguage(Language language, TRGameVersion game)
    {
        string path = $"Resources/{game}/Strings/G11N/gamestrings_{language.Tag}.json";
        if (!File.Exists(path))
        {
            return null;
        }

        return JsonConvert.DeserializeObject<TRGameStrings>(File.ReadAllText(path, Encoding.UTF8));
    }
}
