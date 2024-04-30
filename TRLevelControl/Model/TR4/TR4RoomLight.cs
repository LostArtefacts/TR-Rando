namespace TRLevelControl.Model;

public class TR4RoomLight
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public TRColour Colour { get; set; }
    public byte LightType { get; set; }
    public byte Unknown { get; set; }
    public byte Intensity { get; set; }
    public float In { get; set; }
    public float Out { get; set; }
    public float Length { get; set; }
    public float CutOff { get; set; }
    public float Dx { get; set; }
    public float Dy { get; set; }
    public float Dz { get; set; }
}
