namespace TRLevelControl.Model;

public class TRGShader
{
    public uint Type { get; set; }
    public uint Unknown1 { get; set; }
    public uint Unknown2 { get; set; }
    public uint Unknown3 { get; set; }
    public uint Unknown4 { get; set; }
    public Tuple<uint, uint>[] IndexInfo { get; set; }
}
