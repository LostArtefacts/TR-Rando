using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TRGE.Core;

namespace TR2RandomizerCore.Randomizers
{
    public class GameStringRandomizer : RandomizerBase
    {
        public TR23ScriptEditor ScriptEditor { get; set; }
        public bool RetainKeyItemNames { get; set; }

        private GameStrings _gameStrings;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            _gameStrings = JsonConvert.DeserializeObject<GameStrings>(File.ReadAllText(@"Resources\Strings\gamestrings.json"));

            TR23Script script = ScriptEditor.Script as TR23Script;
            List<string> gamestrings1 = new List<string>(script.GameStrings1);
            List<string> gamestrings2 = new List<string>(script.GameStrings2);

            ProcessGlobalStrings(0, gamestrings1);
            ProcessGlobalStrings(1, gamestrings2);

            script.GameStrings1 = gamestrings1.ToArray();
            script.GameStrings2 = gamestrings2.ToArray();

            foreach (AbstractTRScriptedLevel level in ScriptEditor.ScriptedLevels)
            {
                ProcessLevelStrings(level);
            }
        }

        private void ProcessGlobalStrings(int globalStringsIndex, List<string> scriptStrings)
        {
            if (globalStringsIndex > _gameStrings.GlobalStrings.Length - 1)
            {
                return;
            }

            GlobalStrings globalStrings = _gameStrings.GlobalStrings[globalStringsIndex];

            if (globalStrings.GroupedStrings != null)
            {
                foreach (Dictionary<int, string[]> grouping in globalStrings.GroupedStrings)
                {
                    // We pick a random string index based on the first mapping and use it for the others, so this
                    // assumes all items in the group have the same number of available options.
                    int randomIndex = -1;
                    foreach (int stringIndex in grouping.Keys)
                    {
                        if (randomIndex == -1)
                        {
                            randomIndex = _generator.Next(0, grouping[stringIndex].Length);
                        }

                        scriptStrings[stringIndex] = grouping[stringIndex][randomIndex];
                    }
                }
            }

            if (globalStrings.StandaloneStrings != null)
            {
                foreach (int stringIndex in globalStrings.StandaloneStrings.Keys)
                {
                    string[] options = globalStrings.StandaloneStrings[stringIndex];
                    scriptStrings[stringIndex] = options[_generator.Next(0, options.Length)];
                }
            }
        }

        private void ProcessLevelStrings(AbstractTRScriptedLevel level)
        {
            string levelID = level.LevelFileBaseName.ToUpper();
            if (!_gameStrings.LevelStrings.ContainsKey(levelID))
            {
                return;
            }

            LevelStrings levelStrings = _gameStrings.LevelStrings[levelID];

            if (levelStrings.Names != null && levelStrings.Names.Length > 0)
            {
                level.Name = levelStrings.Names[_generator.Next(0, levelStrings.Names.Length)];
            }

            if (RetainKeyItemNames)
            {
                return;
            }

            if (levelStrings.Keys != null)
            {
                foreach (int keyIndex in levelStrings.Keys.Keys)
                {
                    string[] options = levelStrings.Keys[keyIndex];
                    level.Keys[keyIndex] = options[_generator.Next(0, options.Length)];
                }
            }

            if (levelStrings.Pickups != null)
            {
                foreach (int pickupIndex in levelStrings.Pickups.Keys)
                {
                    string[] options = levelStrings.Pickups[pickupIndex];
                    level.Pickups[pickupIndex] = options[_generator.Next(0, options.Length)];
                }
            }

            if (levelStrings.Puzzles != null)
            {
                foreach (int puzzleIndex in levelStrings.Puzzles.Keys)
                {
                    string[] options = levelStrings.Puzzles[puzzleIndex];
                    level.Puzzles[puzzleIndex] = options[_generator.Next(0, options.Length)];
                }
            }
        }
    }

    public class GameStrings
    {
        public GlobalStrings[] GlobalStrings { get; set; }
        public Dictionary<string, LevelStrings> LevelStrings { get; set; }
    }

    public class GlobalStrings
    {
        public Dictionary<int, string[]>[] GroupedStrings { get; set; }
        public Dictionary<int, string[]> StandaloneStrings { get; set; }
    }

    public class LevelStrings
    {
        public string[] Names { get; set; }
        public Dictionary<int, string[]> Keys { get; set; }
        public Dictionary<int, string[]> Pickups { get; set; }
        public Dictionary<int, string[]> Puzzles { get; set; }
    }
}