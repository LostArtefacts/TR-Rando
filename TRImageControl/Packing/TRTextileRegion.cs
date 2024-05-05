using RectanglePacker.Defaults;
using System.Drawing;
using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TRTextileRegion : DefaultRectangle
{
    public TRImage Image { get; set; }
    public string ID { get; set; }
    public List<TRTextileSegment> Segments { get; set; }

    public TRTextileRegion()
        : base(new())
    {
        Segments = new();
    }

    public TRTextileRegion(TRTextileSegment initialTexture, TRImage image)
        : base(initialTexture.Bounds)
    {
        Image = image;
        Segments = new();
        AddTexture(initialTexture);
    }

    public void GenerateID()
    {
        ID = Image.GenerateID();
    }

    public void AddTexture(TRTextileSegment texture)
    {
        Segments.Add(texture);
    }

    public bool IsObjectTextureFor(int textureIndex)
    {
        return Segments.Count > 0 && Segments[0].Texture is TRObjectTexture
            && GetTexture(textureIndex) != null;
    }

    public bool IsSpriteTextureFor(int textureIndex)
    {
        return Segments.Count > 0 && Segments[0].Texture is TRSpriteTexture
            && GetTexture(textureIndex) != null;
    }

    public TRTextileSegment GetTexture(int textureIndex)
    {
        return Segments.Find(s => s.Index == textureIndex);
    }

    // Triggered when successfully mapped onto a tile, so inform
    // the indexed textures to update their vertices.
    public void Bind()
    {
        MoveTo(MappedX, MappedY);
    }

    public void MoveTo(Point p, int tileIndex = -1)
    {
        MoveTo(p.X, p.Y, tileIndex);
    }

    public void InheritTextures(TRTextileRegion otherRegion, Point p, int tileIndex)
    {
        // Get the old segment first to reposition its children
        otherRegion.MoveTo(p, tileIndex);

        // Copy all textures from the old segment into this
        Segments.AddRange(otherRegion.Segments);

        // Clear the other's list of segments, effectively nullifying it
        otherRegion.Segments.Clear();
    }

    // We work out the difference in x/y values here so that child
    // segments are repositioned correctly.
    public void MoveTo(int x, int y, int tileIndex = -1)
    {
        Point root = Segments[0].Position;

        foreach (TRTextileSegment texture in Segments)
        {
            Point position = texture.Position;
            position.X = position.X - root.X + x;
            position.Y = position.Y - root.Y + y;
            texture.Position = position;

            if (tileIndex != -1)
            {
                texture.Commit(tileIndex);
            }
        }
    }

    // Triggered when removed from a tile, so inform
    // the indexed textures to invalidate themselves.
    public void Unbind()
    {
        foreach (TRTextileSegment texture in Segments)
        {
            texture.Invalidate();
        }
    }

    // Triggered when changes are to be saved back to the level.
    // Index is that of containing tile.
    public void Commit(int tileIndex)
    {
        foreach (TRTextileSegment texture in Segments)
        {
            texture.Commit(tileIndex);
        }
    }

    public TRTextileRegion Clone()
    {
        TRTextileRegion copy = new(Segments[0].Clone(), Image.Clone());
        for (int i = 1; i < Segments.Count; i++)
        {
            copy.AddTexture(Segments[i].Clone());
        }
        return copy;
    }
}
