using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TR2RandomizerCore.Globalisation
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
            Language[] languages = JsonConvert.DeserializeObject<Language[]>(File.ReadAllText(@"Resources\Strings\languages.json"));
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

        public GameStrings GetGameStrings(string tag)
        {
            return GetGameStrings(GetLanguage(tag));
        }

        public GameStrings GetGameStrings(Language language)
        {
            if (!_languageMap.ContainsKey(language))
            {
                throw new KeyNotFoundException(string.Format("There is no language defined for {0} ({1})).", language.Name, language.Tag));
            }

            if (_languageMap[language] == null && !language.IsHybrid)
            {
                string path = string.Format(@"Resources\Strings\G11N\gamestrings_{0}.json", language.Tag);
                _languageMap[language] = JsonConvert.DeserializeObject<GameStrings>(File.ReadAllText(path, Encoding.UTF8));
            }

            return _languageMap[language];
        }

        public GameStrings GetDefaultGameStrings()
        {
            return GetGameStrings(GetLanguage(Language.DefaultTag));
        }
    }
}