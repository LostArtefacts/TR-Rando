using System;
using System.Collections.Generic;
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

        public abstract void Randomize(int seed);

        public RandomizerBase()
        {
            _levels = LevelNames.AsList;

            _reader = new TR2LevelReader();
            _writer = new TR2LevelWriter();
        }
    }
}
