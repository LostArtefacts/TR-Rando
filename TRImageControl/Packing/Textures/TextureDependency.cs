using System.Drawing;

namespace TRImageControl.Packing;

public class TextureDependency<T>
    where T : Enum
{
    public List<T> Types { get; set; } = new();
    public int TileIndex { get; set; }
    public Rectangle Bounds { get; set; }

    public void AddType(T type)
    {
        if (!Types.Contains(type))
        {
            Types.Add(type);
        }
    }
}
