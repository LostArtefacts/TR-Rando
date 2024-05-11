using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR2TexturePacker : TRTexturePacker
{
    private readonly TR2Level _level;

    public override int NumLevelImages => _level.Images16.Count;

    public TR2TexturePacker(TR2Level level, int maximumTiles = 16)
        : base(maximumTiles)
    {
        _level = level;
        LoadLevel();
    }

    public override TRImage GetImage(int tileIndex)
    {
        return new(_level.Images16[tileIndex].Pixels);
    }

    public override void SetImage(int tileIndex, TRImage image)
    {
        _level.Images16[tileIndex].Pixels = image.ToRGB555();
    }

    protected override void CreateImageSpace(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _level.Images16.Add(new());
            _level.Images8.Add(new() { Pixels = new byte[TRConsts.TPageSize] });
        }
    }

    protected override List<TRTextileSegment> LoadObjectSegments()
    {
        List<TRTextileSegment> segments = new();
        for (int i = 0; i < _level.ObjectTextures.Count; i++)
        {
            if (!_level.ObjectTextures[i].IsValid())
                continue;

            segments.Add(new()
            {
                Index = i,
                Texture = _level.ObjectTextures[i],
            });
        }

        return segments;
    }

    protected override List<TRTextileSegment> LoadSpriteSegments()
    {
        List<TRTextileSegment> segments = new();
        List<TRSpriteTexture> sprites = _level.Sprites.SelectMany(s => s.Value.Textures).ToList();
        for (int i = 0; i < sprites.Count; i++)
        {
            segments.Add(new()
            {
                Index = i,
                Texture = sprites[i],
            });
        }

        return segments;
    }
}
