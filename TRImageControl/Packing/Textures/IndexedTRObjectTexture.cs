using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class IndexedTRObjectTexture : AbstractIndexedTRTexture
{
    private TRObjectTexture _texture;

    public override int Atlas
    {
        get => _texture.Atlas;
        set => _texture.Atlas = (ushort)value;
    }

    public TRObjectTexture Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            GetBoundsFromTexture();
        }
    }

    protected override void GetBoundsFromTexture()
    {
        _bounds = Texture.Bounds;
    }

    protected override void ApplyBoundDiffToTexture(int xDiff, int yDiff)
    {
    }

    public bool IsTriangle
        => Texture.IsTriangle;

    public override AbstractIndexedTRTexture Clone()
    {
        return new IndexedTRObjectTexture
        {
            Index = Index,
            Classification = Classification,
            Texture = Texture.Clone()
        };
    }
}
