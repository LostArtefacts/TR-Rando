namespace TRDataControl.Environment;

public class EMOptions
{
    public bool EnableHardMode { get; set; }
    public List<EMTag> ExcludedTags { get; set; }
    public EMExclusionMode ExclusionMode { get; set; }
}
