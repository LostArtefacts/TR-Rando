namespace TREnvironmentEditor.Model;

public interface ITextureModifier
{
    void RemapTextures(Dictionary<ushort, ushort> indexMap);
}
