namespace TRLevelControl.Model;

public class TR4TextureData
{
    public List<TRTexImage32> Images32 { get; set; }
    public List<TRTexImage16> Images16 { get; set; }
    public int Count => Images32.Count;
}
