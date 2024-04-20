namespace TRLevelControl.Model;

public class TR5Textiles
{
    public TR4TextureData Rooms { get; set; }
    public TR4TextureData Objects { get; set; }
    public TRTexImage32 Shine { get; set; }
    public TRTexImage32 Sky { get; set; }
    public TRTexImage32 Font { get; set; }

    public int Count => Rooms.Count + Objects.Count + 3;

    public TR5Textiles()
    {
        Rooms = new();
        Objects = new();
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
        if (index == Count - 3)
        {
            return Shine;
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
