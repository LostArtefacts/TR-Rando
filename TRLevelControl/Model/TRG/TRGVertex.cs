namespace TRLevelControl.Model;

public class TRGVertex
{
    public TRVertex Vertex { get; set; }
    public short Unknown { get; set; }
    public TRVertex8 Normal { get; set; }
    public byte Texture { get; set; }
    public TRColour Ambient1 { get; set; }
    public TRColour Ambient2 { get; set; }
    public byte U {  get; set; }
    public byte V { get; set; }
}
