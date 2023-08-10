using Newtonsoft.Json;
using System.Text;

namespace TRRandomizerCore.Globalisation;

public class G11N
{
    private static readonly List<Language> _definedLanguages;

    static G11N()
    {
        _definedLanguages = JsonConvert.DeserializeObject<List<Language>>(File.ReadAllText(
            @"Resources\Shared\G11N\languages.json"));
    }

    private readonly G11NGame _game;
    private readonly SortedDictionary<Language, IGameStrings> _languageMap;
    private readonly SortedSet<Language> _realLanguages;

    public Language[] RealLanguages => _realLanguages.ToArray();

    public G11N(G11NGame game)
    {
        _game = game;
        _languageMap = new SortedDictionary<Language, IGameStrings>();
        _realLanguages = new SortedSet<Language>();

        foreach (Language language in _definedLanguages)
        {
            IGameStrings strings;
            if (!language.IsHybrid && (strings = LoadLanguage(language, _game)) != null)
            {
                _languageMap[language] = strings;
                _realLanguages.Add(language);
            }
        }
    }

    public static List<Language> GetSupportedLanguages(G11NGame game) =>
        _definedLanguages.Where(l => l.IsHybrid || LoadLanguage(l, game) != null).ToList();

    public static Language GetLanguage(string tag)
    {
        tag = tag.ToUpper();
        return _definedLanguages.Find(l => l.Tag.ToUpper().Equals(tag));
    }

    public IGameStrings GetDefaultGameStrings()
    {
        return GetGameStrings(GetLanguage(Language.DefaultTag));
    }

    public IGameStrings GetGameStrings(string tag)
    {
        return GetGameStrings(GetLanguage(tag));
    }

    public IGameStrings GetGameStrings(Language language)
    {
        if (!_languageMap.ContainsKey(language))
        {
            throw new KeyNotFoundException(string.Format("There is no language defined for {0} ({1})).", language.Name, language.Tag));
        }

        return _languageMap[language];
    }

    private static IGameStrings LoadLanguage(Language language, G11NGame game)
    {
        string path = $@"Resources\{game}\Strings\G11N\gamestrings_{language.Tag}.json";
        if (!File.Exists(path))
        {
            return null;
        }

        switch (game)
        {
            case G11NGame.TR1:
                return JsonConvert.DeserializeObject<TR1GameStrings>(File.ReadAllText(path, Encoding.UTF8));
            case G11NGame.TR2:
            case G11NGame.TR3:
                return JsonConvert.DeserializeObject<TR23GameStrings>(File.ReadAllText(path, Encoding.UTF8));
            default:
                return null;
        }
    }
}

public enum G11NGame
{
    TR1,
    TR2,
    TR3
}
