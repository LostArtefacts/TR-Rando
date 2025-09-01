using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors.TR2;

public interface ITR2ProcessorTask
{
    void Run(TR2CombinedLevel level);
}
