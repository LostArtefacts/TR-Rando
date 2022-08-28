using System;
using TRGE.Core;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Processors;

namespace TRRandomizerCore.Randomizers
{
    public abstract class BaseTR1Randomizer : TR1LevelProcessor, IRandomizer
    {
        public RandomizerSettings Settings { get; internal set; }

        protected Random _generator;

        public abstract void Randomize(int seed);
    }
}