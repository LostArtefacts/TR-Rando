using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMirrorObjectTexture : BaseEMFunction
{
    public ushort[] Textures { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        MirrorObjectTextures(level.ObjectTextures);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        MirrorObjectTextures(level.ObjectTextures);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        MirrorObjectTextures(level.ObjectTextures);
    }

    private void MirrorObjectTextures(List<TRObjectTexture> levelTextures)
    {
        foreach (ushort textureRef in Textures)
        {
            levelTextures[textureRef].FlipVertical();
        }
    }
}
