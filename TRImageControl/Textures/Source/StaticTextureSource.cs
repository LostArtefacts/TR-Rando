using System.Drawing;

namespace TRImageControl.Textures;

public class StaticTextureSource<E> : AbstractTextureSource
    where E : Enum
{
    public string PNGPath { get; set; }
    public E SpriteSequence { get; set; }
    public bool IsSpriteSequence => !EqualityComparer<E>.Default.Equals(SpriteSequence, default);
    public Dictionary<E, Dictionary<Color, int>> EntityColourMap { get; set; }
    public Dictionary<E, Dictionary<Color, int>> EntityColourMap8 { get; set; }
    public Dictionary<E, Dictionary<int, int>> EntityTextureMap { get; set; }
    public IEnumerable<E> ColourEntities => EntityColourMap?.Keys;
    public IEnumerable<E> TextureEntities => EntityTextureMap?.Keys;
    public Dictionary<string, List<Rectangle>> VariantMap { get; set; }
    public override string[] Variants => VariantMap.Keys.ToArray();
    public bool HasVariants => VariantMap.Count > 0;

    private TRImage _image;
    public TRImage Image
    {
        get => _image ??= new(PNGPath);
    }

    public TRImage ClonedBitmap => Image.Clone();

    public override bool Equals(object obj)
    {
        return obj is StaticTextureSource<E> source && PNGPath == source.PNGPath;
    }

    public override int GetHashCode()
    {
        return -193438157 + EqualityComparer<string>.Default.GetHashCode(PNGPath);
    }
}
