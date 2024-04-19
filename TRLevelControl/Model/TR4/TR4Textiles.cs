namespace TRLevelControl.Model;

public class TR4Textiles
{
    // TRObjectTexture sequencing is based on this order.
    public TR4TextureData Rooms { get; set; }
    public TR4TextureData Objects { get; set; }
    public TR4TextureData Bump { get; set; }
    public TRTexImage32 Sky { get; set; }
    public TRTexImage32 Font { get; set; }

    public int Count => Rooms.Count + Objects.Count + Bump.Count + 2;

    public TR4Textiles()
    {
        Rooms = new();
        Objects = new();
        Bump = new();
    }

    public TRTexImage32 GetImage32(int index)
    {
        if (index < Rooms.Count)
        {
            return Rooms.Images32[index];
        }
        if (index < Rooms.Count + Objects.Count)
        {
            return Objects.Images32[index - Rooms.Count];
        }
        if (index < Rooms.Count + Objects.Count + Bump.Count)
        {
            return Bump.Images32[index - Rooms.Count - Objects.Count];
        }
        if (index == Count - 2)
        {
            return Sky;
        }
        if (index == Count - 1)
        {
            return Font;
        }
        throw new IndexOutOfRangeException();
    }
}
