namespace TRLevelControl.Model;

public class TR4Texture32Chunk : TR4Chunk
{
    public List<TRTexImage32> Rooms { get; set; }
    public List<TRTexImage32> Objects { get; set; }
    public List<TRTexImage32> Bump { get; set; }
}
