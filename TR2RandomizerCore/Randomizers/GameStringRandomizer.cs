using System;
using System.Collections.Generic;
using TR2RandomizerCore.Globalisation;
using TRGE.Core;

namespace TR2RandomizerCore.Randomizers
{
    public class GameStringRandomizer : RandomizerBase
    {
        private const int _maxLevelNameLength = 24;

        public TR23ScriptEditor ScriptEditor { get; set; }
        public Language Language { get; set; }
        public bool RetainKeyItemNames { get; set; }

        private GameStrings _gameStrings, _defaultGameStrings;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            if (!Language.IsHybrid)
            {
                _gameStrings = G11N.Instance.GetGameStrings(Language);
            }
            _defaultGameStrings = G11N.Instance.GetDefaultGameStrings();

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

        private GameStrings GetGameStrings()
        {
            // This allows for a hybrid language to be used, so each call will randomly pick another language.
            if (Language.IsHybrid)
            {
                Language[] availableLangs = G11N.Instance.RealLanguages;
                return G11N.Instance.GetGameStrings(availableLangs[_generator.Next(0, availableLangs.Length)]);
            }

            return _gameStrings;
        }

        private GlobalStrings GetGlobalStrings(int index)
        {
            return GetGameStrings().GlobalStrings[index];
        }

        private LevelStrings GetLevelStrings(string lvlName)
        {
            return GetGameStrings().LevelStrings[lvlName];
        }

        private void ProcessGlobalStrings(int globalStringsIndex, List<string> scriptStrings)
        {
            if (globalStringsIndex > _defaultGameStrings.GlobalStrings.Length - 1)
            {
                return;
            }

            GlobalStrings defaultGlobalStrings = _defaultGameStrings.GlobalStrings[globalStringsIndex];

            if (defaultGlobalStrings.GroupedStrings != null)
            {
                for (int i = 0; i < defaultGlobalStrings.GroupedStrings.Length; i++)
                {
                    Dictionary<int, string[]> grouping = defaultGlobalStrings.GroupedStrings[i];
                    // We pick a random string index based on the first mapping and use it for the others, so this
                    // assumes all items in the group have the same number of available options.
                    int randomIndex = -1;
                    foreach (int stringIndex in grouping.Keys)
                    {
                        if (randomIndex == -1)
                        {
                            randomIndex = _generator.Next(0, grouping[stringIndex].Length);
                        }

                        // Call GetGlobalStrings again in case Hybrid is in use.
                        string[] options = GetGlobalStrings(globalStringsIndex).GroupedStrings[i][stringIndex];
                        if (randomIndex >= options.Length)
                        {
                            // Fall back to default if the language doesn't have this index defined
                            options = grouping[stringIndex];
                        }
                        
                        scriptStrings[stringIndex] = GameStrings.Encode(options[randomIndex]);
                    }
                }
            }

            if (defaultGlobalStrings.StandaloneStrings != null)
            {
                foreach (int stringIndex in defaultGlobalStrings.StandaloneStrings.Keys)
                {
                    string[] options = GetGlobalStrings(globalStringsIndex).StandaloneStrings[stringIndex];
                    scriptStrings[stringIndex] = GameStrings.Encode(options[_generator.Next(0, options.Length)]);
                }
            }
        }

        private void ProcessLevelStrings(AbstractTRScriptedLevel level)
        {
            string levelID = level.LevelFileBaseName.ToUpper();
            if (!_defaultGameStrings.LevelStrings.ContainsKey(levelID))
            {
                return;
            }

            LevelStrings defaultLevelStrings = _defaultGameStrings.LevelStrings[levelID];

            if (defaultLevelStrings.Names != null && defaultLevelStrings.Names.Length > 0)
            {
                string[] options = GetLevelStrings(levelID).Names;
                string levelName;
                do
                {
                    levelName = options[_generator.Next(0, options.Length)];
                }
                while (levelName.Length > _maxLevelNameLength);

                level.Name = GameStrings.Encode(levelName);
            }

            if (RetainKeyItemNames)
            {
                return;
            }

            if (defaultLevelStrings.Keys != null)
            {
                foreach (int keyIndex in defaultLevelStrings.Keys.Keys)
                {
                    string[] options = GetLevelStrings(levelID).Keys[keyIndex];
                    level.Keys[keyIndex] = GameStrings.Encode(options[_generator.Next(0, options.Length)]);
                }
            }

            if (defaultLevelStrings.Pickups != null)
            {
                foreach (int pickupIndex in defaultLevelStrings.Pickups.Keys)
                {
                    string[] options = GetLevelStrings(levelID).Pickups[pickupIndex];
                    level.Pickups[pickupIndex] = GameStrings.Encode(options[_generator.Next(0, options.Length)]);
                }
            }

            if (defaultLevelStrings.Puzzles != null)
            {
                foreach (int puzzleIndex in defaultLevelStrings.Puzzles.Keys)
                {
                    string[] options = GetLevelStrings(levelID).Puzzles[puzzleIndex];
                    level.Puzzles[puzzleIndex] = GameStrings.Encode(options[_generator.Next(0, options.Length)]);
                }
            }
        }
    }
}