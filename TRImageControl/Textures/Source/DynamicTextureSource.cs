namespace TRImageControl.Textures;

public class DynamicTextureSource : AbstractTextureSource
{
    public Dictionary<string, HSBOperation> OperationMap { get; set; }
    public override string[] Variants => OperationMap.Keys.ToArray();
}
