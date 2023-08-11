using TRRandomizerCore.Editors;
using TRRandomizerCore.Processors;

namespace TRRandomizerCore.Randomizers;

public abstract class BaseTR2Randomizer : TR2LevelProcessor, IRandomizer
{
    public RandomizerSettings Settings { get; internal set; }

    protected Random _generator;

    public abstract void Randomize(int seed);
}
