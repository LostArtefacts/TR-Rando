using System;
using TR2RandomizerCore.Processors;

namespace TR2RandomizerCore.Randomizers
{
    public abstract class RandomizerBase : LevelProcessor, IRandomizer
    {
        protected Random _generator;

        public abstract void Randomize(int seed);
    }
}