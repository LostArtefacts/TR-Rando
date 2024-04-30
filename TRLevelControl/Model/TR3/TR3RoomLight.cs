namespace TRLevelControl.Model;

public class TR3RoomLight
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public TRColour Colour { get; set; }
    public byte LightType { get; set; }
    //8 bytes, depends on LightType (sun or spot)
    public short[] LightProperties { get; set; }
}
