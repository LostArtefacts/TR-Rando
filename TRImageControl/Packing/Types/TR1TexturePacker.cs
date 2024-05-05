using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR1TexturePacker : TRTexturePacker
{
    private readonly TR1Level _level;

    public TRPalette8Control PaletteControl { get; set; }
    public override int NumLevelImages => _level.Images8.Count;

    public TR1TexturePacker(TR1Level level, int maximumTiles = 16)
        : base(maximumTiles)
    {
        _level = level;
        LoadLevel();
    }

    public override TRImage GetImage(int tileIndex)
    {
        return new(_level.Images8[tileIndex].Pixels, _level.Palette);
    }

    public override void SetImage(int tileIndex, TRImage image)
    {
        PaletteControl ??= new()
        {
            Level = _level
        };
        PaletteControl.ChangedTiles[tileIndex] = image;
    }

    protected override void CreateImageSpace(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _level.Images8.Add(new() { Pixels = new byte[TRConsts.TPageSize] });
        }
    }

    protected override List<TRTextileSegment> LoadObjectSegments()
    {
        List<TRTextileSegment> segments = new();
        for (int i = 0; i < _level.ObjectTextures.Count; i++)
        {
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

    protected override void PostCommit()
    {
        PaletteControl?.MergeTiles();
    }
}
