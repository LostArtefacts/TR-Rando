using System.Numerics;

namespace TRLevelControl.Model;

public abstract class TR5GeneralLight : TR5RoomLight
{
    public float Inner { get; set; }
    public float Outer { get; set; }
    public float InnerAngle { get; set; }
    public float OuterAngle { get; set; }
    public float Range { get; set; }
    public Vector3 Direction { get; set; }
}
