using System;
using System.Collections.Generic;
using System.IO;
using TRGE.Core;
using TRLevelReader;
using TRLevelReader.Model;

namespace TR2RandomizerCore.Randomizers
{
    public abstract class RandomizerBase : IRandomizer
    {
        protected TR2LevelReader _reader;
        protected TR2LevelWriter _writer;
        protected TR2Level _levelInstance;
        protected TR23ScriptedLevel _scriptedLevelInstance;
        protected Random _generator;

        internal List<TR23ScriptedLevel> Levels { get; set; }
        internal TRSaveMonitor SaveMonitor;

        public string BasePath { get; set; }

        public abstract void Randomize(int seed);

        public RandomizerBase()
        {
            _reader = new TR2LevelReader();
            _writer = new TR2LevelWriter();
        }

        protected void LoadLevelInstance(TR23ScriptedLevel scriptedLevel)
        {
            _scriptedLevelInstance = scriptedLevel;
            _levelInstance = LoadLevel(scriptedLevel.LevelFileBaseName);
        }

        public TR2Level LoadLevel(string name)
        {
            string fullPath = Path.Combine(BasePath, name);
            return _reader.ReadLevel(fullPath);
        }

        protected void SaveLevelInstance()
        {
            SaveLevel(_levelInstance, _scriptedLevelInstance.LevelFileBaseName);
        }

        public void SaveLevel(TR2Level level, string name)
        {
            string fullPath = Path.Combine(BasePath, name);
            _writer.WriteLevelToFile(level, fullPath);
        }
    }
}