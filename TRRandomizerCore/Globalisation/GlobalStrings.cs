using System.Collections.Generic;

namespace TRRandomizerCore.Globalisation;

public class GlobalStrings
{
    public Dictionary<int, string[]>[] GroupedStrings { get; set; }
    public Dictionary<int, string[]> StandaloneStrings { get; set; }
}
