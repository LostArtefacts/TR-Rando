using TRRandomizerCore.Editors;
using TRRandomizerCore.Processors;

namespace TRRandomizerCore.Randomizers;

public abstract class BaseTR3RRandomizer : TR3RLevelProcessor, IRandomizer
{
    public RandomizerSettings Settings { get; internal set; }

    protected Random _generator;

    public abstract void Randomize(int seed);
}
