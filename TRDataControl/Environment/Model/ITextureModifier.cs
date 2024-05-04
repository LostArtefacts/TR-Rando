namespace TRDataControl.Environment;

public interface ITextureModifier
{
    void RemapTextures(Dictionary<ushort, ushort> indexMap);
}
