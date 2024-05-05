using RectanglePacker.Defaults;
using System.Drawing;

namespace TRImageControl.Packing;

public class TexturedTileSegment : DefaultRectangle
{
    public TRImage Image { get; private set; }
    public List<AbstractIndexedTRTexture> Textures { get; private set; }
    public AbstractIndexedTRTexture FirstTexture => Textures[0];
    public int FirstTextureIndex => FirstTexture.Index;
    public string FirstClassification => FirstTexture.Classification;

    public TexturedTileSegment(AbstractIndexedTRTexture initialTexture, TRImage image)
        : base(initialTexture.Bounds)
    {
        Image = image;
        Textures = new List<AbstractIndexedTRTexture>();
        AddTexture(initialTexture);
    }

    public void AddTexture(AbstractIndexedTRTexture texture)
    {
        Textures.Add(texture);
    }

    public bool IsObjectTextureFor(int textureIndex)
    {
        if (Textures.Count == 0 || Textures[0] is IndexedTRSpriteTexture)
        {
            return false;
        }
        return IsFor(textureIndex);
    }

    public bool IsSpriteTextureFor(int textureIndex)
    {
        if (Textures.Count == 0 || Textures[0] is IndexedTRObjectTexture)
        {
            return false;
        }
        return IsFor(textureIndex);
    }

    private bool IsFor(int textureIndex)
    {
        return GetTexture(textureIndex) != null;
    }

    public AbstractIndexedTRTexture GetTexture(int textureIndex)
    {
        for (int i = 0; i < Textures.Count; i++)
        {
            AbstractIndexedTRTexture texture = Textures[i];
            if (texture.Index == textureIndex)
            {
                return texture;
            }
        }
        return null;
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

    public void InheritTextures(TexturedTileSegment otherSegment, Point p, int tileIndex)
    {
        // Get the old segment first to reposition its children
        otherSegment.MoveTo(p, tileIndex);

        // Copy all textures from the old segment into this
        Textures.AddRange(otherSegment.Textures);

        // Clear the other's list of segments, effectively nullifying it
        otherSegment.Textures.Clear();
    }

    // We work out the difference in x/y values here and pass these 
    // to the child areas to allow them to calculate where they
    // need to be.
    public void MoveTo(int x, int y, int tileIndex = -1)
    {
        int xDiff = Math.Abs(Bounds.X - x);
        int yDiff = Math.Abs(Bounds.Y - y);

        if (x < Bounds.X)
        {
            xDiff *= -1;
        }
        if (y < Bounds.Y)
        {
            yDiff *= -1;
        }

        foreach (AbstractIndexedTRTexture texture in Textures)
        {
            texture.MoveBy(xDiff, yDiff);
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
        foreach (AbstractIndexedTRTexture texture in Textures)
        {
            texture.Invalidate();
        }
    }

    // Triggered when changes are to be saved back to the level.
    // Index is that of containing tile.
    public void Commit(int tileIndex)
    {
        foreach (AbstractIndexedTRTexture texture in Textures)
        {
            texture.Commit(tileIndex);
        }
    }

    public TexturedTileSegment Clone()
    {
        TexturedTileSegment copy = new(FirstTexture.Clone(), Image.Clone());
        for (int i = 1; i < Textures.Count; i++)
        {
            copy.AddTexture(Textures[i].Clone());
        }
        return copy;
    }
}
