using System.Collections.Generic;

namespace TREnvironmentEditor.Helpers;

public class EMOptions
{
    public bool EnableHardMode { get; set; }
    public IEnumerable<EMTag> ExcludedTags { get; set; }
    public EMExclusionMode ExclusionMode { get; set; }
}
