namespace TRLevelControl.Model;

public class FDActionItem : ICloneable
{
    public FDTrigAction Action { get; set; }
    public short Parameter { get; set; }
    public FDCameraAction CamAction { get; set; }

    public FDActionItem Clone()
    {
        return new()
        {
            Action = Action,
            Parameter = Parameter,
            CamAction = CamAction?.Clone()
        };
    }

    object ICloneable.Clone()
        => Clone();
}
