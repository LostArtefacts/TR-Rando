namespace TRLevelControl.Model;

public class TR4Texture16Chunk : TR4Chunk
{
    public List<TRTexImage16> Rooms { get; set; }
    public List<TRTexImage16> Objects { get; set; }
    public List<TRTexImage16> Bump { get; set; }
}
