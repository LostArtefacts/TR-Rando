using TRLevelControl.Model.Base.Enums;

namespace TRLevelControl.Model;

public class TRZoneGroup : Dictionary<FlipStatus, TRZone>
{
    /// <summary>
    /// Zone values when flipmap is off.
    /// </summary>
    public TRZone NormalZone
    {
        get => this[FlipStatus.Off];
        set => this[FlipStatus.Off] = value;
    }
    /// <summary>
    /// Zone values when flipmap is on.
    /// </summary>
    public TRZone AlternateZone
    {
        get => this[FlipStatus.On];
        set => this[FlipStatus.On] = value;
    }
}
