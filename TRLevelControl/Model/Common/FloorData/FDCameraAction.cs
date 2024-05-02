namespace TRLevelControl.Model;

public class FDCameraAction : ICloneable
{
    public byte Timer { get; set; }
    public bool Once { get; set; }
    public byte MoveTimer { get; set; }

    public FDCameraAction Clone()
        => (FDCameraAction)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
