using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TRTexture16Importer.Textures
{
    public class GlobalGrouping<E>
        where E : Enum
    {
        private static readonly string _groupingDataPath = @"Static\global_grouping.json";

        public Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> Sources { get; private set; }
        public List<TextureGrouping<E>> Grouping { get; private set; }

        public GlobalGrouping(TextureDatabase<E> database)
        {
            Sources = new Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>>();
            Grouping = new List<TextureGrouping<E>>();

            Dictionary<string, object> globalData = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(Path.Combine(database.DataPath, _groupingDataPath)));

            // These sources will be added to all levels automatically without having to explicitly map them. The targets
            // can be empty for such things as dynamic sprite sequence mapping, or filled with shared values in all levels
            // (not currently used).
            if (globalData.ContainsKey("GlobalSources"))
            {
                Dictionary<string, List<StaticTextureTarget>> sources = JsonConvert.DeserializeObject<Dictionary<string, List<StaticTextureTarget>>>(globalData["GlobalSources"].ToString());
                foreach (string sourceName in sources.Keys)
                {
                    Sources.Add(database.GetStaticSource(sourceName), sources[sourceName]);
                }
            }

            if (globalData.ContainsKey("GlobalGrouping"))
            {
                List<Dictionary<string, object>> groupListData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(globalData["GlobalGrouping"].ToString());
                foreach (IDictionary<string, object> groupData in groupListData)
                {
                    TextureGrouping<E> grouping = new TextureGrouping<E>
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
                            Dictionary<StaticTextureSource<E>, string> map = new Dictionary<StaticTextureSource<E>, string>();
                            foreach (string sourceName in alternatives[theme].Keys)
                            {
                                map.Add(database.GetStaticSource(sourceName), alternatives[theme][sourceName]);
                            }
                            grouping.ThemeAlternatives.Add(theme, map);
                        }
                    }

                    Grouping.Add(grouping);
                }
            }
        }

        public List<TextureGrouping<E>> GetGrouping(IEnumerable<StaticTextureSource<E>> sources)
        {
            List<TextureGrouping<E>> groupSet = new List<TextureGrouping<E>>();

            foreach (StaticTextureSource<E> source in sources)
            {
                TextureGrouping<E> grouping = GetGrouping(source);

                if (grouping != null && !sources.Any(s => grouping.Masters.Contains(s)))
                {
                    groupSet.Add(grouping);
                }
            }

            return groupSet;
        }

        public TextureGrouping<E> GetGrouping(StaticTextureSource<E> source)
        {
            foreach (TextureGrouping<E> grouping in Grouping)
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