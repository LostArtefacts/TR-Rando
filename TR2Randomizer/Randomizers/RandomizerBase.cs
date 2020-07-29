using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TR2Randomizer.Randomizers
{
    public abstract class RandomizerBase : IRandomizer
    {
        protected TR2LevelReader _reader;
        protected TR2LevelWriter _writer;
        protected TR2Level _levelInstance;
        protected Random _generator;
        protected List<string> _levels;
        protected string _basePath;

        public string BasePath 
        {
            get => _basePath;
            set
            {
                _basePath = value;
                _levels = LevelNames.AsList.Select(x => Path.Combine(_basePath, x)).ToList();
            }
        }

        public abstract void Randomize(int seed);

        public RandomizerBase()
        {
            _levels = LevelNames.AsList;

            _reader = new TR2LevelReader();
            _writer = new TR2LevelWriter();
        }
    }
}
