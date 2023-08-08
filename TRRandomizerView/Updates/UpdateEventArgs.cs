using System;

namespace TRRandomizerView.Updates;

public class UpdateEventArgs : EventArgs
{
    public Update Update { get; private set; }

    public UpdateEventArgs(Update update)
    {
        Update = update;
    }
}
