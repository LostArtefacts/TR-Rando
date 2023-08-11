namespace TRLevelControl.Model;

public class TR4Texture32Chunk : TR4Chunk
{
    public List<TR4TexImage32> Rooms { get; set; }
    public List<TR4TexImage32> Objects { get; set; }
    public List<TR4TexImage32> Bump { get; set; }
}
