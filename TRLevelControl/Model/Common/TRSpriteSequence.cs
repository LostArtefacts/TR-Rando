namespace TRLevelControl.Model;

public class TRSpriteSequence : ICloneable
{
    public List<TRSpriteTexture> Textures { get; set; } = new();

    public TRSpriteSequence Clone()
    {
        return new()
        {
            Textures = new(Textures.Select(t => t.Clone()))
        };
    }

    object ICloneable.Clone()
        => Clone();
}
