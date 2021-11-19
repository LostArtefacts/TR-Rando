using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRandomizerCore.Globalisation
{
    public class G11N
    {
        private static G11N _instance;

        public static G11N Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new G11N();
                }
                return _instance;
            }
        }

        private readonly SortedDictionary<Language, GameStrings> _languageMap;

        private readonly SortedSet<Language> _realLanguages;

        public Language[] Languages => _languageMap.Keys.ToArray();
        public Language[] RealLanguages => _realLanguages.ToArray();

        private G11N()
        {
            _languageMap = new SortedDictionary<Language, GameStrings>();

            Language[] languages = JsonConvert.DeserializeObject<Language[]>(File.ReadAllText(
                @"Resources\Shared\G11N\languages.json"));

            _realLanguages = new SortedSet<Language>();

            foreach (Language language in languages)
            {
                // Use lazy-loading of the actual data
                _languageMap[language] = null;
                if (!language.IsHybrid)
                {
                    _realLanguages.Add(language);
                }
            }
        }

        public Language GetLanguage(string tag)
        {
            tag = tag.ToUpper();
            return _languageMap.Keys.ToList().Find(l => l.Tag.ToUpper().Equals(tag));
        }

        public GameStrings GetGameStrings(string tag, G11NGame game)
        {
            return GetGameStrings(GetLanguage(tag), game);
        }

        public GameStrings GetGameStrings(Language language, G11NGame game)
        {
            if (!_languageMap.ContainsKey(language))
            {
                throw new KeyNotFoundException(string.Format("There is no language defined for {0} ({1})).", language.Name, language.Tag));
            }

            if (_languageMap[language] == null && !language.IsHybrid)
            {
                try
                {
                    string path = string.Format(@"Resources\{0}\Strings\G11N\gamestrings_{1}.json", game.ToString(), language.Tag);
                    _languageMap[language] = JsonConvert.DeserializeObject<GameStrings>(File.ReadAllText(path, Encoding.UTF8));
                }
                catch (FileNotFoundException)
                {
                    //A language for a game isn't supported
                }
            }

            return _languageMap[language];
        }

        public GameStrings GetDefaultGameStrings(G11NGame game)
        {
            return GetGameStrings(GetLanguage(Language.DefaultTag), game);
        }
    }

    public enum G11NGame
    {
        TR2,
        TR3
    }
}