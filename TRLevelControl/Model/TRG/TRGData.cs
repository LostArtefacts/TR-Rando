namespace TRLevelControl.Model;

public class TRGData
{
    public List<TRColour4[]> AmbientCubes { get; set; }
    public byte[] Unknown1 { get; set; }
    public byte Unknown2 { get; set; }
    public byte Unknown3 { get; set; }
    public List<byte[]> Unknown4 { get; set; }
    public List<byte[]> Unknown5 { get; set; }
    public List<TRGMesh> Meshes { get; set; }
    public List<ushort> Textures { get; set; }
    public List<uint> Indices { get; set; }
    public List<TRGVertex> Vertices { get; set; }
}
