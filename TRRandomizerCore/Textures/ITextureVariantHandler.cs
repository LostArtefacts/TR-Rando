using TRImageControl.Textures;

namespace TRRandomizerCore.Textures;

public interface ITextureVariantHandler
{
    string GetSourceVariant(AbstractTextureSource source);
    void StoreVariant(AbstractTextureSource source, string variant);
}
