namespace TRImageControl.Packing;

public interface ITexturePositionMonitor<T>
    where T : Enum
{
    Dictionary<T, List<int>> GetMonitoredIndices();
    void OnTexturesPositioned(Dictionary<T, List<PositionedTexture>> texturePositions);
    void OnTexturesRemoved(List<T> types);
}
