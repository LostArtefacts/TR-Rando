using System;
using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Globalisation;
using TRGE.Core;
using TRLevelReader.Helpers;

namespace TRRandomizerCore.Randomizers
{
    public class TR2GameStringRandomizer : BaseTR2Randomizer
    {
        private const int _maxLevelNameLength = 24;

        private G11N _g11n;
        private TR23GameStrings _gameStrings, _defaultGameStrings;

        public override void Randomize(int seed)
        {
            if (Settings.RandomizeGameStrings)
            {
                _generator = new Random(seed);
                _g11n = new G11N(G11NGame.TR2);

                if (!Settings.GameStringLanguage.IsHybrid)
                {
                    _gameStrings = _g11n.GetGameStrings(Settings.GameStringLanguage) as TR23GameStrings;
                }
                _defaultGameStrings = _g11n.GetDefaultGameStrings() as TR23GameStrings;

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

            if (Settings.ReassignPuzzleNames)
            {
                // This is specific to the Dagger of Xian if it appears in other levels with the dragon. We'll just
                // use whatever has already been allocated as the dagger name in Lair.
                string daggerName = ScriptEditor.ScriptedLevels.ToList().Find(l => l.Is(TR2LevelNames.LAIR)).Puzzles[1];
                foreach (AbstractTRScriptedLevel level in ScriptEditor.ScriptedLevels)
                {
                    MoveAndReplacePuzzle(level, 1, 2, daggerName);
                }
            }

            SaveScript();

            TriggerProgress();
        }

        private TR23GameStrings GetGameStrings()
        {
            // This allows for a hybrid language to be used, so each call will randomly pick another language.
            if (Settings.GameStringLanguage.IsHybrid)
            {
                Language[] availableLangs = _g11n.RealLanguages;
                return _g11n.GetGameStrings(availableLangs[_generator.Next(0, availableLangs.Length)]) as TR23GameStrings;
            }

            return _gameStrings;
        }

        private GlobalStrings GetGlobalStrings(int index)
        {
            return GetGameStrings().GlobalStrings[index];
        }

        private TR23LevelStrings GetLevelStrings(string lvlName)
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
                            // Ensure to use one from the languages options rather than defaulting
                            int customRandomIndex = _generator.Next(0, options.Length);
                            scriptStrings[stringIndex] = _defaultGameStrings.Encode(options[customRandomIndex]);
                        }
                        else
                        {
                            scriptStrings[stringIndex] = _defaultGameStrings.Encode(options[randomIndex]);
                        }
                    }
                }
            }

            if (defaultGlobalStrings.StandaloneStrings != null)
            {
                foreach (int stringIndex in defaultGlobalStrings.StandaloneStrings.Keys)
                {
                    string[] options = GetGlobalStrings(globalStringsIndex).StandaloneStrings[stringIndex];
                    scriptStrings[stringIndex] = _defaultGameStrings.Encode(options[_generator.Next(0, options.Length)]);
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

            TR23LevelStrings defaultLevelStrings = _defaultGameStrings.LevelStrings[levelID];

            if (!Settings.RetainLevelNames && defaultLevelStrings.Names != null && defaultLevelStrings.Names.Length > 0)
            {
                string[] options = GetLevelStrings(levelID).Names;
                string levelName;
                do
                {
                    levelName = options[_generator.Next(0, options.Length)];
                }
                while (levelName.Length > _maxLevelNameLength);

                level.Name = _defaultGameStrings.Encode(levelName);
            }

            if (Settings.RetainKeyItemNames)
            {
                return;
            }

            if (defaultLevelStrings.Keys != null)
            {
                foreach (int keyIndex in defaultLevelStrings.Keys.Keys)
                {
                    string[] options = GetLevelStrings(levelID).Keys[keyIndex];
                    level.Keys[keyIndex] = _defaultGameStrings.Encode(options[_generator.Next(0, options.Length)]);
                }
            }

            if (defaultLevelStrings.Pickups != null)
            {
                foreach (int pickupIndex in defaultLevelStrings.Pickups.Keys)
                {
                    string[] options = GetLevelStrings(levelID).Pickups[pickupIndex];
                    level.Pickups[pickupIndex] = _defaultGameStrings.Encode(options[_generator.Next(0, options.Length)]);
                }
            }

            if (defaultLevelStrings.Puzzles != null)
            {
                foreach (int puzzleIndex in defaultLevelStrings.Puzzles.Keys)
                {
                    string[] options = GetLevelStrings(levelID).Puzzles[puzzleIndex];
                    level.Puzzles[puzzleIndex] = _defaultGameStrings.Encode(options[_generator.Next(0, options.Length)]);
                }
            }
        }

        private void MoveAndReplacePuzzle(AbstractTRScriptedLevel level, int currentIndex, int newIndex, string replacement)
        {
            if (level.Puzzles[currentIndex] != replacement)
            {
                if (level.Puzzles[currentIndex] != "P" + (currentIndex + 1))
                {
                    level.Puzzles[newIndex] = level.Puzzles[currentIndex];
                }
                level.Puzzles[currentIndex] = replacement;
            }
        }
    }
}