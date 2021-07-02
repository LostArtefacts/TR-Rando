using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRTexture16Importer.Textures.Source;

namespace TRTexture16Importer.Textures.Grouping
{
    public class TextureGroupingSet
    {
        public List<TextureGrouping> Grouping { get; private set; }

        public TextureGroupingSet(TextureDatabase database)
        {
            List<TextureGrouping> globalGrouping = new List<TextureGrouping>();

            List<Dictionary<string, object>> groupListData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(File.ReadAllText(@"Resources\Textures\Source\Static\global_grouping.json"));
            foreach (IDictionary<string, object> groupData in groupListData)
            {
                TextureGrouping grouping = new TextureGrouping
                {
                    Leader = database.GetStaticSource(groupData["Leader"].ToString())
                };

                if (groupData.ContainsKey("Masters"))
                {
                    // Masters can be defined and if they are present in a source list, the leader will be ignored
                    SortedSet<string> masters = JsonConvert.DeserializeObject<SortedSet<string>>(groupData["Masters"].ToString());
                    foreach (string sourceName in masters)
                    {
                        grouping.Masters.Add(database.GetStaticSource(sourceName));
                    }
                }

                SortedSet<string> followers = JsonConvert.DeserializeObject<SortedSet<string>>(groupData["Followers"].ToString());
                foreach (string sourceName in followers)
                {
                    grouping.Followers.Add(database.GetStaticSource(sourceName));
                }

                if (groupData.ContainsKey("ThemeAlternatives"))
                {
                    Dictionary<string, Dictionary<string, string>> alternatives = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(groupData["ThemeAlternatives"].ToString());
                    foreach (string theme in alternatives.Keys)
                    {
                        Dictionary<StaticTextureSource, string> map = new Dictionary<StaticTextureSource, string>();
                        foreach (string sourceName in alternatives[theme].Keys)
                        {
                            map.Add(database.GetStaticSource(sourceName), alternatives[theme][sourceName]);
                        }
                        grouping.ThemeAlternatives.Add(theme, map);
                    }
                }

                globalGrouping.Add(grouping);
            }

            Grouping = globalGrouping;
        }

        public List<TextureGrouping> GetGrouping(IEnumerable<StaticTextureSource> sources)
        {
            List<TextureGrouping> groupSet = new List<TextureGrouping>();

            foreach (StaticTextureSource source in sources)
            {
                TextureGrouping grouping = GetGrouping(source);

                if (grouping != null && !sources.Any(s => grouping.Masters.Contains(s)))
                {
                    groupSet.Add(grouping);
                }
            }

            return groupSet;
        }

        public TextureGrouping GetGrouping(StaticTextureSource source)
        {
            foreach (TextureGrouping grouping in Grouping)
            {
                if (grouping.Leader.Equals(source))
                {
                    return grouping;
                }
            }
            return null;
        }
    }
}