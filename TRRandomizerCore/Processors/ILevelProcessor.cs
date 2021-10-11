using System;

namespace TRRandomizerCore.Processors
{
    public interface ILevelProcessor
    {
        void HandleException(Exception e);
        bool TriggerProgress(int progress);
    }
}