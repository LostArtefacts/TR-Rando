using TRLevelControl.Model.Base.Enums;

namespace TRLevelControl.Model;

public class TR2ZoneGroup : Dictionary<FlipStatus, TR2Zone>
{
    /// <summary>
    /// Zone values when flipmap is off.
    /// </summary>
    public TR2Zone NormalZone
    {
        get => this[FlipStatus.Off];
        set => this[FlipStatus.Off] = value;
    }
    /// <summary>
    /// Zone values when flipmap is on.
    /// </summary>
    public TR2Zone AlternateZone
    {
        get => this[FlipStatus.On];
        set => this[FlipStatus.On] = value;
    }
}
