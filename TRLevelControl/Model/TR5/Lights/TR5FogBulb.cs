namespace TRLevelControl.Model;

public class TR5FogBulb : TR5RoomLight
{
    public override TR4RoomLightType Type => TR4RoomLightType.FogBulb;
    public float Radius { get; set; }
    public float Density { get; set; }
}
